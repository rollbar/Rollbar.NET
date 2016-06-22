using Xunit;

namespace RollbarDotNet.Test
{
    public class RollbarFixture
    {
        public RollbarFixture()
        {
            Rollbar.Init(new RollbarConfig
            {
                AccessToken = "17965fa5041749b6bf7095a190001ded",
                Environment = "unit-tests"
            });
        }

        [Fact]
        public void ReportException()
        {
            var guid = Rollbar.Report(new System.Exception("test exception"), ErrorLevel.Error, null);
            Assert.NotNull(guid);
        }

        [Fact]
        public void ReportMessage()
        {
            var guid = Rollbar.Report("test message", ErrorLevel.Error, null);
            Assert.NotNull(guid);
        }
    }
}