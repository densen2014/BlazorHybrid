// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

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

    List<BtnFun>? DeskList { get; set; }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            DeskList = new List<BtnFun>()
            {
                new BtnFun("定位",async ()=>await ShowBottomMessage("定位:" + (await Tools.GetCurrentLocation()).message)),
                new BtnFun("定位缓存",async() =>await ShowBottomMessage("定位缓存:" + (await Tools.GetCachedLocation()).message)),
                new BtnFun("图库",async() => await ShowPhoto()),
                new BtnFun("拍照(原生)",async() => await TakePhotoNative()),
                new BtnFun("拍照(H5)",async() => {await TakePhotoH5();StateHasChanged(); }),
                new BtnFun("拍照(Blazor)",async() => {await TakePhotoBlazor();StateHasChanged(); }),
                new BtnFun("扫码",() => {ShowScanBarcode=!ShowScanBarcode;StateHasChanged(); }),
                new BtnFun("导航",null,Tools.NavigateToMadrid),
                new BtnFun("文件",async () => await ShowBottomMessage("选择文件:" + await Tools.PickFile())),
                new BtnFun("蓝牙",async() => {await 蓝牙();StateHasChanged(); }),
                new BtnFun("NFC",() => NavigationManager.NavigateTo("NfcBase")),
                new BtnFun("NFC(xaml)",null,Tools.ReadNFC),
                new BtnFun("原生消息框",async () => await Tools.Alert("Alert", $"hello from native UI", "OK")),
                new BtnFun("外部网站(NavM)",async() =>{ await APP1();StateHasChanged(); }),
                new BtnFun("重载主页",()=>Tools.LoadUrl(null)),
                new BtnFun("app1.es",()=>Tools.LoadUrl("https://blazor.app1.es")),
                new BtnFun("bing",()=>Tools.LoadUrl("https://www.bing.com")),
                new BtnFun("执行JS",async()=> await Tools.ExecuteScriptAsync()),
                new BtnFun("设置",null,Tools.ShowSettingsUI),
                new BtnFun("定位权限",null,Tools.CheckPermissionsLocation),
                new BtnFun("拍照权限",null, Tools.CheckPermissionsCamera),
                new BtnFun("蓝牙权限",null,Tools.CheckPermissionsBluetooth),
                new BtnFun("NFC权限",null,Tools.CheckPermissionsNFC),
                new BtnFun("返回",async ()=>await BackToHome()),
                new BtnFun("PDF阅读器",()=>NavigationManager.NavigateTo("pdfReaders")),
                new BtnFun("思维导图",()=>NavigationManager.NavigateTo("MindMaps")),
                new BtnFun("语音合成/识别",()=>NavigationManager.NavigateTo("Speechs")),
                new BtnFun("文件",()=>NavigationManager.NavigateTo("Files")),
                new BtnFun("上传文件",()=>NavigationManager.NavigateTo("FileUpload")),
                new BtnFun("APP文件夹",()=>NavigationManager.NavigateTo("AppFiles")),
                new BtnFun("JsBridge",()=>NavigationManager.NavigateTo("JsBridge")),
                new BtnFun("串口",()=>NavigationManager.NavigateTo("WebSerials")),
                new BtnFun("web蓝牙",()=>Tools.LoadUrl("https://blazor.app1.es/Bluetooth")),
                new BtnFun("视频播放器",()=>Tools.LoadUrl("https://blazor.app1.es/videoPlayers")),
                new BtnFun("OCR",()=>Tools.LoadUrl("https://blazor.app1.es/ocr")),
                new BtnFun("翻译",()=>Tools.LoadUrl("https://blazor.app1.es/Translate")),
                new BtnFun("OpenAI",()=>Tools.LoadUrl("https://blazor.app1.es/OpenAI")),
           };

            StateHasChanged();
        }
    }

    private Task 蓝牙()
    {
        NavigationManager.NavigateTo("/bluetooth");
        return Task.CompletedTask;
    }

    private Task APP1()
    {
        NavigationManager.NavigateTo("https://blazor.app1.es/", true);
        return Task.CompletedTask;
    }

    private async Task OnListViewItemClick(BtnFun item)
    {
        item.ClickAction?.Invoke();
        if (item.ClickFunc != null)
        {
            item.Description = await item.ClickFunc.Invoke();
        }
    }

    public class BtnFun
    {
        public BtnFun(string Name, Action? ClickAction)
        {
            this.ClickAction = ClickAction;
            this.Name = Name;
        }

        public BtnFun(string CustomerID, Action? ClickAction, Func<Task<string>>? ClickFunc)
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


