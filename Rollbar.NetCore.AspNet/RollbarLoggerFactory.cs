namespace Rollbar.NetCore.AspNet
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Implements Rollbar version of Microsoft.Extensions.Logging.ILoggerFactory.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Logging.ILoggerFactory" />
    public class RollbarLoggerFactory
            : ILoggerFactory
    {
        private readonly RollbarLoggerProvider _loggerProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLoggerFactory" /> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="rollbarOptions">The rollbar options.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        public RollbarLoggerFactory(
                    IConfiguration config
                    , IOptions<RollbarOptions> rollbarOptions
                    , IHttpContextAccessor httpContextAccessor
                    )
        {
            this._loggerProvider =
                new RollbarLoggerProvider(config, rollbarOptions, httpContextAccessor);
        }

        /// <summary>
        /// Adds an <see cref="T:Microsoft.Extensions.Logging.ILoggerProvider" /> to the logging system.
        /// </summary>
        /// <param name="provider">The <see cref="T:Microsoft.Extensions.Logging.ILoggerProvider" />.</param>
        public void AddProvider(ILoggerProvider provider)
        {
            //no op...
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Extensions.Logging.ILogger" /> instance.
        /// </summary>
        /// <param name="categoryName">The category name for messages produced by the logger.</param>
        /// <returns>
        /// The <see cref="T:Microsoft.Extensions.Logging.ILogger" />.
        /// </returns>
        public ILogger CreateLogger(string categoryName)
        {
            return this._loggerProvider.CreateLogger(categoryName);
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
#pragma warning disable CA1063 // Implement IDisposable Correctly
        public void Dispose()
#pragma warning restore CA1063 // Implement IDisposable Correctly
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
