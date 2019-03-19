namespace Rollbar
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal static class RollbarErrorUtility
    {
        public static void Report(RollbarLogger rollbarLogger, object dataObject, InternalRollbarError rollbarError, string message, Exception exception)
        {
            var rollbarException = new RollbarException(rollbarError, message ?? rollbarError.ToString(), exception);
            var rollbarEvent = new InternalErrorEventArgs(rollbarLogger, dataObject, rollbarException, rollbarException.Message);

            if (rollbarLogger != null)
            {
                rollbarLogger.OnRollbarEvent(rollbarEvent);
            }
            else
            {
                RollbarQueueController.Instance.OnRollbarEvent(rollbarEvent);
            }

        }
    }
}
