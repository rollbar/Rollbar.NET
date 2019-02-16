#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;

    [TestClass]
    [TestCategory(nameof(RollbarPackagingStrategyFixture))]
    public class RollbarPackagingStrategyFixture
    {
        [TestInitialize]
        public void SetupFixture()
        {
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void ExceptionPackagingStrategyTest()
        {
            const string rollbarDataTitle = "You have some coding to do...";
            const string exceptionMessage = "Forgotten method";
            System.Exception exception = new NotImplementedException(exceptionMessage);

            IRollbarPackagingStrategy packagingStrategy = 
                new ExceptionPackagingStrategy(exception, rollbarDataTitle);

            Assert.IsFalse(packagingStrategy.MustApplySynchronously, "Expected to be an async strategy!");

            Data rollbarData = packagingStrategy.PackageAsRollbarData();

            Assert.AreEqual(rollbarDataTitle, rollbarData.Title,  "Data title is properly set!");
            Assert.IsNotNull(rollbarData.Body);
            Assert.IsNotNull(rollbarData.Body.Trace);
            Assert.IsNull(rollbarData.Body.Message);
            Assert.IsNull(rollbarData.Body.TraceChain);
            Assert.IsNull(rollbarData.Body.CrashReport);

            Assert.IsNotNull(rollbarData.Body.Trace.Exception);
            Assert.AreEqual(exceptionMessage, rollbarData.Body.Trace.Exception.Message);
            Assert.AreEqual(exception.GetType().FullName, rollbarData.Body.Trace.Exception.Class);
        }

        [TestMethod]
        public void OuterExceptionPackagingStrategyTest()
        {
            const string rollbarDataTitle = "You have some coding to do...";
            const string innerExceptionMessage = "Forgotten method";
            System.Exception innerException = new NotImplementedException(innerExceptionMessage);
            const string exceptionMessage = "Application level exception";
            System.Exception exception = new ApplicationException(exceptionMessage, innerException);

            IRollbarPackagingStrategy packagingStrategy =
                new ExceptionPackagingStrategy(exception, rollbarDataTitle);

            Assert.IsFalse(packagingStrategy.MustApplySynchronously, "Expected to be an async strategy!");

            Data rollbarData = packagingStrategy.PackageAsRollbarData();

            Assert.AreEqual(rollbarDataTitle, rollbarData.Title, "Data title is properly set!");
            Assert.IsNotNull(rollbarData.Body);
            Assert.IsNotNull(rollbarData.Body.TraceChain);
            Assert.IsNull(rollbarData.Body.Trace);
            Assert.IsNull(rollbarData.Body.Message);
            Assert.IsNull(rollbarData.Body.CrashReport);

            Assert.AreEqual(2, rollbarData.Body.TraceChain.Length);

            Assert.IsNotNull(rollbarData.Body.TraceChain[0]);
            Assert.IsNotNull(rollbarData.Body.TraceChain[0].Exception);
            Assert.AreEqual(exceptionMessage, rollbarData.Body.TraceChain[0].Exception.Message);
            Assert.AreEqual(exception.GetType().FullName, rollbarData.Body.TraceChain[0].Exception.Class);

            Assert.IsNotNull(rollbarData.Body.TraceChain[1]);
            Assert.IsNotNull(rollbarData.Body.TraceChain[1].Exception);
            Assert.AreEqual(innerExceptionMessage, rollbarData.Body.TraceChain[1].Exception.Message);
            Assert.AreEqual(innerException.GetType().FullName, rollbarData.Body.TraceChain[1].Exception.Class);
        }

        [TestMethod]
        public void AggregateExceptionPackagingStrategyTest()
        {
            const string rollbarDataTitle = "You have some coding to do...";
            const string innerExceptionMessage1 = "Forgotten method";
            System.Exception innerException1 = new NotImplementedException(innerExceptionMessage1);
            const string innerExceptionMessage2 = "Forgotten null-check";
            System.Exception innerException2 = new NullReferenceException(innerExceptionMessage2);
            const string exceptionMessage = "Application level exception";
            System.Exception exception = new AggregateException(exceptionMessage, innerException1, innerException2);

            IRollbarPackagingStrategy packagingStrategy =
                new ExceptionPackagingStrategy(exception, rollbarDataTitle);

            Assert.IsFalse(packagingStrategy.MustApplySynchronously, "Expected to be an async strategy!");

            Data rollbarData = packagingStrategy.PackageAsRollbarData();

            Assert.AreEqual(rollbarDataTitle, rollbarData.Title, "Data title is properly set!");
            Assert.IsNotNull(rollbarData.Body);
            Assert.IsNotNull(rollbarData.Body.TraceChain);
            Assert.IsNull(rollbarData.Body.Trace);
            Assert.IsNull(rollbarData.Body.Message);
            Assert.IsNull(rollbarData.Body.CrashReport);

            Assert.AreEqual(2, rollbarData.Body.TraceChain.Length);

            Assert.IsNotNull(rollbarData.Body.TraceChain[0]);
            Assert.IsNotNull(rollbarData.Body.TraceChain[0].Exception);
            Assert.AreEqual(innerExceptionMessage1, rollbarData.Body.TraceChain[0].Exception.Message);
            Assert.AreEqual(innerException1.GetType().FullName, rollbarData.Body.TraceChain[0].Exception.Class);

            Assert.IsNotNull(rollbarData.Body.TraceChain[1]);
            Assert.IsNotNull(rollbarData.Body.TraceChain[1].Exception);
            Assert.AreEqual(innerExceptionMessage2, rollbarData.Body.TraceChain[1].Exception.Message);
            Assert.AreEqual(innerException2.GetType().FullName, rollbarData.Body.TraceChain[1].Exception.Class);
        }

        [TestMethod]
        public void MessagePackagingStrategyTest()
        {
            const string rollbarDataTitle = "Got a message...";
            const string message = "My message to report to Rollbar";
            IDictionary<string, object> extraInfo = new Dictionary<string, object>()
            {
                { "extra1", "Info 1"},
                { "extra2", "Info 2"},
            };

            IRollbarPackagingStrategy packagingStrategy =
                new MessagePackagingStrategy(message, rollbarDataTitle, extraInfo);

            Assert.IsFalse(packagingStrategy.MustApplySynchronously, "Expected to be an async strategy!");

            Data rollbarData = packagingStrategy.PackageAsRollbarData();

            Assert.AreEqual(rollbarDataTitle, rollbarData.Title, "Data title is properly set!");
            Assert.IsNotNull(rollbarData.Body);
            Assert.IsNotNull(rollbarData.Body.Message);
            Assert.IsNull(rollbarData.Body.Trace);
            Assert.IsNull(rollbarData.Body.TraceChain);
            Assert.IsNull(rollbarData.Body.CrashReport);

            Assert.AreEqual(3, rollbarData.Body.Message.Count);
            Assert.AreEqual(message, rollbarData.Body.Message.Body);
            Assert.IsTrue(rollbarData.Body.Message.ContainsKey("body"));
            Assert.IsTrue(rollbarData.Body.Message.ContainsKey("extra1"));
            Assert.IsTrue(rollbarData.Body.Message.ContainsKey("extra2"));
            Assert.AreEqual(message, rollbarData.Body.Message["body"]);
            Assert.AreEqual("Info 1", rollbarData.Body.Message["extra1"]);
            Assert.AreEqual("Info 2", rollbarData.Body.Message["extra2"]);
        }
    }
}
