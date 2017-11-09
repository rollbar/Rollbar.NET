using System;
using System.Collections.Generic;
using System.Text;

namespace Rollbar
{
    public static class RollbarFactory
    {
        public static IRollbar CreateNew()
        {
            return RollbarFactory.CreateNew(false);
        }

        internal static IRollbar CreateNew(bool isSingleton)
        {
            return new RollbarLogger(isSingleton);
        }
    }
}
