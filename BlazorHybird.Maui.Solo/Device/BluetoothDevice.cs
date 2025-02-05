// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

namespace BlazorHybrid.Components;

/// <summary>
/// 蓝牙设备
/// </summary>
public class BluetoothDevice
{
    /// <summary>
    /// 设备名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 设备数值:例如心率/电量%
    /// </summary>
    public decimal? Value { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// 错误
    /// </summary>
    public string? Error { get; set; }

}

