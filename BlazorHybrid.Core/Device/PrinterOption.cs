using System.ComponentModel;

namespace BlazorHybrid.Core.Device;


/// <summary>
/// 打印机类型
/// </summary>
public enum PrinterType
{
    [Description("CPCL指令打印机")]
    CPCL,
    [Description("ESC指令打印机")]
    ESC,
    [Description("TSPL指令打印机")]
    TSPL,
}

public class PrinterOption
{
    /// <summary>
    /// 打印机类型
    /// </summary>
    /// <returns></returns>
    [DisplayName("打印机类型")]
    public PrinterType? Type { get; set; } = PrinterType.CPCL;

    /// <summary>
    /// 初始搜索设备名称前缀,默认null
    /// </summary>
    /// <returns></returns>
    [DisplayName("初始搜索设备名称前缀,默认null")]
    public string? NamePrefix { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    /// <returns></returns>
    [DisplayName("设备名称")]
    public string? Devicename { get; set; }

    /// <summary>
    /// 设备ID
    /// </summary>
    /// <returns></returns>
    [DisplayName("设备ID")]
    public string? DeviceID { get; set; }

    // 蓝牙设备/通用属性协议 BluetoothDevice 
    // 串行设备端口 Port 
    // 串行设备写入句柄 SerialWriter
    // 蓝牙设备描述符  MyDescriptor  myDescriptor.writeValue(buffer)

    //> Service: e7810a71-73ae-499d-8c15-faa9aef0c3f2
    //>> Characteristic: bef8d6c9-9c21-4c9e-b632-bd58c1009f9f
    //>> Desc: 00002902-0000-1000-8000-00805f9b34fb

    /// <summary>
    /// 服务UUID/ServiceUUID, 默认0xff00. [非空!]<para></para>
    /// 常见打印机ServiceUUID:
    /// <para>[通用型号/BMAU32/QR380A]</para>
    /// 0000ff00-0000-1000-8000-00805f9b34fb => 0xff00
    /// <para>[InnerPrinter商米内置/BlueToothPrinter/FK-POSP58A+]</para>
    /// e7810a71-73ae-499d-8c15-faa9aef0c3f2<para></para>
    /// <para>[HM-A300]</para>
    /// 0000fee7-0000-1000-8000-00805f9b34fb
    /// <para></para>
    /// 其他设备ServiceUUID:<para></para>
    /// 'heart_rate'<para></para>
    /// 0x1802<para></para>
    /// 0x1803<para></para>
    /// </summary>
    /// <returns></returns>
    [DisplayName("服务UUID / Service UUID")]
    public object? ServiceUuid { get; set; } = 0xff00;

    [DisplayName("服务筛选器")]
    public object? FiltersServices { get; set; } = null;

    /// <summary>
    /// 特征UUID / Characteristic UUID
    /// </summary>
    /// <returns></returns>
    [DisplayName("特征UUID / Characteristic UUID")]
    public object? CharacteristicUuid { get; set; } = 0xff02;

    // 描述UUID / Descriptor UUID 

    /// <summary>
    /// 数据切片大小,默认100
    /// </summary>
    /// <returns></returns>
    [DisplayName("数据切片大小,默认100")]
    public int MaxChunk { get; set; } = 100;
}

