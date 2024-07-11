// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using AME;
using BlazorHybrid.Core.Device;
using BlazorHybrid.Maui.Shared;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static BlazorHybrid.Core.Device.BleUUID;

namespace HybridWebView;

public partial class BtPrinter : ContentView 
{
    private string PrinterNameKey = "PrinterName";
    private string printerName = "Unknown";

    public BtPrinter()
    {
        InitializeComponent();
        PrinterName.Text = printerName = Preferences.Default.Get(PrinterNameKey, printerName); 
    }

    private void OnTestClicked(object sender, EventArgs e)
    {
        printerName = $"Printer {DateTime.Now.Microsecond}";
        Preferences.Default.Set(PrinterNameKey, printerName);
        PrinterName.Text = printerName;
    }

    
}
