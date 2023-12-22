using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorHybrid.Core;

public partial interface INativeFeatures
{
    bool IsMaui();
    Task<string> CheckPermissionsCamera();
    Task<string> TakePhoto();
    Task<string> PickFile();

    Task<string> CheckPermissionsLocation(); 

    Task<(double? latitude, double? longitude, string message)> GetCachedLocation();
    Task<(double? latitude, double? longitude, string message)> GetCurrentLocation();

    Task<string> CheckMock();

    double? DistanceBetweenTwoLocations();

    Task<string> ShowSettingsUI();
    string? GetAppInfo();
    Task<string> NavigateToMadrid();
    Task<string> NavigateTo(double latitude, double longitude, string? name = null);
    Task<string> NavigateToPlazaDeEspana();
    Task<string> NavigateToPlazaDeEspanaByPlacemark();
    Task<string> DriveToPlazaDeEspana();
    Task<string> TakeScreenshotAsync();

    string? CacheDirectory();
    string? AppDataDirectory();

    /// <summary>
    /// 获取串口列表
    /// </summary>
    /// <returns></returns>
    List<string>? GetPortlist();

    /// <summary>
    /// 打印
    /// </summary>
    /// <returns></returns>
    Task<string> Print();

    /// <summary>
    /// 读NFC
    /// </summary>
    /// <returns></returns>
    Task<string> ReadNFC();
    Task<string> CheckPermissionsNFC();

    /// <summary>
    /// 客户显示屏
    /// </summary>
    /// <returns></returns>
    Task<string> ExtDSP();

    /// <summary>
    /// 原生消息框
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task Alert(string title, string message, string cancel);

    void LoadUrl(string? url);

    Task ExecuteScriptAsync(string js = "alert('hello from WebView JS')");
    Task<string> SetFlashlight(bool on);

}
