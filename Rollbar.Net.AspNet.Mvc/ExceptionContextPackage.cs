namespace Rollbar.Net.AspNet.Mvc
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
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;

    /// <summary>
    /// Class ExceptionContextPackage.
    /// Implements the <see cref="Rollbar.RollbarPackageBase" /></summary>
    /// <seealso cref="Rollbar.RollbarPackageBase" />
    public class ExceptionContextPackage
            : RollbarPackageBase
    {
        /// <summary>
        /// The exception context
        /// </summary>
        private readonly ExceptionContext _exceptionContext;

        private readonly string _message;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionContextPackage" /> class.
        /// </summary>
        /// <param name="exceptionContext">The exception context.</param>
        public ExceptionContextPackage(ExceptionContext exceptionContext)
            : this(exceptionContext, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionContextPackage" /> class.
        /// </summary>
        /// <param name="exceptionContext">The exception context.</param>
        /// <param name="message">The message.</param>
        public ExceptionContextPackage(ExceptionContext exceptionContext, string message)
            : base(false)
        {
            this._exceptionContext = exceptionContext;
            this._message = message;
        }

        /// <summary>
        /// Produces the rollbar data.
        /// </summary>
        /// <returns>Rollbar Data DTO or null (if packaging is not applicable in some cases).</returns>
        protected override Data ProduceRollbarData()
        {
            if (this._exceptionContext == null)
            {
                return null;
            }

            // let's use composition of available strategies:    

            IRollbarPackage packagingStrategy = new ExceptionPackage(this._exceptionContext.Exception, this._message);

            if (this._exceptionContext?.RequestContext?.HttpContext != null)
            {
                packagingStrategy = new HttpContextPackageDecorator(packagingStrategy, this._exceptionContext.RequestContext.HttpContext);
            }

            //var httpRequest = this._exceptionContext?.RequestContext?.HttpContext?.Request;
            //if (httpRequest != null)
            //{
            //    packagingStrategy = new HttpRequestPackageDecorator(packagingStrategy, httpRequest);
            //}
            //var httpResponse = this._exceptionContext?.RequestContext?.HttpContext?.Response;
            //if (httpResponse != null)
            //{
            //    packagingStrategy = new HttpResponsePackageDecorator(packagingStrategy, httpResponse);
            //}

            Data rollbarData = packagingStrategy.PackageAsRollbarData();

            return rollbarData;
        }
    }
}
