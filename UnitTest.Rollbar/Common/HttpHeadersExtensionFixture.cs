namespace UnitTest.Rollbar.Common
{
    using global::Rollbar;
    using global::Rollbar.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Net.Http.Headers;
    using Moq;
    using System.Linq;

    /// <summary>
    /// Defines test class HttpHeadersExtensionFixture.
    /// </summary>
    [TestClass]
    [TestCategory(nameof(HttpHeadersExtensionFixture))]
    public class HttpHeadersExtensionFixture
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
        public void TestGetHeaderValuesSafely()
        {
            var headersMock = new Mock<HttpHeaders>();
            HttpHeaders headers = headersMock.Object;

            // empty headers test:
            var result = headers.GetHeaderValuesSafely("DoesNotMatter");
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());

            // valid header test:
            var headerName = "HeaderName";
            var headerValue = "HeaderValue";
            headers.Add(headerName, headerValue);
            result = headers.GetHeaderValuesSafely(headerName);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());

            // valid header test:
            headerName = "MultiValueHeader";
            var headerValue1 = "HeaderValue1";
            var headerValue2 = "HeaderValue2";
            headers.Add(headerName, new string[] { headerValue1, headerValue2, });
            result = headers.GetHeaderValuesSafely(headerName);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(headerValue1, result.FirstOrDefault());
            Assert.AreEqual(headerValue2, result.LastOrDefault());

            // invalid header test:
            result = headers.GetHeaderValuesSafely("WRONG");
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        /// <summary>
        /// Defines the test method TestGetLastHeaderValueSafely.
        /// </summary>
        [TestMethod]
        public void TestGetLastHeaderValueSafely()
        {
            var headersMock = new Mock<HttpHeaders>();
            HttpHeaders headers = headersMock.Object;

            // empty headers test:
            var result = headers.GetLastHeaderValueSafely("DoesNotMatter");
            Assert.IsNull(result);

            // valid header test:
            var headerName = "HeaderName";
            var headerValue = "HeaderValue";
            headers.Add(headerName, headerValue);
            result = headers.GetLastHeaderValueSafely(headerName);
            Assert.IsNotNull(result);
            Assert.AreEqual(headerValue, result);

            // valid header test:
            headerName = "MultiValueHeader";
            var headerValue1 = "HeaderValue1";
            var headerValue2 = "HeaderValue2";
            headers.Add(headerName, new string[] {headerValue1, headerValue2, });
            result = headers.GetLastHeaderValueSafely(headerName);
            Assert.IsNotNull(result);
            Assert.AreEqual(headerValue2, result);

            // invalid header test:
            result = headers.GetLastHeaderValueSafely("WRONG");
            Assert.IsNull(result);
        }

        /// <summary>
        /// Defines the test method TestParseHeaderValueSafelyIfAny.
        /// </summary>
        [TestMethod]
        public void TestParseHeaderValueSafelyIfAny()
        {
            var headersMock = new Mock<HttpHeaders>();
            HttpHeaders headers = headersMock.Object;

            // empty headers test:
            var result = headers.ParseHeaderValueSafelyIfAny<int>("DoesNotMatter", int.TryParse);
            Assert.IsNull(result);
            Assert.IsFalse(result.HasValue);

            // valid header test:
            var headerName = "HeaderName";
            var headerValue = 123;
            headers.Add(headerName, headerValue.ToString());
            result = headers.ParseHeaderValueSafelyIfAny<int>(headerName, int.TryParse);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(headerValue, result.Value);

            // valid header test:
            headerName = "MultiValueHeader";
            var headerValue1 = 123;
            var headerValue2 = 321;
            headers.Add(headerName, new string[] { headerValue1.ToString(), headerValue2.ToString(), });
            result = headers.ParseHeaderValueSafelyIfAny<int>(headerName, int.TryParse);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(headerValue2, result.Value);

            // invalid header test:
            result = headers.ParseHeaderValueSafelyIfAny<int>("WRONG", int.TryParse);
            Assert.IsNull(result);
            Assert.IsFalse(result.HasValue);
        }

        /// <summary>
        /// Defines the test method TestParseHeaderValueSafelyOrDefault.
        /// </summary>
        [TestMethod]
        public void TestParseHeaderValueSafelyOrDefault()
        {
            var defaultValue = int.MaxValue;
            var headersMock = new Mock<HttpHeaders>();
            HttpHeaders headers = headersMock.Object;

            // empty headers test:
            var result = headers.ParseHeaderValueSafelyOrDefault<int>("DoesNotMatter", int.TryParse, defaultValue);
            Assert.IsNotNull(result);
            Assert.AreEqual(defaultValue, result);

            // valid header test:
            var headerName = "HeaderName";
            var headerValue = 123;
            headers.Add(headerName, headerValue.ToString());
            result = headers.ParseHeaderValueSafelyOrDefault<int>(headerName, int.TryParse, defaultValue);
            Assert.IsNotNull(result);
            Assert.AreEqual(headerValue, result);

            // valid header test:
            headerName = "MultiValueHeader";
            var headerValue1 = 123;
            var headerValue2 = 321;
            headers.Add(headerName, new string[] { headerValue1.ToString(), headerValue2.ToString(), });
            result = headers.ParseHeaderValueSafelyOrDefault<int>(headerName, int.TryParse, defaultValue);
            Assert.IsNotNull(result);
            Assert.AreEqual(headerValue2, result);

            // invalid header test:
            result = headers.ParseHeaderValueSafelyOrDefault<int>("WRONG", int.TryParse, defaultValue);
            Assert.IsNotNull(result);
            Assert.AreEqual(defaultValue, result);
        }

        /// <summary>
        /// Defines the test method TestParseRateLimitResetHeader.
        /// </summary>
        [TestMethod]
        public void TestParseRateLimitResetHeader()
        {
            //X-Rate-Limit-Reset: 1554828920

            var defaultValue = DateTimeOffset.MaxValue;
            var headersMock = new Mock<HttpHeaders>();
            HttpHeaders headers = headersMock.Object;

            // empty headers test:
            var result = headers.ParseHeaderValueSafelyOrDefault<DateTimeOffset>("DoesNotMatter", DateTimeUtil.TryParseFromUnixTimestampInSecondsString, defaultValue);
            Assert.IsNotNull(result);
            Assert.AreEqual(defaultValue, result);

            // valid header test:
            var headerName = "X-Rate-Limit-Reset";
            var headerValue = "1554828920";
            headers.Add(headerName, headerValue.ToString());
            result = headers.ParseHeaderValueSafelyOrDefault<DateTimeOffset>(headerName, DateTimeUtil.TryParseFromUnixTimestampInSecondsString, defaultValue);
            Assert.IsNotNull(result);
            Assert.AreEqual(DateTimeUtil.ConvertFromUnixTimestampInSeconds(long.Parse(headerValue)), result);
        }
    }
}
