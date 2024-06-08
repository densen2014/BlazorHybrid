// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Maui.Platform;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.WebView.Maui;
using System.Diagnostics;
using System.Runtime.InteropServices;
#if ANDROID
using Android.Webkit;
using AndroidX.Activity;
using AWebView = Android.Webkit.WebView;
#elif WINDOWS
using BlazorMaui.Platforms.Windows;
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.UI.Xaml.Controls.WebView2;
#elif IOS || MACCATALYST
using Foundation;
using WebKit;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
#elif TIZEN
using TWebView = Tizen.NUI.BaseComponents.WebView;
#elif WEBVIEW2_WINFORMS
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.Web.WebView2.WinForms.WebView2;
#elif WEBVIEW2_WPF
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.Web.WebView2.Wpf.WebView2;
#endif

namespace BlazorHybrid.Maui.Shared;

public partial class InitBlazorWebView : Page
{
    protected string UploadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "uploads");
    BlazorWebView? _blazorWebView;

#nullable disable

#if WINDOWS
    /// <summary>
    /// Gets the <see cref="WebView2Control"/> instance that was initialized.
    /// </summary>
    public WebView2Control WebView { get; internal set; }
#elif ANDROID
    /// <summary>
    /// Gets the <see cref="AWebView"/> instance that was initialized.
    /// </summary>
    public AWebView WebView { get; internal set; }
#elif MACCATALYST || IOS
    /// <summary>
    /// Gets the <see cref="WKWebView"/> instance that was initialized.
    /// the default values to allow further configuring additional options.
    /// </summary>
    public WKWebView WebView { get; internal set; }
#elif TIZEN
		/// <summary>
		/// Gets the <see cref="TWebView"/> instance that was initialized.
		/// </summary>
		public TWebView WebView { get; internal set; }
#endif
#nullable enable


    public InitBlazorWebView(BlazorWebView blazorWebView)
    {
        _blazorWebView = blazorWebView;
        _blazorWebView.BlazorWebViewInitialized += BlazorWebViewInitialized;
        _blazorWebView.BlazorWebViewInitializing += BlazorWebViewInitializing;
        _blazorWebView.UrlLoading += BlazorWebViewUrlLoading;
    }



    public virtual void BlazorWebViewInitialized(object? sender, BlazorWebViewInitializedEventArgs e)
    {
#if ANDROID
        if (e.WebView.Context?.GetActivity() is not ComponentActivity activity)
        {
            throw new InvalidOperationException($"The permission-managing WebChromeClient requires that the current activity be a '{nameof(ComponentActivity)}'.");
        }

        e.WebView.Settings.JavaScriptEnabled = true;
        e.WebView.Settings.AllowFileAccess = true;
        e.WebView.Settings.AllowFileAccessFromFileURLs = true;
        e.WebView.Settings.AllowUniversalAccessFromFileURLs = true;
        e.WebView.Settings.LightTouchEnabled = true;
        e.WebView.Settings.MixedContentMode = MixedContentHandling.AlwaysAllow;
        e.WebView.Settings.MediaPlaybackRequiresUserGesture = false;
        e.WebView.Settings.SetGeolocationEnabled(true);
        e.WebView.Settings.SetGeolocationDatabasePath(e.WebView.Context?.FilesDir?.Path);
        e.WebView.SetWebChromeClient(new BlazorWebviewPermissions(e.WebView.WebChromeClient!, activity));
        e.WebView.Download += async (s, e) => await WebView_DownloadAsync(s, e);
#elif WINDOWS
        e.WebView.CoreWebView2.DownloadStarting += (async (s, e) => await CoreWebView2_DownloadStartingAsync(s, e));
        var permissionHandler =
#if HANDLE_WEBVIEW2_PERMISSIONS_SILENTLY
        new SilentPermissionRequestHandler();
#else
        new DialogPermissionRequestHandler(e.WebView);
#endif

        e.WebView.CoreWebView2.PermissionRequested += permissionHandler.OnPermissionRequested;
#elif IOS

        //关闭回弹效果

        //用来控制滚动视图是否反弹，bounces默认是YES，当它为NO的时候，其他两个属性值设置无效，滚动视图无法反弹；只有当bounces是YES的时候，其他两个属性设置才有效
        //e.WebView.ScrollView.Bounces = false;
        //设置垂直方向的反弹是否有效.
        //e.WebView.ScrollView.AlwaysBounceVertical= false;
        //设置水平方向的反弹是否有效 
        //e.WebView.ScrollView.AlwaysBounceHorizontal = false;
        //如果设置为 true 且 Bounces 为 true，则在缩放超过该限制后，滚动视图将围绕缩放限制弹跳
        //e.WebView.ScrollView.BouncesZoom = false;
        // e.WebView.TintColor = UIKit.UIColor.Green;
        e.WebView.AllowsLinkPreview=true;
        e.WebView.AllowsBackForwardNavigationGestures = true; 
#endif
        WebView = e.WebView;

        MauiFeatureService.WebView = WebView;

        //使用 JsBridge
        InitializeBridgeAsync();
    }

    public virtual void BlazorWebViewInitializing(object? sender, BlazorWebViewInitializingEventArgs e)
    {
#if IOS || MACCATALYST
        //如果 true为 ，则 HTML5 视频可以内联播放，如果 false 则使用本机填充屏幕控制器。
        e.Configuration.AllowsInlineMediaPlayback = true;
        e.Configuration.AllowsAirPlayForMediaPlayback = true;
        e.Configuration.AllowsPictureInPictureMediaPlayback = true;
        e.Configuration.MediaTypesRequiringUserActionForPlayback = WebKit.WKAudiovisualMediaTypes.None;
        if (e.Configuration.DefaultWebpagePreferences != null)
        { 
            e.Configuration.DefaultWebpagePreferences.AllowsContentJavaScript=true;
        }
#endif
    }

    public virtual void BlazorWebViewUrlLoading(object? sender, UrlLoadingEventArgs e)
    {
        Debug.WriteLine(e.Url);
        //if (e.Url.Host != "0.0.0.0")
        //{
        //外部链接WebView内打开,例如pdf浏览器
        e.UrlLoadingStrategy = UrlLoadingStrategy.OpenInWebView;

        //拦截可处理 IOS || MACCATALYST 下载文件, 简单测试一下
        if (e.Url.ToString().EndsWith(".exe") || e.Url.ToString().EndsWith(".jpg") || e.Url.ToString().EndsWith(".png") || e.Url.Scheme == "blob")
        {
            Task.Run(async () => await DownloadAsync(e.Url));
        }
        //}
    }

    #region download
#if WINDOWS
    private async Task CoreWebView2_DownloadStartingAsync(object sender, CoreWebView2DownloadStartingEventArgs e)
    {
        var downloadOperation = e.DownloadOperation;
        string fileName = Path.GetFileName(e.ResultFilePath);
        var filePath = Path.Combine(UploadPath, fileName);

        //指定下载保存位置
        e.ResultFilePath = filePath;
        await DisplayAlert("提示", $"下载文件完成 {fileName}", "OK");
    }
#endif


#if ANDROID
    public virtual async Task WebView_DownloadAsync(object? sender, DownloadEventArgs e)
    {
        Uri uri = new Uri(e.Url!);
        await DownloadAsync(uri, e.Mimetype);
    }
#endif

    public virtual async Task DownloadAsync(string url, string? mimeType = null)
    {
        Uri uri = new Uri(url);
        await DownloadAsync(uri, mimeType);
    }

    public virtual async Task DownloadAsync(Uri uri, string? mimeType = null)
    {
        string fileName = Path.GetFileName(uri.LocalPath);
        var httpClient = new HttpClient();
        var filePath = Path.Combine(UploadPath, fileName);
#if ANDROID
        if (uri.Scheme == "data")
        {
            fileName = DataUrl2Filename(uri.OriginalString);
            filePath = Path.Combine(UploadPath, $"{DateTime.Now.ToString("yyyy-MM-dd-hhmmss")}-{fileName}");
            var bytes = DataUrl2Bytes(uri.OriginalString);
            File.WriteAllBytes(filePath, bytes);
            await DisplayAlert("提示", $"下载文件完成 {fileName}", "OK");
            return;
        }
#endif
        byte[] fileBytes = await httpClient.GetByteArrayAsync(uri);
        File.WriteAllBytes(filePath, fileBytes);
        await DisplayAlert("提示", $"下载文件完成 {fileName}", "OK");
    }

    public static string DataUrl2Filename(string base64encodedstring)
    {
        var filename = Regex.Match(base64encodedstring, @"data:text/(?<filename>.+?);(?<type2>.+?),(?<data>.+)").Groups["filename"].Value;
        return filename;
    }

    /// <summary>
    /// 从 DataUrl 转换为 Stream
    /// <para>Convert from a DataUrl to an Stream</para>
    /// </summary>
    /// <param name="base64encodedstring"></param>
    /// <returns></returns>
    public static byte[] DataUrl2Bytes(string base64encodedstring)
    {
        var base64Data = Regex.Match(base64encodedstring, @"data:text/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
        var bytes = Convert.FromBase64String(base64Data);
        return bytes;
    }

    #endregion

    public async Task ButtonShowCounter_Click(string msg)
    {
        await DisplayAlert("Alert", msg, "OK");
    }

    public async Task ExecuteScriptAsync(string js = "alert('hello from WebView JS')")
    {
#if WINDOWS 
        await WebView.ExecuteScriptAsync(js);
#elif ANDROID
        WebView.EvaluateJavaScript(new EvaluateJavaScriptAsyncRequest(js));
#elif MACCATALYST || IOS
        await WebView.EvaluateJavaScriptAsync(js);
#elif TIZEN
#endif
    }

    public void LoadUrl(string url)
    {
#if WINDOWS
        url ??= "https://0.0.0.0/";
        WebView.CoreWebView2.Navigate(url);
#elif ANDROID
        url ??= "https://0.0.0.0/";
        WebView.LoadUrl(url);
#elif MACCATALYST || IOS
        url ??= "app://0.0.0.0/";
        WebView.LoadRequest(new NSUrlRequest(new NSUrl(url)));
#elif TIZEN
#endif
    }


    #region JsBridge

    static BridgeObject obj = new BridgeObject();

    /// <summary>
    /// 自定义宿主类，用于向网页注册C#对象，供JS调用
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class Bridge
    {
        public string Func(string param) => $"Func返回 {param} {obj.MacAdress}";
        public string Print(object param) => $"Print返回 {param}";

    }

    public class BridgeObject
    {
        public string MacAdress { get; set; } = Guid.NewGuid().ToString(); 
    }

    public async void InitializeBridgeAsync()
    {

#if WINDOWS
        //解决方案如下:
        //https://stackoverflow.com/questions/72683537/corewebview2-addhostobjecttoscript-throws-system-exception?WT.mc_id=DT-MVP-5005078
        //https://learn.microsoft.com/en-us/microsoft-edge/webview2/how-to/winrt-from-js?tabs=winui3%2Cwinrtcsharp&WT.mc_id=DT-MVP-5005078

        //WebView.CoreWebView2.AddHostObjectToScript("bridge", new Bridge());
        //await WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("var bridge= window.chrome.webview.hostObjects.bridge;");
        //await WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync($"localStorage.setItem('macAdress', '{obj.MacAdress}')");
#elif ANDROID
#elif MACCATALYST || IOS
#elif TIZEN
#endif

    }

    #endregion
}
