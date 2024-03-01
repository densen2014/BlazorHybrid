using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiBarcodeScan.Platforms.Android
{
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
}
