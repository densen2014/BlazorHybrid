// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorHybrid.Core.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorHybrid.Core;

public class NullFeatureService : INativeFeatures
{
    public event Action<string>? OnMessage;
    public event Action<string>? OnDataReceived;
    public event Action<bool>? OnStateConnect;
    public event Action<string>? UpdateDevicename;
    public event Action<object>? UpdateValue;
    public event Action<string>? UpdateStatus;
    public event Action<BluetoothDevice>? UpdateDeviceInfo;
    public event Action<string>? UpdateError;

    public bool IsMaui() => false;
    public Task<string> CheckPermissionsCamera() => Task.FromResult("未实现");
    public Task<string> CheckPermissionsLocation() => Task.FromResult("未实现");
    public Task<string> CheckMock() => Task.FromResult("未实现");

    public double? DistanceBetweenTwoLocations() => 0;

    public Task<(double? latitude, double? longitude, string message)> GetCachedLocation()
    {
        double? latitude = null;
        double? longitude = null;
        string? message = "未实现";
        return Task.FromResult((latitude, longitude, message));
    }

    public Task<(double? latitude, double? longitude, string message)> GetCurrentLocation()
    {
        double? latitude = null;
        double? longitude = null;
        string? message = "未实现";
        return Task.FromResult((latitude, longitude, message));
    }

    public Task<string> TakePhoto() => Task.FromResult("未实现");
    public Task<string> ShowSettingsUI() => Task.FromResult("未实现");
    //public string? GetAppInfo() => "未实现";
    public string GetAppInfo()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1";
        return version == "1" ? "" : ("V" + (version.IndexOf('.') == -1 || version.Split('.').Count() == 3 ? version : version.Substring(0, version.LastIndexOf('.'))));
    }
    public Task<string> NavigateToMadrid() => Task.FromResult("未实现");
    public Task<string> NavigateToPlazaDeEspana() => Task.FromResult("未实现");
    public Task<string> NavigateToPlazaDeEspanaByPlacemark() => Task.FromResult("未实现");
    public Task<string> DriveToPlazaDeEspana() => Task.FromResult("未实现");
    public Task<string> TakeScreenshotAsync() => Task.FromResult("未实现");


    public List<string>? GetPortlist()
    {
        return null;
    }
    public string? CacheDirectory() => AppDomain.CurrentDomain.BaseDirectory;
    public string? AppDataDirectory() => AppDomain.CurrentDomain.BaseDirectory;

    public Task<string> Print()
    {
        return Task.FromResult("未实现");
    }

    public Task<string> ReadNFC()
    {
        return Task.FromResult("未实现");
    }

    public Task<string> ExtDSP()
    {
        return Task.FromResult("未实现");
    }

    public Task<string> NavigateTo(double latitude, double longitude, string? name = null)
    {
        return Task.FromResult("未实现");
    }

    public Task<string> PickFile()
    {
        return Task.FromResult("未实现");
    }

    public Task<string> CheckPermissionsBluetooth()
    {
        return Task.FromResult("未实现");
    }

    public void SetTagDeviceName(BleTagDevice ble)
    {
        throw new NotImplementedException();
    }

    public Task<List<BleDevice>?> StartScanAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> DisConnectDeviceAsync()
    {
        throw new NotImplementedException();
    }

    public Task<List<BleService>?> ConnectToKnownDeviceAsync(Guid deviceID, string? deviceName = null)
    {
        throw new NotImplementedException();
    }

    public Task<List<BleCharacteristic>?> GetCharacteristicsAsync(Guid serviceid)
    {
        throw new NotImplementedException();
    }

    public Task<string?> ReadDeviceName(Guid? serviceid, Guid? characteristic)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]?> ReadDataAsync(Guid characteristic)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SendDataAsync(Guid characteristic, byte[] ary)
    {
        throw new NotImplementedException();
    }

    public Task GetBatteryLevel()
    {
        throw new NotImplementedException();
    }

    public Task<bool> BluetoothIsBusy()
    {
        throw new NotImplementedException();
    }

    public Task Alert(string title, string message, string cancel)
    {
        throw new NotImplementedException();
    }

    public void LoadUrl(string? url)
    {
        throw new NotImplementedException();
    }

    public Task ExecuteScriptAsync(string js = "alert('hello from WebView JS')")
    {
        throw new NotImplementedException();
    }

    public Task<string> CheckPermissionsNFC() => Task.FromResult("未实现");

    public Task<(string message, object callback)> CallNativeFeatures(EnumNativeFeatures features, object[]? args, bool? on)
    {
        return Task.FromResult(("未实现", new object()));
    }

    public Task<List<string>?> ConnectDeviceAsync(BleTagDevice ble, bool getNotify = false, byte[]? sentbytes = null)
    {
        throw new NotImplementedException();
    }
}
