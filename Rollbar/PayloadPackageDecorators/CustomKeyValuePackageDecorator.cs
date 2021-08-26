namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Rollbar.DTOs;

    /// <summary>
    /// Class CustomKeyValuePackageDecorator.
    /// Implements the <see cref="Rollbar.RollbarPackageDecoratorBase" />
    /// </summary>
    /// <seealso cref="Rollbar.RollbarPackageDecoratorBase" />
    public class CustomKeyValuePackageDecorator
        : RollbarPackageDecoratorBase
    {
        /// <summary>
        /// The custom
        /// </summary>
        private readonly IDictionary<string, object?>? _custom;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomKeyValuePackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="custom">The custom.</param>
        public CustomKeyValuePackageDecorator(
            IRollbarPackage packageToDecorate, 
            IDictionary<string, object?>? custom
            )
            : this(packageToDecorate, custom, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomKeyValuePackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="custom">The custom.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> [must apply synchronously].</param>
        public CustomKeyValuePackageDecorator(
            IRollbarPackage packageToDecorate, 
            IDictionary<string, object?>? custom, 
            bool mustApplySynchronously
            ) 
            : base(packageToDecorate, mustApplySynchronously)
        {
            this._custom = custom;
        }

        /// <summary>
        /// Decorates the specified rollbar data.
        /// </summary>
        /// <param name="rollbarData">The rollbar data.</param>
        protected override void Decorate(Data? rollbarData)
        {
            if(rollbarData == null)
            {
                return;
            }

            if (this._custom == null || this._custom.Count == 0)
            {
                return; // nothing to decorate with...
            }

            if (rollbarData.Custom == null || rollbarData.Custom.Count == 0)
            {
                // assign custom key-values:
                rollbarData.Custom = this._custom;
            }
            else
            {
                // add custom key-values:
                foreach (var key in this._custom.Keys)
                {
                    rollbarData.Custom[key] = this._custom[key];
                }
            }
        }
    }
}
