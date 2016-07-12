using System;
using System.Collections.Generic;
using Xunit;

namespace RollbarDotNet.Test
{
    public class RollbarFixture
    {
        private string _accessToken = "17965fa5041749b6bf7095a190001ded";

        public RollbarFixture()
        {
            Rollbar.Init(new RollbarConfig
            {
                AccessToken = _accessToken,
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
        public void ReportFromCatch()
        {
            try
            {
                Rollbar.Init(new RollbarConfig
                {
                    AccessToken = _accessToken,
                    Environment = Environment.MachineName
                });
                var a = 10;
                var b = 0;
                var c = a / b;
            }
            catch (System.Exception e)
            {
                var log = "Multi" + Environment.NewLine + "Lines";
                var dictionary = new Dictionary<string, object>
                {
                    { "message", log }
                };
                Rollbar.Report(e, ErrorLevel.Error, dictionary);
            }
        }

        [Fact]
        public void ReportMessage()
        {
            var guid = Rollbar.Report("test message", ErrorLevel.Error, null);
            Assert.NotNull(guid);
        }
    }
}