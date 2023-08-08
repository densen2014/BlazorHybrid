namespace BlazorHybrid.Maui.Shared;

/// <summary>
/// 自定义NFC权限，MAUI暂不提供NFC权限
/// </summary>
public class NfcPermissions : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions => GetRequiredPermissions();

    //根据安卓平台版本，返回对应的申请权限
    private (string androidPermission, bool isRuntime)[] GetRequiredPermissions()
    {
        var permissions = new List<string>();
            permissions.Add(global::Android.Manifest.Permission.Nfc);
            permissions.Add(global::Android.Manifest.Permission.NfcTransactionEvent);
            permissions.Add(global::Android.Manifest.Permission.NfcPreferredPaymentInfo);
 
        var result = new List<(string androidPermission, bool isRuntime)>();
        foreach (var permission in permissions)
        {
            result.Add((permission, true));
        }

        return result.ToArray();
    }
}
