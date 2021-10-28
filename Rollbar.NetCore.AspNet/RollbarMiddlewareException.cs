namespace Rollbar.NetCore.AspNet
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Class RollbarMiddlewareException.
    /// Implements the <see cref="System.Exception" />
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class RollbarMiddlewareException
        : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarMiddlewareException"/> class.
        /// </summary>
        public RollbarMiddlewareException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarMiddlewareException"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public RollbarMiddlewareException(Exception exception)
            : this("The included internal exception processed by the Rollbar middleware", exception)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarMiddlewareException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RollbarMiddlewareException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarMiddlewareException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public RollbarMiddlewareException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarMiddlewareException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        protected RollbarMiddlewareException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
