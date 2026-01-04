using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using loko.Framework;
using loko.Helpers;
using loko.Models;
using loko.Services.Implementations;
using loko.Services.Interfaces;
using loko.Views;
using System.Collections.ObjectModel;

namespace loko.ViewModels;

public partial class ConnectionPageViewModel : BaseViewModel
{
    private readonly IPermissionService _permissionService;
    private readonly IBLEConnectionService _bleConnectionService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<SettingsOptions> _settings;

    private bool _shouldChangeRootPage = false;
    private Type _newRootPageType = null;

    [ObservableProperty]
    private bool _isScanning;


    [ObservableProperty]
    private int _deviceBatteryLevel;

    public ConnectionPageViewModel(IBLEConnectionService bleConnectionService, INavigationService navigationService, IPermissionService permissionService)
    {
        _permissionService = permissionService;
        _bleConnectionService = bleConnectionService;
        _navigationService = navigationService;
        _bleConnectionService.Init();
        _bleConnectionService.ScanningEnded += OnScanningEnded;
        _bleConnectionService.OnDeviceBatteryLevelChanged += OnDeviceBatteryLevelChanged;
        _bleConnectionService.DeviceDiscovered += OnDeviceDiscovered;

        BLEDevices = new ObservableCollection<BLEDevice>();

        Settings = new()
        {

            new SettingsOptions
            {
                Label = "Ground Battery Level",
                IsNeedToShowContent = false,
                Command = expandCommand,
                Contents = new ()
            },
            new SettingsOptions
            {
                Label = "GeoFence",
                Command = ExpandCommand,
                Contents = new()
                {
                    new SettingsOptionsContent
                    {
                        Label = "Text",
                        IsNeedToShowToggle = false
                    },
                    new SettingsOptionsContent
                    {
                        Label = "Sound",
                        Command = SelectCommand
                    },
                    new SettingsOptionsContent
                    {
                        Label = "Vibration",
                        Command = SelectCommand
                    }
                }
            },
            new SettingsOptions
            {
                Label = "Offline Map",
                Command = ExpandCommand,
                Contents = new()
                {
                    new SettingsOptionsContent
                    {
                        Label = "OSM",
                        Command = SelectCommand,
                        SettingsType = SettingsType.OfflineMap,
                        IsSelected = AppPreferences.GetSelectedMapType() == "OSM"

                    },
                }
            },
            new SettingsOptions
            {
                Label = "Map view",
                Command = ExpandCommand,
                Contents = new()
                {
                    new SettingsOptionsContent
                    {
                        Label = "Satellite",
                        Command = SelectCommand,
                        SettingsType = SettingsType.MapType,
                        IsSelected = loko.MauiProgram.MapType == Microsoft.Maui.Maps.MapType.Satellite
                    },
                }
            },
            new SettingsOptions
            {
                Label = "Connection",
                Command = ExpandCommand,
                IsOneSelection = true,
                IsScrolable = true,
                Contents = new()
                {
                    new SettingsOptionsContent
                    {
                        Label = "BLE",
                        IsNeedToShowToggle = false
                    },

                   
                }
            }, 
            new SettingsOptions
            {
                Label = "Archive",
                Command = ExpandCommand,
                IsClickedCommand = true,
                IsScrolable = true,
                Contents = new()
               
            }
        };
    }

    public async Task InitializeBluetooth()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            bool bluetoothPermissionGranted = await _permissionService.RequestBluetoothPermissions();
        }
        //bool bluetoothPermissionGranted = await _permissionService.RequestBluetoothPermissions();
        //if (bluetoothPermissionGranted)
        //{
        //    // Proceed with Bluetooth operations
        //}
        //else
        //{


            //}
    }

    private void OnDeviceDiscovered(object sender, BLEDevice device)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var existingDevice = BLEDevices.FirstOrDefault(x => x.Device.Id.Equals(device.Device.Id));
            if (existingDevice == null)
            {
                BLEDevices.Add(device);
                UpdateConnectionSettings(device);
            }
            else
            {
                // Update existing device if needed
                existingDevice.IsConnected = device.IsConnected;
                UpdateConnectionSettings(existingDevice);
            }
        });
    }

    private void UpdateConnectionSettings(BLEDevice device)
    {
        var connectionSettings = Settings.First(x => x.Label == "Connection");
        var existingContent = connectionSettings.Contents.FirstOrDefault(x => x.Label == device.Device.Name);
        if (existingContent == null)
        {
            connectionSettings.Contents.Add(new SettingsOptionsContent()
            {
                Label = device.Device.Name,
                Command = SelectCommand,
                SettingsType = SettingsType.BLE,
                IsSelected = device.IsConnected
            });
        }
        else
        {
            existingContent.IsSelected = device.IsConnected;
        }
    }

    public async override Task InitializeAsync()
    {
        await base.InitializeAsync();
        await InitializeBluetooth();
        await Init();
    }

    public void StartScanningForDevices()
    {
        if (!IsScanning)
        {
            IsScanning = true;
            _bleConnectionService.ScanDevices();
        }
    }

    public void StopScanningForDevices()
    {
        _bleConnectionService.StopScanning();
    }

    public override async Task GoBackTask()
    {
        Dispose();
        if (_shouldChangeRootPage && _newRootPageType != null)
        {
            await _navigationService.ChangeRootPage(_newRootPageType);
            _shouldChangeRootPage = false;
            _newRootPageType = null;
        }
        else
        {
            await _navigationService.NavigateBack();
        }
        //await _navigationService.NavigateBack();
    }

    private void OnDeviceBatteryLevelChanged(object sender, EventArgs e)
    {
        DeviceBatteryLevel = _bleConnectionService.DeviceBatteryLevel;
    }

    [RelayCommand]
    private async Task DownloadMap()
    {
        // TODO: Implement logic to navigate to the Download Map page
        await _navigationService.NavigateToDownloadMapPage();
    }

    [RelayCommand]
    private async Task MyOfflineMaps()
    {
        // TODO: Implement logic to navigate to the My Offline Maps page
        await _navigationService.NavigateToMyOfflineMapsPage();
    }


    [RelayCommand]
    private async Task SwitchingOn(BLEDevice bleDevice)
    {
        await _bleConnectionService.ConnectToDevice(bleDevice, async (obj) =>
        {
            await App.Current.MainPage.DisplayAlert("Error with connecting", obj.Message, "OK");
        });
    }

    [RelayCommand]
    private async Task SwitchingOff(BLEDevice bleDevice)
    {
        await _bleConnectionService.DisconnectFromDevice(bleDevice, async (obj) =>
        {
            await App.Current.MainPage.DisplayAlert("Error with disconnecting", obj.Message, "OK");
        });
    }

    [RelayCommand]
    private async Task Expand(SettingsOptions setting)
    {
        foreach (var option in Settings)
        {
            if (option != setting)
            {
                option.IsExpanded = false;
            }
        }
    }

    [RelayCommand]
    private async Task Select(SettingsOptionsContent content)
    {
        if (content.SettingsType == SettingsType.BLE)
        {
            var selectedDevice = BLEDevices.FirstOrDefault(x => x.Device.Name == content.Label);
            if (selectedDevice != null)
            {
                if (selectedDevice.IsConnected)
                {
                    await SwitchingOff(selectedDevice);
                }
                else
                {
                    await SwitchingOn(selectedDevice);
                }                
            }
        }

        if (content.SettingsType == SettingsType.MapType)
        {
            if (!content.IsSelected)
                MauiProgram.MapType = Microsoft.Maui.Maps.MapType.Satellite;
            else
                MauiProgram.MapType = Microsoft.Maui.Maps.MapType.Street;
        }

        if (content.SettingsType == SettingsType.OfflineMap)
        {
            App.UseOSM = !content.IsSelected;
            Console.WriteLine($"Toggling OSM. New value: UseOSM: {App.UseOSM}");

            // Save the selected map type
            AppPreferences.SetSelectedMapType(App.UseOSM ? "OSM" : "Google");

            _shouldChangeRootPage = true;
            _newRootPageType = App.UseOSM ? typeof(OSMPage) : typeof(MapPage);
        }

        if (content.SettingsType == SettingsType.Dates)
        {
            var p = new Dictionary<string, object>()
            {
                { nameof(ArchivePageViewModel.Title), content.Label}
            };
            await _navigationService.NavigateToArcivePage(p);            
        }
        content.IsSelected = !content.IsSelected;
        //OnPropertyChanged(nameof(content));
    }

    [RelayCommand]
    private async Task DeleteArchive(SettingsOptionsContent content)
    {
        var db = await DeviceDetailsDB.Instance;
        await db.DeleteRecordsByDate(content.Label);
        var t = Settings.FirstOrDefault(x => x.Label == "Archive").Contents.Remove(content);
    }

    private async Task GetDBListDates()
    {
        var db = await DeviceDetailsDB.Instance;
        var records = await db.GetDatesFromRecords();
        var content = new List<SettingsOptionsContent>();
        foreach (var record in records)
        {
            content.Add(new SettingsOptionsContent
            {
                Label = record,
                Command = SelectCommand,
                IsNeedToShowToggle = false,
                SettingsType = SettingsType.Dates
            });
        }
        Settings.FirstOrDefault(x => x.Label == "Archive").Contents = new(content);
    }

    public ObservableCollection<BLEDevice> BLEDevices { get; set; }

    public async Task Init()
    {
        DeviceBatteryLevel = _bleConnectionService.DeviceBatteryLevel;

        await GetDBListDates();

        // Load known devices immediately
        //LoadKnownDevices();
        LoadAvailableDevices();

        // Start a new scan for additional devices
        await _bleConnectionService.ScanDevices();
    }



    //private void LoadKnownDevices()
    //{
    //    var knownDevices = _bleConnectionService.GetKnownDevices();
    //    foreach (var device in knownDevices)
    //    {
    //        if (!BLEDevices.Any(x => x.Device.Id.Equals(device.Device.Id)))
    //        {
    //            BLEDevices.Add(device);
    //            UpdateConnectionSettings(device);
    //        }
    //    }
    //}

    private void OnScanningEnded(object sender, EventArgs e)
    {
        IsScanning = false;
        LoadAvailableDevices();
    }

    private void LoadAvailableDevices()
    {
        var availableDevices = _bleConnectionService.GetAvailableDevices();
        foreach (var device in availableDevices)
        {
            if (!BLEDevices.Any(x => x.Device.Id == device.Device.Id))
            {
                BLEDevices.Add(device);
                UpdateConnectionSettings(device);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _bleConnectionService.DeviceDiscovered -= OnDeviceDiscovered;
            _bleConnectionService.ScanningEnded -= OnScanningEnded;
            _bleConnectionService.OnDeviceBatteryLevelChanged -= OnDeviceBatteryLevelChanged;
            //_bleConnectionService.DeviceDiscovered -= OnDeviceDiscovered;
        }
    }

}