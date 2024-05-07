// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using Android;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Webkit;
using AndroidX.Activity;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using AndroidX.Core.Content;
using Java.Interop;
using View = Android.Views.View;
using WebView = Android.Webkit.WebView;

namespace BlazorHybrid.Maui.Shared;

internal class BlazorWebviewPermissions : WebChromeClient, IActivityResultCallback
{
    // 该类实现了一个匹配工作流推荐的权限请求工作流
    // 由官方 Android 开发者文档提供。
    // 请参阅：https://developer.android.com/training/permissions/requesting#workflow 请求权限
    // 当前实现支持位置、摄像头和麦克风权限。要添加你自己的，
    // 更新 s rationales By Permission 字典以包含您要求许可的理由。
    // 如有必要，您可能还需要更新 s required Permissions By Webkit Resource 以定义特定的方式
    // Webkit 资源映射到 Android 权限。

    // 在真实的应用程序中，您可能会针对您的应用程序的功能使用更有说服力的理由。
    private const string CameraAccessRationale = "此应用程序需要访问您的相机。请根据要求授予权限.";
    private const string LocationAccessRationale = "此应用程序需要访问您的位置。请根据要求授予权限.";
    private const string MicrophoneAccessRationale = "此应用程序需要访问您的麦克风。请根据要求授予权限.";
    private const string VideoAccessRationale = "此应用程序需要访问您的相机录像。请根据要求授予权限.";

    private static readonly Dictionary<string, string> s_rationalesByPermission = new()
    {
        [Manifest.Permission.Camera] = CameraAccessRationale,
        [Manifest.Permission.AccessFineLocation] = LocationAccessRationale,
        [Manifest.Permission.RecordAudio] = MicrophoneAccessRationale,
        [Manifest.Permission.ModifyAudioSettings] = MicrophoneAccessRationale,
        [Manifest.Permission.CaptureVideoOutput] = VideoAccessRationale,
        // 添加更多支持权限时添加更多理由。
    };

    private static readonly Dictionary<string, string[]> s_requiredPermissionsByWebkitResource = new()
    {
        [PermissionRequest.ResourceVideoCapture] = new[] { Manifest.Permission.Camera },
        [PermissionRequest.ResourceAudioCapture] = new[] { Manifest.Permission.ModifyAudioSettings, Manifest.Permission.RecordAudio },
        // 根据需要添加更多 Webkit 资源 -> Android 权限映射。
    };

    private readonly WebChromeClient _blazorWebChromeClient;
    private readonly ComponentActivity _activity;
    private readonly ActivityResultLauncher _requestPermissionLauncher;

    private Action<bool>? _pendingPermissionRequestCallback;

    public BlazorWebviewPermissions(WebChromeClient blazorWebChromeClient, ComponentActivity activity)
    {
        _blazorWebChromeClient = blazorWebChromeClient;
        _activity = activity;
        _requestPermissionLauncher = _activity.RegisterForActivityResult(new ActivityResultContracts.RequestPermission(), this);
    }

    public override void OnCloseWindow(Android.Webkit.WebView? window)
    {
        _blazorWebChromeClient.OnCloseWindow(window);
        _requestPermissionLauncher.Unregister();
    }

    public override void OnGeolocationPermissionsShowPrompt(string? origin, GeolocationPermissions.ICallback? callback)
    {
        ArgumentNullException.ThrowIfNull(callback, nameof(callback));

        RequestPermission(Manifest.Permission.AccessFineLocation, isGranted => callback.Invoke(origin, isGranted, false));
    }
    //public override void OnPermissionRequest(PermissionRequest? request)
    //{
    //    request.Grant(request.GetResources());
    //}

    public override void OnPermissionRequest(PermissionRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        if (request.GetResources() is not { } requestedResources)
        {
            request.Deny();
            return;
        }

        RequestAllResources(requestedResources, grantedResources =>
        {
            if (grantedResources.Count == 0)
            {
                request.Deny();
            }
            else
            {
                request.Grant(grantedResources.ToArray());
            }
        });
    }

    private void RequestAllResources(Memory<string> requestedResources, Action<List<string>> callback)
    {
        if (requestedResources.Length == 0)
        {
            // 没有可请求的资源 - 使用空列表调用回调。
            callback(new());
            return;
        }

        var currentResource = requestedResources.Span[0];
        var requiredPermissions = s_requiredPermissionsByWebkitResource.GetValueOrDefault(currentResource, Array.Empty<string>());

        RequestAllPermissions(requiredPermissions, isGranted =>
        {
            // 递归剩余的资源。如果第一个资源被授予，使用修改后的回调
            // 将第一个资源添加到授予的资源列表中。
            RequestAllResources(requestedResources[1..], !isGranted ? callback : grantedResources =>
            {
                grantedResources.Add(currentResource);
                callback(grantedResources);
            });
        });
    }

    private void RequestAllPermissions(Memory<string> requiredPermissions, Action<bool> callback)
    {
        if (requiredPermissions.Length == 0)
        {
            // 没有权限请求 - 成功！
            callback(true);
            return;
        }

        RequestPermission(requiredPermissions.Span[0], isGranted =>
        {
            if (isGranted)
            {
                // 递归剩余的权限。
                RequestAllPermissions(requiredPermissions[1..], callback);
            }
            else
            {
                // 未授予第一个所需的权限。现在失败，不要尝试授予
                // 剩余的权限。
                callback(false);
            }
        });
    }

    private void RequestPermission(string permission, Action<bool> callback)
    {
        // 此方法实现此处描述的工作流程：
        // https://developer.android.com/training/permissions/requesting#workflow_for_requesting_permissions

        if (ContextCompat.CheckSelfPermission(_activity, permission) == Permission.Granted)
        {
            callback.Invoke(true);
        }
        else if (_activity.ShouldShowRequestPermissionRationale(permission) && s_rationalesByPermission.TryGetValue(permission, out var rationale))
        {
            new AlertDialog.Builder(_activity)
                .SetTitle("启用应用权限")!
                .SetMessage(rationale)!
                .SetNegativeButton("不，谢谢", (_, _) => callback(false))!
                .SetPositiveButton("继续", (_, _) => LaunchPermissionRequestActivity(permission, callback))!
                .Show();
        }
        else
        {
            LaunchPermissionRequestActivity(permission, callback);
        }
    }

    private void LaunchPermissionRequestActivity(string permission, Action<bool> callback)
    {
        if (_pendingPermissionRequestCallback is not null)
        {
            throw new InvalidOperationException("不能同时执行多个权限请求.");
        }

        _pendingPermissionRequestCallback = callback;
        _requestPermissionLauncher.Launch(permission);
    }

    void IActivityResultCallback.OnActivityResult(Java.Lang.Object? isGranted)
    {
        var callback = _pendingPermissionRequestCallback;
        _pendingPermissionRequestCallback = null;
        callback?.Invoke((bool)isGranted);
    }

    #region Unremarkable overrides
    // See: https://github.com/dotnet/maui/issues/6565
    public override JniPeerMembers JniPeerMembers => _blazorWebChromeClient.JniPeerMembers;
    public override Bitmap? DefaultVideoPoster => _blazorWebChromeClient.DefaultVideoPoster;
    public override Android.Views.View? VideoLoadingProgressView => _blazorWebChromeClient.VideoLoadingProgressView;
    public override void GetVisitedHistory(IValueCallback? callback)
        => _blazorWebChromeClient.GetVisitedHistory(callback);
    //public override bool OnConsoleMessage(ConsoleMessage? consoleMessage)
    //    => _blazorWebChromeClient.OnConsoleMessage(consoleMessage);
    public override bool OnCreateWindow(WebView? view, bool isDialog, bool isUserGesture, Message? resultMsg)
        => _blazorWebChromeClient.OnCreateWindow(view, isDialog, isUserGesture, resultMsg);
    public override void OnGeolocationPermissionsHidePrompt()
        => _blazorWebChromeClient.OnGeolocationPermissionsHidePrompt();
    public override void OnHideCustomView()
        => _blazorWebChromeClient.OnHideCustomView();
    public override bool OnJsAlert(WebView? view, string? url, string? message, JsResult? result)
        => _blazorWebChromeClient.OnJsAlert(view, url, message, result);
    public override bool OnJsBeforeUnload(WebView? view, string? url, string? message, JsResult? result)
        => _blazorWebChromeClient.OnJsBeforeUnload(view, url, message, result);
    public override bool OnJsConfirm(WebView? view, string? url, string? message, JsResult? result)
        => _blazorWebChromeClient.OnJsConfirm(view, url, message, result);
    public override bool OnJsPrompt(WebView? view, string? url, string? message, string? defaultValue, JsPromptResult? result)
        => _blazorWebChromeClient.OnJsPrompt(view, url, message, defaultValue, result);
    public override void OnPermissionRequestCanceled(PermissionRequest? request)
        => _blazorWebChromeClient.OnPermissionRequestCanceled(request);
    public override void OnProgressChanged(WebView? view, int newProgress)
        => _blazorWebChromeClient.OnProgressChanged(view, newProgress);
    public override void OnReceivedIcon(WebView? view, Bitmap? icon)
        => _blazorWebChromeClient.OnReceivedIcon(view, icon);
    public override void OnReceivedTitle(WebView? view, string? title)
        => _blazorWebChromeClient.OnReceivedTitle(view, title);
    public override void OnReceivedTouchIconUrl(WebView? view, string? url, bool precomposed)
        => _blazorWebChromeClient.OnReceivedTouchIconUrl(view, url, precomposed);
    public override void OnRequestFocus(WebView? view)
        => _blazorWebChromeClient.OnRequestFocus(view);
    public override void OnShowCustomView(View? view, ICustomViewCallback? callback)
        => _blazorWebChromeClient.OnShowCustomView(view, callback);
    public override bool OnShowFileChooser(WebView? webView, IValueCallback? filePathCallback, FileChooserParams? fileChooserParams)
        => _blazorWebChromeClient.OnShowFileChooser(webView, filePathCallback, fileChooserParams);
    #endregion
}
