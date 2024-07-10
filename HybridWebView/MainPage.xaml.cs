// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using AME;
using BlazorHybrid.Core;
using BootstrapBlazor.Components;
using Microsoft.Maui.Controls;
using System.Text;
using WebViewNativeApi; 

using AME;
using BlazorHybrid.Core.Device;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static BlazorHybrid.Core.Device.BleUUID;

namespace HybridWebView
{
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

    internal class NativeApi : object
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

        private string CpclCommands =
          "! 10 200 200 400 1\r\n" +
          "BEEP 1\r\n" +
          "PW 380\r\n" +
          "SETMAG 1 1\r\n" +
          "CENTER\r\n" +
          "TEXT 10 2 10 40 Micro Bar\r\n" +
          "TEXT 12 3 10 75 Blazor\r\n" +
          "TEXT 10 2 10 350 eMenu\r\n" +
          "B QR 30 150 M 2 U 7\r\n" +
          "MA,https://google.com\r\n" +
          "ENDQR\r\n" +
          "FORM\r\n" +
          "PRINT\r\n";

        public async Task<string> print_ticket(string data)
        {
            try
            {
                await SendDataAsyncPrinter(CpclCommands);
            }
            catch (Exception e)
            {
                var err = e.Message;
                await Application.Current!.MainPage!.DisplayAlert("提示", err, "OK");
                return err;
            }
            return "ok";
        }

        protected INativeFeatures? Tools { get; set; }
        BleTagDevice BleInfo { get; set; } = new(
            "E3PLUS(Cpcl)",
            "E3PLUS",
            PrinterServiceUUID,
            PrinterCharacteristicUUID);
        private BluetoothPrinterOption Option = new();
        private async Task SendDataAsyncPrinter(string commands)
        {
            Tools??= DependencyService.Get<INativeFeatures>();
            if (!await Tools.SendDataAsync(BleInfo.Characteristic, commands, Option.Chunk))
            {
                var message = $"打印数据出错";
                await Application.Current!.MainPage!.DisplayAlert("提示", message, "OK");
                //await ToastService.Warning("提示", message);
            }

        }


    }
}
