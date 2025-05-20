// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorHybrid.Maui.Shared;

namespace bh006_NFC_tag;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        new InitBlazorWebView(blazorWebView);
    }
}
