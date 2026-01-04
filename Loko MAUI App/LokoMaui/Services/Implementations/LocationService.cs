using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loko.Services.Implementations
{
    public class LocationService
    {
        private static bool _isListening;
        public event EventHandler<Location> LocationChanged;
        private GeolocationListeningRequest _request;

        public async Task Start()
        {
            if (_isListening)
                return;

            _request = new GeolocationListeningRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(1));

            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (status != PermissionStatus.Granted)
                        return;
                }

                Geolocation.LocationChanged += Geolocation_LocationChanged;
                if (_request is not null)
                {
                    _isListening = await Geolocation.StartListeningForegroundAsync(_request);
                }
         
            }
            catch (Exception ex)
            {
               
            }
        }

        public void Stop()
        {
            if (!_isListening)
                return;

            Geolocation.LocationChanged -= Geolocation_LocationChanged;
            Geolocation.StopListeningForeground();
            _isListening = false;
            Console.WriteLine("Stopped listening");
        }

        private void Geolocation_LocationChanged(object sender, GeolocationLocationChangedEventArgs args)
        {
            Console.WriteLine($"Location update received: {args.Location.Latitude}, {args.Location.Longitude}");
            LocationChanged?.Invoke(this, args.Location);
        }
    }
}
