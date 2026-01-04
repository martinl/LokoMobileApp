using System.Linq;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Graphics.Drawables;
using loko.Controls.Renderers;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Platform;
using IMap = Microsoft.Maui.Maps.IMap;

namespace loko;

public class CustomMapHandler : MapHandler
{

    private readonly Dictionary<string, BitmapDescriptor> _iconMap = [];

    public static readonly IPropertyMapper<IMap, IMapHandler> CustomMapper =
        new PropertyMapper<IMap, IMapHandler>(Mapper)
        {
            [nameof(IMap.Pins)] = MapPins
        };

    public CustomMapHandler() : base(CustomMapper, CommandMapper)
    {

    }

    public CustomMapHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null) : base(
        mapper ?? CustomMapper, commandMapper ?? CommandMapper)
    {
    }

    public List<(IMapPin pin, Marker marker)> Markers { get; } = new();

    protected override void ConnectHandler(MapView platformView)
    {
        base.ConnectHandler(platformView);
        var mapReady = new MapCallbackHandler(this);
        PlatformView.GetMapAsync(mapReady);        
    }    

    private static new void MapPins(IMapHandler handler, IMap map)
    {
        if (handler is CustomMapHandler mapHandler)
        {
            var pinsToAdd = map.Pins.Where(x => x.MarkerId == null).ToList();
            var pinsToRemove = mapHandler.Markers.Where(x => !map.Pins.Contains(x.pin)).ToList();
            foreach (var marker in pinsToRemove)
            {
                marker.marker.Remove();
                mapHandler.Markers.Remove(marker);
            }

            mapHandler.AddPins(pinsToAdd);
        }
    }

    private void AddPins(IEnumerable<IMapPin> mapPins)
    {
        if (Map is null || MauiContext is null)
        {
            return;
        }

        foreach (var pin in mapPins)
        {
            var pinHandler = pin.ToHandler(MauiContext);
            if (pinHandler is IMapPinHandler mapPinHandler)
            {
                var markerOption = mapPinHandler.PlatformView;
                if ((pin as Microsoft.Maui.Controls.Maps.Pin).BindingContext is ExtendedPin ep)
                {
                    if (ep.Details.IsLastKnownDeviceLocation)
                    {
                        markerOption.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.locoLoc));
                    }
                    else
                    {
                        markerOption.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.red_pin));
                        markerOption.Anchor(0.5f, 0.5f);
                    }
                }
                else
                {
                    if (pin.Label == "Your location")
                        markerOption.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.userLoc));
                }
                AddMarker(Map, pin, markerOption);                
            }
        }
    }

    private void AddMarker(GoogleMap map, IMapPin pin, MarkerOptions markerOption)
    {
        var marker = map.AddMarker(markerOption);
        pin.MarkerId = marker.Id;
        Markers.Add((pin, marker));
    }
    
}
