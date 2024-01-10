#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.NetPlatformExtensions
{
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using global::Rollbar.NetPlatformExtensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;
    using System.Collections.Generic;


    [TestClass]
    [TestCategory(nameof(RollbarLoggerFixture))]
    public class RollbarLoggerFixture
        : RollbarLiveFixtureBase
    {

        [TestInitialize]
        public override void SetupFixture()
        {
            base.SetupFixture();
        }

        [TestCleanup]
        public override void TearDownFixture()
        {
            base.TearDownFixture();
        }


        [Ignore]
        [TestMethod]
        public void TestBasics()
        {
            RollbarLogger rl = new RollbarLogger(@"RollbarLoggerExtension", this.ProvideLiveRollbarConfig(), new RollbarOptions());

            using(rl.BeginScope(@"ScopeState"))
            {
                this.IncrementCount<CommunicationEventArgs>();
                rl.Log(LogLevel.Critical,default(EventId),@"LogState",new System.Exception(@"LogException"),default(Func<string,System.Exception,string>));
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
            }

            var payload = this.GetAllEvents<CommunicationEventArgs>().First().Payload;
            Assert.IsTrue(payload.Contains("\"custom\":{\"LogEventID\":\"0\",\"RollbarLoggerName\":\"RollbarLoggerExtension\""));
        }
    }
}
