namespace UnitTest.Rollbar.RollbarPerformance
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class Constants
    {
        public const string RollbarAccessToken = "17965fa5041749b6bf7095a190001ded";
        public const string RollbarEnvironment = "RollbarPerformanceEvaluation";

        public static readonly TimeSpan RollbarBlockingTimeout = TimeSpan.FromSeconds(5);

        public const int TotalMeasurementSamples = 20; // how many time to perform each measurement...
        public const int SmallToMediumMessageMultiplier = 10;
        public const int MediumToLargeMessageMultiplier = 20;
        public const int SmallToMediumExceptionCallStackDepthMultiplier = 5;
        public const int MediumToLargeExceptionCallStackDepthMultiplier = 15;

    }
}
