using BlazorHybrid.Core.Device;
using BlazorHybrid.Maui.Shared;
using BootstrapBlazor.Components;
using BootstrapBlazor.WebAPI.Services;
using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace bh003_ble.Pages;

public partial class Index : IAsyncDisposable
{
    [Inject, NotNull]
    BluetoothLEServices? MyBleTester { get; set; }
    [Inject, NotNull] protected IStorage? Storage { get; set; }
    [Inject, NotNull] protected ToastService? ToastService { get; set; }

    public void SetTagDeviceName(BleTagDevice ble)
    {
        MyBleTester.TagDevice = ble;

        if (!isInit)
        {
            MyBleTester.OnMessage += OnMessage;
            MyBleTester.OnDataReceived += OnDataReceived;
            MyBleTester.OnStateConnect += OnStateConnect;
            isInit = true;
        }
    }

    public event Action<string>? OnMessage;
    public event Action<string>? OnDataReceived;
    public event Action<bool>? OnStateConnect;

    bool isInit = false;

    public async Task<List<BleDevice>?> StartScanAsync() => await MyBleTester.StartScanAsync();

    public async Task<List<BleService>?> ConnectToKnownDeviceAsync(Guid deviceID, string? deviceName = null) => await MyBleTester.ConnectToKnownDeviceAsync(deviceID, deviceName);

    public async Task<List<BleCharacteristic>?> GetCharacteristicsAsync(Guid serviceid) => await MyBleTester.GetCharacteristicsAsync(serviceid);

    public async Task<string?> ReadDeviceName(Guid? serviceid, Guid? characteristic) => await MyBleTester.ReadDeviceName(serviceid, characteristic);

    public async Task<byte[]?> ReadDataAsync(Guid characteristic) => await MyBleTester.ReadDataAsync(characteristic);

    public async Task<bool> SendDataAsync(Guid characteristic, byte[] ary) => await MyBleTester.SendDataAsync(characteristic, ary);

    public async Task<bool> DisConnectDeviceAsync() => await MyBleTester.DisConnectDeviceAsync();


    public Task<bool> BluetoothIsBusy() => MyBleTester.BluetoothIsBusy();




    private bool IsScanning = false;
    private List<BleDevice>? Devices { get; set; }
    private List<BleService>? Services { get; set; }
    private List<BleCharacteristic>? Characteristics { get; set; }
    private string? ReadResult { get; set; }
    private string? Message { get; set; } = "";

    BleTagDevice BleInfo { get; set; } = new BleTagDevice();

    private List<SelectedItem> DemoList { get; set; } = new List<SelectedItem>() { new SelectedItem() { Text = "测试数据", Value = "" } };
    private List<SelectedItem> DeviceList { get; set; } = new List<SelectedItem>();
    private List<SelectedItem> ServiceidList { get; set; } = new List<SelectedItem>();
    private List<SelectedItem> CharacteristicList { get; set; } = new List<SelectedItem>();

    private Dictionary<string, object>? IsScanningCss => IsScanning ? new() { { "disabled", "" }, } : null;

    bool IsAutoConnect { get; set; }
    bool IsAuto { get; set; }
    bool IsInit { get; set; }

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

            if (await BluetoothIsBusy())
            {
                await ToastService.Warning("蓝牙正在使用中，请稍后再试");
                return false;
            }
            OnMessage += Tools_OnMessage;
            OnDataReceived += Tools_OnDataReceived;
            OnStateConnect += Tools_OnStateConnect;
            SetTagDeviceName(BleInfo);
            IsInit = true;

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
        if (Message != null && Message.Length > 500) Message = Message.Substring(0, 500);
        Message = $"{message}\r\n{Message}";
        await InvokeAsync(StateHasChanged);
    }


    //扫描外设
    private async void ScanDevice()
    {
        if (!await Init()) return;

        IsScanning = true;
        Devices = null;
        Services = null;
        Characteristics = null;
        Message = "";
        ReadResult = "";
        DeviceList = new List<SelectedItem>() { new SelectedItem() { Text = "请选择...", Value = "" } };

        //开始扫描
        Devices = await StartScanAsync();

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
        if (await DisConnectDeviceAsync())
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
        Services = await ConnectToKnownDeviceAsync(BleInfo.DeviceID, BleInfo.Name);
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
        Characteristics = await GetCharacteristicsAsync(BleInfo.Serviceid);
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
        ReadResult = await ReadDeviceName(BleInfo.Serviceid, BleInfo.Characteristic);
        await Storage.SetValue("bleCharacteristic", BleInfo.Characteristic.ToString());

        if (!string.IsNullOrEmpty(ReadResult)) await ToastService.Information("读取成功", ReadResult);
        //异步更新UI
        await InvokeAsync(StateHasChanged);
    }

    private async Task ReadDataAsync()
    {
        Message = "";
        //读取数值
        var res = await ReadDataAsync(BleInfo.Characteristic);
        if (!string.IsNullOrEmpty(ReadResult)) await ToastService.Information("读取成功", res.ToString());

        //异步更新UI
        await InvokeAsync(StateHasChanged);
    }

    private async Task SendDataAsync()
    {
        Message = "";
        //读取数值
        var res = await SendDataAsync(BleInfo.Characteristic, null);
        await ToastService.Information("成功发送", res.ToString());

        //异步更新UI
        await InvokeAsync(StateHasChanged);
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        OnMessage -= Tools_OnMessage;
        OnDataReceived -= Tools_OnDataReceived;
        OnStateConnect -= Tools_OnStateConnect;
        return new ValueTask();
    }

}

