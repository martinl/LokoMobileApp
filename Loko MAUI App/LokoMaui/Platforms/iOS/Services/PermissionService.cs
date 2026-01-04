using loko.Services.Interfaces;

namespace loko;

public class PermissionService : IPermissionService
{
    public Task<bool> RequestBluetoothPermissions()
    {
        return Task.FromResult(true);
    }

    public Task<bool> RequestLocationPermissions()
    {
        return Task.FromResult(true);
    }
}