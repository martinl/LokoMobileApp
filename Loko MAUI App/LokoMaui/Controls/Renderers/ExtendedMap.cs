using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Map = Microsoft.Maui.Controls.Maps.Map;

namespace loko.Controls.Renderers
{
    public class ExtendedMap : Map
    {
        public List<ExtendedPin> ExtendedPins;
        public ExtendedMap()
        {
            ExtendedPins = new List<ExtendedPin>();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == "ItemsSource")
            {
                (ItemsSource as ObservableCollection<ExtendedPin>).CollectionChanged += MapItemsSource_CollectionChanged;
            }
        }

        private void MapItemsSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    var pin = (e.NewItems[0] as ExtendedPin);
                    if ((ItemsSource as ObservableCollection<ExtendedPin>).Count == 1)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        { 
                            MoveToRegion(MapSpan.FromCenterAndRadius(new Location(pin.Location.Latitude, pin.Location.Longitude),
                                                                              Distance.FromKilometers(10)));
                        });
                    }                    
                }
                if ((ItemsSource as ObservableCollection<ExtendedPin>).Count > 1)
                {
                    AddPolilynes();
                }
            }
            catch (Exception ex)
            {
                var esdf = ex;
            }
        }

        private readonly SemaphoreSlim _polilyneSemaphore = new SemaphoreSlim(1);
        private void AddPolilynes()
        {
            try
            {
                _polilyneSemaphore.Wait();
                MapElements.Clear();
                var ls = new List<ExtendedPin>((ItemsSource as ObservableCollection<ExtendedPin>)).ToList();
                var devices = ls.Select(x => x.Details.DeviceId).Distinct().ToList();
                foreach (var device in devices)
                {
                    var polilyne = new Polyline
                    {
                        StrokeColor = Color.FromArgb("#f01c1f"),
                        StrokeWidth = 2
                    };
                    foreach (ExtendedPin extendedPin in ls)
                    {
                        if (extendedPin.Details.DeviceId == device)
                            polilyne.Geopath.Add(extendedPin.Location);
                    }
                    MapElements.Add(polilyne);
                }
            }
            catch (Exception ex)
            {
                var e = ex;
            }
            finally 
            { 
                _polilyneSemaphore.Release(); 
            }
        }
    }
}