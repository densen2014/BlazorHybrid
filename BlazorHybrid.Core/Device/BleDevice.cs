// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BootstrapBlazor.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BlazorHybrid.Core.Device;

public class BleUUID
{

    //Android的SSP（协议栈默认）的UUID：00001101-0000-1000-8000-00805F9B34FB，只有使用该UUID才能正常和外部的，也是SSP串口的蓝牙设备去连接。
    public static readonly string SerialPortServiceUUID = "00001101-0000-1000-8000-00805F9B34FB";

    //正常打印机的打印服务
    public static readonly string PrinterNormalServiceUUID = "e7810a71-73ae-499d-8c15-faa9aef0c3f2";
    public static readonly string PrinterNormalCharacteristicUUID = "BEF8D6C9-9C21-4C9E-B632-BD58C1009F9F";

    public static readonly string PrinterServiceUUID = "0000ff00-0000-1000-8000-00805f9b34fb";

    public static readonly string PrinterCharacteristicUUID = "0000ff02-0000-1000-8000-00805f9b34fb";

    public static readonly string GattServiceUUID = "49535343-fe7d-4ae5-8fa9-9fafd205e455";
    public static readonly string GattCharacteristicUUID = "49535343-8841-43F4-A8D4-ECBE34729BB3";
}

/// <summary>
/// 蓝牙设备
/// </summary>
public class BleTagDevice
{

    /// <summary>
    /// 搜索超时时间,默认10秒
    /// </summary>
    [AutoGenerateColumn(GroupName = "配置", GroupOrder = 1)]
    [DisplayName("搜索超时时间,默认10秒")]
    public int ScanTimeout { get; set; } = 10;

    [AutoGenerateColumn(GroupName = "配置", GroupOrder = 1)]
    [DisplayName("按名称查找")]
    public bool ByName { get; set; }

    /// <summary>
    /// 打印机类型
    /// </summary>
    [AutoGenerateColumn(GroupName = "通用", GroupOrder = 0)]
    [DisplayName("打印机类型")]
    public BlePrinterType PrinterType { get; set; }


    /// <summary>
    /// 设备名称
    /// </summary>
    [AutoGenerateColumn(GroupName = "通用", GroupOrder = 0)]
    [DisplayName("设备名称")]
    public string? Name { get; set; }

    /// <summary>
    /// 设备ID
    /// </summary>
    [AutoGenerateColumn(GroupName = "通用", GroupOrder = 0)]
    [DisplayName("设备ID")]
    public Guid DeviceID { get; set; }

    /// <summary>
    /// 服务ID
    /// </summary>
    [AutoGenerateColumn(GroupName = "蓝牙", GroupOrder = 1)]
    [DisplayName("服务ID")]
    public Guid Serviceid { get; set; }

    /// <summary>
    /// 特征ID
    /// </summary>
    [AutoGenerateColumn(GroupName = "蓝牙", GroupOrder = 1)]
    [DisplayName("特征ID")]
    public Guid Characteristic { get; set; }

    public BleTagDevice() { }

    public BleTagDevice(string? name, Guid deviceID, Guid serviceid, Guid characteristic, int scanTimeout = 10, BlePrinterType printerType = BlePrinterType.CPCL)
    {
        Name = name;
        DeviceID = deviceID;
        Serviceid = serviceid;
        Characteristic = characteristic;
        ScanTimeout = scanTimeout;
        PrinterType = printerType;
    }

    public BleTagDevice(string? name, string deviceID, string serviceid, string characteristic, int scanTimeout = 10, BlePrinterType printerType = BlePrinterType.CPCL)
    {
        Name = name;
        if (Guid.TryParse(deviceID, out var _deviceID))
        {
            DeviceID = _deviceID;
        }
        else
        {
            DeviceID = Guid.NewGuid();
            ByName = true;
        }
        Serviceid = Guid.Parse(serviceid);
        Characteristic = Guid.Parse(characteristic);
        ScanTimeout = scanTimeout;
        PrinterType = printerType;
    }
}

/// <summary>
/// 打印机类型
/// </summary>
public enum BlePrinterType
{
    CPCL,
    ESCPOS,
    Other
}

/// <summary>
/// 蓝牙设备信息
/// </summary>
public class BleDevice
{
    /// <summary>
    /// 设备Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [DisplayName("设备名称")]
    public string? Name { get; set; }

    /// <summary>
    /// 信号
    /// </summary>
    [DisplayName("信号")]
    public int Rssi { get; set; }

    /// <summary>
    /// 可连接
    /// </summary>
    [DisplayName("可连接")]
    public bool IsConnectable { get; set; }

    /// <summary>
    /// 服务列表
    /// </summary>
    [DisplayName("服务列表")]
    public List<BleService> Services { get; set; } = new List<BleService>();

    [DisplayName("备注")]
    public string? Remark { get; set; }

    [DisplayName("服务")]
    public string? ServicesRemark { get; set; }


}

/// <summary>
/// 蓝牙设备服务
/// </summary>
public class BleService
{
    /// <summary>
    /// 服务ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 服务名称。
    /// 如果 <see cref="Id"/> 是标准 Id，则返回名称。请参阅<see cref="KnownServices"/>
    /// </summary>
    [DisplayName("服务名称")]
    public string? Name { get; set; }

    /// <summary>
    /// 指示服务类型是主要还是次要
    /// </summary>
    [DisplayName("主服务")]
    public bool IsPrimary { get; set; }

    /// <summary>
    /// 特征列表
    /// </summary>
    public List<BleCharacteristic> Characteristics { get; set; } = new List<BleCharacteristic>();

    [DisplayName("备注")]
    public string? Remark { get; set; }


}

/// <summary>
/// 蓝牙设备特征
/// </summary>
public class BleCharacteristic
{

    /// <summary>
    /// 特征ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 特征Uuid
    /// </summary>
    public string? Uuid { get; set; }

    /// <summary>
    /// 特征名称.
    /// 如果 <see cref="Id"/> 是标准特征的 id，则返回名称.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 获取 <see cref="Value"/> 作为 UTF8 编码字符串表示形式
    /// </summary>
    public string? StringValue { get; set; }

    /// <summary>
    /// 指示是否可以读取特征
    /// </summary>
    public bool CanRead { get; set; }

    /// <summary>
    /// 表示该特征是否可写
    /// </summary>
    public bool CanWrite { get; set; }

    /// <summary>
    /// 指示该特性是否支持通知
    /// </summary>
    public bool CanUpdate { get; set; }
}
