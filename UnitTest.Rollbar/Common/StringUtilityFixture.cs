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
    [TestCategory(nameof(StringUtilityFixture))]
    public class StringUtilityFixture
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

    }
}
