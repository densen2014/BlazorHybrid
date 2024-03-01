// ********************************** 
//  
// 大王派你去巡山
// 
// **********************************

namespace HuaweiBarcodeScan.Platforms.Android;

public class MainActivityService : IMainActivityService
{
    public event EventHandler<string>? ScanResult;

    public void StartScan()
    {
        // 调用 MainActivity 中的自定义方法
        MainActivity.Instance.LaunchScanActivity();
    }
    public void OnScanResult(string result)
    {
        ScanResult?.Invoke(this, result);
    }
}
