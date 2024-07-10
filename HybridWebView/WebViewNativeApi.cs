using System.Reflection;
using System.Text.Json;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Text;

namespace WebViewNativeApi;

public class NativeBridge
{
    private const string DEFAULT_SCHEME = "native://";
    private const string INTERFACE_JS = "window['createNativeBridgeProxy'] = " +
        "(name, methods, properties, scheme = '" + DEFAULT_SCHEME + "') =>" +
        "{" +
        "    let apiCalls = new Map();" +
        "" +
        "    function createRequest(target, success, reject, argumentsList) {" +
        "        let uuid = crypto.randomUUID();" +
        "        while(apiCalls.has(uuid)) { uuid = crypto.randomUUID(); };" +
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

    private readonly WebView? _webView = null;
    private readonly Dictionary<(string, string), object> _targets = new();
    private bool _isInit = false;
    private (string?, string?, string?, object?) _query = ("", "", "", null);

    public string? TargetJS;

    public NativeBridge(WebView? wv)
    {
        if (wv != null)
        {
            _webView = wv;
            _webView.Navigated += OnWebViewInit;
            _webView.Navigating += OnWebViewNavigatin;
        }
    }

    public void AddTarget(string name, object obj, string sheme = DEFAULT_SCHEME)
    {
        if (obj == null)
            return;
        _targets.Add((name, sheme), obj);
        if (_isInit)
            AddTargetToWebView(name, obj, sheme);
    }

    private void OnWebViewInit(object? sender, WebNavigatedEventArgs e)
    {
        if (!_isInit)
        {
            RunJS(INTERFACE_JS);
            if (TargetJS != null)
                RunJS(TargetJS);
            foreach (KeyValuePair<(string, string), object> entry in _targets)
                AddTargetToWebView(entry.Key.Item1, entry.Value, entry.Key.Item2);
            _isInit = true;
        }
    }

    private void OnWebViewNavigatin(object? sender, WebNavigatingEventArgs e)
    {
        if (!_isInit)
            return;

        foreach (KeyValuePair<(string, string), object> entry in _targets)
        {
            string startStr = entry.Key.Item2 + entry.Key.Item1;
            if (!e.Url.StartsWith(startStr))
                continue;

            string request = e.Url[(e.Url.IndexOf(startStr) + startStr.Length)..].ToLower();
            request = request.Trim(['/', '\\']);
            string[] requestArgs = request.Split('/');
            if (requestArgs.Length < 2)
                return;
            e.Cancel = true;

            string prop = requestArgs[0];
            string token = requestArgs[1];

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

    private void AddTargetToWebView(string name, object obj, string sheme)
    {
        Type type = obj.GetType();
        List<string> methods = new List<string>();
        List<string> properties = new List<string>();
        foreach (MethodInfo method in type.GetMethods())
            methods.Add(method.Name);
        foreach (PropertyInfo p in type.GetProperties())
            properties.Add(p.Name);

        RunJS("window." + name + " = window.createNativeBridgeProxy('" + name + "', " + JsonSerializer.Serialize(methods) + ", " +
            JsonSerializer.Serialize(properties) + ", '" + sheme + "');");
    }

    private static bool IsAsyncMethod(MethodInfo method)
    {
        Type attType = typeof(AsyncStateMachineAttribute);
        var attrib = (AsyncStateMachineAttribute?) method.GetCustomAttribute(attType);
        return (attrib != null);
    }

    private async void RunCommand(string name, string token, string prop, object obj)
    {
        try
        {
            Type type = obj.GetType();
            string? readArguments = await RunJS("window." + name + ".getArguments('" + token + "');");
            JsonElement[]? jsonObjects = JsonSerializer.Deserialize<JsonElement[]>(Regex.Unescape(readArguments));
            MethodInfo? method = type.GetMethod(prop);
            if (method != null)
            {
                var parameters = method.GetParameters();
                object[] arguments = new object[parameters.Length];
                foreach (ParameterInfo arg in parameters)
                {
                    if (jsonObjects.Length <= arg.Position)
                    {
                        arguments[arg.Position] = arg.DefaultValue;
                    }
                    else
                    {
                        JsonElement jsonObject = jsonObjects[arg.Position];
                        arguments[arg.Position] = jsonObject.Deserialize(arg.ParameterType);
                    }
                }

                object? result = method.Invoke(obj, arguments);
                string serializedRet = "null";
                if (result != null)
                {
                    if (IsAsyncMethod(method))
                    {
                        Task task = (Task)result;
                        await task.ConfigureAwait(false);
                        result = (object)((dynamic)task).Result;
                    }
                    serializedRet = JsonSerializer.Serialize(result);
                }

                await RunJS("window." + name + ".returnValue('" + token + "', " + serializedRet + ");");
            }
            else
            {
                PropertyInfo? propety = type.GetProperty(prop);
                if (propety != null)
                {
                    if (jsonObjects != null && jsonObjects.Length > 0)
                        propety.SetValue(obj, jsonObjects[0].Deserialize(propety.PropertyType));
                    string result = JsonSerializer.Serialize(propety.GetValue(obj, null));
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
            string error = e.Message + " (" + e.GetHashCode().ToString() + ")";
            error = error.Replace("\\n", " ");
            error = error.Replace("\n", " ");
            error = error.Replace("\"", "&quot;");
            await RunJS("window." + name + ".rejectCall('" + token + "', '" + error + "');");
        }
    }

    public async Task sendEvent(string type, Dictionary<string, string>? detail = null, bool optBubbles = false, bool optCancelable = false, bool optComposed = false)
    {
        List<string> opts = new List<string>();
        if (optBubbles)
            opts.Add("bubbles: true");
        if (optCancelable)
            opts.Add("cancelable: true");
        if (optComposed)
            opts.Add("composed: true");
        if (detail != null)
            opts.Add("detail: " + JsonSerializer.Serialize(detail));

        string optsStr = (opts.Count > 0 ? ", { " + String.Join(", ", opts) + " }" : "");
        await RunJS("const nativeEvent = new CustomEvent('" + type + "'" + optsStr + "); document.dispatchEvent(nativeEvent);");
    }

    public Task<string?> RunJS(string code)
    {
        if (_webView == null)
        {
            return Task.FromResult<string?>(null);
        }
        return _webView.Dispatcher.DispatchAsync(() =>
        {
            string resultCode = code;
            if (resultCode.Contains("\\n") || resultCode.Contains('\n'))
                resultCode = "console.error('Called js from native api contain new line symbols!')";
            else
                resultCode = "try { " + resultCode + " } catch(e) { console.error(e); }";
            var result = _webView.EvaluateJavaScriptAsync(resultCode);
            return result;
        });
    }
}
 

