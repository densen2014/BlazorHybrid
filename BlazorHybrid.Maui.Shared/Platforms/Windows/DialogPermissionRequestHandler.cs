// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using DocumentFormat.OpenXml.Presentation;
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
            //CoreWebView2PermissionKind.MultipleAutomaticDownloads => "此应用程序需要自动下载多个文件。请根据要求授予权限.",
            //CoreWebView2PermissionKind.FileReadWrite => "此应用程序需要读取和写入设备上的文件或文件夹。请根据要求授予权限.",
            //CoreWebView2PermissionKind.Autoplay => "此应用程序需要站点上自动播放音频和视频。请根据要求授予权限.",
            //CoreWebView2PermissionKind.LocalFonts => "此应用程序需要使用Web字体。请根据要求授予权限.",
            //CoreWebView2PermissionKind.MidiSystemExclusiveMessages => "此应用程序需要向/从 MIDI（乐器数字接口）设备发送和接收系统独占消息。请根据要求授予权限.",
            //CoreWebView2PermissionKind.WindowManagement => "此应用程序需要在屏幕上打开和放置窗口。请根据要求授予权限.",
            _ => "未知资源",
        };

    //RT CoreWebView2 还没实现的权限

    //MultipleAutomaticDownloads	0x7	表示自动下载多个文件的权限。当快速连续触发多个下载时会请求许可。
    //FileReadWrite	0x8	表示读取和写入设备上的文件或文件夹的权限。当开发人员使用文件系统访问 API向最终用户显示文件或文件夹选择器，然后请求用户选择的“读写”权限时，会请求权限。
    //Autoplay	0x9	表示允许在站点上自动播放音频和视频。此权限会影响音频和视频 HTML 元素的自动播放属性和播放方法，以及 Web Audio API 的启动方法。有关详细信息，请参阅媒体和 Web 音频 API 的自动播放指南。
    //LocalFonts	0xa	表示在设备上使用字体的权限。当开发人员使用本地字体访问 API查询可用于设置 Web 内容样式的系统字体时，需要获得许可。
    //MidiSystemExclusiveMessages	0xb	表示允许向/从 MIDI（乐器数字接口）设备发送和接收系统独占消息。当开发人员使用Web MIDI API请求访问系统专有 MIDI 消息时，需要获得许可。
    //WindowManagement	0xc	表示允许在屏幕上打开和放置窗口。开发者使用多屏窗口放置API获取屏幕详细信息时需要获得许可。

}
