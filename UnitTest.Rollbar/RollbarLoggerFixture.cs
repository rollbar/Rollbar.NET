namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    [TestCategory("RollbarLoggerFixture")]
    public class RollbarLoggerFixture
    {
        private IRollbar _logger = null;

        [TestInitialize]
        public void SetupFixture()
        {
            RollbarConfig loggerConfig =
                new RollbarConfig(RollbarUnitTestSettings.AccessToken) { Environment = RollbarUnitTestSettings.Environment, };
            _logger = RollbarFactory.CreateNew().Configure(loggerConfig);
        }

        [TestCleanup]
        public void TearDownFixture()
        {

        }

        [TestMethod]
        public void ReportException()
        {
            try
            {
                _logger.Log(ErrorLevel.Error, new System.Exception("test exception"));
            }
            catch
            {
                Assert.IsTrue(false); 
            }
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
                try
                {
                    _logger.Error(new System.Exception("outer exception", ex));
                }
                catch
                {
                    Assert.IsTrue(false);
                }
            }
        }

        [TestMethod]
        public void ReportMessage()
        {
            try
            {
                _logger.Log(ErrorLevel.Error, "test message");
            }
            catch
            {
                Assert.IsTrue(false);
            }
        }
    }
}
