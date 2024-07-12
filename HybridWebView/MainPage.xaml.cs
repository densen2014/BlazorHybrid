// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using WebViewNativeApi;

namespace HybridWebView;

public partial class MainPage : TabbedPage
{
    private NativeBridge? api;

    public MainPage()
    {
        InitializeComponent();
        WebView? wvBrowser = FindByName("webView") as WebView;
        api = new NativeBridge(wvBrowser);
        api.AddTarget("dialogs", new NativeApi());

        ////if IOS || MacOS
        //webView.Source = new HtmlWebViewSource
        //{
        //    Html = """
        //    <!DOCTYPE html>
        //    <html>
        //    <head>
        //    </head>
        //    <body>

        //        <div style='display: flex; flex-direction: column; justify-content: center; align-items: center; width: 100%'>
        //            <h2 style='font-family: script'><i>Fancy Web Title</i></h2>
        //            <div id='webtext' style='font-family: script'><b>This web text will change when you push the native button.</b></div>
        //            <button style='height:48px; margin-left: 15px; margin-right: 15px; width: 128px; background: lightblue' id='hereBtn' onclick='getConfig()'>getConfig</button>
        //            <button style='height:48px; margin-left: 15px; margin-right: 15px; width: 128px; background: lightblue' id='hereBtn' onclick='openDialog()'>openDialog</button>
        //            <button style='height:48px; margin-left: 15px; margin-right: 15px; width: 128px; background: lightblue' id='hereBtn' onclick='saveFile()'>saveFile</button>
        //            <button style='height:48px; margin-left: 15px; margin-right: 15px; width: 128px; background: lightblue' id='hereBtn' onclick='printTicket("123")'>printTicket</button>
        //        </div>

        //        <script>

        //            function getConfig() {
        //                var promise = window.dialogs.get_config();
        //                runCommand(promise);
        //            }
        //            function openDialog() {
        //                var promise = window.dialogs.open_file_dialog();
        //                runCommand(promise);
        //            }

        //            function saveFile() {
        //                var promise = window.dialogs.save_file("test file", "test.txt");
        //                runCommand(promise);
        //            }

        //            function printTicket(data) {
        //                var promise = window.dialogs.print_ticket(data);
        //                runCommand(promise);
        //            }

        //            function openDrawer() {
        //                var promise = window.dialogs.open_drawer();
        //                runCommand(promise);
        //            }

        //            function getDrawerStatus() {
        //                var promise = window.dialogs.get_drawer_status();
        //                runCommand(promise);
        //            }

        //            function showPriceText(data) {
        //                var promise = window.dialogs.showPriceText(data);
        //                runCommand(promise);
        //            }

        //            function runCommand(promise) {
        //                promise.then((fileData) => {
        //                    let text = atob(fileData);
        //                    var el = document.getElementById('webtext');
        //                    el.innerHTML = text;
        //                    console.log(text);
        //                });
        //            }
        //        </script>
        //    </body>
        //    </html>
        //    """
        //};

    }

}

