namespace loko;

using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Maps;
using loko.Controls.Renderers;
using static Android.Gms.Maps.GoogleMap;

class MapCallbackHandler(CustomMapHandler mapHandler) : Java.Lang.Object, IOnMapReadyCallback
{
	public void OnMapReady(GoogleMap googleMap)
	{
		googleMap.UiSettings.ZoomControlsEnabled = false;
		googleMap.UiSettings.MyLocationButtonEnabled = false;
		mapHandler.UpdateValue(nameof(IMap.Pins));
		mapHandler.Map?.SetOnMarkerClickListener(new CustomMarkerClickListener(mapHandler));
		mapHandler.Map?.SetOnInfoWindowClickListener(new CustomInfoWindowClickListener(mapHandler));
		mapHandler.Map?.SetInfoWindowAdapter(new CustomInfoWindowAdapter(mapHandler));
    }
}

internal class CustomInfoWindowAdapter(CustomMapHandler mapHandler)
    : Java.Lang.Object, GoogleMap.IInfoWindowAdapter
{
    public View GetInfoContents(Marker marker)
    {
        return null;
    }

    public View GetInfoWindow(Marker marker)
    {
        if (Android.App.Application.Context.GetSystemService(Context.LayoutInflaterService) is LayoutInflater inflater)
        {
            var pin = mapHandler.Markers.FirstOrDefault(x => x.marker.Id == marker.Id).pin;
            if (pin == null)
                return null;
            if((pin as Microsoft.Maui.Controls.Maps.Pin).BindingContext is ExtendedPin ep)
            {

            var view = inflater.Inflate(Resource.Layout.MapInfoWindow, null);
            var infoDescription = view.FindViewById<TextView>(Resource.Id.InfoWindow_description);
            if (infoDescription != null)
            {
                infoDescription.Text = ep.Details?.ToString();
            }

            return view;
            }
        }
        return null;
    }
}
