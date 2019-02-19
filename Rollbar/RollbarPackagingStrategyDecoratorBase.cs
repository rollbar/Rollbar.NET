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
        /// Produces the rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        protected override Data ProduceRollbarData()
        {
            // this is a decorator, it does not produce Rollbar Data, but decorates provided one (if any)
            return null; 
        }

        /// <summary>
        /// Decorates the specified rollbar data.
        /// </summary>
        /// <param name="rollbarData">The rollbar data.</param>
        protected abstract void Decorate(Data rollbarData);

        /// <summary>
        /// Packages as rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        public override Data PackageAsRollbarData()
        {
            Data rollbarData = this._strategyToDecorate?.PackageAsRollbarData();
            this.Decorate(rollbarData);
            return rollbarData;
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
                // here, we are looking for the first/closest (if any) strategy 
                // in the decoration chain that must be applied synchronously:
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

        /// <summary>
        /// Gets the rollbar data packaged by this strategy (if any).
        /// </summary>
        /// <value>The rollbar data.</value>
        public override Data RollbarData
        {
            get
            {
                // a decorator is not expected to have its own Rollbar Data,
                // but can get it from a wrapped/decorated strategy (if any):
                return this._strategyToDecorate?.RollbarData;
            }
        }

    }
}
