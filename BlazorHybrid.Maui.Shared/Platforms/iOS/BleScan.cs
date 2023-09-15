using CoreLocation;
using Foundation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using xamarin.beacon.Interface;
using xamarin.beacon.Model;

#if IOS14_2_OR_GREATER
#endif

namespace xamarin.beacon.iOS.Services;

public class BleScan //: iOSScan
{
    private CLLocationManager? _locationManager;
    private CLBeaconRegion? _region;
    public ObservableCollection<SharedBeacon> ReceivedBeacons { get; set; } = new ObservableCollection<SharedBeacon>();
    List<SharedBeacon> _sharedBeacons = new List<SharedBeacon>();
    public event Action<string>? OnMessage;


    public CLLocationManager beaconImpl
    {
        get
        {
            if (_locationManager == null)
                _locationManager = scannerconfig();
            return _locationManager;

        }
    }

    public void InitializeScannerService()
    {
        if (_locationManager == null)
            _locationManager = scannerconfig();
    }

    public CLLocationManager scannerconfig()
    {
        _locationManager = new CLLocationManager();
        _locationManager.AuthorizationChanged += LocationManagerAuthorizationChanged;
        _locationManager.DidRangeBeacons += LocationManagerDidRangeBeacons;
        //_locationManager.DidRangeBeacons += InsertBeacons;
        _locationManager.RequestAlwaysAuthorization();
        _region = new CLBeaconRegion(new NSUuid(), "MyRegion");

        return _locationManager;
    }

    void LocationManagerAuthorizationChanged(object? sender, CLAuthorizationChangedEventArgs e)
    {
        OnMessage?.Invoke($"Authorisation state: {e.Status}");
        Debug.WriteLine("Authorisation state: {0}", e.Status);
        if (e.Status == CLAuthorizationStatus.AuthorizedAlways)
        {
            _locationManager?.StartRangingBeacons(_region);
        }
    }

    public void startranging()
    {
        _locationManager?.StartRangingBeacons(_region);
    }

    public void stopranging()
    {
        _locationManager?.StopRangingBeacons(_region);

    }

    void LocationManagerDidRangeBeacons(object? sender, CLRegionBeaconsRangedEventArgs e)
    {
        CLBeacon? nearest = null;
        _sharedBeacons = new List<SharedBeacon>();

        foreach (var beacon in e.Beacons.Where(b => b.Rssi != 0))
        {
            var rssi_to_string = beacon.Rssi.ToString();
            var rssi_to_int = int.Parse(rssi_to_string, NumberStyles.AllowLeadingSign);
            Debug.WriteLine(rssi_to_int);

            var distance = getdistance(rssi_to_int, -69);


            Debug.WriteLine("Ranged {0} {1}.{2} - distance {3}",
                beacon.ProximityUuid.AsString(),
                beacon.Major,
                beacon.Rssi,
                distance);

            OnMessage?.Invoke($"Ranged {beacon.ProximityUuid.AsString()} {beacon.Major}.{beacon.Rssi} - distance {distance}");

            _sharedBeacons.Add(new SharedBeacon("Conatct_tracing", "66:4E:32:66:3C:61", beacon.ProximityUuid.AsString(), beacon.Major.ToString(), beacon.Minor.ToString(), distance, rssi_to_int));

            if (nearest == null || nearest.Rssi < beacon.Rssi)
                nearest = beacon;
        }

        Task.Run(() =>
        {
            // I send beacons to XF project
            if (_sharedBeacons.Count > 0)
            {
                Debug.WriteLine("I SEND TO XF " + _sharedBeacons.Count + " BEACONS");
                OnMessage?.Invoke("I SEND TO XF " + _sharedBeacons.Count + " BEACONS");
                //Xamarin.Forms.MessagingCenter.Send<App, List<SharedBeacon>>((App)Xamarin.Forms.Application.Current, "BeaconsReceived", _sharedBeacons);
            }
        });

    }

    private double getdistance(int rssi, int txpower)
    {
        return Math.Pow(10d, ((double)txpower - rssi) / (10 * 2));
    }

}

