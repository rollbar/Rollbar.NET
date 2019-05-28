#define TRACE

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar
{
    using Rollbar.Common;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using Rollbar.NetStandard;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;

#if NETFX
    using System.Web.Hosting;
#endif

    /// <summary>
    /// RollbarQueueController singleton.
    /// It keeps track of payload queues of every instance of RollbarLogger.
    /// It is also responsible for async processing of queues on its own worker thread
    /// (including retries as necessary).
    /// It makes sure that Rollbar access token rate limits handled properly.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class RollbarQueueController
        : IDisposable
#if NETFX
        , IRegisteredObject
#endif
    {
        #region singleton implementation

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static RollbarQueueController Instance
        {
            get
            {
                return NestedSingleInstance.Instance;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarQueueController" /> class from being created.
        /// </summary>
        private RollbarQueueController()
        {
            this.Start();
        }

        /// <summary>
        /// Class NestedSingleInstance. This class cannot be inherited.
        /// </summary>
        private sealed class NestedSingleInstance
        {
            /// <summary>
            /// Prevents a default instance of the <see cref="NestedSingleInstance"/> class from being created.
            /// </summary>
            private NestedSingleInstance()
            {
            }

            /// <summary>
            /// The instance
            /// </summary>
            internal static readonly RollbarQueueController Instance =
                new RollbarQueueController();
        }

        #endregion singleton implementation

        /// <summary>
        /// The trace source
        /// </summary>
        private static readonly TraceSource traceSource = new TraceSource(typeof(RollbarQueueController).FullName);

        /// <summary>
        /// Enum PayloadTraceSources
        /// </summary>
        public enum PayloadTraceSources
        {
            /// <summary>
            /// The rollbar transmitted payloads
            /// </summary>
            RollbarTransmittedPayloads,

            /// <summary>
            /// The rollbar omitted payloads
            /// </summary>
            RollbarOmittedPayloads,
        }

        private static readonly TraceSource transmittedPayloadsTraceSource = new TraceSource(PayloadTraceSources.RollbarTransmittedPayloads.ToString());
        private static readonly TraceSource omittedPayloadsTraceSource = new TraceSource(PayloadTraceSources.RollbarOmittedPayloads.ToString());

        /// <summary>
        /// The sleep interval
        /// </summary>
        internal readonly TimeSpan _sleepInterval = TimeSpan.FromMilliseconds(25);

        /// <summary>
        /// The total retries
        /// </summary>
        internal readonly int _totalRetries = 3;

        /// <summary>
        /// The HTTP clients by proxy settings
        /// </summary>
        private readonly ConcurrentDictionary<string, HttpClient> _httpClientsByProxySettings = 
            new ConcurrentDictionary<string, HttpClient>();

        /// <summary>
        /// Provides the HTTP client.
        /// </summary>
        /// <param name="proxyAddress">The proxy address.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="proxyPassword">The proxy password.</param>
        /// <returns>HttpClient.</returns>
        internal HttpClient ProvideHttpClient(string proxyAddress = null, string proxyUsername = null, string proxyPassword = null)
        {
            string proxySettings = string.Empty;
            if (!string.IsNullOrWhiteSpace(proxyAddress) 
                || !string.IsNullOrWhiteSpace(proxyUsername) 
                || !string.IsNullOrWhiteSpace(proxyPassword)
                )
            {
                proxySettings = $"{proxyAddress?.ToLower()}+{proxyUsername?.ToLower()}+{proxyPassword?.ToLower()}";
            }

            HttpClient httpClient = null;
            if (this._httpClientsByProxySettings.TryGetValue(proxySettings, out httpClient))
            {
                return httpClient;
            }

            httpClient = HttpClientUtility.CreateHttpClient(proxyAddress, proxyUsername, proxyPassword);
            if (this._httpClientsByProxySettings.TryAdd(proxySettings, httpClient))
            {
                return httpClient;
            }
            else if (this._httpClientsByProxySettings.TryGetValue(proxySettings, out httpClient))
            {
                return httpClient;
            }
            else
            {
                this.OnRollbarEvent(
                    new InternalErrorEventArgs(null, null, null, $"ProvideHttpClient(...) is completely messed up for {proxySettings}!")
                    );
            }

            return null;
        }

        /// <summary>
        /// Occurs after a Rollbar internal event happens.
        /// </summary>
        public event EventHandler<RollbarEventArgs> InternalEvent;

        /// <summary>
        /// Registers the specified queue.
        /// </summary>
        /// <param name="queue">The queue.</param>
        internal void Register(PayloadQueue queue)
        {
            lock (this._syncLock)
            {
                Assumption.AssertTrue(!this._allQueues.Contains(queue), nameof(queue));

                this._allQueues.Add(queue);
                this.IndexByToken(queue);
                ((RollbarConfig) queue.Logger.Config).Reconfigured += Config_Reconfigured;

                // The following debug line causes stack overflow when RollbarTraceListener is activated:                
                Debug.WriteLineIf(RollbarTraceListener.InstanceCount == 0, this.GetType().Name + ": Registered a queue. Total queues count: " + this._allQueues.Count + ".");
            }
        }

        /// <summary>
        /// Unregisters the specified queue.
        /// </summary>
        /// <param name="queue">The queue.</param>
        private void Unregister(PayloadQueue queue)
        {
            lock (this._syncLock)
            {
                Assumption.AssertTrue(!queue.Logger.IsSingleton, nameof(queue.Logger.IsSingleton));
                Assumption.AssertTrue(this._allQueues.Contains(queue), nameof(queue));

                this.DropIndexByToken(queue);
                this._allQueues.Remove(queue);
                ((RollbarConfig)queue.Logger.Config).Reconfigured -= Config_Reconfigured;
                Debug.WriteLine(this.GetType().Name + ": Unregistered a queue. Total queues count: " + this._allQueues.Count + ".");
            }
        }

        /// <summary>
        /// Gets the queues count.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns>System.Int32.</returns>
        internal int GetQueuesCount(string accessToken = null)
        {
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                if (this._queuesByAccessToken.TryGetValue(accessToken, out AccessTokenQueuesMetadata metadata))
                {
                    return metadata.Queues.Count;
                }
                return 0;
            }

            int result = 0;
            foreach(var md in this._queuesByAccessToken.Values)
            {
                result += md.Queues.Count;
            }
            return result;
        }

        /// <summary>
        /// The synchronize lock
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// The rollbar comm thread
        /// </summary>
        private Thread _rollbarCommThread;

        /// <summary>
        /// All queues
        /// </summary>
        private readonly HashSet<PayloadQueue> _allQueues =
            new HashSet<PayloadQueue>();

        /// <summary>
        /// The queues by access token
        /// </summary>
        private readonly Dictionary<string, AccessTokenQueuesMetadata> _queuesByAccessToken = 
            new Dictionary<string, AccessTokenQueuesMetadata>();

        /// <summary>
        /// Keeps the processing all queues.
        /// </summary>
        /// <param name="data">The data.</param>
        private void KeepProcessingAllQueues(object data)
        {
            CancellationToken cancellationToken = (CancellationToken) data;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    lock(this._syncLock)
                    {
                        ProcessAllQueuesOnce();
                    }
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (System.Threading.ThreadAbortException tae)
                {
                    return;
                }
                catch (System.Exception ex)
                {
                    //TODO: do we want to direct the exception 
                    //      to some kind of Rollbar notifier maintenance "access token"?
                }
#pragma warning restore CS0168 // Variable is declared but never used

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                Thread.Sleep(this._sleepInterval);
            }

            CompleteProcessing();
        }

        /// <summary>
        /// Processes all queues once.
        /// </summary>
        private void ProcessAllQueuesOnce()
        {
            foreach(var token in this._queuesByAccessToken.Keys)
            {
                ProcessQueues(this._queuesByAccessToken[token]);
            }
        }

        /// <summary>
        /// Processes the queues.
        /// </summary>
        /// <param name="tokenMetadata">The token metadata.</param>
        private void ProcessQueues(AccessTokenQueuesMetadata tokenMetadata)
        {
            // let's see if we can unregister any recently released queues:
            var releasedQueuesToRemove = tokenMetadata.Queues.Where(q => q.IsReleased && (q.GetPayloadCount() == 0)).ToArray();
            if (releasedQueuesToRemove != null && releasedQueuesToRemove.LongLength > 0)
            {
                foreach (var queue in releasedQueuesToRemove)
                {
                    this.Unregister(queue);
                }
            }

            // process the access token's queues:
            foreach (var queue in tokenMetadata.Queues)
            {
                if (DateTimeOffset.Now < queue.NextDequeueTime)
                {
                    // this means the queue overrides its reporting rate limit via its configuration settings
                    // let's observe its settings and skip processing:
                    continue;
                }

                if (tokenMetadata.IsTransmissionSuspended && DateTimeOffset.Now < tokenMetadata.NextTimeTokenUsage)
                {
                    // the token is suspended and the next usage time is not reached,
                    // let's flush the token queues (we are not allowed to transmit anyway)
                    // and quit processing this token's queues this time (until next processing iteration):
                    foreach (var tokenQueue in tokenMetadata.Queues)
                    {
                        foreach (var flushedBundle in tokenQueue.Flush())
                        {
                            this.OnRollbarEvent(
                                new PayloadDropEventArgs(
                                    queue.Logger,
                                    flushedBundle.GetPayload(),
                                    PayloadDropEventArgs.DropReason.TokenSuspension
                                    )
                                );
                        }
                    }
                    return;
                }

                RollbarResponse response = null;
                PayloadBundle payloadBundle = Process(queue, out response);

                if (payloadBundle != null && response == null)
                {
                    var bundle = queue.Dequeue();
                    this.OnRollbarEvent(
                        new PayloadDropEventArgs(queue.Logger, bundle.GetPayload(), PayloadDropEventArgs.DropReason.AllTransmissionRetriesFailed)
                        );
                }

                if (payloadBundle == null || response == null)
                {
                    continue;
                }

                tokenMetadata.UpdateNextTimeTokenUsage(response.RollbarRateLimit);

                switch (response.Error)
                {
                    case (int) RollbarApiErrorEventArgs.RollbarError.None:
                        payloadBundle.Signal?.Release();
                        queue.Dequeue();
                        break;
                    case (int) RollbarApiErrorEventArgs.RollbarError.TooManyRequests:
                        ObeyPayloadTimeout(payloadBundle, queue);
                        this.OnRollbarEvent(
                            new RollbarApiErrorEventArgs(queue.Logger, payloadBundle.GetPayload(), response)
                            );
                        return;
                    default:
                        ObeyPayloadTimeout(payloadBundle, queue);
                        this.OnRollbarEvent(
                            new RollbarApiErrorEventArgs(queue.Logger, payloadBundle.GetPayload(), response)
                            );
                        break;
                }
            }
        }

        /// <summary>
        /// Obeys the payload timeout.
        /// </summary>
        /// <param name="payloadBundle">The payload bundle.</param>
        /// <param name="queue">The queue.</param>
        private void ObeyPayloadTimeout(PayloadBundle payloadBundle, PayloadQueue queue)
        {
            if (payloadBundle.TimeoutAt.HasValue && (DateTime.Now.Add(this._sleepInterval) >= payloadBundle.TimeoutAt.Value))
            {
                queue.Dequeue();
            }
        }

        /// <summary>
        /// Processes the specified queue.
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <param name="response">The response.</param>
        /// <returns>PayloadBundle.</returns>
        private PayloadBundle Process(PayloadQueue queue, out RollbarResponse response)
        {
            response = null;

            PayloadBundle payloadBundle;
            Payload payload = null;

            bool ignorableBundle = false;
            do
            {
                payloadBundle = queue.Peek();
                if (payloadBundle == null)
                {
                    return null; // the queue is already empty, nothing to process...
                }

                try
                {
                    ignorableBundle = (payloadBundle.Ignorable || payloadBundle.GetPayload() == null);
                }
                catch (System.Exception ex)
                {
                    RollbarErrorUtility.Report(
                        null,
                        payloadBundle,
                        InternalRollbarError.DequeuingError,
                        "While attempting to dequeue a payload bundle...",
                        ex,
                        payloadBundle
                        );
                    ignorableBundle = true; // since something is not kosher about this bundle/payload, it is wise to ignore one...
                }

                if (ignorableBundle)
                {
                    queue.Dequeue(); //throw away the ignorable...
                    this.OnRollbarEvent(
                        new PayloadDropEventArgs(queue.Logger, null, PayloadDropEventArgs.DropReason.IgnorablePayload)
                        );
                }
                else
                {
                    payload = payloadBundle.GetPayload();
                }
            }
            while (ignorableBundle);

            if (payloadBundle == null || payload == null) // one more sanity check before proceeding further...
            {
                return null;
            }

            if (queue.Logger?.Config != null && !queue.Logger.Config.Transmit)
            {
                response = new RollbarResponse();
                this.OnRollbarEvent(
                    new TransmissionOmittedEventArgs(queue.Logger, payload)
                );
                return payloadBundle;
            }

            int retries = this._totalRetries;
            while (retries > 0)
            {
                try
                {
                    response = queue.Client.PostAsJson(payloadBundle);
                }
                catch (WebException ex)
                {
                    retries--;
                    this.OnRollbarEvent(
                        new CommunicationErrorEventArgs(queue.Logger, payload, ex, retries)
                        );
                    payloadBundle.Register(ex);
                    Thread.Sleep(this._sleepInterval); // wait a bit before we retry it...
                    continue;
                }
                catch (ArgumentNullException ex)
                {
                    retries = 0;
                    this.OnRollbarEvent(
                        new CommunicationErrorEventArgs(queue.Logger, payload, ex, retries)
                        );
                    payloadBundle.Register(ex);
                    continue;
                }
                catch (System.Exception ex)
                {
                    retries = 0;
                    this.OnRollbarEvent(
                        new CommunicationErrorEventArgs(queue.Logger, payload, ex, retries)
                        );
                    payloadBundle.Register(ex);
                    continue;
                }
                retries = 0;
            }

            if (response != null)
            {
                this.OnRollbarEvent(
                    new CommunicationEventArgs(queue.Logger, payload, response)
                    );
            }
            else
            {
                queue.Dequeue(); //we tried our best...
                payloadBundle.Register(new RollbarException(InternalRollbarError.DequeuingError, "Payload dropped!"));
            }

            return payloadBundle;
        }

        /// <summary>
        /// Handles the Reconfigured event of the Config control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Config_Reconfigured(object sender, EventArgs e)
        {
            lock (this._syncLock)
            {
                RollbarConfig config = (RollbarConfig)sender;
                Assumption.AssertNotNull(config, nameof(config));

                PayloadQueue queue = config.Logger.Queue;
                Assumption.AssertNotNull(queue, nameof(queue));

                //refresh indexing:
                this.DropIndexByToken(queue);
                this.IndexByToken(queue);
                Debug.WriteLine(this.GetType().Name + ": Re-indexed a reconfigured queue. Total queues count: " + this._allQueues.Count + ".");
            }
        }

        /// <summary>
        /// Indexes the by token.
        /// </summary>
        /// <param name="queue">The queue.</param>
        private void IndexByToken(PayloadQueue queue)
        {
            string queueToken = queue.Logger.Config.AccessToken;
            if (queueToken == null)
            {
                //this is a valid case for the RollbarLogger singleton instance,
                //when the instance is created but not configured yet...
                return;
            }

            if (!this._queuesByAccessToken.TryGetValue(queueToken, out AccessTokenQueuesMetadata tokenMetadata))
            {
                tokenMetadata = new AccessTokenQueuesMetadata(queueToken);
                this._queuesByAccessToken.Add(queueToken, tokenMetadata);
            }
            tokenMetadata.Queues.Add(queue);
            queue.AccessTokenQueuesMetadata = tokenMetadata;
        }

        /// <summary>
        /// Drops the index by token.
        /// </summary>
        /// <param name="queue">The queue.</param>
        private void DropIndexByToken(PayloadQueue queue)
        {
            foreach (var tokenMetadata in this._queuesByAccessToken.Values)
            {
                if (tokenMetadata.Queues.Contains(queue))
                {
                    tokenMetadata.Queues.Remove(queue);
                    queue.AccessTokenQueuesMetadata = null;
                    break;
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="E:RollbarEvent" /> event.
        /// </summary>
        /// <param name="e">The <see cref="RollbarEventArgs"/> instance containing the event data.</param>
        internal void OnRollbarEvent(RollbarEventArgs e)
        {
            Assumption.AssertNotNull(e, nameof(e));

            EventHandler<RollbarEventArgs> handler = InternalEvent;

            if (handler != null)
            {
                handler(this, e);
            }

            e.Logger?.OnRollbarEvent(e);

            const string category = nameof(this.OnRollbarEvent);// "OnRollbarEvent(...)";
            const int id = 0;
            switch (e)
            {
                case InternalErrorEventArgs internalErrorEvent:
                    traceSource.TraceData(TraceEventType.Critical, id, category, e.TraceAsString());
                    break;
                case CommunicationErrorEventArgs commErrorEvent:
                case RollbarApiErrorEventArgs apiErrorEvent:
                    traceSource.TraceData(TraceEventType.Error, id, category, e.TraceAsString());
                    break;
                case CommunicationEventArgs commEvent:
                    transmittedPayloadsTraceSource.TraceData(TraceEventType.Information, id, e.Payload);
                    traceSource.TraceData(TraceEventType.Information, id, category, e.TraceAsString());
                    break;
                case TransmissionOmittedEventArgs transmissionOmittedEvent:
                    omittedPayloadsTraceSource.TraceData(TraceEventType.Information, id, e.Payload);
                    traceSource.TraceData(TraceEventType.Warning, id, category, e.TraceAsString());
                    break;
                case PayloadDropEventArgs payloadDropEvent:
                default:
                    traceSource.TraceData(TraceEventType.Warning, id, category, e.TraceAsString());
                    break;
            }
            traceSource.Flush();
        }

        /// <summary>
        /// Gets the total payload count across all the queues.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int GetTotalPayloadCount()
        {
            lock (this._syncLock)
            {
                int count = 0;
                foreach (var queue in this._allQueues)
                {
                    count += queue.GetPayloadCount();
                }
                return count;
            }
        }

        /// <summary>
        /// Gets the payload count.
        /// </summary>
        /// <param name="accessToken">Converts to ken.</param>
        /// <returns>System.Int32.</returns>
        public int GetPayloadCount(string accessToken)
        {
            int counter = 0;
            AccessTokenQueuesMetadata tokenMetadata = null;
            lock (this._syncLock)
            {
                if (this._queuesByAccessToken.TryGetValue(accessToken, out tokenMetadata))
                {
                    foreach(var queue in tokenMetadata.Queues)
                    {
                        counter += queue.GetPayloadCount();
                    }
                }
            }
            return counter;
        }

        /// <summary>
        /// Gets the payload count.
        /// </summary>
        /// <param name="rollbar">The rollbar.</param>
        /// <returns>System.Int32.</returns>
        public int GetPayloadCount(IRollbar rollbar)
        {
            return this.GetPayloadCount(rollbar.Config.AccessToken);
        }

        /// <summary>
        /// Gets the recommended timeout.
        /// </summary>
        /// <param name="accessToken">The Rollbar access token.</param>
        /// <returns>TimeSpan.</returns>
        public TimeSpan GetRecommendedTimeout(string accessToken)
        {
            TimeSpan payloadTimeout = TimeSpan.Zero;
            int totalPayloads = 0;
            AccessTokenQueuesMetadata tokenMetadata = null;
            lock (this._syncLock)
            {
                if (this._queuesByAccessToken.TryGetValue(accessToken, out tokenMetadata))
                {
                    foreach (var queue in tokenMetadata.Queues)
                    {
                        totalPayloads += queue.GetPayloadCount();
                        TimeSpan queueTimeout = 
                            queue.Logger.Config.MaxReportsPerMinute.HasValue ?
                            TimeSpan.FromTicks(TimeSpan.FromMinutes(1).Ticks / queue.Logger.Config.MaxReportsPerMinute.Value)
                            : TimeSpan.Zero;
                        if (payloadTimeout < queueTimeout)
                        {
                            payloadTimeout = queueTimeout;
                        }
                    }
                }
            }
            return TimeSpan.FromTicks((totalPayloads + 1) * payloadTimeout.Ticks);
        }

        /// <summary>
        /// Gets the recommended timeout.
        /// </summary>
        /// <param name="rollbar">The rollbar.</param>
        /// <returns>TimeSpan.</returns>
        public TimeSpan GetRecommendedTimeout(IRollbar rollbar)
        {
            return this.GetRecommendedTimeout(rollbar.Config.AccessToken);
        }

        /// <summary>
        /// Gets the recommended timeout.
        /// </summary>
        /// <returns>TimeSpan.</returns>
        public TimeSpan GetRecommendedTimeout()
        {
            TimeSpan timeout = TimeSpan.Zero;
            string[] accessTokens;
            lock (this._syncLock)
            {
                accessTokens = this._queuesByAccessToken.Keys.ToArray();
            }

            if (accessTokens == null)
            {
                return timeout;
            }

            foreach(var token in accessTokens)
            {
                TimeSpan tokenTimeout = this.GetRecommendedTimeout(token);
                if (timeout < tokenTimeout)
                {
                    timeout = tokenTimeout;
                }
            }
            return timeout;
        }

        /// <summary>
        /// Flushes the queues.
        /// All current payloads in every queue get removed (without transmitting them to the Rollbar API).
        /// </summary>
        public void FlushQueues()
        {
            lock (this._syncLock)
            {
                foreach(var queue in this._allQueues)
                {
                    foreach (var flushedBundle in queue.Flush())
                    {
                        this.OnRollbarEvent(
                            new PayloadDropEventArgs(
                                queue.Logger, 
                                flushedBundle.GetPayload(), 
                                PayloadDropEventArgs.DropReason.RollbarQueueControllerFlushedQueues
                                )
                            );
                    }
                }
            }
        }

        /// <summary>
        /// The cancellation token source
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Starts this instance.
        /// </summary>
        private void Start()
        {
            if (this._rollbarCommThread == null)
            {
#if NETFX
                HostingEnvironment.RegisterObject(this);
#endif
                this._rollbarCommThread = new Thread(new ParameterizedThreadStart(this.KeepProcessingAllQueues))
                {
                    IsBackground = true,
                    Name = "RollbarProcessor",
                    //Priority = ThreadPriority.AboveNormal,
                };

                this._cancellationTokenSource = new CancellationTokenSource();
                this._rollbarCommThread.Start(_cancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// Completes the processing.
        /// </summary>
        private void CompleteProcessing()
        {
            Debug.WriteLine("Entering " +this.GetType().FullName + "." + nameof(this.CompleteProcessing) + "() method...");
#if NETFX
            HostingEnvironment.UnregisterObject(this);
#endif
            this._cancellationTokenSource.Dispose();
            this._cancellationTokenSource = null;
            this._rollbarCommThread = null;
        }

#if NETFX


        /// <summary>
        /// Stops the queues processing.
        /// </summary>
        public void Stop(bool immediate)
        {
            if (!immediate && this._cancellationTokenSource != null)
            {
                this._cancellationTokenSource.Cancel();
                return;
            }

            this._cancellationTokenSource.Cancel();
            if (this._rollbarCommThread != null)
            {
                this._rollbarCommThread.Join(TimeSpan.FromSeconds(60));
                CompleteProcessing();
            }
        }

#endif


        #region IDisposable Support

        /// <summary>
        /// The disposed value
        /// </summary>
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    CompleteProcessing();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RollbarQueueController() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>This code added to correctly implement the disposable pattern.</remarks>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
