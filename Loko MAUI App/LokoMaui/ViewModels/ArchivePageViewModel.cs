using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using loko.Controls.Renderers;
using loko.Framework;
using loko.Models;
using loko.Services.Implementations;
using loko.Services.Interfaces;
using Microsoft.Maui.Maps;
using System.Collections.ObjectModel;

namespace loko.ViewModels;

public partial class ArchivePageViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<ExtendedPin> _mapItemsSource;

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private ObservableCollection<DeviceModel> _devicesList;

    [ObservableProperty]
    private MapType _mapType;

    private DeviceDetailsDB DataBase;

    public ArchivePageViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        MapItemsSource = new();
        DevicesList = new();
    }

    public async override Task InitializeAsync()
    {
        await base.InitializeAsync();
        await Init();        

        MapType = MauiProgram.MapType;
    }

    public override Task UnInitializeAsync()
    {
        return base.UnInitializeAsync();
    }


    public async Task Init()
    {
        await GetDBListDates();
    }

    public override async Task GoBackTask()
    {
        Dispose();

        await _navigationService.NavigateBack();
    }

    private async Task GetDBListDates()
    {
        DataBase = await DeviceDetailsDB.Instance;
        var records = await DataBase.GetRecordsByDate(Title);
        var ids = records.Select(x => x.DeviceId).Distinct().ToList();

        foreach (var id in ids)
        {
            DevicesList.Add(new DeviceModel
            {
                Label = id.ToString(),
                IsSelected = true,
            });
            var points = records.Where(x => x.DeviceId == id).ToList();
            foreach (var point in points)
            {
                point.IsLastKnownDeviceLocation = points.IndexOf(point) == points.Count - 1;
                MapItemsSource.Add(GetExtendedPin(point));
            }
        };
    }

    private ExtendedPin GetExtendedPin(BLEDeviceDetails deviceDetails)
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
            IsLastKnownDeviceLocation = deviceDetails.IsLastKnownDeviceLocation
        };

        return pin;
    }

    [RelayCommand]
    private async Task SelectDevice(DeviceModel deviceModel)
    {
        if (!deviceModel.IsSelected)
        {
            var records = await DataBase.GetRecordsByDate(Title);
            var pins = records.Where(x => x.DeviceId == int.Parse(deviceModel.Label)).ToList();
            foreach (var pin in pins)
            {
                if (pins.IndexOf(pin) == pins.Count - 1)
                {
                    pin.IsLastKnownDeviceLocation = true;
                }
                MapItemsSource.Add(GetExtendedPin(pin));
            }
        }
        else
        {
            var records = MapItemsSource.Where(x => x.Details.DeviceId == int.Parse(deviceModel.Label)).ToList();
            foreach (var pin in records)
            {
                MapItemsSource.Remove(MapItemsSource.FirstOrDefault(x => x.Location.Latitude == pin.Details.Latitude && x.Location.Longitude == pin.Details.Longitude));
            }
        }
        deviceModel.IsSelected = !deviceModel.IsSelected;
    }

    
}
