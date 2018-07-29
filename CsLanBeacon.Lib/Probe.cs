using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsLanBeacon.Lib
{
    public class Probe : BeaconComponentBase
    {
        public EventHandler ProbeActiveEvent;
        public EventHandler ProbeStoppedEvent;
        public EventHandler ProbeBroadcastEvent;
        public EventHandler<ProbeResponseEventArgs> ProbeReceivedResponseEvent;

        private int _probeReceivePort;
        public int ProbeReceivePort
        {
            get { return _probeReceivePort; }
            set
            {
                if (value < IPEndPoint.MinPort || value > IPEndPoint.MaxPort)
                    throw new ArgumentException(String.Format("{0} is not a valid port.", value));

                if (CurrentState == State.STOPPED)
                {
                    _probeReceivePort = value;
                }
            }
        }

        private TimeSpan _waitTimeBetweenPings;
        public TimeSpan WaitTimeBetweenPings
        {
            get { return _waitTimeBetweenPings; }
            set
            {
                if (CurrentState == State.STOPPED)
                {
                    _waitTimeBetweenPings = value;
                }
            }
        }

        private bool canReceiveNew = true;
        private object canReceiveLock = new object();

        public Probe(string key, TimeSpan waitTimeBetweenPings, int beaconPort = 8080, int probeReceivePort = 8081) : base(key, beaconPort)
        {
            WaitTimeBetweenPings = waitTimeBetweenPings;
            ProbeReceivePort = probeReceivePort;
        }

        public override void Start()
        {
            if (CurrentState == State.STOPPED)
            {
                this.tokenSource = new CancellationTokenSource();
                var token = this.tokenSource.Token;
                this._currentState = State.RUNNING;

                Task.Run(() =>
                {
                    var ip = this.GetOwnIpAddress();
                    var message = Encoding.ASCII.GetBytes(Key);

                    this.ProbeActiveEvent?.Invoke(this, new EventArgs());

                    using (var probeClient = new UdpClient(new IPEndPoint(ip, ProbeReceivePort)))
                    {
                        var startTime = DateTime.Now;

                        while (!token.IsCancellationRequested)
                        {
                            var currentTime = DateTime.Now;
                            var elapsedTime = currentTime.Subtract(startTime);
                            var allowBroadcast = elapsedTime.CompareTo(WaitTimeBetweenPings) > 0;
                            var remainingTime = WaitTimeBetweenPings.Subtract(elapsedTime);

                            lock (this.canReceiveLock)
                            {
                                if (this.canReceiveNew)
                                {
                                    this.canReceiveNew = false;
                                    probeClient.BeginReceive(this.HandleBeginReceive, probeClient);
                                }
                            }

                            // This way mutliple clients are able to respond without the probe sending another 
                            // set of broadcasts. One broadcast is send every specified interval while multiple
                            // beacons may respond in this time period.
                            if (allowBroadcast)
                            {
                                startTime = currentTime;

                                this.ProbeBroadcastEvent?.Invoke(this, new EventArgs());
                                probeClient.Send(message, message.Length, new IPEndPoint(IPAddress.Broadcast, Port));
                            }

                            // It is possible for the time to be negative when the elapsedTime is slightly larger
                            // than the desired waiting time! This is mostly seen when debugging the application.
                            // The "total" number is used since it is a double while the "seconds" one is an int.
                            if (remainingTime.TotalSeconds < 0)
                            {
                                remainingTime = TimeSpan.FromSeconds(0);
                            }

                            this.sync.WaitOne(remainingTime);
                        }
                    }
                }, token)
                .ContinueWith((prevTask) =>
                {
                    this.ProbeStoppedEvent?.Invoke(this, new EventArgs());
                });
            }
        }

        private IPAddress GetOwnIpAddress()
        {
            IPAddress ip;

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect("8.8.8.8", IPEndPoint.MaxPort);
                ip = ((IPEndPoint)socket.LocalEndPoint).Address;
            }

            return ip;
        }

        public override void Stop()
        {
            if (CurrentState == State.RUNNING)
            {
                this._currentState = State.STOPPED;
                this.tokenSource.Cancel();
                this.sync.Set();

                // Ensure that another start of the same probe can receive answers again.
                lock (this.canReceiveLock)
                {
                    this.canReceiveNew = true;
                }
            }
        }

        private void HandleBeginReceive(IAsyncResult asyncResult)
        {
            try
            {
                this.sync.Set();

                lock (this.canReceiveLock)
                {
                    this.canReceiveNew = true;
                }

                var probingServer = asyncResult.AsyncState as UdpClient;
                var beacon = new IPEndPoint(IPAddress.Any, 0);
                var bytes = probingServer.EndReceive(asyncResult, ref beacon);
                var message = Encoding.ASCII.GetString(bytes);

                if (Key.Equals(message))
                {
                    this.ProbeReceivedResponseEvent?.Invoke(this, new ProbeResponseEventArgs(beacon));
                }
            }
            // Needed since the disposed object is used once when the worker Task is cancelled.
            catch (ObjectDisposedException e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}
