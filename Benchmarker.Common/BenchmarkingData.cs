namespace Benchmarker.Common
{
    using Rollbar.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal static class BenchmarkingData
    {

        public static readonly Exception Exception_Small = BenchmarkingData.ProvideException(PayloadSize.Small);
        public static readonly Exception Exception_Medium = BenchmarkingData.ProvideException(PayloadSize.Medium);
        public static readonly Exception Exception_Large = BenchmarkingData.ProvideException(PayloadSize.Large);

        public static readonly string Message_Small = BenchmarkingData.ProvideMessage(PayloadSize.Small);
        public static readonly string Message_Medium = BenchmarkingData.ProvideMessage(PayloadSize.Medium);
        public static readonly string Message_Large = BenchmarkingData.ProvideMessage(PayloadSize.Large);

        static BenchmarkingData()
        {
        }

        private static string ProvideMessage(PayloadSize payloadSize)
        {
            string smallMessage = @"Small message 1234 56789! ";
            StringBuilder result = new StringBuilder(smallMessage);

            int multiplier = 1;
            switch (payloadSize)
            {
                case PayloadSize.Small:
                    multiplier = 1;
                    break;
                case PayloadSize.Medium:
                    multiplier = 1 * BenchmarkingConstants.SmallToMediumMessageMultiplier;
                    break;
                case PayloadSize.Large:
                    multiplier = 1 * BenchmarkingConstants.SmallToMediumMessageMultiplier * BenchmarkingConstants.MediumToLargeMessageMultiplier;
                    break;
                default:
                    Assumption.FailValidation("Unexpected value!", nameof(payloadSize));
                    break;
            }

            int counter = 0;
            while (++counter < multiplier)
            {
                result.AppendLine(smallMessage);
            }

            return result.ToString();
        }

        private static Exception ProvideException(PayloadSize payloadSize)
        {
            const int totalExceptionFramesBaseline = 3;
            const int initialMultiplier = 1;

            int multiplier = 1;
            switch (payloadSize)
            {
                case PayloadSize.Small:
                    multiplier = initialMultiplier;
                    break;
                case PayloadSize.Medium:
                    multiplier = initialMultiplier 
                        * BenchmarkingConstants.SmallToMediumExceptionCallStackDepthMultiplier
                        ;
                    break;
                case PayloadSize.Large:
                    multiplier = initialMultiplier 
                        * BenchmarkingConstants.SmallToMediumExceptionCallStackDepthMultiplier 
                        * BenchmarkingConstants.MediumToLargeExceptionCallStackDepthMultiplier
                        ;
                    break;
                default:
                    Assumption.FailValidation("Unexpected value!", nameof(payloadSize));
                    break;
            }

            return ExceptionSimulator.GetExceptionWith(multiplier * totalExceptionFramesBaseline);
        }
    }
}
