// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorHybrid.Components;

namespace BlazorHybird.Maui.Solo
{
    public partial class MainPage : ContentPage
    {
        InitBlazorWebView initBlazorWebView;

        public MainPage()
        {
            InitializeComponent();

            blazorWebView.HostPage = "wwwroot/index.html";
            initBlazorWebView = new InitBlazorWebView(blazorWebView);

            MauiFeatureService.Nfcs = new NfcPage();
        }

        private async void ButtonWebviewAlert_Click(object sender, EventArgs e) => await initBlazorWebView.ExecuteScriptAsync();
        //private void ButtonWebviewAlert_Click(object sender, EventArgs e) => initBlazorWebView.InitializeBridgeAsync();

        private void ButtonHome_Click(object sender, EventArgs e) => initBlazorWebView.LoadUrl(null);
        //private void ButtonBing_Click(object sender, EventArgs e) => initBlazorWebView.LoadUrl("https://www.bing.com");

    }
}
