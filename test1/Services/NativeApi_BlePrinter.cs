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

namespace MauiBridge;

internal partial class NativeApi
{
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
    bool IsInit { get; set; }

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

    public async Task<string> connect_printer()
    {
        try
        {
            var res = await connectPrinter();
            return $"connectPrinter {(res ? "ok" : "false")}";
        }
        catch (Exception e)
        {
            var err = e.Message;
            await Tools.Alert("提示", err, "OK");
            return $"connectPrinter Error: {err}";
        }
    }

    private async Task<bool> connectPrinter()
    {
        Tools ??= Services.GetRequiredService<INativeFeatures>();
        BleTools ??= Services.GetRequiredService<BluetoothLEServices>();
        if (!IsInit) await Init();
        await GetConfigAsync();
        var res = await BleTools.ConnectDeviceAsync(BleInfo, false);
        return res != null;
    }

    public async Task<string> print_ticket(string[] data)
    {
        var ticekt = string.Empty;
        if (data == null || data.Length == 0)
        {
            return "fail";
        }

        try
        {
            data ??= [
                "Big Home",
                "NIF B12345678",
                "Calle Estrella, 25",
                "28006, Madrid",
                "<br>",
                "Producto$Precio",
                "<newline>",
                "COLA$1.99",
                "COLA x 2$1.99",
                "COLA Zero$1.99",
                "<newline>",
                "Total$10.00",
                "Entregado$5.00",
                "Cambio$5.00",
                "<br>",
                "Base$IVA",
                "21%$5.00",
                "4%$5.00",
                "--- Gracias por su visita ---",
                "<qr>https://app1.es/1121"
               ];

            var tatils = "";
            var qrcode = "";
            var left = 0;
            var width = 400;
            //标签高度,使用计算后数值
            var startPos = 190;
            foreach (var item in data.Skip(4))
            {
                var list = item.Split('$');
                if (list.Length > 1 && item.StartsWith("Total"))
                {
                    //合计行放大字体,并分割左右对齐
                    tatils += $"SETMAG 2 1\r\nT 2 0 0 {startPos} {list[0]}\r\nRIGHT\r\nT 2 0 0 {startPos} {list[1]}\r\nSETMAG 1 1\r\nLEFT\r\n";
                }
                else if (list.Length > 1)
                {
                    //普通分割左右对齐
                    tatils += $"T 2 0 0 {startPos} {list[0]}\r\nRIGHT\r\nT 2 0 0 {startPos} {list[1]}\r\nLEFT\r\n";
                }
                else if (item == "<newline>")
                {
                    //画一条分割线
                    tatils += $"LINE 2 {startPos} {width} {startPos} 1\r\n";
                }
                else if (item == "<br>")
                {
                    //换行
                    tatils += $"T 2 0 0 {startPos} \r\n";
                }
                else if (item.StartsWith("---"))
                {
                    //居中
                    startPos += 20;
                    var _item = item.Replace("---", "");
                    tatils += $"CENTER\r\nT 2 0 0 {startPos} {_item}\r\nLEFT\r\n";
                }
                else if (item.StartsWith("<qr>"))
                {
                    qrcode = item.Replace("<qr>", "");
                    continue;
                }
                else
                {
                    tatils += $"T 2 0 0 {startPos} {item}\r\n";
                }

                //特别设置 <br> 高度只加10
                startPos += (item == "<br>" ? 10 : 30);
            }

            if (qrcode != "")
            {
                startPos += 20;
                qrcode = $"""
CENTER
B QR 0 {startPos} M 2 U 7
MA,{qrcode}
ENDQR
LEFT
""";
                startPos += 200;
            }

            //! 0 200 200 290 1 => x方向偏移为0, x和y方向的打印分表率为200DPI, 标签高度为290点，打印数量为1
            //TEXT 4 0 30 40 Hello World => 使用4号字， 在（30,40）坐标处打印 Hello World
            var codes = $"""
! {left} 200 200 {startPos} 1
BEEP 1
PW {width}
CENTER
SETMAG 2 1
T 2 0 0 40 {data[0]} 
SETMAG 1 1
T 2 0 0 80 {data[1]}
T 2 0 0 115 {data[2]} 
T 2 0 0 150 {data[3]}
LEFT
SETMAG 1 1
{tatils}
{qrcode}
FORM
PRINT
""";

            await SendDataAsyncPrinter(ticekt);
        }
        catch (Exception e)
        {
            var err = e.Message;
            await Tools.Alert("提示", err, "OK");
            return err;
        }
        return "print_ticket ok";
    }

    public async Task<string> print_barcode(string[] data)
    {
        if (data == null || data.Length < 3)
        {
            return "fail";
        }

        try
        {
            CpclBarcode = "! 0 200 200 280 1\r\n" +
    "PW 450\r\n" +
    "CENTER\r\n" +
    "SETMAG 1 1\r\n" +
    "ENCODING GB18030\r\n" +
    $"TEXT 4 11 30 40 {data[0]}\r\n" +
    "BARCODE-TEXT 7 0 5\r\n" +
    $"BARCODE 128 1 1 50 10 100 {data[1]}\r\n" +
    "BARCODE-TEXT OFF\r\n" +
    "SETMAG 2 2\r\n" +
    "ENCODING ASCII\r\n" +
    $"TEXT 4 11 30 210 PVP:  {data[2]}\r\n" +
    "FORM\r\n" +
    "PRINT\r\n";
            await SendDataAsyncPrinter(CpclBarcode);
        }
        catch (Exception e)
        {
            var err = e.Message;
            await Tools.Alert("提示", err, "OK");
            return err;
        }
        return "print_barcode ok";
    }

    public async Task<string> print_cpcl_raw(string[] data)
    {
        var ticekt = string.Empty;
        if (data == null || data.Length == 0)
        {
            return "fail";
        }

        try
        {
            data.ToList().ForEach(a => ticekt += a + "\r\n");
            await SendDataAsyncPrinter(ticekt);
        }
        catch (Exception e)
        {
            var err = e.Message;
            await Tools.Alert("提示", err, "OK");
            return err;
        }
        return "print_cpcl_raw ok";
    }

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
        //this.title. = $"{message}\r\n{Message}";
        return Task.CompletedTask;
    }

    private Task Tools_OnStateConnect(bool obj)
    {
        IsConnected = obj;
        return Task.CompletedTask;
    }

}
