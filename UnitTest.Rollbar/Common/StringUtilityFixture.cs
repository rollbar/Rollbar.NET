namespace UnitTest.Rollbar.Common
{
    using global::Rollbar;
    using global::Rollbar.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Defines test class StringUtilityFixture.
    /// </summary>
    [TestClass]
    [TestCategory(nameof(StringUtilityFixture))]
    public class StringUtilityFixture
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
        /// Defines the test method TestEncodingBytesCountCalculation.
        /// </summary>
        [TestMethod]
        public void TestEncodingBytesCountCalculation()
        {
            var testRecords = new (string Input, int MaxBytes, int ExactBytes)[] 
            {
                (null, 0, 0),
                (string.Empty, 3, 0),
                ("", 3, 0),
                ("1", 6, 1),
                ("A", 6, 1),
                ("a", 6, 1),
                ("д", 6, 2),
                ("Ⓢ", 6, 3),
                ("⁵", 6, 3),
                ("∞", 6, 3),
                ("Ⓢ ⁵ ∞", 18, 11),
            };

            foreach(var record in testRecords)
            {
                Trace.WriteLine(
                    record.Input + " : "
                    + StringUtility.CalculateMaxEncodingBytes(record.Input, Encoding.UTF8) + " : "
                    + StringUtility.CalculateExactEncodingBytes(record.Input, Encoding.UTF8)
                    );
                Assert.AreEqual(record.MaxBytes, StringUtility.CalculateMaxEncodingBytes(record.Input, Encoding.UTF8));
                Assert.AreEqual(record.ExactBytes, StringUtility.CalculateExactEncodingBytes(record.Input, Encoding.UTF8));
            }
        }

        /// <summary>
        /// Defines the test method TestTruncation.
        /// </summary>
        [TestMethod]
        public void TestTruncation()
        {
            string testInput = "Ⓢ ⁵ ∞B д 1 A";

            Trace.WriteLine("Input: ".ToUpper());
            Trace.WriteLine(
                testInput + " : "
                + StringUtility.CalculateExactEncodingBytes(testInput, Encoding.UTF8)
                );

            Trace.WriteLine("Truncations: ".ToUpper());
            int bytesLimit = StringUtility.CalculateExactEncodingBytes(testInput, Encoding.UTF8) + 1;
            while(bytesLimit >= 0)
            {
                Trace.WriteLine(
                    bytesLimit + " bytes limit : "
                    + StringUtility.Truncate(testInput, Encoding.UTF8, bytesLimit)
                    );
                bytesLimit--;
            }
        }

        /// <summary>
        /// Defines the test method TestIntParsing.
        /// </summary>
        [TestMethod]
        public void TestIntParsing()
        {
            Assert.IsFalse(StringUtility.Parse<int>("WRONG", int.TryParse).HasValue);

            Assert.IsTrue(StringUtility.Parse<int>("123", int.TryParse).HasValue);
            Assert.IsTrue(StringUtility.Parse<int>("-123", int.TryParse).HasValue);

            Assert.AreEqual(123, StringUtility.Parse<int>("123", int.TryParse).Value);
            Assert.AreEqual(-123, StringUtility.Parse<int>("-123", int.TryParse).Value);
        }

        /// <summary>
        /// Defines the test method TestIntParsingWithDefault.
        /// </summary>
        [TestMethod]
        public void TestIntParsingWithDefault()
        {
            const int defaultValue = int.MaxValue;
            Assert.AreEqual(defaultValue, StringUtility.ParseOrDefault<int>("WRONG", int.TryParse, defaultValue));

            Assert.AreEqual(123, StringUtility.ParseOrDefault<int>("123", int.TryParse, defaultValue));
            Assert.AreEqual(-123, StringUtility.ParseOrDefault<int>("-123", int.TryParse, defaultValue));
        }

        /// <summary>
        /// Defines the test method TestDateTimeOffsetParsing.
        /// </summary>
        [TestMethod]
        public void TestDateTimeOffsetParsing()
        {
            Assert.IsFalse(StringUtility.Parse<DateTimeOffset>("WRONG", DateTimeOffset.TryParse).HasValue);

            DateTimeOffset testDate = DateTimeOffset.Now;
            testDate = new DateTimeOffset(
                testDate.Date.Year, testDate.Date.Month, testDate.Date.Day, 
                testDate.DateTime.Hour, testDate.DateTime.Minute, testDate.DateTime.Second, 
                testDate.Offset);
            Assert.IsTrue(StringUtility.Parse<DateTimeOffset>(testDate.ToString(), DateTimeOffset.TryParse).HasValue);
            Assert.AreEqual(testDate, StringUtility.Parse<DateTimeOffset>(testDate.ToString(), DateTimeOffset.TryParse).Value);
        }

        /// <summary>
        /// Defines the test method TestDateTimeOffsetParsingWithDefault.
        /// </summary>
        [TestMethod]
        public void TestDateTimeOffsetParsingWithDefault()
        {
            DateTimeOffset defaultValue = DateTimeOffset.MaxValue;
            Assert.AreEqual(defaultValue, StringUtility.ParseOrDefault<DateTimeOffset>("WRONG", DateTimeOffset.TryParse, defaultValue));

            DateTimeOffset testDate = DateTimeOffset.Now;
            testDate = new DateTimeOffset(
                testDate.Date.Year, testDate.Date.Month, testDate.Date.Day,
                testDate.DateTime.Hour, testDate.DateTime.Minute, testDate.DateTime.Second,
                testDate.Offset);
            Assert.AreEqual(testDate, StringUtility.ParseOrDefault<DateTimeOffset>(testDate.ToString(), DateTimeOffset.TryParse, defaultValue));
        }
    }
}
