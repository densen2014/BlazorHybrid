// **********************************
// Densen Informatica 中讯科技
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com
// **********************************

namespace test1;

using MauiBridge;
using WebViewNativeApi;

public partial class MainPage : TabbedPage
{
    private NativeBridge? api;

    public MainPage()
    {
        InitializeComponent();

        //附加本机功能处理
        WebView? wvBrowser = FindByName("webView") as WebView;
        api = new NativeBridge(wvBrowser);
        api.AddTarget("dialogs", new NativeApi());
#if MACCATALYST
        Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("Inspect", (handler, view) =>
        {
            if (OperatingSystem.IsMacCatalystVersionAtLeast(16, 6))
                handler.PlatformView.Inspectable = true;
        });
#endif
    }
}

