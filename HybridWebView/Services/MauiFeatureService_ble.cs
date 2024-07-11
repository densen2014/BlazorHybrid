// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

#if WINDOWS
using Windows.Storage;
using Windows.Media.Capture;
using Windows.System;
using Windows.Foundation.Collections;
using WinRT.Interop;
#endif
#if ANDROID
using Android.Webkit;
using AndroidX.Activity;
using AWebView = Android.Webkit.WebView;
#elif WINDOWS
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.UI.Xaml.Controls.WebView2;
#elif IOS || MACCATALYST
using Foundation;
using WebKit;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
#elif TIZEN
using TWebView = Tizen.NUI.BaseComponents.WebView;
#elif WEBVIEW2_WINFORMS
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.Web.WebView2.WinForms.WebView2;
#elif WEBVIEW2_WPF
using Microsoft.Web.WebView2.Core;
using WebView2Control = Microsoft.Web.WebView2.Wpf.WebView2;
#endif
using BlazorHybrid.Core.Device;

namespace BlazorHybrid.Maui.Shared;

/// <summary>
/// 蓝牙接口实现分部类
/// </summary>
public partial class MauiFeatureService
{

    public Task<bool> ResetBluetooth() => MyBleTester.ResetBluetooth();
    public async Task<List<BleDevice>?> StartScanAsync() => await MyBleTester.StartScanAsync();

    public async Task<List<BleDevice>?> StartScanAsync(Guid? deviceGuid = null, Guid[]? serviceUuids = null) => await MyBleTester.StartScanAsync(deviceGuid, serviceUuids);

    public async Task<List<string>?> ConnectDeviceAsync(BleTagDevice ble, bool getNotify = false, byte[]? sentbytes = null) => await MyBleTester.ConnectDeviceAsync(ble, getNotify, sentbytes);

    public async Task<List<BleService>?> ConnectToKnownDeviceAsync(Guid deviceID, string? deviceName = null) => await MyBleTester.ConnectToKnownDeviceAsync(deviceID, deviceName);

    public async Task<List<BleCharacteristic>?> GetCharacteristicsAsync(Guid serviceid) => await MyBleTester.GetCharacteristicsAsync(serviceid);

    public async Task<string?> ReadDeviceName(Guid? serviceid, Guid? characteristic) => await MyBleTester.ReadDeviceName(serviceid, characteristic);

    public async Task<byte[]?> ReadDataAsync(Guid characteristic) => await MyBleTester.ReadDataAsync(characteristic);

    public async Task<bool> SendDataAsync(Guid characteristic, byte[]? ary) => await MyBleTester.SendDataAsync(characteristic, ary);

    public async Task<bool> SendDataAsyncChunk(Guid characteristic, byte[]? ary, int chunk = 20) => await MyBleTester.SendDataAsync(characteristic, ary, chunk);

    public async Task<bool> SendDataAsync(Guid characteristic, string commands, int chunk = 0) => await MyBleTester.SendDataAsync(characteristic, commands, chunk);

    public async Task<bool> DisConnectDeviceAsync() => await MyBleTester.DisConnectDeviceAsync();

    public async Task GetBatteryLevel() => await MyBleTester.GetBatteryLevel();

    public Task<bool> BluetoothIsBusy() => MyBleTester.BluetoothIsBusy();

}
