namespace Rollbar.NetPlatformExtensions
{
    using System;
    using System.Threading;
    using Rollbar.Common;
    using Rollbar.Diagnostics;

    /// <summary>
    /// Implements Rollbar Scope stack.
    /// </summary>
    public class RollbarScope
    {
        /// <summary>
        /// The maximum items reached warning
        /// </summary>
        public const string MaxItemsReachedWarning = "MaxItems limit was reached! Suspending further reports per current scope.";

        /// <summary>
        /// The name
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The state
        /// </summary>
        private readonly object _state;

        /// <summary>
        /// The log items count per this scope
        /// </summary>
        private int _logItemsCount; // counts log items instances per scope...

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarScope"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="state">The state.</param>
        public RollbarScope(string name, object state)
        {
            this._name = name;
            this._state = state;
            this._logItemsCount = 0;
        }

        /// <summary>
        /// Gets or sets the next.
        /// </summary>
        /// <value>The next.</value>
        public RollbarScope Next { get; set; }

        /// <summary>
        /// The current scope
        /// </summary>
        private static AsyncLocal<RollbarScope> currentScope =
            new AsyncLocal<RollbarScope>();

        /// <summary>
        /// Gets or sets the current.
        /// </summary>
        /// <value>The current.</value>
        public static RollbarScope Current
        {
            set { currentScope.Value = value; }
            get { return currentScope.Value; }
        }

        /// <summary>
        /// Increments the log items count.
        /// </summary>
        public void IncrementLogItemsCount()
        {
            Interlocked.Increment(ref this._logItemsCount);
        }

        /// <summary>
        /// Gets the log items count.
        /// </summary>
        /// <value>The log items count.</value>
        public int LogItemsCount
        {
            get { return this._logItemsCount; }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return this._name; }
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>The state.</value>
        public object State
        {
            get { return this._state; }
        }

        /// <summary>
        /// Pushes the specified scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <returns>IDisposable.</returns>
        public static IDisposable Push(RollbarScope scope)
        {
            Assumption.AssertNotNull(scope, nameof(scope));

            var temp = Current;
            Current = scope;
            Current.Next = temp;

            return new DisposableAction(
                () => { RollbarScope.Current = RollbarScope.Current.Next; }
                );
        }

    }
}
