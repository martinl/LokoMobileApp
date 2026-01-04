using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using loko.Controls.Renderers;
using loko.Framework;
using loko.Helpers.Extensions;
using loko.Models;
using loko.Services.Implementations;
using loko.Services.Interfaces;
using loko.Views;
using Microsoft.Maui.Maps;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace loko.ViewModels;

public partial class MapViewModel : BaseViewModel
{
    private readonly IBLEConnectionService _bleConnectionService;

    private readonly IPermissionService _permissionService;
    private readonly INavigationService _navigationService;
    private LocationService _locationService;

    private DeviceDetailsDB DataBase;

    [ObservableProperty]
    private Location _currentUserLocation;

    [ObservableProperty]
    private MapType _mapType;

    [ObservableProperty]
    private int _zoomPressed;

    [ObservableProperty]
    private ObservableCollection<DeviceModel> _devicesList;

    [ObservableProperty]
    private ObservableCollection<ExtendedPin> _places;

    [ObservableProperty]
    private ObservableCollection<ExtendedMapsuiPin> _placesMapsui;

    public ObservableCollection<BLEDeviceDetails> PinsDetails { get; set; }

    public bool _isCheckingPermission { get; set; }

    public MapViewModel(IBLEConnectionService bleConnectionService, INavigationService navigationService, IPermissionService permissionService)
    {
        _navigationService = navigationService;
        _bleConnectionService = bleConnectionService;
        _permissionService = permissionService;
        _locationService = new LocationService();
        _locationService.LocationChanged += OnLocationChanged;


        PinsDetails = new();
        Places = new();
        DevicesList = new();
        PlacesMapsui = new();
    }



    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        if (_isCheckingPermission)
            return;
        await Init();
        MapType = MauiProgram.MapType;
        await _locationService.Start();

    }

    public override Task UnInitializeAsync()
    {
        if (!_isCheckingPermission)
            Dispose();
        return base.UnInitializeAsync();
    }

    private void OnLocationChanged(object sender, Location location)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentUserLocation = location;
            
        });
    }

    private void OnDevicesScanningEnded(object sender, EventArgs args)
    {
        if(_bleConnectionService.CurrentDevice is not null)
        {
            _bleConnectionService.CurrentDevice.PropertyChanged += OnDevicePropertyChanged;
            Console.WriteLine($"Device-PropertyChanded +++ {_bleConnectionService.CurrentDevice.Device.Name}");
        }
    }

    private void OnDevicePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BLEDevice.Details))
        {
            var bleDevice = sender as BLEDevice;
            var locations = bleDevice.Locations;
            var deviceDetails = locations.FirstOrDefault(x => x.DeviceId.Equals(bleDevice.Details.DeviceId));

            deviceDetails.DateTime = DateTime.Now;
            Device.BeginInvokeOnMainThread(async () =>
                await GetDataFromDevice(deviceDetails)
);
        }
        if (e.PropertyName == "IsConnected")
        {
            var bleDevice = sender as BLEDevice;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await LoadFromDB();
            });
        }
    }

    public async Task Init()
    {
        _isCheckingPermission = true;
        if (!await RuntimePermission.CheckAndRequestPermissionAsync(new LocationWhenInUse()))
            return;
        _isCheckingPermission = false;
        var userLocation = await GetUserLocation();
        if (userLocation != null)
            CurrentUserLocation = userLocation;

        DataBase = await DeviceDetailsDB.Instance;
        
        if (_bleConnectionService.CurrentDevice is not null)
        {
            _bleConnectionService.CurrentDevice.PropertyChanged += OnDevicePropertyChanged;
            Console.WriteLine($"Device-PropertyChanded +++ {_bleConnectionService.CurrentDevice.Device.Name}");
            if(Places.Count == 0)
            {
                await LoadFromDB();
            }

            if(PlacesMapsui.Count == 0)
            {
                await LoadFromDB2();
            }
        }

        Console.WriteLine($"DBPath - {DeviceDetailsDB.DatabasePath}");
    }

    private async Task LoadFromDB()
    {
        var records = await DataBase.GetRecordsByDate(DateTime.Now.ToString("MM'/'dd'/'yyyy"));
        if (!records.Any())
            return;
        var ids = records.Select(x => x.DeviceId).Distinct();
        foreach (var id in ids)
        {
            var device = new DeviceModel { Label = id.ToString(), IsSelected = true };
            if (!DevicesList.Any(x => x.Label == device.Label))
                DevicesList.Add(device);
            var recordsByDevice = records.Where(x => x.DeviceId == id).ToList();
            foreach (var record in recordsByDevice)
            {
                //PinsDetails.Add(record);
                record.IsLastKnownDeviceLocation = recordsByDevice.IndexOf(record) == recordsByDevice.Count - 1;
                if (record.Latitude != 0 || record.Longitude != 0) // Add this check
                {
                    Places.Add(GetExtendedPin(record, true));
                }
            }
        }
    }

    private async Task LoadFromDB2()
    {
        var records = await DataBase.GetRecordsByDate(DateTime.Now.ToString("MM'/'dd'/'yyyy"));
        if (!records.Any())
            return;
        var ids = records.Select(x => x.DeviceId).Distinct();
        foreach (var id in ids)
        {
            var device = new DeviceModel { Label = id.ToString(), IsSelected = true };
            if (!DevicesList.Any(x => x.Label == device.Label))
                DevicesList.Add(device);
            var recordsByDevice = records.Where(x => x.DeviceId == id).ToList();
            foreach (var record in recordsByDevice)
            {
                //PinsDetails.Add(record);
                record.IsLastKnownDeviceLocation = recordsByDevice.IndexOf(record) == recordsByDevice.Count - 1;

                if (record.Latitude != 0 || record.Longitude != 0) // Add this check
                {
                    PlacesMapsui.Add(GetExtendedPin2(record, true));
                }
            }
        }
    }

    private async Task GetDataFromDevice(BLEDeviceDetails bLEDeviceDetails)
    {
        if (bLEDeviceDetails == null)
        {
            return;
        }

        if (!DevicesList.Any(x => x.Label == bLEDeviceDetails.DeviceId.ToString()))
        {
            var newDevice = new DeviceModel
            {
                Label = bLEDeviceDetails.DeviceId.ToString(),
                IsSelected = true
            };
            DevicesList.Add(newDevice);
        }

        if (DevicesList.FirstOrDefault(x => x.Label == bLEDeviceDetails.DeviceId.ToString()).IsSelected)
        {
            if (bLEDeviceDetails.Latitude != 0 || bLEDeviceDetails.Longitude != 0) // Add this check
            {
                Places.Add(GetExtendedPin(bLEDeviceDetails));
                PlacesMapsui.Add(GetExtendedPin2(bLEDeviceDetails));
            }
        }

        var res = await DataBase.SaveRecordToBaseAsync(bLEDeviceDetails);
    }

    private async Task<Location> GetUserLocation()
    {
        string errorMassage;

        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(3));
            var cts = new CancellationTokenSource();
            var location = await Geolocation.GetLastKnownLocationAsync();

            location ??= await Geolocation.GetLocationAsync(request, cts.Token);
            return location;
        }
        catch (FeatureNotSupportedException fnsEx)
        {
            errorMassage = fnsEx.Message;
            // Handle not supported on device exception
        }
        catch (FeatureNotEnabledException fneEx)
        {
            errorMassage = fneEx.Message;
            // Handle not enabled on device exception
        }
        catch (PermissionException pEx)
        {
            errorMassage = pEx.Message;
            // Handle permission exception
        }
        catch (Exception ex)
        {
            errorMassage = ex.Message;
            // Unable to get location
        }

        if (!string.IsNullOrEmpty(errorMassage))
        {
            await App.Current.MainPage.DisplayAlert("Error", errorMassage, "OK");
        }
        return null;
    }

    [RelayCommand]
    private async Task Settings()
    {
        await _navigationService.NavigateToConnectionPage();        
    }

    [RelayCommand]
    private async Task ShowUserLocation()
    {
        var userLocation = await Geolocation.GetLastKnownLocationAsync();
        if (userLocation == null)
        {
            var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(3));
            userLocation = await Geolocation.GetLocationAsync(request);
        }

        if (userLocation != null)
        {
            CurrentUserLocation = userLocation;
            ZoomPressed++;
        }
        else
        {
            // Handle case where location couldn't be determined
            Console.WriteLine("Unable to get current location.");
        }
    }

    protected override void Dispose(bool disposing)
    {
        
        if (disposing)
        {
            if (_bleConnectionService.CurrentDevice is not null)
            {
                _bleConnectionService.CurrentDevice.PropertyChanged -= OnDevicePropertyChanged;
                Console.WriteLine($"Device-PropertyChanded --- {_bleConnectionService.CurrentDevice.Device.Name}");
            }
            if (Places?.Any() == true)
                Places = new();
            if(DevicesList?.Any() == true)
                DevicesList = new();
            if(PlacesMapsui?.Any() == true)
                PlacesMapsui = new();
            _locationService.LocationChanged -= OnLocationChanged;
            _locationService.Stop();

            //_locationService.LocationChanged -= OnLocationChanged;
            //_locationService.StopListening();
        }
        base.Dispose(disposing);
    }

    [RelayCommand]
    private async Task SelectDevice(DeviceModel deviceModel)
    {
        if (!deviceModel.IsSelected)
        {
            var recordsByDevice = await DataBase.GetRecordsByDeviceId(int.Parse(deviceModel.Label));
            foreach (var record in recordsByDevice)
            {
                //PinsDetails.Add(pin);
                record.IsLastKnownDeviceLocation = recordsByDevice.IndexOf(record) == recordsByDevice.Count - 1;
                Device.BeginInvokeOnMainThread(() =>
                    Places.Add(GetExtendedPin(record, true)));

                Device.BeginInvokeOnMainThread(() =>
                    PlacesMapsui.Add(GetExtendedPin2(record, true)));
            }
        }
        else
        {
            var records = Places.Where(x => x.Details.DeviceId == int.Parse(deviceModel.Label)).ToList();
            foreach (var pin in records)
            {
                //PinsDetails.Remove(pin);
                Device.BeginInvokeOnMainThread(() =>
                    Places.Remove(Places.FirstOrDefault(x => x.Location.Latitude == pin.Details.Latitude && x.Location.Longitude == pin.Details.Longitude)));
                Device.BeginInvokeOnMainThread(() =>
                    PlacesMapsui.Remove(PlacesMapsui.FirstOrDefault(x => x.Position.Latitude == pin.Details.Latitude && x.Position.Longitude == pin.Details.Longitude)));
            }
        }
        deviceModel.IsSelected = !deviceModel.IsSelected;
    }

    private ExtendedPin GetExtendedPin(BLEDeviceDetails deviceDetails, bool isFromDB = false)
    {
        var pin = new ExtendedPin(
            new Location(deviceDetails.Latitude, deviceDetails.Longitude),
            deviceDetails.ToString(),
            ""
            );
        pin.Details = new BLEDeviceDetails
        {
            DeviceId = deviceDetails.DeviceId,
            Latitude = deviceDetails.Latitude,
            Longitude = deviceDetails.Longitude,
            BatteryLevel = deviceDetails.BatteryLevel,
            DateTime = deviceDetails.DateTime,
            IsUserLocation = false,
            IsLastKnownDeviceLocation = isFromDB ? deviceDetails.IsLastKnownDeviceLocation : true
        };
        if (Places.Count > 0 && !isFromDB)
        {
            try
            {
                var lastPin = Places.Where(x => x.Details.DeviceId == pin.Details.DeviceId).LastOrDefault();
                if (lastPin != null)
                {
                    lastPin.Details.IsLastKnownDeviceLocation = false;
                    Places.Remove(Places.Where(x => x.Details.DeviceId == pin.Details.DeviceId).Last());
                    Places.Add(lastPin);
                }
            }
            catch (Exception ex) 
            {
                var e = ex;
            }
            finally
            {
            }
        }

        return pin;
    }

    private ExtendedMapsuiPin GetExtendedPin2(BLEDeviceDetails deviceDetails, bool isFromDB = false)
    {
        var pin = new ExtendedMapsuiPin(
            new Mapsui.UI.Maui.Position(deviceDetails.Latitude, deviceDetails.Longitude),
            deviceDetails.ToString()
            );
        pin.Details = new BLEDeviceDetails
        {
            DeviceId = deviceDetails.DeviceId,
            Latitude = deviceDetails.Latitude,
            Longitude = deviceDetails.Longitude,
            BatteryLevel = deviceDetails.BatteryLevel,
            DateTime = deviceDetails.DateTime,
            IsUserLocation = false,
            IsLastKnownDeviceLocation = isFromDB ? deviceDetails.IsLastKnownDeviceLocation : true
        };
        if (PlacesMapsui.Count > 0 && !isFromDB)
        {
            try
            {
                var lastPin = PlacesMapsui.Where(x => x.Details.DeviceId == pin.Details.DeviceId).LastOrDefault();
                if (lastPin != null)
                {
                    lastPin.Details.IsLastKnownDeviceLocation = false;
                    PlacesMapsui.Remove(PlacesMapsui.Where(x => x.Details.DeviceId == pin.Details.DeviceId).Last());
                    PlacesMapsui.Add(lastPin);
                }
            }
            catch (Exception ex)
            {
                var e = ex;
            }
            finally
            {
            }
        }

        return pin;
    }
}