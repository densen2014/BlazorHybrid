// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using Foundation;

namespace MauiWithLibs;
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    public static string GetName() => "ios";
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
