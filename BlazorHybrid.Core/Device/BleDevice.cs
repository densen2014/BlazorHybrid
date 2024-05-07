// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using System;

namespace BlazorHybrid.Core.Device;

/// <summary>
/// 蓝牙设备
/// </summary>

public class BleTagDevice
{
    public string? Name { get; set; }
    public Guid DeviceID { get; set; }
    public Guid Serviceid { get; set; }
    public Guid Characteristic { get; set; }

    public int ScanTimeout { get; set; } = 10;
}

public class BleDevice
{
    /// <summary>
    /// 设备Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    public string? Name { get; set; }


}

public class BleService
{
    /// <summary>
    /// Id of the Service.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the service.
    /// Returns the name if the <see cref="Id"/> is a standard Id. See <see cref="KnownServices"/>.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Indicates whether the type of service is primary or secondary.
    /// </summary>
    public bool IsPrimary { get; set; }
}

public class BleCharacteristic
{

    /// <summary>
    /// Id of the characteristic.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// TODO: review: do we need this in any case?
    /// Uuid of the characteristic.
    /// </summary>
    public string? Uuid { get; set; }

    /// <summary>
    /// Name of the characteristic.
    /// Returns the name if the <see cref="Id"/> is a id of a standard characteristic.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets <see cref="Value"/> as UTF8 encoded string representation.
    /// </summary>
    public string? StringValue { get; set; }

    /// <summary>
    /// Indicates wheter the characteristic can be read or not.
    /// </summary>
    public bool CanRead { get; set; }

    /// <summary>
    /// Indicates wheter the characteristic can be written or not.
    /// </summary>
    public bool CanWrite { get; set; }

    /// <summary>
    /// Indicates wheter the characteristic supports notify or not.
    /// </summary>
    public bool CanUpdate { get; set; }
}
