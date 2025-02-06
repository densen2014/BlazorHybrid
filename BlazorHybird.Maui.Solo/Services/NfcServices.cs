// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using DH.NFC;

namespace BlazorHybrid.Components;

/// <summary>
/// Nfc服务
/// </summary>
public class NfcServices
{

    public Task GetBatteryLevel()
    {
        // Event raised when a ndef message is received.
        CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
        // Event raised when a ndef message has been published.
        CrossNFC.Current.OnMessagePublished += Current_OnMessagePublished;
        // Event raised when a tag is discovered. Used for publishing.
        CrossNFC.Current.OnTagDiscovered += Current_OnTagDiscovered;
        // Event raised when NFC listener status changed
        CrossNFC.Current.OnTagListeningStatusChanged += Current_OnTagListeningStatusChanged;

        // Android Only:
        // Event raised when NFC state has changed.
        CrossNFC.Current.OnNfcStatusChanged += Current_OnNfcStatusChanged;

        // iOS Only: 
        // Event raised when a user cancelled NFC session.
        CrossNFC.Current.OniOSReadingSessionCancelled += Current_OniOSReadingSessionCancelled;

        UpdateDevicename?.Invoke($"设备 ");


        UpdateValue?.Invoke($"%");

        var record = new NFCNdefRecord
        {
            TypeFormat = NFCNdefTypeFormat.Mime,
            MimeType = "application/com.companyname.yourapp",
            //Payload = NFCUtils.EncodeToByteArray(_writePayload)
        };
        return Task.CompletedTask;
    }

    private void Current_OniOSReadingSessionCancelled(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    private void Current_OnNfcStatusChanged(bool isEnabled)
    {
        throw new NotImplementedException();
    }

    private void Current_OnTagListeningStatusChanged(bool isListening)
    {
        throw new NotImplementedException();
    }

    private void Current_OnTagDiscovered(ITagInfo tagInfo, bool format)
    {
        throw new NotImplementedException();
    }

    private void Current_OnMessagePublished(ITagInfo tagInfo)
    {
        throw new NotImplementedException();
    }

    private void Current_OnMessageReceived(ITagInfo tagInfo)
    {
        throw new NotImplementedException();
    }

    public event Action<string>? UpdateDevicename;

    public event Action<object>? UpdateValue;

    public event Action<string>? UpdateStatus;

    public event Action<BluetoothDevice>? UpdateDeviceInfo;

    public event Action<string>? UpdateError;
}
