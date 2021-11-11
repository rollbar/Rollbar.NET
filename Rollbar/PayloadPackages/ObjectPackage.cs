namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using Rollbar.Infrastructure;

    /// <summary>
    /// Class ObjectPackage.
    /// Implements the <see cref="Rollbar.RollbarPackageBase" />
    /// </summary>
    /// <seealso cref="Rollbar.RollbarPackageBase" />
    public class ObjectPackage
        : RollbarPackageBase
    {
        /// <summary>
        /// The object to package
        /// </summary>
        private readonly object _objectToPackage;
        /// <summary>
        /// The rollbar data title
        /// </summary>
        private readonly string? _rollbarDataTitle;
        /// <summary>
        /// The custom
        /// </summary>
        private IDictionary<string, object?>? _custom;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPackage"/> class.
        /// </summary>
        /// <param name="objectToPackage">The object to package.</param>
        public ObjectPackage(
            object objectToPackage
            )
            : this(objectToPackage, null, null, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPackage"/> class.
        /// </summary>
        /// <param name="objectToPackage">The object to package.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> [must apply synchronously].</param>
        public ObjectPackage(
            object objectToPackage,
            bool mustApplySynchronously
            )
            : this(objectToPackage, null, null, mustApplySynchronously)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPackage"/> class.
        /// </summary>
        /// <param name="objectToPackage">The object to package.</param>
        /// <param name="custom">The custom.</param>
        public ObjectPackage(
            object objectToPackage,
            IDictionary<string, object?>? custom
            )
            : this(objectToPackage, null, custom, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPackage"/> class.
        /// </summary>
        /// <param name="objectToPackage">The object to package.</param>
        /// <param name="custom">The custom.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> [must apply synchronously].</param>
        public ObjectPackage(
            object objectToPackage,
            IDictionary<string, object?>? custom,
            bool mustApplySynchronously
            )
            : this(objectToPackage, null, custom, mustApplySynchronously)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPackage"/> class.
        /// </summary>
        /// <param name="objectToPackage">The object to package.</param>
        /// <param name="rollbarDataTitle">The rollbar data title.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> [must apply synchronously].</param>
        public ObjectPackage(
            object objectToPackage,
            string rollbarDataTitle,
            bool mustApplySynchronously
            )
            : this(objectToPackage, rollbarDataTitle, null, mustApplySynchronously)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPackage"/> class.
        /// </summary>
        /// <param name="objectToPackage">The object to package.</param>
        /// <param name="rollbarDataTitle">The rollbar data title.</param>
        /// <param name="custom">The custom.</param>
        public ObjectPackage(
            object objectToPackage,
            string? rollbarDataTitle,
            IDictionary<string, object?>? custom
            )
            : this(objectToPackage, rollbarDataTitle, custom, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPackage"/> class.
        /// </summary>
        /// <param name="objectToPackage">The object to package.</param>
        /// <param name="rollbarDataTitle">The rollbar data title.</param>
        /// <param name="custom">The custom.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> [must apply synchronously].</param>
        public ObjectPackage(
            object objectToPackage, 
            string? rollbarDataTitle, 
            IDictionary<string, object?>? custom, 
            bool mustApplySynchronously
            ) 
            : base(mustApplySynchronously)
        {
            Assumption.AssertNotNull(objectToPackage, nameof(objectToPackage));

            this._objectToPackage = objectToPackage;
            this._rollbarDataTitle = rollbarDataTitle;
            this._custom = custom;
        }

        /// <summary>
        /// Produces the rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        protected override Data? ProduceRollbarData()
        {
            Data? data = null;
            switch(this._objectToPackage)
            {
                case Data dataObj:
                    data = dataObj;
                    break;
                case IRollbarPackage rollbarPackageObj:
                    data = rollbarPackageObj.PackageAsRollbarData();
                    break;
                default:
                    Body? body = this._objectToPackage as Body;
                    if (body == null)
                    {
                        body = RollbarUtility.PackageAsPayloadBody(this._objectToPackage, ref this._custom);
                    }

                    data = new Data(null, body, this._custom);
                    break;
            }

            if (data != null && !string.IsNullOrWhiteSpace(this._rollbarDataTitle))
            {
                data.Title = this._rollbarDataTitle;
            }

            return data;
        }
    }
}
