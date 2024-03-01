// ********************************** 
//  
// 大王派你去巡山
// 
// **********************************

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Huawei.Hms.Hmsscankit;
using Huawei.Hms.Ml.Scan;
using HuaweiBarcodeScan.Platforms.Android;

namespace HuaweiBarcodeScan;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    //用于保存当前Activity的实例，以便在服务中使用其提供的功能
    public static MainActivity? Instance { get; private set; }
    //保存服务实例，以便将结果传递给服务
    public static IMainActivityService? ScanService { get; set; }
    //请求代码，以便在收到结果时判断是不是自己的请求返回结果了
    private const int REQUEST_CUSTOM_CODE_SCAN = 0x01;//自定义扫码，这里不做演示
    private const int REQUEST_CLASSIC_CODE_SCAN = 0x02;//默认扫码
    private const int DEFINED_CODE = 222;//当用于接收授权结果时判断是不是自己请求的
    public void LaunchScanActivity()
    {
        //检查权限并启动扫码
        if (CheckPermission(new string[] { Android.Manifest.Permission.Camera }, DEFINED_CODE))
        {
            StartScan(this, REQUEST_CLASSIC_CODE_SCAN, new HmsScanAnalyzerOptions.Creator()
                .SetHmsScanTypes(HmsScan.QrcodeScanType)
                .SetPhotoMode(false).Create());
        }
    }
    private bool CheckPermission(string[] permissions, int requestCode)
    {
        var hasAllPermissions = true;
        foreach (string permission in permissions)
        {
            if (ContextCompat.CheckSelfPermission(this, permission) == Permission.Denied)
            {
                hasAllPermissions = false;
                ActivityCompat.RequestPermissions(this, permissions, requestCode);
            }
        }

        return hasAllPermissions;
    }
    /// <summary>
    /// 启动扫码，启动前应检查是否有相机权限
    /// </summary>
    /// <param name="activity"></param>
    /// <param name="request_code"></param>
    /// <param name="ops"></param>
    private void StartScan(Activity activity, int request_code, HmsScanAnalyzerOptions ops)
    {
        ScanUtil.StartScan(activity, request_code, ops);
    }
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        if (resultCode != Result.Ok || data == null)
        {
            Toast.MakeText(this, "结果异常", ToastLength.Short)?.Show();
            return;
        }
        HmsScan? hmsScan;
        switch (requestCode)
        {
            case REQUEST_CUSTOM_CODE_SCAN:
                //这里不演示，直接把结果设为null
                hmsScan = null;
                break;
            case REQUEST_CLASSIC_CODE_SCAN:
                hmsScan = null;
                if (data.Extras != null)
                {
                    hmsScan = data.Extras.Get(ScanUtil.Result) as HmsScan;
                }
                break;
            default:
                hmsScan = null;
                break;
        }
        if (hmsScan != null && !string.IsNullOrWhiteSpace(hmsScan.OriginalValue))
        {
            // 将结果传递给 ScanService
            (ScanService as MainActivityService)?.OnScanResult(hmsScan.OriginalValue);
        }
    }
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Instance = this;
    }
    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        bool hasAllPermissions = true;
        for (int i = 0; i < permissions.Length; i++)
        {
            if (grantResults[i] == Permission.Denied)
            {
                hasAllPermissions = false;
                break;
            }
        }
        if (hasAllPermissions)
        {
            if (requestCode == DEFINED_CODE)
            {
                StartScan(this, REQUEST_CLASSIC_CODE_SCAN, new HmsScanAnalyzerOptions.Creator()
                .SetHmsScanTypes(HmsScan.QrcodeScanType)
                .SetPhotoMode(false).Create());
            }
        }
    }
}
