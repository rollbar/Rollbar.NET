namespace UnitTest.Rollbar.Common
{
    using global::Rollbar;
    using global::Rollbar.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Defines test class ExceptionUtilityFixture.
    /// </summary>
    [TestClass]
    [TestCategory(nameof(ExceptionUtilityFixture))]
    public class ExceptionUtilityFixture
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
        /// Defines the test method BasicTest.
        /// </summary>
        [TestMethod]
        public void BasicTest()
        {
            var exception = ExceptionSimulator.GetExceptionWith(10, "SimulatedException");
            var localVars = ExceptionUtility.SnapLocalVariables(exception);
        }
    }
}
