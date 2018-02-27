#if NETCOREAPP

namespace Rollbar.AspNetCore
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class LoggingBuilderExtensions
    {
        public static ILoggingBuilder AddRollbar(
            this ILoggingBuilder builder 
            )
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, RollbarLoggerProvider>());

            return builder;
        }
    }
}

#endif
