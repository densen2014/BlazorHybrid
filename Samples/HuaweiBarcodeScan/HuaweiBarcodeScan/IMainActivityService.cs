// ********************************** 
//  
// 大王派你去巡山
// 
// **********************************

namespace HuaweiBarcodeScan;

public interface IMainActivityService
{
    /// <summary>
    /// 获取结果，为方便演示这里直接返回扫描到的第一个码值的字符串。
    /// </summary>
    event EventHandler<string>? ScanResult;
    /// <summary>
    /// 启动扫码功能
    /// </summary>
    void StartScan();
}
