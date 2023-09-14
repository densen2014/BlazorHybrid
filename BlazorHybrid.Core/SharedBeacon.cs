using System;
using System.ComponentModel;
using System.Drawing;

namespace xamarin.beacon.Model
{
    public class SharedBeacon : INotifyPropertyChanged
    {

        public int Rssi { get; set; }
        public string BluetoothAddress { get; }
        public string Id1 { get; set; }
        public string Id2 { get; set; }
        public string Id3 { get; set; }
        public double Distance { get; set; }
        public string BluetoothName { get; set; }
        public bool IsNew
        {
            get
            {

                if (FirstReceivedDateTime != null && CurrentDateTime != null && ((CurrentDateTime - FirstReceivedDateTime).TotalSeconds <= 5))
                    return true;

                return false;

            }
        }
        public DateTime LastReceivedDateTime { get; set; }
        public DateTime FirstReceivedDateTime { get; set; }
        public DateTime CurrentDateTime { get; set; }
        /// <summary>
        /// The beacon is not received by 10'', so it's old
        /// </summary>
        public bool IsOld
        {
            get
            {

                if (LastReceivedDateTime != null && CurrentDateTime != null && ((CurrentDateTime - LastReceivedDateTime).TotalSeconds > 10))
                    return true;

                return false;

            }
        }

        public bool CanDelete
        {
            get
            {

                if (LastReceivedDateTime != null && CurrentDateTime != null && ((CurrentDateTime - LastReceivedDateTime).TotalSeconds > 20))
                    return true;

                return false;

            }
        }

        public bool ForceDelete
        {
            get
            {

                if (LastReceivedDateTime != null && CurrentDateTime != null && ((CurrentDateTime - LastReceivedDateTime).TotalSeconds > 60))
                    return true;

                return false;

            }
        }

        public Color BackgroundColor
        {
            get
            {
                if (CanDelete)
                    return Color.Red;
                else if (IsOld)
                    return Color.Orange;
                else if (IsNew)
                    return Color.Green;

                return Color.White;
            }
        }

        public SharedBeacon(string bluetoothName, string bluetoothAddress, string id1, string id2, string id3, double distance, int rssi)
        {
            BluetoothName = bluetoothName;
            BluetoothAddress = bluetoothAddress;
            Id1 = id1;
            Id2 = id2;
            Id3 = id3;
            Distance = distance;
            Rssi = rssi;
            LastReceivedDateTime = DateTime.Now;
            FirstReceivedDateTime = LastReceivedDateTime;
            CurrentDateTime = LastReceivedDateTime;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void Update(DateTime now, double distance, int rssi)
        {
            LastReceivedDateTime = now;
            Distance = distance;
            Rssi = rssi;
        }
    }
}
