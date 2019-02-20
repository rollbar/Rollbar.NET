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
    /// </summary>
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
        /// <value>
        /// The instance.
        /// </value>
        public static RollbarQueueController Instance
        {
            get
            {
                return NestedSingleInstance.Instance;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarQueueController"/> class from being created.
        /// </summary>
        private RollbarQueueController()
        {
            this.Start();
        }

        private sealed class NestedSingleInstance
        {
            private NestedSingleInstance()
            {
            }

            internal static readonly RollbarQueueController Instance = 
                new RollbarQueueController();
        }

        #endregion singleton implementation


        internal readonly TimeSpan _sleepInterval = TimeSpan.FromMilliseconds(50);
        internal readonly int _totalRetries = 3;

        private readonly ConcurrentDictionary<string, HttpClient> _httpClientsByProxySettings = 
            new ConcurrentDictionary<string, HttpClient>();

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
        /// <returns></returns>
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

        private readonly object _syncLock = new object();

        private Thread _rollbarCommThread;

        private readonly HashSet<PayloadQueue> _allQueues =
            new HashSet<PayloadQueue>();

        private readonly Dictionary<string, AccessTokenQueuesMetadata> _queuesByAccessToken = 
            new Dictionary<string, AccessTokenQueuesMetadata>();

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

        private void ProcessAllQueuesOnce()
        {
            foreach(var token in this._queuesByAccessToken.Keys)
            {
                if (this._queuesByAccessToken[token].NextTimeTokenUsage.HasValue
                    && this._queuesByAccessToken[token].NextTimeTokenUsage.Value > DateTimeOffset.Now
                    )
                {
                    //skip this token's queue for now, until past NextTimeTokenUsage:
                    continue;
                }
                ProcessQueues(this._queuesByAccessToken[token]);
            }
        }

        private void ProcessQueues(AccessTokenQueuesMetadata tokenMetadata)
        {
            foreach (var queue in tokenMetadata.Queues)
            {
                if (DateTimeOffset.Now >= queue.NextDequeueTime)
                {
                    RollbarResponse response = null;
                    PayloadQueuePackage payloadPackage = Process(queue, out response);
                    if (payloadPackage == null || response == null)
                    {
                        continue;
                    }

                    switch (response.Error)
                    {
                        case (int)RollbarApiErrorEventArgs.RollbarError.None:
                            payloadPackage.Signal?.Release();
                            queue.Dequeue();
                            tokenMetadata.ResetTokenUsageDelay();
                            break;
                        case (int)RollbarApiErrorEventArgs.RollbarError.TooManyRequests:
                            ObeyPayloadTimeout(payloadPackage, queue);
                            tokenMetadata.IncrementTokenUsageDelay();
                            this.OnRollbarEvent(
                                new RollbarApiErrorEventArgs(queue.Logger, payloadPackage.GetPayload(), response)
                                );
                            return;
                        default:
                            ObeyPayloadTimeout(payloadPackage, queue);
                            this.OnRollbarEvent(
                                new RollbarApiErrorEventArgs(queue.Logger, payloadPackage.GetPayload(), response)
                                );
                            break;
                    }

                }
            }

            // let's see if we can unregister any recently released queues:
            var releasedQueuesToRemove = tokenMetadata.Queues.Where(q => q.IsReleased && (q.GetPayloadCount() == 0)).ToArray();
            if (releasedQueuesToRemove != null && releasedQueuesToRemove.LongLength > 0)
            {
                foreach (var queue in releasedQueuesToRemove)
                {
                    this.Unregister(queue);
                }
            }

        }

        private void ObeyPayloadTimeout(PayloadQueuePackage payloadPackage, PayloadQueue queue)
        {
            if (payloadPackage.TimeoutAt.HasValue && (DateTime.Now.Add(this._sleepInterval) >= payloadPackage.TimeoutAt.Value))
            {
                queue.Dequeue();
            }
        }

        private PayloadQueuePackage Process(PayloadQueue queue, out RollbarResponse response)
        {
            response = null;

            PayloadQueuePackage payloadPackage = queue.Peek();

            Payload payload = payloadPackage.GetPayload();
            if (payload == null)
            {
                return null;
            }

            int retries = this._totalRetries;
            while (retries > 0)
            {
                try
                {
                    response = queue.Client.PostAsJson(payloadPackage);
                }
                catch (WebException ex)
                {
                    retries--;
                    this.OnRollbarEvent(
                        new CommunicationErrorEventArgs(queue.Logger, payload, ex, retries)
                        );
                    continue;
                }
                catch (ArgumentNullException ex)
                {
                    retries = 0;
                    this.OnRollbarEvent(
                        new CommunicationErrorEventArgs(queue.Logger, payload, ex, retries)
                        );
                    continue;
                }
                catch (System.Exception ex)
                {
                    retries = 0;
                    this.OnRollbarEvent(
                        new CommunicationErrorEventArgs(queue.Logger, payload, ex, retries)
                        );
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

            return payloadPackage;
        }

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
        }

        private void DropIndexByToken(PayloadQueue queue)
        {
            foreach (var tokenMetadata in this._queuesByAccessToken.Values)
            {
                if (tokenMetadata.Queues.Contains(queue))
                {
                    tokenMetadata.Queues.Remove(queue);
                    break;
                }
            }
        }

        private void OnRollbarEvent(RollbarEventArgs e)
        {
            Assumption.AssertNotNull(e, nameof(e));
            Assumption.AssertNotNull(e.Logger, nameof(e.Logger));

            EventHandler<RollbarEventArgs> handler = InternalEvent;

            if (handler != null)
            {
                handler(this, e);
            }

            e.Logger.OnRollbarEvent(e);
        }

        /// <summary>
        /// Gets the total payload count across all the queues.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>Gets the payload count.</summary>
        /// <param name="rollbar">The rollbar.</param>
        /// <returns>System.Int32.</returns>
        public int GetPayloadCount(IRollbar rollbar)
        {
            return this.GetPayloadCount(rollbar.Config.AccessToken);
        }

        /// <summary>Gets the recommended timeout.</summary>
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
                        TimeSpan queueTimeout = TimeSpan.FromTicks(TimeSpan.FromMinutes(1).Ticks / queue.Logger.Config.MaxReportsPerMinute);
                        if (payloadTimeout < queueTimeout)
                        {
                            payloadTimeout = queueTimeout;
                        }
                    }
                }
            }
            return TimeSpan.FromTicks((totalPayloads + 1) * payloadTimeout.Ticks);
        }

        /// <summary>Gets the recommended timeout.</summary>
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
                    queue.Flush();
                }
            }
        }

        private CancellationTokenSource _cancellationTokenSource;

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
                    Name = "Rollbar Communication Thread"
                };

                this._cancellationTokenSource = new CancellationTokenSource();
                this._rollbarCommThread.Start(_cancellationTokenSource.Token);
            }
        }

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

        private bool disposedValue = false; // To detect redundant calls

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
        /// <remarks>
        /// This code added to correctly implement the disposable pattern.
        /// </remarks>
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
