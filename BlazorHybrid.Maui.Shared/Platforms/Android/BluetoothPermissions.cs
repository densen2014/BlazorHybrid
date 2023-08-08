namespace BlazorHybrid.Maui.Shared;

/// <summary>
/// 自定义蓝牙权限，MAUI暂不提供蓝牙权限
/// </summary>
public class BluetoothPermissions : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions => GetRequiredPermissions();

    //根据安卓平台版本，返回对应的申请权限
    private (string androidPermission, bool isRuntime)[] GetRequiredPermissions()
    {
        var permissions = new List<string>();

        if (DeviceInfo.Version.Major >= 12)
        {
            // Android 版本大于等于 12 时，申请新的蓝牙权限
            permissions.Add(global::Android.Manifest.Permission.BluetoothScan);
            permissions.Add(global::Android.Manifest.Permission.BluetoothConnect);
        }
        else
        {
            //csproj文件指定SupportedOSPlatformVersion android 28.0可以继续使用安卓9的权限
            permissions.Add(global::Android.Manifest.Permission.Bluetooth);
            permissions.Add(global::Android.Manifest.Permission.BluetoothAdmin);
            permissions.Add(global::Android.Manifest.Permission.AccessCoarseLocation);
            permissions.Add(global::Android.Manifest.Permission.AccessFineLocation);
        }

        var result = new List<(string androidPermission, bool isRuntime)>();
        foreach (var permission in permissions)
        {
            result.Add((permission, true));
        }

        return result.ToArray();
    }
}
