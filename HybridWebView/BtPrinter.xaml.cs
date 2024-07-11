// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

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
