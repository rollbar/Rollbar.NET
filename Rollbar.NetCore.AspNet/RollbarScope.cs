namespace Rollbar.NetCore.AspNet
{
    using System;
    using System.Threading;
    using Rollbar.Common;
    using Rollbar.Diagnostics;

    /// <summary>
    /// Implements Rollbar Scope stack.
    /// </summary>
    internal class RollbarScope
    {
        public const string MaxItemsReachedWarning = "MaxItems limit was reached! Suspending further reports per current scope.";

        private readonly string _name;

        private readonly object _state;

        private int _logItemsCount; // counts log items instances per scope...

        public RollbarScope(string name, object state)
        {
            this._name = name;
            this._state = state;
            this._logItemsCount = 0;
        }

        public RollbarHttpContext HttpContext { get; set; }

        public RollbarScope Next { get; set; }

        private static AsyncLocal<RollbarScope> currentScope =
            new AsyncLocal<RollbarScope>();

        public static RollbarScope Current
        {
            set { currentScope.Value = value; }
            get { return currentScope.Value; }
        }

        public void IncrementLogItemsCount()
        {
            Interlocked.Increment(ref this._logItemsCount);
        }

        public int LogItemsCount
        {
            get { return this._logItemsCount; }
        }

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
