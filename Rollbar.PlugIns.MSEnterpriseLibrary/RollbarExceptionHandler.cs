namespace Rollbar.PlugIns.MSEnterpriseLibrary
{
    using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
    using System;
    using System.Collections.Generic;

#pragma warning disable CS1584 // XML comment has syntactically incorrect cref attribute
#pragma warning disable CS1658 // Warning is overriding an error
    /// <summary>
    /// Class RollbarExceptionHandler.
    /// Implements the <see cref="Rollbar.PlugIns.PlugInCore{System.Exception, System.Exception}" />
    /// Implements the <see cref="Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.IExceptionHandler" />
    /// </summary>
    /// <seealso cref="Rollbar.PlugIns.PlugInCore{System.Exception, System.Exception}" />
    /// <seealso cref="Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.IExceptionHandler" />
#pragma warning restore CS1584 // XML comment has syntactically incorrect cref attribute
#pragma warning restore CS1658 // Warning is overriding an error
    public class RollbarExceptionHandler
        : PlugInCore<Exception, Exception>
        , IExceptionHandler
    {
        /// <summary>
        /// The custom prefix
        /// </summary>
        private const string customPrefix = "msEntLib";

        /// <summary>
        /// The rollbar error level by plug-in error level
        /// </summary>
        private static readonly IDictionary<Exception, ErrorLevel> rollbarErrorLevelByPlugInErrorLevel = new Dictionary<Exception, ErrorLevel>
        {
            // empty mapping in this scenario...
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarExceptionHandler" /> class.
        /// </summary>
        /// <param name="rollbarAccessToken">The rollbar access token.</param>
        /// <param name="rollbarEnvironment">The rollbar environment.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The rollbar blocking logging timeout.</param>
        public RollbarExceptionHandler(
            string rollbarAccessToken,
            string rollbarEnvironment,
            TimeSpan? rollbarBlockingLoggingTimeout
            )
            : this(
                  CreateConfig(rollbarAccessToken: rollbarAccessToken, rollbarEnvironment: rollbarEnvironment),
                  rollbarBlockingLoggingTimeout
                  )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarExceptionHandler" /> class.
        /// </summary>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="rollbarBlockingTimeout">The rollbar blocking timeout.</param>
        public RollbarExceptionHandler(
            IRollbarConfig rollbarConfig,
            TimeSpan? rollbarBlockingTimeout
            )
            : base(rollbarErrorLevelByPlugInErrorLevel, customPrefix, rollbarConfig, rollbarBlockingTimeout)
        {
        }

        /// <summary>
        /// Extracts the message for a Rollbar payload from the incoming plug-in's data event.
        /// </summary>
        /// <param name="plugInEventData">The plug in event data.</param>
        /// <returns>System.String.</returns>
        protected override string ExtractMessage(Exception plugInEventData)
        {
            return plugInEventData.Message;
        }

        /// <summary>
        /// Extracts the exception for a Rollbar payload from the incoming plug-in's data event.
        /// </summary>
        /// <param name="plugInEventData">The plug in event data.</param>
        /// <returns>Exception.</returns>
        protected override Exception ExtractException(Exception plugInEventData)
        {
            return plugInEventData;
        }

        /// <summary>
        /// Extracts the custom properties  for a Rollbar payload from the incoming plug-in's data event.
        /// </summary>
        /// <param name="plugInEventData">The plug in event data.</param>
        /// <returns>Usually, either a data structure or a key-value dictionary returned as a System.Object.</returns>
        protected override object ExtractCustomProperties(Exception plugInEventData)
        {
            return null;
        }

        /// <summary>
        /// Translates the specified plug-in error level to Rollbar ErrorLevel.
        /// </summary>
        /// <param name="plugInErrorLevel">The plug-in error level.</param>
        /// <returns>ErrorLevel.</returns>
        protected override ErrorLevel Translate(Exception plugInErrorLevel)
        {
            return ErrorLevel.Error;
        }

        /// <summary>
        /// When implemented by a class, handles an <see cref="T:System.Exception" />.
        /// </summary>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="handlingInstanceId">The unique ID attached to the handling chain for this handling instance.</param>
        /// <returns>Modified exception to pass to the next exceptionHandlerData in the chain.</returns>
        public Exception HandleException(Exception exception, Guid handlingInstanceId)
        {
            base.ReportToRollbar(exception, exception);
            return exception;
        }

    }
}
