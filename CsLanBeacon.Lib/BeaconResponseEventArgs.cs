using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CsLanBeacon.Lib
{
    public class BeaconResponseEventArgs : EventArgs
    {
        public IPEndPoint BeaconEndpoint { get; set; }

        public BeaconResponseEventArgs(IPEndPoint beaconEndpoint)
        {
            BeaconEndpoint = beaconEndpoint;
        }
    }
}
