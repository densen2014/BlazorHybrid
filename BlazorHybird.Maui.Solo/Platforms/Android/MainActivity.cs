// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BlazorHybird.Maui.Solo;
using DH.NFC;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter(new[] { NfcAdapter.ActionNdefDiscovered }, Categories = new[] { Intent.CategoryDefault }, DataMimeType = NfcPage.MIME_TYPE)]
[IntentFilter(new[] { Platform.Intent.ActionAppAction },
              Categories = new[] { global::Android.Content.Intent.CategoryDefault })]
public class MainActivity : MauiAppCompatActivity
{
    DateTime? lastBackKeyDownTime;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        // 初始化
        CrossNFC.Init(this);

        base.OnCreate(savedInstanceState);

        // 申请所需权限 也可以再使用的时候去申请
        //ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.Camera, Manifest.Permission.RecordAudio, Manifest.Permission.ModifyAudioSettings }, 0);

    }

    protected override void OnResume()
    {
        base.OnResume();

        // 恢复时重新启动NFC监听（Android 10+需要）
        CrossNFC.OnResume();
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);

        // 标签发现拦截
        CrossNFC.OnNewIntent(intent);

        //AppActions 
        Platform.OnNewIntent(intent);
    }

    public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent? e)
    {
        if (keyCode == Keycode.Back && e != null && e.Action == KeyEventActions.Down)
        {
            if (!lastBackKeyDownTime.HasValue || DateTime.Now - lastBackKeyDownTime.Value > new TimeSpan(0, 0, 2))
            {
                Toast.MakeText(this.ApplicationContext, "再按一次退出程序", ToastLength.Short).Show();
                lastBackKeyDownTime = DateTime.Now;
            }
            else
            {
                Finish();
            }
            return true;
        }

        return base.OnKeyDown(keyCode, e);
    }
}
