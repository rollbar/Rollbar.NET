#if NETCOREAPP

namespace Rollbar.AspNetCore
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
        private readonly string _name = null;
        private readonly object _state = null;

        public RollbarScope(string name, object state)
        {
            this._name = name;
            this._state = state;
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

#endif