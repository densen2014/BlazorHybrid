// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorHybrid.Core.Device;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System.Data;
using System.Text;


namespace BlazorHybrid.Maui.Shared;

/// <summary>
/// 蓝牙服务
/// </summary>
public partial class BluetoothLEServices
{
    public List<BleDevice> Devices { get; set; } = new List<BleDevice>();

    /// <summary>
    /// 特定查找的设备名称
    /// </summary>
    public BleTagDevice TagDevice { get; set; } = new BleTagDevice();

    /// <summary>
    /// <c>IAdapter</c> 中设备事件的事件参数，
    /// 例如<c>已播发设备</c>、<c>已发现设备</c>、<c>已连接设备</c>和<c>已断开设备</c>
    /// </summary>
    public IDevice? Device = null;

    /// <summary>
    /// 设备信息
    /// </summary>
    public string? TagDeviceInfo { get; private set; }

    /// <summary>
    /// 读取设备名称结果
    /// </summary>
    public string? ReadDeviceNameResult { get; private set; }

    /// <summary>
    /// 通知特征值已更新结果
    /// </summary>
    public List<string> HbResults { get; } = new List<string>();

    /// <summary>
    /// 消息
    /// </summary>
    public event Action<string>? OnMessage;

    /// <summary>
    /// 数据接收
    /// </summary>
    public event Action<string>? OnDataReceived;

    /// <summary>
    /// 连接状态
    /// </summary>
    public event Action<bool>? OnStateConnect;

    private IBluetoothLE? CurrentBle;
    private IAdapter? CurrentAdapter;

    private CancellationTokenSource? _scanForAedCts;

    public BluetoothLEServices()
    {
    }

    private void LazyLoad()
    {
        if (CurrentAdapter == null)
        {
            CurrentBle = CrossBluetoothLE.Current;
            CurrentAdapter = CrossBluetoothLE.Current.Adapter;

            CurrentBle.StateChanged += Ble_StateChanged;

            CurrentAdapter.DeviceConnected += Adapter_DeviceConnected;
            CurrentAdapter.DeviceDisconnected += Adapter_DeviceDisconnected;
        }
    }

    #region BLE状态管理

    public Task<bool> BluetoothIsBusy()
    {
        return Task.FromResult(Device != null && Device.State == DeviceState.Connected);
    }

    private void Ble_StateChanged(object? sender, BluetoothStateChangedArgs e)
    {
        OnMessage?.Invoke($"蓝牙状态改变为{e.NewState}");
    }

    private void Adapter_DeviceConnected(object? sender, DeviceEventArgs e)
    {
        OnMessage?.Invoke($"设备连接成功，{e.Device.Name}");
    }

    private void Adapter_DeviceDisconnected(object? sender, DeviceEventArgs e)
    {
        OnMessage?.Invoke($"断开连接{e.Device.Name}");
        bool connectState = false;
        OnStateConnect?.Invoke(connectState);
    }

    #endregion

    #region 扫描外设

    /// <summary>
    /// 开始扫描
    /// </summary>
    /// <returns></returns>
    public async Task<List<BleDevice>?> StartScanAsync(Guid? deviceGuid = null, Guid[]? serviceUuids = null, ScanFilterOptions? scanFilterOptions = null)
    {
        Devices = new List<BleDevice>();

        //检查获取蓝牙权限
        bool isPermissionPass = await CheckAndRequestBluetoothPermission();
        if (!isPermissionPass)
            return null;

        _scanForAedCts = new CancellationTokenSource();

        LazyLoad();

        if (CurrentAdapter == null)
        {
            OnMessage?.Invoke("蓝牙适配器未初始化");
            return null;
        }
        if (CurrentBle==null || !CurrentBle.IsAvailable)
        {
            OnMessage?.Invoke("蓝牙不可用");
            return null;
        }

        try
        {

            CurrentAdapter!.DeviceDiscovered += Adapter_DeviceDiscovered;
            CurrentAdapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;

            //蓝牙扫描时间
            CurrentAdapter.ScanTimeout = TagDevice.ScanTimeout * 1000;

            //默认LowPower
            CurrentAdapter.ScanMode = ScanMode.LowPower;

            OnMessage?.Invoke($"开始扫描外设, IsAvailable={CurrentBle.IsAvailable}, IsOn={CurrentBle.IsOn}, State={CurrentBle.State}, ScanMode={CurrentAdapter.ScanMode}, ScanTimeout={CurrentAdapter.ScanTimeout / 1000}");

            if (deviceGuid != null && deviceGuid != Guid.Empty)
                Device = await CurrentAdapter.ConnectToKnownDeviceAsync(deviceGuid.Value, cancellationToken: _scanForAedCts.Token);
            if (scanFilterOptions != null)
                await CurrentAdapter.StartScanningForDevicesAsync(scanFilterOptions: scanFilterOptions, cancellationToken: _scanForAedCts.Token);
            else if (serviceUuids == null)
                await CurrentAdapter.StartScanningForDevicesAsync(cancellationToken: _scanForAedCts.Token);
            else
                await CurrentAdapter.StartScanningForDevicesAsync(serviceUuids, cancellationToken: _scanForAedCts.Token);


            OnMessage?.Invoke($"结束扫描外设");
        }
        catch (OperationCanceledException)
        {
            OnMessage?.Invoke($"扫描外设任务取消");
        }
        catch (Exception ex)
        {
            OnMessage?.Invoke($"扫描外设出错, {ex.Message}");
        }
        finally
        {
            CurrentAdapter.DeviceDiscovered -= Adapter_DeviceDiscovered;
            CurrentAdapter.ScanTimeoutElapsed -= Adapter_ScanTimeoutElapsed;
        }

        return Devices;
    }

    /// <summary>
    /// 检查获取蓝牙权限
    /// </summary>
    /// <returns></returns>
    public async Task<bool> CheckAndRequestBluetoothPermission()
    {
#if ANDROID 
        var status = await Microsoft.Maui.ApplicationModel.Permissions.CheckStatusAsync<BluetoothPermissions>();

        if (status != PermissionStatus.Granted)
        {
            status = await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<BluetoothPermissions>();
        }

        return status == PermissionStatus.Granted;
#else
        return true;
#endif
    }

    /// <summary>
    /// 查找到设备处理
    /// </summary>
    private void Adapter_DeviceDiscovered(object? sender, DeviceEventArgs e)
    {
        //[0:] 扫描到蓝牙设备honor Band 4-7E8, Id=00000000-0000-0000-0000-f4bf805ad7e8, Name=honor Band 4-7E8, Rssi=-50, State=Disconnected, AdvertisementRecords.Count=5
        OnMessage?.Invoke($"扫描到蓝牙设备{e.Device}, Id={e.Device.Id}, Name={e.Device.Name}, Rssi={e.Device.Rssi}, State={e.Device.State}, AdvertisementRecords.Count={e.Device.AdvertisementRecords.Count}");

        Devices.Add(new BleDevice() { Id = e.Device.Id, Name = e.Device.Name });

        if ((TagDevice.DeviceID != Guid.Empty && TagDevice.DeviceID == e.Device.Id) || (!string.IsNullOrWhiteSpace(TagDevice.Name) && string.Compare(e.Device.Name, TagDevice.Name, true) == 0))
        {
            TagDeviceInfo = $"{e.Device}, Id={e.Device.Id}, Name={e.Device.Name}, Rssi={e.Device.Rssi}, State={e.Device.State}, AdvertisementRecords.Count={e.Device.AdvertisementRecords.Count}";

            OnMessage?.Invoke($"*找到指定设备* {TagDeviceInfo}");

            Device = e.Device;

            //如果找到目标外设，退出扫描
            if (!_scanForAedCts!.IsCancellationRequested)
                _scanForAedCts.Cancel(false);
        }
    }

    private void Adapter_ScanTimeoutElapsed(object? sender, EventArgs e)
    {
        OnMessage?.Invoke("蓝牙扫描超时结束");
    }

    #endregion

    #region 连接外设

    //连接蓝牙外设
    public async Task<List<string>?> ConnectDeviceAsync(BleTagDevice ble)
    {
        List<string> result = new List<string>();

        if (ble.DeviceID != Guid.Empty)
        {
            TagDevice.DeviceID = ble.DeviceID;
            TagDevice.Name = "";
            await StartScanAsync(ble.DeviceID);
        }
        else if (ble.Name != null)
        {
            TagDevice.Name = ble.Name;
            TagDevice.DeviceID = Guid.Empty;
            await StartScanAsync();
        }
        if (Device == null)
        {
            OnMessage?.Invoke($"没有找到{TagDevice.Name}");
            return null;
        }

        OnMessage?.Invoke($"开始连接{TagDevice.Name}");

        //这个是函数只发起连接，不代表连接成功
        try
        {
            //连接外设
            //设置第二个参数 forceBleTransport = true, 否则错误GattCallback error: 133
            // 连接设备
            await CurrentAdapter!.ConnectToDeviceAsync(Device, new ConnectParameters(false, forceBleTransport: true));
            OnMessage?.Invoke("连接成功");
        }
        catch (DataException ex)
        {
            OnMessage?.Invoke($"Gatt 错误:{ex.Message}");
            return null;
        }


        if (Device!.State == DeviceState.Connected)
        {
            OnMessage?.Invoke($"连接成功{TagDevice.Name}");

            if (ble.Serviceid == Guid.Empty)
            {
                var getServices = await Device.GetServicesAsync();
                ble.Serviceid = getServices.Where(a => a.IsPrimary == true).Select(a => a.Id).FirstOrDefault();
            }

            var genericService = await Device.GetServiceAsync(ble.Serviceid);
            if (genericService == null)
            {
                OnMessage?.Invoke($"获取常规信息服务{ble.Serviceid}失败");
                return null;
            }

            OnMessage?.Invoke($"开始获取特征值");

            //获取特征值集合
            var characteristics = await genericService.GetCharacteristicsAsync();
            if (ble.Characteristic == Guid.Empty)
            {
                ble.Characteristic = characteristics.FirstOrDefault().Id;
            }

            var deviceNameCharacteristic = characteristics.FirstOrDefault(x => x.Id == ble.Characteristic);
            if (deviceNameCharacteristic == null)
            {
                OnMessage?.Invoke($"获取设备名特征值{ble.Characteristic}失败");
                return null;
            }

            bool connectState = true;
            OnStateConnect?.Invoke(connectState);//这里获取成功才为真的连接成功，并可以开始读取Notify数据

            //新增测试代码
            var notifyCharacteristic = await genericService.GetCharacteristicAsync(ble.Characteristic);

            if (notifyCharacteristic != null)
            {
                // 订阅通知，在特征值的 Received 事件中处理设备返回的数据
                notifyCharacteristic.ValueUpdated += NotifyCharacteristic_ValueUpdated;
                await notifyCharacteristic.StartUpdatesAsync();
            }
            else if (notifyCharacteristic == null)
            {
                return null;
            }

            return result;
        }

        //订阅连接丢失
        CurrentAdapter.DeviceDisconnected += CurrentAdapter_DeviceDisconnected;

        //订阅连接断开
        CurrentAdapter.DeviceConnectionLost += CurrentAdapter_DeviceConnectionLost;
        return null; //连接失败
    }

    //连接指定 deviceID 蓝牙外设
    public async Task<List<BleService>?> ConnectToKnownDeviceAsync(Guid deviceID, string? deviceName = null)
    {
        deviceName = deviceName ?? deviceID.ToString();
        if (deviceID != Guid.Empty)
        {
            LazyLoad();
            _scanForAedCts = new CancellationTokenSource();
            Device = await CurrentAdapter!.ConnectToKnownDeviceAsync(deviceID, cancellationToken: _scanForAedCts.Token);
        }
        if (Device == null)
        {
            OnMessage?.Invoke($"没有找到{deviceName}");
            return null;
        }

        OnMessage?.Invoke($"开始连接{deviceName}");

        //这个是函数只发起连接，不代表连接成功
        try
        {
            //连接外设
            //设置第二个参数 forceBleTransport = true, 否则错误GattCallback error: 133
            // 连接设备
            await CurrentAdapter!.ConnectToDeviceAsync(Device, new ConnectParameters(false, forceBleTransport: true));
            OnMessage?.Invoke("连接成功");
        }
        catch (DataException ex)
        {
            OnMessage?.Invoke($"Gatt 错误:{ex.Message}");
            return null;
        }


        if (Device!.State == DeviceState.Connected)
        {
            OnMessage?.Invoke($"连接成功{TagDevice.Name}");

            var getServices = await Device.GetServicesAsync();
            var list = new List<BleService>();
            getServices.ToList().ForEach(a =>
            {
                OnMessage?.Invoke($"获取服务, Id={a.Id}, Name={a.Name}, IsPrimary={a.IsPrimary},");
                list.Add(new BleService() { Id = a.Id, Name = a.Name, IsPrimary = a.IsPrimary });
            });


            return list;
        }

        //订阅连接丢失
        CurrentAdapter.DeviceDisconnected += CurrentAdapter_DeviceDisconnected;

        //订阅连接断开
        CurrentAdapter.DeviceConnectionLost += CurrentAdapter_DeviceConnectionLost;
        return null; //连接失败
    }

    public async Task<List<BleCharacteristic>?> GetCharacteristicsAsync(Guid serviceid)
    {
        if (Device == null)
        {
            OnMessage?.Invoke($"没有找到");
            return null;
        }

        //{0000180a-0000-1000-8000-00805f9b34fb}
        var genericService = await Device.GetServiceAsync(serviceid);

        if (genericService == null)
        {
            OnMessage?.Invoke($"获取常规信息服务失败");
            return null;
        }

        OnMessage?.Invoke($"开始获取特征值");

        var characteristics = await genericService.GetCharacteristicsAsync();
        var list = new List<BleCharacteristic>();
        if (characteristics == null)
        {
            OnMessage?.Invoke($"获取特征值失败.");
            return null;
        }
        else
        {
            characteristics.ToList().ForEach(a =>
            {
                OnMessage?.Invoke($"获取特征, Id={a.Id}, Name={a.Name}, Uuid={a.Uuid}, CanRead={a.CanRead}, CanUpdate={a.CanUpdate}, CanWrite={a.CanWrite}, StringValue={a.StringValue},");
                list.Add(new BleCharacteristic()
                {
                    Id = a.Id,
                    Name = a.Name,
                    Uuid = a.Uuid,
                    CanRead = a.CanRead,
                    CanUpdate = a.CanUpdate,
                    CanWrite = a.CanWrite,
                    StringValue = a.StringValue
                });
            });
            return list;
        }
    }

    //订阅连接丢失
    private void CurrentAdapter_DeviceConnectionLost(object? sender, DeviceErrorEventArgs e)
    {
        OnMessage?.Invoke($"蓝牙连接丢失, {e.Device?.State}");

        CurrentAdapter!.DeviceConnectionLost -= CurrentAdapter_DeviceConnectionLost;
        CurrentAdapter.DeviceDisconnected -= CurrentAdapter_DeviceDisconnected;
    }

    //订阅连接断开
    private void CurrentAdapter_DeviceDisconnected(object? sender, DeviceEventArgs e)
    {
        OnMessage?.Invoke($"蓝牙连接状态变化, {e.Device?.State}");

        CurrentAdapter!.DeviceConnectionLost -= CurrentAdapter_DeviceConnectionLost;
        CurrentAdapter.DeviceDisconnected -= CurrentAdapter_DeviceDisconnected;
    }
    public async Task<bool> DisConnectDeviceAsync()
    {
        if (Device == null)
        {
            OnMessage?.Invoke($"没有连接{TagDevice.Name}");
            return false;
        }
        if (Device.State == DeviceState.Connected)
        {
            await StopUpdatesAsync();
            await CurrentAdapter!.DisconnectDeviceAsync(Device);
        }

        OnMessage?.Invoke($"断开连接{TagDevice.Name}");
        return true;
    }

    public async Task<bool> StopUpdatesAsync()
    {
        if (Device == null)
        {
            OnMessage?.Invoke($"没有连接{TagDevice.Name}");
            return false;
        }
        if (Notify != null && Device.State == DeviceState.Connected)
        {
            await Notify.StopUpdatesAsync();
            Notify = null;
            OnMessage?.Invoke($"停止监听{TagDevice.Name}");
        }

        return true;
    }

    #endregion

    #region 读写数据

    ICharacteristic? Notify;

    //读取设备名
    public async Task<string?> ReadDeviceName(Guid? serviceid, Guid? characteristic)
    {
        ReadDeviceNameResult = null;
        await StopUpdatesAsync();

        OnMessage?.Invoke($"开始获取服务");

        //获取服务集合
        var services = await Device!.GetServicesAsync();
        var infoes = services.Select(x => $"{x.Id}: Name={x.Name}, IsPrimary={x.IsPrimary}");
        string msg = $"服务Uuid: " + string.Join(", ", infoes);
        OnMessage?.Invoke(msg);

        var deviceNameCharacteristic = await GetdeviceNameCharacteristic(serviceid, characteristic);

        if (deviceNameCharacteristic == null)
        {
            OnMessage?.Invoke("获取特征失败.");
        }
        else if (deviceNameCharacteristic.CanRead)
        {
            //读取设备名特征值
            var ary = await ReadDataAsync(deviceNameCharacteristic);
            //var ary = await StartNotifyDataAsync(deviceNameCharacteristic);

            if (ary != null && ary.Length != 0)
            {
                //getUint8：读取1个字节，返回一个无符号的8位整数。
                //  logII('> Battery Level is ' + value.getUint8(0) + '%');
                var hr = (sbyte)ary[0];
                ReadDeviceNameResult = Encoding.ASCII.GetString(ary) + $"  电池 {hr}%";

            }
            else if (ary == null)
            {
                OnMessage?.Invoke("数据为空");
            }
        }
        else if (deviceNameCharacteristic.CanUpdate)
        {
            OnMessage?.Invoke("不可读, 接收消息通知.");
            #region notify类型特征值接收消息通知

            Notify = deviceNameCharacteristic;
            deviceNameCharacteristic.ValueUpdated += NotifyCharacteristic_ValueUpdated;
            await deviceNameCharacteristic.StartUpdatesAsync();

            #endregion
        }


        return ReadDeviceNameResult;
    }

    double parseHeartRate(byte[] heartRateRecord)
    {
        var flags = heartRateRecord[0];
        var offset = 1;
        double intervalLengthInSeconds = 0;
        short hr = 0;
        bool HRC2 = (flags & 1) == 1;
        if (HRC2) //this means the BPM is un uint16
        {
            hr = BitConverter.ToInt16(heartRateRecord, offset);
            offset += 2;
        }
        else //BPM is uint8
        {
            hr = heartRateRecord[offset];
            offset += 1;
        }

        //see if EE is available
        //if so, pull 2 bytes
        bool ee = (flags & (1 << 3)) != 0;
        if (ee)
            offset += 2;

        //see if RR is present
        //if so, the number of RR values is total bytes left / 2 (size of uint16)
        bool rr = (flags & (1 << 4)) != 0;
        if (rr)
        {
            int count = (heartRateRecord.Length - offset) / 2;
            for (int i = 0; i < count; i++)
            {
                //each existence of these values means an R-Wave was already detected
                //the ushort means the time (1/1024 seconds) since last r-wave
                ushort value = BitConverter.ToUInt16(heartRateRecord, offset);

                intervalLengthInSeconds = value / 1024.0;
                offset += 2;
            }
        }
        return hr;
    }

    async Task<ICharacteristic?> GetdeviceNameCharacteristic(Guid? serviceid, Guid? characteristic)
    {
        //获取常规信息服务UUID: 
        Guid genericServiceGuid = serviceid ?? Guid.Parse("00001800-0000-1000-8000-00805f9b34fb");

        var genericService = await Device.GetServiceAsync(genericServiceGuid);
        if (genericService == null)
        {
            OnMessage?.Invoke($"获取常规信息服务{genericServiceGuid}失败");
            return null;
        }

        OnMessage?.Invoke($"开始获取特征值");

        //获取特征值集合
        var characteristics = await genericService.GetCharacteristicsAsync();
        var infoes = characteristics.Select(x => $"{x.Id}: {x.Properties}");
        var msg = $"特征值: " + string.Join(", ", infoes);
        OnMessage?.Invoke(msg);

        //获取设备名特征值
        var deviceNameCharacteristicGuid = characteristic ?? Guid.Parse("00002a00-0000-1000-8000-00805f9b34fb");
        var deviceNameCharacteristic = characteristics.FirstOrDefault(x => x.Id == deviceNameCharacteristicGuid);
        if (deviceNameCharacteristic == null)
        {
            OnMessage?.Invoke($"获取设备名特征值{deviceNameCharacteristicGuid}失败");
            return null;
        }

        return deviceNameCharacteristic;
    }



    public async Task<byte[]?> ReadDataAsync(Guid characteristicID)
    {
        var characteristic = await GetdeviceNameCharacteristic(TagDevice.Serviceid, characteristicID);
        if (characteristic == null)
        {
            OnMessage?.Invoke("获取特征失败.");
            return null;
        }
        return await ReadDataAsync(characteristic);

    }
    //读特征值
    private async Task<byte[]?> ReadDataAsync(ICharacteristic characteristic)
    {
        try
        {
            //读取数据
            var res = await characteristic.ReadAsync();
            byte[] ary = res.data;

            OnMessage?.Invoke($"读取成功，长度={ary.Length}");

            if (ary != null)
            {
                ReadDeviceNameResult = Encoding.ASCII.GetString(ary);
                OnMessage?.Invoke($"读取结果 {ReadDeviceNameResult}");

            }

            return ary;
        }
        catch (Exception ex)
        {
            OnMessage?.Invoke($"读取错误, 目标设备蓝牙连接状态={Device?.State}, {ex.Message}");

            return null;
        }

    }

    public async Task<bool> SendDataAsync(Guid characteristicID, byte[] ary)
    {
        var characteristic = await GetdeviceNameCharacteristic(TagDevice.Serviceid, characteristicID);
        if (characteristic == null)
        {
            OnMessage?.Invoke("获取特征失败.");
            return false;
        }
        return await SendDataAsync(characteristic, ary);
    }

    //写特征值
    private async Task<bool> SendDataAsync(ICharacteristic characteristic, byte[] ary)
    {
        try
        {
            //写入数据
            var res = await characteristic.WriteAsync(ary);
            bool writeSuccess = res > 0;

            OnMessage?.Invoke($"写入结果={writeSuccess}，长度={ary.Length}");

            return writeSuccess;
        }
        catch (Exception ex)
        {
            OnMessage?.Invoke($"写入错误, 目标设备蓝牙连接状态={Device?.State}, {ex.Message}");

            return false;
        }
    }

    private void NotifyCharacteristic_ValueUpdated(object? sender, CharacteristicUpdatedEventArgs e)
    {
        byte[] Tary = e.Characteristic.Value;

        if (Tary != null)
        {
            HbResults.Add(Encoding.ASCII.GetString(Tary));
            // 处理数据
            var data = parseHeartRate(Tary);
            // 触发事件通知页面数据更新
            OnDataReceived?.Invoke($"蓝牙通知新数据 {data}");
            //上述为新增测试刷新代码
            string msg = $"{DateTime.Now}, 收到特征值更新事件, 特征值={e.Characteristic.Id}, 数据包长度={Tary?.Length}";
            OnMessage?.Invoke(msg);
        }
        else
        {
            OnMessage?.Invoke("没有成功获取到数据");
        }
    }

    public async Task<bool> ValueUpdated()
    {
        //获取服务集合
        var services = await Device!.GetServicesAsync();
        var infoes = services.Select(x => $"{x.Id}: Name={x.Name}, IsPrimary={x.IsPrimary}");
        string msg = $"服务Uuid: " + string.Join(", ", infoes);
        OnMessage?.Invoke(msg);

        //获取常规信息服务UUID: 
        //Guid genericServiceGuid = Guid.Parse("00001800-0000-1000-8000-00805f9b34fb");
        Guid genericServiceGuid = Guid.Parse("0000FEE9-0000-1000-8000-00805F9B34FB");
        var genericService = await Device.GetServiceAsync(genericServiceGuid);
        if (genericService == null)
        {
            OnMessage?.Invoke($"获取常规信息服务{genericServiceGuid}失败");
            return false;
        }

        OnMessage?.Invoke($"开始获取特征值");

        //获取特征值集合
        var characteristics = await genericService.GetCharacteristicsAsync();
        infoes = characteristics.Select(x => $"{x.Id}: {x.Properties}");
        msg = $"特征值: " + string.Join(", ", infoes);
        OnMessage?.Invoke(msg);

        //获取设备名特征值
        //Guid deviceNameCharacteristicGuid = Guid.Parse("00002a00-0000-1000-8000-00805f9b34fb");
        Guid deviceNameCharacteristicGuid = Guid.Parse("D44BC439-ABFD-45A2-B575-925416129601");
        var deviceNameCharacteristic = characteristics.FirstOrDefault(x => x.Id == deviceNameCharacteristicGuid);
        if (deviceNameCharacteristic == null)
        {
            OnMessage?.Invoke($"获取设备名特征值{deviceNameCharacteristicGuid}失败");
            return false;
        }
        //新增测试代码
        var notifyCharacteristic = await genericService.GetCharacteristicAsync(deviceNameCharacteristicGuid);

        if (notifyCharacteristic != null)
        {
            // 订阅通知，在特征值的 Received 事件中处理设备返回的数据
            notifyCharacteristic.ValueUpdated += NotifyCharacteristic_ValueUpdated;
            await notifyCharacteristic.StartUpdatesAsync();
        }
        else if (notifyCharacteristic == null)
        {
            return false;
        }

        return true;
    }

    #endregion
}

