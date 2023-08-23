using LibraryShared;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Windows;
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

    void BlazorWebViewInitialized(object sender, BlazorWebViewInitializedEventArgs e)
    {
        //下载开始时引发 DownloadStarting，阻止默认下载
        e.WebView.CoreWebView2.DownloadStarting += CoreWebView2_DownloadStarting;

        //指定下载保存位置
        e.WebView.CoreWebView2.Profile.DefaultDownloadFolderPath = UploadPath;

        ////[无依赖发布webview2程序] 固定版本运行时环境的方式来实现加载网页
        ////设置web用户文件夹 
        //var browserExecutableFolder = "c:\\wb2";
        //var userData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BlazorWinFormsApp");
        //Directory.CreateDirectory(userData);
        //var creationProperties = new CoreWebView2CreationProperties()
        //{
        //    UserDataFolder = userData,
        //    BrowserExecutableFolder = browserExecutableFolder
        //};
        //mainBlazorWebView.WebView.CreationProperties = creationProperties;

        WpfService.WebView = e.WebView;
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
}
