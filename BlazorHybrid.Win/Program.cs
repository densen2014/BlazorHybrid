// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

namespace BlazorHybrid.Win;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            MessageBox.Show(text: error.ExceptionObject.ToString(), caption: "Error");
        };

        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Startup.Init();
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }
}
