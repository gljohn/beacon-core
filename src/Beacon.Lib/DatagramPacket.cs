using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Beacon.Lib
{
    internal class DatagramPacket {

        private object _data;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="data"></param>
        public DatagramPacket(object data) {
            this._data = data;          
        }

        /// <summary>
        /// Convert a string to network bytes
        /// </summary>
        public IEnumerable<byte> Encode() {
            var bytes = Encoding.UTF8.GetBytes((string)this._data);
            var len = IPAddress.HostToNetworkOrder((short)bytes.Length);

            return BitConverter.GetBytes(len).Concat(bytes);
        }

        /// <summary>
        /// Convert network bytes to a string
        /// </summary>
        public string Decode() {
            var listData = this._data as IList<byte> ?? ((IEnumerable<byte>)this._data).ToList();

            var len = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(listData.Take(2).ToArray(), 0));
            if (listData.Count() < 2 + len) throw new ArgumentException("Too few bytes in packet");

            return Encoding.UTF8.GetString(listData.Skip(2).Take(len).ToArray());
        }

    }
}
