// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using static BootstrapBlazor.Components.UploadToBase64;

namespace BlazorHybrid.Shared.Pages;

public partial class Step2Holes
{

    bool IsShow { get; set; } = false;

    List<string?>? PhotoList { get; set; }


    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {

        }
    }

    private async Task ClickRow(SysLog hole)
    {
        States.Width = await JS.InvokeAsync<int>("eval", "window.innerWidth");

        States.Steps = 3;
        await Changed.InvokeAsync();
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
        builder.AddAttribute(11, nameof(TableToolbarButton<SysLog>.Text), "平面图");
        builder.AddAttribute(12, nameof(TableToolbarButton<SysLog>.Icon), "fas fa-thumbtack");
        builder.AddAttribute(13, nameof(TableToolbarButton<SysLog>.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, e =>
        {
            _ = ShowPhoto();
        }));
        builder.CloseComponent();
    };

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
    private async Task ShowPhoto()
    {
        IsShow = !IsShow;
        if (IsShow)
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

    private async Task OnError(string message)
    {
        await ToastService.Error("保存照片出错", message);
    }



}


