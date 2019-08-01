namespace Rollbar.Net.AspNet.Mvc
{
    using System.Web.Mvc;
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

            Data rollbarData = packagingStrategy.PackageAsRollbarData();

            return rollbarData;
        }
    }
}
