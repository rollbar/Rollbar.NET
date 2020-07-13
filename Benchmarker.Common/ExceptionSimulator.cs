namespace Benchmarker.Common
{
    using System;

    /// <summary>
    /// Class ExceptionSimulator.
    /// </summary>
    public static class ExceptionSimulator
    {
        /// <summary>
        /// Gets the exception chain of.
        /// </summary>
        /// <param name="totalExceptions">The total exceptions.</param>
        /// <returns>Exception.</returns>
        public static Exception GetExceptionChainOf(int totalExceptions)
        {
            if (--totalExceptions > 0)
            {
                return new ApplicationException(
                    "Outer exception #" + totalExceptions, 
                    GetExceptionChainOf(totalExceptions)
                    );
            }
            const int totalMostInnerExceptionFrames = 5;
            return GetExceptionWith(totalMostInnerExceptionFrames);
        }

        /// <summary>
        /// Gets the exception with.
        /// </summary>
        /// <param name="totalFrames">The total frames.</param>
        /// <param name="exceptionMessage">The exception message.</param>
        /// <returns>Exception.</returns>
        public static Exception GetExceptionWith(
            int totalFrames, 
            string exceptionMessage = "Default exception message."
            )
        {
            try
            {
                Method(totalFrames, exceptionMessage);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                return ex;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return null;
        }

        /// <summary>
        /// Methods the specified call depth.
        /// </summary>
        /// <param name="callDepth">The call depth.</param>
        /// <param name="exceptionMessage">The exception message.</param>
        private static void Method(int callDepth, string exceptionMessage)
        {
            while (--callDepth > 0)
            {
                Method(callDepth, exceptionMessage);
            }
            ExceptionalMethod(exceptionMessage);
        }

        /// <summary>
        /// Exceptionals the method.
        /// </summary>
        /// <param name="exceptionMessage">The exception message.</param>
        /// <exception cref="ApplicationException"></exception>
        private static void ExceptionalMethod(string exceptionMessage = "Some nasty application exception!")
        {
            throw new ApplicationException(exceptionMessage);
        }
    }
}
