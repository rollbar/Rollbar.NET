namespace Benchmarker.Common
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal static class BenchmarkingConstants
    {
        public const int SmallToMediumMessageMultiplier = 10;
        public const int MediumToLargeMessageMultiplier = 20;
        public const int SmallToMediumExceptionCallStackDepthMultiplier = 5;
        public const int MediumToLargeExceptionCallStackDepthMultiplier = 15;

        //public static readonly TimeSpan RollbarBlockingTimeout = TimeSpan.FromSeconds(5);
    }
}
