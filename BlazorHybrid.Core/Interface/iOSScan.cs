using System;
using System.Collections.Generic;
using System.Text;

namespace xamarin.beacon.Interface
{
    public interface iOSScan
    {
        void InitializeScannerService();
        void startranging();
        void stopranging();
    }
}
