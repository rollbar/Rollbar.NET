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
    [TestCategory(nameof(PayloadBundleFixture))]
    public class PayloadBundleFixture
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
        public void BasicPayloadBundleTest()
        {
            const int totalExceptionFrames = 5;
            System.Exception exceptionObj = ExceptionSimulator.GetExceptionWith(totalExceptionFrames);
            IRollbarPackage package = new ObjectPackage(exceptionObj, true);
            Assert.IsTrue(package.MustApplySynchronously);
            Assert.IsNull(package.RollbarData);
            //var rollbarData = package.PackageAsRollbarData();
            //Assert.AreSame(rollbarData, package.RollbarData);

            RollbarConfig config = new RollbarConfig("ACCESS_TOKEN") { Environment = "ENV", };
            PayloadBundle bundle = new PayloadBundle(config, package, ErrorLevel.Critical);
            var payload = bundle.GetPayload();
        }
    }
}
