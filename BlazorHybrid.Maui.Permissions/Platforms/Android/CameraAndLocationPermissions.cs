using Android.Webkit;

namespace BlazorHybrid.Maui.Shared; 

public class MauiWebChromeClient : WebChromeClient
{
    public override void OnPermissionRequest(PermissionRequest? request)
    {
        request?.Grant(request.GetResources());
    }
}


