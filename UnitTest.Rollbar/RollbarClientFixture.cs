#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using global::Rollbar.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Diagnostics;

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
                new RollbarConfig(RollbarUnitTestSettings.AccessToken) { Environment = RollbarUnitTestSettings.Environment, };
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        // NOTE: we need to figure out how we can simulate programmatically network coditions that
        //       cause prolonged network operation timeouts (over 20 sec), so we can test HttpClient's
        //       set to let's say 10 sec.
        //[TestMethod]
        //public void BasicTest()
        //{
        //    MessagePackage package = new MessagePackage(@"Test message");

        //    RollbarConfig config = new RollbarConfig();
        //    config.Reconfigure(this._loggerConfig);
        //    config.ProxyAddress = @"http://10.0.1.11";

        //    RollbarLogger logger = (RollbarLogger) RollbarFactory.CreateNew(config);
            
        //    RollbarClient client = new RollbarClient(logger);
            
        //    PayloadBundle bundle = new PayloadBundle(logger, package, ErrorLevel.Info);
        //    client.EnsureHttpContentToSend(bundle);

        //    Stopwatch sw = Stopwatch.StartNew();
        //    var response = client.PostAsJson(@"http://api.rollbar.com", config.AccessToken, "payload");
        //    sw.Stop();
        //    TimeSpan postTimespan = sw.Elapsed;
        //}
    }
}
