using CsLanBeacon.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLanBeacon.ConsoleBeacon
{
    class Program
    {
        static void Main(string[] args)
        {
            var beacon = new Beacon("CustomKey");

            beacon.BeaconActiveEvent += (s, e) => { Console.WriteLine("Beacon active."); };
            beacon.BeaconStoppedEvent += (s, e) => { Console.WriteLine("Beacon stopped."); };
            beacon.BeaconBroadcastReceivedEvent += (s, e) => { Console.WriteLine("Broadcast received."); };
            beacon.BeaconRespondEvent += (s, e) => { Console.WriteLine("Sending response."); };
            beacon.Start();

            Console.ReadLine();
            beacon.Stop();
            Console.ReadLine();
        }
    }
}
