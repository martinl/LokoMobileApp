using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace loko.Models
{
    public partial class BLEDeviceDetails : ObservableObject
    {
        [ObservableProperty]
        private double _latitude;
        partial void OnLatitudeChanged(double value)
        {
            SendLocationChanged();
        }

        [ObservableProperty]
        private double _longitude;
        partial void OnLongitudeChanged(double value)
        {
            SendLocationChanged();
        }

        [ObservableProperty]
        private int _batteryLevel;

        [ObservableProperty]
        private bool _isUserLocation;

        [ObservableProperty]
        private bool _isLastKnownDeviceLocation;

        [ObservableProperty]
        private DateTime _dateTime;

        public event EventHandler LocationChanged;

        public int DeviceId { get; set; }

        public override string ToString()
        {
            return
                $"ID: {DeviceId}{Environment.NewLine}" +
                $"{nameof(Latitude)}: {Latitude:#.####}{Environment.NewLine}" +
                $"{nameof(Longitude)}: {Longitude:#.####}{Environment.NewLine}" +
                $"{nameof(BatteryLevel)}: {BatteryLevel}" + Environment.NewLine +
                $"Time: {DateTime:HH:mm}";
        }

        public void SendLocationChanged()
        {
            LocationChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}