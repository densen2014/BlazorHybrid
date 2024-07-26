// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BootstrapBlazor.Components;
using System.ComponentModel;
namespace BlazorHybrid.Core.Device;

public class BluetoothPrinterOption : BleTagDevice
{

    // 配置

    [AutoGenerateColumn(GroupName = "配置", GroupOrder = 0)]
    [DisplayName("自动连接")]
    public bool AutoConnect { get; set; }

    [AutoGenerateColumn(GroupName = "配置", GroupOrder = 0)]
    [DisplayName("搜索名称过滤")]
    public string? NameFilter { get; set; }

    [AutoGenerateColumn(GroupName = "配置", GroupOrder = 0)]
    [DisplayName("只搜索打印机")]
    public bool PrinterOnly { get; set; } = true;

    [AutoGenerateColumn(GroupName = "配置", GroupOrder = 0)]
    [DisplayName("最低RSSI(-db)")]
    public int MinRssi { get; set; } = 80;

    [AutoGenerateColumn(GroupName = "配置", GroupOrder = 0)]
    [DisplayName("分包")]
    public int Chunk { get; set; } = 20;

    // Store name
    [AutoGenerateColumn(GroupName = "店名", GroupOrder = 2)]
    [DisplayName("店名")]
    public string? StoreTitle { get; set; }

    [AutoGenerateColumn(GroupName = "店名", GroupOrder = 2)]
    [DisplayName("店名位置")]
    public string? StorePoint { get; set; } = "1 0 0 10";

    [AutoGenerateColumn(GroupName = "店名", GroupOrder = 2)]
    [DisplayName("店名大小")]
    public string? StoreTitleSize { get; set; } = "1 1";

    // Name
    [AutoGenerateColumn(GroupName = "名称", GroupOrder = 2)]
    [DisplayName("名称位置")]
    public string? NamePoint { get; set; } = "2 0 10 50";

    [AutoGenerateColumn(GroupName = "名称", GroupOrder = 2)]
    [DisplayName("名称大小")]
    public string? NameSize { get; set; } = "1 2";

    // Barcode
    [AutoGenerateColumn(GroupName = "条码", GroupOrder = 3)]
    [DisplayName("条码位置")]
    public string? BarcodePoint { get; set; } = "1 0 50 0 110";

    // Price
    [AutoGenerateColumn(GroupName = "价格", GroupOrder = 4)]
    [DisplayName("价格位置")]
    public string? PricePoint { get; set; } = "4 0 0 205";

    [AutoGenerateColumn(GroupName = "价格", GroupOrder = 4)]
    [DisplayName("价格大小")]
    public string? PriceSize { get; set; } = "2 2";

    // Euros
    [AutoGenerateColumn(GroupName = "价格后缀", GroupOrder = 5)]
    [DisplayName("Euros名称")]
    public string? PriceTagEuros { get; set; } = "Euros";

    [AutoGenerateColumn(GroupName = "价格后缀", GroupOrder = 5)]
    [DisplayName("Euros位置")] //pos4
    public string? PriceTagPoint { get; set; } = "4 0 0 230";

    [AutoGenerateColumn(GroupName = "价格后缀", GroupOrder = 5)]
    [DisplayName("Euros大小")]
    public string? PriceTagSize { get; set; } = "1 1";

    // PVP 
    [AutoGenerateColumn(GroupName = "价格前缀", GroupOrder = 6)]
    [DisplayName("PVP名称")]
    public string? PriceTagPVP { get; set; } = "PVP";

    [AutoGenerateColumn(GroupName = "价格前缀", GroupOrder = 6)]
    [DisplayName("PVP位置")]
    public string? PriceTagPVPPoint { get; set; } = "4 0 10 230";


    // Label
    [AutoGenerateColumn(GroupName = "标签", GroupOrder = 7)]
    [DisplayName("标签尺寸")]
    public string? LabelPoint { get; set; } = "0 200 200 290";

    [AutoGenerateColumn(GroupName = "标签", GroupOrder = 7)]
    [DisplayName("标签宽度")]
    public int? LabelWidth { get; set; } = 450;

    // QR
    [AutoGenerateColumn(GroupName = "QR", GroupOrder = 8)]
    [DisplayName("QR标签抬头1")]
    public string? StoreTitleQR1 { get; set; }

    [AutoGenerateColumn(GroupName = "QR", GroupOrder = 8)]
    [DisplayName("QR标签抬头2")]
    public string? StoreTitleQR2 { get; set; }

    [AutoGenerateColumn(GroupName = "QR", GroupOrder = 8)]
    [DisplayName("QR标签抬头3")]
    public string? StoreTitleQR3 { get; set; }

    [AutoGenerateColumn(GroupName = "QR", GroupOrder = 8)]
    [DisplayName("QR标签尺寸")]
    public string? StoreQRSize { get; set; } = "10 200 200 400";

    [AutoGenerateColumn(GroupName = "QR", GroupOrder = 8)]
    [DisplayName("QR标签宽度")]
    public int? StoreQRWidth { get; set; } = 450;

    [AutoGenerateColumn(GroupName = "QR", GroupOrder = 8)]
    [DisplayName("QR ESC价格大小")]
    public int? StoreQREscSize { get; set; } = 51;

    // 测试数据
    [AutoGenerateColumn(GroupName = "测试数据", GroupOrder = 9)]
    [DisplayName("测试店名")]
    public string? TestTitle { get; set; } = "Informatica Co.Ltd.";

    [AutoGenerateColumn(GroupName = "测试数据", GroupOrder = 9)]
    [DisplayName("测试条码")]
    public string? TestBarcode { get; set; } = "1234567890123";

    [AutoGenerateColumn(GroupName = "测试数据", GroupOrder = 9)]
    [DisplayName("测试价格")]
    public string? TestPrice { get; set; } = "15.99";

}
