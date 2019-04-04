namespace UnitTest.Rollbar.Mocks
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Class FaultyPackage mock.
    /// Implements the <see cref="Rollbar.RollbarPackageBase" />
    /// It throws an exception during data packaging operation.
    /// </summary>
    /// <seealso cref="Rollbar.RollbarPackageBase" />
    public class FaultyPackage
        : RollbarPackageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FaultyPackage"/> class.
        /// </summary>
        /// <param name="mustApplySynchronously">if set to <c>true</c> the strategy must be apply synchronously.</param>
        public FaultyPackage(bool mustApplySynchronously) 
            : base(mustApplySynchronously)
        {
        }

        /// <summary>
        /// Produces the rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        /// <exception cref="NotImplementedException">Intentionally faulty package for unit testing!</exception>
        protected override Data ProduceRollbarData()
        {
            throw new NotImplementedException("Intentionally faulty package for unit testing!");
        }
    }
}
