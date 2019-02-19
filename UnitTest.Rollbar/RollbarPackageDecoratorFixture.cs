#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;

    [TestClass]
    [TestCategory(nameof(RollbarPackageDecoratorFixture))]
    public class RollbarPackageDecoratorFixture
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
        public void PersonPackageDecoratorTest()
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

            const string personID = "007";
            const string personUsername = "JamesBond";
            const string personEmail = "jbond@mi6.uk";
            packagingStrategy = new PersonPackageDecorator(packagingStrategy, personID, personUsername, personEmail);

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

            // Person decoration specific asserts:

            Assert.IsNotNull(rollbarData.Person);
            Assert.AreEqual(personID, rollbarData.Person.Id);
            Assert.AreEqual(personUsername, rollbarData.Person.UserName);
            Assert.AreEqual(personEmail, rollbarData.Person.Email);

            // Make sure the Data.Timestamp was not overwritten:

            Assert.IsTrue(rollbarData.Timestamp.HasValue);
            Assert.AreEqual(initialTimestamp, rollbarData.Timestamp.Value);
            System.Diagnostics.Debug.WriteLine($"Initial timestamp: {initialTimestamp}");
            System.Diagnostics.Debug.WriteLine($"Decorated timestamp: {rollbarData.Timestamp.Value}");
        }

    }
}
