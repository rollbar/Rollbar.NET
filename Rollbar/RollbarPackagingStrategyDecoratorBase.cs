namespace Rollbar
{
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class RollbarPackagingStrategyDecoratorBase
        : RollbarPackagingStrategyBase
        , IRollbarPackagingStrategy
    {
        private readonly IRollbarPackagingStrategy _strategyToDecorate;

        private RollbarPackagingStrategyDecoratorBase()
        {
        }

        protected RollbarPackagingStrategyDecoratorBase(IRollbarPackagingStrategy strategyToDecorate)
        {
            Assumption.AssertNotNull(strategyToDecorate, nameof(strategyToDecorate));

            this._strategyToDecorate = strategyToDecorate;
        }

        public override Data PackageAsRollbarData()
        {
            return this._strategyToDecorate?.PackageAsRollbarData();
        }
    }
}
