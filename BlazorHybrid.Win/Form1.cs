// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using BlazorHybrid.Shared;
using Microsoft.Web.WebView2.Core;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Web.WebView2.WinForms;
using System.Windows.Forms;
#nullable disable

namespace BlazorHybrid.Win;

public partial class Form1 : Form
{
    protected string UploadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "uploads");
    BlazorWebView blazorWebView;
    public Form1()
    {
        InitializeComponent();
        dockTop.Visible = false;

        Text = "BlazorHybrid.Win";


        blazorWebView = new BlazorWebView()
        {
            Dock = DockStyle.Fill,
            HostPage = "wwwroot/index.html",
            Services = Startup.Services
        };

        //无依赖发布webview2程序
        //Initialize();

        blazorWebView.RootComponents.Add<App>("#app");
        Controls.Add(blazorWebView);
        blazorWebView.BringToFront();

        blazorWebView.BlazorWebViewInitialized += BlazorWebViewInitialized;

        blazorWebView.UrlLoading +=
            (sender, urlLoadingEventArgs) =>
            {
                if (urlLoadingEventArgs.Url.Host != "0.0.0.0")
                {
                    //外部链接WebView内打开,例如pdf浏览器
                    Console.WriteLine(urlLoadingEventArgs.Url);
                    urlLoadingEventArgs.UrlLoadingStrategy =
                        UrlLoadingStrategy.OpenInWebView;
                }
            };
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
        blazorWebView.WebView.CreationProperties = creationProperties;

        //无依赖发布ok
        //无依赖单文件,提示path找不到 The path is empty. (Parameter 'path') at System.IO.Path.GetFullPath(String path)
    }

    void BlazorWebViewInitialized(object sender, EventArgs e)
    {
        //下载开始时引发 DownloadStarting，阻止默认下载
        blazorWebView.WebView.CoreWebView2.DownloadStarting += CoreWebView2_DownloadStarting;

        //指定下载保存位置
        blazorWebView.WebView.CoreWebView2.Profile.DefaultDownloadFolderPath = UploadPath;

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
    private void ButtonShowCounter_Click(object sender, EventArgs e)
    {
        MessageBox.Show(
          owner: this,
          text: $"Current counter value is: {Startup._appState.Counter}",
          caption: "Counter");
    }

    private void ButtonWebviewAlert_Click(object sender, EventArgs e)
    {
        //blazorWebView.WebView.CoreWebView2.ExecuteScriptAsync("showAlert()");
        blazorWebView.WebView.CoreWebView2.ExecuteScriptAsync("alert('hello from native UI')");
    }

    private void ButtonHome_Click(object sender, EventArgs e)
    {
        blazorWebView.WebView.CoreWebView2.Navigate("https://0.0.0.0/");
    }
}
