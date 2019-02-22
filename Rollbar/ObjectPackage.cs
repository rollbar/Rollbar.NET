namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;

    public class ObjectPackage
        : RollbarPackageBase
    {
        private readonly object _objectToPackage;
        private readonly string _rollbarDataTitle;
        private IDictionary<string, object> _custom; 

        public ObjectPackage(
            object objectToPackage
            )
            : this(objectToPackage, null, null, false)
        {
        }

        public ObjectPackage(
            object objectToPackage,
            bool mustApplySynchronously
            )
            : this(objectToPackage, null, null, mustApplySynchronously)
        {
        }

        public ObjectPackage(
            object objectToPackage,
            IDictionary<string, object> custom
            )
            : this(objectToPackage, null, custom, false)
        {
        }

        public ObjectPackage(
            object objectToPackage,
            IDictionary<string, object> custom,
            bool mustApplySynchronously
            )
            : this(objectToPackage, null, custom, mustApplySynchronously)
        {
        }

        public ObjectPackage(
            object objectToPackage,
            string rollbarDataTitle,
            bool mustApplySynchronously
            )
            : this(objectToPackage, rollbarDataTitle, null, mustApplySynchronously)
        {
        }

        public ObjectPackage(
            object objectToPackage,
            string rollbarDataTitle,
            IDictionary<string, object> custom
            )
            : this(objectToPackage, rollbarDataTitle, custom, false)
        {
        }

        public ObjectPackage(
            object objectToPackage, 
            string rollbarDataTitle, 
            IDictionary<string, object> custom, 
            bool mustApplySynchronously
            ) 
            : base(mustApplySynchronously)
        {
            Assumption.AssertNotNull(objectToPackage, nameof(objectToPackage));

            this._objectToPackage = objectToPackage;
            this._rollbarDataTitle = rollbarDataTitle;
            this._custom = custom;
        }

        protected override Data ProduceRollbarData()
        {
            Data data = null;
            switch(this._objectToPackage)
            {
                case Data dataObj:
                    data = dataObj;
                    return data;
                case IRollbarPackage rollbarPackageObj:
                    data = rollbarPackageObj.PackageAsRollbarData();
                    return data;
                default:
                    Body body = this._objectToPackage as Body;
                    if (body == null)
                    {
                        body = RollbarUtility.PackageAsPayloadBody(this._objectToPackage, ref this._custom);
                    }

                    data = new Data(null, body, this._custom);
                    return data;
            }
        }
    }
}
