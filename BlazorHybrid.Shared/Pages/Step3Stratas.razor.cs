// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Diagnostics.CodeAnalysis;

namespace BlazorHybrid.Shared.Pages;

public partial class Step3Stratas
{
    List<string?> PhotoList { get; set; } = new List<string?>();

    bool IsTakingPhoto { get; set; } = false;

    bool ShowImageList { get; set; } = true;

    Geolocations? Geolocations { get; set; }

    private Geolocationitem Model { get; set; } = new Geolocationitem();

    protected Modal? ExtraLargeModal { get; set; }

    int[]? PageItemsSource { get; set; }  

    protected override void OnInitialized()
    {
        //如果是手机浏览或者浏览器宽度小于800,一页显示一条记录
        if (States.IsMobile || States.Width < 800)
        {
            PageItemsSource = new int[] { 1, 20, 50 };
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Delay(500);
            await GetPhotoAsync();
        }
    }

    private RenderFragment RenderTableToolbarBefore() => builder =>
    {
        builder.OpenComponent<TableToolbarButton<SysLog>>(0);
        builder.AddAttribute(1, nameof(TableToolbarButton<SysLog>.Text), "返回");
        builder.AddAttribute(3, nameof(TableToolbarButton<SysLog>.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, e =>
        {
            States.Steps -= 1;
            Changed.InvokeAsync();
        }));
        builder.CloseComponent();

        builder.OpenComponent<TableToolbarButton<SysLog>>(10);
        builder.AddAttribute(11, nameof(TableToolbarButton<SysLog>.Text), "拍照");
        builder.AddAttribute(12, nameof(TableToolbarButton<SysLog>.Icon), "fa-solid fas fa-camera");
        builder.AddAttribute(13, nameof(TableToolbarButton<SysLog>.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, e =>
        {
            _ = TakePhoto();
        }));
        builder.CloseComponent();
    };

    private async Task GetPhotoAsync()
    {
        if (PhotoList.Count == 0)
        {
            //await ToastService.Information("载入图片中", "请稍候...");

            var res = await DataService.GetPhotosAsync();

            if (res != null && res.Any())
            {
                PhotoList.AddRange(res);
                //await ToastService.Information("提示", "图片载入完成");
            }
            else
            {
                //await ToastService.Information("提示", "无相关图片");
            }
        }

        ShowImageList = true;
        StateHasChanged();

    }

    private async Task TakePhoto()
    {
        if (OperatingSystem.IsMacCatalyst() || (DataService.SysInfo?.ForceNativeFunction ?? false))
        {
            await TakePhotoNative();
        }
        else
        {
            IsTakingPhoto = !IsTakingPhoto;
            if (IsTakingPhoto)
            {
                ShowImageList = false;
                await ExtraLargeModal!.Show();
                await GetLocation();
            }
        }

    }

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
            await GetLocation();
            var res = await DataService.SavePhotoAsync(Model, base64);
            if (res != null) PhotoList = res;
            await ShowBottomMessage($"保存照片成功. ({Model.Longitude},{Model.Latitude})");
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
                var res = await DataService.SavePhotoAsync(Model, url);
                if (res != null) PhotoList = res;
                await ShowBottomMessage($"保存照片成功. ({Model.Longitude},{Model.Latitude})");
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

    private Task<SysLog> AddAsync(SysLog item)
    {
        item.Operator = item.Operator ?? States.User!.UserID;
        return Task.FromResult(item);
    }

    private Task<SysLog> SaveAsync(SysLog item, ItemChangedType changedType)
    {
        if (changedType == ItemChangedType.Update)
        {
            //item.Revised = States.User!.UserID;
        }
        return Task.FromResult(item);
    }

    private Task AfterSaveAsync()
    {
        return Task.CompletedTask;
    }

    private async Task GetLocation()
    {
        if (Tools.IsMaui() || (DataService.SysInfo?.ForceNativeFunction ?? false))
        {
            var res = await Tools.GetCurrentLocation();
            if (res.latitude != null)
            {
                Model.Longitude = Convert.ToDecimal(res.longitude);
                Model.Latitude = Convert.ToDecimal(res.latitude);
                StateHasChanged();
            }
        }
        else
        {
            if (Geolocations != null)
            {
                await Geolocations.GetLocation();
            }
        }
    }

    private Task OnResult(Geolocationitem geolocations)
    {
        Model = geolocations;
        StateHasChanged();
        return Task.CompletedTask;
        //await ToastService.Success("完成", $"获取定位成功. ({geolocations.Longitude},{geolocations.Latitude})");
    } 

}


