namespace Rollbar.NetCore.AspNet
{
    using System.Threading;

    /// <summary>
    /// Implements Rollbar Scope stack.
    /// </summary>
    internal class RollbarScope 
        : NetPlatformExtensions.RollbarScope
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarScope"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="state">The state.</param>
        public RollbarScope(string name, object? state)
            : base(name, state)
        {
        }

        /// <summary>
        /// Gets or sets the HTTP context.
        /// </summary>
        /// <value>The HTTP context.</value>
        public RollbarHttpContext? HttpContext { get; set; }

        private static AsyncLocal<RollbarScope?> currentScope =
            new AsyncLocal<RollbarScope?>();

        public static new RollbarScope? Current
        {
            set
            {
                currentScope.Value = value;
            }
            get
            {
                return currentScope.Value;
            }
        }

    }
}
