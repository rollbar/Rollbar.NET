#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar.Common
{
    using global::Rollbar;
    using global::Rollbar.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;

    [TestClass]
    [TestCategory(nameof(NameValueCollectionExtensionFixture))]
    public class NameValueCollectionExtensionFixture
    {
        [TestInitialize]
        public void SetupFixture()
        {
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        private static NameValueCollection GetNameValueCollectionSample()
        {
            NameValueCollection nvc = new NameValueCollection();
            nvc["key1"] = "value1";
            nvc["key2"] = "value2";
            nvc["key3"] = "value3";

            return nvc;
        }

        [TestMethod]
        public void TestToObjectDictionary()
        {
            NameValueCollection nvc = GetNameValueCollectionSample();

            var objectDictionary = nvc.ToObjectDictionary();
            Assert.IsNotNull(objectDictionary);
            Assert.AreEqual(nvc.Count, objectDictionary.Count);
            Assert.AreEqual(nvc.Keys.Count, objectDictionary.Keys.Count);
            Assert.AreEqual(nvc["key1"], objectDictionary["key1"]);
            Assert.AreNotEqual(nvc["key1"], objectDictionary["key2"]);
        }

        [TestMethod]
        public void TestToStringDictionary()
        {
            NameValueCollection nvc = GetNameValueCollectionSample();

            var objectDictionary = nvc.ToStringDictionary();
            Assert.IsNotNull(objectDictionary);
            Assert.AreEqual(nvc.Count, objectDictionary.Count);
            Assert.AreEqual(nvc.Keys.Count, objectDictionary.Keys.Count);
            Assert.AreEqual(nvc["key1"], objectDictionary["key1"]);
            Assert.AreNotEqual(nvc["key1"], objectDictionary["key2"]);
        }
    }
}
