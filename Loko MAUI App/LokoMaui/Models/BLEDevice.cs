using loko.Helpers;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;

namespace loko.Models
{
    public class BLEDevice : BindableBase
    {
        private BLEDeviceDetails _details;

        private bool _isConnected;

        private bool _isSwitchedOn;

        private bool _isSwitchingOn;

        private bool _isSwitchingOff;

        public BLEDevice(IDevice device, bool isConnected = false)
        {
            Device = device;

            Locations = new List<BLEDeviceDetails>();
        }

        public IDevice Device { get; }

        public ICharacteristic Characteristic { get; set; }

        public List<BLEDeviceDetails> Locations { get; set; }

        public BLEDeviceDetails Details
        {
            get => _details;
            set
            {
                _details = value;
                OnPropertyChanged(nameof(Details));
                OnPropertyChanged(nameof(Details.Latitude));
                OnPropertyChanged(nameof(Details.Longitude));
                OnPropertyChanged(nameof(Details.BatteryLevel));
                OnPropertyChanged(nameof(Details.DateTime));

                Console.WriteLine($"{nameof(Details.DeviceId)}: {Details.DeviceId}");
                Console.WriteLine($"{nameof(Details.Latitude)}: {Details.Latitude}");
                Console.WriteLine($"{nameof(Details.Longitude)}: {Details.Longitude}");
                Console.WriteLine($"{nameof(Details.BatteryLevel)}: {Details.BatteryLevel}");
                Console.WriteLine($"{nameof(Details.DateTime)}: {Details.DateTime}");
            }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        public bool IsSwitchedOn
        {
            get => _isSwitchedOn;
            set
            {
                _isSwitchedOn = value;
                OnPropertyChanged(nameof(IsSwitchedOn));
            }
        }

        public bool IsSwitchingOn
        {
            get => _isSwitchingOn;
            set
            {
                _isSwitchingOn = value;
                OnPropertyChanged(nameof(IsSwitchingOn));
            }
        }

        public bool IsSwitchingOff
        {
            get => _isSwitchingOff;
            set
            {
                _isSwitchingOff = value;
                OnPropertyChanged(nameof(IsSwitchingOff));
            }
        }
    }
}