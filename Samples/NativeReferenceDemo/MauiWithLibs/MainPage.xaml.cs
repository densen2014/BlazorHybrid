// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using System.Runtime.InteropServices; 

namespace MauiWithLibs;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
        var count1 = getClickCount();
        Label1 .Text = $"Hello, start with {count1}!";
    }

#if ANDROID
    [DllImport("libsAndroid")]
    public static extern int getClickCount();  

    [DllImport("libsAndroid")]
    public static extern int getClickCount2();
#elif MACCATALYST || IOS
    [DllImport("__Internal")]
    private static extern int getClickCount();
#else

    public int getClickCount()
    {
        count+1000;
        return count;
    }
#endif


    private void OnCounterClicked(object sender, EventArgs e)
    {
        count = getClickCount();

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}

