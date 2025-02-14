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
using static BlazorHybrid.Core.Device.BleExtension;
using static BlazorHybrid.Core.Device.BleUUID;
using static BlazorHybrid.Core.Device.PrinterCommands;

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
            BleInfo.ScanTimeout = 10;
            Tools.SetTagDeviceName(BleInfo);
            IsInit = true;

            StateHasChanged();

            if (Option.DeviceID != Guid.Empty)
            {
                BleInfo.Name = Option.Name;
                BleInfo.DeviceID = Option.DeviceID;
                BleInfo.Serviceid = Option.Serviceid;
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

    private async Task ResetConfig(int format = 0)
    {
        switch (format)
        {
            case 1:
                Option.StorePoint = "1 0 0 10";
                Option.StoreTitleSize = "1 1";

                Option.NamePoint = "2 0 10 50";
                Option.NameSize = "1 2";

                Option.BarcodePoint = "1 0 50 0 110";

                Option.PricePoint = "4 0 0 205";
                Option.PriceSize = "2 2";

                Option.PriceTagEuros = "Euros";
                Option.PriceTagPoint = "4 0 0 230";
                Option.PriceTagSize = "1 1";

                Option.PriceTagPVP = "PVP";
                Option.PriceTagPVPPoint = "4 0 10 230";

                Option.LabelSize = "0 200 200 290";
                Option.LabelWidth = 450;
                break;
            case 2:
                Option.StorePoint = "1 0 0 250";
                Option.StoreTitleSize = "1 1";

                Option.NamePoint = "1 0 10 10";
                Option.NameSize = "2 2";

                Option.BarcodePoint = "1 0 50 0 70";

                Option.PricePoint = "2 0 0 170";
                Option.PriceSize = "3 3";

                Option.PriceTagEuros = "Euros";
                Option.PriceTagPoint = "1 0 0 200";
                Option.PriceTagSize = "2 2";

                Option.PriceTagPVP = "PVP";
                Option.PriceTagPVPPoint = "1 0 10 200";

                Option.LabelSize = "0 200 200 290";
                Option.LabelWidth = 450;
                break;
            case 3:
                Option.StorePoint = "1 0 70 5";
                Option.StoreTitleSize = "1 1";

                Option.NamePoint = "1 0 70 10";
                Option.NameSize = "1 2";

                Option.BarcodePoint = "1 0 39 70 65";

                Option.PricePoint = "2 0 70 145";
                Option.PriceSize = "2 2";

                Option.PriceTagEuros = "Euros";
                Option.PriceTagPoint = "4 0 0 150";
                Option.PriceTagSize = "1 1";

                Option.PriceTagPVP = "PVP";
                Option.PriceTagPVPPoint = "4 11 150 150";

                Option.LabelSize = "0 200 200 190";
                Option.LabelWidth = 450;
                break;
            default:
                Option = new();
                break;
        }
        //await Storage.SetValue("BluetoothPrinterConfig", Option.ObjectToJson());
        StateHasChanged();
        await ToastService.Success("提示", $"复位配置{format}成功");
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
        Messages = $"{message}\r\n{Messages}";
        if (message.StartsWith("扫描到蓝牙设备: "))
        {
            Devices ??= new List<BleDevice>();
            Devices!.Add(new BleDevice() { Name = message.TrimStart("扫描到蓝牙设备: ") });
        }
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// 扫描外设
    /// </summary>
    /// <returns></returns>
    private async Task ScanDevice()
    {
        if (!await Init() || IsScanning)
        {
            return;
        }

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

        //开始扫描
        Devices = await Tools.StartScanAsync(serviceUuids: Option.PrinterOnly ? PrinterCharacteristicUuids:null, option: Option);

        if (Devices != null && Devices.Count > 0)
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
                if (Option.AutoConnectService)
                {
                    //连接外设
                    var services = await Tools.ConnectToKnownDeviceAsync(bleDevice.Id, bleDevice.Name);
                    if (services != null && services.Count > 0)
                    {
                        if (Option.PrinterOnly)
                        {
                            if (!IsPrinter(services.Select(a => a.Id)))
                            {
                                bleDevice.ServicesRemark = "isNotPrinter";
                                await Tools_OnMessage($"排除{bleDevice.Name}");
                                await InvokeAsync(StateHasChanged);
                                continue;
                            }
                        }

                        bleDevice.ServicesRemark = $"服务: {services.Count}";
                        services.ForEach(a =>
                        {
                            a.Name = GetServicesName(a.Id);
                            if (GetServicesName(a.Id) == "打印服务")
                            {
                                bleDevice.ServicesRemark = $"打印服务,{bleDevice.ServicesRemark}";
                            }
                        });
                        bleDevice.Services.AddRange(services);
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
                                            a.Name = GetCharacteristicsName(a.Id);
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
                    }
                    await OnDisConnectDevice(true);
                    Messages = $"完成{bleDevice.Name}检查";
                    await InvokeAsync(StateHasChanged);
                }
                DeviceList.Add(
                    new SelectedItem()
                    {
                        Active = IsAutoConnect && bleDevice.Id == BleInfo.DeviceID,
                        Text = string.IsNullOrWhiteSpace(bleDevice.Name) ? bleDevice.Id.ToString() : bleDevice.Name,
                        Value = bleDevice.Id.ToString()
                    });
            }

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
                    Text = a.Name != "Unknown Service" ? $"{a.Name}({GetServicesName(a.Id)})" : GetServicesName(a.Id),
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
            Messages = $"连接{BleInfo.Name}失败. {Messages.Split("\r\n")?.FirstOrDefault()}";
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
    /// 选择服务,获取特征
    /// </summary>
    /// <returns></returns>
    private async Task OnServiceidSelect()
    {
        Characteristics = null;
        Messages = "";
        ReadResult = "";
        CharacteristicList = new List<SelectedItem>() { new SelectedItem() { Text = "请选择...", Value = "" } };
        Characteristics = await Tools.GetCharacteristicsAsync(BleInfo.Serviceid);
        if (Characteristics != null && Characteristics.Count > 0)
        {
            Characteristics.ForEach(a => CharacteristicList.Add(
                new SelectedItem()
                {
                    Active = IsAutoConnect && a.Id == BleInfo.Characteristic,
                    Text = a.Name != "Unknown characteristic" ? $"{a.Name}({(a.CanRead ? "R" : "-")}{(a.CanWrite ? "W" : "-")}{(a.CanUpdate ? "U" : "-")})({GetCharacteristicsName(a.Id)})" : $"({(a.CanRead ? "R" : "-")}{(a.CanWrite ? "W" : "-")}{(a.CanUpdate ? "U" : "-")})({GetCharacteristicsName(a.Id)})",
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

    /// <summary>
    /// 选择特征, 读取数值
    /// </summary>
    /// <returns></returns>
    private async Task OnCharacteristSelect(SelectedItem item)
    {
        if (IsAutoConnect) return;
        try
        {
            if (string.IsNullOrWhiteSpace(item.Value))
            {
                return;
            }
            BleInfo.Characteristic = Guid.Parse(item.Value);
            await ReadDeviceName();
        }
        catch (Exception)
        {
            var message = $"特征码无效";
            await ToastService.Error("提示", message);
        }
    }

    /// <summary>
    /// 读取数值
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// CPCL票据例子
    /// </summary>
    /// <returns></returns>
    private async Task SendDataAsyncCPCL()
    { 
        await SendDataAsyncPrinter(CpclTicket);
    }


    /// <summary>
    /// 标签文件通常以“!”字符作为开头，后接“x”偏置参数、“x”和“y”轴分辨率、标签长度以及要打印的标签数量。包含这些参数的行称为命令起始行。任何情况下，标签文件都是以命令起始行开头，以“PRINT”命令结尾。用于构建具体标签的命令置于这两项命令之间
    /// 指令语法 ! {offset} 200 200 {height} { qty}
    /// 标签横向偏移量 横向DPI 纵向DPI 标签高度 打印数量
    /// </summary>
    /// <returns></returns>
    public string CPCL_Init(string labelSize, int qty = 1)
    {
        return $"! {labelSize} {qty}\r\n";
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

    private async Task SendDataAsyncCPCLTest() => await SendDataAsyncPrinter(PrintCpclLabel(Option));
    private async Task SendDataAsyncCPCLBarcode() => await SendDataAsyncPrinter(CpclBarcode);
    private async Task SendDataAsyncESC() => await SendDataAsyncPrinter(PrintTicketESCPOS_barcode(Option));
    private async Task SendDataAsyncCPCLQR() => await SendDataAsyncPrinter(CpclCommands);

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


