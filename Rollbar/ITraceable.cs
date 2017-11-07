namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface ITraceable
    {
        string TraceAsString(string indent = "");
    }
}
