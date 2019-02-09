namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;

    public class ExceptionPackagingStrategy
        : RollbarPackagingStrategyBase
    {
        private readonly System.Exception _exceptionToPackage;
        private readonly string _rollbarDataTitle;

        private ExceptionPackagingStrategy()
        {
        }

        public ExceptionPackagingStrategy(System.Exception exceptionToPackage)
            : this(exceptionToPackage, null)
        {
        }

        public ExceptionPackagingStrategy(System.Exception exceptionToPackage, string rollbarDataTitle)
        {
            Assumption.AssertNotNull(exceptionToPackage, nameof(exceptionToPackage));

            this._exceptionToPackage = exceptionToPackage;
            this._rollbarDataTitle = rollbarDataTitle;
        }

        public override Data PackageAsRollbarData()
        {
            Body rollbarBody = new Body(this._exceptionToPackage);
            IDictionary<string, object> custom = null;
            RollbarUtility.SnapExceptionDataAsCustomData(this._exceptionToPackage, ref custom);
            Data rollbarData = new Data(rollbarBody, custom);
            rollbarData.Title = this._rollbarDataTitle;

            return rollbarData;
        }
    }
}
