namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Rollbar.Common;
    using Rollbar.DTOs;

    public class RollbarPayloadManipulationOptions
        : ReconfigurableBase<RollbarPayloadManipulationOptions, IRollbarPayloadManipulationOptions>
        , IRollbarPayloadManipulationOptions
    {
        public RollbarPayloadManipulationOptions(
            Action<Payload> transform = null, 
            Action<Payload> truncate = null, 
            Func<Payload, bool> checkIgnore = null)
        {
            Transform = transform;
            Truncate = truncate;
            CheckIgnore = checkIgnore;
        }

        public Action<Payload> Transform
        {
            get;
            set;
        }
        public Action<Payload> Truncate
        {
            get;
            set;
        }
        public Func<Payload, bool> CheckIgnore
        {
            get;
            set;
        }

        public IRollbarPayloadManipulationOptions Reconfigure(IRollbarPayloadManipulationOptions likeMe)
        {
            return base.Reconfigure(likeMe);
        }

        public override Validator GetValidator()
        {
            return null;
        }

        public override string TraceAsString(string indent)
        {
            return base.TraceAsString(indent);
        }
    }
}
