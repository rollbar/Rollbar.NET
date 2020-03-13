#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.PayloadPackageDecorators
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;

    [TestClass]
    [TestCategory(nameof(CustomKeyValuePackageDecoratorFixture))]
    public class CustomKeyValuePackageDecoratorFixture
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
        public void BasicTest()
        {
            const string rollbarDataTitle = "Let's test some strategy decoration...";
            const string exceptionMessage = "Someone forgot null-test!";
            System.Exception exception = new NullReferenceException(exceptionMessage);

            IRollbarPackage packagingStrategy =
                new ExceptionPackage(exception, rollbarDataTitle);

            Assert.IsFalse(packagingStrategy.MustApplySynchronously, "Expected to be an async strategy!");

            Data rollbarData = packagingStrategy.PackageAsRollbarData();

            Assert.AreEqual(rollbarDataTitle, rollbarData.Title, "Data title is properly set!");
            Assert.IsNotNull(rollbarData.Body);
            Assert.IsNotNull(rollbarData.Body.Trace);
            Assert.IsNull(rollbarData.Body.Message);
            Assert.IsNull(rollbarData.Body.TraceChain);
            Assert.IsNull(rollbarData.Body.CrashReport);

            Assert.IsNotNull(rollbarData.Body.Trace.Exception);
            Assert.AreEqual(exceptionMessage, rollbarData.Body.Trace.Exception.Message);
            Assert.AreEqual(exception.GetType().FullName, rollbarData.Body.Trace.Exception.Class);

            Assert.IsTrue(rollbarData.Timestamp.HasValue);
            long initialTimestamp = rollbarData.Timestamp.Value;


            const string customKey1 = "customKey1";
            const string customValue1 = "customValue1";

            const string customKey2 = "customKey2";
            const string customValue2 = "customValue2";

            Dictionary<string, object> customData = new Dictionary<string, object>()
            {
                { customKey1, customValue1},
                { customKey2, customValue2},
            };

            packagingStrategy = new CustomKeyValuePackageDecorator(packagingStrategy, customData);

            // All the asserts prior to the decoration should be good again:

            Assert.IsFalse(packagingStrategy.MustApplySynchronously, "Expected to be an async strategy!");

            rollbarData = packagingStrategy.PackageAsRollbarData();

            Assert.AreEqual(rollbarDataTitle, rollbarData.Title, "Data title is properly set!");
            Assert.IsNotNull(rollbarData.Body);
            Assert.IsNotNull(rollbarData.Body.Trace);
            Assert.IsNull(rollbarData.Body.Message);
            Assert.IsNull(rollbarData.Body.TraceChain);
            Assert.IsNull(rollbarData.Body.CrashReport);

            Assert.IsNotNull(rollbarData.Body.Trace.Exception);
            Assert.AreEqual(exceptionMessage, rollbarData.Body.Trace.Exception.Message);
            Assert.AreEqual(exception.GetType().FullName, rollbarData.Body.Trace.Exception.Class);

            // Custom data decoration specific asserts:

            Assert.IsNotNull(rollbarData.Custom);
            Assert.AreEqual(customData.Count, rollbarData.Custom.Count);
            Assert.IsTrue(rollbarData.Custom.ContainsKey(customKey1));
            Assert.IsTrue(rollbarData.Custom.ContainsKey(customKey2));
            Assert.AreEqual(customValue1, rollbarData.Custom[customKey1]);
            Assert.AreEqual(customValue2, rollbarData.Custom[customKey2]);

            // Make sure the Data.Timestamp was not overwritten:

            Assert.IsTrue(rollbarData.Timestamp.HasValue);
            Assert.AreEqual(initialTimestamp, rollbarData.Timestamp.Value);
            System.Diagnostics.Debug.WriteLine($"Initial timestamp: {initialTimestamp}");
            System.Diagnostics.Debug.WriteLine($"Decorated timestamp: {rollbarData.Timestamp.Value}");
        }
    }
}
