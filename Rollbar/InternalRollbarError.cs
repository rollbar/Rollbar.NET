namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public enum InternalRollbarError
    {
        GeneralError,
        PackagingError,
        BundlingError,
        EnqueuingError,
    }
}
