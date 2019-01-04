namespace UnitTest.Rollbar.PlugIns.MSEnterpriseLibrary
{
    using global::Rollbar;
    using global::Rollbar.PlugIns.MSEnterpriseLibrary;
    using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    [TestClass]
    public class RollbarExceptionHandlerFixture
    {
        private int _rollbarCommunicationEventsCount = 0;

        [TestInitialize]
        public void SetupFixture()
        {
            RollbarQueueController.Instance.InternalEvent += Instance_InternalEvent;
        }

        private void Instance_InternalEvent(object sender, RollbarEventArgs e)
        {
            CommunicationEventArgs communicationEventArgs = e as CommunicationEventArgs;
            if (e != null)
            {
                _rollbarCommunicationEventsCount++;
            }
        }

        [TestCleanup]
        public void TearDownFixture()
        {

        }

        [TestMethod]
        public void TestBasics()
        {
            _rollbarCommunicationEventsCount = 0;
            
            IExceptionHandler exceptionHandler = null;
            int expectedCount = 0;

            exceptionHandler = new RollbarExceptionHandler(RollbarUnitTestSettings.AccessToken, RollbarUnitTestSettings.Environment, null);
            exceptionHandler.HandleException(new Exception("RollbarExceptionHandlerFixture: TestBasics non-blocking..."), Guid.NewGuid());
            expectedCount++;

            exceptionHandler = new RollbarExceptionHandler(RollbarUnitTestSettings.AccessToken, RollbarUnitTestSettings.Environment, TimeSpan.FromSeconds(5));
            exceptionHandler.HandleException(new Exception("RollbarExceptionHandlerFixture: TestBasics blocking..."), Guid.NewGuid());
            expectedCount++;

            Assert.AreEqual(expectedCount, _rollbarCommunicationEventsCount);
        }
    }
}
