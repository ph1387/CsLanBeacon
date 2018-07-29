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
        public State CurrentState
        {
            get { return _currentState; }
        }

        protected string _key;
        public string Key
        {
            get { return _key; }
        }

        protected int _port;
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
