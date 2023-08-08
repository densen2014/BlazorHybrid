// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BootstrapBlazor.Components;
using BootstrapBlazor.WebAPI.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace BlazorHybrid.Shared.Pages;

public partial class TakePhoto
{
    [Inject, NotNull] ToastService? ToastService { get; set; }
    [Inject, NotNull] protected IStorage? Storage { get; set; }

    [Parameter] public EventCallback<string> Capture { get; set; }

    public bool IsInit { get; set; }

    //public bool Cam1080 { get; set; } = true;

    [NotNull]
    private Cams SelectedEnumItem = Cams.P1080;

    enum Cams
    {
        [Description("标清")]
        P480,
        [Description("高清")]
        P1080,
        [Description("超清")]
        P2048,
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //Cam1080 = await Storage.GetValue("Cam1080", "true") == "true";
            Enum.TryParse(await Storage.GetValue("Cams", "P1080"), out SelectedEnumItem);
            IsInit = true;
            StateHasChanged();
        }
    }

    private async Task OnValueChanged(bool val)
    {
        await Storage.SetValue("Cam1080", val ? "true" : "false");
    }
    private async Task OnSelectedChanged(IEnumerable<SelectedItem> values, Cams val)
    {
        await Storage.SetValue("Cams", val.ToString());
        //StateHasChanged();
    }

    private async Task OnCapture(string url)
    {
        try
        {
            await Capture.InvokeAsync(url);
            //await camera!.Dispose ();
        }
        catch
        {
            await ToastService.Error("错误", "保存照片失败");
        }
    }

}


