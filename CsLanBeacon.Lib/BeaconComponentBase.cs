using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsLanBeacon.Lib
{
    public abstract class BeaconComponentBase
    {
        protected State _currentState = State.STOPPED;
        /// <summary>
        /// The state the BeaconComponent is currently in. Changes to properties can only 
        /// be applied when it is set to "STOPPED".
        /// </summary>
        public State CurrentState
        {
            get { return _currentState; }
        }

        protected string _key;
        /// <summary>
        /// The key that is used to identify a listening Beacon on the network. Both the 
        /// Beacon as well as the Probe must use the same one in order to find one another.
        /// </summary>
        public string Key
        {
            get { return _key; }
        }

        protected int _port;
        /// <summary>
        /// The port the Beacon is listening and the Probe is probing on. Must be the same 
        /// for both parties.
        /// </summary>
        public int Port
        {
            get { return _port; }
            set
            {
                if (value < IPEndPoint.MinPort || value > IPEndPoint.MaxPort)
                    throw new ArgumentException(String.Format("{0} is not a valid port.", value));

                if (CurrentState == State.STOPPED)
                {
                    _port = value;
                }
            }
        }

        public BeaconComponentBase(string key, int port = 8080)
        {
            if (key == null)
                throw new ArgumentException("Key must not be null.");

            this._key = key;
            Port = port;
        }

        protected CancellationTokenSource tokenSource;
        protected AutoResetEvent sync = new AutoResetEvent(false);

        public abstract void Start();
        public abstract void Stop();
    }
}
