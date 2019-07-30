namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;

    /// <summary>
    /// Defines test class RollbarDataScrubbingHelperFixture.
    /// </summary>
    [TestClass]
    [TestCategory(nameof(RollbarDataScrubbingHelperFixture))]
    public class RollbarDataScrubbingHelperFixture
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
            var sets = new[]
            {
                RollbarDataScrubbingHelper.Instance.GetCommonCreditCardCvvFields(),
                RollbarDataScrubbingHelper.Instance.GetCommonCreditCardNumberFields(),
                RollbarDataScrubbingHelper.Instance.GetCommonHttpHeaderFields(),
                RollbarDataScrubbingHelper.Instance.GetCommonPasswordFields(),
            };

            // test combining sets:
            int expectedCombinedTotal = sets.Aggregate(0, (total, next) => total += next.Count);
            var combinedSet = RollbarDataScrubbingHelper.Combine(sets);
            Assert.AreEqual(
                expectedCombinedTotal, 
                combinedSet.Count, 
                "Combining distinct sets..."
                );

            // test removing subset:
            expectedCombinedTotal -= 
                RollbarDataScrubbingHelper.Instance.GetCommonHttpHeaderFields().Count;
            Assert.AreEqual(
                expectedCombinedTotal,
                RollbarDataScrubbingHelper.Remove(
                    combinedSet, 
                    RollbarDataScrubbingHelper.Instance.GetCommonHttpHeaderFields()
                    ).Count,
                "Removing distinct subset..."
                );
            Assert.AreEqual(
                combinedSet.Count,
                RollbarDataScrubbingHelper.Remove(
                    combinedSet,
                    new [] {"non-existing1", "non-existing2", }
                ).Count,
                "Removing non-subset..."
            );
        }

        /// <summary>
        /// Defines the test method GetCommonCreditCardCvvFields.
        /// </summary>
        [TestMethod]
        public void GetCommonCreditCardCvvFields()
        {
            try
            {
                RollbarDataScrubbingHelper.Instance.GetCommonCreditCardCvvFields();
            }
            catch (Exception e)
            {
                Assert.Fail("Should never have an exception thrown!", e);
            }
        }

        /// <summary>
        /// Defines the test method GetCommonCreditCardNumberFieldsTest.
        /// </summary>
        [TestMethod]
        public void GetCommonCreditCardNumberFieldsTest()
        {
            try
            {
                RollbarDataScrubbingHelper.Instance.GetCommonCreditCardNumberFields();
            }
            catch (Exception e)
            {
                Assert.Fail("Should never have an exception thrown!", e);
            }
        }

        /// <summary>
        /// Defines the test method GetCommonHttpHeaderFieldsTest.
        /// </summary>
        [TestMethod]
        public void GetCommonHttpHeaderFieldsTest()
        {
            try
            {
                RollbarDataScrubbingHelper.Instance.GetCommonHttpHeaderFields();
            }
            catch (Exception e)
            {
                Assert.Fail("Should never have an exception thrown!", e);
            }
        }

        /// <summary>
        /// Defines the test method GetCommonPasswordFieldsTest.
        /// </summary>
        [TestMethod]
        public void GetCommonPasswordFieldsTest()
        {
            try
            {
                RollbarDataScrubbingHelper.Instance.GetCommonPasswordFields();
            }
            catch (Exception e)
            {
                Assert.Fail("Should never have an exception thrown!", e);
            }
        }

    }
}
