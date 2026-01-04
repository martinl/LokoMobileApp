using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.Maui.ApplicationModel.Permissions;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace loko.Helpers.Extensions;

public static class RuntimePermission
{
    public static async Task<bool> IsPermissionGranted<T>(params T[] permissionList) where T : BasePermission
    {
        var result = true;

        foreach (var item in permissionList)
        {
            if (!await CheckAndRequestPermissionAsync(item))
            {
                result = false;
            }
        }

        return result;
    }

    public static async Task<bool> CheckAndRequestPermissionAsync<T>(T permission) where T : BasePermission
    {
        var status = await permission.CheckStatusAsync();
        if (status != PermissionStatus.Granted)
        {
            status = await permission.RequestAsync();
        }

        return status == PermissionStatus.Granted;
    }
}
