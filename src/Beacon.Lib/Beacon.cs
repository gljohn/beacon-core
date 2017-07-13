using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Beacon.Lib
{
    /// <summary>
    /// Instances of this class can be autodiscovered on the local network through UDP broadcasts
    /// </summary>
    /// <remarks>
    /// The advertisement consists of the beacon's application type and a short beacon-specific string.
    /// </remarks>
    public class Beacon : IDisposable {

        internal const int DiscoveryPort = 35891;
        private readonly UdpClient udp;
        private Core.BeaconConfig _config;

        /// <summary>
        /// 
        /// </summary>
        public bool Stopped { get; private set; }



        public Beacon(Core.BeaconConfig config) { //string beaconType, ushort advertisedPort)
            this._config = config;
          /* OLD STUFF REMOVE LATER
           * ==========================
             BeaconType     = beaconType;
             AdvertisedPort = advertisedPort;
             BeaconData     = "";
           */

            udp = new UdpClient();
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udp.Client.Bind(new IPEndPoint(IPAddress.Any, DiscoveryPort));

            try {
                //udp.AllowNatTraversal(true);

                //Hopefully the below works ok and AllowNatTraversal does nothing more. 
                //https://msdn.microsoft.com/en-us/library/system.net.sockets.udpclient.allownattraversal(v=vs.110).aspx
                udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.IPProtectionLevel, IPProtectionLevel.Unrestricted);
            } catch (SocketException ex) {
                Debug.WriteLine("Error switching on NAT traversal: " + ex.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error switching on NAT traversal: " + e.Message);
            }
        }

        public void Start()
        {
            Stopped = false;
            //udp.BeginReceive(ProbeReceived, null);
            Listen();
        }

        private async void Listen()
        {
            var result = await udp.ReceiveAsync();
            ProbeReceived(result);
        }

        public void Stop()
        {
            Stopped = true;
        }

        private void ProbeReceived(UdpReceiveResult ar) { //IAsyncResult ar)
            var remote = ar.RemoteEndPoint; //new IPEndPoint(IPAddress.Any, 0);
            var bytes = ar.Buffer;

            // Compare beacon type to probe type
            var typeBytes = new DatagramPacket(this._config.BeaconType).Encode();
            if (HasPrefix(bytes, typeBytes))
            {
                // If true, respond again with our type, port and payload
                var responseData = new DatagramPacket(this._config.BeaconType).Encode() //Encode(BeaconType)
                    .Concat(BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder((short)this._config.AdvertisedPort)))
                    .Concat(new DatagramPacket(this._config.BeaconData).Encode()).ToArray();
                //udp.Send(responseData, responseData.Length, remote);
                var data = new DatagramPacket(responseData).Decode();
                udp.SendAsync(responseData, responseData.Length, remote);
            }

            if (!Stopped) Listen();// udp.BeginReceive(ProbeReceived, null);
        }

        internal static bool HasPrefix<T>(IEnumerable<T> haystack, IEnumerable<T> prefix)
        {
            return haystack.Count() >= prefix.Count() &&
                haystack.Zip(prefix, (a, b) => a.Equals(b)).All(_ => _);
        }
        
        public void Dispose()
        {
            Stop();
        }
    }
}
