namespace Rollbar.NetCore.AspNet
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    public class RollbarMiddlewareException
        : Exception
    {
        public RollbarMiddlewareException()
        {
        }

        public RollbarMiddlewareException(string message) 
            : base(message)
        {
        }

        public RollbarMiddlewareException(Exception exception)
            : this("The included internal exception processed by the Rollbar middleware", exception)
        {

        }

        public RollbarMiddlewareException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RollbarMiddlewareException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
