using loko.Models;
using Microsoft.Maui.Controls.Maps;

namespace loko.Controls.Renderers
{
    public class ExtendedPin : Pin
    {
        public BLEDeviceDetails Details { get; set; }
        public string Description { get; set; }

        public ExtendedPin(Location position, string address, string description)
        {
            Location = position;
            Address = address;
            Description = description;             
        }

        //public BLEDeviceDetails Details
        //{
        //    get => _details;
        //    set
        //    {
        //        if (_details != null)
        //        {
        //            // Unsubscribe from the old details instance
        //            _details.LocationChanged -= OnLocationChanged;
        //        }

        //        _details = value;

        //        if (_details != null)
        //        {
        //            // Subscribe to the new details instance
        //            _details.LocationChanged += OnLocationChanged;
        //            // Update pin location immediately to reflect current details
        //            this.Position = new Position(_details.Latitude, _details.Longitude);
        //        }
        //    }
        //}

        //private void OnLocationChanged(object sender, EventArgs e)
        //{
        //    // When the location changes, update the pin's position
        //    if (sender is BLEDeviceDetails details)
        //    {
        //        this.Position = new Position(details.Latitude, details.Longitude);
        //    }
        //}

    }
}