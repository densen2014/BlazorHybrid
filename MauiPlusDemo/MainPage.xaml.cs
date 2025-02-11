// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using MauiPlus;
using System.Reflection.Emit;
using System.Reflection;

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
                var nativeApiInstance = CreateNativeApiInstance().Result;
                TestCreateNativeApiInstance(nativeApiInstance);
                api = new NativeBridge(wvBrowser);
                //api.AddTarget("dialogs", new NativeApi());
                api.AddTarget("dialogs", nativeApiInstance!);
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

        private async void TestCreateNativeApiInstance(object? nativeApiInstance)
        {
            if (nativeApiInstance != null)
            {
                Console.WriteLine("Dynamic compilation and loading succeeded.");

                // 打印出所有方法名称，确认方法确实存在
                var methods = nativeApiInstance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
                foreach (var methodInfo in methods)
                {
                    Console.WriteLine($"Found method: {methodInfo.Name}");
                }

                // 调用异步方法
                var method = nativeApiInstance.GetType().GetMethod("get_config", BindingFlags.Instance | BindingFlags.Public);
                if (method != null)
                {
                    var task = (Task<string>)method.Invoke(nativeApiInstance, null);
                    string result = await task;
                    Console.WriteLine(result);
                }
                else
                {
                    Console.WriteLine("Method 'get_config' not found.");
                }
            }
            else
            {
                Console.WriteLine("Dynamic compilation and loading failed.");
            }
        }

        private async Task<object?> CreateNativeApiInstance()
        {
            string code = """
using System;
using System.Threading.Tasks;

public class NativeApi
{
    public string set_config()
    {
        return "set_config ok";
    }

    public async Task<string> get_config()
    {
        await Task.Delay(200); // 模拟异步操作
        return "get_config 123";
    }

    public async Task<string> open_file_dialog()
    {
        await Task.Delay(500); // 模拟异步操作
        return "open_file_dialog ok";
    }

    public string save_file(string content, string filename)
    {
        return "save_file ok";
    }
}
""";

            return await DynamicCompiler.CompileAndLoad(code, "NativeApi");
        }
    }

}
