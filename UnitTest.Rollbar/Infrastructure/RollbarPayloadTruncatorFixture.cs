namespace UnitTest.Rollbar.Infrastructure
{
    using System.Text;

    using global::Rollbar;
    using global::Rollbar.DTOs;
    using global::Rollbar.Infrastructure;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Newtonsoft.Json;

    using Rollbar.Infrastructure;

    [TestClass()]
    [TestCategory(nameof(RollbarPayloadTruncatorFixture))]
    public class RollbarPayloadTruncatorFixture
    {
        [TestInitialize]
        public void SetupFixture()
        {
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod()]
        public void TruncatePayloadTest()
        {
            MessagePackage package = new MessagePackage(ProduceLongMessage());
            using var logger = FakeLogger();
            PayloadBundle bundle = new PayloadBundle(logger, package, ErrorLevel.Error);
            var payload = bundle.GetPayload();
            Assert.IsNotNull(payload);
            string jsonData = JsonConvert.SerializeObject(payload);
            Assert.IsNotNull(jsonData);
            int untruncatedLength = jsonData.Length;

            var truncator = new RollbarPayloadTruncator(null);
            bool result = truncator.TruncatePayload(bundle);
            payload = bundle.GetPayload();
            Assert.IsNotNull(payload);
            jsonData = JsonConvert.SerializeObject(payload);
            Assert.IsNotNull(jsonData);
            int truncatedLength = jsonData.Length;
            Assert.IsTrue(untruncatedLength > truncatedLength);
        }

        private static string ProduceLongMessage()
        {
            StringBuilder sb = new StringBuilder();
            int countDown = 100000;
            while (countDown-- > 0)
            {
                sb.Append("Very-Long-Message-");
            }

            return sb.ToString();
        }

        private static IRollbar FakeLogger()
        {
            return RollbarFactory.CreateNew();
        }
    }
}