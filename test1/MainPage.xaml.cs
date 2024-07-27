// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

namespace test1;
using WebViewNativeApi;

public partial class MainPage : TabbedPage
{
    private NativeBridge? api;

    public MainPage()
    {
        InitializeComponent();
        WebView? wvBrowser = FindByName("webView") as WebView;
        api = new NativeBridge(wvBrowser);
        api.AddTarget("dialogs", new NativeApi());

        #if false
        webView.Source = new HtmlWebViewSource
        {
            Html = """
            <!DOCTYPE html>
            <html lang="en" data-bs-theme='light'>

            <head>
                <meta charset="utf-8" />
                <meta http-equiv="content-type" content="text/html; charset=UTF-8" />
                <meta http-equiv="X-UA-Compatible" content="IE=edge">
                <meta name="viewport" content="width=device-width, initial-scale=1.0, minimum-scale=1.0, maximum-scale=1.0, user-scalable=no, viewport-fit=cover">
                <title>Native API</title>
            </head>
            <body>

                <div style='display: flex; flex-direction: column; justify-content: center; align-items: center; width: 100%'>
                    <h2 style='font-family: script'><i>Hybrid WebView</i></h2>
                    <div id='webtext' style='font-family: script'><b>Native API</b></div><br />
                    <button style='height:48px; margin-left: 15px; margin-right: 15px; width: 128px; background: lightblue' id='hereBtn' onclick='getConfig()'>getConfig</button>
                    <button style='height:48px; margin-left: 15px; margin-right: 15px; width: 128px; background: lightblue' id='hereBtn' onclick='openDialog()'>openDialog</button>
                    <button style='height:48px; margin-left: 15px; margin-right: 15px; width: 128px; background: lightblue' id='hereBtn' onclick='saveFile()'>saveFile</button>
                    <button style='height:48px; margin-left: 15px; margin-right: 15px; width: 128px; background: lightblue' id='hereBtn' onclick='connectAndPrint()'>connect and print</button>
                    <button style='height:48px; margin-left: 15px; margin-right: 15px; width: 128px; background: lightblue' id='hereBtn' onclick='connectPrinter()'>connectPrinter</button>
                    <button style='height:48px; margin-left: 15px; margin-right: 15px; width: 128px; background: lightblue' id='hereBtn' onclick='printTicket("123")'>printTicket</button>
                    <button style='height:48px; margin-left: 15px; margin-right: 15px; width: 128px; background: lightblue' id='hereBtn' onclick='isShell()'>isShell</button>
                </div>

                <script>
                    function uuidv4() {
                        return "10000000-1000-4000-8000-100000000000".replace(/[018]/g, c =>
                            (+c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> +c / 4).toString(16)
                        );
                    }

                    function getConfig() {
                        var promise = window.dialogs.get_config();
                        runCommand(promise);
                    }
                    function openDialog() {
                        var promise = window.dialogs.open_file_dialog();
                        runCommand(promise, true);
                    }

                    function saveFile() {
                        var promise = window.dialogs.save_file("test file", "test.txt");
                        runCommand(promise);
                    }

                    function connectPrinter() {
                        var promise = window.dialogs.connect_printer();
                        runCommand(promise);
                    }

                    function printTicket(data) {
                        var promise = window.dialogs.print_ticket(data);
                        runCommand(promise);
                    }

                    function connectAndPrint() {
                        connectPrinter();
                        printTicket('test test test');
                    }

                    function openDrawer() {
                        var promise = window.dialogs.open_drawer();
                        runCommand(promise);
                    }

                    function getDrawerStatus() {
                        var promise = window.dialogs.get_drawer_status();
                        runCommand(promise);
                    }

                    function showPriceText(data) {
                        var promise = window.dialogs.showPriceText(data);
                        runCommand(promise);
                    }

                    function runCommand(promise, isEncode = false) {
                        promise.then((fileData) => {
                            let text = isEncode ? atob(fileData) : (fileData.Result !== undefined ? fileData.Result : fileData);
                            var el = document.getElementById('webtext');
                            el.innerHTML = text;
                            console.log(text);
                        });
                    }
            
                    function isShell() {
                        var isShell = window.dialogs !== undefined;
                        var el = document.getElementById('webtext');
                        el.innerHTML = isShell?'BB+':'-';
                    }
            
                    var text = uuidv4();
                    console.log(uuidv4());
                    var el = document.getElementById('webtext');
                    el.innerHTML = text;
                </script>
            </body>
            </html>
            """
        };
#endif
    }
}

