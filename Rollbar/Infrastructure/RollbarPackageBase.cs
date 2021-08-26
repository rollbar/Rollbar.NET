namespace Rollbar
{
    using Rollbar.Common;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class RollbarPackageBase.
    /// Implements the <see cref="Rollbar.IRollbarPackage" />
    /// Implements the <see cref="Rollbar.Common.IErrorCollector" />
    /// </summary>
    /// <seealso cref="Rollbar.IRollbarPackage" />
    /// <seealso cref="Rollbar.Common.IErrorCollector" />
    public abstract class RollbarPackageBase
        : IRollbarPackage
        , IErrorCollector

    {
        /// <summary>
        /// The must apply synchronously
        /// </summary>
        protected readonly bool applySynchronously = false;

        /// <summary>
        /// The rollbar data resulted from this package instance.
        /// </summary>
        private Data? _rollbarData;

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarPackageBase"/> class from being created.
        /// </summary>
        private RollbarPackageBase()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarPackageBase"/> class.
        /// </summary>
        /// <param name="mustApplySynchronously">if set to <c>true</c> the strategy must be apply synchronously.</param>
        protected RollbarPackageBase(bool mustApplySynchronously)
        {
            this.applySynchronously = mustApplySynchronously;
        }

        /// <summary>
        /// Produces the rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        protected abstract Data? ProduceRollbarData();

        /// <summary>
        /// Gets a value indicating whether to package synchronously (within the logging method call).
        /// The logging methods will return very quickly when this flag is off. In the off state,
        /// the packaging strategy will be invoked during payload transmission on a dedicated worker thread.
        /// </summary>
        /// <value><c>true</c> if needs to package synchronously; otherwise, <c>false</c>.</value>
        public virtual bool MustApplySynchronously { get { return this.applySynchronously; } }

        /// <summary>
        /// Gets the rollbar data packaged by this strategy (if any).
        /// </summary>
        /// <value>The rollbar data.</value>
        public virtual Data? RollbarData
        {
            get { return this._rollbarData; }
        }

        /// <summary>
        /// Packages as rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        public virtual Data? PackageAsRollbarData()
        {
            if (this._rollbarData != null)
            {
                //we do not want to do the job more than once:
                return this._rollbarData;
            }

            this._rollbarData = this.ProduceRollbarData();

            // a packaging strategy decorator is never expected to have its own valid instance of this._rollbarData:
            Assumption.AssertFalse(
                this.GetType().IsSubclassOf(typeof(RollbarPackageDecoratorBase)) 
                && (this._rollbarData != null), 
                nameof(this._rollbarData)
                );

            return this._rollbarData;
        }

        #region IErrorCollector

        /// <summary>
        /// The package-related exceptions (if any) that happened during the package lifetime.
        /// </summary>
        /// <value>The exceptions.</value>
        public IReadOnlyCollection<System.Exception> Exceptions
        {
            get { return this._errorCollector.Exceptions; }
        }

        /// <summary>
        /// Registers the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Register(System.Exception exception)
        {
            this._errorCollector.Register(exception);
        }

        private readonly IErrorCollector _errorCollector = new ErrorCollector();

        #endregion IErrorCollctor
    }
}
