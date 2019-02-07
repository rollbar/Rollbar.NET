namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Rollbar.DTOs;

    public abstract class RollbarPackagingStrategyBase
        : IRollbarPackagingStrategy
    {
        public abstract Data PackageAsRollbarData();

        public virtual Task<Data> PackageAsRollbarDataAsync()
        {
            return Task.Run(() => this.PackageAsRollbarData());
        }
    }
}
