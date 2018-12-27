#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using global::Rollbar;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    //using System.Linq;
    //using System.Threading;
    //using System.Threading.Tasks;
    //using global::Rollbar.DTOs;
    //using Exception = System.Exception;

    [TestClass]
    [TestCategory(nameof(RollbarUtilFixture))]
    public class RollbarUtilFixture
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
        public void TestSnapExceptionDataAsCustomData()
        {
            IDictionary<string, object> customData = null;

            var exceptionData = new[] {
                new { Key = 1, Value = "one"  },
                new { Key = 2, Value = "two"  },
                new { Key = 3, Value = "three"  },
            };

            var mostInnerException = new ArgumentException("Most Inner exception.");
            for (int dataIndx = 0; dataIndx <= 1; dataIndx++)
            {
                mostInnerException.Data[exceptionData[dataIndx].Key] = exceptionData[dataIndx].Value;
            }
            Assert.IsNull(customData);
            //expected to allocate customData and add some entries:
            RollbarUtility.SnapExceptionDataAsCustomData(mostInnerException, ref customData);
            Assert.IsNotNull(customData);
            Assert.AreEqual(2, customData.Count);

            var innerException = new NullReferenceException("Inner exception.", mostInnerException);
            for (int dataIndx = 1; dataIndx <= 2; dataIndx++)
            {
                innerException.Data[exceptionData[dataIndx].Key] = exceptionData[dataIndx].Value;
            }
            //expected to append more entries:
            RollbarUtility.SnapExceptionDataAsCustomData(innerException, ref customData);
            Assert.IsNotNull(customData);
            Assert.AreEqual(4, customData.Count);
            //expected to not double-enter same entries:
            RollbarUtility.SnapExceptionDataAsCustomData(innerException, ref customData);
            Assert.IsNotNull(customData);
            Assert.AreEqual(4, customData.Count);

            var ex = new Exception("Exception", innerException);
            for (int dataIndx = 0; dataIndx <= 2; dataIndx++)
            {
                ex.Data[exceptionData[dataIndx].Key] = exceptionData[dataIndx].Value;
            }
            ex.Data["nullValueKey"] = null;
            //expected to append more entries:
            RollbarUtility.SnapExceptionDataAsCustomData(ex, ref customData);
            Assert.IsNotNull(customData);
            Assert.AreEqual(8, customData.Count);

            customData = null;
            var aggregateException = new AggregateException("Aggregate Exception", innerException, mostInnerException, ex);
            aggregateException.Data["aggregateKey"] = "Aggregate Value";
            //expected to allocate cuastomData and add entries:
            RollbarUtility.SnapExceptionDataAsCustomData(aggregateException, ref customData);
            Assert.IsNotNull(customData);
            Assert.AreEqual(9, customData.Count);
        }

    }

}
