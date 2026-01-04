using loko.Controls.Renderers;
using loko.Framework;
using loko.Helpers;
using loko.Models;
using loko.ViewModels;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using Mapsui.UI.Maui;
using Mapsui.Utilities;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;


//using Microsoft.Maui.Controls.Maps;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using static Microsoft.Maui.Controls.Device;
using Brush = Mapsui.Styles.Brush;
using Color = Mapsui.Styles.Color;

namespace loko.Views;

public partial class OSMPage : BaseContentPage<MapViewModel>
{
    private CancellationTokenSource _locationUpdateTokenSource;
    private bool _isUpdatingLocation;

    public static bool ZoomOnFirstPinData = true;
    public static bool InitialPersonZoom = true;
    private WritableLayer _pinLayer;
    private WritableLayer _calloutLayer;
    private TileLayer _customTileLayer;
    private CustomTileSource _customTileSource;
    public Dictionary<int, byte[]> iconDict = new Dictionary<int, byte[]>();
    public OSMPage(MapViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
        //var x = GetMarkerBitmapId();
        InitializeMapAsync();
        AdjustForStatusBar();
    }


    private async Task InitializeMapAsync()
    {
        try
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                SetupMapsui();
                mapsuiView.Refresh();
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing map: {ex.Message}");
        }
    }


    private void AdjustForStatusBar()
    {
        if (DeviceInfo.Platform == DevicePlatform.iOS)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
               
                MainGrid.Margin = new Thickness(0, -60, 0, -60);
            });
        }
    }
    private void SetupMapsui()
    {
        try
        {
            if (mapsuiView?.Map == null)
            {
                mapsuiView.Map = new Mapsui.Map();
            }
            else
            {
                mapsuiView.Map.Layers.Clear();
            }

            var viewModel = BindingContext as MapViewModel;
            double currentLat = viewModel?.CurrentUserLocation?.Latitude ?? 41.87;
            double currentLon = viewModel?.CurrentUserLocation?.Longitude ?? 12.56;


            double initialResolution = 305.74811309814453; // This corresponds to zoom level 12 in TMS

            var centerPosition = new Mapsui.UI.Maui.Position(currentLat, currentLat);
            var mapCenter = centerPosition.ToMapsui();

            // Use the Navigator to set the view
            mapsuiView.Map.Navigator.CenterOn(mapCenter.X, mapCenter.Y);
            mapsuiView.Map.Navigator.ZoomTo(initialResolution);

            mapsuiView.Refresh();


            var cacheDirectory = Path.Combine(FileSystem.AppDataDirectory, "MapTileCache");
            _customTileSource = new CustomTileSource(cacheDirectory, currentLat, currentLon);

            _customTileLayer = new TileLayer(_customTileSource) { Name = "Custom OSM Layer" };
            mapsuiView.Map.Layers.Add(_customTileLayer);

            _pinLayer = new WritableLayer { Name = "PinLayer" };
            mapsuiView.Map.Layers.Add(_pinLayer);
            Console.WriteLine("Added pin layer");

            _calloutLayer = new WritableLayer { Name = "CalloutLayer" };
            mapsuiView.Map.Layers.Add(_calloutLayer);

            mapsuiView.Map.Widgets.Clear();

          
            mapsuiView.IsClippedToBounds = false;
            mapsuiView.Margin = new Thickness(0);
            mapsuiView.VerticalOptions = LayoutOptions.Fill;
            mapsuiView.HorizontalOptions = LayoutOptions.Fill;

            mapsuiView.MyLocationEnabled = true;
            mapsuiView.MyLocationFollow = true;

            UpdateMapViewLocation();



            mapsuiView.Refresh();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SetupMapsui: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
        }
    }

    //private void SetInitialMapView()
    //{
    //    if (mapsuiView?.Map == null)
    //    {
    //        Console.WriteLine("Map is not initialized");
    //        return;
    //    }
    //    // Replace these values with the center of your downloaded area
    //    double centerLat = 40.0028; // Example
    //    double centerLon = 49.0060; 

    //    double initialResolution = 305.74811309814453; // This corresponds to zoom level 12 in TMS

    //    var centerPosition = new Mapsui.UI.Maui.Position(centerLat, centerLon);
    //    var mapCenter = centerPosition.ToMapsui();

    //    // Use the Navigator to set the view
    //    mapsuiView.Map.Navigator.CenterOn(mapCenter.X, mapCenter.Y);
    //    mapsuiView.Map.Navigator.ZoomTo(initialResolution);

    //    // Ensure the changes take effect
    //    mapsuiView.Map.Navigator.UpdateAnimations();
    //    mapsuiView.Refresh();
    //}



    private void HideCallout()
    {
        _calloutLayer.Clear();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is null)
            return;
        var viewModel = BindingContext as MapViewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        viewModel.PlacesMapsui.CollectionChanged += OnPlacesMapsuiCollectionChanged;
        UpdatePins(viewModel.PlacesMapsui);
    }


    private async Task StartPeriodicLocationUpdates()
    {
        if (_isUpdatingLocation)
            return;

        _isUpdatingLocation = true;
        _locationUpdateTokenSource = new CancellationTokenSource();

        while (!_locationUpdateTokenSource.IsCancellationRequested)
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location == null)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5));
                    location = await Geolocation.GetLocationAsync(request);
                }

                if (location != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (BindingContext is MapViewModel viewModel)
                        {
                            viewModel.CurrentUserLocation = location;
                            //UpdateMapViewLocationWithoutZoom();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting location: {ex.Message}");
            }

            await Task.Delay(3000, _locationUpdateTokenSource.Token); // Update every 3 seconds
        }

        _isUpdatingLocation = false;
    }


    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MapViewModel.CurrentUserLocation))
        {
            MainThread.BeginInvokeOnMainThread(UpdateMapViewLocationWithoutZoom);
        }
        else if (e.PropertyName == nameof(MapViewModel.ZoomPressed))
        {
            MainThread.BeginInvokeOnMainThread(UpdateMapViewLocation);
        }
    }

    private void OnPlacesMapsuiCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdatePins((sender as System.Collections.ObjectModel.ObservableCollection<ExtendedMapsuiPin>));
        });
    }

    /*
     * *********************************
     */
    public static int GetBitmapIdForMapsui(string imageName)
    {
        
        using (var stream = GetImageStream(imageName))
        {
            if (stream == null)
                throw new FileNotFoundException($"Image file not found: {imageName}");

            // Create a unique key for this image
            string key = $"image_{imageName}";

            // Check if the bitmap is already registered
            if (BitmapRegistry.Instance.TryGetBitmapId(key, out var bitmapId))
            {
                return bitmapId;
            }
            else
            {
                // If not registered, create a new bitmap and register it
                var bitmap = BitmapRegistry.Instance.Register(stream.ToBytes());
                return bitmap;
            }
        }
    }

    public static byte[] GetImageAsByteArrayMapsui(string imageName)
    {

        using (var stream = GetImageStream(imageName))
        {
            if (stream == null)
                throw new FileNotFoundException($"Image file not found: {imageName}");

            return stream.ToBytes();
        }
    }


    private static Stream GetImageStream(string imageName)
    {
        // First, try to get the image from the embedded resources
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(imageName, StringComparison.OrdinalIgnoreCase));

        if (resourceName != null)
            return assembly.GetManifestResourceStream(resourceName);

        // If not found in embedded resources, try to load from the file system
        var filePath = Path.Combine(FileSystem.AppDataDirectory, imageName);
        if (File.Exists(filePath))
            return File.OpenRead(filePath);

        // If still not found, return null
        return null;
    }


    private IStyle CreatePinStyle(string label)
    {
        var symbolStyle = new SymbolStyle
        {
            SymbolType = SymbolType.Image,
            BitmapId = GetBitmapIdForMapsui("userpin.png"),
            //Fill = new Brush(Mapsui.Styles.Color.Purple),
            //Outline = { Color = Mapsui.Styles.Color.White, Width = 0.5 },
            SymbolScale = 0.5
        };

        return symbolStyle;
    }

    private void UpdateMapViewLocation()
    {
        if (BindingContext is MapViewModel viewModel && viewModel.CurrentUserLocation != null)
        {
            var position = new Mapsui.UI.Maui.Position(viewModel.CurrentUserLocation.Latitude, viewModel.CurrentUserLocation.Longitude);
            mapsuiView.MyLocationLayer.UpdateMyLocation(position,true);
            var currentResolution = mapsuiView.Map.Navigator.Viewport.Resolution;
            const double desiredResolution = 2; // This is an example value

            if (InitialPersonZoom)
            {
                InitialPersonZoom = false;
                mapsuiView.Map.Navigator.ZoomTo(2);

            }
            else if (currentResolution > desiredResolution)
            {
                // Current view is zoomed out more than desired, so zoom in
                var newResolution = ZoomHelper.GetResolutionToZoomIn(mapsuiView.Map.Navigator.Resolutions, currentResolution);
                mapsuiView.Map.Navigator.ZoomTo(newResolution);
            }
            
            mapsuiView.Map.Navigator.CenterOn(position.ToMapsui().X, position.ToMapsui().Y);
            mapsuiView.Refresh();
        }
    }

    private void UpdateMapViewToFirstPinLocation(Position pinPosition)
    {
       
        mapsuiView.Map.Navigator.ZoomTo(1);
        mapsuiView.Map.Navigator.CenterOn(pinPosition.ToMapsui().X, pinPosition.ToMapsui().Y);
        mapsuiView.Refresh();
        
    }

    private void UpdateMapViewLocationWithoutZoom()
    {
        if (BindingContext is MapViewModel viewModel && viewModel.CurrentUserLocation != null)
        {
            var position = new Mapsui.UI.Maui.Position(viewModel.CurrentUserLocation.Latitude, viewModel.CurrentUserLocation.Longitude);
            mapsuiView.MyLocationLayer.UpdateMyLocation(position, true);
            mapsuiView.Refresh();
        }
    }


    //private void UpdateUserLocation()
    //{
    //    if(BindingContext == null)
    //    {
    //        return;
    //    }
    //    var viewModel = BindingContext as MapViewModel;
    //    if (viewModel?.CurrentUserLocation == null) return;

    //    var userPosition = new Location(viewModel.CurrentUserLocation.Latitude, viewModel.CurrentUserLocation.Longitude);
    //    double latitude = viewModel.CurrentUserLocation.Latitude;
    //    double longitude = viewModel.CurrentUserLocation.Longitude;
    //    var userPoint = SphericalMercator.FromLonLat(userPosition.Longitude, userPosition.Latitude);
    //    var pos = new Position(latitude, longitude);



    //    mapsuiView.MyLocationLayer.UpdateMyLocation(pos, true);
    //    mapsuiView.Map.Navigator.CenterOn(userPoint.x, userPoint.y);
    //    mapsuiView.Map.Navigator.ZoomTo(1000); // Adjust zoom level as needed
    //    mapsuiView.Refresh();

    //    //if (_pinLayer != null)
    //    //{
    //    //    _pinLayer.Clear();
    //    //    mapsuiView.Refresh();
    //    //}


    //    //if (userPosition != null && _pinLayer != null)
    //    //{
    //    //    var userLocationFeature = new PointFeature(userPoint.x, userPoint.y);
    //    //    userLocationFeature["Label"] = "Your location";
    //    //    userLocationFeature.Styles.Add(CreatePinStyle("Your location"));
    //    //    _pinLayer.Add(userLocationFeature);
    //    //    _customTileSource.UpdateLocation(userPosition.Latitude, userPosition.Longitude);

    //    //    mapsuiView.Map.Navigator.CenterOn(userPoint.x, userPoint.y);
    //    //    mapsuiView.Map.Navigator.ZoomTo(1000); // Adjust zoom level as needed
    //    //    mapsuiView.Refresh();
    //    //}

    //}


    private void UpdatePins(System.Collections.ObjectModel.ObservableCollection<ExtendedMapsuiPin> pins)
    {
        try
        {
            var userLocation = mapsuiView.MyLocationLayer.MyLocation;
            mapsuiView.Pins.Clear();

            if (pins == null || pins.Count == 0)
            {
                Console.WriteLine("Error: pins collection is null");
                return;
            }


            foreach (var pin in pins)
            {
                if (pin == null || pin.Position == null)
                {
                    Console.WriteLine($"Warning: Invalid pin {pin?.Label ?? "Unknown"}");
                    continue;
                }

                if (pin.Details.IsLastKnownDeviceLocation)
                {
                    pin.Type = PinType.Icon;
                    pin.Icon = GetCachedIcon(1, "locoLoc.png");
                    pin.Scale = 0.6f;
                }
                else
                {
                    pin.Type = PinType.Icon;
                    pin.Icon = GetCachedIcon(2, "redpin.png");
                    pin.Scale = 1f;
                }

                pin.Callout.Title = $"ID: {pin.Details.DeviceId}\n" +
                                    $"Latitude: {pin.Position.Latitude:F4}\n" +
                                    $"Longitude: {pin.Position.Longitude:F4}\n" +
                                    $"BatteryLevel: {pin.Details.BatteryLevel}\n" +
                                    $"Time: {pin.Details.DateTime:HH:mm}";
                pin.Callout.BackgroundColor = Microsoft.Maui.Graphics.Color.FromHex("#CC001226");
                pin.Callout.Color = Microsoft.Maui.Graphics.Color.FromHex("#12C04D");
                pin.Callout.ArrowWidth = 0;
                pin.Callout.ArrowHeight = 0;
                pin.Callout.RectRadius = 10;
                pin.Callout.TitleFontSize = 16;
                pin.Callout.TitleFontColor = Microsoft.Maui.Graphics.Color.FromHex("#FFFFFF");
                pin.Callout.TitleFontAttributes = FontAttributes.None;
                pin.Callout.StrokeWidth = 1;
                pin.Callout.TitleFontName = "MontserratLightFont";
                pin.Callout.SubtitleFontName = "MontserratLightFont";
                pin.Callout.TitleTextAlignment = TextAlignment.Start;
                pin.Callout.SubtitleTextAlignment = TextAlignment.Start;

                // Adjust padding to give some space around the text
                pin.Callout.Padding = new Thickness(15, 15, 15, 15);
                pin.Callout.Spacing = 1;

                mapsuiView.Pins.Add(pin);
                if (pin != null && pin.Position != null && ZoomOnFirstPinData)
                {
                    mapsuiView.Refresh();
                    ZoomOnFirstPinData = false;
                    UpdateMapViewToFirstPinLocation(pin.Position);
                }
            }

            if (userLocation != null)
            {
                mapsuiView.MyLocationLayer.UpdateMyLocation(userLocation, true);
            }

            // Refresh the map to show the new pins
            mapsuiView.Refresh();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdatePins: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
        }
    }

    private void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        HideAllCallouts();
    }

    private void OnPinClicked(object sender, Mapsui.UI.Maui.PinClickedEventArgs e)
    {
        if (e.Pin is ExtendedMapsuiPin clickedPin)
        {
            e.Pin.ShowCallout();
        }
    }

    private void HideAllCallouts()
    {
        foreach (var pin in mapsuiView.Pins)
        {
            pin.HideCallout();
        }
    }

    private byte[] GetCachedIcon(int iconKey, string iconName)
    {
        if (!iconDict.TryGetValue(iconKey, out byte[] iconData))
        {
            iconData = GetImageAsByteArrayMapsui(iconName);
            iconDict[iconKey] = iconData;
        }
        return iconData;
    }

    private void StopLocationUpdates()
    {
        _locationUpdateTokenSource?.Cancel();
        _isUpdatingLocation = false;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        mapsuiView.MapClicked += OnMapClicked;
        mapsuiView.PinClicked += OnPinClicked;

        if (BindingContext is not null)
        {
            var viewModel = BindingContext as MapViewModel;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            viewModel.PlacesMapsui.CollectionChanged += OnPlacesMapsuiCollectionChanged;
            UpdatePins(viewModel.PlacesMapsui);
        }
        



        _ = StartPeriodicLocationUpdates();

    }

    protected override void OnDisappearing()
    {
        mapsuiView.MapClicked -= OnMapClicked;
        mapsuiView.PinClicked -= OnPinClicked;
        StopLocationUpdates();

        if (BindingContext is not null)
        {
            var viewModel = BindingContext as MapViewModel;
            viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            viewModel.PlacesMapsui.CollectionChanged -= OnPlacesMapsuiCollectionChanged;
        }
        //_locationUpdateTokenSource?.Cancel();
        base.OnDisappearing();
    }

}
