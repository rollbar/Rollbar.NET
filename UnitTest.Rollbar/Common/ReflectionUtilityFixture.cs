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
    [TestCategory(nameof(ReflectionUtilityFixture))]
    public class ReflectionUtilityFixture
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
        public void TestGetAllDataFields()
        {
            var dataFields = ReflectionUtility.GetAllDataFields(new MockClass().GetType());
            Assert.IsNotNull(dataFields, "not null");
            Assert.IsTrue(dataFields.Length > 0, "non-empty data fields array");

            var instanceFields = ReflectionUtility.GetAllPublicInstanceProperties(new MockClass().GetType());
            var staticFields = ReflectionUtility.GetAllPublicStaticProperties(new MockClass().GetType());
            Assert.AreEqual(dataFields.Length, instanceFields.Length + staticFields.Length, "all fields = instance fields + static fields");
        }

        [TestMethod]
        public void TestGetAllPublicInstanceProperties()
        {
            var dataFields = ReflectionUtility.GetAllPublicInstanceProperties(new MockClass().GetType());
            Assert.IsNotNull(dataFields, "not null");
            Assert.IsTrue(dataFields.Length > 0, "non-empty data fields array");

            Assert.IsTrue(dataFields.Length < ReflectionUtility.GetAllDataFields(new MockClass().GetType()).Length, "instance fields are subset of all fields");
        }

        [TestMethod]
        public void TestGetAllPublicStaticProperties()
        {
            var staticProps = ReflectionUtility.GetAllPublicStaticProperties(new MockClass().GetType());
            Assert.IsNotNull(staticProps, "not null");
            Assert.IsTrue(staticProps.Length > 0, "non-empty properties array");

            Assert.IsTrue(staticProps.Length < ReflectionUtility.GetAllDataFields(new MockClass().GetType()).Length, "static properties are subset of all properties");
        }

        [TestMethod]
        public void TestGetStaticFieldValue()
        {
            var staticProps = ReflectionUtility.GetAllStaticDataFields(new MockClass().GetType());
            Assert.IsNotNull(staticProps, "not null");
            Assert.IsTrue(staticProps.Length > 0, "non-empty properties array");

            Assert.AreEqual(123, ReflectionUtility.GetStaticFieldValue<int>(staticProps[0]), "static data field value test");
        }

        [TestMethod]
        public void TestGetAllPublicStaticFieldValues()
        {
            int[] staticIntPropValues = ReflectionUtility.GetAllPublicStaticFieldValues<int>(new MockClass().GetType());
            Assert.IsNotNull(staticIntPropValues, "not null");
            Assert.AreEqual(1, staticIntPropValues.Length, "stayic int properties count");
            Assert.AreEqual(321, staticIntPropValues.First(), "static paublic property value");
        }

        [TestMethod]
        public void TestGetNestedTypes()
        {
            var publicNestedTypes = 
                ReflectionUtility.GetNestedTypes(new MockClass().GetType());
            var protectedNestedTypes = 
                ReflectionUtility.GetNestedTypes(new MockClass().GetType(), BindingFlags.NonPublic);
            var allNestedTypes = 
                ReflectionUtility.GetNestedTypes(new MockClass().GetType(), BindingFlags.Public | BindingFlags.NonPublic);

            Assert.IsNotNull(publicNestedTypes);
            Assert.IsNotNull(protectedNestedTypes);
            Assert.IsNotNull(allNestedTypes);
            Assert.AreEqual(1, publicNestedTypes.Length);
            Assert.AreEqual(1, protectedNestedTypes.Length);
            Assert.AreEqual(allNestedTypes.Length, publicNestedTypes.Length + protectedNestedTypes.Length);            
        }

        [TestMethod]
        public void TestGetNestedTypeByName()
        {
            var observedType = new MockClass().GetType();
            var nestedType = 
                ReflectionUtility.GetNestedTypeByName(
                    observedType, 
                    nameof(MockClass.PublicNestedMock)
                    );
            Assert.IsNotNull(nestedType);
            Assert.AreEqual(nameof(MockClass.PublicNestedMock), nestedType.Name);

            var protectedNestedTypeName = "ProtectedNestedMock";
            var protectedNestedType = 
                ReflectionUtility.GetNestedTypeByName(
                    observedType, 
                    protectedNestedTypeName,
                    BindingFlags.NonPublic
                    );
            Assert.IsNotNull(protectedNestedType);
            Assert.AreEqual(protectedNestedTypeName, protectedNestedType.Name);

            Assert.AreNotEqual(protectedNestedType.Name, nestedType.Name);
        }

        [TestMethod]
        public void TestGetSubClassesOf()
        {
            var observedType = typeof(RollbarPackageBase);
            var subclassTypes = 
                ReflectionUtility.GetSubClassesOf(observedType);
            Assert.IsNotNull(subclassTypes);
            Assert.IsTrue(subclassTypes.Length > 0);
        }

        [TestMethod]
        public void TestDoesTypeImplementInterface()
        {
            var observedType = typeof(RollbarPackageBase);
            var interfaceOfInterest = typeof(IRollbarPackage);
            var result = 
                ReflectionUtility.DoesTypeImplementInterface(
                    observedType, 
                    interfaceOfInterest
                    );
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestGetTypesHierarchy()
        {
            var observedType = typeof(ExceptionPackage);
            var typeHierarchy = 
                ReflectionUtility.GetTypesHierarchy(observedType);
            Assert.IsNotNull(typeHierarchy);
            Assert.IsTrue(typeHierarchy.Length > 0);
            Assert.AreSame(observedType, typeHierarchy[0]);
        }

        [TestMethod]
        public void TestGetBaseTypesHierarchy()
        {
            var observedType = typeof(ExceptionPackage);
            var typeHierarchy = 
                ReflectionUtility.GetTypesHierarchy(observedType);
            Assert.IsNotNull(typeHierarchy);
            Assert.IsTrue(typeHierarchy.Length > 0);
            Assert.AreSame(observedType, typeHierarchy[0]);

            var baseTypesHierarchy = 
                ReflectionUtility.GetBaseTypesHierarchy(observedType);
            Assert.IsNotNull(baseTypesHierarchy);
            Assert.IsTrue(baseTypesHierarchy.Length > 0);
            Assert.IsTrue(!baseTypesHierarchy.Contains(observedType));

            Assert.AreEqual(typeHierarchy.Length, baseTypesHierarchy.Length + 1);
        }

        [TestMethod]
        public void TestGetCommonHierarchyTypes()
        {
            var observedType1 = typeof(ExceptionPackage);
            var observedType2 = typeof(MessagePackage);

            var commonTypes = 
                ReflectionUtility.GetCommonHierarchyTypes(observedType1, observedType2);
            Assert.IsNotNull(commonTypes);
            Assert.IsTrue(commonTypes.Length > 0);
            Assert.IsFalse(commonTypes.Contains(observedType1));
            Assert.IsFalse(commonTypes.Contains(observedType2));

            commonTypes = 
                ReflectionUtility.GetCommonHierarchyTypes(observedType1, observedType1);
            Assert.IsNotNull(commonTypes);
            Assert.IsTrue(commonTypes.Length > 0);
            Assert.IsTrue(commonTypes.Contains(observedType1));
            Assert.IsFalse(commonTypes.Contains(observedType2));
        }

        [TestMethod]
        public void TestGetTopCommonSuperType()
        {
            var observedType1 = typeof(ExceptionPackage);
            var observedType2 = typeof(MessagePackage);

            Assert.AreSame(typeof(RollbarPackageBase), ReflectionUtility.GetTopCommonSuperType(observedType1, observedType2));

            Assert.AreSame(observedType1, ReflectionUtility.GetTopCommonSuperType(observedType1, observedType1));
        }

        [TestMethod]
        public void TestGetImplementedInterfaceTypes()
        {
            var observedType = typeof(MockClass);

            var implementedInterfaces = ReflectionUtility.GetImplementedInterfaceTypes(observedType);

            Assert.IsTrue(implementedInterfaces.Contains(typeof(InterfaceA)));
            Assert.IsTrue(implementedInterfaces.Contains(typeof(InterfaceB)));
            Assert.IsFalse(implementedInterfaces.Contains(typeof(InterfaceC)));
        }

        [TestMethod]
        public void TestGetCommonImplementedInterfaces()
        {
            Assert.IsTrue(ReflectionUtility.GetCommonImplementedInterfaces(typeof(MockClass), typeof(MockClass)) != null);
            Assert.IsTrue(ReflectionUtility.GetCommonImplementedInterfaces(typeof(MockClass), typeof(MockClass)).Length > 0);
            Assert.IsTrue(ReflectionUtility.GetCommonImplementedInterfaces(typeof(MockClass), typeof(MockClass)).Contains(typeof(InterfaceA)));
            Assert.IsTrue(ReflectionUtility.GetCommonImplementedInterfaces(typeof(MockClass), typeof(MockClass)).Contains(typeof(InterfaceB)));
            Assert.IsFalse(ReflectionUtility.GetCommonImplementedInterfaces(typeof(MockClass), typeof(MockClass)).Contains(typeof(InterfaceC)));

            Assert.IsTrue(ReflectionUtility.GetCommonImplementedInterfaces(typeof(MockClass), typeof(MockA)) != null);
            Assert.IsTrue(ReflectionUtility.GetCommonImplementedInterfaces(typeof(MockClass), typeof(MockA)).Length > 0);
            Assert.IsTrue(ReflectionUtility.GetCommonImplementedInterfaces(typeof(MockClass), typeof(MockA)).Contains(typeof(InterfaceA)));
            Assert.IsFalse(ReflectionUtility.GetCommonImplementedInterfaces(typeof(MockClass), typeof(MockA)).Contains(typeof(InterfaceB)));

            Assert.IsTrue(ReflectionUtility.GetCommonImplementedInterfaces(new Type[] {typeof(MockClass), typeof(MockA), typeof(MockB), typeof(MockC), }).Length == 0);
            Assert.IsTrue(ReflectionUtility.GetCommonImplementedInterfaces(new Type[] {typeof(MockClass), typeof(MockA), typeof(MockB), }).Length == 0);
            Assert.IsTrue(ReflectionUtility.GetCommonImplementedInterfaces(new Type[] {typeof(MockClass), typeof(MockA), }).Length > 0);
        }
    }


    #region mocking birds

    public interface InterfaceA
    {
    }

    public interface InterfaceB
    {
    }

    public interface InterfaceC
    {

    }

    public class MockA
        : InterfaceA
    {
    }

    public class MockB
        : InterfaceB
    {
    }

    public class MockC
        : InterfaceC
    {
    }


    public class MockClass
        : InterfaceA
        , InterfaceB
    {
        public enum PublicNestedMock
        {
            One,
            Two,
        }

        protected enum ProtectedNestedMock
        {
            One,
            Two,
        }

        public const int StaticDataField = 321;
        public static int StaticProp {get;set;} = 123;
        public int IntProp {get;set;}
        public string StringProp {get;set;} = "String Property Value";
    }

    #endregion mocking birds

}
