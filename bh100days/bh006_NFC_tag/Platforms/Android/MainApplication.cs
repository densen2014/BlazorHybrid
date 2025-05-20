// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using Android.App;
using Android.Runtime;

[assembly: UsesFeature(Android.Manifest.Permission.NfcTransactionEvent, Required = false)]
[assembly: UsesFeature(Android.Manifest.Permission.NfcPreferredPaymentInfo, Required = false)]
[assembly: UsesFeature(Android.Manifest.Permission.Nfc, Required = false)]
[assembly: UsesPermission(Android.Manifest.Permission.NfcTransactionEvent)]
[assembly: UsesPermission(Android.Manifest.Permission.NfcPreferredPaymentInfo)]
[assembly: UsesPermission(Android.Manifest.Permission.Nfc)]
[assembly: UsesPermission(Android.Manifest.Permission.AccessNetworkState)]
[assembly: UsesPermission(Android.Manifest.Permission.Internet)]

namespace bh006_NFC_tag;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
