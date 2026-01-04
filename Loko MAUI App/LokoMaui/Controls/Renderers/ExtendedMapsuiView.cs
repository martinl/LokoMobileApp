using loko.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = NetTopologySuite.Geometries.Point;
using Color = Mapsui.Styles.Color;
using Brush = Mapsui.Styles.Brush;
using Mapsui.Styles;
using Mapsui.Layers;
using Mapsui.UI.Maui;
using System.Collections.ObjectModel;
using NetTopologySuite.Geometries;
using Mapsui.Nts;
using System.Collections.Specialized;
using Mapsui.Extensions;
using Mapsui.Rendering.Skia;
//using Microsoft.Maui.Controls.Maps;

namespace loko.Controls.Renderers
{
    //public class ExtendedMapsuiView : MapView
    //{
    //    private ObservableCollection<ExtendedMapsuiPin> _extendedPins;
    //    private WritableLayer _pinLayer;
    //    private WritableLayer _polylineLayer;

    //    public ExtendedMapsuiView()
    //    {
    //        _extendedPins = new ObservableCollection<ExtendedMapsuiPin>();
    //        _pinLayer = new WritableLayer();
    //        _polylineLayer = new WritableLayer();
    //        Map.Layers.Add(_pinLayer);
    //        Map.Layers.Add(_polylineLayer);
    //    }


    //    //protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    //    //{
    //    //    base.OnPropertyChanged(propertyName);
    //    //    if (propertyName == nameof(ItemsSource))
    //    //    {
    //    //        if (ItemsSource is ObservableCollection<ExtendedPin> pins)
    //    //        {
    //    //            pins.CollectionChanged -= MapItemsSource_CollectionChanged; // Remove any existing handler
    //    //            pins.CollectionChanged += MapItemsSource_CollectionChanged; // Add the handler
    //    //        }
    //    //    }
    //    //}
    //    public ObservableCollection<ExtendedMapsuiPin> CustomItemsSource
    //    {
    //        get { return _extendedPins; }
    //        set
    //        {
    //            if (_extendedPins != null)
    //            {
    //                _extendedPins.CollectionChanged -= MapItemsSource_CollectionChanged;
    //            }
    //            _extendedPins = value ?? new ObservableCollection<ExtendedMapsuiPin>();
    //            _extendedPins.CollectionChanged += MapItemsSource_CollectionChanged;
    //            UpdatePins();
    //            AddPolylines();
    //        }
    //    }

    //    private void MapItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    //    {
    //        try
    //        {
    //            if (e.Action == NotifyCollectionChangedAction.Add)
    //            {
    //                var pin = e.NewItems[0] as ExtendedMapsuiPin;
    //                if (_extendedPins.Count == 1)
    //                {
    //                    MainThread.BeginInvokeOnMainThread(() =>
    //                    {
    //                        Map.Navigator.CenterOn(pin.Position.Latitude, pin.Position.Longitude);
    //                        Map.Navigator.ZoomTo(10000); // Equivalent to about 10km radius
    //                    });
    //                }
    //            }
    //            if (_extendedPins.Count > 1)
    //            {
    //                AddPolylines();
    //            }
    //            UpdatePins();
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"Error in MapItemsSource_CollectionChanged: {ex.Message}");
    //        }
    //    }

    //    private void UpdatePins()
    //    {
    //        _pinLayer.Clear();
    //        foreach (var pin in _extendedPins)
    //        {
    //            var feature = new PointFeature(pin.Position.Latitude, pin.Position.Longitude);
    //            feature.Styles.Add(CreatePinStyle(pin));
    //            _pinLayer.Add(feature);
    //        }
    //    }

    //    private IStyle CreatePinStyle(ExtendedMapsuiPin pin)
    //    {
    //        var symbolStyle = new SymbolStyle
    //        {
    //            SymbolType = SymbolType.Ellipse,
    //            Fill = new Brush(Color.Blue),
    //            Outline = { Color = Color.White, Width = 2 },
    //            SymbolScale = 0.5f
    //        };

    //        if (pin.Details?.IsLastKnownDeviceLocation == true)
    //        {
    //            symbolStyle.Fill = new Brush(Color.Green);
    //        }
    //        else if (pin.Label == "Your location")
    //        {
    //            symbolStyle.Fill = new Brush(Color.Red);
    //        }
    //        else
    //        {
    //            symbolStyle.Fill = new Brush(Mapsui.Styles.Color.Blue);
    //        }

    //        return symbolStyle;
    //    }

    //    private void AddPolylines()
    //    {
    //        _polylineLayer.Clear();
    //        var devices = _extendedPins.Select(x => x.Details.DeviceId).Distinct().ToList();
    //        foreach (var device in devices)
    //        {
    //            var coordinates = _extendedPins
    //                .Where(x => x.Details.DeviceId == device)
    //                .Select(x => new Coordinate(x.Position.Longitude, x.Position.Latitude))
    //                .ToArray(); // Convert to array here

    //            var polyline = new LineString(coordinates);

    //            var feature = new GeometryFeature(polyline);
    //            feature.Styles.Add(new VectorStyle
    //            {
    //                Line = new Pen(Color.FromArgb(255, 240, 28, 31), 2)
    //            });
    //            _polylineLayer.Add(feature);
    //        }
    //    }
    //}

    public class ExtendedMapsuiPin : Pin
    {
        public BLEDeviceDetails Details { get; set; }

        public ExtendedMapsuiPin(Mapsui.UI.Maui.Position position, string label)
        {
            Position = position;
            Label = label;
        }




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


    }
}
