using BlazorHybrid.Maui.Shared;

namespace BlazorHybrid.Maui;

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

    //private async void ButtonShowCounter_Click(object sender, EventArgs e)
    //{
    //    await DisplayAlert("Alert", "Current", "OK");
    //    await initBlazorWebView.ButtonShowCounter_Click($"Current counter value is: {MauiProgram._appState.Counter}"); 
    //}
  

    private async void ButtonWebviewAlert_Click(object sender, EventArgs e) => await initBlazorWebView.ExecuteScriptAsync();
    //private void ButtonWebviewAlert_Click(object sender, EventArgs e) => initBlazorWebView.InitializeBridgeAsync();

    private void ButtonHome_Click(object sender, EventArgs e) => initBlazorWebView.LoadUrl(null);
    //private void ButtonBing_Click(object sender, EventArgs e) => initBlazorWebView.LoadUrl("https://www.bing.com");

    //private async void Button_Clicked(object sender, EventArgs e)
    //{
    //    ContentPage page = new NfcPage();
    //    //await Application.Current.MainPage.Navigation.PopToRootAsync();
    //    //await Application.Current.MainPage.Navigation.PushAsync(page); 
    //    await Application.Current.MainPage.Navigation.PushModalAsync(page);

    //}
}
