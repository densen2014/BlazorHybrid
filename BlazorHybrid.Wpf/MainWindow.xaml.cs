using BlazorHybrid.Win.Shared;
using LibraryShared;
using System.Windows;
#nullable disable

namespace BlazorHybrid.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    InitBlazorWebView initBlazorWebView;

    public MainWindow()
    {

        Resources.Add("services", Startup.Services);
        InitializeComponent();

        dockTop.Visibility = Visibility.Collapsed;

        blazorWebView.HostPage = "wwwroot/index.html";
        blazorWebView.Services = Startup.Services;

        initBlazorWebView = new InitBlazorWebView(blazorWebView);

    }

    private void ButtonShowCounter_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
          owner: this,
          messageBoxText: $"Current counter value is: {Startup._appState.Counter}",
          caption: "Counter");
    }

    private void ButtonWebviewAlert_Click(object sender, RoutedEventArgs e)
    {
        //blazorWebView.WebView.CoreWebView2.ExecuteScriptAsync("showAlert()");
        blazorWebView.WebView.CoreWebView2.ExecuteScriptAsync("alert('hello from native UI')");
    }

    private void ButtonHome_Click(object sender, RoutedEventArgs e)
    {
        blazorWebView.WebView.CoreWebView2.Navigate("https://0.0.0.0/");
    }
}
