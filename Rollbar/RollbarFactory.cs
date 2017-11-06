using System;
using System.Collections.Generic;
using System.Text;

namespace Rollbar
{
    public static class RollbarFactory
    {
        public static IRollbar CreateNew()
        {
            return new RollbarLogger();
        }
    }
}
