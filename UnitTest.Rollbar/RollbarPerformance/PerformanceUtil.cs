namespace UnitTest.Rollbar.RollbarPerformance
{
    using global::Rollbar.Classification;
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
    }
}
