// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorHybrid;
using BlazorHybrid.Shared;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using System.IO.Compression;

AppState _appState = new();
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes =
    ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "image/svg+xml" });
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(a =>
{
    a.DetailedErrors = true;
    a.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(2);
    a.MaxBufferedUnacknowledgedRenderBatches = 20;
    a.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
}).AddHubOptions(o =>
{
    o.EnableDetailedErrors = true;
    //单个传入集线器消息的最大大小。默认 32 KB	
    o.MaximumReceiveMessageSize = null;
    //可为客户端上载流缓冲的最大项数。 如果达到此限制，则会阻止处理调用，直到服务器处理流项。
    o.StreamBufferCapacity = 20;
});
builder.Services.AddSharedExtensions(); 
builder.Services.AddSingleton(_appState);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseResponseCompression();

app.UseHttpsRedirection();
var provider = new FileExtensionContentTypeProvider { Mappings = { [".properties"] = "application/octet-stream" , [".mp4"] = "application/octet-stream" } };

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});
app.UseStaticFiles();
//app.UseDefaultFiles();
app.UseDirectoryBrowser(new DirectoryBrowserOptions()
{
    RequestPath = new PathString("/pic")
});
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
