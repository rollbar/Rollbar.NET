namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using Rollbar.Infrastructure;

    /// <summary>
    /// Class ExceptionPackage.
    /// Implements the <see cref="Rollbar.RollbarPackageBase" />
    /// </summary>
    /// <seealso cref="Rollbar.RollbarPackageBase" />
    public class ExceptionPackage
        : RollbarPackageBase
    {
        /// <summary>
        /// The exception to package
        /// </summary>
        private readonly System.Exception _exceptionToPackage;
        /// <summary>
        /// The rollbar data title
        /// </summary>
        private readonly string? _rollbarDataTitle;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionPackage"/> class.
        /// </summary>
        /// <param name="exceptionToPackage">The exception to package.</param>
        public ExceptionPackage(System.Exception exceptionToPackage)
            : this(exceptionToPackage, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionPackage"/> class.
        /// </summary>
        /// <param name="exceptionToPackage">The exception to package.</param>
        /// <param name="rollbarDataTitle">The rollbar data title.</param>
        public ExceptionPackage(System.Exception exceptionToPackage, string? rollbarDataTitle)
            : this(exceptionToPackage, rollbarDataTitle, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionPackage"/> class.
        /// </summary>
        /// <param name="exceptionToPackage">The exception to package.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> the strategy must be apply synchronously.</param>
        public ExceptionPackage(System.Exception exceptionToPackage, bool mustApplySynchronously)
            : this(exceptionToPackage, null, mustApplySynchronously)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionPackage" /> class.
        /// </summary>
        /// <param name="exceptionToPackage">The exception to package.</param>
        /// <param name="rollbarDataTitle">The rollbar data title.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> the strategy must be apply synchronously.</param>
        public ExceptionPackage(System.Exception exceptionToPackage, string? rollbarDataTitle, bool mustApplySynchronously)
            : base(mustApplySynchronously)
        {
            Assumption.AssertNotNull(exceptionToPackage, nameof(exceptionToPackage));

            this._exceptionToPackage = exceptionToPackage;
            this._rollbarDataTitle = rollbarDataTitle;
        }

        /// <summary>
        /// Produces the rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        protected override Data ProduceRollbarData()
        {
            Body rollbarBody = new Body(this._exceptionToPackage);
            IDictionary<string, object?>? custom = null;
            RollbarUtility.SnapExceptionDataAsCustomData(this._exceptionToPackage, ref custom);
            Data rollbarData = new Data(rollbarBody, custom);
            rollbarData.Title = this._rollbarDataTitle;

            return rollbarData;
        }
    }
}
