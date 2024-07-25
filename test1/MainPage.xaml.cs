namespace test1;
using WebViewNativeApi;

public partial class MainPage : TabbedPage
{
    private NativeBridge? api;

    public MainPage()
    {
        InitializeComponent();
        WebView? wvBrowser = FindByName("webView") as WebView;
        api = new NativeBridge(wvBrowser);
        api.AddTarget("dialogs", new NativeApi());

    }
}

