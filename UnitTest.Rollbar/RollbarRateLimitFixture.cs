namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using global::Rollbar.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Net.Http.Headers;
    using Moq;
    using System.Linq;

    /// <summary>
    /// Defines test class RollbarRateLimitFixture.
    /// </summary>
    [TestClass]
    [TestCategory(nameof(RollbarRateLimitFixture))]
    public class RollbarRateLimitFixture
    {
        /// <summary>
        /// Setups the fixture.
        /// </summary>
        [TestInitialize]
        public void SetupFixture()
        {
        }

        /// <summary>
        /// Tears down fixture.
        /// </summary>
        [TestCleanup]
        public void TearDownFixture()
        {
        }

        /// <summary>
        /// Defines the test method TestRollbarRateLimit.
        /// </summary>
        [TestMethod]
        public void TestRollbarRateLimit()
        {
            //X-Rate-Limit-Limit: 5000
            //X-Rate-Limit-Remaining: 4992
            //X-Rate-Limit-Reset: 1554828920

            var headersMock = new Mock<HttpHeaders>();
            HttpHeaders headers = headersMock.Object;

            var limitHeader = RollbarRateLimit.RollbarRateLimitHeaders.Limit;
            var limitValue = 5000;
            headers.Add(limitHeader, limitValue.ToString());

            var remainingHeader = RollbarRateLimit.RollbarRateLimitHeaders.Remaining;
            var remainingValue = 4992;
            headers.Add(remainingHeader, remainingValue.ToString());

            var resetHeader = RollbarRateLimit.RollbarRateLimitHeaders.Reset;
            var resetValue = 1554828920;
            headers.Add(resetHeader, resetValue.ToString());


            RollbarRateLimit rollbarRateLimit = new RollbarRateLimit(headers);
            Assert.AreEqual(limitValue, rollbarRateLimit.WindowLimit);
            Assert.AreEqual(remainingValue, rollbarRateLimit.WindowRemaining);
            Assert.AreEqual(DateTimeUtil.ConvertFromUnixTimestampInSeconds(resetValue), rollbarRateLimit.WindowEnd);
        }
    }
}
