using System.Threading.Tasks;

namespace loko.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> RequestBluetoothPermissions();

        Task<bool> RequestLocationPermissions();
    }
}