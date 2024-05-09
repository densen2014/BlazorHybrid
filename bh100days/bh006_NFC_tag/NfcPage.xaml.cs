#if WINDOWS
using BlazorMaui.Platforms.Windows;
#endif
using DH.NFC;
using System.Text;

namespace bh006_NFC_tag;

public partial class NfcPage : ContentPage
{
    public NfcPage()
    {
        InitializeComponent();

#if WINDOWS
        WindowsTools.SetTitle("NFC功能");
#endif
    }

    public const string ALERT_TITLE = "NFC";
    public const string MIME_TYPE = "application/com.densen.nfc";

    NFCNdefTypeFormat _type;
    bool _makeReadOnly = false;
    bool _eventsAlreadySubscribed = false;
    bool _isDeviceiOS = false;

    //https://gitee.com/dengho/DH.Maui.FrameWork/blob/master/Demo/DH.NFCDemo/NfcPage.xaml.cs

    /// <summary>
    /// 跟踪Android设备是否仍在监听的属性，
    /// 因此它可以向用户指示这一点。
    /// </summary>
    public bool DeviceIsListening
    {
        get => _deviceIsListening;
        set
        {
            _deviceIsListening = value;
            OnPropertyChanged(nameof(DeviceIsListening));
        }
    }
    private bool _deviceIsListening;

    private bool _nfcIsEnabled;
    public bool NfcIsEnabled
    {
        get => _nfcIsEnabled;
        set
        {
            _nfcIsEnabled = value;
            OnPropertyChanged(nameof(NfcIsEnabled));
            OnPropertyChanged(nameof(NfcIsDisabled));
        }
    }

    public bool NfcIsDisabled => !NfcIsEnabled;

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 为了支持Mifare Classic 1K标签（读/写），必须将传统模式设置为true。
        CrossNFC.Legacy = false;

        if (CrossNFC.IsSupported)
        {
            if (!CrossNFC.Current.IsAvailable)
                await ShowAlert("NFC is not available");

            NfcIsEnabled = CrossNFC.Current.IsEnabled;
            if (!NfcIsEnabled)
                await ShowAlert("NFC is disabled");

            if (DeviceInfo.Platform == DevicePlatform.iOS)
                _isDeviceiOS = true;

            //// 自定义NFC配置（例如法语的UI消息）
            //CrossNFC.Current.SetConfiguration(new NfcConfiguration
            //{
            //	DefaultLanguageCode = "fr",
            //	Messages = new UserDefinedMessages
            //	{
            //		NFCSessionInvalidated = "Session invalidée",
            //		NFCSessionInvalidatedButton = "OK",
            //		NFCWritingNotSupported = "L'écriture des TAGs NFC n'est pas supporté sur cet appareil",
            //		NFCDialogAlertMessage = "Approchez votre appareil du tag NFC",
            //		NFCErrorRead = "Erreur de lecture. Veuillez rééssayer",
            //		NFCErrorEmptyTag = "Ce tag est vide",
            //		NFCErrorReadOnlyTag = "Ce tag n'est pas accessible en écriture",
            //		NFCErrorCapacityTag = "La capacité de ce TAG est trop basse",
            //		NFCErrorMissingTag = "Aucun tag trouvé",
            //		NFCErrorMissingTagInfo = "Aucune information à écrire sur le tag",
            //		NFCErrorNotSupportedTag = "Ce tag n'est pas supporté",
            //		NFCErrorNotCompliantTag = "Ce tag n'est pas compatible NDEF",
            //		NFCErrorWrite = "Aucune information à écrire sur le tag",
            //		NFCSuccessRead = "Lecture réussie",
            //		NFCSuccessWrite = "Ecriture réussie",
            //		NFCSuccessClear = "Effaçage réussi"
            //	}
            //});

            await AutoStartAsync().ConfigureAwait(false);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        try
        {
            Task.Run(() => StopListening());
        }
        catch
        {
        }
        return base.OnBackButtonPressed();
    }

    /// <summary>
    /// 自动开始收听
    /// </summary>
    /// <returns></returns>
    async Task AutoStartAsync()
    {
        // 在Android上阻止Java.Lang.IllegalStateException的一些延迟“仅当您的活动恢复时才能启用前台调度”
        await Task.Delay(500);
        await StartListeningIfNotiOS();
    }

    /// <summary>
    /// 订阅NFC活动
    /// </summary>
    void SubscribeEvents()
    {
        if (_eventsAlreadySubscribed)
            UnsubscribeEvents();

        _eventsAlreadySubscribed = true;

        CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
        CrossNFC.Current.OnMessagePublished += Current_OnMessagePublished;
        CrossNFC.Current.OnTagDiscovered += Current_OnTagDiscovered;
        CrossNFC.Current.OnNfcStatusChanged += Current_OnNfcStatusChanged;
        CrossNFC.Current.OnTagListeningStatusChanged += Current_OnTagListeningStatusChanged;

        if (_isDeviceiOS)
            CrossNFC.Current.OniOSReadingSessionCancelled += Current_OniOSReadingSessionCancelled;
    }

    /// <summary>
    /// 取消订阅NFC活动
    /// </summary>
    void UnsubscribeEvents()
    {
        CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceived;
        CrossNFC.Current.OnMessagePublished -= Current_OnMessagePublished;
        CrossNFC.Current.OnTagDiscovered -= Current_OnTagDiscovered;
        CrossNFC.Current.OnNfcStatusChanged -= Current_OnNfcStatusChanged;
        CrossNFC.Current.OnTagListeningStatusChanged -= Current_OnTagListeningStatusChanged;

        if (_isDeviceiOS)
            CrossNFC.Current.OniOSReadingSessionCancelled -= Current_OniOSReadingSessionCancelled;

        _eventsAlreadySubscribed = false;
    }

    /// <summary>
    /// 侦听器状态更改时引发的事件
    /// </summary>
    /// <param name="isListening"></param>
    void Current_OnTagListeningStatusChanged(bool isListening) => DeviceIsListening = isListening;

    /// <summary>
    /// NFC状态更改时引发的事件
    /// </summary>
    /// <param name="isEnabled">NFC status</param>
    async void Current_OnNfcStatusChanged(bool isEnabled)
    {
        NfcIsEnabled = isEnabled;
        await ShowAlert($"NFC has been {(isEnabled ? "enabled" : "disabled")}");
    }

    /// <summary>
    /// 收到NDEF消息时引发的事件
    /// </summary>
    /// <param name="tagInfo">Received <see cref="ITagInfo"/></param>
    async void Current_OnMessageReceived(ITagInfo tagInfo)
    {
        if (tagInfo == null)
        {
            await ShowAlert("No tag found");
            return;
        }

        // 自定义序列号
        var identifier = tagInfo.Identifier;
        var serialNumber = NFCUtils.ByteArrayToHexString(identifier, ":");
        var title = !string.IsNullOrWhiteSpace(serialNumber) ? $"标签 [{serialNumber}]" : "标签信息";

        if (!tagInfo.IsSupported)
        {
            await ShowAlert("Unsupported tag (app)", title);
        }
        else if (tagInfo.IsEmpty)
        {
            await ShowAlert("Empty tag", title);
        }
        else
        {
            var first = tagInfo.Records[0];
            await ShowAlert(GetMessage(first), title);
        }
    }

    /// <summary>
    /// Event raised when user cancelled NFC session on iOS 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Current_OniOSReadingSessionCancelled(object? sender, EventArgs e) => Debug("iOS NFC Session has been cancelled");

    /// <summary>
    /// 在标记上发布数据时引发的事件
    /// </summary>
    /// <param name="tagInfo">发布<see cref="ITagInfo"/></param>
    async void Current_OnMessagePublished(ITagInfo tagInfo)
    {
        try
        {
            ChkReadOnly.IsChecked = false;
            CrossNFC.Current.StopPublishing();
            if (tagInfo.IsEmpty)
                await ShowAlert("Formatting tag operation successful");
            else
                await ShowAlert("Writing tag operation successful");
        }
        catch (Exception ex)
        {
            await ShowAlert(ex.Message);
        }
    }

    /// <summary>
    /// 发现NFC标签时引发的事件
    /// </summary>
    /// <param name="tagInfo"><see cref="ITagInfo"/>被发布</param>
    /// <param name="format">设置标签格式</param>
    async void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
    {
        if (!CrossNFC.Current.IsWritingTagSupported)
        {
            await ShowAlert("此设备不支持写入标签");
            return;
        }

        try
        {
            NFCNdefRecord? record = null;
            switch (_type)
            {
                case NFCNdefTypeFormat.WellKnown:
                    record = new NFCNdefRecord
                    {
                        TypeFormat = NFCNdefTypeFormat.WellKnown,
                        MimeType = MIME_TYPE,
                        Payload = NFCUtils.EncodeToByteArray("DH.Maui.FrameWork是很棒的!"),
                        LanguageCode = "en"
                    };
                    break;
                case NFCNdefTypeFormat.Uri:
                    record = new NFCNdefRecord
                    {
                        TypeFormat = NFCNdefTypeFormat.Uri,
                        Payload = NFCUtils.EncodeToByteArray("https://gitee.com/dengho/DH.Maui.FrameWork")
                    };
                    break;
                case NFCNdefTypeFormat.Mime:
                    record = new NFCNdefRecord
                    {
                        TypeFormat = NFCNdefTypeFormat.Mime,
                        MimeType = MIME_TYPE,
                        Payload = NFCUtils.EncodeToByteArray("DH.Maui.FrameWork是很棒的!")
                    };
                    break;
                default:
                    break;
            }

            if (!format && record == null)
                throw new Exception("记录不能为空.");

            tagInfo.Records = new[] { record };

            if (format)
                CrossNFC.Current.ClearMessage(tagInfo);
            else
            {
                CrossNFC.Current.PublishMessage(tagInfo, _makeReadOnly);
            }
        }
        catch (Exception ex)
        {
            await ShowAlert(ex.Message);
        }
    }

    /// <summary>
    /// 单击"读取标签"按钮时开始监听NFC标签
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void Button_Clicked_StartListening(object sender, System.EventArgs e) => await BeginListening();

    /// <summary>
    /// 停止监听NFC标签
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void Button_Clicked_StopListening(object sender, System.EventArgs e) => await StopListening();

    /// <summary>
    /// 当将引发<see cref="Current_OnTagDiscovered(ITagInfo, bool)"/>事件时，启动发布操作以写入文本标签
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void Button_Clicked_StartWriting(object sender, System.EventArgs e) => await Publish(NFCNdefTypeFormat.WellKnown);

    /// <summary>
    /// 当将引发<see cref="Current_OnTagDiscovered(ITagInfo, bool)"/>事件时，启动发布操作以写入URI标签
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void Button_Clicked_StartWriting_Uri(object sender, System.EventArgs e) => await Publish(NFCNdefTypeFormat.Uri);

    /// <summary>
    /// 当将引发<see cref="Current_OnTagDiscovered(ITagInfo, bool)"/>事件时，启动发布操作以写入自定义标签
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void Button_Clicked_StartWriting_Custom(object sender, System.EventArgs e) => await Publish(NFCNdefTypeFormat.Mime);

    /// <summary>
    /// 当将引发<see cref="Current_OnTagDiscovered(ITagInfo, bool)"/>事件时，启动发布操作以格式化标签
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void Button_Clicked_FormatTag(object sender, System.EventArgs e) => await Publish();

    /// <summary>
    /// 将数据发布到标签的任务
    /// </summary>
    /// <param name="type"><see cref="NFCNdefTypeFormat"/></param>
    /// <returns>要执行的任务</returns>
    async Task Publish(NFCNdefTypeFormat? type = null)
    {
        await StartListeningIfNotiOS();
        try
        {
            _type = NFCNdefTypeFormat.Empty;
            if (ChkReadOnly.IsChecked)
            {
                if (!await DisplayAlert("警告", "使标签为只读操作是永久性的，无法撤消。您确定要继续吗？", "是", "否"))
                {
                    ChkReadOnly.IsChecked = false;
                    return;
                }
                _makeReadOnly = true;
            }
            else
                _makeReadOnly = false;

            if (type.HasValue) _type = type.Value;
            CrossNFC.Current.StartPublishing(!type.HasValue);
        }
        catch (Exception ex)
        {
            await ShowAlert(ex.Message);
        }
    }

    /// <summary>
    /// 返回NDEF记录中的标记信息
    /// </summary>
    /// <param name="record"><see cref="NFCNdefRecord"/></param>
    /// <returns>标签信息</returns>
    string GetMessage(NFCNdefRecord record)
    {
        var message = $"消息: {record.Message}";
        message += Environment.NewLine;
        message += $"原始消息: {Encoding.UTF8.GetString(record.Payload)}";
        message += Environment.NewLine;
        message += $"类型: {record.TypeFormat}";

        if (!string.IsNullOrWhiteSpace(record.MimeType))
        {
            message += Environment.NewLine;
            message += $"Mime类型: {record.MimeType}";
        }

        return message;
    }

    /// <summary>
    /// 在调试控制台中编写调试消息
    /// </summary>
    /// <param name="message">要显示的消息</param>
    void Debug(string message) => System.Diagnostics.Debug.WriteLine(message);

    /// <summary>
    /// 显示消息
    /// </summary>
    /// <param name="message">要显示的消息</param>
    /// <param name="title">消息标题</param>
    /// <returns>要执行的任务</returns>
    Task ShowAlert(string message, string? title = null) => DisplayAlert(string.IsNullOrWhiteSpace(title) ? ALERT_TITLE : title, message, "OK");

    /// <summary>
    /// 如果用户的设备平台不是iOS，则开始监听NFC标签的任务
    /// </summary>
    /// <returns>要执行的任务</returns>
    async Task StartListeningIfNotiOS()
    {
        if (_isDeviceiOS)
        {
            SubscribeEvents();
            return;
        }
        await BeginListening();
    }

    /// <summary>
    /// 安全开始监听NFC标签的任务
    /// </summary>
    /// <returns>要执行的任务</returns>
    async Task BeginListening()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SubscribeEvents();
                CrossNFC.Current.StartListening();
            });
        }
        catch (Exception ex)
        {
            await ShowAlert(ex.Message);
        }
    }

    /// <summary>
    /// 安全停止监听NFC标签的任务
    /// </summary>
    /// <returns>要执行的任务</returns>
    async Task StopListening()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {

                    CrossNFC.Current.StopListening();
                    UnsubscribeEvents();
                }
                catch (Exception ex)
                {
                    await ShowAlert(ex.Message);
                }
            });
        }
        catch (Exception ex)
        {
            await ShowAlert(ex.Message);
        }
    }

    async  void Button_Clicked_Close(object sender, System.EventArgs e)
    {
        //await Application.re
          Application.Current!.MainPage!.Navigation.RemovePage(this);
        await Application.Current.MainPage.Navigation.PushModalAsync(new MainPage());
    }

}
