using System;
using System.Net;

namespace Beacon.Core
{
    public class BeaconConfig
    {
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
