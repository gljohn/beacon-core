﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Beacon.Lib
{
    /// <summary>
    /// Counterpart of the beacon, searches for beacons
    /// </summary>
    /// <remarks>
    /// The beacon list event will not be raised on your main thread!
    /// </remarks>
    public class Probe : IDisposable
    {
        /// <summary>
        /// Remove beacons older than this
        /// </summary>
        private static readonly TimeSpan BeaconTimeout = new TimeSpan(0, 0, 0, 5); // seconds

        public event Action<IEnumerable<Core.BeaconLocation>> BeaconsUpdated;

        private readonly Thread thread;
        private readonly EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private readonly UdpClient udp = new UdpClient();
        private IEnumerable<Core.BeaconLocation> currentBeacons = Enumerable.Empty<Core.BeaconLocation>();

        private bool running = true;

        public Probe(string beaconType)
        {
            if (string.IsNullOrEmpty(beaconType))
            {
                throw new ArgumentNullException("beaconType");
            }


            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            BeaconType = beaconType;
            thread = new Thread(BackgroundLoop) { IsBackground = true };

            udp.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            try 
            {
                //udp.AllowNatTraversal(true);
                udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.IPProtectionLevel, IPProtectionLevel.Unrestricted);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error switching on NAT traversal: " + ex.Message);
            }

            //udp.BeginReceive(ResponseReceived, null);

            Listen();
        }

        private async void Listen()
        {
            var result = await udp.ReceiveAsync();
            ResponseReceived(result);
        }

        public void Start()
        {
            thread.Start();
        }

        private void ResponseReceived(UdpReceiveResult ar) {//IAsyncResult ar)
            var remote = ar.RemoteEndPoint; //new IPEndPoint(IPAddress.Any, 0);
            var bytes = ar.Buffer; // udp.EndReceive(ar, ref remote);
            string value = new DatagramPacket(bytes).Decode();
            var typeBytes = new DatagramPacket(BeaconType).Encode().ToList(); //Beacon.Encode(BeaconType).ToList();
            Debug.WriteLine(string.Join(", ", typeBytes.Select(_ => (char)_)));
            if (Beacon.HasPrefix(bytes, typeBytes))
            {
                try
                {
                    var portBytes = bytes.Skip(typeBytes.Count()).Take(2).ToArray();
                    var port      = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(portBytes, 0));
                    var payload = new DatagramPacket(bytes.Skip(typeBytes.Count() + 2)).Decode(); //Beacon.Decode(bytes.Skip(typeBytes.Count() + 2));
                    NewBeacon(new Core.BeaconLocation(new IPEndPoint(remote.Address, port), payload, DateTime.Now));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

            //udp.BeginReceive(ResponseReceived, null);
            Listen();
        }

        public string BeaconType { get; private set; }

        private void BackgroundLoop()
        {
            while (running)
            {
                BroadcastProbe();
                waitHandle.WaitOne(2000);
                PruneBeacons();
            }
        }

        private void BroadcastProbe()
        {
            var probe = new DatagramPacket(BeaconType).Encode().ToArray(); //Beacon.Encode(BeaconType).ToArray();
            udp.SendAsync(probe, probe.Length, new IPEndPoint(IPAddress.Broadcast, Beacon.DiscoveryPort));
            //udp.Send(probe, probe.Length, new IPEndPoint(IPAddress.Broadcast, Beacon.DiscoveryPort));
        }

        private void PruneBeacons()
        {
            var cutOff = DateTime.Now - BeaconTimeout;
            var oldBeacons = currentBeacons.ToList();
            var newBeacons = oldBeacons.Where(_ => _.LastAdvertised >= cutOff).ToList();
            if (EnumsEqual(oldBeacons, newBeacons)) return;

            var u = BeaconsUpdated;
            if (u != null) u(newBeacons);
            currentBeacons = newBeacons;
        }

        private void NewBeacon(Core.BeaconLocation newBeacon)
        {
            var newBeacons = currentBeacons
                .Where(_ => !_.Equals(newBeacon))
                .Concat(new [] { newBeacon })
                .OrderBy(_ => _.Data)
                .ThenBy(_ => _.Address, IPEndPointComparer.Instance)
                .ToList();
            var u = BeaconsUpdated;
            if (u != null) u(newBeacons);
            currentBeacons = newBeacons;
        }

        private static bool EnumsEqual<T>(IEnumerable<T> xs, IEnumerable<T> ys)
        {
            return xs.Zip(ys, (x, y) => x.Equals(y)).Count() == xs.Count();
        }

        public void Stop()
        {
            running = false;
            waitHandle.Set();
            thread.Join();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
