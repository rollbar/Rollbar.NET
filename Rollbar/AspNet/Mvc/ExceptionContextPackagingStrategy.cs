#if NETFX

namespace Rollbar.AspNet.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using Rollbar.Common;
    using Rollbar.DTOs;

    /// <summary>
    /// Class ExceptionContextPackagingStrategy.
    /// Implements the <see cref="Rollbar.RollbarPackagingStrategyBase" /></summary>
    /// <seealso cref="Rollbar.RollbarPackagingStrategyBase" />
    public class ExceptionContextPackagingStrategy
            : RollbarPackagingStrategyBase
    {
        /// <summary>
        /// The exception context
        /// </summary>
        private readonly ExceptionContext _exceptionContext;

        private readonly string _message;

        /// <summary>
        /// Prevents a default instance of the <see cref="ExceptionContextPackagingStrategy" /> class from being created.
        /// </summary>
        private ExceptionContextPackagingStrategy()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionContextPackagingStrategy" /> class.
        /// </summary>
        /// <param name="exceptionContext">The exception context.</param>
        public ExceptionContextPackagingStrategy(ExceptionContext exceptionContext)
            : this(exceptionContext, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionContextPackagingStrategy" /> class.
        /// </summary>
        /// <param name="exceptionContext">The exception context.</param>
        /// <param name="message">The message.</param>
        public ExceptionContextPackagingStrategy(ExceptionContext exceptionContext, string message)
        {
            this._exceptionContext = exceptionContext;
            this._message = message;
        }

        /// <summary>
        /// Packages as rollbar data.
        /// </summary>
        /// <returns>Rollbar.DTOs.Data.</returns>
        public override Data PackageAsRollbarData()
        {
            if (this._exceptionContext == null)
            {
                return null;
            }

            Body rollbarBody = new Body(this._exceptionContext.Exception);
            IDictionary<string, object> custom = null;
            RollbarUtility.SnapExceptionDataAsCustomData(this._exceptionContext.Exception, ref custom);
            Data rollbarData = new Data(rollbarBody, custom);
            rollbarData.Title = this._message;

            var httpRequest = this._exceptionContext?.RequestContext?.HttpContext?.Request;
            if (httpRequest != null)
            {
                // try harvesting Request DTO info:
                var request = new Request();

                request.Url = httpRequest.Url.ToString();
                request.Method = httpRequest.HttpMethod;
                request.Headers = httpRequest.Headers?.ToStringDictionary();
                request.GetParams = httpRequest.QueryString?.ToObjectDictionary();
                request.PostParams = httpRequest.Unvalidated?.Form.ToObjectDictionary();

                // add posted files to the post collection
                try
                {
                    if (httpRequest.Files?.Count > 0)
                    {
                        foreach (var file in httpRequest.Files.AllKeys.ToDictionary(k => k, k => httpRequest.Files[k].RenderAsString()))
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
                var forwardedFor = httpRequest.Headers["X-Forwarded-For"];
                if (!string.IsNullOrEmpty(forwardedFor) && forwardedFor.Contains(","))
                {
                    forwardedFor = forwardedFor.Split(',').Last().Trim();
                }
                request.UserIp = forwardedFor ?? httpRequest.UserHostAddress;

                request.Params = httpRequest.RequestContext.RouteData.Values.ToDictionary(v => v.Key, v => v.Value.ToString()).ToObjectDictionary();

                rollbarData.Request = request;

                // try harvesting custom HTTP session info:
                var httpSession = httpRequest.RequestContext?.HttpContext?.Session;
                if (httpSession != null)
                {
                    request["session"] = httpSession.Keys
                        .Cast<string>()
                        .Where(key => key != null)
                        .ToDictionary(key => key, key => httpSession[key].RenderAsString());
                }
            }

            var serverVariables = httpRequest?.ServerVariables;
            if (serverVariables != null)
            {
                // try harvesting Person DTO info:
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
                rollbarData.Person = new Person($"{Environment.MachineName}\\{Environment.UserName}")
                {
                    UserName = Environment.UserName,
                };
            }

            return rollbarData;
        }
    }
}

#endif
