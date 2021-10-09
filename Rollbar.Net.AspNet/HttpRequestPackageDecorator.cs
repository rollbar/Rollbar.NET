namespace Rollbar.Net.AspNet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Hosting;
    using Rollbar.Common;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;

    /// <summary>
    /// Class HttpRequestPackageDecorator.
    /// Implements the <see cref="Rollbar.RollbarPackageDecoratorBase" /></summary>
    /// <seealso cref="Rollbar.RollbarPackageDecoratorBase" />
    public class HttpRequestPackageDecorator
                : RollbarPackageDecoratorBase
    {

        /// <summary>
        /// The HTTP request
        /// </summary>
        private readonly HttpRequestBase _httpRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestPackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="httpRequest">The HTTP request.</param>
        public HttpRequestPackageDecorator(IRollbarPackage packageToDecorate, HttpRequest httpRequest)
            : this(packageToDecorate, new HttpRequestWrapper(httpRequest), false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestPackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> [must apply synchronously].</param>
        public HttpRequestPackageDecorator(IRollbarPackage packageToDecorate, HttpRequest httpRequest, bool mustApplySynchronously)
                    : this(packageToDecorate, new HttpRequestWrapper(httpRequest), mustApplySynchronously)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestPackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="httpRequest">The HTTP request.</param>
        public HttpRequestPackageDecorator(IRollbarPackage packageToDecorate, HttpRequestBase httpRequest)
            : this(packageToDecorate, httpRequest, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestPackageDecorator"/> class.
        /// </summary>
        /// <param name="packageToDecorate">The package to decorate.</param>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <param name="mustApplySynchronously">if set to <c>true</c> [must apply synchronously].</param>
        public HttpRequestPackageDecorator(IRollbarPackage packageToDecorate, HttpRequestBase httpRequest, bool mustApplySynchronously)
                    : base(packageToDecorate, mustApplySynchronously)
        {
            Assumption.AssertNotNull(httpRequest, nameof(httpRequest));

            this._httpRequest = httpRequest;
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

            if(this._httpRequest == null || rollbarData == null)
            {
                return; //nothing to do...
            }

            // try harvesting Request DTO info:
            ///////////////////////////////////

            Request request = new();
            request.Url = this._httpRequest.Url?.ToString();
            request.Method = this._httpRequest.HttpMethod;
            request.Headers = this._httpRequest.Headers?.ToCompactStringDictionary();

            try
            {
                request.GetParams = this._httpRequest.QueryString?.ToCompactObjectDictionary();
            }
            catch
            {
                // calls in try-block may throw an exception. we are just trying our best...
            }
            try
            {
                request.PostParams = this._httpRequest.Unvalidated?.Form?.ToCompactObjectDictionary();
            }
            catch
            {
                // calls in try-block may throw an exception. we are just trying our best...
            }

            // add posted files to the post collection
            try
            {
                if (this._httpRequest.Files?.Count > 0)
                {
                    var files = 
                        this._httpRequest.Files.AllKeys.ToDictionary(k => k, k => this._httpRequest.Files[k].RenderAsString());
                    if (request.PostParams == null)
                    {
                        request.PostParams = new Dictionary<string, object>(files.Count);
                    }
                    foreach (var file in files)
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
            var forwardedFor = this._httpRequest.Headers?["X-Forwarded-For"];
            if (!string.IsNullOrEmpty(forwardedFor) && forwardedFor!.Contains(","))
            {
                forwardedFor = forwardedFor.Split(',').Last().Trim();
            }

            try
            {
                request.UserIp = 
                    forwardedFor ?? this._httpRequest.UserHostAddress;
            }
            catch
            {
                // calls in try-block may throw an exception. we are just trying our best...
            }

            try
            {
                request.Params = 
                    this._httpRequest.RequestContext.RouteData.Values
                    .ToDictionary(v => v.Key, v => v.Value.ToString()).ToObjectDictionary();
            }
            catch
            {
                // calls in try-block may throw an exception. we are just trying our best...
            }

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

            try
            {
                var serverVariables = this._httpRequest?.ServerVariables;
                if (serverVariables != null)
                {
                    // try harvesting Person DTO info:
                    //////////////////////////////////
                    string[] serverVarsOfInterest = new[]
                    {
                        "AUTH_USER",
                        "LOGON_USER",
                        "REMOTE_USER",
                    };
                    string? username = null;
                    foreach (var serverVar in serverVarsOfInterest)
                    {
                        try
                        {
                            username = serverVariables[serverVar];
                        }
                        catch
                        {
                            // calls in try-block may throw an exception. we are just trying our best...
                        }
                        if (!string.IsNullOrWhiteSpace(username))
                        {
                            break;
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(username))
                    {
                        rollbarData.Person = new Person(username!)
                        {
                            UserName = username!,
                        };
                    }

                    // try harvesting Server DTO info:
                    //////////////////////////////////
                    var host = serverVariables.Get("HTTP_HOST");

                    if (string.IsNullOrEmpty(host))
                    {
                        host = serverVariables.Get("SERVER_NAME");
                    }

                    var root = serverVariables.Get("APPL_PHYSICAL_PATH");

                    if (string.IsNullOrEmpty(root))
                    {
                        root = HttpRuntime.AppDomainAppPath ?? Environment.CurrentDirectory;
                    }

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
            }
            catch
            {
                // calls in try-block may throw an exception. we are just trying our best...
            }
        }
    }
}
