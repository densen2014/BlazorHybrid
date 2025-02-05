// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

namespace BlazorHybird.Maui.Solo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "BlazorHybird.Maui.Solo" };
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
}
