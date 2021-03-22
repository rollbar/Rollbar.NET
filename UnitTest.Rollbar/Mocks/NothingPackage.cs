namespace UnitTest.Rollbar.Mocks
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Class NothingPackage.
    /// Implements the <see cref="Rollbar.RollbarPackageBase" />
    /// Produces null Data during packaging operation.
    /// </summary>
    /// <seealso cref="Rollbar.RollbarPackageBase" />
    public class NothingPackage
        : RollbarPackageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NothingPackage"/> class.
        /// </summary>
        /// <param name="mustApplySynchronously">if set to <c>true</c> the strategy must be apply synchronously.</param>
        public NothingPackage(bool mustApplySynchronously) 
            : base(mustApplySynchronously)
        {
        }

        /// <summary>
        /// Produces the rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        protected override Data ProduceRollbarData()
        {
            return null;
        }
    }
}
