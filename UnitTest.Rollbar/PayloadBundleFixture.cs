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
        private static readonly RollbarInfrastructureConfig infrastructureConfig;
        static PayloadBundleFixture()
        {
            infrastructureConfig = new RollbarInfrastructureConfig(RollbarUnitTestSettings.AccessToken, RollbarUnitTestSettings.Environment);
            if(!RollbarInfrastructure.Instance.IsInitialized)
            {
                RollbarInfrastructure.Instance.Init(infrastructureConfig);
            }
        }

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

            RollbarDestinationOptions destinationOptions = 
                new RollbarDestinationOptions("ACCESS_TOKEN", "ENV");
            IRollbarLoggerConfig config = infrastructureConfig.RollbarLoggerConfig;
            config.RollbarDestinationOptions.Reconfigure(destinationOptions);
            using (IRollbar rollbarLogger = RollbarFactory.CreateNew(config))
            {
                PayloadBundle bundle = new PayloadBundle(rollbarLogger as RollbarLogger, package, ErrorLevel.Critical);
                var payload = bundle.GetPayload();
            }
        }
    }
}
