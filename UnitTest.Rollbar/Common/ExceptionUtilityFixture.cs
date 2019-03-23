#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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

    [TestClass]
    [TestCategory(nameof(ExceptionUtilityFixture))]
    public class ExceptionUtilityFixture
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
        public void BasicTest()
        {
            var exception = ExceptionSimulator.GetExceptionWith(10, "SimulatedException");
            var localVars = ExceptionUtility.SnapLocalVariables(exception);
        }
    }
}
