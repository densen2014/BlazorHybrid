﻿using System;
namespace BlazorHybrid.Core.Device;

public class BluetoothPrinterConfig
{
    // Store name
    public string? StoreTitle { get; set; }
    public string? StoreTitleSize { get; set; } = "1 1";
    public string? StorePoint { get; set; } = "1 0 0 10";

    // Name
    public string? NameSize { get; set; } = "1 2";
    public string? NamePoint { get; set; } = "2 0 10 50";

    // Barcode
    public string? Barcode { get; set; } = "1 0 50 0 110";

    // Price
    public string? PricePoint { get; set; } = "4 0 0 205";
    public string? PriceSize { get; set; } = "2 2";

    // Price PVP and Euros
    public string? PriceTagSize { get; set; } = "1 1";
    public string? PriceTagPoint { get; set; } = "4 0 10 230";
    public string? PriceTagPoint2 { get; set; } = "4 0 10 230";
    public string? PriceTagPVP { get; set; } = "PVP";
    public string? PriceTagEuros { get; set; } = "Euros";

    // Label
    public string? LabelPoint { get; set; } = "0 200 200 290";
    public string? Labelwidth { get; set; } = "450";
    public string? NameFilter { get; set; }

    // 配置
    public string? DeviceID { get; set; }
    public string? DeviceName { get; set; }
    public string? ServiceID { get; set; }
    public string? CharacteristicID { get; set; }
    public bool AutoConnect { get; set; }

    // 测试数据
    public string? TestTitle { get; set; } = "Informatica Co.Ltd.";
    public string? TestBarcode { get; set; } = "1234567890123";
    public string? TestPrice { get; set; } = "15.99";

}
