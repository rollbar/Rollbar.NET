namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;

    /// <summary>
    /// Class ExceptionPackagingStrategy.
    /// Implements the <see cref="Rollbar.RollbarPackagingStrategyBase" />
    /// </summary>
    /// <seealso cref="Rollbar.RollbarPackagingStrategyBase" />
    public class ExceptionPackagingStrategy
        : RollbarPackagingStrategyBase
    {
        /// <summary>
        /// The exception to package
        /// </summary>
        private readonly System.Exception _exceptionToPackage;
        /// <summary>
        /// The rollbar data title
        /// </summary>
        private readonly string _rollbarDataTitle;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionPackagingStrategy"/> class.
        /// </summary>
        /// <param name="exceptionToPackage">The exception to package.</param>
        public ExceptionPackagingStrategy(System.Exception exceptionToPackage)
            : this(exceptionToPackage, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionPackagingStrategy"/> class.
        /// </summary>
        /// <param name="exceptionToPackage">The exception to package.</param>
        /// <param name="rollbarDataTitle">The rollbar data title.</param>
        public ExceptionPackagingStrategy(System.Exception exceptionToPackage, string rollbarDataTitle)
            : base(false)
        {
            Assumption.AssertNotNull(exceptionToPackage, nameof(exceptionToPackage));

            this._exceptionToPackage = exceptionToPackage;
            this._rollbarDataTitle = rollbarDataTitle;
        }

        /// <summary>
        /// Packages as rollbar data.
        /// </summary>
        /// <returns>Data.</returns>
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
