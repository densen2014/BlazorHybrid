// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorHybrid.Core;
using BlazorHybrid.Core.Device;
using BlazorHybrid.Maui.Shared;
using BootstrapBlazor.WebAPI.Services;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using static BlazorHybrid.Core.Device.BleUUID;
using static test1.MauiProgram;

namespace test1;

internal partial class NativeApi
{
    private string PrinterNameKey = "PrinterName";
    private string printerName = "Unknown";
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

    private string CpclBarcode =
    "! 0 200 200 280 1\r\n" +
    "PW 450\r\n" +
    "CENTER\r\n" +
    "SETMAG 1 1\r\n" +
    "ENCODING GB18030\r\n" +
    "TEXT 4 11 30 40 Coca-Cola 可口可乐\r\n" +
    "BARCODE-TEXT 7 0 5\r\n" +
    "BARCODE 128 1 1 50 10 100 123456789\r\n" +
    "BARCODE-TEXT OFF\r\n" +
    "SETMAG 2 2\r\n" +
    "ENCODING ASCII\r\n" +
    "TEXT 4 11 30 210 PVP:  123.45\r\n" +
    "FORM\r\n" +
    "PRINT\r\n";

    [NotNull]
    INativeFeatures? Tools { get; set; }

    [NotNull]
    BluetoothLEServices? BleTools { get; set; }

    [NotNull]
    protected IStorage? Storage { get; set; }

    BleTagDevice BleInfo { get; set; } = new(
        "E3PLUS(Cpcl)",
        "E3PLUS",
        PrinterServiceUUID,
        PrinterCharacteristicUUID);

    private BluetoothPrinterOption Option = new();
    private bool IsConnected { get; set; }
    private string? Log { get; set; }
    private bool IsInit { get; set; }

    async Task<bool> Init()
    {
        try
        {
            if (IsInit) return true;
            Storage ??= Services.GetRequiredService<IStorage>();
            if (await BleTools.BluetoothIsBusy())
            {
                await Tools.Alert("提示", "蓝牙正在使用中，请稍后再试", "OK");
                return false;
            }
            BleTools.OnMessage += async (m) => await Tools_OnMessage(m);
            BleTools.OnDataReceived += async (m) => await Tools_OnMessage(m);
            BleTools.OnStateConnect += async (o) => await Tools_OnStateConnect(o);
            //初始化蓝牙扫描超时时间
            BleInfo.ScanTimeout = 5;
            BleTools.SetTagDeviceName(BleInfo);
            IsInit = true;
            return true;

        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex.Message);
        }
        return false;
    }

    /// <summary>
    /// 获取本地打印机名称设置
    /// </summary>
    /// <returns></returns>
    public Task<string> get_config()
    {
        printerName = Preferences.Default.Get(PrinterNameKey, printerName);
        return Task.FromResult(printerName);
    }


    /// <summary>
    /// 连接打印机
    /// </summary>
    /// <returns></returns>
    public async Task<string> connect_printer()
    {
        try
        {
            Log = "";
            var res = await connectPrinter();
            return $"connectPrinter {(res ? "ok" : "false")},{Log}";
        }
        catch (Exception e)
        {
            var err = e.Message;
            await Tools.Alert("提示", err, "OK");
            return $"connectPrinter Error: {err}";
        }
    }

    /// <summary>
    /// 连接打印机内部方法
    /// </summary>
    /// <returns></returns>
    private async Task<bool> connectPrinter()
    {
        Tools ??= Services.GetRequiredService<INativeFeatures>();
        BleTools ??= Services.GetRequiredService<BluetoothLEServices>();
        if (!IsInit) await Init();
        await GetConfigAsync();
        var res = await BleTools.ConnectDeviceAsync(BleInfo, false);
        return res != null;
    }

    /// <summary>
    /// 打印Cpcl条码测试
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task<string> print_ticket(string data)
    {
        try
        {
            Log = "";
            await SendDataAsyncPrinter(CpclBarcode);
        }
        catch (Exception e)
        {
            var err = e.Message;
            await Tools.Alert("提示", err, "OK");
            return err;
        }
        return $"print_ticket OK,{Log}";
    }

    /// <summary>
    /// 发送数据到打印机
    /// </summary>
    /// <param name="commands"></param>
    /// <returns></returns>
    private async Task SendDataAsyncPrinter(string commands)
    {
        if (!IsConnected)
        {
            await connectPrinter();
        }
        if (!await BleTools.SendDataAsync(BleInfo.Characteristic, commands, Option.Chunk))
        {
            await Tools.Alert("提示", "打印数据出错", "OK");
        }
    }

    /// <summary>
    /// 获取本地打印机配置,json格式存入本地
    /// </summary>
    /// <returns></returns>
    async Task GetConfigAsync()
    {
        var configJson = await Storage.GetValue("BluetoothPrinterConfig", string.Empty);
        if (!string.IsNullOrWhiteSpace(configJson))
        {
            try
            {
                var config = JsonConvert.DeserializeObject<BluetoothPrinterOption>(configJson);
                if (config != null)
                {
                    Option = config;
                    if (Option.DeviceID != Guid.Empty)
                    {
                        BleInfo.Name = Option.Name;
                        BleInfo.DeviceID = Option.DeviceID;
                        BleInfo.Serviceid = Option.Serviceid;
                        BleInfo.Characteristic = Option.Characteristic;
                        BleInfo.ScanTimeout = Option.ScanTimeout;
                        BleInfo.ByName = Option.ByName;
                        BleInfo.PrinterType = Option.PrinterType;
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

    private Task Tools_OnMessage(string message)
    {
        Log = $"{message}\r\n{Log}";
        return Task.CompletedTask;
    }

    private Task Tools_OnStateConnect(bool obj)
    {
        IsConnected = obj;
        return Task.CompletedTask;
    }

}
