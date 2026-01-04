using loko.Models;
using loko.Services.Interfaces;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;

namespace loko.Services.Implementations
{
    public class BLEConnectionService : IBLEConnectionService
    {
        private const int MaxMTUSize = 512;

        private bool _isBusy;

        private int _deviceBatteryLevel;
        public int DeviceBatteryLevel
        {
            get => _deviceBatteryLevel;
            set
            {
                _deviceBatteryLevel = value;
                OnDeviceBatteryLevelChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler OnDeviceBatteryLevelChanged;

        public BLEConnectionService()
        {
            BLEDevices = new List<BLEDevice>();
        }

        private static Lazy<IBluetoothLE> LazyBluetoothLE =>
            new Lazy<IBluetoothLE>(() => CrossBluetoothLE.Current);

        private List<BLEDevice> BLEDevices { get; }

        private IBluetoothLE BluetoothLE => LazyBluetoothLE.Value;

        private IAdapter Adapter => LazyBluetoothLE.Value.Adapter;

        public event EventHandler ScanningEnded;
        public event EventHandler<BLEDevice> DeviceDiscovered;

        public BLEDevice CurrentDevice { get; set; }

        public async Task Init()
        {
            BluetoothLE.StateChanged += OnStateChanged;
            Adapter.DeviceDiscovered += OnDeviceDiscovered;
            Adapter.ScanTimeoutElapsed += OnScanTimeoutElapsed;
        }

        public async Task ScanDevices()
        {
            await Adapter.StartScanningForDevicesAsync();
        }

        public void StopScanning()
        {
           
            Adapter.StopScanningForDevicesAsync();
        }

        public List<BLEDevice> GetAvailableDevices()
        {
            return BLEDevices;
        }

        public BLEDevice GetAvailableDeviceByGUID(Guid deviceId)
        {
            return BLEDevices.FirstOrDefault(x => x.Device.Id.Equals(deviceId));
        }

        public async Task ConnectToDevice(BLEDevice bleDevice, Action<Exception> onException = null)
        {
            try
            {
                if (_isBusy)
                    return;

                if (BLEDevices.Any(x => x.IsConnected))
                {
                    await App.Current.MainPage.DisplayAlert("Error", "You can't connect to multiple BLE devices", "OK");
                    return;
                }

                bleDevice.IsSwitchingOn = true;

                async void OnDeviceConnected(object sender, DeviceEventArgs eventArgs)
                {
                    Adapter.DeviceConnected -= OnDeviceConnected;
                    bleDevice.IsConnected = eventArgs.Device.State == DeviceState.Connected;
                    await bleDevice.Device.RequestMtuAsync(MaxMTUSize);
                    await ConnectToDeviceService(bleDevice);
                }

                Adapter.DeviceConnected += OnDeviceConnected;
                var parameters = new ConnectParameters(forceBleTransport: true);
                await Adapter.ConnectToDeviceAsync(bleDevice.Device, parameters);

                CurrentDevice = bleDevice;
            }
            catch (Exception ex)
            {
                bleDevice.IsSwitchingOn = false;
                bleDevice.IsSwitchedOn = false;
                onException?.Invoke(ex);
            }
            finally
            {
                bleDevice.IsSwitchingOn = false;
                _isBusy = false;
            }
        }

        public async Task DisconnectFromDevice(BLEDevice bleDevice, Action<Exception> onException = null)
        {
            try
            {
                if (_isBusy)
                    return;

                bleDevice.IsSwitchingOff = true;

                void OnDeviceDisconnected(object sender, DeviceEventArgs eventArgs)
                {
                    Adapter.DeviceDisconnected -= OnDeviceDisconnected;
                    bleDevice.IsConnected = eventArgs.Device.State == DeviceState.Connected;
                    DisconnectFromDeviceService(bleDevice);
                }

                Adapter.DeviceDisconnected += OnDeviceDisconnected;
                await Adapter.DisconnectDeviceAsync(bleDevice.Device);

                CurrentDevice = null;
            }
            catch (Exception ex)
            {
                bleDevice.IsSwitchingOff = false;
                bleDevice.IsConnected = false;
                onException?.Invoke(ex);
            }
            finally
            {
                bleDevice.IsSwitchingOff = false;
                _isBusy = false;
            }
        }

        private async Task ConnectToDeviceService(BLEDevice bleDevice)
        {
            var services = await bleDevice.Device.GetServicesAsync();
            foreach (var service in services)
            {
                var characteristics = await service.GetCharacteristicsAsync();
                foreach (var characteristic in characteristics
                             .Where(x => x.Properties == CharacteristicPropertyType.Notify))
                {
                    if (characteristic.CanUpdate)
                    {
                        bleDevice.Characteristic = characteristic;
                        bleDevice.Characteristic.ValueUpdated += OnDeviceCharacteristicValueUpdated;

                        await bleDevice.Characteristic.StartUpdatesAsync();
                    }
                }
            }
        }

        private void OnDeviceCharacteristicValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
        {
            try
            {
                var receivedBytes = e.Characteristic.Value;
                var stringValue = Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);

                if (string.IsNullOrEmpty(stringValue))
                    return;

                var stringArray = stringValue.Split(',');

                if (sender is ICharacteristic characteristic)
                {
                    var device = GetAvailableDeviceByGUID(characteristic.Service.Device.Id);

                    if (device == null)
                        return;
                    if (stringArray.Length == 1)
                    {
                        var dbl = stringArray[0].Trim();
                        float.TryParse(dbl, NumberStyles.Float, CultureInfo.InvariantCulture, out var  devicebtl);
                        DeviceBatteryLevel = (int)devicebtl;
                    }
                    else
                    {

                        int.TryParse(stringArray[0], out var id);
                        int.TryParse(stringArray[4], out var batteryLevel);
                        float latitude;
                        float longitude;
                        CultureInfo culture = CultureInfo.InvariantCulture;

                        if (DeviceInfo.Platform == DevicePlatform.iOS)
                        {
                            float.TryParse(stringArray[2], NumberStyles.Float, culture, out latitude);
                            float.TryParse(stringArray[3], NumberStyles.Float, culture, out longitude);
                        }
                        else
                        {
                            float.TryParse(stringArray[2], NumberStyles.Float, culture, out latitude);
                            float.TryParse(stringArray[3], NumberStyles.Float, culture, out longitude);
                        }

                        var location = new BLEDeviceDetails
                        {
                            DeviceId = id,
                            Latitude = latitude,
                            Longitude = longitude,
                            BatteryLevel = batteryLevel
                        };

                        // device.Locations.Add(location);

                        // traces.Add(location);
                        var deviceLocation = device.Locations.FirstOrDefault(x => x.DeviceId.Equals(id));
                        if (deviceLocation == null)
                            device.Locations.Add(location);
                        else
                        {
                            //if (deviceLocation.Latitude.Equals(location.Latitude) &&
                            //    deviceLocation.Longitude.Equals(location.Longitude))
                            //    return;

                            deviceLocation.Latitude = location.Latitude;
                            deviceLocation.Longitude = location.Longitude;
                            deviceLocation.SendLocationChanged();
                        }

                        device.Details = location;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void DisconnectFromDeviceService(BLEDevice bleDevice)
        {
            if (bleDevice.Characteristic != null)
                bleDevice.Characteristic.ValueUpdated -= OnDeviceCharacteristicValueUpdated;
        }

        private void OnStateChanged(object sender, BluetoothStateChangedArgs e)
        {

        }

        private void OnDeviceDiscovered(object sender, DeviceEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Device.Name))
                return;

            var existingDevice = BLEDevices.FirstOrDefault(x => x.Device.Id.Equals(e.Device.Id));
            if (existingDevice == null)
            {
                var newDevice = new BLEDevice(e.Device, e.Device.State == DeviceState.Connected);
                BLEDevices.Add(newDevice);
                DeviceDiscovered?.Invoke(this, newDevice);
            }
            else
            {
                // Update existing device if needed
                existingDevice.IsConnected = e.Device.State == DeviceState.Connected;
                DeviceDiscovered?.Invoke(this, existingDevice);
            }
        }

        private void OnScanTimeoutElapsed(object sender, EventArgs e)
        {
            ScanningEnded?.Invoke(this, EventArgs.Empty);
        }
    }
}