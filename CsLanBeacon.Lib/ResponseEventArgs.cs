using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CsLanBeacon.Lib
{
    public class ResponseEventArgs : EventArgs
    {
        public IPEndPoint Endpoint { get; set; }

        public ResponseEventArgs(IPEndPoint endpoint)
        {
            Endpoint = endpoint;
        }
    }
}
