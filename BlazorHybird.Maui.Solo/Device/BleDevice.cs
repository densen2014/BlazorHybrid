// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BlazorHybrid.Components;

/// <summary>
/// 蓝牙设备
/// </summary>
public class BleTagDevice
{

    /// <summary>
    /// 设备名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 设备ID
    /// </summary>
    public Guid DeviceID { get; set; }

    /// <summary>
    /// 服务ID
    /// </summary>
    public Guid Serviceid { get; set; }

    /// <summary>
    /// 特征ID
    /// </summary>
    public Guid Characteristic { get; set; }

    /// <summary>
    /// 搜索超时时间,默认10秒
    /// </summary>
    public int ScanTimeout { get; set; } = 10;

    public bool ByName { get; set; }  

    /// <summary>
    /// 打印机类型
    /// </summary>
    public BlePrinterType PrinterType { get; set; }

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
            DeviceID= Guid.NewGuid();
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
    /// 如果 <see cref="Id"/> 是标准 Id，则返回名称
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
    /// 获取 Value 作为 UTF8 编码字符串表示形式
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
