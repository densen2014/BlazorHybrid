// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using AME;
using BlazorHybrid.Core.Device;
using Plugin.BLE.Abstractions;
using System;

namespace BlazorHybrid.Core.Device;

/// <summary>
/// 蓝牙服务扩展
/// </summary>
public static class BleExtension
{
    #region 服务

    /// <summary>
    /// SSP串口的蓝牙设备,Android的SSP 协议栈默认 ，只有使用该UUID才能正常和外部的，也是SSP串口的蓝牙设备去连接。
    /// </summary>
    public static readonly Guid SerialPortService = Guid.Parse("00001101-0000-1000-8000-00805F9B34FB");

    /// <summary>
    /// GATT
    /// </summary>
    public static readonly Guid GattService = Guid.Parse("49535343-fe7d-4ae5-8fa9-9fafd205e455");

    /// <summary>
    /// 电池电量
    /// </summary>
    public static readonly Guid Battery_Service = Guid.Parse("0000180f-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 心率服务
    /// </summary>
    public static readonly Guid HeartRateMeasurement_Service = Guid.Parse("00002a37-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 设备信息服务
    /// </summary>
    public static readonly Guid DeviceInformation_Service = Guid.Parse("0000180a-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 常规信息服务
    /// </summary>
    public static readonly Guid Generic_Service = Guid.Parse("0000FEE9-0000-1000-8000-00805F9B34FB");

    /// <summary>
    /// 打印服务
    /// </summary>
    public static readonly Guid PrinterNormalService = Guid.Parse("e7810a71-73ae-499d-8c15-faa9aef0c3f2");

    /// <summary>
    /// 打印服务: 通用型号/BMAU32/QR380A
    /// </summary>
    public static readonly Guid Print_Service = Guid.Parse("0000ff00-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 打印服务: InnerPrinter商米内置/BlueToothPrinter/FK-POSP58A+
    /// </summary>
    public static readonly Guid Print_Service2 = Guid.Parse("00001800-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 打印服务: InnerPrinter
    /// </summary>
    public static readonly Guid InnerPrinter_Service = Guid.Parse("00001101-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 打印服务: HM-A300
    /// </summary>
    public static readonly Guid Print_Service3 = Guid.Parse("0000fee7-0000-1000-8000-00805f9b34fb");

    #endregion

    #region 特征值
    /// <summary>
    /// 设备名特征值
    /// </summary>
    public static readonly Guid DeviceNameCharacteristic = Guid.Parse("D44BC439-ABFD-45A2-B575-925416129601");

    /// <summary>
    /// 设备名特征值2
    /// </summary>
    public static readonly Guid DeviceNameCharacteristic2 = Guid.Parse("00002a00-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 电池电量特征值
    /// </summary>
    public static readonly Guid BatteryLevelCharacteristic = Guid.Parse("00002a19-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 写入特征
    /// </summary>
    public static readonly Guid WRITEUUID = Guid.Parse("0000ff02-0000-1000-8000-00805f9b34fb");   

    /// <summary>
    /// 监听特征
    /// </summary>
    public static readonly Guid NOTIFYUUID = Guid.Parse("0000ff01-0000-1000-8000-00805f9b34fb");  

    /// <summary>
    /// 监听特征
    /// </summary>
    public static readonly Guid Notification= Guid.Parse("0000ff03-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 打印通道
    /// </summary>
    public static readonly Guid PrinterNormalCharacteristic = Guid.Parse("BEF8D6C9-9C21-4C9E-B632-BD58C1009F9F");

    /// <summary>
    /// 打印通道
    /// </summary>
    public static readonly Guid PrinterCharacteristic = Guid.Parse("0000ff02-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// 打印通道
    /// </summary>
    public static readonly Guid InnerPrinterCharacteristic = Guid.Parse("00001101-0000-1000-8000-00805f9b34fb");

    /// <summary>
    /// GATT
    /// </summary>
    public static readonly Guid GattCharacteristic = Guid.Parse("49535343-8841-43F4-A8D4-ECBE34729BB3");

    #endregion

    /// <summary>
    /// 打印服务集合
    /// </summary>
    public static Guid[] PrinterServiceUuids = [Print_Service, Print_Service2, Print_Service3, PrinterNormalService, InnerPrinter_Service];

    /// <summary>
    /// 打印通道特征集合
    /// </summary>
    public static Guid[] PrinterCharacteristicUuids = [PrinterNormalCharacteristic, PrinterCharacteristic, InnerPrinterCharacteristic];

    public static bool IsPrinter(this Guid guid) => PrinterServiceUuids.Contains(guid);
    public static bool IsPrinter(this IEnumerable<Guid> guids) => guids.Where(a => PrinterServiceUuids.Contains(a)).Any();
    public static bool IsPrintCharacteristics(this Guid guid) => PrinterCharacteristicUuids.Contains(guid);
    public static bool IsPrintCharacteristics(this IEnumerable<Guid> guids) => guids.Where(a => PrinterCharacteristicUuids.Contains(a)).Any();
    public static string ToShortName (this Guid guid) => guid.ToString().TrimEnd("-0000-1000-8000-00805f9b34fb").Replace("00000000-0000-0000-0000-", "");
    public static string GetServicesName(this Guid guid)
    {
        var name = "";
        if (PrinterServiceUuids.Contains(guid))
        {
            name = "打印服务";
        }
        else if (guid == GattService)
        {
            name = "GATT*";
        } 
        return name!=""?$"{name}({guid.ToShortName()})":
            guid.ToShortName();
    }

    public static string GetCharacteristicsName(this Guid guid)
    {
        var name = "";

        if (PrinterCharacteristicUuids.Contains(guid))
        {
            name = "打印通道";
        }
        else if (guid == GattCharacteristic)
        {
            name = "GATT*";
        }
        else if (guid == WRITEUUID)
        {
            name = "写入";
        }
        else if (guid == NOTIFYUUID || guid == Notification)
        {
            name = "监听";
        }
        return name != "" ? $"{name}({guid.ToShortName()})" :
            guid.ToShortName();
    }
}
