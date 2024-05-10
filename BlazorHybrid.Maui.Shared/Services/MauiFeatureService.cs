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
using BlazorMaui.Platforms.Windows;
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
using BlazorHybrid.Core;
using BlazorHybrid.Core.Device;
using Microsoft.Maui.Platform;
using System.Diagnostics.CodeAnalysis;
using System.IO.Ports;

namespace BlazorHybrid.Maui.Shared;

public class MauiFeatureService : Page, INativeFeatures
{
    [NotNull]
    BluetoothLEServices? MyBleTester;


    public event Action<string>? OnMessage;
    public event Action<string>? OnDataReceived;
    public event Action<bool>? OnStateConnect;
    public event Action<string>? UpdateDevicename;

    public event Action<object>? UpdateValue;

    public event Action<string>? UpdateStatus;

    public event Action<BluetoothDevice>? UpdateDeviceInfo;

    public event Action<string>? UpdateError;

    private CancellationTokenSource? _cancelTokenSource;
    private bool _isCheckingLocation;

    public MauiFeatureService(BluetoothLEServices myBleTester)
    {
        MyBleTester = myBleTester;
    }

    public bool IsMaui() => true;
    public Task<string> ShowSettingsUI()
    {
        //显示应用设置
        AppInfo.Current.ShowSettingsUI();
        return Task.FromResult("OK");
    }

    public string GetAppInfo()
    {
        try
        {
            //读取应用信息
            string name = AppInfo.Current.Name;
            string package = AppInfo.Current.PackageName;
            string version = AppInfo.Current.VersionString;
            string build = AppInfo.Current.BuildString;
            //return $"{name},{version}.{build}";
            return version == "1" ? "" : ("V" + (version.IndexOf('.') == -1 || version.Split('.').Count() == 3 ? version : version.Substring(0, version.LastIndexOf('.'))));

        }
        catch (Exception)
        {
            return AppInfo.Current.VersionString;
        }
    }


#if ANDROID
    public async Task<PermissionStatus> CheckAndRequestLocationPermission()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<CameraAndLocationPerms>();

        if (status == PermissionStatus.Granted)
            return status;

        if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
        {
            // Prompt the user to turn on in settings
            // On iOS once a permission has been denied it may not be requested again from the application
            return status;
        }

        if (Permissions.ShouldShowRationale<CameraAndLocationPerms>())
        {
            // Prompt the user with additional information as to why the permission is needed
        }

        status = await Permissions.RequestAsync<CameraAndLocationPerms>();

        return status;
    }



#endif


    public async Task<string> CheckPermissionsCamera()
    {
        var res = "";
        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
        }
        res = status.ToString();
        status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Microphone>();
        }
        res += " " + status.ToString();
        status = await Permissions.CheckStatusAsync<Permissions.Speech>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Speech>();
        }
        res += " " + status.ToString();

        return res;
    }

    public async Task<string> CheckPermissionsLocation()
    {
        //检查权限的当前状态
        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();

        //请求权限
        if (status != PermissionStatus.Granted && status != PermissionStatus.Denied)
        {
            status = await Permissions.RequestAsync<Permissions.LocationAlways>();
        }
        if (status != PermissionStatus.Granted)
        {

            //检查权限的当前状态
            status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            //请求权限
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
        }

        return status.ToString();
    }

#if ANDROID
    public async Task<string> CheckPermissionsBluetooth()
    {
        var status = await Permissions.CheckStatusAsync<BluetoothPermissions>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<BluetoothPermissions>();
        }

        return status.ToString();
    }
#else
    public Task<string> CheckPermissionsBluetooth()
    {
        return Task.FromResult("无需授权");
    }
#endif

    /// <summary>
    /// 拍照
    /// CapturePhotoAsync调用该方法以打开相机, 让用户拍照。 如果用户拍照, 该方法的返回值将是非 null 值。
    /// 以下代码示例使用媒体选取器拍摄照片并将其保存到缓存目录：
    /// </summary>
    public async Task<string> TakePhoto()
    {
        await CheckPermissionsCamera();

        if (MediaPicker.Default.IsCaptureSupported)
        {
#if WINDOWS
            CameraCaptureUI(new MediaPickerOptions() { Title = "拍照" });
            var res = await CaptureFileAsync(CameraCaptureUIMode.Photo);
            return res?.Path;
#else
            FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

            if (photo != null)
            {
                // save the file into local storage
                string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                using Stream sourceStream = await photo.OpenReadAsync();
                using FileStream localFileStream = File.OpenWrite(localFilePath);

                await sourceStream.CopyToAsync(localFileStream);
                return localFilePath;
            }
            else
            {
                photo = await MediaPicker.Default.PickPhotoAsync();
                if (photo != null)
                {
                    return photo.FullPath;
                }
            }
            return null;
#endif
        }

        return null;
    }

#if WINDOWS

    private LauncherOptions _launcherOptions;

    public void CameraCaptureUI(MediaPickerOptions options)
    {
        var hndl = WindowStateManager.Default.GetActiveWindow()?.GetWindowHandle();
        if (hndl != null)
        {
            _launcherOptions = new LauncherOptions();
            InitializeWithWindow.Initialize(_launcherOptions, hndl.Value);

            _launcherOptions.TreatAsUntrusted = false;
            _launcherOptions.DisplayApplicationPicker = false;
            _launcherOptions.TargetApplicationPackageFamilyName = "Microsoft.WindowsCamera_8wekyb3d8bbwe";
        }
    }

    public async Task<StorageFile> CaptureFileAsync(CameraCaptureUIMode mode)
    {
        var extension = mode == CameraCaptureUIMode.Photo ? ".jpg" : ".mp4";

        var currentAppData = ApplicationData.Current;
        var tempLocation = currentAppData.LocalCacheFolder;
        var tempFileName = $"CCapture{extension}";
        var tempFile = await tempLocation.CreateFileAsync(tempFileName, CreationCollisionOption.GenerateUniqueName);
        var token = Windows.ApplicationModel.DataTransfer.SharedStorageAccessManager.AddFile(tempFile);

        var set = new ValueSet();
        if (mode == CameraCaptureUIMode.Photo)
        {
            set.Add("MediaType", "photo");
            set.Add("PhotoFileToken", token);
        }
        else
        {
            set.Add("MediaType", "video");
            set.Add("VideoFileToken", token);
        }

        var uri = new Uri("microsoft.windows.camera.picker:");
        var result = await Windows.System.Launcher.LaunchUriForResultsAsync(uri, _launcherOptions, set);
        if (result.Status == LaunchUriStatus.Success && result.Result != null)
        {
            return tempFile;
        }

        return null;
    }
#endif

    /// <summary>
    /// 获取最后一个已知位置, 设备可能已缓存设备的最新位置。
    /// 使用此方法 GetLastKnownLocationAsync 访问缓存的位置（如果可用）。
    /// 这通常比执行完整位置查询更快, 但可能不太准确。
    /// 如果不存在缓存位置, 此方法将 null返回 。
    /// </summary>
    /// <returns></returns>
    public async Task<(double? latitude, double? longitude, string message)> GetCachedLocation()
    {
        await CheckPermissionsLocation();
        string? result = null;
        try
        {
            Location? location = await Geolocation.Default.GetLastKnownLocationAsync();

            if (location != null)
            {
                result = $"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}";
                Console.WriteLine(result);
                result = $"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}";
            }
        }
        catch (FeatureNotSupportedException fnsEx)
        {
            // Handle not supported on device exception
            result = $"设备不支持, {fnsEx.Message}";
        }
        catch (FeatureNotEnabledException fneEx)
        {
            // Handle not enabled on device exception
            result = $"未在设备上启用, {fneEx.Message}";
        }
        catch (PermissionException pEx)
        {
            // Handle permission exception
            result = $"权限, {pEx.Message}";
        }
        catch (Exception ex)
        {
            // Unable to get location
            result = $"无法获取位置, {ex.Message}";
        }
        finally
        {
            _isCheckingLocation = false;
        }
        return (null, null, result ?? "无");
    }


    /// <summary>
    /// 获取当前位置
    /// 虽然检查设备 的最后已知位置 可能更快, 但它可能不准确。
    /// 使用该方法 GetLocationAsync 查询设备的当前位置。
    /// 可以配置查询的准确性和超时。
    /// 最好是使用 GeolocationRequest 和 CancellationToken 参数的方法重载, 
    /// 因为可能需要一些时间才能获取设备的位置。
    /// </summary>
    /// <returns></returns>
    public async Task<(double? latitude, double? longitude, string message)> GetCurrentLocation()
    {
        await CheckPermissionsLocation();
        string? result = null;
        try
        {
            _isCheckingLocation = true;

            GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

            _cancelTokenSource = new CancellationTokenSource();

#if IOS
            //从 iOS 14 开始, 用户可能会限制应用检测完全准确的位置。
            //该 Location.ReducedAccuracy 属性指示位置是否使用降低的准确性。
            //若要请求完全准确性, 请将 GeolocationRequest.RequestFullAccuracy 属性设置为 true
            request.RequestFullAccuracy = true;
#endif
            //TODO mac会弹窗两次请求权限，提issue
            Location? location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

            if (location != null)
            {
                result = $"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}";
                Console.WriteLine(result);
                return (location.Latitude, location.Longitude, result);
            }
        }
        catch (FeatureNotSupportedException fnsEx)
        {
            // Handle not supported on device exception
            result = $"设备不支持, {fnsEx.Message}";
        }
        catch (FeatureNotEnabledException fneEx)
        {
            // Handle not enabled on device exception
            result = $"未在设备上启用, {fneEx.Message}";
        }
        catch (PermissionException pEx)
        {
            // Handle permission exception
            result = $"权限, {pEx.Message}";
        }
        catch (Exception ex)
        {
            // Unable to get location
            result = $"无法获取位置, {ex.Message}";
        }
        finally
        {
            _isCheckingLocation = false;
        }
        return (null, null, result ?? "无");
    }

    public void CancelRequest()
    {
        if (_isCheckingLocation && _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested == false)
            _cancelTokenSource.Cancel();
    }

    /// <summary>
    ///检测模拟位置
    ///一些设备可能会从提供程序或通过提供模拟位置的应用程序返回模拟位置。
    ///可以使用任意Location值来检测此情况IsFromMockProvider：
    /// </summary>
    /// <returns></returns>
    public async Task<string> CheckMock()
    {
        GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium);
        Location? location = await Geolocation.Default.GetLocationAsync(request);

        if (location != null && location.IsFromMockProvider)
        {
            return "来着模拟位置";
        }
        return "无";
    }

    //马德里市中心到西班牙广场之间的距离
    Location boston = new Location(40.4381311, -3.8196197);
    Location sanFrancisco = new Location(40.3989384, -3.6907709);


    /// <summary>
    /// 两个位置之间的距离
    /// 该方法 Location.CalculateDistance 计算两个地理位置之间的距离。
    /// 此计算距离不考虑道路或其他路径, 只是地球表面两点之间的最短距离。
    /// 此计算称为 大圆距离 计算
    /// </summary>
    public double? DistanceBetweenTwoLocations()
    {
        return Location.CalculateDistance(boston, sanFrancisco, DistanceUnits.Miles);
    }

    /// <summary>
    /// 使用地图
    /// </summary>
    /// <returns></returns>
    public async Task<string> NavigateToMadrid()
    {
        var location = new Location(40.4381311, -3.8196197);
        var options = new MapLaunchOptions { Name = "Madrid 马德里" };

        try
        {
            if (await Map.Default.TryOpenAsync(location, options) == false)
            {
                return "地图打开失败";
            }
            return "OK";
        }
        catch (Exception ex)
        {
            return $"没有可打开的地图应用程序, 消息: {ex.Message}";
        }
    }

    /// <summary>
    /// 导航到位置
    /// </summary>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<string> NavigateTo(double latitude, double longitude, string? name = null)
    {
        var location = new Location(latitude, longitude);
        var options = new MapLaunchOptions { Name = name ?? $"{location.Latitude},{location.Longitude}" };

        try
        {
            if (await Map.Default.TryOpenAsync(location, options) == false)
            {
                return "地图打开失败";
            }
            return "OK";
        }
        catch (Exception ex)
        {
            return $"没有可打开的地图应用程序, 消息: {ex.Message}";
        }
    }

    /// <summary>
    /// 使用 a Placemark 打开地图时, 需要更多信息。 此信息可帮助地图应用搜索要查找的位置。
    /// </summary>
    /// <returns></returns>
    public async Task<string> NavigateToPlazaDeEspana()
    {
        var placemark = new Placemark
        {
            CountryName = "Spain",
            AdminArea = "MA",
            Thoroughfare = "Plaza de España",
            Locality = "Madrid"
        };
        var options = new MapLaunchOptions { Name = "Plaza de España 西班牙广场" };

        try
        {
            await Map.Default.OpenAsync(placemark, options);
            return "OK";
        }
        catch (Exception ex)
        {
            return $"没有可打开的地图应用程序或无法定位地标, 消息: {ex.Message}";
        }
    }

    /// <summary>
    /// 扩展方法
    /// </summary>
    /// <returns></returns>
    public async Task<string> NavigateToPlazaDeEspanaByPlacemark()
    {
        var placemark = new Placemark
        {
            CountryName = "Spain",
            AdminArea = "MA",
            Thoroughfare = "Plaza de España",
            Locality = "Madrid"
        };

        var options = new MapLaunchOptions { Name = "Plaza de España 西班牙广场" };

        try
        {
            await placemark.OpenMapsAsync(options);
            return "OK";
        }
        catch (Exception ex)
        {
            return $"没有可打开的地图应用程序或无法定位地标, 消息: {ex.Message}";
        }
    }



    /// <summary>
    /// 添加导航
    /// <para></para>
    /// 打开地图时, 可以计算从设备的当前位置到指定位置的路由。 将 MapLaunchOptions 类型传递给 Map.OpenAsync 方法, 指定导航模式。 以下示例打开地图应用并指定驾驶导航模式：
    /// </summary>
    /// <returns></returns>
    public async Task<string> DriveToPlazaDeEspana()
    {
        var location = new Location(40.3989384, -3.6907709);
        var options = new MapLaunchOptions
        {
            Name = "Plaza de España 西班牙广场",
            NavigationMode = NavigationMode.Driving
        };

        try
        {
            await Map.Default.OpenAsync(location, options);
            return "OK";
        }
        catch (Exception ex)
        {
            return $"没有可打开的地图应用程序或无法定位地标, 消息: {ex.Message}";
        }
    }

    /// <summary>
    /// 捕获屏幕快照
    /// </summary>
    /// <returns></returns>
    public async Task<string> TakeScreenshotAsync()
    {
        if (Screenshot.Default.IsCaptureSupported)
        {
            IScreenshotResult screen = await Screenshot.Default.CaptureAsync();

            Stream stream = await screen.OpenReadAsync();

            //var imageSource= ImageSource.FromStream(() => stream);
            string localFilePath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.png");
            using FileStream localFileStream = File.OpenWrite(localFilePath);

            await stream.CopyToAsync(localFileStream);
            return localFilePath;
        }

        return null;
    }

#if WINDOWS
    public List<string> GetPortlist()
    {
        return SerialPort.GetPortNames().ToList();
    }
#elif ANDROID || IOS || MACCATALYST
    public List<string>? GetPortlist()
    {
        if (OperatingSystem.IsIOS() || OperatingSystem.IsMacCatalyst())
        {
            return null;
        }
        else if (OperatingSystem.IsAndroid())
        {
            return null;
        }
        else
        {
            return null;
        }

    }
#else
    public List<string>? GetPortlist()
    {
        if (OperatingSystem.IsWindows())
        {
            return SerialPort.GetPortNames().ToList();
        }
        else
        {
            return null;
        }
    }
#endif
    public string CacheDirectory() => FileSystem.CacheDirectory;
    public string AppDataDirectory() => FileSystem.AppDataDirectory;

    public Task<string> Print()
    {
        return Task.FromResult("未实现");
    }

#if ANDROID
    public async Task<string> CheckPermissionsNFC()
    {
        var status = await Permissions.CheckStatusAsync<NfcPermissions>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<NfcPermissions>();
        }

        return status.ToString();
    }
#else
    public Task<string> CheckPermissionsNFC()
    {
        return Task.FromResult("无需授权");
    }
#endif

    /// <summary>
    /// NFC
    /// </summary>
    public Action<bool> OpenNFC = Nfc_OpenNFC;

    public static void Nfc_OpenNFC(bool obj)
    {
        Application.Current?.Dispatcher.Dispatch(async () =>
        {
            //await Application.Current.MainPage.Navigation.PopToRootAsync();
            //await Application.Current.MainPage.Navigation.PushAsync(page); 
            await Application.Current.MainPage!.Navigation.PushModalAsync(Nfcs);
        });
    }
    public static ContentPage? Nfcs { get; set; }

    public Task<string> ReadNFC()
    {
        if (OpenNFC != null)
        {
            OpenNFC(true);
            return Task.FromResult("OK");
        }
        else
        {
            return Task.FromResult("NO NFC");
        }
    }

    public Task<string> ExtDSP()
    {
        return Task.FromResult("未实现");
    }

    public async Task<string> PickFile()
    {
        try
        {
            PickOptions options = new PickOptions();
            var res = await PickAndShow(options);
            return res!.FullPath;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }


    /// <summary>
    /// 从设备选取文件
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public async Task<FileResult?> PickAndShow(PickOptions options, bool test = false)
    {

        var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.my.comic.extension" } }, // UTType values
                    { DevicePlatform.Android, new[] { "application/comics" } }, // MIME type
                    { DevicePlatform.WinUI, new[] { ".cbr", ".cbz" } }, // file extension
                    { DevicePlatform.Tizen, new[] { "*/*" } },
                    { DevicePlatform.macOS, new[] { "cbr", "cbz" } }, // UTType values
                });

        if (test) options = new()
        {
            PickerTitle = "Please select a comic file",
            FileTypes = customFileType,
        };


        var result = await FilePicker.Default.PickAsync(options);
        if (result != null)
        {
            if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
            {
                using var stream = await result.OpenReadAsync();
                var image = ImageSource.FromStream(() => stream);
            }
            return result;
        }
        else
        {
            throw new FileNotFoundException("没选择任何文件");
        }

    }

    public void SetTagDeviceName(BleTagDevice ble)
    {
        MyBleTester.TagDevice = ble;

        if (!isInit)
        {
            MyBleTester.OnMessage += OnMessage;
            MyBleTester.OnDataReceived += OnDataReceived;
            MyBleTester.OnStateConnect += OnStateConnect;
            isInit = true;
        }
    }

    bool isInit = false;

    public async Task<List<BleDevice>?> StartScanAsync() => await MyBleTester.StartScanAsync();

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

    public async Task Alert(string title, string message, string cancel)
    {
        //Application.Current?.Dispatcher.Dispatch(async () =>
        //{
        await Application.Current!.MainPage!.DisplayAlert(title, message, cancel);
        //});
        //return Task.CompletedTask;
    }

#nullable disable

#if WINDOWS
    /// <summary>
    /// Gets the <see cref="WebView2Control"/> instance that was initialized.
    /// </summary>
    public static WebView2Control WebView { get; set; }
#elif ANDROID
    /// <summary>
    /// Gets the <see cref="AWebView"/> instance that was initialized.
    /// </summary>
    public static AWebView WebView { get; set; }
#elif MACCATALYST || IOS
    /// <summary>
    /// Gets the <see cref="WKWebView"/> instance that was initialized.
    /// the default values to allow further configuring additional options.
    /// </summary>
    public static WKWebView WebView { get; set; }
#elif TIZEN
		/// <summary>
		/// Gets the <see cref="TWebView"/> instance that was initialized.
		/// </summary>
		public static TWebView WebView { get; set; }
#endif
#nullable enable



    public void LoadUrl(string? url)
    {
        if (WebView == null) return;
#if WINDOWS
        url ??= "https://0.0.0.0/";
        WebView.CoreWebView2.Navigate(url);
#elif ANDROID
        url ??= "https://0.0.0.0/";
        WebView.LoadUrl(url);
#elif MACCATALYST || IOS
        url ??= "app://0.0.0.0/";
        WebView.LoadRequest(new NSUrlRequest(new NSUrl(url)));
#elif TIZEN
#endif
    }

    public async Task ExecuteScriptAsync(string js = "alert('hello from WebView JS')")
    {
        if (WebView == null) return;
#if WINDOWS 
        await WebView.ExecuteScriptAsync(js);
#elif ANDROID
        WebView.EvaluateJavaScript(new EvaluateJavaScriptAsyncRequest(js));
#elif MACCATALYST || IOS
        await WebView.EvaluateJavaScriptAsync(js);
#elif TIZEN
#endif
    }

    public async Task<(string message, object callback)> CallNativeFeatures(EnumNativeFeatures features, object[]? args, bool? on)
    {
        switch (features)
        {
            case EnumNativeFeatures.Flashlight:
                var res = await SetFlashlight(on: on ?? false);
                return (res, new object());
            case EnumNativeFeatures.SensorSpeed:
                break;
            case EnumNativeFeatures.Accelerometer:
                break;
            case EnumNativeFeatures.Barometer:
                break;
            case EnumNativeFeatures.Compass:
                break;
            case EnumNativeFeatures.Shake:
                break;
            case EnumNativeFeatures.Gyroscope:
                break;
            case EnumNativeFeatures.Magnetometer:
                break;
            case EnumNativeFeatures.Orientation:
                break;
            default:
                break;
        }
        return ("未实现", new object());
    }

    public async Task<string> SetFlashlight(bool on)
    {

        try
        {
            if (on)
                await Flashlight.Default.TurnOnAsync();
            else
                await Flashlight.Default.TurnOffAsync();
        }
        catch (FeatureNotSupportedException ex)
        {
            // Handle not supported on device exception
            return ex.Message;
        }
        catch (PermissionException ex)
        {
            // Handle permission exception
            return ex.Message;
        }
        catch (Exception ex)
        {
            // Unable to turn on/off flashlight
            return ex.Message;
        }
        return "OK";
    }

}
