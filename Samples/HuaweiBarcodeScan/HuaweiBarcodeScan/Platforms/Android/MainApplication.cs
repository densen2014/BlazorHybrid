// ********************************** 
//  
// 大王派你去巡山
// 
// **********************************

using Android.App;
using Android.Runtime;

namespace HuaweiBarcodeScan;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
