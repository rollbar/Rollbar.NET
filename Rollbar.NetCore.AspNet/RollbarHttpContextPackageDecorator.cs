namespace Rollbar.NetCore.AspNet
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Rollbar.Common;
    using Rollbar.DTOs;

    /// <summary>
    /// Class RollbarHttpContextPackageDecorator.
    /// Implements the <see cref="Rollbar.RollbarPackageDecoratorBase" />
    /// </summary>
    /// <seealso cref="Rollbar.RollbarPackageDecoratorBase" />
    public class RollbarHttpContextPackageDecorator
        : RollbarPackageDecoratorBase
    {
        /// <summary>
        /// The rollbar HTTP context
        /// </summary>
        private readonly RollbarHttpContext _rollbarHttpContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarHttpContextPackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="rollbarHttpContext">The rollbar HTTP context.</param>
        public RollbarHttpContextPackageDecorator(IRollbarPackage packageToDecorate, RollbarHttpContext rollbarHttpContext) 
            : this(packageToDecorate, rollbarHttpContext, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarHttpContextPackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="rollbarHttpContext">The rollbar HTTP context.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> [must apply synchronously].</param>
        public RollbarHttpContextPackageDecorator(IRollbarPackage packageToDecorate, RollbarHttpContext rollbarHttpContext, bool mustApplySynchronously) 
            : base(packageToDecorate, mustApplySynchronously)
        {
            this._rollbarHttpContext = rollbarHttpContext;
        }

        /// <summary>
        /// Decorates the specified rollbar data.
        /// </summary>
        /// <param name="rollbarData">The rollbar data.</param>
        protected override void Decorate(Data rollbarData)
        {
            if (this._rollbarHttpContext == null)
            {
                return; //nothing to decorate with...
            }

            Dictionary<string, object> customRequestFields = null;
            if (this._rollbarHttpContext != null)
            {
                customRequestFields = new Dictionary<string, object>();
                customRequestFields.Add("httpRequestTimestamp", this._rollbarHttpContext.Timestamp);
                if (this._rollbarHttpContext.HttpAttributes != null)
                {
                    customRequestFields.Add("httpRequestID", this._rollbarHttpContext.HttpAttributes.RequestID);
                    customRequestFields.Add("statusCode", this._rollbarHttpContext.HttpAttributes.StatusCode);
                    customRequestFields.Add("scheme", this._rollbarHttpContext.HttpAttributes.Scheme);
                    customRequestFields.Add("protocol", this._rollbarHttpContext.HttpAttributes.Protocol);
                }
            }

            if (customRequestFields != null && customRequestFields.Count > 0)
            {
                if (rollbarData.Request == null)
                {
                    rollbarData.Request = new Request(customRequestFields);
                }
                else
                {
                    foreach(var item in customRequestFields)
                    {
                        rollbarData.Request.Add(item);
                    }
                }
            }

            if (this._rollbarHttpContext.HttpAttributes != null)
            {
                if (rollbarData.Request == null)
                {
                    rollbarData.Request = new Request();
                }

                rollbarData.Request.Url = 
                    this._rollbarHttpContext.HttpAttributes.Host.Value + this._rollbarHttpContext.HttpAttributes.Path;
                rollbarData.Request.QueryString = 
                    this._rollbarHttpContext.HttpAttributes.Query.Value;
                rollbarData.Request.Params = null;

                rollbarData.Request.Headers = 
                    new Dictionary<string, string>(this._rollbarHttpContext.HttpAttributes.Headers.Count);
                foreach (var header in this._rollbarHttpContext.HttpAttributes.Headers)
                {
                    if (header.Value.Count == 0)
                    {
                        continue;
                    }

                    rollbarData.Request.Headers.Add(header.Key, StringUtility.Combine(header.Value, ", "));
                }

                rollbarData.Request.Method = this._rollbarHttpContext.HttpAttributes.Method;
            }
        }

    }
}
