#if NETCOREAPP

namespace Rollbar.AspNetCore
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class LoggerFactoryExtensions
    {
        public static ILoggerFactory AddRollbar(
            this ILoggerFactory factory
            , IConfiguration configuration
            //,ILogger logger = null
            //,bool dispose = false
            )
        {
            //if (factory == null)
            //{
            //    throw new ArgumentNullException(nameof(factory));
            //}

            factory.AddProvider(
                new RollbarLoggerProvider(configuration)
                //new RollbarLoggerProvider(logger, dispose)
                );

            return factory;
        }
    }
}

#endif
