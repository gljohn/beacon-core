using System;
using System.Collections.Generic;

namespace Beacon
{
    public class Client
    {
        private Lib.Probe _probe;

        public event Action<IEnumerable<Core.BeaconLocation>> BeaconsUpdated
        {
            add { this._probe.BeaconsUpdated += value; }
            remove { this._probe.BeaconsUpdated -= value; }
        }

        public Client()
        {
            this._probe = new Lib.Probe("beaconType");
        }
        
        public void Start()
        {
            this._probe.Start();
        }
     
        public void Stop()
        {
            this._probe.Stop();
        }

       
    }
}
