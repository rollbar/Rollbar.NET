#if NETCOREAPP

namespace Rollbar.AspNetCore
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class RollbarLoggerFactory
        : ILoggerFactory
    {
        private readonly RollbarLoggerProvider _loggerProvider;

        public RollbarLoggerFactory()
        {
            //this._loggerProvider = new RollbarLoggerProvider();
        }

        public void AddProvider(ILoggerProvider provider)
        {
            //no op...
            //throw new NotImplementedException();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return this._loggerProvider.CreateLogger(categoryName);
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
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RollbarLoggerFactory() {
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

#endif