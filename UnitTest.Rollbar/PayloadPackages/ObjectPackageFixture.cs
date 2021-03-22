#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using UnitTest.Rollbar.RollbarPerformance;

    [TestClass]
    [TestCategory(nameof(ObjectPackageFixture))]
    public class ObjectPackageFixture
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
        public void ExceptionObjectPackageTest()
        {
            const int totalExceptionFrames = 5;
            System.Exception exceptionObj = ExceptionSimulator.GetExceptionWith(totalExceptionFrames);
            IRollbarPackage package = new ObjectPackage(exceptionObj, true);
            Assert.IsTrue(package.MustApplySynchronously);
            Assert.IsNull(package.RollbarData);
            var rollbarData = package.PackageAsRollbarData();
            Assert.AreSame(rollbarData, package.RollbarData);
            //TODO: compare more relevant data DTO properties to exceptionObj....

        }
    }
}
