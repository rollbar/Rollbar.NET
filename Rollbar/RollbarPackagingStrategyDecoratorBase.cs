namespace Rollbar
{
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Class RollbarPackagingStrategyDecoratorBase.
    /// Implements the <see cref="Rollbar.RollbarPackagingStrategyBase" />
    /// Implements the <see cref="Rollbar.IRollbarPackagingStrategy" />
    /// </summary>
    /// <seealso cref="Rollbar.RollbarPackagingStrategyBase" />
    /// <seealso cref="Rollbar.IRollbarPackagingStrategy" />
    public abstract class RollbarPackagingStrategyDecoratorBase
        : RollbarPackagingStrategyBase
        , IRollbarPackagingStrategy
    {
        /// <summary>
        /// The strategy to decorate
        /// </summary>
        private readonly IRollbarPackagingStrategy _strategyToDecorate;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarPackagingStrategyDecoratorBase" /> class.
        /// </summary>
        /// <param name="strategyToDecorate">The strategy to decorate.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> the strategy must be apply synchronously.</param>
        protected RollbarPackagingStrategyDecoratorBase(IRollbarPackagingStrategy strategyToDecorate, bool mustApplySynchronously)
            : base(mustApplySynchronously)
        {
            Assumption.AssertNotNull(strategyToDecorate, nameof(strategyToDecorate));

            this._strategyToDecorate = strategyToDecorate;
        }

        /// <summary>
        /// Packages as rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        public override Data PackageAsRollbarData()
        {
            return this._strategyToDecorate?.PackageAsRollbarData();
        }

        /// <summary>
        /// Gets a value indicating whether to package synchronously (within the logging method call).
        /// The logging methods will return very quickly when this flag is off. In the off state,
        /// the packaging strategy will be invoked during payload transmission on a dedicated worker thread.
        /// </summary>
        /// <value><c>true</c> if needs to package synchronously; otherwise, <c>false</c>.</value>
        public override bool MustApplySynchronously
        {
            get
            {
                if (this._mustApplySynchronously)
                {
                    return true;
                }
                else if (this._strategyToDecorate != null)
                {
                    return this._strategyToDecorate.MustApplySynchronously;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
