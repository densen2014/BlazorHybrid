// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorHybrid.Core;
using BlazorHybrid.Core.Device;
using BootstrapBlazor.WebAPI.Services;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using WebViewNativeApi;
using static BlazorHybrid.Core.Device.BleUUID;
using static HybridWebView.MauiProgram;

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
                await Tools.Alert("提示", err, "OK");
                return err;
            }
            return "ok";
        }

        [NotNull]
        INativeFeatures? Tools { get; set; }

        [NotNull]
        protected IStorage? Storage { get; set; }

        BleTagDevice BleInfo { get; set; } = new(
            "E3PLUS(Cpcl)",
            "E3PLUS",
            PrinterServiceUUID,
            PrinterCharacteristicUUID);

        private BluetoothPrinterOption Option = new();
        private bool IsConnected { get; set; }
        bool IsInit { get; set; }

        private async Task SendDataAsyncPrinter(string commands)
        {
            Tools??= Services.GetRequiredService<INativeFeatures>();
            if (!IsInit) await Init();
            await GetConfigAsync();
            await Tools.ConnectDeviceAsync(BleInfo, false);
            await SendDataAsyncPrinter(commands); 
        }

        async Task GetConfigAsync()
        {
            var configJson = await Storage.GetValue("BluetoothPrinterConfig", string.Empty);
            if (configJson != null)
            {
                try
                {
                    var config = JsonConvert.DeserializeObject<BluetoothPrinterOption>(configJson);
                    if (config != null)
                    {
                        Option = config;
                        if (!string.IsNullOrEmpty(Option.DeviceID))
                        {
                            BleInfo.Name = Option.DeviceName;
                            BleInfo.DeviceID = Guid.Parse(Option.DeviceID);
                            if (!string.IsNullOrEmpty(Option.ServiceID)) BleInfo.Serviceid = Guid.Parse(Option.ServiceID);
                            if (!string.IsNullOrEmpty(Option.CharacteristicID)) BleInfo.Characteristic = Guid.Parse(Option.CharacteristicID);
                        }
                    }
                }
                catch
                {
                }
            }
            else
            {
            }

        }

        async Task<bool> Init()
        {
            try
            { 
                if (IsInit) return true;
                Storage??= Services.GetRequiredService<IStorage>();
                if (await Tools.BluetoothIsBusy())
                {
                    await Tools.Alert("提示", "蓝牙正在使用中，请稍后再试", "OK"); 
                    return false;
                }
                Tools.OnMessage += async (m) => await Tools_OnMessage(m);
                Tools.OnDataReceived += async (m) => await Tools_OnMessage(m);
                Tools.OnStateConnect += async (o) => await Tools_OnStateConnect(o);
                //初始化蓝牙扫描超时时间
                BleInfo.ScanTimeout = 5;
                Tools.SetTagDeviceName(BleInfo);
                IsInit = true;
                return true;

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            return false;
        }

        private Task Tools_OnMessage(string message)
        {
            //this.title. = $"{message}\r\n{Message}";
            return Task.CompletedTask; 
        }
        private Task Tools_OnStateConnect(bool obj)
        {
            IsConnected = obj;
            return Task.CompletedTask;
        }

    }
}
