#if NETFX

namespace Rollbar.AspNet.Mvc
{
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;

    public static class RollbarAssistant
    {
        public static Data CreateRollbarDataDto(ExceptionContext exceptionContext)
        {
            throw new NotImplementedException();

            Body rollbarBody = new Body(exceptionContext.Exception);
            
        }

        /// <summary>
        /// Gets the current HTTP request.
        /// </summary>
        /// <returns>System.Web.HttpRequest object or null.</returns>
        private static HttpRequest GetCurrentHttpRequestOrDefault()
        {
            HttpRequest httpRequest = null;

            try
            {
                httpRequest = HttpContext.Current?.Request;
            }
            catch (System.Exception exception)
            {
                System.Diagnostics.Trace.TraceError(
                    $"EXCEPTION while attempting to get current HTTP request object:{Environment.NewLine}{exception}"
                    );
            }

            return httpRequest;
        }
    }
}

#endif
