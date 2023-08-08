using BlazorHybrid.Core.Device;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorHybrid.Core;

public partial interface INativeFeatures
{

    Task<bool> BluetoothIsBusy();

    /// <summary>
    /// 获取BLE低功耗蓝牙信息
    /// </summary>
    /// <returns></returns>
    Task<string> CheckPermissionsBluetooth();

    /// <summary>
    /// 设置特定查找的设备名称
    /// </summary>
    void SetTagDeviceName(BleTagDevice ble);

    /// <summary>
    /// 扫描外设,返回设备列表
    /// </summary>
    /// <returns></returns>
    Task<List<BleDevice>?> StartScanAsync();

    /// <summary>
    /// 连接外设
    /// </summary>
    /// <returns></returns>
    Task<List<string>?> ConnectDeviceAsync(BleTagDevice ble);

    /// <summary>
    /// 断开连接
    /// </summary>
    /// <returns></returns>
    Task<bool> DisConnectDeviceAsync();

    Task<List<BleService>?> ConnectToKnownDeviceAsync(Guid deviceID, string? deviceName = null);

    Task<List<BleCharacteristic>?> GetCharacteristicsAsync(Guid serviceid);

    /// <summary>
    /// 读写数据
    /// </summary>
    /// <returns></returns>
    Task<string?> ReadDeviceName(Guid? serviceid, Guid? characteristic);

    /// <summary>
    /// 读数据
    /// </summary>
    /// <param name="characteristic"></param>
    /// <returns></returns>
    Task<byte[]?> ReadDataAsync(Guid characteristic);

    /// <summary>
    /// 写数据
    /// </summary>
    /// <param name="characteristic"></param>
    /// <param name="ary"></param>
    /// <returns></returns>

    Task<bool> SendDataAsync(Guid characteristic, byte[] ary);
    Task GetBatteryLevel();

    /// <summary>
    /// 消息
    /// </summary>
    event Action<string>? OnMessage;

    /// <summary>
    /// 数据接收
    /// </summary>
    event Action<string>? OnDataReceived;

    /// <summary>
    /// 连接状态
    /// </summary>
    event Action<bool>? OnStateConnect;

    event Action<string>? UpdateDevicename;
    event Action<object>? UpdateValue;
    event Action<string>? UpdateStatus;
    event Action<BluetoothDevice>? UpdateDeviceInfo;
    event Action<string>? UpdateError;

}
