﻿using System;
using System.Collections.Generic;

namespace Beacon
{
    public class Client : IDisposable
    {
        private Lib.Probe _probe;

        public event Action<IEnumerable<Core.BeaconLocation>> BeaconsUpdated
        {
            add { this._probe.BeaconsUpdated += value; }
            remove { this._probe.BeaconsUpdated -= value; }
        }

        public Client(string beaconType)
        {
            this._probe = new Lib.Probe(beaconType);
        }
        
        public void Start()
        {
            this._probe.Start();
        }
     
        public void Stop()
        {
            this._probe.Stop();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    this._probe.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                this._probe = null;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Client() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion



    }
}
