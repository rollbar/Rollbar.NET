#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using global::Rollbar.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Threading;
    using System.Linq;

    [TestClass]
    [TestCategory(nameof(RollbarClientFixture))]
    public class RollbarClientFixture
    {

        private RollbarConfig _loggerConfig;

        [TestInitialize]
        public void SetupFixture()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            this._loggerConfig =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken) 
                { 
                    Environment = RollbarUnitTestSettings.Environment, 
                };
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void BasicTest()
        {
            MessagePackage package = new MessagePackage($"{nameof(RollbarClientFixture)}.BasicTest");

            // [DO]: let's send a payload with default config settings (including default value for the PayloadPostTimeout)
            // [EXPECT]: under all the good networking conditions sending a payload should succeed
            RollbarConfig config = new RollbarConfig();
            config.Reconfigure(this._loggerConfig);
            using (var logger = (RollbarLogger)RollbarFactory.CreateNew(config))
            {
                var client = new RollbarClient(logger);
                var bundle = new PayloadBundle(logger, package, ErrorLevel.Info);
                client.EnsureHttpContentToSend(bundle);
                var response = client.PostAsJson(bundle);
                Assert.IsNotNull(response);
                Assert.AreEqual(0, response.Error);
                Assert.AreEqual(0, bundle.Exceptions.Count);
            }

            // [DO]: let's send a payload using unreasonably short PayloadPostTimeout
            // [EXPECT]: even under all the good networking conditions sending a payload should not succeed
            config.PayloadPostTimeout = TimeSpan.FromMilliseconds(50); // too short
            using (var logger = (RollbarLogger)RollbarFactory.CreateNew(config))
            {
                var client = new RollbarClient(logger);
                var bundle = new PayloadBundle(logger, package, ErrorLevel.Info);
                client.EnsureHttpContentToSend(bundle);
                var response = client.PostAsJson(bundle);
                Assert.IsNull(response);
                Assert.AreEqual(1, bundle.Exceptions.Count);
                //Assert.AreEqual("While PostAsJson(PayloadBundle payloadBundle)...", bundle.Exceptions.First().Message);
                //Assert.IsTrue(bundle.Exceptions.First().InnerException.Message.StartsWith("One or more errors occurred"));
                //Assert.AreEqual("A task was canceled.",  bundle.Exceptions.First().InnerException.InnerException.Message);
            }

            // [DO]: let's send a payload using reasonably long PayloadPostTimeout
            // [EXPECT]: even under all the good networking conditions sending a payload should succeed
            config.PayloadPostTimeout = TimeSpan.FromMilliseconds(1000); // long enough
            using (var logger = (RollbarLogger)RollbarFactory.CreateNew(config))
            {
                var client = new RollbarClient(logger);
                var bundle = new PayloadBundle(logger, package, ErrorLevel.Info);
                client.EnsureHttpContentToSend(bundle);
                var response = client.PostAsJson(bundle);
                Assert.IsNotNull(response);
                Assert.AreEqual(0, response.Error);
                Assert.AreEqual(0, bundle.Exceptions.Count);
            }
        }
    }
}
