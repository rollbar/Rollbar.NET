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
    [TestCategory(nameof(IpAddressUtilityFixture))]
    public class IpAddressUtilityFixture
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
        public void TestIpv4AddressAnonymizing()
        {
            string ipUnderTest = "10.0.11.33";
            string anonymizedIP = IpAddressUtility.Anonymize(ipUnderTest);
            Assert.AreEqual("10.0.11.0/24", anonymizedIP);
        }

        [TestMethod]
        public void TestIpv6AddressAnonymizing()
        {
            string ipUnderTest = "2001:0db8:0000:0042:0000:8a2e:0370:7334";
            string anonymizedIP = IpAddressUtility.Anonymize(ipUnderTest);
            Assert.AreEqual("2001:0db8:00...", anonymizedIP);
        }
    }
}
