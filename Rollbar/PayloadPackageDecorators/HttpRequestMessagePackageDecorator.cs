namespace Rollbar
{
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Net.Http;
    using Rollbar.DTOs;
    using Rollbar.Diagnostics;
    using Rollbar.Common;
    using System.Linq;

#if (NETFX)
    using System.ServiceModel.Channels;
    using System.Web;
#endif

    /// <summary>
    /// Class HttpRequestMessagePackageDecorator.
    /// Implements the <see cref="Rollbar.RollbarPackageDecoratorBase" />
    /// </summary>
    /// <seealso cref="Rollbar.RollbarPackageDecoratorBase" />
    public class HttpRequestMessagePackageDecorator
        : RollbarPackageDecoratorBase
    {
        private static readonly TraceSource traceSource = new TraceSource(typeof(HttpRequestMessagePackageDecorator).FullName ?? "HttpRequestMessagePackageDecorator");

        /// <summary>
        /// The HTTP request message
        /// </summary>
        private readonly HttpRequestMessage _httpRequestMessage;
        /// <summary>
        /// The rollbar configuration
        /// </summary>
        private readonly IRollbarLoggerConfig _rollbarConfig;
        /// <summary>
        /// The arbitrary key value pairs
        /// </summary>
        private readonly IDictionary<string, object?>? _arbitraryKeyValuePairs;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestMessagePackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="httpRequestMessage">The HTTP request message.</param>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        public HttpRequestMessagePackageDecorator(
            IRollbarPackage packageToDecorate,
            HttpRequestMessage httpRequestMessage,
            IRollbarLoggerConfig rollbarConfig
            )
            : this(packageToDecorate, httpRequestMessage, rollbarConfig, null, false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestMessagePackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="httpRequestMessage">The HTTP request message.</param>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        public HttpRequestMessagePackageDecorator(
            IRollbarPackage packageToDecorate,
            HttpRequestMessage httpRequestMessage,
            IRollbarLoggerConfig rollbarConfig,
            IDictionary<string, object?>? arbitraryKeyValuePairs
            )
            : this(packageToDecorate, httpRequestMessage, rollbarConfig, arbitraryKeyValuePairs, false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestMessagePackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="httpRequestMessage">The HTTP request message.</param>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> [must apply synchronously].</param>
        public HttpRequestMessagePackageDecorator(
            IRollbarPackage packageToDecorate,
            HttpRequestMessage httpRequestMessage,
            IRollbarLoggerConfig rollbarConfig,
            bool mustApplySynchronously
            )
            : this(packageToDecorate, httpRequestMessage, rollbarConfig, null, mustApplySynchronously)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestMessagePackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="httpRequestMessage">The HTTP request message.</param>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> [must apply synchronously].</param>
        public HttpRequestMessagePackageDecorator(
            IRollbarPackage packageToDecorate, 
            HttpRequestMessage  httpRequestMessage, 
            IRollbarLoggerConfig rollbarConfig,
            IDictionary<string, object?>? arbitraryKeyValuePairs,
            bool mustApplySynchronously
            ) 
            : base(packageToDecorate, mustApplySynchronously)
        {
            Assumption.AssertNotNull(httpRequestMessage, nameof(httpRequestMessage));
            Assumption.AssertNotNull(rollbarConfig, nameof(rollbarConfig));

            this._httpRequestMessage = httpRequestMessage;
            this._rollbarConfig = rollbarConfig;
            this._arbitraryKeyValuePairs = arbitraryKeyValuePairs;
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

            if(this._httpRequestMessage == null)
            {
                return; // there is nothing to decorate with...
            }


            if (rollbarData.Request == null)
            {
                rollbarData.Request = new Request(this._arbitraryKeyValuePairs);
            }

            rollbarData.Request.Url = this._httpRequestMessage.RequestUri?.AbsoluteUri;
            rollbarData.Request.QueryString = this._httpRequestMessage.RequestUri?.Query;
            rollbarData.Request.Params = null;

            rollbarData.Request.Headers = new Dictionary<string, string>(this._httpRequestMessage.Headers.Count());
            foreach (var header in this._httpRequestMessage.Headers)
            {
                rollbarData.Request.Headers.Add(header.Key, StringUtility.Combine(header.Value, ", "));
            }

            rollbarData.Request.Method = this._httpRequestMessage.Method.Method;
            switch (rollbarData.Request.Method.ToUpperInvariant())
            {
                case "POST":
                    var task = this._httpRequestMessage.Content?.ReadAsStringAsync();
                    task?.Wait();
                    rollbarData.Request.PostBody = task?.Result;
                    rollbarData.Request.PostParams = null;
                    break;
                case "GET":
                    rollbarData.Request.GetParams = null;
                    break;
                default:
                    traceSource.TraceInformation(
                        $"No-op processing {rollbarData.Request.Method.ToUpperInvariant()} HTTP method."
                        );
                    break;
            }

#if (NETFX)
            if (this._rollbarConfig == null)
            {
                return;
            }

            string? userIP = null;
            const string HttpContextProperty = "MS_HttpContext";
            const string RemoteEndpointMessagePropery = "System.ServiceModel.Channels.RemoteEndpointMessageProperty";
            if (this._httpRequestMessage.Properties.ContainsKey(HttpContextProperty))
            {
                HttpContextBase? ctx = this._httpRequestMessage.Properties[HttpContextProperty] as HttpContextBase;
                if (ctx != null)
                {
                    userIP = ctx.Request.UserHostAddress;
                }
            }
            else if (this._httpRequestMessage.Properties.ContainsKey(RemoteEndpointMessagePropery))
            {
                RemoteEndpointMessageProperty? remoteEndpoint =
                    this._httpRequestMessage.Properties[RemoteEndpointMessagePropery] as RemoteEndpointMessageProperty;
                if (remoteEndpoint != null)
                {
                    userIP = remoteEndpoint.Address;
                }
            }

            rollbarData.Request.UserIp =
                HttpRequestMessagePackageDecorator.DecideCollectableUserIPValue(userIP, this._rollbarConfig.RollbarDataSecurityOptions.IpAddressCollectionPolicy);
#endif

        }

        /// <summary>
        /// Decides whether to collect user ip value or not.
        /// </summary>
        /// <param name="initialIPAddress">The initial ip address.</param>
        /// <param name="ipAddressCollectionPolicy">The ip address collection policy.</param>
        /// <returns>The IP value as System.String.</returns>
        private static string? DecideCollectableUserIPValue(
            string? initialIPAddress, 
            IpAddressCollectionPolicy ipAddressCollectionPolicy
            )
        {
            if(string.IsNullOrWhiteSpace(initialIPAddress))
            {
                return null;
            }

            switch (ipAddressCollectionPolicy)
            {
                case IpAddressCollectionPolicy.Collect:
                    return initialIPAddress;
                case IpAddressCollectionPolicy.CollectAnonymized:
                    return IpAddressUtility.Anonymize(initialIPAddress!);
                case IpAddressCollectionPolicy.DoNotCollect:
                    return null;
                default:
                    Assumption.FailValidation(
                        "Unexpected enumeration value!"
                        , nameof(ipAddressCollectionPolicy)
                        );
                    break;
            }

            return null;
        }

    }
}
