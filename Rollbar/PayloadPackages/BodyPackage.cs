namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using Rollbar.DTOs;

    /// <summary>
    /// Class BodyPackage.
    /// Implements the <see cref="Rollbar.RollbarPackageBase" />
    /// </summary>
    /// <seealso cref="Rollbar.RollbarPackageBase" />
    public class BodyPackage
        : RollbarPackageBase
    {
        /// <summary>
        /// The rollbar configuration
        /// </summary>
        private readonly IRollbarLoggerConfig? _rollbarConfig;
        /// <summary>
        /// The body to package
        /// </summary>
        private readonly Body _bodyToPackage;
        /// <summary>
        /// The custom
        /// </summary>
        private readonly IDictionary<string, object?>? _custom;

        /// <summary>
        /// Initializes a new instance of the <see cref="BodyPackage"/> class.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="bodyToPackage">The body to package.</param>
        public BodyPackage(IRollbarLoggerConfig? rollbarConfig, Body bodyToPackage)
            : this(rollbarConfig, bodyToPackage, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BodyPackage"/> class.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="bodyToPackage">The body to package.</param>
        /// <param name="custom">The custom.</param>
        public BodyPackage(IRollbarLoggerConfig? rollbarConfig, Body bodyToPackage, IDictionary<string, object?>? custom) 
            : base(false)
        {
            this._rollbarConfig = rollbarConfig;
            this._bodyToPackage = bodyToPackage;
            this._custom = custom;
        }

        /// <summary>
        /// Produces the rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        protected override Data ProduceRollbarData()
        {
            Data data = new Data(this._rollbarConfig, this._bodyToPackage, this._custom);

            return data;
        }
    }
}
