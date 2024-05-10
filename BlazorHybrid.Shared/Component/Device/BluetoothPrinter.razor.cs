// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BootstrapBlazor.Components;
using OpenXmlPowerTools;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BlazorHybrid.Core.Device;

public partial class BluetoothPrinter : IAsyncDisposable
{
    private bool IsScanning = false;

    private string btnclass = "col-3 col-sm-3 col-md-4 col-lg-auto";

    BleTagDevice BleInfo { get; set; } = new BleTagDevice();

    private string? NameFilter = "QR380A";

    private string? ReadResult;

    private new string? Message = "";

    private bool autoread;

    private bool IsConnected { get; set; }
    bool IsAutoConnect { get; set; }
    bool IsAuto { get; set; }
    bool IsInit { get; set; }

    /// <summary>
    /// 设备列表
    /// </summary>
    private List<BleDevice>? Devices { get; set; }

    /// <summary>
    /// 服务列表
    /// </summary>
    private List<BleService>? Services;

    /// <summary>
    /// 特征列表
    /// </summary>
    private List<BleCharacteristic>? Characteristics;

    private List<SelectedItem> PrinterDemoList { get; set; } = new List<SelectedItem>() { new SelectedItem() { Text = "型号", Value = "" } };

    /// <summary>
    /// 设备下拉列表
    /// </summary>
    private List<SelectedItem> DeviceList { get; set; } = new List<SelectedItem>();

    /// <summary>
    /// 服务下拉列表
    /// </summary>
    private List<SelectedItem> ServiceidList { get; set; } = new List<SelectedItem>();

    /// <summary>
    /// 特征下拉列表
    /// </summary>
    private List<SelectedItem> CharacteristicList { get; set; } = new List<SelectedItem>();

    private Dictionary<string, object>? IsScanningCss => IsScanning ? new() { { "disabled", "" }, } : null;

    /// <summary>
    /// 演示设备列表
    /// </summary>
    List<BleTagDevice> PrinterList = [
        new("BMAU32-2AA8(Cpcl)",
            "bf5453be-dbf8-36b4-892d-dd7d811d5156",
            "0000ff00-0000-1000-8000-00805f9b34fb",
            "0000ff02-0000-1000-8000-00805f9b34fb"),
        new ("QR380A-165D(Cpcl)" ,
            "00000000-0000-0000-0000-047f0ea2165d",
            "0000ff00-0000-1000-8000-00805f9b34fb",
            "0000ff02-0000-1000-8000-00805f9b34fb"),
        new ("QR380A(Cpcl)" ,
            "QR380A",
            "0000ff00-0000-1000-8000-00805f9b34fb",
            "0000ff02-0000-1000-8000-00805f9b34fb"),
        new ("BMAU32(Cpcl)" ,
            "BMAU32",
            "0000ff00-0000-1000-8000-00805f9b34fb",
            "0000ff02-0000-1000-8000-00805f9b34fb"),
        new ("SUNMI/InnerPrinter/FK-POSP58A+/BlueToothPrinter(ESCPOS)" ,
            "00001101-0000-1000-8000-00805F9B34FB" ,
            "e7810a71-73ae-499d-8c15-faa9aef0c3f2" ,
            "BEF8D6C9-9C21-4C9E-B632-BD58C1009F9F",
            printerType : BlePrinterType.ESCPOS),
        new ("HM-A300" ,
            "00001101-0000-1000-8000-00805F9B34FB" ,
            "0000fee7-0000-1000-8000-00805f9b34fb" ,
            "BEF8D6C9-9C21-4C9E-B632-BD58C1009F9F")
        ];

    #region 表格相关

    [NotNull]
    Table<BleDevice>? Table { get; set; }

    private Func<BleDevice, Task>? OnDoubleClickRowCallback()
    {
        Func<BleDevice, Task>? ret = null;
        ret = async foo =>
        {
            await Table.ExpandDetailRow(foo);
        };
        return ret;
    }

    [NotNull]
    Table<BleService>? Table2 { get; set; }

    private Func<BleService, Task>? OnDoubleClickRowCallback2()
    {
        Func<BleService, Task>? ret = null;
        ret = async foo =>
        {
            await Table2.ExpandDetailRow(foo);
        };
        return ret;
    }

    private ConcurrentDictionary<string, List<BleService>> Cache { get; } = new();
    private bool ShowDetailRow(BleService item) => !string.IsNullOrEmpty(item.Remark);
    private List<BleService> GetDetailDataSource(BleDevice device)
    {
        var cacheKey = device.Id.ToString();
        return Cache.GetOrAdd(cacheKey, key => GetDetailRowsByName(device.Id));
    }

    private List<BleService> GetDetailRowsByName(Guid deviceId)
    {
        var _device = Devices!.Where(a => a.Id == deviceId).FirstOrDefault();
        _device ??= new BleDevice();
        return (_device ??= new BleDevice()).Services;
    }

    #endregion 

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Init();
        }
    }

    async Task<bool> Init()
    {
        try
        {

            if (IsInit) return true;

            if (!Tools.IsMaui())
            {
                await ToastService.Warning("提示", "目前只支持MAUI");
                return false;
            }
            if (await Tools.BluetoothIsBusy())
            {
                await ToastService.Warning("提示", "蓝牙正在使用中，请稍后再试");
                return false;
            }
            Tools.OnMessage += async (m) => await Tools_OnMessage(m);
            Tools.OnDataReceived += async (m) => await Tools_OnDataReceived(m);
            Tools.OnStateConnect += async (o) => await Tools_OnStateConnect(o);
            //初始化蓝牙扫描超时时间
            BleInfo.ScanTimeout = 5;
            Tools.SetTagDeviceName(BleInfo);
            IsInit = true;

            PrinterList.ForEach(a => PrinterDemoList.Add(new SelectedItem() { Text = a.Name ?? "未知设备", Value = a.DeviceID.ToString() }));

            StateHasChanged();

            var deviceID = await Storage.GetValue("bleDeviceID", string.Empty);
            NameFilter = await Storage.GetValue("bleNameFilter", string.Empty);
            if (!string.IsNullOrEmpty(deviceID))
            {
                BleInfo.Name = await Storage.GetValue("bleDeviceName", string.Empty);
                BleInfo.DeviceID = Guid.Parse(deviceID);
                var serviceid = await Storage.GetValue("bleServiceid", string.Empty);
                if (!string.IsNullOrEmpty(serviceid)) BleInfo.Serviceid = Guid.Parse(serviceid);
                var characteristic = await Storage.GetValue("bleCharacteristic", string.Empty);
                if (!string.IsNullOrEmpty(characteristic)) BleInfo.Characteristic = Guid.Parse(characteristic);
                var auto = await Storage.GetValue("bleAutoConnect", string.Empty);
                if (auto == "True")
                {
                    IsAuto = true;
                    await ConnectLastDevice();

                }
            }
            return true;

        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex.Message);
        }
        return false;
    }


    private async Task ConnectLastDevice()
    {
        Services = null;
        Characteristics = null;
        Message = "";
        ReadResult = "";
        Devices = new List<BleDevice>() { new BleDevice() { Id = BleInfo.DeviceID, Name = BleInfo.Name } };
        DeviceList = new List<SelectedItem>() { new SelectedItem() { Text = BleInfo.Name ?? "未知设备", Value = BleInfo.DeviceID.ToString() } };
        IsAutoConnect = true;
        await OnDeviceSelect();
        IsAutoConnect = false;
    }

    private async Task OnStateChanged(bool value)
    {
        await Storage.SetValue("bleAutoConnect", value.ToString());
    }

    private async Task OnPrinterSelect(SelectedItem item)
    {
        if (IsAutoConnect || item.Value == "") return;
        var res = PrinterList.Where(a => a.Name == item.Text).FirstOrDefault();
        if (res != null)
        {
            BleInfo = res;
            BleInfo.ScanTimeout = BleInfo.ByName ? 10 : 5;
            await SendDataAsyncCPCLBarcode(BleInfo);
            //await ConnectLastDevice();
            //await SendDataAsyncCPCLBarcode();
        }
        else
        {
            Message = "出错";
            await ToastService.Information("提示", Message);
        }
    }

    private async Task Tools_OnStateConnect(bool obj)
    {
        IsConnected = obj;
        await InvokeAsync(StateHasChanged);
    }

    private async Task Tools_OnDataReceived(string message)
    {
        ReadResult = message;
        await Tools_OnMessage(message);
        await InvokeAsync(StateHasChanged);
    }

    private async Task Tools_OnMessage(string message)
    {
        //if (Message !=null && Message.Length >1500) Message= Message.Substring (0, 1500);
        Message = $"{message}\r\n{Message}";
        await InvokeAsync(StateHasChanged);
    }

    private async Task ScanPrinterDevice() => await ScanDevice();

    //扫描外设
    private async Task ScanDevice()
    {
        if (!await Init()) return;
        await Storage.SetValue("bleNameFilter", NameFilter);
        //BleInfo.Name = "QR380A-165D";

        IsScanning = true;
        Devices = null;
        Services = null;
        Characteristics = null;
        Message = "";
        ReadResult = "";
        DeviceList = new List<SelectedItem>() { new SelectedItem() { Text = "请选择...", Value = "" } };

        //开始扫描
        Devices = await Tools.StartScanAsync();
        StateHasChanged();

        if (Devices != null)
        {
             if (!string.IsNullOrEmpty(NameFilter))
            {
                Devices = Devices.Where(a => a.IsConnectable == true && a.Name != null && a.Name.Contains(NameFilter)).OrderBy(a => a.Name).ToList();
            }
            else
            {
                Devices = Devices.Where(a => a.IsConnectable == true).OrderBy(a => a.Name).ToList();
            }
 

            foreach (var bleDevice in Devices)
            {
                DeviceList.Add(
                    new SelectedItem()
                    {
                        Active = IsAutoConnect && bleDevice.Id == BleInfo.DeviceID,
                        Text = $"{bleDevice.Name}({bleDevice.Id})",
                        Value = bleDevice.Id.ToString()
                    });
                //await InvokeAsync(StateHasChanged);

                _ = Task.Run(async () =>
                {
                    await Task.Delay(200);
                    //连接外设
                    var services = await Tools.ConnectToKnownDeviceAsync(bleDevice.Id, bleDevice.Name);
                    if (services != null)
                    {
                        bleDevice.Services.AddRange(services);
                        bleDevice.ServicesRemark = $"服务: {services.Count}";
                        await InvokeAsync(StateHasChanged);
                        var stop = false;
                        foreach (var bleService in services)
                        {
                            if (bleService != null && bleService.Id != Guid.Empty && bleService.IsPrimary)
                            {
                                if (!stop)
                                {
                                    var characteristics = await Tools.GetCharacteristicsAsync(bleService!.Id);
                                    if (characteristics != null)
                                    {
                                        bleService.Characteristics.AddRange(characteristics);
                                        bleService.Remark = $"特征: {characteristics.Count}";
                                        if (characteristics.Where(a => a.CanWrite).Count() > 0)
                                        {
                                            //stop = true;
                                            bleDevice.ServicesRemark += "+w";
                                            await InvokeAsync(StateHasChanged);
                                        }
                                    }
                                    else
                                    {
                                        bleService.Remark = "-";
                                    }
                                }
                            }
                        };
                    }
                    else
                    {
                        bleDevice.ServicesRemark = $"连接失败";
                        Message = $"连接{bleDevice.Name}失败";
                        await ToastService.Error("提示", Message);
                    }

                    Message = $"完成";
                    await ToastService.Success("提示", Message);
                    await InvokeAsync(StateHasChanged);

                });
            };
        }

        IsScanning = false;

        //异步更新UI
        await InvokeAsync(StateHasChanged);
    }

    //连接外设
    private async Task OnDeviceSelect(SelectedItem item)
    {
        if (IsAutoConnect || item.Value == "") return;
        BleInfo.Name = item.Text;
        BleInfo.DeviceID = Guid.Parse(item.Value);
        await OnDeviceSelect();
    }

    private async Task OnReset()
    {
        await OnDisConnectDevice();
        Devices = null;
    }
    private async Task OnDisConnectDevice()
    {
        if (await Tools.DisConnectDeviceAsync())
        {
            Message = "断开成功";
            await ToastService.Success("提示", Message);
        }
        else
        {
            Message = "断开失败";
            await ToastService.Error("提示", Message);
        }
        Services = null;
        Characteristics = null;
        Message = "";
        ReadResult = "";
    }

    /// <summary>
    /// 获取服务
    /// </summary>
    /// <returns></returns>
    private async Task OnDeviceSelect()
    {

        Services = null;
        Characteristics = null;
        Message = "";
        ReadResult = "";
        ServiceidList = new List<SelectedItem>() { new SelectedItem() { Text = "请选择...", Value = "" } };
        //连接外设
        Services = await Tools.ConnectToKnownDeviceAsync(BleInfo.DeviceID, BleInfo.Name?.Split("(").FirstOrDefault());
        if (Services != null)
        {
            Services.ForEach(a => ServiceidList.Add(
                new SelectedItem()
                {
                    Active = IsAutoConnect && a.Id == BleInfo.Serviceid,
                    Text = a.Name != "Unknown Service" ? $"{a.Name}({a.Id})" : a.Id.ToString(),
                    Value = a.Id.ToString()
                })
            );
            await Storage.SetValue("bleDeviceID", BleInfo.DeviceID.ToString());
            await Storage.SetValue("bleDeviceName", BleInfo.Name ?? "上次设备");
            if (BleInfo.Serviceid != Guid.Empty && IsAutoConnect)
            {
                await OnServiceidSelect();
            }
        }
        else
        {
            Message = $"连接{BleInfo.Name}失败";
            await ToastService.Error("提示", Message);
        }

        //异步更新UI
        await InvokeAsync(StateHasChanged);
    }


    private async Task OnServiceidSelect(SelectedItem item)
    {
        if (IsAutoConnect || item.Value == "") return;
        BleInfo.Serviceid = Guid.Parse(item.Value);
        await OnServiceidSelect();
    }

    /// <summary>
    /// 获取特征
    /// </summary>
    /// <returns></returns>
    private async Task OnServiceidSelect()
    {
        Characteristics = null;
        Message = "";
        ReadResult = "";
        CharacteristicList = new List<SelectedItem>() { new SelectedItem() { Text = "请选择...", Value = "" } };
        Characteristics = await Tools.GetCharacteristicsAsync(BleInfo.Serviceid);
        if (Characteristics != null)
        {
            Characteristics.ForEach(a => CharacteristicList.Add(
                new SelectedItem()
                {
                    Active = IsAutoConnect && a.Id == BleInfo.Characteristic,
                    Text = a.Name != "Unknown characteristic" ? $"{a.Name}({(a.CanRead ? "R" : "-")}{(a.CanWrite ? "W" : "-")}{(a.CanUpdate ? "U" : "-")}{a.StringValue})({a.Id})" : $"({(a.CanRead ? "R" : "-")}{(a.CanWrite ? "W" : "-")}{(a.CanUpdate ? "U" : "-")}{a.StringValue})({a.Id})",
                    Value = a.Id.ToString()
                })
            );
            await Storage.SetValue("bleServiceid", BleInfo.Serviceid.ToString());
        }
        else
        {
            Message = $"获取特征失败. {BleInfo.Serviceid}";
            await ToastService.Error("提示", Message);
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task OnCharacteristSelect(SelectedItem item)
    {
        if (IsAutoConnect) return;
        BleInfo.Characteristic = Guid.Parse(item.Value);
        await ReadDeviceName();
    }

    //读取数值
    private async Task ReadDeviceName()
    {
        Message = "";

        //读取数值
        ReadResult = await Tools.ReadDeviceName(BleInfo.Serviceid, BleInfo.Characteristic);
        await Storage.SetValue("bleCharacteristic", BleInfo.Characteristic.ToString());

        if (!string.IsNullOrEmpty(ReadResult)) await ToastService.Information("读取成功", ReadResult);
        //异步更新UI
        await InvokeAsync(StateHasChanged);
    }

    private async Task ReadDataAsync()
    {
        Message = "";
        //读取数值
        var res = await Tools.ReadDataAsync(BleInfo.Characteristic);
        if (!string.IsNullOrEmpty(ReadResult)) await ToastService.Information("读取成功", res?.ToString());

        //异步更新UI
        await InvokeAsync(StateHasChanged);
    }

    private async Task SendDataAsync()
    {
        Message = "";
        //读取数值
        var res = await Tools.SendDataAsync(BleInfo.Characteristic, null);
        await ToastService.Information("成功发送", res.ToString());

        //异步更新UI
        await InvokeAsync(StateHasChanged);
    }

    private async Task SendDataAsyncCPCL()
    {
        //CPCL指令套打
        StringBuilder cmds = new();
        cmds.AppendLine("!0 200 200 777 1");
        cmds.AppendLine("N-DOTS");
        cmds.AppendLine("PAGE-WIDTH 595");
        cmds.AppendLine("SETBOLD 1");
        cmds.AppendLine("SETMAG 3 3");
        cmds.AppendLine($"TEXT 10 0 8 195 测试-11-12-11");
        cmds.AppendLine("SETMAG 0 0");
        cmds.AppendLine("SETBOLD 0");
        cmds.AppendLine("LINE 0 156 585 156 2");
        cmds.AppendLine("LINE 0 301 585 301 2");
        cmds.AppendLine("LINE 0 518 585 518 2");
        cmds.AppendLine("LINE 0 724 585 724 2");
        cmds.AppendLine("BOX 4 12 585 724 1");
        cmds.AppendLine("BOX 4 156 585 301 1");
        cmds.AppendLine("BOX 4 581 585 724 1");
        cmds.AppendLine($"TEXT 4 0 22 327 投递机构：测试机构2");
        cmds.AppendLine($"TEXT 4 0 22 450 处理终端：DLV1233213123123123311");
        cmds.AppendLine($"TEXT 4 0 22 387 封包时间：{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}");
        cmds.AppendLine("SETBOLD 1");
        cmds.AppendLine($"TEXT 4 0 14 529 格口：121");
        cmds.AppendLine($"TEXT 4 0 397 529 件数：111");
        cmds.AppendLine($"TEXT 4 0 193 529 序号：123");
        cmds.AppendLine("SETBOLD 0");
        cmds.AppendLine($"TEXT 4 0 18 595 测试");
        cmds.AppendLine($"TEXT 55 0 18 733 打印时间：{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}");
        cmds.AppendLine($"BARCODE 128 1 20 80 80 27 12345678912346654");//条码
        cmds.AppendLine("CENTR");
        cmds.AppendLine($"TEXT 2 0 120 122 P12348785454445555");
        cmds.AppendLine("FORM");
        cmds.AppendLine("PRINT");

        await SendDataAsyncPrinter(cmds.ToString());
    }

    private string CpclCommands = @"! 10 200 200 400 1
BEEP 1
PW 380
SETMAG 1 1
CENTER
TEXT 10 2 10 40 Micro Bar
TEXT 12 3 10 75 Blazor
TEXT 10 2 10 350 eMenu
B QR 30 150 M 2 U 7
MA,https://google.com
ENDQR
FORM
PRINT
";
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

    // string = ESC + "!" + Chr(0) + ESC + "a" + Chr(1) + (str_title.value.length > 0 ? str_title.value : str_barcode
    //     .value) + CRLF + CRLF + CRLF + qrCode(str_barcode.value) +
    //CRLF + CRLF + CRLF;

    //public string PrintLabelBMAU(string title, string barcode, string price)
    //{
    //    // Store name
    //    string title0 = plus.storage.GetItem("title0") ?? "";
    //    string size1 = plus.storage.GetItem("size1") ?? "1 1";
    //    string BMAUtitle0 = plus.storage.GetItem("BMAUtitle0") ?? "1 0 0 10";

    //    // Name
    //    string size2 = plus.storage.GetItem("size2") ?? "1 2";
    //    string BMAUtitle = plus.storage.GetItem("BMAUtitle") ?? "2 0 10 50";

    //    // Barcode
    //    string BMAUbarcode = plus.storage.GetItem("BMAUbarcode") ?? "1 0 50 0 110";

    //    // Price
    //    string BMAUprice = plus.storage.GetItem("BMAUprice") ?? "4 0 0 205";
    //    string size3 = plus.storage.GetItem("size3") ?? "2 2";

    //    // Price PVP and Euros
    //    string size4 = plus.storage.GetItem("size4") ?? "1 1";
    //    string pos4 = plus.storage.GetItem("pos4") ?? "4 0 10 230";
    //    string pos5 = plus.storage.GetItem("pos5") ?? "4 0 10 230";

    //    // Label
    //    string LABELsize = plus.storage.GetItem("labelsize") ?? "0 200 200 290";
    //    string LABELWidth = plus.storage.GetItem("labelwidth") ?? "450";

    //    return "! " + LABELsize + " 1\r\n" +
    //           "BEEP 1" + "\r\n" +
    //           "PW " + LABELWidth + "\r\n" +
    //           "CENTER\r\n" +
    //           "SETMAG " + size1 + "\r\n" +
    //           "TEXT " + BMAUtitle0 + " " + title0 + "\r\n" +
    //           "SETMAG " + size2 + "\r\n" +
    //           "TEXT " + BMAUtitle + " " + title + "\r\n" +
    //           "BARCODE-TEXT 7 0 5\r\n" +
    //           "BARCODE 128 " + BMAUbarcode + " " + barcode + "\r\n" +
    //           "BARCODE-TEXT OFF\r\n" +
    //           "SETBOLD 1\r\n" +
    //           "SETMAG " + size3 + "\r\n" +
    //           "TEXT " + BMAUprice + " " + price + "\r\n" +
    //           "SETMAG " + size4 + "\r\n" +
    //           "LEFT\r\n" +
    //           "TEXT " + pos5 + " PVP:\r\n" +
    //           "RIGHT\r\n" +
    //           "TEXT " + pos4 + " Euros\r\n" +
    //           "SETBOLD 0\r\n" +
    //           "FORM\r\n" +
    //           "PRINT\r\n";
    //}

    //public string PrintTicketESCPOS_barcode(string title, string barcode, string price, bool printbarcode = false)
    //{
    //    if (barcode == "") return "";

    //    //店名
    //    string title0 = plus.storage.GetItem("title0") ?? "";
    //    string OutputDataQrCode = "";
    //    //initialize
    //    OutputDataQrCode += ESC + "@";
    //    if (title0.Length > 0)
    //    {
    //        //setTextBold
    //        OutputDataQrCode += ESC + "E" + (char)1;
    //        //setJustification center
    //        OutputDataQrCode += ESC + "a" + (char)1;
    //        OutputDataQrCode += title0 + CRLF;
    //    }
    //    //setTextBold
    //    OutputDataQrCode += ESC + "E" + (char)1;
    //    //setJustification center
    //    OutputDataQrCode += ESC + "a" + (char)1;
    //    //setTextSize
    //    //OutputDataQrCode += GS + '!' + (char)1;
    //    OutputDataQrCode += escPriceSize.value + " " + title + CRLF;
    //    //OutputDataQrCode += ESC + "d" + (char)1;
    //    if (printbarcode)
    //    {
    //        //initialize
    //        OutputDataQrCode += ESC + "@";
    //        //barcode
    //        OutputDataQrCode += GS + 'h2' + GS + 'w' + (char)2 + GS + "k" + (char)73 + (char)(barcode.Length + 2) + (char)123 +
    //            (char)66 + barcode; //+ NUL
    //                                //OutputDataQrCode += CRLF;
    //    }
    //    //setTextSize reset
    //    OutputDataQrCode += ESC + "!" + (char)0;
    //    OutputDataQrCode += ESC + "a" + (char)0;
    //    OutputDataQrCode += ESC + "E" + (char)1;
    //    OutputDataQrCode += barcode;
    //    OutputDataQrCode += CRLF;
    //    //setTextSize reset
    //    OutputDataQrCode += ESC + "!" + (char)0;
    //    OutputDataQrCode += ESC + "E" + (char)0;
    //    OutputDataQrCode += ESC + "a" + (char)2;
    //    //setTextSize
    //    OutputDataQrCode += GS + '!' + (char)1;
    //    OutputDataQrCode += "PVP: ";
    //    //setTextSize
    //    OutputDataQrCode += GS + '!' + (char)escPriceSize.value;
    //    OutputDataQrCode += price;
    //    //setTextSize
    //    OutputDataQrCode += GS + '!' + (char)1;
    //    OutputDataQrCode += " EUR";
    //    OutputDataQrCode += CRLF;
    //    OutputDataQrCode += ESC + "d" + (char)3;
    //    return OutputDataQrCode;
    //}

    private async Task SendDataAsyncCPCLBarcode(BleTagDevice device)
    {
        Message = "";
        if (IsScanning)
        {
            Message = "蓝牙正在使用中，请稍后再试";
            await ToastService.Warning("提示", Message);
            return;
        }

        Message = "";
        BleInfo = device;
        await Tools.ConnectDeviceAsync(device, false);
        //await ConnectLastDevice();
        await SendDataAsyncPrinter(CpclBarcode);
    }

    private async Task SendDataAsyncCPCLBarcode(string? devicename, Guid deviceId, Guid serviceid)
    {
        Message = "";
        var _device = Devices!.Where(a => a.Id == deviceId).FirstOrDefault();
        var _service = _device!.Services.Where(a => a.Id == serviceid).FirstOrDefault();
        if (_service == null)
        {
            _service = _device!.Services.FirstOrDefault();
        }
        var item = _service?.Characteristics.FirstOrDefault();
        if (item != null && _service!.Id != Guid.Empty)
        {
            await SendDataAsyncCPCLBarcode(new BleTagDevice(devicename, deviceId, _service!.Id, item.Id));
        }
        else
        {
            Message = $"参数无效";
            await ToastService.Warning("提示", Message);
        }
    }

    private async Task SendDataAsyncCPCLBarcode(Guid deviceId, BleService service)
    {
        Message = "";
        var _device = Devices!.Where(a => a.Id == deviceId).FirstOrDefault();
        var _service = _device!.Services.Where(a => a.Id == service.Id).FirstOrDefault();
        var item = _service?.Characteristics.FirstOrDefault();
        if (item != null)
        {
            await SendDataAsyncCPCLBarcode(new BleTagDevice(_device.Name, _device.Id, service.Id, item.Id));
        }
        else
        {
            Message = $"参数无效";
            await ToastService.Warning("提示", Message);
        }
    }

    private async Task SendDataAsyncCPCLBarcode() => await SendDataAsyncPrinter(CpclBarcode);
    private async Task SendDataAsyncESC() => await SendDataAsyncPrinter(CpclCommands);

    private async Task SendDataAsyncPrinter(string commands)
    {

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//注册Nuget包System.Text.Encoding.CodePages中的编码到.NET Core
        var utf8 = Encoding.GetEncoding(65001);
        var gb2312 = Encoding.GetEncoding("gb2312");//这里用转化解决汉字乱码问题Encoding.Default ,936
        var bytesUtf8 = utf8.GetBytes(commands);
        var bytesGb2312 = Encoding.Convert(utf8, gb2312, bytesUtf8);
        if (bytesGb2312 != null)
        {
            var totalCount = bytesGb2312.Length;
            var printPageSize = 512;
            var totalPage = (totalCount / printPageSize) + (totalCount % printPageSize > 0 ? 1 : 0);//返回总页数
            var index = 0;
            for (int i = 1; i <= totalPage; i++)
            {
                byte[] newbytes;
                if (totalCount < printPageSize && totalCount > 0)
                {
                    newbytes = new byte[totalCount];
                    Array.Copy(bytesGb2312, index, newbytes, 0, totalCount);
                }
                else
                {
                    newbytes = new byte[printPageSize];
                    Array.Copy(bytesGb2312, index, newbytes, 0, printPageSize);
                    index += printPageSize;
                    totalCount -= printPageSize;
                }
                if (newbytes != null && newbytes.Any())
                {
                    await Tools.SendDataAsync(BleInfo.Characteristic, newbytes);
                }
            }
        }
        else
        {
            Message = $"打印数据无效";
            await ToastService.Warning("提示", Message);
        }


        //异步更新UI
        //await InvokeAsync(StateHasChanged);
    }
    ValueTask IAsyncDisposable.DisposeAsync()
    {
        Tools.OnMessage -= async (m) => await Tools_OnMessage(m);
        Tools.OnDataReceived -= async (m) => await Tools_OnDataReceived(m);
        Tools.OnStateConnect -= async (o) => await Tools_OnStateConnect(o);
        return new ValueTask();
    }
}


