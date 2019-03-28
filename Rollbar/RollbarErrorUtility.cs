namespace Rollbar
{
    using System;

    /// <summary>
    /// Class RollbarErrorUtility.
    /// </summary>
    internal static class RollbarErrorUtility
    {
        /// <summary>
        /// Reports the specified rollbar logger.
        /// </summary>
        /// <param name="rollbarLogger">The rollbar logger.</param>
        /// <param name="dataObject">The data object.</param>
        /// <param name="rollbarError">The rollbar error.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public static void Report(
            RollbarLogger rollbarLogger, 
            object dataObject, 
            InternalRollbarError rollbarError, 
            string message, 
            Exception exception
            )
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
