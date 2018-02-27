#if NETCOREAPP

namespace Rollbar.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class RollbarLoggerScope
        : IDisposable
    {
        private const string defaultName = "None";

        private readonly RollbarLoggerProvider _loggerProvider = null;
        private readonly object _state = null;
        private readonly IDisposable _chainedDisposable = null;

        public RollbarLoggerScope(RollbarLoggerProvider loggerProvider, object state, IDisposable chainedDisposable = null)
        {
            this._loggerProvider = loggerProvider;
            this._state = state;
            this._chainedDisposable = chainedDisposable;
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

                    this._chainedDisposable?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RollbarLoggerScope() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}

#endif
