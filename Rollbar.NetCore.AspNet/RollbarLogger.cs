namespace Rollbar.NetCore.AspNet
{
    using System;
    using mslogging = Microsoft.Extensions.Logging;
    using Rollbar.Diagnostics;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Implements RollbarLogger.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Logging.ILogger" />
    /// <seealso cref="System.IDisposable" />
    public class RollbarLogger
            : NetPlatformExtensions.RollbarLogger
    {

        private readonly IHttpContextAccessor? _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLogger" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="rollbarOptions">The options.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        public RollbarLogger(string name
            , IRollbarLoggerConfig rollbarConfig
            , NetPlatformExtensions.RollbarOptions? rollbarOptions
            , IHttpContextAccessor? httpContextAccessor
            )
            : base(name, rollbarConfig, rollbarOptions)
        {
            this._httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>
        /// An IDisposable that ends the logical operation scope on dispose.
        /// </returns>
        public override IDisposable BeginScope<TState>(TState state)
        {
            Assumption.AssertTrue(!object.Equals(state, default(TState)), nameof(state));

            var scope = new RollbarScope(this.Name, state);
            scope.HttpContext = RollbarScope.Current?.HttpContext ?? new RollbarHttpContext();
            return RollbarScope.Push(scope);
        }

        /// <summary>
        /// Composes the rolbar package.
        /// </summary>
        /// <typeparam name="TState">The type of the t state.</typeparam>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="state">The state.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="formatter">The formatter.</param>
        /// <returns>IRollbarPackage (if any) or null.</returns>
        protected override IRollbarPackage? ComposeRollbarPackage<TState>(
            mslogging.EventId eventId,
            TState state,
            Exception exception,
            Func<TState,Exception,string> formatter
            )
        {
            IRollbarPackage? package =  base.ComposeRollbarPackage(eventId,state,exception,formatter);
            if(package != null)
            {
                var currentContext = GetCurrentContext();
                if(currentContext != null)
                {
                    package = new RollbarHttpContextPackageDecorator(package, currentContext, true);
                }
            }

            return package;
        }

        private RollbarHttpContext GetCurrentContext()
        {
            var context = RollbarScope.Current?.HttpContext ?? new RollbarHttpContext();

            if (context.HttpAttributes == null 
                && this._httpContextAccessor != null 
                && this._httpContextAccessor.HttpContext != null
                )
            {
                context.HttpAttributes = new RollbarHttpAttributes(this._httpContextAccessor.HttpContext);
            }

            return context;
        }
    }
}
