namespace loko;

using CoreGraphics;
using CoreLocation;
using loko.Controls.Renderers;
using MapKit;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Maps.Platform;
using Microsoft.Maui.Platform;
using System.Collections.ObjectModel;
using UIKit;

public class CustomMapHandler : MapHandler
{
	private static UIView? _lastTouchedView;
    private static UIView _customPinView;
	private static ObservableCollection<ExtendedPin> _extendedPins;

    public static readonly IPropertyMapper<IMap, IMapHandler> CustomMapper =
		new PropertyMapper<IMap, IMapHandler>(Mapper)
		{
			[nameof(IMap.Pins)] = MapPins,
		};

	public CustomMapHandler() : base(CustomMapper, CommandMapper)
	{
	}

	public CustomMapHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null) : base(
		mapper ?? CustomMapper, commandMapper ?? CommandMapper)
	{
	}

	public List<IMKAnnotation> Markers { get; } = new();

	protected override void ConnectHandler(MauiMKMapView platformView)
	{
		base.ConnectHandler(platformView);
		platformView.GetViewForAnnotation += GetViewForAnnotations;
		platformView.DidSelectAnnotationView += OnDidSelectAnnotationView;
		platformView.DidDeselectAnnotationView += OnDidDeselectAnnotationView;
    }

    protected override void DisconnectHandler(MauiMKMapView platformView)
    {
        base.DisconnectHandler(platformView);
        platformView.GetViewForAnnotation = null;
        platformView.DidSelectAnnotationView -= OnDidSelectAnnotationView;
        platformView.DidDeselectAnnotationView -= OnDidDeselectAnnotationView;
    }

    private static void OnDidSelectAnnotationView(object sender, MKAnnotationViewEventArgs e)
    {
        var extendedMkAnnotationView = e.View as CustomAnnotation;
        if (extendedMkAnnotationView == null)
            return;

        _customPinView = new UIView();
        _customPinView.BackgroundColor = Color.FromHex("#CC001226").ToUIColor();
        _customPinView.Layer.CornerRadius = 8;
        _customPinView.Layer.MasksToBounds = true;
        _customPinView.Layer.BorderColor = Color.FromHex("#12C04D").ToCGColor();
        _customPinView.Layer.BorderWidth = 2;

        _customPinView.Frame = new CGRect(0, 0, 200, 120);
        var detailsLabel = new UILabel(new CGRect(0, 0, 200, 120));
        detailsLabel.Text = extendedMkAnnotationView.Details.ToString();
        detailsLabel.Lines = 5;
        detailsLabel.TextColor = Colors.White.ToUIColor();
        detailsLabel.Bounds = detailsLabel.Frame.Inset(20, 5);
        _customPinView.AddSubview(detailsLabel);

        if (e.View.Subviews.Any())
        {
            var extendedMkAnnotationViewSubview = e.View.Subviews[0];
            _customPinView.Center = new CGPoint(extendedMkAnnotationViewSubview.Center.X,
                -extendedMkAnnotationViewSubview.Bounds.Height);
            extendedMkAnnotationViewSubview.RemoveFromSuperview();
        }
        e.View.AddSubview(_customPinView);
    }

    void OnDidDeselectAnnotationView(object sender, MKAnnotationViewEventArgs e)
    {
        if (!e.View.Selected)
        {
            _customPinView?.RemoveFromSuperview();
            _customPinView?.Dispose();
            _customPinView = null;
        }
    }

	private static MKAnnotationView GetViewForAnnotations(MKMapView mapView, IMKAnnotation annotation)
	{
		

        MKAnnotationView annotationView;
        var extendedPin = GetExtendedPin(annotation as MKPointAnnotation);
        if (extendedPin == null)
        {
            annotationView = mapView.DequeueReusableAnnotation("test");

            if (annotationView == null)
            {
                annotationView = new MKAnnotationView(annotation, "test");

                annotationView.CalloutOffset = new CGPoint(0, 0);

            }
            annotationView.Image = UIImage.FromBundle("userpin.png");
            annotationView.Frame = new CGRect(0, 0, 15, 15);
            annotationView.CanShowCallout = true;

            return annotationView;
        }

        annotationView = mapView.DequeueReusableAnnotation(extendedPin.Details.ToString());
        if (annotationView == null)
        {
            annotationView = new CustomAnnotation(annotation, extendedPin.Details.ToString());
			
            annotationView.CalloutOffset = new CGPoint(0, 0);
            ((CustomAnnotation)annotationView).Details = extendedPin.Details;
        }
        if (extendedPin.Details.IsLastKnownDeviceLocation)
        {
            annotationView.Image = UIImage.FromBundle("locoloc.png");
            annotationView.Frame = new CGRect(0, 0, 20, 20);
        }
        else
            annotationView.Image = UIImage.FromBundle("redpin.png");
        annotationView.CanShowCallout = true;
        
		return annotationView;
	}	

	private static new void MapPins(IMapHandler handler, IMap map)
	{
		if (handler is CustomMapHandler mapHandler)
		{
			foreach (var marker in mapHandler.Markers)
			{
				mapHandler.PlatformView.RemoveAnnotation(marker);
			}

			mapHandler.Markers.Clear();
			mapHandler.AddPins(map.Pins);
		}
		_extendedPins = new ObservableCollection<ExtendedPin>();
		foreach (var pin in map.Pins)
		{
			if((pin as Microsoft.Maui.Controls.Maps.Pin).BindingContext is ExtendedPin ep)
            {
				_extendedPins.Add(ep);
			}
		}

	}

	private void AddPins(IEnumerable<IMapPin> mapPins)
	{
		if (MauiContext is null)
		{
			return;
		}

		foreach (var pin in mapPins)
		{
			var pinHandler = pin.ToHandler(MauiContext);
			if (pinHandler is IMapPinHandler mapPinHandler)
			{
				var markerOption = mapPinHandler.PlatformView;
				
				AddMarker(PlatformView, pin, Markers, markerOption);
			}
		}
	}

	private static void AddMarker(MauiMKMapView map, IMapPin pin, List<IMKAnnotation> markers, IMKAnnotation annotation)
	{
		map.AddAnnotation(annotation);
		pin.MarkerId = annotation;
		markers.Add(annotation);
	}

	private static ExtendedPin GetExtendedPin(MKPointAnnotation annotation)
	{
		if (_extendedPins is not null)
		{
			var position = new Location(annotation.Coordinate.Latitude, annotation.Coordinate.Longitude);
			foreach (var extendedPin in _extendedPins)
			{
				if (extendedPin.Location == position)
				{
					return extendedPin;
				}
			}
		}
		return null;
	}
}