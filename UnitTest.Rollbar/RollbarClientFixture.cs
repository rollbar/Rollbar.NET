#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using global::Rollbar.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Threading;
    using System.Linq;
    using UnitTest.RollbarTestCommon;
    using global::Rollbar.Infrastructure;

    [TestClass]
    [TestCategory(nameof(RollbarClientFixture))]
    public class RollbarClientFixture
    {

        [TestInitialize]
        public void SetupFixture()
        {
            RollbarUnitTestEnvironmentUtil.SetupLiveTestRollbarInfrastructure();
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        private static MessagePackage CrateTestPackage()
        {
            MessagePackage package =
                new MessagePackage($"{nameof(RollbarClientFixture)}.BasicTest");
            return package;
        }

        [Ignore]
        [TestMethod]
        public void TestUsingDefaultConfig()
        {

            // [DO]: let's send a payload with default config settings (including default value for the PayloadPostTimeout)
            // [EXPECT]: under all the good networking conditions sending a payload should succeed
            using(var logger = (RollbarLogger) RollbarFactory.CreateNew(RollbarInfrastructure.Instance.Config.RollbarLoggerConfig))
            {
                RollbarUnitTestEnvironmentUtil.TraceCurrentRollbarInfrastructureConfig();
                RollbarUnitTestEnvironmentUtil.Trace(logger.Config, "Logger_Config");
                var client = new RollbarClient(logger);
                var bundle = new PayloadBundle(logger, RollbarClientFixture.CrateTestPackage(), ErrorLevel.Info);
                client.EnsureHttpContentToSend(bundle);
                var response = client.PostAsJson(bundle);
                RollbarUnitTestEnvironmentUtil.Trace(response, "Rollbar API HTTP response");
                Assert.IsNotNull(response);
                Assert.AreEqual(0, response.Error);
                Assert.AreEqual(0, bundle.Exceptions.Count);
            }

        }

        [TestMethod]
        public void TestUsingUnreasonablyShortTimeout()
        {
            // [DO]: let's send a payload using unreasonably short PayloadPostTimeout
            // [EXPECT]: even under all the good networking conditions sending a payload should not succeed
            RollbarInfrastructureOptions infrastructureOptions =
                new RollbarInfrastructureOptions();
            infrastructureOptions.PayloadPostTimeout =
                TimeSpan.FromMilliseconds(5); // short enough
            RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions
                .Reconfigure(infrastructureOptions);
            using(var logger = (RollbarLogger) RollbarFactory.CreateNew(RollbarInfrastructure.Instance.Config.RollbarLoggerConfig))
            {
                RollbarUnitTestEnvironmentUtil.TraceCurrentRollbarInfrastructureConfig();
                RollbarUnitTestEnvironmentUtil.Trace(logger.Config, "Logger_Config");
                var client = new RollbarClient(logger);
                var bundle = new PayloadBundle(logger, RollbarClientFixture.CrateTestPackage(), ErrorLevel.Info);
                client.EnsureHttpContentToSend(bundle);
                var response = client.PostAsJson(bundle);
                RollbarUnitTestEnvironmentUtil.Trace(response, "Rollbar API HTTP response");
                Assert.IsNull(response);
                Assert.AreEqual(1, bundle.Exceptions.Count);
                //Assert.AreEqual("While PostAsJson(PayloadBundle payloadBundle)...", bundle.Exceptions.First().Message);
                //Assert.IsTrue(bundle.Exceptions.First().InnerException.Message.StartsWith("One or more errors occurred"));
                //Assert.AreEqual("A task was canceled.",  bundle.Exceptions.First().InnerException.InnerException.Message);
            }
        }

        [Ignore]
        [TestMethod]
        public void TestUsingLongEnoughTimeout()
        {

            // [DO]: let's send a payload using reasonably long PayloadPostTimeout
            // [EXPECT]: even under all the good networking conditions sending a payload should succeed
            RollbarInfrastructureOptions infrastructureOptions =
                new RollbarInfrastructureOptions();
            infrastructureOptions.PayloadPostTimeout =
                TimeSpan.FromMilliseconds(2000); // long enough
            RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions
                .Reconfigure(infrastructureOptions);
            using(var logger = (RollbarLogger)RollbarFactory.CreateNew(RollbarInfrastructure.Instance.Config.RollbarLoggerConfig))
            {
                RollbarUnitTestEnvironmentUtil.TraceCurrentRollbarInfrastructureConfig();
                RollbarUnitTestEnvironmentUtil.Trace(logger.Config, "Logger_Config");
                var client = new RollbarClient(logger);
                var bundle = new PayloadBundle(logger, RollbarClientFixture.CrateTestPackage(), ErrorLevel.Info);
                client.EnsureHttpContentToSend(bundle);
                var response = client.PostAsJson(bundle);
                RollbarUnitTestEnvironmentUtil.Trace(response, "Rollbar API HTTP response");
                Assert.IsNotNull(response, "let's send a payload using reasonably long PayloadPostTimeout");
                Assert.AreEqual(0, response.Error);
                Assert.AreEqual(0, bundle.Exceptions.Count);
            }
        }
    }
}
