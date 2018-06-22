[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar
{
    using Rollbar.Common;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using Rollbar.Serialization.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
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
#if NETFX
        : IRegisteredObject
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
            static NestedSingleInstance()
            {
            }

            internal static readonly RollbarQueueController Instance = 
                new RollbarQueueController();
        }

        #endregion singleton implementation


        private readonly TimeSpan _sleepInterval = TimeSpan.FromMilliseconds(250);

        private HttpClient _httpClient = null;
        private string _proxySettings = null;

        public HttpClient ProvideHttpClient(string proxySettings)
        {
            if (this._httpClient != null)
            {
                Assumption.AssertTrue(
                    (string.IsNullOrWhiteSpace(proxySettings) && string.IsNullOrWhiteSpace(this._proxySettings))
                    || (string.Compare(proxySettings, this._proxySettings, true) == 0)
                    , nameof(proxySettings)
                    );
                // reuse what is already there: 
                return this._httpClient;
            }

            // create new instance:
            this._proxySettings = proxySettings;
            this._httpClient = HttpClientUtil.CreateHttpClient(proxySettings);
            return this._httpClient;
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
                Debug.WriteLine(this.GetType().Name + ": Registered a queue. Total queues count: " + this._allQueues.Count + ".");
            }
        }

        /// <summary>
        /// Unregisters the specified queue.
        /// </summary>
        /// <param name="queue">The queue.</param>
        internal void Unregister(PayloadQueue queue)
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

        private Thread _rollbarCommThread = null;

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
                    break;

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
                    Payload payload = Process(queue, out response);
                    if (payload == null || response == null)
                    {
                        continue;
                    }

                    switch (response.Error)
                    {
                        case (int)RollbarApiErrorEventArgs.RollbarError.None:
                            payload.Signal?.Release();
                            queue.Dequeue();
                            tokenMetadata.ResetTokenUsageDelay();
                            break;
                        case (int)RollbarApiErrorEventArgs.RollbarError.TooManyRequests:
                            ObeyPayloadTimeout(payload, queue);
                            tokenMetadata.IncrementTokenUsageDelay();
                            this.OnRollbarEvent(
                                new RollbarApiErrorEventArgs(queue.Logger, payload, response)
                                );
                            return;
                        default:
                            ObeyPayloadTimeout(payload, queue);
                            this.OnRollbarEvent(
                                new RollbarApiErrorEventArgs(queue.Logger, payload, response)
                                );
                            break;
                    }

                }
            }
        }

        private void ObeyPayloadTimeout(Payload payload, PayloadQueue queue)
        {
            if (payload.TimeoutAt.HasValue && (DateTime.Now.Add(this._sleepInterval) >= payload.TimeoutAt.Value))
            {
                queue.Dequeue();
            }
        }

        private Payload Process(PayloadQueue queue, out RollbarResponse response)
        {
            response = null;

            Payload payload = queue.Peek();
            if (payload == null)
            {
                return null;
            }

            int retries = 3;
            while (retries > 0)
            {
                try
                {
                    response = queue.Client.PostAsJson(payload);
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

            return payload;
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
        /// Flushes the queues. All current payloads in every queue get removed.
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

        private CancellationTokenSource _cancellationTokenSource = null;

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
    }
}
