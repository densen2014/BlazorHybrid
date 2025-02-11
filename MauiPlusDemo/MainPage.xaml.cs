// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using MauiPlus;

namespace MauiPlusDemo
{
    public partial class MainPage : ContentPage
    {
        private NativeBridge? api;

        public MainPage()
        {
            InitializeComponent();

            //附加本机功能处理
            WebView? wvBrowser = FindByName("webView") as WebView;
            if (wvBrowser != null)
            {
                LoadHtmlToWebView(wvBrowser);
                api = new NativeBridge(wvBrowser);
                api.AddTarget("dialogs", new NativeApi());
            }

#if MACCATALYST
    Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("Inspect", (handler, view) =>
    {
        if (OperatingSystem.IsMacCatalystVersionAtLeast(16, 6))
            handler.PlatformView.Inspectable = true;
    });
#endif
        }

        private async void LoadHtmlToWebView(WebView wvBrowser)
        {
            // 加载本地 HTML 文件
            var htmlSource = new HtmlWebViewSource
            {
                BaseUrl = FileSystem.AppDataDirectory,
                Html = await LoadLocalHtml("demo.html")
            };
            wvBrowser.Source = htmlSource;
        }

        private async Task<string> LoadLocalHtml(string fileName)
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
            using var reader = new StreamReader(stream);

            var contents = await reader.ReadToEndAsync();
            return contents;
        }
    }

}
