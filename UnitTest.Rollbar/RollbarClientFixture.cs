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

        private RollbarInfrastructureConfig _config;

        [TestInitialize]
        public void SetupFixture()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            RollbarDestinationOptions destinationOptions =
                new RollbarDestinationOptions(RollbarUnitTestSettings.AccessToken, RollbarUnitTestSettings.Environment);

            this._config = new RollbarInfrastructureConfig();
            this._config.RollbarLoggerConfig.RollbarDestinationOptions.Reconfigure(destinationOptions);

            if(!RollbarInfrastructure.Instance.IsInitialized)
            {
                RollbarInfrastructure.Instance.Init(this._config);
            }

            RollbarQueueController.Instance.InternalEvent += Instance_InternalEvent;
            RollbarQueueController.Instance.FlushQueues();
        }

        private void Instance_InternalEvent(object sender, RollbarEventArgs e)
        {
            Console.WriteLine("ROLBAR EVENT:");
            Console.WriteLine("=============");
            Console.WriteLine(e.TraceAsString());
            Console.WriteLine("=============");
            Console.WriteLine();
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
            //RollbarInfrastructureConfig config = new RollbarInfrastructureConfig();
            //config.Reconfigure(this._loggerConfig);
            using (var logger = (RollbarLogger)RollbarFactory.CreateNew(this._config.RollbarLoggerConfig))
            {
                //Console.WriteLine(logger.Config.TraceAsString());
                var client = new RollbarClient(logger);
                var bundle = new PayloadBundle(logger, package, ErrorLevel.Info);
                client.EnsureHttpContentToSend(bundle);
                var response = client.PostAsJson(bundle);
                Assert.IsNotNull(response);
                Assert.AreEqual(0, response.Error);
                Assert.AreEqual(0, bundle.Exceptions.Count);
            }

            RollbarInfrastructureOptions infrastructureOptions = new RollbarInfrastructureOptions();
            RollbarDeveloperOptions developerOptions = 
                new RollbarDeveloperOptions(
                    ErrorLevel.Debug, 
                    true, 
                    true, 
                    false, 
                    TimeSpan.FromMilliseconds(5)
                    );

            // [DO]: let's send a payload using unreasonably short PayloadPostTimeout
            // [EXPECT]: even under all the good networking conditions sending a payload should not succeed
            Console.WriteLine("*** Let's send a payload using unreasonably short PayloadPostTimeout");
            infrastructureOptions.PayloadPostTimeout = TimeSpan.FromMilliseconds(5); // too short
            this._config.RollbarInfrastructureOptions.Reconfigure(infrastructureOptions);
            this._config.RollbarLoggerConfig.RollbarDeveloperOptions.Reconfigure(developerOptions);
            RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.Reconfigure(infrastructureOptions);
            using (var logger = (RollbarLogger)RollbarFactory.CreateNew(this._config.RollbarLoggerConfig))
            {
                Console.WriteLine(this._config.TraceAsString());
                var client = new RollbarClient(logger);
                var bundle = new PayloadBundle(logger, package, ErrorLevel.Info);
                client.EnsureHttpContentToSend(bundle);
                var response = client.PostAsJson(bundle);
                Console.WriteLine(logger.Config.TraceAsString());
                Console.WriteLine(response?.TraceAsString());
                Assert.IsNull(response);
                Assert.AreEqual(1, bundle.Exceptions.Count);
                //Assert.AreEqual("While PostAsJson(PayloadBundle payloadBundle)...", bundle.Exceptions.First().Message);
                //Assert.IsTrue(bundle.Exceptions.First().InnerException.Message.StartsWith("One or more errors occurred"));
                //Assert.AreEqual("A task was canceled.",  bundle.Exceptions.First().InnerException.InnerException.Message);
            }

            // [DO]: let's send a payload using reasonably long PayloadPostTimeout
            // [EXPECT]: even under all the good networking conditions sending a payload should succeed
            Console.WriteLine("*** Let's send a payload using reasonably long PayloadPostTimeout");
            infrastructureOptions.PayloadPostTimeout = TimeSpan.FromMilliseconds(2000); // long enough
            this._config.RollbarInfrastructureOptions.Reconfigure(infrastructureOptions);
            RollbarInfrastructure.Instance.Config.RollbarInfrastructureOptions.Reconfigure(infrastructureOptions);
            using(var logger = (RollbarLogger)RollbarFactory.CreateNew(this._config.RollbarLoggerConfig))
            {
                Console.WriteLine(this._config.TraceAsString());
                var client = new RollbarClient(logger);
                var bundle = new PayloadBundle(logger, package, ErrorLevel.Info);
                client.EnsureHttpContentToSend(bundle);
                var response = client.PostAsJson(bundle);
                Assert.IsNotNull(response, "let's send a payload using reasonably long PayloadPostTimeout");
                Assert.AreEqual(0, response.Error);
                Assert.AreEqual(0, bundle.Exceptions.Count);
            }
        }
    }
}
