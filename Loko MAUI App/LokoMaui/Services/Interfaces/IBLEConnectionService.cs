using loko.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace loko.Services.Interfaces
{
    public interface IBLEConnectionService
    {
        event EventHandler<BLEDevice> DeviceDiscovered;
        event EventHandler ScanningEnded;

        Task Init();

        Task ScanDevices();
        void StopScanning();

        BLEDevice CurrentDevice { get; set; }

        List<BLEDevice> GetAvailableDevices();

        BLEDevice GetAvailableDeviceByGUID(Guid deviceId);

        Task ConnectToDevice(BLEDevice bleDevice, Action<Exception> onException = null);

        Task DisconnectFromDevice(BLEDevice bleDevice, Action<Exception> onException = null);

        int DeviceBatteryLevel { get; }
        event EventHandler OnDeviceBatteryLevelChanged;
    }
}