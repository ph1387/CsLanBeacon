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
    public class Beacon : BeaconComponentBase
    {
        /// <summary>
        /// Event fired when the underlying working task is started. The event is handled
        /// in a different thread. Therefore a dispatcher is needed when performing changes
        /// in i.e. the UI thread!
        /// </summary>
        public EventHandler BeaconActiveEvent;
        /// <summary>
        /// Event fired when the underlying working task is stopped. The event is handled
        /// in a different thread. Therefore a dispatcher is needed when performing changes
        /// in i.e. the UI thread!
        /// </summary>
        public EventHandler BeaconStoppedEvent;
        /// <summary>
        /// Event fired when the underlying working task sends a response to a Probe's broadcast. 
        /// The BeaconResponseEventArgs contain information about the sender.
        /// The event is handled in a different thread. Therefore a dispatcher is needed when 
        /// performing changes in i.e. the UI thread!
        /// </summary>
        public EventHandler<ResponseEventArgs> BeaconResponseEvent;

        /// <summary>
        /// A Beacon that responds to broadcasting Probes on the lan.
        /// </summary>
        /// <param name="key">The key that is used for identifying a matching Probe on the lan.</param>
        /// <param name="port">The port the Beacon is listening on.</param>
        public Beacon(string key, int port = 8080) : base(key, port)
        {

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
                    this.BeaconActiveEvent?.Invoke(this, new EventArgs());

                    using (var server = new UdpClient(Port))
                    {
                        while (!token.IsCancellationRequested)
                        {
                            server.BeginReceive(this.HandleBeginReceive, server);

                            this.sync.WaitOne();
                        }
                    }
                }, token)
                .ContinueWith((prevTask) =>
                {
                    this.BeaconStoppedEvent?.Invoke(this, new EventArgs());
                });
            }
        }

        public override void Stop()
        {
            if (CurrentState == State.RUNNING)
            {
                this._currentState = State.STOPPED;
                this.tokenSource.Cancel();
                this.sync.Set();
            }
        }

        private void HandleBeginReceive(IAsyncResult asyncResult)
        {
            try
            {
                this.sync.Set();

                var server = asyncResult.AsyncState as UdpClient;
                var client = new IPEndPoint(IPAddress.Any, 0);
                var bytes = server.EndReceive(asyncResult, ref client);
                var message = Encoding.ASCII.GetString(bytes);

                if (Key.Equals(message))
                {
                    var responseBytes = Encoding.ASCII.GetBytes(Key);
                    var responseEndpoint = new IPEndPoint(client.Address, client.Port);

                    this.BeaconResponseEvent?.Invoke(this, new ResponseEventArgs(responseEndpoint));
                    server.Send(responseBytes, responseBytes.Length, responseEndpoint);
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
