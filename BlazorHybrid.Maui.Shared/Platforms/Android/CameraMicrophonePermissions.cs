namespace BlazorHybrid.Maui.Shared;

/// <summary>
/// 请求摄像机和麦克风
/// </summary>
public class CameraMicrophonePermissions : Permissions.BasePlatformPermission
    { 
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            new List<(string androidPermission, bool isRuntime)>
            {
                            (global::Android.Manifest.Permission.Camera, true),
                            (global::Android.Manifest.Permission.CaptureAudioOutput, true),
                            (global::Android.Manifest.Permission.CaptureSecureVideoOutput, true),
                            (global::Android.Manifest.Permission.CaptureVideoOutput, true),
                            (global::Android.Manifest.Permission.RecordAudio, true),
                            (global::Android.Manifest.Permission.Vibrate , true),
                            (global::Android.Manifest.Permission.ModifyAudioSettings , true)
            }.ToArray();
    }

 

