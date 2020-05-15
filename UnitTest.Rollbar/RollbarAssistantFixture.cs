#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    [TestClass]
    [TestCategory(nameof(RollbarAssistantFixture))]
    public class RollbarAssistantFixture
    {
        [TestInitialize]
        public void SetupFixture()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [TestCleanup]
        public void TearDownFixture()
        {
        }

        [TestMethod]
        public void TestCaptureStateOfEnum_KindOf()
        {
            var objUnderTest = EnumType.Two;

            var stateVars = RollbarAssistant.CaptureState(objUnderTest, nameof(objUnderTest));

            Assert.IsNull(stateVars);

            stateVars = RollbarAssistant.CaptureState(typeof(EnumType));

            Assert.IsNull(stateVars);
        }

        [TestMethod]
        public void TestCaptureStateOfInterface_KindOf()
        {
            var stateVars = RollbarAssistant.CaptureState(typeof(IInterface));

            Assert.IsNull(stateVars);
        }

        [TestMethod]
        public void TestCaptureStateOfObjectInstance()
        {
            var objUnderTest = new InstanceType();

            var stateVars = RollbarAssistant.CaptureState(objUnderTest, nameof(objUnderTest));

            Assert.AreEqual(2 + 4, stateVars.Count);

            foreach(var key in stateVars.Keys)
            {
                Console.WriteLine($"{key} = {stateVars[key]}");
            }
        }

        [TestMethod]
        public void TestCaptureStateOfStaticType()
        {
            var stateVars = RollbarAssistant.CaptureState(typeof(StaticType));

            Assert.AreEqual(4, stateVars.Count);

            foreach (var key in stateVars.Keys)
            {
                Console.WriteLine($"{key} = {stateVars[key]}");
            }
        }

        [TestMethod]
        public void TestCombinedStateCapture()
        {
            var objUnderTest = new InstanceType();

            var combinedState = RollbarAssistant.CaptureState(objUnderTest, nameof(objUnderTest));
            RollbarAssistant.CaptureState(typeof(StaticType), combinedState);

            Assert.AreEqual(
                RollbarAssistant.CaptureState(objUnderTest, nameof(objUnderTest)).Count + RollbarAssistant.CaptureState(typeof(StaticType)).Count
                , combinedState.Count
                );
        }

        #region data mocks

        enum EnumType
        {
            One,
            Two,
        }

        interface IInterface
        {
            int IntProperty { get; set; }
            string ROStringProperty { get; }
        }


        static class StaticType
        {
            // 1
            private const int BaseConstant = 10;



            // 2
#pragma warning disable CS0414 // The field is assigned but its value is never used
            private static int _baseIntField = 3;
#pragma warning restore CS0414 // The field is assigned but its value is never used

            // 3
            public static object BaseNullProperty
            {
                get { return StaticType._baseNullField; }
            }
            private static object _baseNullField = null;

            // 4
            public static string BaseAutoProperty { get; set; } = "BaseAutoProperty value";
        }

        class InstanceType
            : InstanceTypeBase
        {
            // 1
            public int AutoProperty { get; } = 99;

            // 2
            public static string TypeName { get; } = nameof(InstanceType);
        }

        abstract class InstanceTypeBase
        {
            // 1
            private const int BaseConstant = 10;

            // 2
#pragma warning disable CS0414 // The field is assigned but its value is never used
            private int _baseIntField = 3;
#pragma warning restore CS0414 // The field is assigned but its value is never used

            // 3
            public object BaseNullProperty
            {
                get { return this._baseNullField; }
            }
            private object _baseNullField = null;

            // 4
            public string BaseAutoProperty { get; set; } = "BaseAutoProperty value";
        }

        #endregion data mocks

    }
}
