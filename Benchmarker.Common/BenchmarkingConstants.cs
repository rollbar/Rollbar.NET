namespace Benchmarker.Common
{
    using System;

    internal static class BenchmarkingConstants
    {
        public const int SmallToMediumMessageMultiplier = 10;
        public const int MediumToLargeMessageMultiplier = 20;
        public const int SmallToMediumExceptionCallStackDepthMultiplier = 5;
        public const int MediumToLargeExceptionCallStackDepthMultiplier = 15;

        public static readonly TimeSpan RollbarBlockingTimeout = TimeSpan.FromSeconds(5);
    }
}
