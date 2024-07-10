// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using System.Text;
using WebViewNativeApi;

namespace HybridWebView
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private NativeBridge? api;

        public MainPage()
        {
            InitializeComponent();
            WebView? wvBrowser = FindByName("webView") as WebView;
            api = new NativeBridge(wvBrowser);
            api.AddTarget("dialogs", new NativeApi());

        }

    }

    class NativeApi : object
    {
        public async Task<string> open_file_dialog()
        {
            //work in ui thread
            var res =
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    var result = await FilePicker.Default.PickAsync(new PickOptions());
                    if (result == null)
                    {
                        return "";
                    }
                    using var stream = await result.OpenReadAsync();
                    StreamReader reader = new StreamReader(stream);
                    return Convert.ToBase64String(Encoding.UTF8.GetBytes(reader.ReadToEnd()));

                }
                catch (Exception e)
                {
                    var err = e.Message;
                    return err;
                }
            });
            return res;
        }

        public async Task<string> save_file(string data, string fileName)
        {
            try
            {
                string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, fileName);

                using FileStream outputStream = File.OpenWrite(targetFile);
                using StreamWriter streamWriter = new(outputStream);

                await streamWriter.WriteAsync(data);
            }
            catch (Exception e)
            {
                var err = e.Message;
                return err;
            }
            return "ok";
        }
    }
}
