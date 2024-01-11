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
    using System.Threading;
    using log4net;
    using log4net.Config;
    using global::Rollbar.Infrastructure;

    /// <summary>
    /// Defines test class RollbarAppenderFixture.
    /// </summary>
    [TestClass]
    [TestCategory(nameof(RollbarAppenderFixture))]
    public class RollbarAppenderFixture
    {
        /// <summary>
        /// The rollbar comm events
        /// </summary>
        private readonly List<CommunicationEventArgs> _rollbarCommEvents = new List<CommunicationEventArgs>();

        /// <summary>
        /// The rollbar comm error events
        /// </summary>
        private readonly List<CommunicationErrorEventArgs> _rollbarCommErrorEvents = new List<CommunicationErrorEventArgs>();

        /// <summary>
        /// Setups the fixture.
        /// </summary>
        [TestInitialize]
        public void SetupFixture()
        {
            RollbarInfrastructureConfig infrastructureConfig = new RollbarInfrastructureConfig(RollbarUnitTestSettings.AccessToken, RollbarUnitTestSettings.Environment);
            RollbarInfrastructure.Instance.Init(infrastructureConfig);
            this._rollbarCommEvents.Clear();
            this._rollbarCommErrorEvents.Clear();
            RollbarQueueController.Instance.FlushQueues();
            RollbarQueueController.Instance.InternalEvent += Instance_InternalEvent;
        }

        /// <summary>
        /// Handles the InternalEvent event of the Instance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RollbarEventArgs"/> instance containing the event data.</param>
        private void Instance_InternalEvent(object sender, RollbarEventArgs e)
        {
            string eventTrace = $"##################{Environment.NewLine}{e.TraceAsString()}{Environment.NewLine}";
            Console.WriteLine(eventTrace);
            System.Diagnostics.Trace.WriteLine(eventTrace);

            CommunicationEventArgs communicationEventArgs = e as CommunicationEventArgs;
            if (communicationEventArgs != null)
            {
                this._rollbarCommEvents.Add(communicationEventArgs);
            }

            CommunicationErrorEventArgs communicationErrorEventArgs = e as CommunicationErrorEventArgs;
            if (communicationErrorEventArgs != null)
            {
                this._rollbarCommErrorEvents.Add(communicationErrorEventArgs);
            }

        }

        /// <summary>
        /// Tears down fixture.
        /// </summary>
        [TestCleanup]
        public void TearDownFixture()
        {
            RollbarQueueController.Instance.InternalEvent -= Instance_InternalEvent;

        }

        /// <summary>
        /// Defines the test method TestAppenderReconfiguration.
        /// </summary>
        [Ignore]
        [TestMethod]
        public void TestAppenderReconfiguration()
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

            RollbarLoggerConfig newConfig = new RollbarLoggerConfig();
            newConfig.Reconfigure(appender.RollbarConfig);
            newConfig.RollbarPayloadAdditionOptions.Person = expectedPersons[1];
            appender.RollbarConfig.Reconfigure(newConfig);
            log.Info("Via log4net");

            newConfig = new RollbarLoggerConfig();
            newConfig.Reconfigure(appender.RollbarConfig);
            newConfig.RollbarPayloadAdditionOptions.Person = expectedPersons[2];
            RollbarDataSecurityOptions dataSecurityOptions = new RollbarDataSecurityOptions();
            dataSecurityOptions.ScrubFields = new string[]
            {
                "log4net:UserName",
                "log4net:HostName",
                "log4net:Identity",
            };
            newConfig.RollbarDataSecurityOptions.Reconfigure(dataSecurityOptions);

            appender.RollbarConfig.Reconfigure(newConfig);
            log.Info("Via log4net");

            // wait until all the payloads are processed and transmitted
            Thread.Sleep(TimeSpan.FromSeconds(5));

            Assert.IsTrue(this._rollbarCommEvents.Count == 3 || this._rollbarCommErrorEvents.Count == 3, "Either comm successes or errors are captured...");

            if (this._rollbarCommErrorEvents.Count == 3)
            {
                //this scenario happens on the CI server (Azure Pipelines):
                foreach (var commErrorEvent in this._rollbarCommErrorEvents)
                {
                    Assert.IsTrue(
                        commErrorEvent.Error.Message.Contains(
                            "Preliminary ConnectivityMonitor detected offline status!"),
                        "Matching error message."
                    );
                }
                Assert.IsFalse(this._rollbarCommErrorEvents[0].Payload.Contains("\"person\":{\"id\":"), "checking this._rollbarCommErrorEvents[0].Payload");
                Assert.IsTrue(this._rollbarCommErrorEvents[1].Payload.Contains(expectedPersons[1].Id), "checking this._rollbarCommErrorEvents[1].Payload");
                Assert.IsTrue(this._rollbarCommErrorEvents[2].Payload.Contains(expectedPersons[2].Id), "checking this._rollbarCommErrorEvents[2].Payload");
            }
            else
            {
                Assert.IsFalse(this._rollbarCommEvents[0].Payload.Contains("\"person\":{\"id\":"), "checking this._rollbarCommEvents[0].Payload");
                Assert.IsTrue(this._rollbarCommEvents[1].Payload.Contains(expectedPersons[1].Id), "checking this._rollbarCommEvents[1].Payload");
                Assert.IsTrue(this._rollbarCommEvents[2].Payload.Contains(expectedPersons[2].Id), "checking this._rollbarCommEvents[2].Payload");
            }
        }
    }
}
