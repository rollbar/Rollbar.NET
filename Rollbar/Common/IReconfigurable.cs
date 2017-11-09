using System;
using System.Collections.Generic;
using System.Text;

namespace Rollbar.Common
{
    public interface IReconfigurable<T>
    {
        T Reconfigure(T likeMe);
        event EventHandler Reconfigured;
    }
}
