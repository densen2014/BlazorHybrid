using CoreLocation;
using Foundation;

namespace xamarin.beacon.iOS;

public static class Helpers
{
    // 使用正确的构造函数创建 CLBeacon 区域，如果输入无效则返回 null（无例外）
    public static CLBeaconRegion? CreateRegion(NSUuid uuid, NSNumber major, NSNumber minor)
    {
        if (uuid == null)
            return null;
        if (minor == null)
        {
            if (major == null)
                return new CLBeaconRegion(uuid, Defaults.Identifier);
            else
                return new CLBeaconRegion(uuid, major.UInt16Value, Defaults.Identifier);
        }
        else if (major != null)
        {
            return new CLBeaconRegion(uuid, major.UInt16Value, minor.UInt16Value, Defaults.Identifier);
        }
        return null;
    }

    /// <summary>
    /// 消息
    /// </summary>
    public static event Action<string>? OnMessage;
    static CLLocationManager locationManager = new CLLocationManager();
    static NSUuid beaconId = new NSUuid("FDA50693-A4E2-4FB1-AFCF-C6EB07647825");
    public static void test()
    {
        try
        {

            var beaconRegion = new CLBeaconRegion(beaconId, "My Beacon");
            locationManager.RegionEntered += (s, e) => {
                if (e.Region.Identifier == "My Beacon")
                {
                    OnMessage?.Invoke("Found My Beacon");
                }
            };
            locationManager.StartMonitoring(beaconRegion);

            locationManager.StartRangingBeacons(beaconRegion);

            locationManager.DidRangeBeacons += (lm, rangeEvents) => {
                switch (rangeEvents.Beacons[0].Proximity)
                {
                    case CLProximity.Far:
                        OnMessage?.Invoke("你越来越冷了!");
                        break;
                    case CLProximity.Near:
                        OnMessage?.Invoke("你越来越温暖!");
                        break;
                    case CLProximity.Immediate:
                        OnMessage?.Invoke("你红得火热!");
                        break;
                    case CLProximity.Unknown:
                        OnMessage?.Invoke("我不知道");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);

        }
    }


}
