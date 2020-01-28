namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Rollbar.DTOs;

    /// <summary>
    /// Class DataPackage.
    /// Implements the <see cref="Rollbar.RollbarPackageBase" />
    /// It is essentially a no-op/pass-through packaging strategy.
    /// </summary>
    /// <seealso cref="Rollbar.RollbarPackageBase" />
    public class DataPackage
        : RollbarPackageBase
    {
        /// <summary>
        /// The packaged data
        /// </summary>
        private readonly Data _packagedData;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPackage"/> class.
        /// </summary>
        /// <param name="packagedData">The packaged data.</param>
        public DataPackage(Data packagedData) 
            : base(false)
        {
            this._packagedData = packagedData;
        }

        /// <summary>
        /// Produces the rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        protected override Data ProduceRollbarData()
        {
            return this._packagedData;
        }
    }
}
