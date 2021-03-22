using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTest.Rollbar.Common
{
    using global::Rollbar;
    using global::Rollbar.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Defines test class CategorizedEntitiesFixture.
    /// </summary>
    [TestClass]
    [TestCategory(nameof(CategorizedEntitiesFixture))]
    public class CategorizedEntitiesFixture
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
            CategorizedEntities<Enum, string> categorizedEntities = new CategorizedEntities<Enum, string>();

            // test combined catedories:
            var testData = this.BuildTestData();
            int maxExpectedCategories = 0;
            int maxExpectedEntries = 0;
            foreach (var key in testData.Keys)
            {
                maxExpectedCategories++;
                maxExpectedEntries += testData[key].Length;

                categorizedEntities.RegisterCategory(key, new HashSet<string>(testData[key]));
            }

            Assert.AreEqual(maxExpectedCategories, categorizedEntities.GetCategoriesCount(),
                "Total categories match...");
            Assert.AreEqual(maxExpectedEntries, categorizedEntities.GetEntitiesCount(), "Total entities match...");
            Assert.AreEqual(maxExpectedEntries,
                categorizedEntities.GetEntitiesCount(categorizedEntities.GetCategories()), "Total entities match...");

            // test removal of a category:
            Enum removedCategory = App1.Category3;
            categorizedEntities.UnRegisterCategory(removedCategory);
            Assert.AreEqual(maxExpectedCategories - 1, categorizedEntities.GetCategoriesCount(),
                "Total left-over categories match...");
            Assert.AreEqual(maxExpectedEntries - testData[removedCategory].Length,
                categorizedEntities.GetEntitiesCount(), "Total left-ver entities match...");
            Assert.AreEqual(maxExpectedEntries - testData[removedCategory].Length,
                categorizedEntities.GetEntitiesCount(categorizedEntities.GetCategories()),
                "Total left-over entities match...");

            // test reduction of a category:
            Enum reducedCategory = App1.Category2;
            categorizedEntities.ReduceCategory(reducedCategory,
                new HashSet<string>(new[] { testData[reducedCategory].Last(), testData[App2.Category1].First(), }) //one category-relevant entry, and another one from irrelevant category... 
                );
            Assert.AreEqual(maxExpectedCategories - 1, categorizedEntities.GetCategoriesCount(), "Total left-over categories match...");
            Assert.AreEqual(maxExpectedEntries - testData[removedCategory].Length - 1, categorizedEntities.GetEntitiesCount(), "Total left-ver entities match...");
            Assert.AreEqual(maxExpectedEntries - testData[removedCategory].Length - 1, categorizedEntities.GetEntitiesCount(categorizedEntities.GetCategories()), "Total left-over entities match...");
        }

        /// <summary>
        /// Builds the test data.
        /// </summary>
        /// <returns>Dictionary&lt;Enum, System.String[]&gt;.</returns>
        private Dictionary<Enum, string[]> BuildTestData()
        {
            return new Dictionary<Enum, string[]>
            {
                {App1.Category1, new [] {"app1category1_00",} },
                {App1.Category2, new [] {"app1category2_00", "app1category2_01", } },
                {App1.Category3, new [] {"app1category3_00", "app1category3_01", "app1category3_02", } },

                {App2.Category1, new [] {"app2category1_00",} },
                {App2.Category2, new [] {"app2category2_00", "app2category2_01", } },
            };
        }

        /// <summary>
        /// Enum App1
        /// </summary>
        internal enum App1
        {
            /// <summary>
            /// The category1
            /// </summary>
            Category1,
            /// <summary>
            /// The category2
            /// </summary>
            Category2,
            /// <summary>
            /// The category3
            /// </summary>
            Category3,
        }

        /// <summary>
        /// Enum App2
        /// </summary>
        internal enum App2
        {
            /// <summary>
            /// The category1
            /// </summary>
            Category1,
            /// <summary>
            /// The category2
            /// </summary>
            Category2,
        }

    }
}
