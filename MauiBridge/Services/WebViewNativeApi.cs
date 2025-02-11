// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WebViewNativeApi;

/// <summary>
/// NativeBridge 类, 用于在 .NET MAUI 应用程序中的 WebView 和本地代码之间进行通信, 主要负责在 WebView 和本地代码之间建立桥梁
/// </summary>
public class NativeBridge
{
    /// <summary>
    /// 默认的 URL scheme，用于识别本地调用
    /// </summary>
    private const string DEFAULT_SCHEME = "native://";

    /// <summary>
    /// JavaScript 代码，用于在 WebView 中创建一个代理对象，通过该对象可以调用本地方法
    /// </summary>
    private const string INTERFACE_JS = "window['createNativeBridgeProxy'] = " +
        "(name, methods, properties, scheme = '" + DEFAULT_SCHEME + "') =>" +
        "{" +
        "    let apiCalls = new Map();" +
        "" +
        "    function randomUUID() {" +
        "       return '10000000-1000-4000-8000-100000000000'.replace(/[018]/g, c =>" +
        "               (+c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> +c / 4).toString(16));" +
        "    }" +
        "" +
        "    function createRequest(target, success, reject, argumentsList) {" +
        "        let uuid = randomUUID();" +
        "        while(apiCalls.has(uuid)) { uuid = randomUUID(); };" +
        "        apiCalls.set(uuid, { 'success': success, 'reject': reject, 'arguments': argumentsList });" +
        "        location.href = scheme + name + '/' + target + '/' + uuid + '/';" +
        "    }" +
        "" +
        "    return new Proxy({" +
        "            getArguments : (token) => {" +
        "                return apiCalls.get(token).arguments;" +
        "            }," +
        "            returnValue : (token, value) => {" +
        "                let ret = value;" +
        "                try { ret = JSON.parse(ret); } catch(e) { };" +
        "                let callback = apiCalls.get(token).success;" +
        "                if (callback && typeof callback === 'function')" +
        "                    callback(ret);" +
        "                apiCalls.delete(token);" +
        "            }," +
        "            rejectCall : (token, error) => {" +
        "                let callback = apiCalls.get(token).reject;" +
        "                if (callback && typeof callback === 'function')" +
        "                    callback(error);" +
        "                apiCalls.delete(token);" +
        "            }" +
        "        }," +
        "        {" +
        "            get: (target, prop, receiver) => {" +
        "                if (methods.includes(prop)) {" +
        "                    return new Proxy(() => {}, {" +
        "                        apply: (target, thisArg, argumentsList) => {" +
        "                            return new Promise((success, reject) => {" +
        "                                    createRequest(prop, success, reject, argumentsList);" +
        "                                });" +
        "                        }" +
        "                    });" +
        "                }" +
        "                if (!properties.includes(prop)) {" +
        "                    return Reflect.get(target, prop, receiver);" +
        "                }" +
        "                return new Promise((success, reject) => {" +
        "                        createRequest(prop, success, reject, []);" +
        "                    });" +
        "            }," +
        "            set: (target, prop, value) => {" +
        "                return new Promise((success, reject) => {" +
        "                        createRequest(prop, success, reject, [value]);" +
        "                    });" +
        "            }" +
        "        });" +
        "};";

    /// <summary>
    /// WebView 控件的引用
    /// </summary>
    private readonly WebView? _webView = null;

    /// <summary>
    /// 用于存储本地对象的字典,存储目标对象及其名称和 scheme
    /// </summary>
    private readonly Dictionary<(string, string), object> _targets = [];

    /// <summary>
    /// 是否已经初始化
    /// </summary>
    private bool _isInit = false;

    /// <summary>
    /// 存储当前的查询信息
    /// </summary>
    private (string?, string?, string?, object?) _query = ("", "", "", null);

    /// <summary>
    /// 存储上一次导航的域名
    /// </summary>
    private string? lastDomain;

    /// <summary>
    /// 存储要注入的目标 JavaScript 代码
    /// </summary>
    public string? TargetJS;

    /// <summary>
    /// 构造函数，初始化 WebView 并注册导航事件
    /// </summary>
    /// <param name="wv"></param>
    public NativeBridge(WebView? wv)
    {
        if (wv != null)
        {
            _webView = wv;
            _webView.Navigated += OnWebViewInit;
            _webView.Navigating += OnWebViewNavigatin;
        }
    }

    /// <summary>
    /// 添加目标对象及其名称和 scheme
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    /// <param name="sheme"></param>
    public void AddTarget(string name, object obj, string sheme = DEFAULT_SCHEME)
    {
        if (obj == null)
        {
            return;
        }

        _targets.Add((name, sheme), obj);
        if (_isInit)
        {
            AddTargetToWebView(name, obj, sheme);
        }
    }

    /// <summary>
    /// WebView 初始化事件处理程序,在 WebView 导航完成后调用，注入 JavaScript 代码并初始化目标对象。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnWebViewInit(object? sender, WebNavigatedEventArgs e)
    {

        var currentDomain = new Uri(e.Url).Host;
        if (lastDomain != currentDomain)
        {
            _isInit = false;

            lastDomain = currentDomain;
        }
        else
        {
            var isInjected = await RunJS("window.dialogs !== undefined");
            if (isInjected == "false")
            {
                _isInit = false;
            }
        }

        if (!_isInit)
        {
            _ = await RunJS(INTERFACE_JS);
            if (TargetJS != null)
            {
                _ = await RunJS(TargetJS);
            }

            foreach (KeyValuePair<(string, string), object> entry in _targets)
            {
                AddTargetToWebView(entry.Key.Item1, entry.Value, entry.Key.Item2);
            }

            _isInit = true;
        }
    }

    /// <summary>
    /// WebView 导航事件处理程序,在 WebView 导航时调用，根据 URL 判断是否调用本地方法。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnWebViewNavigatin(object? sender, WebNavigatingEventArgs e)
    {
        if (!_isInit)
        {
            return;
        }

        foreach (KeyValuePair<(string, string), object> entry in _targets)
        {
            var startStr = entry.Key.Item2 + entry.Key.Item1;
            if (!e.Url.StartsWith(startStr))
            {
                continue;
            }

            var request = e.Url[(e.Url.IndexOf(startStr) + startStr.Length)..].ToLower();
            request = request.Trim(['/', '\\']);
            var requestArgs = request.Split('/');
            if (requestArgs.Length < 2)
            {
                return;
            }

            e.Cancel = true;

            var prop = requestArgs[0];
            var token = requestArgs[1];

            Type type = entry.Value.GetType();
            if (type.GetMember(prop) == null)
            {
                RunJS("window." + entry.Key.Item1 + ".rejectCall('" + token + "', 'Member not found!');");
                return;
            }

            _query = (entry.Key.Item1, token, prop, entry.Value);
            Task.Run(() =>
            {
                RunCommand(_query.Item1, _query.Item2, _query.Item3, _query.Item4);
                _query = ("", "", "", null);
            });
            return;
        }
    }


    /// <summary>
    /// 将目标对象的方法和属性注入到 WebView 中。
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    /// <param name="sheme"></param>
    private void AddTargetToWebView(string name, object obj, string sheme)
    {
        var type = obj.GetType();
        var methods = new List<string>();
        var properties = new List<string>();
        foreach (MethodInfo method in type.GetMethods())
        {
            methods.Add(method.Name);
        }

        foreach (PropertyInfo p in type.GetProperties())
        {
            properties.Add(p.Name);
        }

        RunJS("window." + name + " = window.createNativeBridgeProxy('" + name + "', " + JsonSerializer.Serialize(methods) + ", " +
            JsonSerializer.Serialize(properties) + ", '" + sheme + "');");
    }

    /// <summary>
    /// 判断方法是否为异步方法
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    private static bool IsAsyncMethod(MethodInfo method)
    {
        var attType = typeof(AsyncStateMachineAttribute);
        var attrib = (AsyncStateMachineAttribute?)method.GetCustomAttribute(attType);
        return (attrib != null);
    }

    /// <summary>
    /// 调用本地方法,执行本地方法或属性访问，并将结果返回给 WebView
    /// </summary>
    /// <param name="name"></param>
    /// <param name="token"></param>
    /// <param name="prop"></param>
    /// <param name="obj"></param>
    private async void RunCommand(string name, string token, string prop, object obj)
    {
        try
        {
            var type = obj.GetType();
            var readArguments = await RunJS("window." + name + ".getArguments('" + token + "');");
            var jsonObjects = JsonSerializer.Deserialize<JsonElement[]>(Regex.Unescape(readArguments ?? ""));
            var method = type.GetMethod(prop);
            if (method != null)
            {
                var parameters = method.GetParameters();
                var arguments = new object[parameters.Length];
                if (jsonObjects != null && jsonObjects.Length > 0)
                {
                    foreach (var arg in parameters)
                    {
                        if (jsonObjects.Length <= arg.Position && arg.DefaultValue != null)
                        {
                            arguments[arg.Position] = arg.DefaultValue;
                        }
                        else
                        {
                            var jsonObject = jsonObjects[arg.Position];
                            var jsonObject2 = jsonObject.Deserialize(arg.ParameterType);
                            if (jsonObject2 != null)
                            {
                                arguments[arg.Position] = jsonObject2;
                            }
                        }
                    }
                }

                var result = method.Invoke(obj, arguments);
                var serializedRet = "null";
                if (result != null)
                {
                    if (IsAsyncMethod(method))
                    {
                        Task task = (Task)result;
                        await task.ConfigureAwait(false);
                        result = ((dynamic)task).Result;
                    }
                    serializedRet = JsonSerializer.Serialize(result);
                }

                await RunJS("window." + name + ".returnValue('" + token + "', " + serializedRet + ");");
            }
            else
            {
                var propety = type.GetProperty(prop);
                if (propety != null)
                {
                    if (jsonObjects != null && jsonObjects.Length > 0)
                    {
                        propety.SetValue(obj, jsonObjects[0].Deserialize(propety.PropertyType));
                    }

                    var result = JsonSerializer.Serialize(propety.GetValue(obj, null));
                    await RunJS("window." + name + ".returnValue('" + token + "', " + result + ");");
                }
                else
                {
                    await RunJS("window." + name + ".rejectCall('" + token + "', 'Member not found!');");
                }
            }
        }
        catch (Exception e)
        {
            var error = e.Message + " (" + e.GetHashCode().ToString() + ")";
            error = error.Replace("\\n", " ");
            error = error.Replace("\n", " ");
            error = error.Replace("\"", "&quot;");
            await RunJS("window." + name + ".rejectCall('" + token + "', '" + error + "');");
        }
    }

    /// <summary>
    /// 发送自定义事件到 WebView
    /// </summary>
    /// <param name="type"></param>
    /// <param name="detail"></param>
    /// <param name="optBubbles"></param>
    /// <param name="optCancelable"></param>
    /// <param name="optComposed"></param>
    /// <returns></returns>
    public async Task sendEvent(string type, Dictionary<string, string>? detail = null, bool optBubbles = false, bool optCancelable = false, bool optComposed = false)
    {
        List<string> opts = [];
        if (optBubbles)
        {
            opts.Add("bubbles: true");
        }

        if (optCancelable)
        {
            opts.Add("cancelable: true");
        }

        if (optComposed)
        {
            opts.Add("composed: true");
        }

        if (detail != null)
        {
            opts.Add("detail: " + JsonSerializer.Serialize(detail));
        }

        var optsStr = (opts.Count > 0 ? ", { " + string.Join(", ", opts) + " }" : "");
        await RunJS("const nativeEvent = new CustomEvent('" + type + "'" + optsStr + "); document.dispatchEvent(nativeEvent);");
    }

    /// <summary>
    /// 在 WebView 中执行 JavaScript 代码
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public Task<string?> RunJS(string code)
    {
        if (_webView == null)
        {
            return Task.FromResult<string?>(null);
        }
        return _webView.Dispatcher.DispatchAsync(() =>
        {
            var resultCode = code;
            if (resultCode.Contains("\\n") || resultCode.Contains('\n'))
            {
                resultCode = "console.error('Called js from native api contain new line symbols!')";
            }
            else
            {
                resultCode = "try { " + resultCode + " } catch(e) { console.error(e); }";
            }

            var result = _webView.EvaluateJavaScriptAsync(resultCode);
            return result;
        });
    }
}


