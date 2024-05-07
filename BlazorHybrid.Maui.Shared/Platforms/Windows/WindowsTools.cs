// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT.Interop;

namespace BlazorMaui.Platforms.Windows;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class WindowsTools
{
    public static AppWindow? AppWindow;

    public static void SetTitle(object currentWindow, string title)
    {
        IntPtr _windowHandle = WindowNative.GetWindowHandle(currentWindow);
        var windowId = Win32Interop.GetWindowIdFromWindow(_windowHandle);

        AppWindow = AppWindow.GetFromWindowId(windowId);
        SetTitle(title);
    }

    public static void SetTitle(string title)
    {
        if (AppWindow != null)
            AppWindow!.Title = title;
    }

}
