#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace UnitTest.Rollbar
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Threading;

    using UnitTest.RollbarTestCommon;

    using global::Rollbar;
    using global::Rollbar.Infrastructure;

    /// <summary>
    /// Defines test class RollbarLiveFixtureBase.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    /// <remarks>This is a base abstraction for creating live Rollbar unit tests
    /// (ones that actually expected to communicate with the Rollbar API).
    /// It allows to set expectations for internal Rollbar events
    /// (as payload delivery to Rollbar API or any communication or internal errors).
    /// It has built-in verification of actual event counts against the expected ones per type of the events.</remarks>
    //[TestClass]
    //[TestCategory(nameof(RollbarLiveFixtureBase))]
    public abstract class RollbarLiveFixtureBase
        : IDisposable
    {
        /// <summary>
        /// The logger configuration
        /// </summary>
        private IRollbarLoggerConfig _loggerConfig;

        /// <summary>
        /// The disposable rollbar instances
        /// </summary>
        private readonly List<IRollbar> _disposableRollbarInstances = new List<IRollbar>();

        /// <summary>
        /// The default rollbar timeout
        /// </summary>
        protected static readonly TimeSpan defaultRollbarTimeout = TimeSpan.FromSeconds(3);

        protected static readonly RollbarInfrastructureConfig infrastructureConfig;
        static RollbarLiveFixtureBase()
        {
            RollbarUnitTestEnvironmentUtil.SetupLiveTestRollbarInfrastructure();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="RollbarLiveFixtureBase"/> class.
        /// </summary>
        protected RollbarLiveFixtureBase()
        {
            RollbarQueueController.Instance.InternalEvent += OnRollbarInternalEvent;
        }

        /// <summary>
        /// Sets the fixture up.
        /// </summary>
        //[TestInitialize]
        public virtual void SetupFixture()
        {
            RollbarUnitTestEnvironmentUtil.SetupLiveTestRollbarInfrastructure();

            RollbarDataSecurityOptions dataSecurityOptions = new RollbarDataSecurityOptions();
            dataSecurityOptions.ScrubFields = new string[] { "secret", "super_secret", };
            RollbarInfrastructure.Instance
                .Config
                .RollbarLoggerConfig
                .RollbarDataSecurityOptions
                .Reconfigure(dataSecurityOptions);

            this._loggerConfig = RollbarInfrastructure.Instance.Config.RollbarLoggerConfig;

            this.Reset();
        }

        /// <summary>
        /// Tears down this fixture.
        /// </summary>
        //[TestCleanup]
        public virtual void TearDownFixture()
        {
            TimeSpan timeout = RollbarQueueController.Instance.GetRecommendedTimeout();
            Thread.Sleep(timeout);
            this.VerifyActualEventsAgainstExpectedTotals();
        }

        /// <summary>
        /// Handles the <see cref="E:RollbarInternalEvent" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RollbarEventArgs"/> instance containing the event data.</param>
        private void OnRollbarInternalEvent(object sender, RollbarEventArgs e)
        {
            if(!(e is CommunicationEventArgs))
            {
            }

            // for basic RollbarRateLimitVerification test:
            //switch(e)
            //{
            //    case RollbarApiErrorEventArgs apiErrorEvent:
            //        //this.ApiErrorEvents.Add(apiErrorEvent);
            //        return;
            //    case CommunicationEventArgs commEvent:
            //        Console.WriteLine(commEvent.EventTimestamp + " SENT: ");
            //        return;
            //    case CommunicationErrorEventArgs commErrorEvent:
            //        //this.CommunicationErrorEvents.Add(commErrorEvent);
            //        return;
            //    case InternalErrorEventArgs internalErrorEvent:
            //        //this.InternalSdkErrorEvents.Add(internalErrorEvent);
            //        return;
            //    case PayloadDropEventArgs payloadDropEvent:
            //        Console.WriteLine(payloadDropEvent.EventTimestamp + " DROP: " + payloadDropEvent.Reason);
            //        return;
            //    default:
            //        //Assert.Fail("Unexpected RollbarEventArgs specialization type!");
            //        return;
            //}

            //Console.WriteLine(e.TraceAsString());
            //Trace.WriteLine(e.TraceAsString());

            this.Register(e);
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        protected void Reset()
        {
            this.ClearAllExpectedEventCounts();
            this.ClearAllRollbarEvents();

            RollbarInfrastructure.Instance.QueueController.FlushQueues();
        }



        /// <summary>
        /// Makes the sure all the payloads processed.
        /// </summary>
        private void MakeSureAllThePayloadsProcessed()
        {
            Thread.Sleep(RollbarQueueController.Instance.GetRecommendedTimeout().Add(TimeSpan.FromSeconds(1)));
            Assert.AreEqual(0, RollbarQueueController.Instance.GetTotalPayloadCount(), "All the payloads are expected to be out of the queues...");
        }
        /// <summary>
        /// Verifies the actual events against expected totals.
        /// </summary>
        private void VerifyActualEventsAgainstExpectedTotals()
        {
            MakeSureAllThePayloadsProcessed();

            foreach (var eventType in this._expectedEventCountByType.Keys)
            {
                string message = $"Matching count of {eventType.Name} events...";
                Console.WriteLine(message);
                Trace.WriteLine(message);
                Assert.AreEqual(this._expectedEventCountByType[eventType], this._rollbarEventsByType[eventType].Count, message);
            }
        }

        #region Actual Rollbar events

        /// <summary>
        /// The rollbar events by type
        /// </summary>
        private readonly IDictionary<Type, List<RollbarEventArgs>> _rollbarEventsByType = new ConcurrentDictionary<Type, List<RollbarEventArgs>>();

        /// <summary>
        /// Registers the specified rollbar event.
        /// </summary>
        /// <param name="rollbarEvent">The <see cref="RollbarEventArgs"/> instance containing the event data.</param>
        private void Register(RollbarEventArgs rollbarEvent)
        {
            if (rollbarEvent == null)
            {
                return;
            }

            var eventType = rollbarEvent.GetType();
            if (this._rollbarEventsByType.TryGetValue(eventType, out var rollbarEvents))
            {
                rollbarEvents.Add(rollbarEvent);
            }
            else
            {
                this._rollbarEventsByType.Add(
                    eventType,
                    new List<RollbarEventArgs>(new[] {rollbarEvent})
                    );
            }
        }

        /// <summary>
        /// Gets all events.
        /// </summary>
        /// <typeparam name="TRollbarEvent">The type of the t rollbar event.</typeparam>
        /// <returns>IReadOnlyCollection&lt;TRollbarEvent&gt;.</returns>
        protected IReadOnlyCollection<TRollbarEvent> GetAllEvents<TRollbarEvent>()
            where TRollbarEvent : RollbarEventArgs
        {
            if (this._rollbarEventsByType.TryGetValue(typeof(TRollbarEvent), out var rollbarEvents))
            {
                return rollbarEvents.Cast<TRollbarEvent>().ToArray();
            }
            else
            {
                return Array.Empty<TRollbarEvent>();
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <typeparam name="TRollbarEvent">The type of the t rollbar event.</typeparam>
        /// <returns>System.Int32.</returns>
        protected int GetCount<TRollbarEvent>()
            where TRollbarEvent : RollbarEventArgs
        {
            if (this._rollbarEventsByType.TryGetValue(typeof(TRollbarEvent), out var rollbarEvents))
            {
                return rollbarEvents.Count;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Clears all rollbar events.
        /// </summary>
        protected void ClearAllRollbarEvents()
        {
            foreach (var eventType in this._rollbarEventsByType.Keys)
            {
                this._rollbarEventsByType[eventType].Clear();
            }
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <typeparam name="TRollbarEvent">The type of the t rollbar event.</typeparam>
        protected void Clear<TRollbarEvent>()
            where TRollbarEvent : RollbarEventArgs
        {
            if (this._rollbarEventsByType.TryGetValue(typeof(TRollbarEvent), out var rollbarEvents))
            {
                rollbarEvents.Clear();
            }
        }

        #endregion Actual Rollbar events

        #region Expected Rollbar events

        /// <summary>
        /// The expected event count by type
        /// </summary>
        private readonly IDictionary<Type, int> _expectedEventCountByType = new ConcurrentDictionary<Type, int>();

        /// <summary>
        /// Increments the count.
        /// </summary>
        /// <typeparam name="TRollbarEvent">The type of the t rollbar event.</typeparam>
        protected void IncrementCount<TRollbarEvent>()
            where TRollbarEvent : RollbarEventArgs
        {
            const int countIncrement = 1;
            this.IncrementCount<TRollbarEvent>(countIncrement);
        }

        /// <summary>
        /// Increments the count.
        /// </summary>
        /// <typeparam name="TRollbarEvent">The type of the t rollbar event.</typeparam>
        /// <param name="countIncrement">The count increment.</param>
        protected void IncrementCount<TRollbarEvent>(int countIncrement)
            where TRollbarEvent : RollbarEventArgs
        {
            var eventType = typeof(TRollbarEvent);
            if (this._expectedEventCountByType.TryGetValue(eventType, out var eventsCount))
            {
                this._expectedEventCountByType[eventType] += countIncrement;
            }
            else
            {
                this._expectedEventCountByType.Add(
                    eventType,
                    countIncrement
                );
            }
        }

        /// <summary>
        /// Clears all expected event counts.
        /// </summary>
        protected void ClearAllExpectedEventCounts()
        {
            foreach (var eventType in this._expectedEventCountByType.Keys)
            {
                this._expectedEventCountByType[eventType] = 0;
            }
        }

        /// <summary>
        /// Clears the expected count.
        /// </summary>
        /// <typeparam name="TRollbarEvent">The type of the t rollbar event.</typeparam>
        protected void ClearExpectedCount<TRollbarEvent>()
            where TRollbarEvent : RollbarEventArgs
        {
            var eventType = typeof(TRollbarEvent);
            if (this._expectedEventCountByType.TryGetValue(eventType, out var eventCount))
            {
                this._expectedEventCountByType[eventType] = 0;
            }
        }

        #endregion Actual Rollbar events

        /// <summary>
        /// Provides the live rollbar configuration.
        /// </summary>
        /// <returns>IRollbarConfig.</returns>
        protected IRollbarLoggerConfig ProvideLiveRollbarConfig()
        {
            return this.ProvideLiveRollbarConfig(RollbarUnitTestSettings.AccessToken, RollbarUnitTestSettings.Environment);
        }

        /// <summary>
        /// Provides the live rollbar configuration.
        /// </summary>
        /// <param name="rollbarAccessToken">The rollbar access token.</param>
        /// <param name="rollbarEnvironment">The rollbar environment.</param>
        /// <returns>IRollbarConfig.</returns>
        protected IRollbarLoggerConfig ProvideLiveRollbarConfig(string rollbarAccessToken, string rollbarEnvironment)
        {
            if (this._loggerConfig == null)
            {
                RollbarDestinationOptions destinationOptions =
                    new RollbarDestinationOptions(rollbarAccessToken, rollbarEnvironment);

                RollbarDataSecurityOptions dataSecurityOptions = new RollbarDataSecurityOptions();
                dataSecurityOptions.ScrubFields = new string[] { "secret", "super_secret", };

                RollbarLoggerConfig loggerConfig = new RollbarLoggerConfig();
                loggerConfig.RollbarDestinationOptions.Reconfigure(destinationOptions);
                loggerConfig.RollbarDataSecurityOptions.Reconfigure(dataSecurityOptions);

                this._loggerConfig = loggerConfig;
            }
            return this._loggerConfig;
        }

        /// <summary>
        /// Provides the disposable rollbar.
        /// </summary>
        /// <returns>IRollbar.</returns>
        protected IRollbar ProvideDisposableRollbar()
        {
            IRollbar rollbar = RollbarFactory.CreateNew(config: this.ProvideLiveRollbarConfig());
            this._disposableRollbarInstances.Add(rollbar);
            return rollbar;
        }

        /// <summary>
        /// Provides the shared rollbar.
        /// </summary>
        /// <returns>IRollbar.</returns>
        protected IRollbar ProvideSharedRollbar()
        {
            if (!RollbarLocator.RollbarInstance.Equals(ProvideLiveRollbarConfig()))
            {
                RollbarLocator.RollbarInstance.Configure(ProvideLiveRollbarConfig());
            }
            return RollbarLocator.RollbarInstance;
        }

        /// <summary>
        /// Verifies the instance operational.
        /// </summary>
        /// <param name="rollbar">The rollbar.</param>
        protected void VerifyInstanceOperational(IRollbar rollbar)
        {
            MakeSureAllThePayloadsProcessed();

            //Assert.IsTrue(0 == RollbarQueueController.Instance.GetTotalPayloadCount(), "Making sure all the queues are clear...");
            int initialCommunicationEventsCount = this.GetCount<CommunicationEventArgs>();
            this.IncrementCount<CommunicationEventArgs>();
            rollbar.AsBlockingLogger(defaultRollbarTimeout).Critical("Making sure Rollbar.NET is operational...");
            Assert.AreEqual(this.GetCount<CommunicationEventArgs>(), initialCommunicationEventsCount + 1, "Confirming Rollbar.NET is operational...");
        }

        [Ignore]
        [TestMethod]
        public void _VerifyInstanceOperationalTest()
        {
            // this test more about verifying if the test harness itself works well:

            this.ClearAllRollbarEvents();

            using(IRollbar rollbar = this.ProvideDisposableRollbar())
            {
                this.VerifyInstanceOperational(rollbar);
            }
        }


        #region IDisposable Support

        /// <summary>
        /// The disposed value
        /// </summary>
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    TimeSpan timeout = RollbarQueueController.Instance.GetRecommendedTimeout();
                    Thread.Sleep(timeout);
                    RollbarQueueController.Instance.InternalEvent -= OnRollbarInternalEvent;
                    foreach (var rollbar in this._disposableRollbarInstances)
                    {
                        rollbar.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RollbarLiveFixtureBase() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
