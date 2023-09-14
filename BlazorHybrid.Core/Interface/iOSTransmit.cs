using System;
using System.Collections.Generic;
using System.Text;

namespace xamarin.beacon.Interface
{
     public interface iOSTransmit
    {
        void InitializeService();

        void StartBroadcasting();
    }
}
