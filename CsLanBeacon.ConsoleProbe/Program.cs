using CsLanBeacon.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLanBeacon.ConsoleProbe
{
    class Program
    {
        static void Main(string[] args)
        {
            var beacon = new Probe("CustomKey", TimeSpan.FromSeconds(1));

            beacon.ProbeActiveEvent += (s, e) => { Console.WriteLine("Probe active."); };
            beacon.ProbeStoppedEvent += (s, e) => { Console.WriteLine("Probe stopped."); };
            beacon.ProbeBroadcastEvent += (s, e) => { Console.WriteLine("Sending broadcast."); };
            beacon.ProbeReceivedResponseEvent += (s, e) => { Console.WriteLine("Received response."); };
            beacon.Start();

            Console.ReadLine();
        }
    }
}
