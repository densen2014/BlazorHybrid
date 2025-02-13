// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorHybrid.Core.Device;
using Plugin.BLE.Abstractions;

namespace BlazorHybrid.Maui.Shared;

/// <summary>
/// 蓝牙服务
/// </summary>
public partial class BluetoothLEServices
{
    /// <summary>
    /// 电池电量
    /// </summary>
    private readonly Guid Battery_Service = Guid.Parse("0000180f-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 心率服务
    /// </summary>
    private readonly Guid HeartRateMeasurement_Service = Guid.Parse("00002a37-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 设备信息服务
    /// </summary>
    private readonly Guid DeviceInformation_Service = Guid.Parse("0000180a-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 常规信息服务
    /// </summary>
    private readonly Guid Generic_Service = Guid.Parse("0000FEE9-0000-1000-8000-00805F9B34FB");

    /// <summary>
    /// 常规信息服务2
    /// </summary>
    private readonly Guid Generic_Service2 = Guid.Parse("00001800-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 打印服务: 通用型号/BMAU32/QR380A
    /// </summary>
    private readonly Guid Print_Service = Guid.Parse("0000ff00-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 打印服务: InnerPrinter商米内置/BlueToothPrinter/FK-POSP58A+
    /// </summary>
    private readonly Guid Print_Service2 = Guid.Parse("00001800-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 打印服务: HM-A300
    /// </summary>
    private readonly Guid Print_Service3 = Guid.Parse("0000fee7-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 透明 UART 服务
    /// </summary>
    private readonly Guid Transparent_UART_Service = Guid.Parse("49535343-FE7D-4AE5-8FA9-9FAFD205E455");

    /// <summary>
    /// 设备名特征值
    /// </summary>
    private readonly Guid DeviceNameCharacteristic = Guid.Parse("D44BC439-ABFD-45A2-B575-925416129601");

    /// <summary>
    /// 设备名特征值2
    /// </summary>
    private readonly Guid DeviceNameCharacteristic2 = Guid.Parse("00002a00-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 电池电量特征值
    /// </summary>
    private readonly Guid BatteryLevelCharacteristic = Guid.Parse("00002a19-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 写入特征
    /// </summary>
    private readonly Guid WRITEUUID = Guid.Parse("0000ff02-0000-1000-8000-00805f9b34fb");   

    /// <summary>
    /// 监听特征
    /// </summary>
    private readonly Guid NOTIFYUUID = Guid.Parse("0000ff01-0000-1000-8000-00805f9b34fb");  

    /// <summary>
    /// 监听特征
    /// </summary>
    private readonly Guid Notification= Guid.Parse("0000ff03-0000-1000-8000-00805f9b34fb");  
 
    public async Task GetBatteryLevel()
    {
        var scanFilterOptions = new ScanFilterOptions
        {
            ServiceUuids = [Battery_Service, HeartRateMeasurement_Service, DeviceInformation_Service]
        };
        var devices = await StartScanAsync(scanFilterOptions: scanFilterOptions);

        if (Device == null)
        {
            OnMessage?.Invoke($"没有找到{TagDevice.Name}");
            return;
        }

        UpdateDevicename?.Invoke($"设备{Device.Name}");

        if (CurrentAdapter == null)
        {
            return;
        }
        await CurrentAdapter.ConnectToDeviceAsync(Device, new ConnectParameters(false, forceBleTransport: true));

        var ReadResult = await ReadDeviceName(Battery_Service, BatteryLevelCharacteristic);

        UpdateValue?.Invoke($"{ReadResult}%");

    }

    public event Action<string>? UpdateDevicename;

    public event Action<object>? UpdateValue;

    public event Action<string>? UpdateStatus;

    public event Action<BluetoothDevice>? UpdateDeviceInfo;

    public event Action<string>? UpdateError;
}
