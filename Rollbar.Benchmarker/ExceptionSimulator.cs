namespace Rollbar.Benchmarker
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class ExceptionSimulator
    {
        public static Exception GetExceptionChainOf(int totalExceptions)
        {
            while (--totalExceptions > 0)
            {
                return new ApplicationException("Outer exception #" + totalExceptions, GetExceptionChainOf(totalExceptions));
            }
            const int totalMostInnerExceptionFrames = 5;
            return GetExceptionWith(totalMostInnerExceptionFrames);
        }

        public static Exception GetExceptionWith(int totalFrames)
        {
            try
            {
                Method(totalFrames);
            }
            catch (Exception ex)
            {
                return ex;
            }

            return null;
        }

        private static void Method(int callDepth)
        {
            while (--callDepth > 0)
            {
                Method(callDepth);
            }
            ExceptionalMethod();
        }

        private static void ExceptionalMethod()
        {
            throw new ApplicationException("Some nasty application exception!");
        }
    }
}
