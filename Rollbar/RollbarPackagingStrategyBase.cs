namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Rollbar.Common;
    using Rollbar.Diagnostics;
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
        /// The time stamp
        /// </summary>
        private readonly DateTime _timeStamp = DateTime.UtcNow;

        /// <summary>
        /// The must apply synchronously
        /// </summary>
        protected readonly bool _mustApplySynchronously = false;

        private Data _rollbarData;

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
        /// Produces the rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        protected abstract Data ProduceRollbarData();

        /// <summary>
        /// Gets a value indicating whether to package synchronously (within the logging method call).
        /// The logging methods will return very quickly when this flag is off. In the off state,
        /// the packaging strategy will be invoked during payload transmission on a dedicated worker thread.
        /// </summary>
        /// <value><c>true</c> if needs to package synchronously; otherwise, <c>false</c>.</value>
        public virtual bool MustApplySynchronously { get { return this._mustApplySynchronously; } }

        /// <summary>
        /// Gets the rollbar data packaged by this strategy (if any).
        /// </summary>
        /// <value>The rollbar data.</value>
        public virtual Data RollbarData
        {
            get { return this._rollbarData; }
        }

        /// <summary>
        /// Packages as rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        public virtual Data PackageAsRollbarData()
        {
            this._rollbarData = this.ProduceRollbarData();

            // a packaging strategy decorator is never expected to have its own valid instance of this._rollbarData:
            Assumption.AssertFalse(
                this.GetType().IsSubclassOf(typeof(RollbarPackagingStrategyDecoratorBase)) 
                && (this._rollbarData != null), 
                nameof(this._rollbarData)
                );

            // here we need to make sure the rollbarData is properly timestamped with this instance's timestamp:
            if (this._rollbarData != null)
            {
                this._rollbarData.Timestamp = DateTimeUtil.ConvertToUnixTimestampInSeconds(this._timeStamp);
            }
            return this._rollbarData;
        }

        //public virtual Task<Data> PackageAsRollbarDataAsync()
        //{
        //    return Task.Run(() => this.PackageAsRollbarData());
        //}
    }
}
