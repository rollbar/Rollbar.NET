namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Rollbar.DTOs;

    /// <summary>
    /// Class RollbarPackagingStrategyBase.
    /// Implements the <see cref="Rollbar.IRollbarPackagingStrategy" />
    /// </summary>
    /// <seealso cref="Rollbar.IRollbarPackagingStrategy" />
    public abstract class RollbarPackagingStrategyBase
        : IRollbarPackagingStrategy
    {
        /// <summary>
        /// The must apply synchronously
        /// </summary>
        protected readonly bool _mustApplySynchronously = false;

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarPackagingStrategyBase"/> class from being created.
        /// </summary>
        private RollbarPackagingStrategyBase()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarPackagingStrategyBase"/> class.
        /// </summary>
        /// <param name="mustApplySynchronously">if set to <c>true</c> the strategy must be apply synchronously.</param>
        protected RollbarPackagingStrategyBase(bool mustApplySynchronously)
        {
            this._mustApplySynchronously = mustApplySynchronously;
        }

        /// <summary>
        /// Gets a value indicating whether to package synchronously (within the logging method call).
        /// The logging methods will return very quickly when this flag is off. In the off state,
        /// the packaging strategy will be invoked during payload transmission on a dedicated worker thread.
        /// </summary>
        /// <value><c>true</c> if needs to package synchronously; otherwise, <c>false</c>.</value>
        public virtual bool MustApplySynchronously { get { return this._mustApplySynchronously; } }

        /// <summary>
        /// Packages as rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        public abstract Data PackageAsRollbarData();

        //public virtual Task<Data> PackageAsRollbarDataAsync()
        //{
        //    return Task.Run(() => this.PackageAsRollbarData());
        //}
    }
}
