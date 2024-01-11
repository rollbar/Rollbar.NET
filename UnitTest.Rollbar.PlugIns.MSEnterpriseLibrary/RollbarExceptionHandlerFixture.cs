namespace UnitTest.Rollbar.PlugIns.MSEnterpriseLibrary
{
    using Benchmarker.Common;
    using global::Rollbar;
    using global::Rollbar.Infrastructure;
    using global::Rollbar.PlugIns.MSEnterpriseLibrary;
    using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    using UnitTest.RollbarTestCommon;

    [TestClass]
    [TestCategory(nameof(RollbarExceptionHandlerFixture))]
    public class RollbarExceptionHandlerFixture
    {
        private int _rollbarCommunicationEventsCount = 0;

        [TestInitialize]
        public void SetupFixture()
        {
            RollbarUnitTestEnvironmentUtil.SetupLiveTestRollbarInfrastructure();
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

        [Ignore]
        [TestMethod]
        public void TestBasics()
        {
            _rollbarCommunicationEventsCount = 0;

            IExceptionHandler exceptionHandler = null;
            const int totalExceptionStackFrames = 10;
            int expectedCount = 0;

            exceptionHandler = new RollbarExceptionHandler(RollbarUnitTestSettings.AccessToken, RollbarUnitTestSettings.Environment, null);
            exceptionHandler.HandleException(
                ExceptionSimulator.GetExceptionWith(totalExceptionStackFrames, "RollbarExceptionHandlerFixture: TestBasics non-blocking..."),
                Guid.NewGuid()
                );
            expectedCount++;

            exceptionHandler = new RollbarExceptionHandler(RollbarUnitTestSettings.AccessToken, RollbarUnitTestSettings.Environment, TimeSpan.FromSeconds(5));
            exceptionHandler.HandleException(
                ExceptionSimulator.GetExceptionWith(totalExceptionStackFrames,"RollbarExceptionHandlerFixture: TestBasics blocking..."),
                Guid.NewGuid()
                );
            expectedCount++;

            Assert.AreEqual(expectedCount, _rollbarCommunicationEventsCount);
        }
    }
}
