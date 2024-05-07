// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BootstrapBlazor.Components;
using System.Text;

namespace BlazorHybrid.Core.Device;

public partial class Bluetooth : IAsyncDisposable
{
    private bool IsScanning = false;
    private List<BleDevice>? Devices { get; set; }
    private List<BleService>? Services;
    private List<BleCharacteristic>? Characteristics;
    private string? ReadResult;
    private new string? Message = "";

    BleTagDevice BleInfo { get; set; } = new BleTagDevice();

    private List<SelectedItem> DemoList { get; set; } = new List<SelectedItem>() { new SelectedItem() { Text = "测试数据", Value = "" } };
    private List<SelectedItem> DeviceList { get; set; } = new List<SelectedItem>();
    private List<SelectedItem> ServiceidList { get; set; } = new List<SelectedItem>();
    private List<SelectedItem> CharacteristicList { get; set; } = new List<SelectedItem>();

    private Dictionary<string, object>? IsScanningCss => IsScanning ? new() { { "disabled", "" }, } : null;

    List<string> DemoDeviceList = new List<string>() {
        "iPad\r\n00000000-0000-0000-0000-6ed0f225bb9d\r\n0000180a-0000-1000-8000-00805f9b34fb\r\n00002a24-0000-1000-8000-00805f9b34fb",
        "iPad\r\n00000000-0000-0000-0000-6ed0f225bb9d\r\n0000180a-0000-1000-8000-00805f9b34fb\r\n00002a29-0000-1000-8000-00805f9b34fb",
        "QR380A-165D\r\n00000000-0000-0000-0000-047f0ea2165d\r\n0000180a-0000-1000-8000-00805f9b34fb\r\n00002a29-0000-1000-8000-00805f9b34fb",
        "QR380A-165D\r\n00000000-0000-0000-0000-047f0ea2165d\r\n0000180a-0000-1000-8000-00805f9b34fb\r\n00002a24-0000-1000-8000-00805f9b34fb",
        "QR380A-165D\r\n00000000-0000-0000-0000-047f0ea2165d\r\n0000180a-0000-1000-8000-00805f9b34fb\r\n00002a25-0000-1000-8000-00805f9b34fb",
        "",
        };

    bool IsAutoConnect { get; set; }
    bool IsAuto { get; set; }
    bool IsInit { get; set; }

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
                await ToastService.Warning("目前只支持MAUI");
                return false;
            }
            if (await Tools.BluetoothIsBusy())
            {
                await ToastService.Warning("蓝牙正在使用中，请稍后再试");
                return false;
            }
            Tools.OnMessage += Tools_OnMessage;
            Tools.OnDataReceived += Tools_OnDataReceived;
            Tools.OnStateConnect += Tools_OnStateConnect;
            Tools.SetTagDeviceName(BleInfo);
            IsInit = true;

            DemoDeviceList.ForEach(a => DemoList.Add(new SelectedItem() { Text = a.Split('\r')[0], Value = a }));

            StateHasChanged();

            var deviceID = await Storage.GetValue("bleDeviceID", string.Empty);
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
                    await AutoRead();

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


    private async Task AutoRead()
    {
        Services = null;
        Characteristics = null;
        Message = "";
        ReadResult = "";
        Devices = new List<BleDevice>() { new BleDevice() { Id = BleInfo.DeviceID, Name = BleInfo.Name } };
        DeviceList = new List<SelectedItem>() { new SelectedItem() { Text = BleInfo.Name, Value = BleInfo.DeviceID.ToString() } };
        IsAutoConnect = true;
        await OnDeviceSelect();
        IsAutoConnect = false;
    }

    private async Task OnStateChanged(bool value)
    {
        await Storage.SetValue("bleAutoConnect", value.ToString());
    }

    private async Task OnDemoDeviceSelect(SelectedItem item)
    {
        if (IsAutoConnect || item.Value == "") return;
        await Test(item.Value);
    }

    private async Task Test(string type)
    {
        var S_Battery = 0x0000180f;

        var BatteryLevel = 0x00002a19;
        var DeviceName = 0x00002a00;
        var ManufacturerName = 0x00002a29;
        var ModelNumber = 0x00002a24;
        var SerialNumber = 0x00002a25;

        var sp = type.Replace("\r\n", ",").Split(',');
        BleInfo.Name = sp[0];
        BleInfo.DeviceID = Guid.Parse(sp[1]);
        BleInfo.Serviceid = Guid.Parse(sp[2]);
        BleInfo.Characteristic = Guid.Parse(sp[3]);

        await AutoRead();
    }

    private void Tools_OnStateConnect(bool obj)
    {

    }

    private async void Tools_OnDataReceived(string message)
    {
        ReadResult = message;
        Tools_OnMessage(message);
        await InvokeAsync(StateHasChanged);
    }

    private async void Tools_OnMessage(string message)
    {
        //if (Message !=null && Message.Length >1500) Message= Message.Substring (0, 1500);
        Message = $"{Message}\r\n{message}";
        await InvokeAsync(StateHasChanged);
    }


    //扫描外设
    private async void ScanDevice()
    {
        if (!await Init()) return;
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

        if (Devices != null)
        {
            Devices.ForEach(a => DeviceList.Add(new SelectedItem() { Active = IsAutoConnect && a.Id == BleInfo.DeviceID, Text = a.Name ?? a.Id.ToString(), Value = a.Id.ToString() }));
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

    private async Task OnDisConnectDevice()
    {
        if (await Tools.DisConnectDeviceAsync())
            await ToastService.Success("断开成功");
        else
            await ToastService.Error("断开失败");
    }

    private async Task OnDeviceSelect()
    {

        Services = null;
        Characteristics = null;
        Message = "";
        ReadResult = "";
        ServiceidList = new List<SelectedItem>() { new SelectedItem() { Text = "请选择...", Value = "" } };
        //连接外设
        Services = await Tools.ConnectToKnownDeviceAsync(BleInfo.DeviceID, BleInfo.Name);
        if (Services != null)
        {
            Services.ForEach(a => ServiceidList.Add(new SelectedItem() { Active = IsAutoConnect && a.Id == BleInfo.Serviceid, Text = a.Name ?? a.Id.ToString(), Value = a.Id.ToString() }));
            await Storage.SetValue("bleDeviceID", BleInfo.DeviceID.ToString());
            await Storage.SetValue("bleDeviceName", BleInfo.Name ?? "上次设备");
            if (BleInfo.Serviceid != Guid.Empty && IsAutoConnect)
            {
                await OnServiceidSelect();
            }
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
    private async Task OnServiceidSelect()
    {
        Characteristics = null;
        Message = "";
        ReadResult = "";
        CharacteristicList = new List<SelectedItem>() { new SelectedItem() { Text = "请选择...", Value = "" } };
        Characteristics = await Tools.GetCharacteristicsAsync(BleInfo.Serviceid);
        if (Characteristics != null)
        {
            Characteristics.ForEach(a => CharacteristicList.Add(new SelectedItem() { Active = IsAutoConnect && a.Id == BleInfo.Characteristic, Text = a.Name ?? a.Id.ToString(), Value = a.Id.ToString() }));
            await Storage.SetValue("bleServiceid", BleInfo.Serviceid.ToString());
            if (BleInfo.Characteristic != Guid.Empty && IsAutoConnect)
            {
                await ReadDeviceName();
            }
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
        if (!string.IsNullOrEmpty(ReadResult)) await ToastService.Information("读取成功", res.ToString());

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

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//注册Nuget包System.Text.Encoding.CodePages中的编码到.NET Core
        Encoding utf8 = Encoding.GetEncoding(65001);
        Encoding gb2312 = Encoding.GetEncoding("gb2312");//这里用转化解决汉字乱码问题Encoding.Default ,936
        byte[] bytesUtf8 = utf8.GetBytes(cmds.ToString());
        byte[] bytesGb2312 = Encoding.Convert(utf8, gb2312, bytesUtf8);
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



        //异步更新UI
        //await InvokeAsync(StateHasChanged);
    }
    ValueTask IAsyncDisposable.DisposeAsync()
    {
        Tools.OnMessage -= Tools_OnMessage;
        Tools.OnDataReceived -= Tools_OnDataReceived;
        Tools.OnStateConnect -= Tools_OnStateConnect;
        return new ValueTask();
    }

}


