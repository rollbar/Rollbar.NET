namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    [TestCategory("RollbarLoggerFixture")]
    public class RollbarLoggerFixture
    {
        private const string accessToken = "17965fa5041749b6bf7095a190001ded";
        private const string environment = "unit-tests";

        private IRollbar _logger = null;

        [TestInitialize]
        public void SetupFixture()
        {
            RollbarConfig loggerConfig =
                new RollbarConfig(accessToken) { Environment = environment, };
            _logger = new RollbarLogger().Configure(loggerConfig);
            //_logger = new RollbarLogger().Configure(accessToken);
        }

        [TestCleanup]
        public void TearDownFixture()
        {

        }

        [TestMethod]
        public void ReportFromCatch()
        {
            try
            {
                var a = 10;
                var b = 0;
                var c = a / b;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(new System.Exception("outer exception", ex));
            }
        }
    }
}
