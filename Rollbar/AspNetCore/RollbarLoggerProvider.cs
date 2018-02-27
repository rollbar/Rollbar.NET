#if NETCOREAPP

namespace Rollbar.AspNetCore
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Rollbar.Diagnostics;
    using Rollbar.NetCore;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [ProviderAlias("Rollbar")]
    internal class RollbarLoggerProvider
        : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, RollbarLogger> _loggers = 
            new ConcurrentDictionary<string, RollbarLogger>();

        private readonly IRollbarConfig _rollbarConfig = null;

        private RollbarLoggerProvider()
        {
        }

        public RollbarLoggerProvider(IConfiguration configuration)
        {
            this._rollbarConfig = RollbarConfigurationUtil.DeduceRollbarConfig(configuration);

            Assumption.AssertNotNull(this._rollbarConfig, nameof(this._rollbarConfig));
            Assumption.AssertNotNullOrWhiteSpace(this._rollbarConfig.AccessToken, nameof(this._rollbarConfig.AccessToken));
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, CreateLoggerImplementation);
        }

        private RollbarLogger CreateLoggerImplementation(string name)
        {
            //var includeScopes = _settings?.IncludeScopes ?? _includeScopes;
            //return new RollbarLogger(name, GetFilter(name, _settings), includeScopes ? _scopeProvider : null, _messageQueue);
            return new RollbarLogger(name, this._rollbarConfig);
        }

        private IRollbarConfig DeduceRollbarConfig(IConfiguration configuration)
        {
            if (RollbarLocator.RollbarInstance.Config.AccessToken != null)
            {
                return RollbarLocator.RollbarInstance.Config;
            }

            // Here we assume that the Rollbar singleton was not explicitly preconfigured 
            // anywhere in the code (Program.cs or Startup.cs), 
            // so we are trying to configure it from IConfiguration:

            Assumption.AssertNotNull(configuration, nameof(configuration));

            const string defaultAccessToken = "none";
            RollbarConfig rollbarConfig = new RollbarConfig(defaultAccessToken);
            AppSettingsUtil.LoadAppSettings(ref rollbarConfig, configuration);

            if (rollbarConfig.AccessToken == defaultAccessToken)
            {
                const string error = "Rollbar.NET notifier is not configured properly.";
                throw new Exception(error);
            }

            RollbarLocator.RollbarInstance
                .Configure(rollbarConfig);

            return rollbarConfig;
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
        // ~RollbarLoggerProvider() {
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