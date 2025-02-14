﻿// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorHybrid.Core.Device;
using Plugin.BLE.Abstractions;
using static BlazorHybrid.Core.Device.BleExtension;

namespace BlazorHybrid.Maui.Shared;

/// <summary>
/// 蓝牙服务
/// </summary>
public partial class BluetoothLEServices
{    
 
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
