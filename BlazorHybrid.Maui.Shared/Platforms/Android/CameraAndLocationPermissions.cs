// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using Android.Webkit;

namespace BlazorHybrid.Maui.Shared;

/// <summary>
/// 请求摄像机和位置
/// </summary>
public class CameraAndLocationPerms : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        new List<(string androidPermission, bool isRuntime)>
        {
                            (global::Android.Manifest.Permission.Camera, true),
                            (global::Android.Manifest.Permission.CaptureAudioOutput, true),
                            (global::Android.Manifest.Permission.CaptureSecureVideoOutput, true),
                            (global::Android.Manifest.Permission.CaptureVideoOutput, true),
                            (global::Android.Manifest.Permission.LocationHardware, true),
                            (global::Android.Manifest.Permission.AccessFineLocation, true),
                            (global::Android.Manifest.Permission.AccessLocationExtraCommands, true),
                            (global::Android.Manifest.Permission.AccessNetworkState, true),
                            (global::Android.Manifest.Permission.CallPhone, true),
                            (global::Android.Manifest.Permission.Flashlight, true),
                            (global::Android.Manifest.Permission.RecordAudio, true),
                            (global::Android.Manifest.Permission.Vibrate , true),
                            (global::Android.Manifest.Permission.WriteSettings , true),
        }.ToArray();
}


public class MauiWebChromeClient : WebChromeClient
{
    public override void OnPermissionRequest(PermissionRequest? request)
    {
        request?.Grant(request.GetResources());
    }
}


