// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorHybrid.Shared;
using BlazorHybrid.Win.Shared;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
#nullable disable

namespace BlazorHybrid.Win;

public partial class Form1 : Form
{
    BlazorWebView blazorWebView;

    InitBlazorWebView initBlazorWebView;

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

        initBlazorWebView = new InitBlazorWebView(blazorWebView);
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
