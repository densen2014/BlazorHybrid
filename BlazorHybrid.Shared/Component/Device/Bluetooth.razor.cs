// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BootstrapBlazor.Components;
using System.IO.Pipelines;

namespace BlazorHybrid.Core.Device;

public partial class Bluetooth : IAsyncDisposable
{
    private bool IsScanning = false;
    private List<BleDevice>? Devices { get; set; }
    private List<BleService>? Services;
    private List<BleCharacteristic>? Characteristics;
    private string? ReadResult;
    private string? Message="";

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

        var deviceID = await Storage.GetValue("bleDeviceID",string.Empty);
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
                IsAuto= true;
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
        ReadResult=message;
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
        if (!await Init()) return ;
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
    ValueTask IAsyncDisposable.DisposeAsync()
    {
        Tools.OnMessage -= Tools_OnMessage;
        Tools.OnDataReceived -= Tools_OnDataReceived;
        Tools.OnStateConnect -= Tools_OnStateConnect;
        return new ValueTask();
    }

}


