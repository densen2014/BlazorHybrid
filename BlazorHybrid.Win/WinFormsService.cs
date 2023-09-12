﻿using BlazorHybrid.Core;
using BlazorHybrid.Core.Device;
using System.Diagnostics;
using System.IO.Ports;
using System.Reflection;
using WebView2Control = Microsoft.Web.WebView2.WinForms.WebView2;

namespace BlazorHybrid.Win;

public class WinFormsService : INativeFeatures
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

    public Task<string> TakePhoto() => Task.FromResult("未实现");
    public Task<string> ShowSettingsUI() => Task.FromResult("未实现");
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

    public List<string> GetPortlist()
    {
        return SerialPort.GetPortNames().ToList();
    }
    public string CacheDirectory() => AppDomain.CurrentDomain.BaseDirectory;
    public string AppDataDirectory() => AppDomain.CurrentDomain.BaseDirectory;


    public Task<string> Print()
    {
        if (MessageBox.Show("列出打印机点[是], 模拟打印点[否]", "WPF", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            string printerList = string.Join(Environment.NewLine, System.Drawing.Printing.PrinterSettings.InstalledPrinters.Cast<string>());
            return Task.FromResult($"InstalledPrinters{Environment.NewLine}{printerList}");

        }
        string Filepath = AppDomain.CurrentDomain.BaseDirectory + "\\test.txt";
        System.IO.File.WriteAllText(Filepath, "test");

        PrintDialog Dialog = new PrintDialog();

        Dialog.ShowDialog();

        ProcessStartInfo printProcessInfo = new ProcessStartInfo()
        {
            Verb = "print",
            CreateNoWindow = true,
            FileName = Filepath,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        //Process printProcess = new Process();
        //printProcess.StartInfo = printProcessInfo;
        //printProcess.Start();

        //printProcess.WaitForInputIdle();

        //Thread.Sleep(3000);

        //if (false == printProcess.CloseMainWindow())
        //{
        //    printProcess.Kill();
        //} 
        return Task.FromResult("模拟打印成功");
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

    public Task<(double? latitude, double? longitude, string message)> GetCurrentLocation()
    {
        throw new NotImplementedException();
    }

    public Task<(double? latitude, double? longitude, string message)> GetCachedLocation()
    {
        throw new NotImplementedException();
    }

    public Task<bool> BluetoothIsBusy()
    {
        throw new NotImplementedException();
    }

    public Task<string> CheckPermissionsBluetooth()
    {
        return Task.FromResult("未实现");
    }

    public void SetTagDeviceName(BleTagDevice ble)
    {
        throw new NotImplementedException();
    }

    public Task<List<BleDevice>?> StartScanAsync(BleOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>?> ConnectDeviceAsync(BleTagDevice ble)
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

    public Task Alert(string title, string message, string cancel)
    {
        throw new NotImplementedException();
    }

    public static WebView2Control? WebView { get; set; }
    public void LoadUrl(string? url)
    {
        if (WebView == null) return;
        url ??= "https://0.0.0.0/";
        WebView.CoreWebView2.Navigate(url);
    }


    public async Task ExecuteScriptAsync(string js = "alert('hello from WebView JS')")
    {
        if (WebView == null) return;
        await WebView.ExecuteScriptAsync(js);
    }

    public Task<string> CheckPermissionsNFC()
    {
        throw new NotImplementedException();
    }
}
