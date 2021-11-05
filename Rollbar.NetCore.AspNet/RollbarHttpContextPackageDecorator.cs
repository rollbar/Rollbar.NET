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
        public RollbarHttpContextPackageDecorator(
            IRollbarPackage packageToDecorate,
            RollbarHttpContext rollbarHttpContext
            )
            : this(packageToDecorate, rollbarHttpContext, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarHttpContextPackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="rollbarHttpContext">The rollbar HTTP context.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> [must apply synchronously].</param>
        public RollbarHttpContextPackageDecorator(
            IRollbarPackage packageToDecorate,
            RollbarHttpContext rollbarHttpContext,
            bool mustApplySynchronously
            )
            : base(packageToDecorate, mustApplySynchronously)
        {
            this._rollbarHttpContext = rollbarHttpContext;
        }

        /// <summary>
        /// Decorates the specified rollbar data.
        /// </summary>
        /// <param name="rollbarData">The rollbar data.</param>
        protected override void Decorate(Data? rollbarData)
        {
            if(this._rollbarHttpContext == null || rollbarData == null)
            {
                return; //nothing to decorate with...
            }

            Dictionary<string, object?> customRequestFields = ExtractCustomRequestFields(this._rollbarHttpContext);
            if (customRequestFields.Count > 0)
            {
                if (rollbarData.Request == null)
                {
                    rollbarData.Request = new Request(customRequestFields);
                }
                else
                {
                    foreach (var item in customRequestFields)
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
                Collect(rollbarData.Request, this._rollbarHttpContext.HttpAttributes);

                if (rollbarData.Response == null)
                {
                    rollbarData.Response = new Response();
                }
                Collect(rollbarData.Response, this._rollbarHttpContext.HttpAttributes);
            }
        }

        /// <summary>
        /// Collects the response specific attributes.
        /// </summary>
        /// <param name="response">
        /// The response.
        /// </param>
        /// <param name="rollbarHttpAttributes">
        /// The rollbar HTTP attributes.
        /// </param>
        private static void Collect(Response response, RollbarHttpAttributes rollbarHttpAttributes)
        {
            response.StatusCode = rollbarHttpAttributes.ResponseStatusCode;
            if (rollbarHttpAttributes.ResponseHeaders?.Count > 0)
            {
                response.Headers =
                    new Dictionary<string, string>(rollbarHttpAttributes.ResponseHeaders.Count);
                foreach (var header in rollbarHttpAttributes.ResponseHeaders)
                {
                    if (header.Value.Count == 0)
                    {
                        continue;
                    }

                    response.Headers.Add(header.Key, StringUtility.Combine(header.Value, ", "));
                }
            }
            if (!string.IsNullOrWhiteSpace(rollbarHttpAttributes.ResponseBody))
            {
                response.Body = rollbarHttpAttributes.ResponseBody;
            }
        }

        /// <summary>
        /// Collects the request specific attributes.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <param name="rollbarHttpAttributes">
        /// The rollbar HTTP attributes.
        /// </param>
        private static void Collect(Request request, RollbarHttpAttributes rollbarHttpAttributes)
        {
            request.Url =
                rollbarHttpAttributes.RequestHost.Value + rollbarHttpAttributes.RequestPath;
            request.QueryString =
                rollbarHttpAttributes.RequestQuery.Value;
            request.Params = null;
            request.Method = rollbarHttpAttributes.RequestMethod;
            if (rollbarHttpAttributes.RequestHeaders?.Count > 0)
            {
                request.Headers =
                    new Dictionary<string, string>(rollbarHttpAttributes.RequestHeaders.Count);
                foreach (var header in rollbarHttpAttributes.RequestHeaders)
                {
                    if (header.Value.Count == 0)
                    {
                        continue;
                    }

                    request.Headers.Add(header.Key, StringUtility.Combine(header.Value, ", "));
                }
            }
            if (!string.IsNullOrWhiteSpace(rollbarHttpAttributes.RequestBody))
            {
                request.PostBody = rollbarHttpAttributes.RequestBody;
            }
        }

        /// <summary>
        /// Extracts the custom request fields.
        /// </summary>
        /// <param name="rollbarHttpContext">
        /// The rollbar HTTP context.
        /// </param>
        /// <returns>
        /// Dictionary&lt;System.String, System.Nullable&lt;System.Object&gt;&gt;.
        /// </returns>
        private static Dictionary<string, object?> ExtractCustomRequestFields(RollbarHttpContext rollbarHttpContext)
        {
            Dictionary<string, object?> customRequestFields = new();
            
            customRequestFields.Add("httpRequestTimestamp", rollbarHttpContext.Timestamp);
            
            if (rollbarHttpContext.HttpAttributes != null)
            {
                customRequestFields.Add("httpRequestID", rollbarHttpContext.HttpAttributes.RequestID);
                customRequestFields.Add("scheme", rollbarHttpContext.HttpAttributes.RequestScheme);
                customRequestFields.Add("protocol", rollbarHttpContext.HttpAttributes.RequestProtocol);
                customRequestFields.Add("statusCode", rollbarHttpContext.HttpAttributes.ResponseStatusCode);
            }

            return customRequestFields;
        }
    }
}
