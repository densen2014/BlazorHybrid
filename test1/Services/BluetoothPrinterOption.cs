// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using System.ComponentModel;
namespace BlazorHybrid.Core.Device;

public class BluetoothPrinterOption: BleTagDevice
{

    // 配置

    [DisplayName("自动连接")]
    public bool AutoConnect { get; set; }

    [DisplayName("只搜索打印机")]
    public bool PrinterOnly { get; set; } = true;

    [DisplayName("最低RSSI(-db)")]
    public int MinRssi { get; set; } = 80;

    [DisplayName("分包")]
    public int Chunk { get; set; } = 20;

    // Store name
    [DisplayName("店名")]
    public string? StoreTitle { get; set; }

    [DisplayName("店名位置")]
    public string? StorePoint { get; set; } = "1 0 0 10";

    [DisplayName("店名大小")]
    public string? StoreTitleSize { get; set; } = "1 1";

    // Name
    [DisplayName("名称位置")]
    public string? NamePoint { get; set; } = "2 0 10 50";

    [DisplayName("名称大小")]
    public string? NameSize { get; set; } = "1 2";

    // Barcode
    [DisplayName("条码位置")]
    public string? BarcodePoint { get; set; } = "1 0 50 0 110";

    // Price
    [DisplayName("价格位置")]
    public string? PricePoint { get; set; } = "4 0 0 205";

    [DisplayName("价格大小")]
    public string? PriceSize { get; set; } = "2 2";

    // Euros
    [DisplayName("Euros名称")]
    public string? PriceTagEuros { get; set; } = "Euros";

    [DisplayName("Euros位置")] //pos4
    public string? PriceTagPoint { get; set; } = "4 0 0 230";

    [DisplayName("Euros大小")]
    public string? PriceTagSize { get; set; } = "1 1";

    // PVP 
    [DisplayName("PVP名称")]
    public string? PriceTagPVP { get; set; } = "PVP";

    [DisplayName("PVP位置")]
    public string? PriceTagPVPPoint { get; set; } = "4 0 10 230";


    // Label
    [DisplayName("标签尺寸")]
    public string? LabelPoint { get; set; } = "0 200 200 290";

    [DisplayName("标签宽度")]
    public int? LabelWidth { get; set; } = 450;

    [DisplayName("名称过滤")]
    public string? NameFilter { get; set; }

    // QR
    [DisplayName("QR标签抬头1")]
    public string? StoreTitleQR1 { get; set; }

    [DisplayName("QR标签抬头2")]
    public string? StoreTitleQR2 { get; set; }

    [DisplayName("QR标签抬头3")]
    public string? StoreTitleQR3 { get; set; }

    [DisplayName("QR标签尺寸")]
    public string? StoreQRSize { get; set; } = "10 200 200 400";

    [DisplayName("QR标签宽度")]
    public int? StoreQRWidth { get; set; } = 450;

    [DisplayName("QR ESC价格大小")]
    public int? StoreQREscSize { get; set; } = 51;

    // 测试数据
    [DisplayName("测试店名")]
    public string? TestTitle { get; set; } = "Informatica Co.Ltd.";

    [DisplayName("测试条码")]
    public string? TestBarcode { get; set; } = "1234567890123";

    [DisplayName("测试价格")]
    public string? TestPrice { get; set; } = "15.99";

}
