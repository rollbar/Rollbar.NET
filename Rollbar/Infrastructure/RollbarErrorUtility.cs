namespace Rollbar.Infrastructure
{
    using Rollbar.Common;
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
        /// <param name="errorCollector">The error collector.</param>
        public static void Report(
            IRollbar? rollbarLogger, 
            object? dataObject, 
            InternalRollbarError rollbarError, 
            string? message, 
            Exception? exception,
            IErrorCollector? errorCollector
            )
        {
            var rollbarException = new RollbarException(rollbarError, message ?? rollbarError.ToString(), exception);
            errorCollector?.Register(rollbarException);

            var rollbarEvent = new InternalErrorEventArgs(rollbarLogger, dataObject, rollbarException, rollbarException.Message);


            if (rollbarLogger is RollbarLogger specificRollbarLogger)
            {
                specificRollbarLogger.OnRollbarEvent(rollbarEvent);
            }
            else
            {
                RollbarQueueController.Instance?.OnRollbarEvent(rollbarEvent);
            }

        }
    }
}
