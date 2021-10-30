namespace UnitTest.Rollbar.RollbarPerformance
{
    using global::Rollbar.Classification;
    using global::Rollbar.Instrumentation;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class PerformanceUtil
    {
        public static Classification GetClassification(ClassificationDeclaration classificationDeclaration)
        {
            var classification = Classification.MatchClassification(classificationDeclaration.GetAllClassifiers());
            return classification;
        }

        public static PerformanceTimerAttribute GetPerformanceTimer(ClassificationDeclaration classificationDeclaration)
        {
            var timer = PerformanceTimerAttribute.StartNew(
                RollbarPerformanceMonitor.Instance,
                PerformanceUtil.GetClassification(classificationDeclaration)
                );
            return timer;
        }

    }
}
