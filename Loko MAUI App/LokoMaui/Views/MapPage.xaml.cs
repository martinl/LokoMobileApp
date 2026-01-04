using loko.Framework;
using loko.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.ComponentModel;
using Map = Microsoft.Maui.Controls.Maps.Map;

namespace loko.Views
{
    public partial class MapPage : BaseContentPage<MapViewModel>
    {

        private CancellationTokenSource _locationUpdateTokenSource;
        private bool _isUpdatingLocation;
        public static bool ZoomOnFirstPinData = true;

        public MapPage(MapViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
            AdjustForStatusBar();
            //var basePath = Path.Combine(FileSystem.AppDataDirectory, "MapTileCache", "baku", "12","2614");
            //var path = basePath;
            //Console.WriteLine(path);
            //var indent = "===>";

            //if (!Directory.Exists(basePath))
            //{
            //    Console.WriteLine($"{path}Directory does not exist: {path}");
            //    //return;
            //}

            //Console.WriteLine($"{indent}Contents of {path}:");

            //// List subdirectories
            //foreach (var dir in Directory.GetDirectories(path))
            //{
            //    Console.WriteLine($"{indent}  [D] {Path.GetFileName(dir)}");
            //    // Recursively list subdirectories (uncomment if you want to go deeper)
            //    // ListSubdirectories(dir, indent + "    ");
            //}

            //// List files (optional, comment out if you only want directories)
            //foreach (var file in Directory.GetFiles(path))
            //{
            //    Console.WriteLine($"{indent}  [F] {Path.GetFileName(file)}");
            //}

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

        private void StopLocationUpdates()
        {
            _locationUpdateTokenSource?.Cancel();
            _isUpdatingLocation = false;
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


        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if(BindingContext is null)
                return;
            (BindingContext as MapViewModel).PropertyChanged += OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((BindingContext as MapViewModel).CurrentUserLocation is null)
                return;
            var userPosition = new Location((BindingContext as MapViewModel).CurrentUserLocation.Latitude,
                (BindingContext as MapViewModel).CurrentUserLocation.Longitude);

            if (e.PropertyName == nameof(MapViewModel.ZoomPressed))
            {
                extMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(userPosition.Latitude, userPosition.Longitude),
                Distance.FromKilometers(1)));
            }
            if (e.PropertyName == nameof(MapViewModel.CurrentUserLocation))
            {
                

                if (e.PropertyName == nameof(MapViewModel.ZoomPressed))
                {
                    extMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(userPosition.Latitude, userPosition.Longitude),
                    Distance.FromKilometers(1)));
                }

                //var userLocationPin = extMap.Pins.FirstOrDefault(x => x.Label.Equals("Your location"));
                //if (userLocationPin == null)
                //{
                //    userLocationPin = new Pin
                //    {
                //        Label = "Your location",
                //        Location = userPosition,
                //        Type = PinType.Generic
                //    };

                //    extMap.Pins.Add(userLocationPin);
                //}
                //else
                //{


                //    userLocationPin.Location = userPosition;



                //}

                //extMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(userPosition.Latitude, userPosition.Longitude),
                //Distance.FromKilometers(1)));

                if (userPosition != null && ZoomOnFirstPinData)
                {
                    ZoomOnFirstPinData = false;
                    extMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(userPosition.Latitude, userPosition.Longitude),
                    Distance.FromKilometers(2)));
                }
                
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //mapsuiView.MapClicked += OnMapClicked;
            //mapsuiView.PinClicked += OnPinClicked;
            
            if (extMap != null)
            {
                extMap.IsShowingUser = true;
            }

            _ = StartPeriodicLocationUpdates();


        }

        protected override void OnDisappearing()
        {
            //mapsuiView.MapClicked -= OnMapClicked;
            //mapsuiView.PinClicked -= OnPinClicked;
            StopLocationUpdates();
            //_locationUpdateTokenSource?.Cancel();
            base.OnDisappearing();
        }
    }
}