#if NETFX

namespace Rollbar.AspNet.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Hosting;
    using Rollbar.Common;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;

    /// <summary>
    /// Class HttpRequestPackagingStrategyDecorator.
    /// Implements the <see cref="Rollbar.RollbarPackagingStrategyDecoratorBase" /></summary>
    /// <seealso cref="Rollbar.RollbarPackagingStrategyDecoratorBase" />
    public class HttpRequestPackagingStrategyDecorator
                : RollbarPackagingStrategyDecoratorBase
    {

        /// <summary>
        /// The HTTP request
        /// </summary>
        private readonly HttpRequestBase _httpRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestPackagingStrategyDecorator" /> class.
        /// </summary>
        /// <param name="strategyToDecorate">The strategy to decorate.</param>
        /// <param name="httpRequest">The HTTP request.</param>
        public HttpRequestPackagingStrategyDecorator(IRollbarPackagingStrategy strategyToDecorate, HttpRequestBase httpRequest)
                    : base(strategyToDecorate, false)
        {
            Assumption.AssertNotNull(httpRequest, nameof(httpRequest));

            this._httpRequest = httpRequest;
        }

        /// <summary>
        /// Packages as rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        public override Data PackageAsRollbarData()
        {
            Data rollbarData = base.PackageAsRollbarData();

            // try harvesting Request DTO info:
            ///////////////////////////////////
            
            var request = new Request();

            request.Url = this._httpRequest.Url.ToString();
            request.Method = this._httpRequest.HttpMethod;
            request.Headers = this._httpRequest.Headers?.ToStringDictionary();
            request.GetParams = this._httpRequest.QueryString?.ToObjectDictionary();
            request.PostParams = this._httpRequest.Unvalidated?.Form.ToObjectDictionary();

            // add posted files to the post collection
            try
            {
                if (this._httpRequest.Files?.Count > 0)
                {
                    foreach (var file in this._httpRequest.Files.AllKeys.ToDictionary(k => k, k => this._httpRequest.Files[k].RenderAsString()))
                    {
                        request.PostParams.Add(file.Key, "FILE: " + file.Value);
                    }
                }

            }
            catch (HttpException)
            {
                // Files from request could not be read here because they are streamed
                // and have been read earlier by e.g. WCF Rest Service or Open RIA Services
            }

            // if the X-Forwarded-For header exists, use that as the user's IP.
            // that will be the true remote IP of a user behind a proxy server or load balancer
            var forwardedFor = this._httpRequest.Headers["X-Forwarded-For"];
            if (!string.IsNullOrEmpty(forwardedFor) && forwardedFor.Contains(","))
            {
                forwardedFor = forwardedFor.Split(',').Last().Trim();
            }
            request.UserIp = forwardedFor ?? this._httpRequest.UserHostAddress;

            request.Params = this._httpRequest.RequestContext.RouteData.Values.ToDictionary(v => v.Key, v => v.Value.ToString()).ToObjectDictionary();

            // try harvesting custom HTTP session info:
            try
            {
                var httpSession = this._httpRequest.RequestContext?.HttpContext?.Session;
                if (httpSession != null)
                {
                    request["session"] = httpSession.Keys
                        .Cast<string>()
                        .Where(key => key != null)
                        .ToDictionary(key => key, key => httpSession[key].RenderAsString());
                }
            }
            catch
            {
                // Session state may not be configured.
            }

            rollbarData.Request = request;

            // let's try to get any useful data from the HTTP request's server variables:
            /////////////////////////////////////////////////////////////////////////////

            var serverVariables = this._httpRequest?.ServerVariables;
            if (serverVariables != null)
            {
                // try harvesting Person DTO info:
                //////////////////////////////////
                string username =
                    serverVariables["AUTH_USER"] ??
                    serverVariables["LOGON_USER"] ??
                    serverVariables["REMOTE_USER"];
                if (!string.IsNullOrWhiteSpace(username))
                {
                    rollbarData.Person = new Person(username)
                    {
                        UserName = username,
                    };
                }

                // try harvesting Server DTO info:
                //////////////////////////////////
                var host = serverVariables.Get("HTTP_HOST");

                if (string.IsNullOrEmpty(host))
                    host = serverVariables.Get("SERVER_NAME");

                var root = serverVariables.Get("APPL_PHYSICAL_PATH");

                if (string.IsNullOrEmpty(root))
                    root = HttpRuntime.AppDomainAppPath ?? Environment.CurrentDirectory;

                var machine = Environment.MachineName;
                var webServer = serverVariables["SERVER_SOFTWARE"];

                if (!string.IsNullOrWhiteSpace(host)
                    || !string.IsNullOrWhiteSpace(root)
                    || !string.IsNullOrWhiteSpace(machine)
                    || !string.IsNullOrWhiteSpace(webServer)
                    )
                {
                    rollbarData.Server = new Server { Host = host, Root = root, };
                    rollbarData.Server["host_machine"] = machine;
                    rollbarData.Server["host_webserver"] = webServer;
                }
            }
            else if (!HostingEnvironment.IsHosted)
            {
                // try harvesting Person DTO info (from Environment):
                /////////////////////////////////////////////////////
                rollbarData.Person = new Person($"{Environment.MachineName}\\{Environment.UserName}")
                {
                    UserName = Environment.UserName,
                };
            }

            // return the now enriched Data DTO:
            return rollbarData;
        }
    }
}

#endif
