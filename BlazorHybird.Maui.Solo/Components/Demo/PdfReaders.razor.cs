// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BootstrapBlazor.Components;

namespace BlazorHybrid.Components;

public sealed partial class PdfReaders
{

    PdfReader? pdfReader { get; set; }

    private string FileNameStream { get; set; } = "https://blazor.app1.es/samples/sample.pdf";

    private async Task Apply()
    {
        if (pdfReader != null) await pdfReader.Refresh();
    }

}
