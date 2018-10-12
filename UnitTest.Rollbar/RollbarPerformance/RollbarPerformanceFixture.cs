#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.RollbarPerformance
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Diagnostics;

    using global::Rollbar;
    using global::Rollbar.Instrumentation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Net.Http;
    using System.Threading;
    using global::Rollbar.Classification;
    using global::Rollbar.Common;

    [TestClass]
    [TestCategory(nameof(RollbarPerformanceFixture))]
    public class RollbarPerformanceFixture
    {
        [TestInitialize]
        public void SetupFixture()
        {
            //SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [TestCleanup]
        public void TearDownFixture()
        {

        }

        [TestMethod]
        public void EvaluatePerformance()
        {
            ClassificationDeclaration classificationDeclaration = new ClassificationDeclaration();
            foreach(var classifier in EnumUtil.GetAllValues<PayloadSize>())
            {
                EvaluateUsing(classifier, classificationDeclaration);
            }
        }

        private void EvaluateUsing(PayloadSize theClassifier, ClassificationDeclaration classificationDeclaration)
        {
            classificationDeclaration.PayloadSize = theClassifier;
            foreach (var classifier in EnumUtil.GetAllValues<PayloadType>())
            {
                EvaluateUsing(classifier, classificationDeclaration);
            }
        }

        private void EvaluateUsing(PayloadType theClassifier, ClassificationDeclaration classificationDeclaration)
        {
            classificationDeclaration.PayloadType = theClassifier;
            foreach (var classifier in EnumUtil.GetAllValues<MethodVariant>())
            {
                EvaluateUsing(classifier, classificationDeclaration);
            }
        }

        private void EvaluateUsing(MethodVariant theClassifier, ClassificationDeclaration classificationDeclaration)
        {
            classificationDeclaration.MethodVariant = theClassifier;
            foreach (var classifier in EnumUtil.GetAllValues<Method>())
            {
                EvaluateUsing(classifier, classificationDeclaration);
            }
        }

        private void EvaluateUsing(Method theClassifier, ClassificationDeclaration classificationDeclaration)
        {
            classificationDeclaration.Method = theClassifier;

            //TODO:
        }
    }
}
