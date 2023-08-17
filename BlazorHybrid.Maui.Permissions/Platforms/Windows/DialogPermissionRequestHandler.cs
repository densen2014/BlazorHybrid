// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace BlazorMaui.Platforms.Windows;

internal class DialogPermissionRequestHandler : IPermissionRequestHandler
{
    private readonly UIElement _parentElement;
    private readonly Dictionary<CoreWebView2PermissionKind, CoreWebView2PermissionState> _cachedPermissions = new();

    public DialogPermissionRequestHandler(UIElement parentElement)
    {
        _parentElement = parentElement;
    }

    public async void OnPermissionRequested(CoreWebView2 sender, CoreWebView2PermissionRequestedEventArgs args)
    {
        args.Handled = true;

        if (_cachedPermissions.TryGetValue(args.PermissionKind, out var permissionState) && permissionState == CoreWebView2PermissionState.Allow)
        {
            args.State = CoreWebView2PermissionState.Allow;
            return;
        }

        var deferral = args.GetDeferral();

        var dialog = new ContentDialog
        {
            XamlRoot = _parentElement.XamlRoot,
            Title = "权限请求",
            Content = $"{args.Uri} 正在请求访问 {GetPermissionName(args.PermissionKind)}",
            PrimaryButtonText = "允许",
            SecondaryButtonText = "拒绝",
        };

        var result = await dialog.ShowAsync();

        args.State = result == ContentDialogResult.Primary ?
            CoreWebView2PermissionState.Allow :
            CoreWebView2PermissionState.Deny;

        _cachedPermissions[args.PermissionKind] = args.State;

        deferral.Complete();
    }

    private static string GetPermissionName(CoreWebView2PermissionKind permissionKind)
        => permissionKind switch
        {
            CoreWebView2PermissionKind.Microphone => "此应用程序需要访问您的麦克风。请根据要求授予权限.",
            CoreWebView2PermissionKind.Camera => "此应用程序需要访问您的相机。请根据要求授予权限.",
            CoreWebView2PermissionKind.Geolocation => "此应用程序需要访问您的位置。请根据要求授予权限.",
            CoreWebView2PermissionKind.Notifications => "此应用程序需要启用通知。请根据要求授予权限.",
            CoreWebView2PermissionKind.OtherSensors => "此应用程序需要访问您设备的通用传感器。请根据要求授予权限.",
            CoreWebView2PermissionKind.ClipboardRead => "此应用程序需要访问您的剪贴板。请根据要求授予权限.",
            _ => "未知资源",
        };
}
