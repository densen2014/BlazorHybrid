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
public static class PrinterCommands
{

    public static string CpclCommands =
                                """
                                ! 10 200 200 400 1
                                BEEP 1
                                PW 380
                                SETMAG 1 1
                                CENTER
                                TEXT 10 2 10 40 Micro Bar
                                TEXT 12 3 10 75 Blazor
                                TEXT 10 2 10 350 eMenu
                                B QR 30 150 M 2 U 7
                                MA,https://google.com
                                ENDQR
                                FORM
                                PRINT
                                """;

    public static string CpclBarcode =
                                """
                                ! 0 200 200 280 1
                                PW 450
                                CENTER
                                SETMAG 1 1
                                ENCODING GB18030
                                TEXT 4 11 30 40 Coca-Cola 可口可乐
                                BARCODE-TEXT 7 0 5
                                BARCODE 128 1 1 50 10 100 123456789
                                BARCODE-TEXT OFF
                                SETMAG 2 2
                                ENCODING ASCII
                                TEXT 4 11 30 210 PVP:  123.45
                                FORM
                                PRINT
                                """;


    public static string CpclTicket =
                                """
                                ! 0 200 200 290 1
                                BEEP 1
                                PW 465
                                SETMAG 2 2
                                T 2 0 40 40 Big Home
                                SETMAG 1 2
                                T 2 0 30 80 NIF B12345678 
                                SETMAG 1 1
                                T 2 0 0 110 -------------
                                T 2 0 0 140 COLA     1.99
                                T 2 0 0 170 COLA x 2 1.99
                                T 2 0 0 200 COLA Zero 1.99
                                T 2 0 0 230 -------------
                                T 2 0 0 260 Total    5.00
                                T 2 0 0 290 --Gracias----

                                FORM
                                PRINT
                                """;

    public static string PrintCpclQR(BluetoothPrinterOption Option,string? title = null, string? barcode = null, string? price = null)
    {
        title = title ?? Option.TestTitle;
        barcode = barcode ?? Option.TestBarcode;
        price = price ?? Option.TestPrice;

        // 方形标签
        // 店名
        string storeTitleQR1 = Option.StoreTitleQR1 ?? "";
        string storeTitleQR2 = Option.StoreTitleQR2 ?? "";
        string storeTitleQR3 = Option.StoreTitleQR3 ?? "";
        // 标签
        string storeQRSize = Option.StoreQRSize ?? "10 200 200 400";
        int storeQRWidth = Option.StoreQRWidth ?? 450;

        return $"""
               ! {storeQRSize} 1            
               BEEP 1
               PW {storeQRWidth}
               CENTER
               TEXT 10 2 10 40 {storeTitleQR1}
               TEXT 12 3 10 75 {storeTitleQR2}
               TEXT 10 2 10 350 {storeTitleQR3}
               B QR 30 150 M 2 U 7
               MA,{barcode}
               ENDQR
               FORM
               PRINT
               """;
    }


    public static string PrintCpclLabel(BluetoothPrinterOption Option, string? title = null, string? barcode = null, string? price = null)
    {
        title = title ?? Option.TestTitle;
        barcode = barcode ?? Option.TestBarcode;
        price = price ?? Option.TestPrice;

        // Store name
        string storeTitle = Option.StoreTitle ?? "";
        string storeTitleSize = Option.StoreTitleSize ?? "1 1";
        string storePoint = Option.StorePoint ?? "1 0 0 10";

        // Name
        string nameSize = Option.NameSize ?? "1 2";
        string namePoint = Option.NamePoint ?? "2 0 10 50";

        // Barcode
        string barcodePoint = Option.BarcodePoint ?? "1 0 50 0 110";

        // Price
        string pricePoint = Option.PricePoint ?? "4 0 0 205";
        string priceSize = Option.PriceSize ?? "2 2";

        // Price PVP and Euros
        string priceTagSize = Option.PriceTagSize ?? "1 1";
        string priceTagPoint = Option.PriceTagPoint ?? "4 0 10 230";
        string priceTagPVPPoint = Option.PriceTagPVPPoint ?? "4 0 10 230";

        // Label 标签规格设定
        string labelSize = Option.LabelSize ?? "0 200 200 290";
        int labelwidth = Option.LabelWidth ?? 450;
        int escPriceSize = Option.StoreQREscSize ?? 51;

        return $"""
               ! {labelSize} 1
               BEEP 1
               PW {labelwidth}
               CENTER
               SETMAG {storeTitleSize}
               TEXT {storePoint} {storeTitle}
               SETMAG {nameSize}
               TEXT {namePoint} {title}
               BARCODE-TEXT 7 0 5
               BARCODE 128 {barcodePoint} {barcode}
               BARCODE-TEXT OFF
               SETBOLD 1
               SETMAG {priceSize}
               TEXT {pricePoint} {price}
               SETMAG {priceTagSize}
               LEFT
               TEXT {priceTagPVPPoint} {Option.PriceTagPVP}:
               RIGHT
               TEXT {priceTagPoint} {Option.PriceTagEuros}
               SETBOLD 0
               FORM
               PRINT
               """;
    }
    public static string Chr(int AsciiCode) => System.Convert.ToChar(AsciiCode).ToString();

    public static string ESC = Chr(27);
    public const string CRLF = "\r\n";
    public static string GS = Chr(29);
    public static string PrintTicketESCPOS_barcode(BluetoothPrinterOption Option, string? title = null, string? barcode = null, string? price = null, bool printbarcode = false)
    {
        title = title ?? Option.TestTitle;
        barcode = barcode ?? Option.TestBarcode;
        price = price ?? Option.TestPrice;
        if (barcode == "") return "";
        int escPriceSize = Option.StoreQREscSize ?? 51;

        //店名
        string title0 = Option.StoreTitle ?? "";
        string OutputDataQrCode = "";
        ////initialize
        OutputDataQrCode += ESC + "@";
        if (title0.Length > 0)
        {
            //setTextBold
            OutputDataQrCode += ESC + "E" + (char)1;
            //setJustification center
            OutputDataQrCode += ESC + "a" + (char)1;
            OutputDataQrCode += title0 + CRLF;
        }
        //setTextBold
        OutputDataQrCode += ESC + "E" + (char)1;
        //setJustification center
        OutputDataQrCode += ESC + "a" + (char)1;
        //setTextSize
        //OutputDataQrCode += GS + '!' + (char)1;
        OutputDataQrCode += escPriceSize + " " + title + CRLF;
        //OutputDataQrCode += ESC + "d" + (char)1;
        if (printbarcode)
        {
            //initialize
            OutputDataQrCode += ESC + "@";
            //barcode
            OutputDataQrCode += GS + "h2" + GS + 'w' + (char)2 + GS + "k" + (char)73 + (char)(barcode.Length + 2) + (char)123 +
                (char)66 + barcode; 
        }
        //setTextSize reset
        OutputDataQrCode += ESC + "!" + (char)0;
        OutputDataQrCode += ESC + "a" + (char)0;
        OutputDataQrCode += ESC + "E" + (char)1;
        OutputDataQrCode += barcode;
        OutputDataQrCode += CRLF;
        //setTextSize reset
        OutputDataQrCode += ESC + "!" + (char)0;
        OutputDataQrCode += ESC + "E" + (char)0;
        OutputDataQrCode += ESC + "a" + (char)2;
        //setTextSize
        OutputDataQrCode += GS + '!' + (char)1;
        OutputDataQrCode += "PVP: ";
        //setTextSize
        OutputDataQrCode += GS + '!' + (char)escPriceSize;
        OutputDataQrCode += price;
        //setTextSize
        OutputDataQrCode += GS + '!' + (char)1;
        OutputDataQrCode += " EUR";
        OutputDataQrCode += CRLF;
        OutputDataQrCode += ESC + "d" + (char)3;
        return OutputDataQrCode;
    }

}
