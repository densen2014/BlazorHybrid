using BlazorHybrid.Maui.Shared;
using Microsoft.Maui.Controls;
using Application = Microsoft.Maui.Controls.Application;

namespace BlazorHybrid.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        //MainPage = new AppModelPage();
        MainPage = new MainPage();
        //MainPage = new NavigationPage(new MainPage());

       
    }


    public static void HandleAppActions(AppAction appAction)
    {
        Current.Dispatcher.Dispatch(async () =>
        {
            ContentPage page = appAction.Id switch
            {
                "battery_info" => new SensorsPage(),
                //"app_info" => new AppModelPage(),
                "nfc" => new NfcPage(),
                _ => default(MainPage)
            };

            if (page != null)
            {
                await Current.MainPage.Navigation.PushModalAsync(page);
                //await Current.MainPage.Navigation.PopToRootAsync();
                //await Current.MainPage.Navigation.PushAsync(page); 
            }
        });
    }
}
