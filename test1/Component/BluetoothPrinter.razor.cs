// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using AME;
using BootstrapBlazor.Components;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static BlazorHybrid.Core.Device.BleUUID;

namespace BlazorHybrid.Core.Device;

public partial class BluetoothPrinter : IAsyncDisposable
{
    private bool IsScanning = false;

    private string btnclass = "col-3 col-sm-3 col-md-4 col-lg-auto";

    BleTagDevice BleInfo { get; set; } = new BleTagDevice();

    private BluetoothPrinterOption Option = new();

    private string? ReadResult;

    private string? Messages = "";

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
            PrinterServiceUUID,
            PrinterCharacteristicUUID),
        new("QR380A-165D(Cpcl)",
            "00000000-0000-0000-0000-047f0ea2165d",
            PrinterServiceUUID,
            PrinterCharacteristicUUID),
        new("QR380A(Cpcl)",
            "QR380A",
            PrinterServiceUUID,
            PrinterCharacteristicUUID),
        new("BMAU32(Cpcl)",
            "BMAU32",
            PrinterServiceUUID,
            PrinterCharacteristicUUID),
        new("E3PLUS(Cpcl)",
            "E3PLUS",
            PrinterServiceUUID,
            PrinterCharacteristicUUID),
        new("SUNMI/InnerPrinter/FK-POSP58A+/BlueToothPrinter(ESCPOS)",
            SerialPortServiceUUID,
            PrinterNormalServiceUUID,
            PrinterNormalCharacteristicUUID,
            printerType: BlePrinterType.ESCPOS),
        new("FK-POSP58A+(ESCPOS)",
            "FK-POS",
            PrinterNormalServiceUUID,
            PrinterNormalCharacteristicUUID,
            printerType: BlePrinterType.ESCPOS),
        new("HM-A300",
            "HM-A300",
            "0000fee7-0000-1000-8000-00805f9b34fb",
            PrinterNormalCharacteristicUUID)
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

            PrinterList.ForEach(a => PrinterDemoList.Add(new SelectedItem() { Text = a.Name ?? "未知设备", Value = a.DeviceID.ToString() }));
            var configJson = await Storage.GetValue("BluetoothPrinterConfig", string.Empty);
            if (configJson != null)
            {
                try
                {
                    var config = JsonConvert.DeserializeObject<BluetoothPrinterOption>(configJson);
                    if (config != null)
                    {
                        Option = config;
                    }
                }
                catch
                {
                    await SaveConfig();
                }

            }
            else
            {
                await SaveConfig();
            }

            StateHasChanged();

            //if (!Tools.IsMaui())
            //{
            //    await ToastService.Warning("提示", "目前只支持MAUI");
            //    return false;
            //}
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

            StateHasChanged();

            if (Option.DeviceID != Guid.Empty)
            {
                BleInfo.Name = Option.Name;
                BleInfo.DeviceID = Option.DeviceID;
                BleInfo.Serviceid =Option.Serviceid;
                BleInfo.Characteristic = Option.Characteristic;
                BleInfo.ScanTimeout = Option.ScanTimeout;
                BleInfo.ByName = Option.ByName;
                BleInfo.PrinterType = Option.PrinterType;

                if (Option.AutoConnect)
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

    private async Task ResetConfig()
    {
        Option = new();
        await Storage.SetValue("BluetoothPrinterConfig", Option.ObjectToJson());
    }

    private async Task SaveConfigOK()
    {
        await Storage.SetValue("BluetoothPrinterConfig", Option.ObjectToJson());
        await ToastService.Success("提示", "保存成功");
    }
    private async Task SaveConfig() => await Storage.SetValue("BluetoothPrinterConfig", Option.ObjectToJson());

    private async Task ConnectLastDevice()
    {
        Services = null;
        Characteristics = null;
        Messages = "";
        ReadResult = "";
        Devices = new List<BleDevice>() { new BleDevice() { Id = BleInfo.DeviceID, Name = BleInfo.Name } };
        DeviceList = new List<SelectedItem>() { new SelectedItem() { Text = BleInfo.Name ?? "未知设备", Value = BleInfo.DeviceID.ToString() } };
        IsAutoConnect = true;
        await OnDeviceSelect();
        IsAutoConnect = false;
    }

    private async Task OnStateChanged(bool value)
    {
        Option.AutoConnect = value;
        await SaveConfig();
    }

    private async Task OnPrinterSelect(SelectedItem item)
    {
        if (IsAutoConnect || item.Value == "") return;
        var res = PrinterList.Where(a => a.Name == item.Text).FirstOrDefault();
        if (res != null)
        {
            BleInfo = res.Clone();
            BleInfo.ScanTimeout = BleInfo.ByName ? 10 : 5;
            await SendDataAsyncCPCLBarcode(BleInfo);
            //await ConnectLastDevice();
            //await SendDataAsyncCPCLBarcode();
        }
        else
        {
            Messages = "出错";
            await ToastService.Information("提示", Messages);
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
        //if (Messages !=null && Message.Length >1500) Message= Message.Substring (0, 1500);
        Messages = $"{message}\r\n{Message}";
        await InvokeAsync(StateHasChanged);
    }

    private async Task ScanPrinterDevice() => await ScanDevice();

    //扫描外设
    private async Task ScanDevice()
    {
        if (!await Init() || IsScanning) return;

        await OnDisConnectDevice(true);
        await SaveConfig();
        //BleInfo.Name = "QR380A-165D";

        IsScanning = true;
        Devices = null;
        Services = null;
        Characteristics = null;
        Messages = "";
        ReadResult = "";
        DeviceList = new List<SelectedItem>() { new SelectedItem() { Text = "请选择...", Value = "" } };

        Guid[]? serviceUuids = Option.PrinterOnly ? [Guid.Parse(PrinterServiceUUID), Guid.Parse(PrinterNormalServiceUUID)] : null;

        //开始扫描
        Devices = await Tools.StartScanAsync(serviceUuids: serviceUuids);
        //StateHasChanged();

        if (Devices != null)
        {
            if (Option.MinRssi != 0)
            {
                Devices = Devices.Where(a => a.Rssi >= -Option.MinRssi).ToList();
            }
            if (!string.IsNullOrEmpty(Option.NameFilter))
            {
                Devices = Devices.Where(a => a.IsConnectable == true && a.Name != null && a.Name.Contains(Option.NameFilter)).OrderBy(a => a.Name).ToList();
            }
            else
            {
                Devices = Devices.Where(a => a.IsConnectable == true).OrderBy(a => a.Name).ToList();
            }
            Devices.ForEach(a => a.ServicesRemark = "-");
            StateHasChanged();

            foreach (var bleDevice in Devices)
            {
                //await InvokeAsync(StateHasChanged);

                //_ = Task.Run(async () =>
                //{
                //    await Task.Delay(200);
                //连接外设
                var services = await Tools.ConnectToKnownDeviceAsync(bleDevice.Id, bleDevice.Name);
                if (services != null)
                {
                    if (serviceUuids != null)
                    {
                        var isPrinter = services.Where(a => serviceUuids.Contains(a.Id)).Any();
                        if (!isPrinter)
                        {
                            bleDevice.ServicesRemark = "isNotPrinter";
                            await Tools_OnMessage($"排除{bleDevice.Name}");
                            await InvokeAsync(StateHasChanged);
                            continue;
                        }
                    }

                    services.ForEach(a =>
                    {
                        if (a.Id == Guid.Parse(PrinterServiceUUID) || a.Id == Guid.Parse(PrinterNormalServiceUUID))
                        {
                            a.Name = "打印服务";
                        }
                        else if (a.Id == Guid.Parse(GattServiceUUID))
                        {
                            a.Name = "GATT*";
                        }
                    });
                    bleDevice.Services.AddRange(services);
                    bleDevice.ServicesRemark = $"服务: {services.Count}";
                    await InvokeAsync(StateHasChanged);
                    var stop = false;
                    var canWriteCount = 0;
                    foreach (var bleService in services)
                    {
                        if (bleService != null && bleService.Id != Guid.Empty && bleService.IsPrimary)
                        {
                            if (!stop)
                            {
                                var characteristics = await Tools.GetCharacteristicsAsync(bleService!.Id);
                                if (characteristics != null)
                                {
                                    characteristics.ForEach(a =>
                                    {
                                        if (a.Id == Guid.Parse(PrinterNormalCharacteristicUUID) || a.Id == Guid.Parse(PrinterCharacteristicUUID))
                                        {
                                            a.Name = "打印通道";
                                        }
                                        else if (a.Id == Guid.Parse(GattCharacteristicUUID))
                                        {
                                            a.Name = "GATT*";
                                        }
                                    });
                                    bleService.Characteristics.AddRange(characteristics);
                                    bleService.Remark = $"特征: {characteristics.Count}";
                                    canWriteCount += characteristics.Where(a => a.CanWrite).Count();
                                }
                                else
                                {
                                    bleService.Remark = "-";
                                }
                            }
                        }
                    }
                    if (canWriteCount > 0)
                    {
                        bleDevice.ServicesRemark += $"({canWriteCount}w)";
                        await InvokeAsync(StateHasChanged);
                    }
                }
                else
                {
                    bleDevice.ServicesRemark = $"连接超时";
                    Messages = $"连接{bleDevice.Name}超时";
                    await ToastService.Error("提示", Messages);
                }
                await OnDisConnectDevice(true);
                Messages = $"完成";
                await ToastService.Success("提示", Messages);
                await InvokeAsync(StateHasChanged);

                DeviceList.Add(
                    new SelectedItem()
                    {
                        Active = IsAutoConnect && bleDevice.Id == BleInfo.DeviceID,
                        Text = string.IsNullOrWhiteSpace(bleDevice.Name) ? bleDevice.Id.ToString() : bleDevice.Name,
                        Value = bleDevice.Id.ToString()
                    });

                //});
            };

            Devices = Devices.Where(a => a.ServicesRemark != "isNotPrinter").ToList();

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

    private async Task OnReset() => await OnReset(false);
    private async Task OnReset(bool silent)
    {
        await OnDisConnectDevice(silent);
        await Tools.ResetBluetooth();
        Devices = null;
    }

    private async Task OnDisConnectDevice() => await OnDisConnectDevice(false);
    private async Task OnDisConnectDevice(bool silent)
    {
        if (await Tools.DisConnectDeviceAsync())
        {
            Messages = "断开成功";
            if (!silent) await ToastService.Success("提示", Messages);
        }
        else
        {
            Messages = "断开失败";
            if (!silent) await ToastService.Error("提示", Messages);
        }
        Services = null;
        Characteristics = null;
        Messages = "";
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
        Messages = "";
        ReadResult = "";
        ServiceidList = new List<SelectedItem>() { new SelectedItem() { Text = "请选择...", Value = "" } };
        //连接外设
        Services = await Tools.ConnectToKnownDeviceAsync(BleInfo.DeviceID, BleInfo.Name?.Split("(").FirstOrDefault());
        if (Services != null)
        {
            IsConnected = true;
            Services.ForEach(a => ServiceidList.Add(
                new SelectedItem()
                {
                    Active = IsAutoConnect && a.Id == BleInfo.Serviceid,
                    Text = a.Name != "Unknown Service" ? $"{a.Name}({a.Id})" : a.Id.ToString(),
                    Value = a.Id.ToString()
                })
            );
            Option.DeviceID = BleInfo.DeviceID;
            Option.Name = BleInfo.Name ?? "上次设备";
            await SaveConfig();
            if (BleInfo.Serviceid != Guid.Empty && IsAutoConnect)
            {
                await OnServiceidSelect();
            }
        }
        else
        {
            Messages = $"连接{BleInfo.Name}失败";
            await ToastService.Error("提示", Messages);
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
        Messages = "";
        ReadResult = "";
        CharacteristicList = new List<SelectedItem>() { new SelectedItem() { Text = "请选择...", Value = "" } };
        Characteristics = await Tools.GetCharacteristicsAsync(BleInfo.Serviceid);
        if (Characteristics != null)
        {
            Characteristics.ForEach(a => CharacteristicList.Add(
                new SelectedItem()
                {
                    Active = IsAutoConnect && a.Id == BleInfo.Characteristic,
                    Text = a.Name != "Unknown characteristic" ? $"{a.Name}({(a.CanRead ? "R" : "-")}{(a.CanWrite ? "W" : "-")}{(a.CanUpdate ? "U" : "-")})({a.Id})" : $"({(a.CanRead ? "R" : "-")}{(a.CanWrite ? "W" : "-")}{(a.CanUpdate ? "U" : "-")})({a.Id})",
                    Value = a.Id.ToString()
                })
            );
            Option.Serviceid = BleInfo.Serviceid;
            await SaveConfig();
        }
        else
        {
            var message = $"获取特征失败. {BleInfo.Serviceid}";
            await ToastService.Error("提示", message);
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
        Messages = "";

        Option.Characteristic = BleInfo.Characteristic;
        await SaveConfig();
        //读取数值
        ReadResult = await Tools.ReadDeviceName(BleInfo.Serviceid, BleInfo.Characteristic);

        if (!string.IsNullOrEmpty(ReadResult)) await ToastService.Information("读取成功", ReadResult);
        //异步更新UI
        await InvokeAsync(StateHasChanged);
    }

    private async Task ReadDataAsync()
    {
        Messages = "";
        //读取数值
        var res = await Tools.ReadDataAsync(BleInfo.Characteristic);
        if (!string.IsNullOrEmpty(ReadResult)) await ToastService.Information("读取成功", res?.ToString());

        //异步更新UI
        await InvokeAsync(StateHasChanged);
    }

    //private async Task SendDataAsync()
    //{
    //    Messages = "";
    //    //读取数值
    //    var res = await Tools.SendDataAsync(BleInfo.Characteristic, null);
    //    await ToastService.Information("成功发送", res.ToString());

    //    //异步更新UI
    //    await InvokeAsync(StateHasChanged);
    //}

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

    // string = ESC + "!" + Chr(0) + ESC + "a" + Chr(1) + (str_title.value.length > 0 ? str_title.value : str_barcode
    //     .value) + CRLF + CRLF + CRLF + qrCode(str_barcode.value) +
    //CRLF + CRLF + CRLF;

    public string PrintLabelBMAU_QR(string? title = null, string? barcode = null, string? price = null)
    {
        title = title ?? Option.TestTitle;
        barcode = barcode ?? Option.TestBarcode;
        price = price ?? Option.TestPrice;

        // 方形标签
        // 店名
        string storeTitleQR1 = Option.StoreTitleQR1 ?? "";
        string storeTitleQR2 = Option.StoreTitleQR2 ?? "";
        string storeTitleQR3 = Option.StoreTitleQR3 ?? "";
        // 标签
        string storeQRSize = Option.StoreQRSize ?? "10 200 200 400";
        string storeQRWidth = Option.StoreQRWidth ?? "450";

        return "! " + storeQRSize + " 1\r\n" +
               "BEEP 1\r\n" +
               "PW " + storeQRWidth + "\r\n" +
               "CENTER\r\n" +
               "TEXT 10 2 10 40 " + storeTitleQR1 + "\r\n" +
               "TEXT 12 3 10 75 " + storeTitleQR2 + "\r\n" +
               "TEXT 10 2 10 350 " + storeTitleQR3 + "\r\n" +
               "B QR 30 150 M 2 U 7\r\n" +
               "MA," + barcode + "\r\n" +
               "ENDQR\r\n" +
               "FORM\r\n" +
               "PRINT\r\n";
    }


    public string PrintLabelBMAU(string? title = null, string? barcode = null, string? price = null)
    {
        title = title ?? Option.TestTitle;
        barcode = barcode ?? Option.TestBarcode;
        price = price ?? Option.TestPrice;

        // Store name
        string storeTitle = Option.StoreTitle ?? "";
        string storeTitleSize = Option.StoreTitleSize ?? "1 1";
        string storePoint = Option.StorePoint ?? "1 0 0 10";

        // Name
        string nameSize = Option.NameSize ?? "1 2";
        string namePoint = Option.NamePoint ?? "2 0 10 50";

        // Barcode
        string _barcode = Option.Barcode ?? "1 0 50 0 110";

        // Price
        string pricePoint = Option.PricePoint ?? "4 0 0 205";
        string priceSize = Option.PriceSize ?? "2 2";

        // Price PVP and Euros
        string priceTagSize = Option.PriceTagSize ?? "1 1";
        string priceTagPoint = Option.PriceTagPoint ?? "4 0 10 230";
        string priceTagPoint2 = Option.PriceTagPoint2 ?? "4 0 10 230";

        // Label
        string labelPoint = Option.LabelPoint ?? "0 200 200 290";
        string labelwidth = Option.Labelwidth ?? "450";

        return "! " + labelPoint + " 1\r\n" +
               "BEEP 1" + "\r\n" +
               "PW " + labelwidth + "\r\n" +
               "CENTER\r\n" +
               "SETMAG " + storeTitleSize + "\r\n" +
               "TEXT " + storePoint + " " + storeTitle + "\r\n" +
               "SETMAG " + nameSize + "\r\n" +
               "TEXT " + namePoint + " " + title + "\r\n" +
               "BARCODE-TEXT 7 0 5\r\n" +
               "BARCODE 128 " + _barcode + " " + barcode + "\r\n" +
               "BARCODE-TEXT OFF\r\n" +
               "SETBOLD 1\r\n" +
               "SETMAG " + priceSize + "\r\n" +
               "TEXT " + pricePoint + " " + price + "\r\n" +
               "SETMAG " + priceTagSize + "\r\n" +
               "LEFT\r\n" +
               "TEXT " + priceTagPoint2 + " " + Option.PriceTagPVP + ":\r\n" +
               "RIGHT\r\n" +
               "TEXT " + priceTagPoint + " " + Option.PriceTagEuros + "\r\n" +
               "SETBOLD 0\r\n" +
               "FORM\r\n" +
               "PRINT\r\n";
    }

    public string PrintTicketESCPOS_barcode(string title, string barcode, string price, bool printbarcode = false)
    {
        if (barcode == "") return "";

        //店名
        string title0 = Option.StoreTitle ?? "";
        string OutputDataQrCode = "";
        ////initialize
        //OutputDataQrCode += ESC + "@";
        //if (title0.Length > 0)
        //{
        //    //setTextBold
        //    OutputDataQrCode += ESC + "E" + (char)1;
        //    //setJustification center
        //    OutputDataQrCode += ESC + "a" + (char)1;
        //    OutputDataQrCode += title0 + CRLF;
        //}
        ////setTextBold
        //OutputDataQrCode += ESC + "E" + (char)1;
        ////setJustification center
        //OutputDataQrCode += ESC + "a" + (char)1;
        ////setTextSize
        ////OutputDataQrCode += GS + '!' + (char)1;
        //OutputDataQrCode += escPriceSize.value + " " + title + CRLF;
        ////OutputDataQrCode += ESC + "d" + (char)1;
        //if (printbarcode)
        //{
        //    //initialize
        //    OutputDataQrCode += ESC + "@";
        //    //barcode
        //    OutputDataQrCode += GS + 'h2' + GS + 'w' + (char)2 + GS + "k" + (char)73 + (char)(barcode.Length + 2) + (char)123 +
        //        (char)66 + barcode; //+ NUL
        //                            //OutputDataQrCode += CRLF;
        //}
        ////setTextSize reset
        //OutputDataQrCode += ESC + "!" + (char)0;
        //OutputDataQrCode += ESC + "a" + (char)0;
        //OutputDataQrCode += ESC + "E" + (char)1;
        //OutputDataQrCode += barcode;
        //OutputDataQrCode += CRLF;
        ////setTextSize reset
        //OutputDataQrCode += ESC + "!" + (char)0;
        //OutputDataQrCode += ESC + "E" + (char)0;
        //OutputDataQrCode += ESC + "a" + (char)2;
        ////setTextSize
        //OutputDataQrCode += GS + '!' + (char)1;
        //OutputDataQrCode += "PVP: ";
        ////setTextSize
        //OutputDataQrCode += GS + '!' + (char)escPriceSize.value;
        //OutputDataQrCode += price;
        ////setTextSize
        //OutputDataQrCode += GS + '!' + (char)1;
        //OutputDataQrCode += " EUR";
        //OutputDataQrCode += CRLF;
        //OutputDataQrCode += ESC + "d" + (char)3;
        return OutputDataQrCode;
    }

    /// <summary>
    /// 打印测试
    /// </summary>
    /// <param name="device">蓝牙设备</param>
    /// <returns></returns>
    private async Task SendDataAsyncCPCLBarcode(BleTagDevice device)
    {
        Messages = "";
        if (IsScanning)
        {
            Messages = "蓝牙正在使用中，请稍后再试";
            await ToastService.Warning("提示", Messages);
            return;
        }

        Messages = "";
        BleInfo = device;
        await Tools_OnMessage("打印测试");
        await Tools.ConnectDeviceAsync(device, false);
        //await ConnectLastDevice();
        await SendDataAsyncPrinter(CpclBarcode);
    }

    /// <summary>
    /// 打印测试。 通过服务ID查找特征列表
    /// </summary>
    /// <param name="devicename">设备名称</param>
    /// <param name="deviceId">设备ID</param>
    /// <param name="serviceid">服务ID</param>
    /// <returns></returns>
    private async Task SendDataAsyncCPCLBarcode(string? devicename, Guid deviceId, Guid serviceid)
    {
        Messages = "";
        var _device = Devices!.Where(a => a.Id == deviceId).FirstOrDefault();
        var _service = _device!.Services.Where(a => a.Id == serviceid).FirstOrDefault();
        if (_service == null)
        {
            _service = _device!.Services.FirstOrDefault();
        }
        var characteristics = _service?.Characteristics.Where(a => a.CanWrite == true && a.Id == Guid.Parse(PrinterNormalCharacteristicUUID) || a.Id == Guid.Parse(PrinterCharacteristicUUID)).FirstOrDefault();
        if (characteristics == null)
        {
            characteristics = _service?.Characteristics.Where(a => a.CanWrite == true).FirstOrDefault();
        }
        if (characteristics != null && _service!.Id != Guid.Empty)
        {
            BleInfo = new BleTagDevice(devicename, deviceId, _service.Id, characteristics.Id);
            await SendDataAsyncCPCLBarcode(BleInfo);
        }
        else
        {
            var message = $"参数无效";
            await ToastService.Warning("提示", message);
        }
    }
    private async Task SaveBleDevice(BleTagDevice BleInfo)
    {
        Option.Name = BleInfo.Name;
        Option.DeviceID = BleInfo.DeviceID;
        Option.Serviceid = BleInfo.Serviceid;
        Option.Characteristic = BleInfo.Characteristic;
        Option.ScanTimeout = BleInfo.ScanTimeout;
        Option.ByName = BleInfo.ByName;
        Option.PrinterType = BleInfo.PrinterType;
        await SaveConfig();
    }

    /// <summary>
    /// 打印测试。 通过服务查找特征列表
    /// </summary>
    /// <param name="deviceId">设备ID</param>
    /// <param name="service">服务∂=</param>
    /// <returns></returns>
    private async Task SendDataAsyncCPCLBarcode(Guid deviceId, BleService service)
    {
        Messages = "";
        var _device = Devices!.Where(a => a.Id == deviceId).FirstOrDefault();
        var _service = _device!.Services.Where(a => a.Id == service.Id).FirstOrDefault();
        var characteristics = _service?.Characteristics.Where(a => a.CanWrite == true && a.Id == Guid.Parse(PrinterNormalCharacteristicUUID) || a.Id == Guid.Parse(PrinterCharacteristicUUID)).FirstOrDefault();
        if (characteristics == null)
        {
            characteristics = _service?.Characteristics.Where(a => a.CanWrite == true).FirstOrDefault();
        }
        if (characteristics != null)
        {
            await SendDataAsyncCPCLBarcode(new BleTagDevice(_device.Name, _device.Id, service.Id, characteristics.Id));
        }
        else
        {
            var message = $"参数无效";
            await ToastService.Warning("提示", message);
        }
    }

    private async Task SendDataAsyncCPCLTest() => await SendDataAsyncPrinter(PrintLabelBMAU());
    private async Task SendDataAsyncCPCLBarcode() => await SendDataAsyncPrinter(CpclBarcode);
    private async Task SendDataAsyncESC() => await SendDataAsyncPrinter(CpclCommands);

    private async Task SendDataAsyncPrinter(string commands)
    {
        await SaveBleDevice(BleInfo);
        if (!await Tools.SendDataAsync(BleInfo.Characteristic, commands, Option.Chunk))
        {
            var message = $"打印数据出错";
            await ToastService.Warning("提示", message);
        }

    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await OnReset();
        Tools.OnMessage -= async (m) => await Tools_OnMessage(m);
        Tools.OnDataReceived -= async (m) => await Tools_OnDataReceived(m);
        Tools.OnStateConnect -= async (o) => await Tools_OnStateConnect(o);
    }
}


