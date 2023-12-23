// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorHybrid.Core;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using static BootstrapBlazor.Components.UploadToBase64;

namespace BlazorHybrid.Shared.Pages;

public partial class Components
{
    /// <summary>
    /// 显示扫码界面
    /// </summary>
    bool ShowScanBarcode { get; set; } = false;
    bool ShowWxQrCode { get; set; } = false;
    bool FlashlightOn { get; set; } = false;
    protected Modal? ExtraModal { get; set; }

    List<ResCustomersDto>? DeskList { get; set; }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            DeskList = new List<ResCustomersDto>()
            {
                new ResCustomersDto("定位",async ()=>await ShowBottomMessage("定位:" + (await Tools.GetCurrentLocation()).message)),
                new ResCustomersDto("定位缓存",async() =>await ShowBottomMessage("定位缓存:" + (await Tools.GetCachedLocation()).message)),
                new ResCustomersDto("图库",async() => await ShowPhoto()),
                new ResCustomersDto("拍照(原生)",async() => await TakePhotoNative()),
                new ResCustomersDto("拍照(H5)",async() => {await TakePhotoH5();StateHasChanged(); }),
                new ResCustomersDto("拍照(Blazor)",async() => {await TakePhotoBlazor();StateHasChanged(); }),
                new ResCustomersDto("扫码",async () => {ShowScanBarcode=!ShowScanBarcode;await ExtraModal!.Show();}),
                new ResCustomersDto("导航",null,Tools.NavigateToMadrid),
                new ResCustomersDto("文件",async () => await ShowBottomMessage("选择文件:" + await Tools.PickFile())),
                new ResCustomersDto("蓝牙",async() => {await 蓝牙();StateHasChanged(); }),
                new ResCustomersDto("NFC",() => NavigationManager.NavigateTo("NfcBase")),
                new ResCustomersDto("NFC(xaml)",null,Tools.ReadNFC),
                new ResCustomersDto("原生消息框",async () => await Tools.Alert("Alert", $"hello from native UI", "OK")),
                new ResCustomersDto("外部网站(NavM)",async() =>{ await APP1();StateHasChanged(); }),
                new ResCustomersDto("重载主页",()=>Tools.LoadUrl(null)),
                new ResCustomersDto("app1.es",()=>Tools.LoadUrl("https://blazor.app1.es")),
                new ResCustomersDto("bing",()=>Tools.LoadUrl("https://www.bing.com")),
                new ResCustomersDto("执行JS",async()=> await Tools.ExecuteScriptAsync()),
                new ResCustomersDto("设置",null,Tools.ShowSettingsUI),
                new ResCustomersDto("定位权限",null,Tools.CheckPermissionsLocation),
                new ResCustomersDto("拍照权限",null, Tools.CheckPermissionsCamera),
                new ResCustomersDto("蓝牙权限",null,Tools.CheckPermissionsBluetooth),
                new ResCustomersDto("NFC权限",null,Tools.CheckPermissionsNFC),
                new ResCustomersDto("手电筒",async() => {await FlashlightToggle(); }),
                new ResCustomersDto("微信扫码",async() => {ShowWxQrCode=!ShowWxQrCode;await ExtraModal!.Show();}),
                new ResCustomersDto("返回",async ()=>await BackToHome()),
                new ResCustomersDto("PDF阅读器",()=>NavigationManager.NavigateTo("pdfReaders")),
                new ResCustomersDto("思维导图",()=>NavigationManager.NavigateTo("MindMaps")),
                new ResCustomersDto("语音合成/识别",()=>NavigationManager.NavigateTo("Speechs")),
                new ResCustomersDto("文件",()=>NavigationManager.NavigateTo("Files")),
                new ResCustomersDto("上传文件",()=>NavigationManager.NavigateTo("FileUpload")),
                new ResCustomersDto("APP文件夹",()=>NavigationManager.NavigateTo("AppFiles")),
                new ResCustomersDto("JsBridge",()=>NavigationManager.NavigateTo("JsBridge")),
                new ResCustomersDto("串口",()=>NavigationManager.NavigateTo("WebSerials")),
                new ResCustomersDto("web蓝牙",()=>Tools.LoadUrl("https://blazor.app1.es/Bluetooth")),
                new ResCustomersDto("视频播放器",()=>Tools.LoadUrl("https://blazor.app1.es/videoPlayers")),
                new ResCustomersDto("OCR",()=>Tools.LoadUrl("https://blazor.app1.es/ocr")),
                new ResCustomersDto("翻译",()=>Tools.LoadUrl("https://blazor.app1.es/Translate")),
                new ResCustomersDto("OpenAI",()=>Tools.LoadUrl("https://blazor.app1.es/OpenAI")),
           };

            StateHasChanged();
        }
    }

    private Task 蓝牙()
    {
        NavigationManager.NavigateTo("/bluetooth");
        return Task.CompletedTask;
    }

    private async Task FlashlightToggle()
    {
        FlashlightOn = !FlashlightOn;
        var res = await Tools.CallNativeFeatures(EnumNativeFeatures.Flashlight, null, FlashlightOn);
        await ShowBottomMessage(res.message);
    }

    private Task APP1()
    {
        NavigationManager.NavigateTo("https://blazor.app1.es/", true);
        return Task.CompletedTask;
    }

    private async Task OnListViewItemClick(ResCustomersDto item)
    {
        item.ClickAction?.Invoke();
        if (item.ClickFunc != null)
        {
            item.Description = await item.ClickFunc.Invoke();
        }
    }

    private Task OnCloseDemoAsync()
    {
        ShowImageList = false;
        IsTakingPhotoH5 = false;
        ShowScanBarcode = false;
        ShowWxQrCode = false;
        StateHasChanged();
        return Task.CompletedTask;
    }

    public class ResCustomersDto
    {
        public ResCustomersDto(string Name, Action? ClickAction)
        {
            this.ClickAction = ClickAction;
            this.Name = Name;
        }

        public ResCustomersDto(string CustomerID, Action? ClickAction, Func<Task<string>>? ClickFunc)
        {
            this.ClickAction = ClickAction;
            this.ClickFunc = ClickFunc;
            this.Name = CustomerID;
        }

        public string Name { get; set; }
        public string? Description { get; set; }

        public Action? ClickAction { get; set; }
        public Func<Task<string>>? ClickFunc { get; set; }
    }

    #region  images 

    List<string?>? PhotoList { get; set; }

    private async Task ShowPhoto()
    {
        ShowImageList = !ShowImageList;
        if (ShowImageList)
        {
            if (PhotoList == null)
            {
                //await ToastService.Information("载入图片中","请稍候...");
                PhotoList = await DataService.GetPhotosAsync();
                if (PhotoList != null && PhotoList.Any())
                {
                    //await ToastService.Information("提示", "图片载入完成");
                }
                else
                {
                    await ShowBottomMessage("无相关图片");
                    //await ToastService.Information("提示", "无相关图片");
                }
            }
        }
        StateHasChanged();
    }

    bool IsTakingPhotoH5 { get; set; } = false;
    private Task TakePhotoH5()
    {
        IsTakingPhotoH5 = !IsTakingPhotoH5;
        return Task.CompletedTask;
    }

    private async Task OnChanged(List<ImageFile> dataUrlList)
    {
        IsBusy = true;
        StateHasChanged();
        if (dataUrlList.Any())
        {
            var list = dataUrlList.Where(a => a.DataUrl != null).Select(a => a.DataUrl ?? "").ToList();
            var res = await DataService.SaveProjectPhotosAsync(list);
            if (res > 0)
            {
                await ToastService.Success("完成", $"已保存{res}张照片.");
                PhotoList = await DataService.GetPhotosAsync();
            }
            else
            {
                await ToastService.Error("错误", "保存照片失败");
            }
        }
        IsBusy = false;
        StateHasChanged();
    }

    private async Task OnResult(string message)
    {
        await ToastService.Success("扫码结果", message);
    }

    private async Task OnError(string message)
    {
        await ToastService.Error("保存照片出错", message);
    }

    protected Modal? ExtraLargeModal { get; set; }
    bool IsTakingPhoto { get; set; } = false;

    bool ShowImageList { get; set; }
    private async Task TakePhotoNative()
    {
        IsBusy = true;
        ShowImageList = false;
        StateHasChanged();

        var fileName = await Tools.TakePhoto();
        if (File.Exists(fileName))
        {
            var bytes = File.ReadAllBytes(fileName);
            var b64String = Convert.ToBase64String(bytes);
            var contentType = Utility.GetMimeType(Path.GetExtension(fileName));
            var base64 = $"data:{(contentType ?? "image/jpeg")};base64," + b64String;
            var res = await DataService.SavePhotoAsync(null, base64);
            if (res != null) PhotoList = res;
            await ShowBottomMessage($"保存照片成功.");
            //StateHasChanged();
        }
        else
        {
            await ToastService.Warning("提醒", "照片无效");
        }

        ShowImageList = true;
        IsBusy = false;
        StateHasChanged();

    }
    private async Task TakePhotoBlazor()
    {
        IsTakingPhoto = !IsTakingPhoto;
        if (IsTakingPhoto)
        {
            ShowImageList = false;
            await ExtraLargeModal!.Show();
        }
    }

    private Task OnCloseAsync()
    {
        IsTakingPhoto = false;
        ShowImageList = !IsTakingPhoto;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task OnCapture(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            await ToastService.Warning("提醒", "照片无效");
        }
        else
        {
            try
            {
                IsTakingPhoto = false;
                ShowImageList = !IsTakingPhoto;
                await ExtraLargeModal!.Close();
                var res = await DataService.SavePhotoAsync(null, url);
                if (res != null) PhotoList = res;
                await ShowBottomMessage($"保存照片成功.");
            }
            catch (Exception)
            {
                IsTakingPhoto = false;
                ShowImageList = !IsTakingPhoto;
                await ExtraLargeModal!.Close();
                await ToastService.Error("错误", "保存照片失败");
            }
        }
        IsBusy = false;
        IsTakingPhoto = false;
        ShowImageList = !IsTakingPhoto;
        StateHasChanged();
    }
    #endregion
}


