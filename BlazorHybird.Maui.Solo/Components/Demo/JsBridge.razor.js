export async function GetMacAdress() {
    //var bridge = chrome.webview.hostObject.bridge;
    var result = await bridge.Func("测试");
    alert(result);
    return result;
}