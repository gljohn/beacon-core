using System;

namespace Beacon
{
    public class Server
    {
        private Lib.Beacon _beacon;

        public Server(Core.BeaconConfig config) {
            this._beacon = new Lib.Beacon(config);
        }

        public void Start()
        {
            this._beacon.Start();
        }

        public void Stop()
        {
            this._beacon.Stop();
        }
    }
}
