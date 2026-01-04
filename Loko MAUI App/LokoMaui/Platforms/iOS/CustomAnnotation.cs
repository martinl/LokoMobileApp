namespace loko;

using loko.Models;
using MapKit;
using Microsoft.Maui.Maps;
using UIKit;

public class CustomAnnotation : MKAnnotationView
{
    public BLEDeviceDetails Details { get; set; }

    public CustomAnnotation(IMKAnnotation annotation, string id)
            : base(annotation, id)
    {
    }
}