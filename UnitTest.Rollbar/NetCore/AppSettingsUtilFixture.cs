#if NETCOREAPP2_0

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.NetCore
{
    using global::Rollbar;
    using global::Rollbar.NetCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.IO;
    using System.Threading;

    [TestClass]
    [TestCategory(nameof(AppSettingsUtilFixture))]
    public class AppSettingsUtilFixture
    {
        [TestInitialize]
        public void SetupFixture()
        {
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void LoadAppSettingsTest()
        {
            RollbarConfig config = new RollbarConfig("default=none");
            AppSettingsUtil.LoadAppSettings(ref config, Path.Combine(Environment.CurrentDirectory, "TestData"), "appsettings.json");

            // The test data looks like this:
            //===============================
            //"Rollbar": {
            //    "AccessToken": "17965fa5041749b6bf7095a190001ded",
            //    "Environment": "AspNetCoreMiddlewareTest",
            //    "Enabled": true,
            //    "MaxReportsPerMinute": 160,
            //    "ReportingQueueDepth": 120,
            //    "LogLevel": "Info",
            //    "ScrubFields": [
            //      "ThePassword",
            //      "TheSecret"
            //    ],
            //    "Person": {
            //      "UserName": "jbond"

            //    }
            //  }

            Assert.AreEqual("17965fa5041749b6bf7095a190001ded", config.AccessToken);
            Assert.AreEqual("AspNetCoreMiddlewareTest", config.Environment);
            Assert.AreEqual(true, config.Enabled);
            Assert.AreEqual(160, config.MaxReportsPerMinute);
            Assert.AreEqual(120, config.ReportingQueueDepth);
            Assert.AreEqual(ErrorLevel.Info, config.LogLevel.Value);
            Assert.IsTrue(config.ScrubFields.Length >= 2);
            Assert.AreEqual("jbond", config.Person.UserName);
        }
    }
}

#endif
