using loko.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace loko
{
    public class PermissionService : IPermissionService
    {
        public async Task<bool> RequestBluetoothPermissions()
        {
            var status = await Permissions.CheckStatusAsync<BluetoothSPermission>();
            if (status == PermissionStatus.Granted)
                return true;

            status = await Permissions.RequestAsync<BluetoothSPermission>();
            

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 11)
            {
                // On Android 11+ we can't request again if it's permanently denied
                return false;
            }
            return status == PermissionStatus.Granted;
        }

        public async Task<bool> RequestLocationPermissions()
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            return status == PermissionStatus.Granted;
        }
    }

    public class BluetoothSPermission : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions
        {
            get
            {
                var permissions = new List<(string, bool)>();

                if (OperatingSystem.IsAndroidVersionAtLeast(31)) // Android 12+
                {
                    permissions.Add((Android.Manifest.Permission.BluetoothScan, true));
                    permissions.Add((Android.Manifest.Permission.BluetoothConnect, true));
                    permissions.Add((Android.Manifest.Permission.BluetoothAdvertise, true));
                }
                else
                {
                    permissions.Add((Android.Manifest.Permission.Bluetooth, true));
                    permissions.Add((Android.Manifest.Permission.BluetoothAdmin, true));
                }

                // Location permissions are required for Bluetooth on Android < 12
                if (OperatingSystem.IsAndroidVersionAtLeast(31))
                {
                    permissions.Add((Android.Manifest.Permission.AccessFineLocation, true));
                    permissions.Add((Android.Manifest.Permission.AccessCoarseLocation, true));
                }

                return permissions.ToArray();
            }
        }
    }
}