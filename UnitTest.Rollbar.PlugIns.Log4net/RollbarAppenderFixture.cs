namespace UnitTest.Rollbar.PlugIns.Log4net
{
    using Benchmarker.Common;
    using global::Rollbar;
    using global::Rollbar.DTOs;
    using global::Rollbar.PlugIns.Log4net;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using log4net.Core;
    using System.Collections.Generic;
    using log4net;
    using log4net.Config;

    /// <summary>
    /// Defines test class RollbarAppenderFixture.
    /// </summary>
    [TestClass]
    [TestCategory(nameof(RollbarAppenderFixture))]
    public class RollbarAppenderFixture
    {
        /// <summary>
        /// The rolbar comm events
        /// </summary>
        private readonly List<CommunicationEventArgs> _rollbarCommEvents = new List<CommunicationEventArgs>();

        /// <summary>
        /// Setups the fixture.
        /// </summary>
        [TestInitialize]
        public void SetupFixture()
        {
            RollbarQueueController.Instance.InternalEvent += Instance_InternalEvent;
        }

        /// <summary>
        /// Handles the InternalEvent event of the Instance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RollbarEventArgs"/> instance containing the event data.</param>
        private void Instance_InternalEvent(object sender, RollbarEventArgs e)
        {
            CommunicationEventArgs communicationEventArgs = e as CommunicationEventArgs;
            if (e != null)
            {
                this._rollbarCommEvents.Add(communicationEventArgs);
            }
        }

        /// <summary>
        /// Tears down fixture.
        /// </summary>
        [TestCleanup]
        public void TearDownFixture()
        {

        }

        /// <summary>
        /// Defines the test method TestBasics.
        /// </summary>
        [TestMethod]
        public void TestBasics()
        {
            Person[] expectedPersons = new Person[]
            {
                null,
                new Person("Person1"),
                new Person("Person2"),
            };

            RollbarAppender appender = new RollbarAppender(
                RollbarUnitTestSettings.AccessToken, 
                RollbarUnitTestSettings.Environment, 
                TimeSpan.FromSeconds(3)
                );

            string repositoryName = typeof(RollbarAppenderFixture).Name;
            var repository = LoggerManager.CreateRepository(repositoryName);
            string loggerName = typeof(RollbarAppenderFixture).Name; 
            BasicConfigurator.Configure(repository, appender);
            ILog log = LogManager.GetLogger(repositoryName, loggerName);

         
            log.Info("Via log4net");
            Assert.AreEqual(1, this._rollbarCommEvents.Count);

            RollbarConfig newConfig = new RollbarConfig();
            newConfig.Reconfigure(appender.RollbarConfig);
            newConfig.Person = expectedPersons[1];
            appender.RollbarConfig.Reconfigure(newConfig);
            log.Info("Via log4net");
            Assert.AreEqual(2, this._rollbarCommEvents.Count);

            newConfig = new RollbarConfig();
            newConfig.Reconfigure(appender.RollbarConfig);
            newConfig.Person = expectedPersons[2];
            appender.RollbarConfig.Reconfigure(newConfig);
            log.Info("Via log4net");
            Assert.AreEqual(3, this._rollbarCommEvents.Count);

            Assert.IsFalse(this._rollbarCommEvents[0].Payload.Contains("Person"));
            Assert.IsTrue(this._rollbarCommEvents[1].Payload.Contains(expectedPersons[1].Id));
            Assert.IsTrue(this._rollbarCommEvents[2].Payload.Contains(expectedPersons[2].Id));
        }
    }
}
