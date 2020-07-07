namespace UnitTest.Rollbar.Common
{
    using global::Rollbar;
    using global::Rollbar.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Defines test class StringUtilityFixture.
    /// </summary>
    [TestClass]
    [TestCategory(nameof(ObjectUtilityFixture))]
    public class ObjectUtilityFixture
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

        [TestMethod]
        public void TestAreSimilarReferences()
        {
            object anObject = new object();
            object anotherObject = anObject;
            object nullObject = null;

            Assert.IsTrue(ObjectUtility.AreSimilarReferences(anObject, anObject));
            Assert.IsTrue(ObjectUtility.AreSimilarReferences(anObject, anotherObject));

            Assert.IsTrue(ObjectUtility.AreSimilarReferences(nullObject, nullObject));
            Assert.IsTrue(ObjectUtility.AreSimilarReferences(nullObject, null));

            Assert.IsFalse(ObjectUtility.AreSimilarReferences(nullObject, anotherObject));
            Assert.IsFalse(ObjectUtility.AreSimilarReferences(anObject, new object()));
        }

        [TestMethod]
        public void TestAreComparableViaProperties()
        {
            Assert.IsFalse(ObjectUtility.AreComparableViaProperties(null, null));
            Assert.IsFalse(ObjectUtility.AreComparableViaProperties(new object(), null));

            Assert.IsFalse(ObjectUtility.AreComparableViaProperties(new AClass(), new BClass()));
            Assert.IsFalse(ObjectUtility.AreComparableViaProperties(new ASubclass(), new BSubclass()));
            Assert.IsTrue(ObjectUtility.AreComparableViaProperties(new AClass(), new AClass()));
            Assert.IsTrue(ObjectUtility.AreComparableViaProperties(new AClass(), new ASubclass()));

            Assert.IsFalse(ObjectUtility.AreComparableViaProperties(new IntAClass(), new IntBClass()));
            Assert.IsTrue(ObjectUtility.AreComparableViaProperties(new IntAClass(), new MultifacedClass()));
        }

    }


    #region mocking birds

    public class AClass {}
    public class BClass {}

    public class ASubclass : AClass {}
    public class BSubclass : BClass {}

    public interface IntA { }
    public interface IntB { }

    public class IntAClass : IntA {}
    public class IntBClass : IntB {}
    public class MultifacedClass : IntA, IntB {}


    #endregion mocking birds

}
