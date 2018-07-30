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
            var probe = new Probe("CustomKey", TimeSpan.FromSeconds(1));

            probe.ProbeActiveEvent += (s, e) => { Console.WriteLine("Probe active."); };
            probe.ProbeStoppedEvent += (s, e) => { Console.WriteLine("Probe stopped."); };
            probe.ProbeBroadcastEvent += (s, e) => { Console.WriteLine("Sending broadcast."); };
            probe.ProbeReceivedResponseEvent += (s, e) =>
            {
                Console.WriteLine(String.Format("Received response from {0}", e.Endpoint));
            };
            probe.Start();

            Console.ReadLine();
            probe.Stop();
            Console.ReadLine();

            // Find multiple endpoints using a single compacted function.
            // Use the normal "await" in a regular program!
            var endpoints = probe.FindBeaconEndpointsAsync(TimeSpan.FromSeconds(5)).Result;
            foreach (var endpoint in endpoints)
            {
                Console.WriteLine(String.Format("Found endpoint: {0}", endpoint));
            }
            Console.ReadLine();
        }
    }
}
