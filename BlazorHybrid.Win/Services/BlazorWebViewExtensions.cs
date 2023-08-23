#if WEBVIEW2_WINFORMS
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.Web.WebView2.WinForms.WebView2;
#elif WEBVIEW2_WPF
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.Web.WebView2.Wpf.WebView2;
#endif

using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms; 
using System.Runtime.InteropServices;
using WebView2Control = Microsoft.Web.WebView2.WinForms.WebView2;
#nullable disable

namespace BlazorHybrid.Win.Shared;

public partial class InitBlazorWebView
{
    BlazorWebView _blazorWebView;

    protected string UploadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "uploads");

    public InitBlazorWebView(BlazorWebView blazorWebView)
    {
        _blazorWebView = blazorWebView;
        _blazorWebView.BlazorWebViewInitialized += BlazorWebViewInitialized;
        //_blazorWebView.BlazorWebViewInitializing += BlazorWebViewInitializing;
        _blazorWebView.UrlLoading += BlazorWebViewUrlLoading;
    }

    public virtual void BlazorWebViewUrlLoading(object? sender, UrlLoadingEventArgs e)
    {
        if (e.Url.Host != "0.0.0.0")
        {
            //外部链接WebView内打开,例如pdf浏览器
            Console.WriteLine(e.Url);
            e.UrlLoadingStrategy =
                UrlLoadingStrategy.OpenInWebView;
        }
    }

    void Initialize()
    {
        //[无依赖发布webview2程序] 固定版本运行时环境的方式来实现加载网页
        //设置运行时文件夹 
        var browserExecutableFolder = Path.Combine(Path.GetFullPath(".."), "WebView2.FixedVersionRuntime");
#if DEBUG
        browserExecutableFolder = @"C:\WebView2.FixedVersionRuntime";
#endif
        if (!Directory.Exists(browserExecutableFolder))
        {
            MessageBox.Show($"请先下载并解压缩 WebView2.FixedVersionRuntime.zip 到 {browserExecutableFolder} 文件夹", "提示");
            return;
        }
        var creationProperties = new CoreWebView2CreationProperties()
        {
            BrowserExecutableFolder = browserExecutableFolder
        };
        _blazorWebView.WebView.CreationProperties = creationProperties;

        //无依赖发布ok
        //无依赖单文件,提示path找不到 The path is empty. (Parameter 'path') at System.IO.Path.GetFullPath(String path)
    }

    void BlazorWebViewInitialized(object sender, BlazorWebViewInitializedEventArgs e)
    {
        //下载开始时引发 DownloadStarting，阻止默认下载
        e.WebView.CoreWebView2.DownloadStarting += CoreWebView2_DownloadStarting;

        //指定下载保存位置
        e.WebView.CoreWebView2.Profile.DefaultDownloadFolderPath = UploadPath;

        WinFormsService.WebView = e.WebView;

        //使用 JsBridge
        InitializeBridgeAsync(e.WebView);
    }

    private void CoreWebView2_DownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
    {
        var downloadOperation = e.DownloadOperation;
        string fileName = Path.GetFileName(e.ResultFilePath);
        var filePath = Path.Combine(UploadPath, fileName);

        //指定下载保存位置
        e.ResultFilePath = filePath;
        MessageBox.Show($"下载文件完成 {fileName}", "提示");
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

    }

    public class BridgeObject
    {
        public string MacAdress { get; set; } = Guid.NewGuid().ToString();
    }

    async void InitializeBridgeAsync(WebView2Control webView)
    {
        webView.CoreWebView2.AddHostObjectToScript("bridge", new Bridge());
        await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("var bridge= window.chrome.webview.hostObjects.bridge;");
        await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync($"localStorage.setItem('macAdress', '{obj.MacAdress}')");

    }

    #endregion
}
