using System;
using System.Net;

namespace Beacon.Core
{
    public class BeaconConfig
    {

        public BeaconConfig(string beaconType, ushort advertisedPort)
        {
            if (string.IsNullOrEmpty(beaconType))
            {
                throw new ArgumentNullException("beaconType");
            }

            if (advertisedPort < 1)
            {
                throw new ArgumentOutOfRangeException("advertisedPort");
            }

            this.BeaconType = beaconType;
            this.AdvertisedPort = advertisedPort;

        }
        /// <summary>
        /// Return the machine's hostname (usually nice to mention in the beacon text)
        /// </summary>
        public static string HostName
        {
            get { return Dns.GetHostName(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string BeaconType { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ushort AdvertisedPort { get; private set; }

       
        /// <summary>
        /// 
        /// </summary>
        public string BeaconData { get; set; }
    }
}
