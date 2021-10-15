namespace UnitTest.Rollbar.Common
{
    using global::Rollbar;
    using global::Rollbar.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Defines test class ValidatorFixture.
    /// </summary>
    [TestClass]
    [TestCategory(nameof(ReconfigurableBaseFixture))]
    public class ReconfigurableBaseFixture
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

        [DataTestMethod]
        public void TestGetHashCode()
        {
            // let's have to default logger configs - A and B:
            RollbarLoggerConfig loggerConfigA = new();
            RollbarLoggerConfig loggerConfigB = new();
            Assert.AreEqual(loggerConfigA.GetHashCode(), loggerConfigB.GetHashCode(), "Both default configs should be similar.");

            // let's invert one bool setting on B:
            loggerConfigB.RollbarDeveloperOptions.RethrowExceptionsAfterReporting = !loggerConfigB.RollbarDeveloperOptions.RethrowExceptionsAfterReporting;
            Assert.AreNotEqual(loggerConfigA.GetHashCode(), loggerConfigB.GetHashCode(), "Expected to be different.");

            // let's invert one bool setting on B one more time (back to the original/default value):
            loggerConfigB.RollbarDeveloperOptions.RethrowExceptionsAfterReporting = !loggerConfigB.RollbarDeveloperOptions.RethrowExceptionsAfterReporting;
            Assert.AreEqual(loggerConfigA.GetHashCode(), loggerConfigB.GetHashCode(), "Expected to be similar again.");
        }

        [DataTestMethod]
        public void TestEquals()
        {
            // let's have to default logger configs - A and B:
            RollbarLoggerConfig loggerConfigA = new();
            RollbarLoggerConfig loggerConfigB = new();
            Assert.IsTrue(loggerConfigA.Equals(loggerConfigB), "Both default configs should be similar.");

            // let's invert one bool setting on B:
            loggerConfigB.RollbarDeveloperOptions.RethrowExceptionsAfterReporting = !loggerConfigB.RollbarDeveloperOptions.RethrowExceptionsAfterReporting;
            Assert.IsFalse(loggerConfigA.Equals(loggerConfigB), "Expected to be different.");

            // let's invert one bool setting on B one more time (back to the original/default value):
            loggerConfigB.RollbarDeveloperOptions.RethrowExceptionsAfterReporting = !loggerConfigB.RollbarDeveloperOptions.RethrowExceptionsAfterReporting;
            Assert.IsTrue(loggerConfigA.Equals(loggerConfigB), "Expected to be similar again.");
        }

        [DataTestMethod]
        public void TestReconfigure()
        {
            // let's have to default logger configs - A and B:
            RollbarLoggerConfig loggerConfigA = new();
            RollbarLoggerConfig loggerConfigB = new();
            Assert.IsTrue(loggerConfigA.Equals(loggerConfigB), "Both default configs should be similar.");

            // let's invert one bool setting on B:
            loggerConfigB.RollbarDeveloperOptions.RethrowExceptionsAfterReporting = !loggerConfigB.RollbarDeveloperOptions.RethrowExceptionsAfterReporting;
            Assert.IsFalse(loggerConfigA.Equals(loggerConfigB), "Expected to be different.");

            // let's reconfigure A like B:
            loggerConfigA.Reconfigure(loggerConfigB);
            Assert.IsTrue(loggerConfigA.Equals(loggerConfigB), "Expected to be similar again.");
            Assert.AreNotSame(loggerConfigA, loggerConfigB, "These are still two different instances.");
        }
    }
}
