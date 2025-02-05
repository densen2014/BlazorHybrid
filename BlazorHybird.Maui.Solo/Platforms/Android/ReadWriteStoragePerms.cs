// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

namespace BlazorHybrid.Components;

/// <summary>
/// 请求读取和写入存储访问
/// </summary>
public class ReadWriteStoragePerms : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        new List<(string androidPermission, bool isRuntime)>
        {
                (global::Android.Manifest.Permission.ReadExternalStorage, true),
                (global::Android.Manifest.Permission.WriteExternalStorage, true)
        }.ToArray();
}
