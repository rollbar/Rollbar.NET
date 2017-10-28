namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Singleton-like locator of the single shared instance of IRollbar component.
    /// </summary>
    public class RollbarLocator
    {
        public static IRollbar RollbarInstance
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
