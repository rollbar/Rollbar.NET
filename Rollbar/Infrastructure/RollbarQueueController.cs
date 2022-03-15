#define TRACE

namespace Rollbar.Infrastructure
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
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using PayloadStore;
    using Serialization.Json;
    using System.IO;

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
    internal sealed class RollbarQueueController
        : IRollbarQueueController
#if NETFX
        , IRegisteredObject
#endif
        , IDisposable
    {
        #region singleton implementation

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static RollbarQueueController? Instance
        {
            get
            {
                return RollbarInfrastructure.Instance.QueueController as RollbarQueueController;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="RollbarQueueController" /> class from being created.
        /// </summary>
        internal RollbarQueueController()
        {
            traceSource.TraceInformation($"Creating the {typeof(RollbarQueueController).Name}...");
        }

        #endregion singleton implementation

        /// <summary>
        /// The trace source
        /// </summary>
        private static readonly TraceSource traceSource = 
            new TraceSource(typeof(RollbarQueueController).FullName ?? "RollbarQueueController");

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

        /// <summary>
        /// The transmitted payloads trace source
        /// </summary>
        private static readonly TraceSource transmittedPayloadsTraceSource =
            new TraceSource(PayloadTraceSources.RollbarTransmittedPayloads.ToString());

        /// <summary>
        /// The omitted payloads trace source
        /// </summary>
        private static readonly TraceSource omittedPayloadsTraceSource =
            new TraceSource(PayloadTraceSources.RollbarOmittedPayloads.ToString());

        /// <summary>
        /// The sleep interval
        /// </summary>
        internal readonly TimeSpan _sleepInterval = TimeSpan.FromMilliseconds(25);

        /// <summary>
        /// The store context (the payload persistence infrastructure)
        /// </summary>
        private IPayloadStoreRepository? _storeRepository = null;

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
        internal HttpClient? ProvideHttpClient(string? proxyAddress = null, string? proxyUsername = null, string? proxyPassword = null)
        {
            string proxySettings = string.Empty;
            if(!string.IsNullOrWhiteSpace(proxyAddress)
                || !string.IsNullOrWhiteSpace(proxyUsername)
                || !string.IsNullOrWhiteSpace(proxyPassword)
                )
            {
                if (RollbarConnectivityMonitor.Instance != null 
                    && !RollbarConnectivityMonitor.Instance.IsDisabled
                    )
                {
                    RollbarConnectivityMonitor.Instance.Disable();
                }
                proxySettings = $"{proxyAddress?.ToLower()}+{proxyUsername?.ToLower()}+{proxyPassword?.ToLower()}";
            }

            HttpClient? httpClient = null;
            if(this._httpClientsByProxySettings.TryGetValue(proxySettings, out httpClient))
            {
                return httpClient;
            }

            httpClient = HttpClientUtility.CreateHttpClient(proxyAddress, proxyUsername, proxyPassword);
            if(this._httpClientsByProxySettings.TryAdd(proxySettings, httpClient))
            {
                return httpClient;
            }
            else if(this._httpClientsByProxySettings.TryGetValue(proxySettings, out httpClient))
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
        public event EventHandler<RollbarEventArgs>? InternalEvent;

        private IRollbarInfrastructureConfig? _config;

        internal void Init(IRollbarInfrastructureConfig config)
        {
            Assumption.AssertNotNull(config, nameof(config));

            this._config = config;
            this._config.Reconfigured += _config_Reconfigured;
            this.EvaluateUseOfLocalPayloadStoreOptions(this._config.RollbarOfflineStoreOptions);

            this.Start();
        }

        private void _config_Reconfigured(object? sender, EventArgs e)
        {
            //NOTE: RollbarConfig - implement
        }

        private void EvaluateUseOfLocalPayloadStoreOptions(IRollbarOfflineStoreOptions options)
        {
            if(!options.EnableLocalPayloadStore)
            {
                if(this._storeRepository != null)
                {
                    this._storeRepository.Dispose();
                    this._storeRepository = null;
                }
                return;
            }

            if(this._storeRepository == null)
            {
                this._storeRepository = PayloadStoreRepositoryHelper.CreatePayloadStoreRepository();
            }

            string? storePath = this.GetLocalPayloadStoreFullPathName(options);
            if (this._storeRepository != null 
                && !string.IsNullOrWhiteSpace(storePath) 
                && string.Compare(storePath, this._storeRepository.GetRollbarStoreDbFullName(), false) != 0
                )
            {
                this._storeRepository.SetRollbarStoreDbFullName(storePath!);
            }

            this._storeRepository?.MakeSureDatabaseExistsAndReady();
        }

        internal string? GetLocalPayloadStoreFullPathName(IRollbarOfflineStoreOptions options)
        {
            string? dbLocation = string.IsNullOrWhiteSpace(options.LocalPayloadStoreLocationPath)
                ? PayloadStoreConstants.DefaultRollbarStoreDbFileLocation
                : options.LocalPayloadStoreLocationPath;

            string? dbFile = string.IsNullOrWhiteSpace(options.LocalPayloadStoreFileName)
                ? PayloadStoreConstants.DefaultRollbarStoreDbFile
                : options.LocalPayloadStoreFileName;

            string? result = string.IsNullOrWhiteSpace(dbLocation)
                ? dbFile
                : Path.Combine(dbLocation, dbFile);

            return result;
        }

        /// <summary>
        /// Registers the specified queue.
        /// </summary>
        /// <param name="queue">The queue.</param>
        internal void Register(PayloadQueue queue)
        {
            lock(this._syncLock)
            {
                Assumption.AssertNotNull(this._config, nameof(this._config));
                Assumption.AssertTrue(!this._allQueues.Contains(queue), nameof(queue));

                this._allQueues.Add(queue);
                this.IndexByToken(queue);

                ((RollbarLoggerConfig) queue.Logger.Config).Reconfigured += LoggerConfig_Reconfigured;

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
            if(queue.Logger == RollbarLocator.RollbarInstance.Logger)
            {
                return; // we do not want to unregister the singleton's queue.
            }

            lock(this._syncLock)
            {
                Assumption.AssertNotNull(this._config, nameof(this._config));
                Assumption.AssertTrue(this._allQueues.Contains(queue), nameof(queue));

                this.DropIndexByToken(queue);
                this._allQueues.Remove(queue);
                ((RollbarLoggerConfig) queue.Logger.Config).Reconfigured -= LoggerConfig_Reconfigured;
                Debug.WriteLine(this.GetType().Name + ": Unregistered a queue. Total queues count: " + this._allQueues.Count + ".");
            }
        }

        /// <summary>
        /// Gets the queues count.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns>System.Int32.</returns>
        internal int GetQueuesCount(string? accessToken = null)
        {
            Assumption.AssertNotNull(this._config, nameof(this._config));

            if(!string.IsNullOrWhiteSpace(accessToken))
            {
                if(this._queuesByAccessToken.TryGetValue(accessToken!, out AccessTokenQueuesMetadata? metadata))
                {
                    return metadata.PayloadQueuesCount;
                }
                return 0;
            }

            int result = 0;
            foreach(var md in this._queuesByAccessToken.Values)
            {
                result += md.PayloadQueuesCount;
            }
            return result;
        }

        internal int GetUnReleasedQueuesCount()
        {
            int result = 0;
            foreach(var md in this._queuesByAccessToken.Values)
            {
                foreach(var queue in md.GetPayloadQueues())
                {
                    if(!queue.IsReleased)
                    {
                        result++;
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Gets the queues.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns>IEnumerable&lt;PayloadQueue&gt;.</returns>
        internal IEnumerable<PayloadQueue> GetQueues(string? accessToken = null)
        {
            Assumption.AssertNotNull(this._config, nameof(this._config));

            if (!string.IsNullOrWhiteSpace(accessToken) 
                && this._queuesByAccessToken.TryGetValue(accessToken!, out AccessTokenQueuesMetadata? metadata)
                )
            {
                return metadata.GetPayloadQueues();
            }
            return new PayloadQueue[0];
        }

        /// <summary>
        /// The synchronize lock
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// The rollbar comm thread
        /// </summary>
        private Thread? _rollbarCommThread;

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
        private void KeepProcessingAllQueues(object? data)
        {
            if(data == null)
            {
                RollbarErrorUtility.Report(
                    null,
                    null,
                    InternalRollbarError.QueueControllerError,
                    "While KeepProcessingAllQueues()...",
                    null,
                    null
                );
                return;
            }

            CancellationToken cancellationToken = (CancellationToken) data;

            while(!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    lock(this._syncLock)
                    {
                        ProcessAllQueuesOnce();
                    }

                    ProcessPersistentStoreOnce();
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch(System.Threading.ThreadAbortException tae)
                {
                    return;
                }
                catch(System.Exception ex)
                {
                    RollbarErrorUtility.Report(
                        null,
                        null,
                        InternalRollbarError.QueueControllerError,
                        "While KeepProcessingAllQueues()...",
                        ex,
                        null
                    );

                    //NOTE: do we want to direct the exception 
                    //      to some kind of Rollbar notifier maintenance "access token"?
                }
#pragma warning restore CS0168 // Variable is declared but never used

                if(cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                Thread.Sleep(this._sleepInterval);
            }

            CompleteProcessing();
        }

        /// <summary>
        /// The stale record age
        /// </summary>
        private static TimeSpan staleRecordAge = TimeSpan.FromDays(7);

        /// <summary>
        /// Processes the persistent store once.
        /// </summary>
        private void ProcessPersistentStoreOnce()
        {
            if(this._storeRepository == null)
            {
                return;
            }

            var destinations = _storeRepository.GetDestinations();
            foreach(var destination in destinations)
            {
                ProcessPersistentStoreOnce(destination);
            }
        }

        /// <summary>
        /// Processes the persistent store once.
        /// </summary>
        /// <param name="destination">The destination.</param>
        private void ProcessPersistentStoreOnce(IDestination destination)
        {
            AccessTokenQueuesMetadata? accessTokenMetadata = null;

            lock(this._syncLock)
            {
                if(!this._queuesByAccessToken.TryGetValue(destination.AccessToken!, out  accessTokenMetadata))
                {
                    accessTokenMetadata = new AccessTokenQueuesMetadata(destination.AccessToken!);
                    this._queuesByAccessToken.Add(destination.AccessToken!, accessTokenMetadata);
                }
            }

            if(accessTokenMetadata.IsTransmissionSuspended && DateTimeOffset.Now < accessTokenMetadata.NextTimeTokenUsage)
            {
                // the token is suspended and the next usage time is not reached,
                // there is no point in continuing persistent store processing for this access token:
                return;
            }

            // 1. delete all the stale records of this destination and save the store context
            //    (if any records were deleted):
            DateTime staleRecordsLimit = DateTime.UtcNow.Subtract(staleRecordAge);
            var staleRecords = this._storeRepository?.GetStaleRecords(staleRecordsLimit);
            if(staleRecords != null && staleRecords.Length > 0)
            {
                this._storeRepository?.DeleteRecords(staleRecords);
            }

            // 2. get the oldest record of this destination and try transmitting it:
            if(RollbarConnectivityMonitor.Instance != null && !RollbarConnectivityMonitor.Instance.IsConnectivityOn)
            {
                return; // there is no point trying to transmit the oldest record (if any)...
            }
            var oldestRecord = this._storeRepository?.GetOldestRecords(destination.ID);
            if(oldestRecord != null)
            {
                var rollbarResponse = TryPosting(oldestRecord);
                if(rollbarResponse == null)
                {
                    return; //could not reach Rollbar API...
                }

                this.OnRollbarEvent(
                    new CommunicationEventArgs(null, oldestRecord.PayloadJson, rollbarResponse)
                );

                // This processor did its best communicating with Rollbar API.
                // Regardless of actual result, update next token usage and consider
                // this payload record processed so it can be deleted:
                accessTokenMetadata.UpdateNextTimeTokenUsage(rollbarResponse.RollbarRateLimit);
                this._storeRepository?.DeleteRecords(oldestRecord);
            }
        }

        /// <summary>
        /// Tries the posting.
        /// </summary>
        /// <param name="payloadRecord">The payload record.</param>
        /// <returns>RollbarResponse.</returns>
        private RollbarResponse? TryPosting(IPayloadRecord payloadRecord)
        {
            if (payloadRecord.ConfigJson == null)
            {
                return null;
            }
            IRollbarLoggerConfig? config = JsonConvert.DeserializeObject<RollbarLoggerConfig>(payloadRecord.ConfigJson);
            if (config == null)
            {
                return null;
            }
            RollbarClient rollbarClient = new RollbarClient(config);

            try
            {
                RollbarResponse? response =
                    rollbarClient.PostAsJson(config.RollbarDestinationOptions.AccessToken, payloadRecord.PayloadJson);
                return response;
            }
            catch(System.Exception ex)
            {
                this.OnRollbarEvent(
                    new CommunicationErrorEventArgs(null, payloadRecord.PayloadJson, ex, 0)
                );

                RollbarErrorUtility.Report(
                    null,
                    payloadRecord,
                    InternalRollbarError.PersistentPayloadRecordRepostError,
                    "While trying to report a stored payload...",
                    ex,
                    null
                );

                return null;
            }
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
            var releasedQueuesToRemove = tokenMetadata.GetPayloadQueues().Where(q => q.IsReleased && (q.GetPayloadCount() == 0)).ToArray();
            if(releasedQueuesToRemove != null && releasedQueuesToRemove.LongLength > 0)
            {
                foreach(var queue in releasedQueuesToRemove)
                {
                    this.Unregister(queue);
                }
            }

            // process the access token's queues:
            foreach(var queue in tokenMetadata.GetPayloadQueues())
            {
                if(DateTimeOffset.Now < queue.NextDequeueTime)
                {
                    // this means the queue overrides its reporting rate limit via its configuration settings
                    // let's observe its settings and skip processing:
                    continue;
                }

                if(tokenMetadata.IsTransmissionSuspended && DateTimeOffset.Now < tokenMetadata.NextTimeTokenUsage)
                {
                    // the token is suspended and the next usage time is not reached,
                    // let's flush the token queues (we are not allowed to transmit anyway)
                    // and quit processing this token's queues this time (until next processing iteration):
                    foreach(var tokenQueue in tokenMetadata.GetPayloadQueues())
                    {
                        foreach(var flushedBundle in tokenQueue.Flush())
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

                PayloadBundle? payloadBundle = null;
                RollbarResponse? response = null;
                try
                {
                    payloadBundle = Process(queue, out response);
                }
                catch(AggregateException aggregateException)
                {
                    if(aggregateException.InnerExceptions.Any(e => e is HttpRequestException))
                    {
                        this.Persist(queue);
                        continue;
                    }
                    else
                    {
                        var bundle = queue.Dequeue();
                        this.OnRollbarEvent(
                            new PayloadDropEventArgs(queue.Logger, bundle?.GetPayload(), PayloadDropEventArgs.DropReason.InvalidPayload)
                        );
                        queue.Dequeue();
                        throw;
                    }
                }
                catch(System.Exception ex)
                {
                    Debug.WriteLine($"EXCEPTION: {ex}");
                    this.Persist(queue);
                    continue;
                }

                if(payloadBundle != null && response == null)
                {
                    var bundle = queue.Dequeue();
                    this.OnRollbarEvent(
                        new PayloadDropEventArgs(queue.Logger, bundle?.GetPayload(), PayloadDropEventArgs.DropReason.AllTransmissionRetriesFailed)
                        );
                }

                if(payloadBundle == null || response == null)
                {
                    continue;
                }

                tokenMetadata.UpdateNextTimeTokenUsage(response.RollbarRateLimit);

                switch(response.Error)
                {
                    case (int) RollbarApiErrorEventArgs.RollbarError.None:
                        payloadBundle.Signal?.Release();
                        queue.Dequeue();
                        break;
                    case (int) RollbarApiErrorEventArgs.RollbarError.TooManyRequests:
                        ObeyPayloadTimeout(payloadBundle, queue);
                        this.OnRollbarEvent(
                            new RollbarApiErrorEventArgs(queue.Logger, payloadBundle?.GetPayload(), response)
                            );
                        return;
                    default:
                        ObeyPayloadTimeout(payloadBundle, queue);
                        this.OnRollbarEvent(
                            new RollbarApiErrorEventArgs(queue.Logger, payloadBundle?.GetPayload(), response)
                            );
                        break;
                }
            }
        }

        /// <summary>
        /// Persists the specified payload queue.
        /// </summary>
        /// <param name="payloadQueue">The payload queue.</param>
        private void Persist(PayloadQueue payloadQueue)
        {
            //if (!payloadQueue.Logger.Config.EnableLocalPayloadStore)
            if(this._storeRepository == null)
            {
                return;
            }

            var items = payloadQueue.GetItemsToPersist();
            if(items == null || items.Length == 0)
            {
                return;
            }

            string? endPoint = payloadQueue.Logger.Config.RollbarDestinationOptions.EndPoint;
            string? accessToken = payloadQueue.AccessTokenQueuesMetadata?.AccessToken;

            if( string.IsNullOrWhiteSpace(endPoint) || string.IsNullOrWhiteSpace(accessToken))
            {
                return;
            }

            var payloads = new List<IPayloadRecord>();
            foreach(var item in items)
            {
                var payloadRecord = this.BuildPayloadRecord(item, payloadQueue);
                if(payloadRecord != null)
                {
                    payloads.Add(payloadRecord);
                }
            }

            try
            {
                this._storeRepository.SavePayloads(endPoint!, accessToken!, payloads);
            }
            catch(System.Exception ex)
            {
                RollbarErrorUtility.Report(
                        payloadQueue.Logger,
                        items.Select(i => i.GetPayload()),
                        InternalRollbarError.PersistentStoreContextError,
                        "While attempting to save persistent store context...",
                        ex,
                        null
                    );
            }

            foreach(var item in items)
            {
                item.Signal?.Release();
            }
        }

        /// <summary>
        /// Builds the payload record.
        /// </summary>
        /// <param name="payloadBundle">The payload bundle.</param>
        /// <param name="payloadQueue">The payload queue.</param>
        /// <returns>PayloadRecord.</returns>
        private IPayloadRecord? BuildPayloadRecord(PayloadBundle payloadBundle, PayloadQueue payloadQueue)
        {
            try
            {
                if(payloadBundle.Ignorable
                    || payloadBundle.GetPayload() == null
                    || !payloadQueue.Client.EnsureHttpContentToSend(payloadBundle)
                )
                {
                    return null;
                }

                if(payloadBundle.AsHttpContentToSend == null)
                {
                    return null;
                }

                Task<string> task = payloadBundle.AsHttpContentToSend.ReadAsStringAsync();
                task.Wait();
                string payloadContent = task.Result;
                Payload? payload = payloadBundle.GetPayload();

                if(payload != null && !string.IsNullOrWhiteSpace(payloadContent))
                {
                    return _storeRepository?.CreatePayloadRecord(payload, payloadContent);
                }
                else
                {
                    return null;
                }
            }
            catch(System.Exception ex)
            {
                RollbarErrorUtility.Report(
                    payloadQueue.Logger,
                    payloadBundle.GetPayload(),
                    InternalRollbarError.PersistentPayloadRecordError,
                    "While attempting to build persistent payload record...",
                    ex,
                    null
                );
                return null;
            }

        }

        /// <summary>
        /// Obeys the payload timeout.
        /// </summary>
        /// <param name="payloadBundle">The payload bundle.</param>
        /// <param name="queue">The queue.</param>
        private void ObeyPayloadTimeout(PayloadBundle payloadBundle, PayloadQueue queue)
        {
            if(payloadBundle.TimeoutAt.HasValue && (DateTime.Now.Add(this._sleepInterval) >= payloadBundle.TimeoutAt.Value))
            {
                queue.Dequeue();
            }
        }

        /// <summary>
        /// Gets the first transmittabl bundle.
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <returns>PayloadBundle.</returns>
        private PayloadBundle? GetFirstTransmittableBundle(PayloadQueue queue)
        {
            PayloadBundle? payloadBundle = null;

            bool ignorableBundle = false;
            do
            {
                payloadBundle = queue.Peek();
                if(payloadBundle == null)
                {
                    return null; // the queue is already empty, nothing to process...
                }

                try
                {
                    ignorableBundle = (payloadBundle.Ignorable || payloadBundle.GetPayload() == null);
                }
                catch(System.Exception ex)
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

                if(ignorableBundle)
                {
                    queue.Dequeue(); //throw away the ignorable...
                    this.OnRollbarEvent(
                        new PayloadDropEventArgs(queue.Logger, null, PayloadDropEventArgs.DropReason.IgnorablePayload)
                    );
                }
            }
            while(ignorableBundle);

            return payloadBundle;
        }

        /// <summary>
        /// Processes the specified queue.
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <param name="response">The response.</param>
        /// <returns>PayloadBundle.</returns>
        private PayloadBundle? Process(PayloadQueue queue, out RollbarResponse? response)
        {
            response = null;

            PayloadBundle? payloadBundle = GetFirstTransmittableBundle(queue);
            if(payloadBundle == null)
            {
                return null;
            }

            Payload? payload = payloadBundle?.GetPayload();
            if(payload == null) // one more sanity check before proceeding further...
            {
                return null;
            }

            if(queue.Logger?.Config != null && !queue.Logger.Config.RollbarDeveloperOptions.Transmit)
            {
                response = new RollbarResponse();
                this.OnRollbarEvent(
                    new TransmissionOmittedEventArgs(queue.Logger, payload)
                );
                return payloadBundle;
            }

            try
            {
                response = queue.Client.PostAsJson(payloadBundle!);
            }
            catch(System.Exception ex)
            {
                this.OnRollbarEvent(
                    new CommunicationErrorEventArgs(queue.Logger, payload, ex, 0)
                );
                payloadBundle?.Register(ex);
                throw;
            }

            if(response != null)
            {
                this.OnRollbarEvent(
                    new CommunicationEventArgs(queue.Logger, payload, response)
                    );
            }
            else
            {
                queue.Dequeue(); //we tried our best...
                payloadBundle?.Register(new RollbarException(InternalRollbarError.DequeuingError, "Payload dropped!"));
            }

            return payloadBundle;
        }

        /// <summary>
        /// Handles the Reconfigured event of the Config control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void LoggerConfig_Reconfigured(object? sender, EventArgs e)
        {
            RollbarLoggerConfig? config = sender as RollbarLoggerConfig;
            Assumption.AssertNotNull(config, nameof(config));

            RollbarLogger? rollbarLogger = config?.Logger as RollbarLogger;
            _ = Assumption.AssertNotNull(rollbarLogger, nameof(rollbarLogger));

            lock(this._syncLock)
            {
                PayloadQueue? queue = rollbarLogger?.Queue;
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
        private void IndexByToken(PayloadQueue? queue)
        {
            string? queueToken = queue?.Logger.Config.RollbarDestinationOptions.AccessToken;
            if(queueToken == null)
            {
                //this is a valid case for the RollbarLogger singleton instance,
                //when the instance is created but not configured yet...
                return;
            }

            if(!this._queuesByAccessToken.TryGetValue(queueToken, out AccessTokenQueuesMetadata? tokenMetadata))
            {
                tokenMetadata = new AccessTokenQueuesMetadata(queueToken);
                this._queuesByAccessToken.Add(queueToken, tokenMetadata);
            }
            tokenMetadata.Register(queue);
        }


        /// <summary>
        /// Drops the index by token.
        /// </summary>
        /// <param name="queue">The queue.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3267:Loops should be simplified with \"LINQ\" expressions", Justification = "More clear code intent.")]
        private void DropIndexByToken(PayloadQueue? queue)
        {
            if(queue == null)
            {
                return;
            }

            foreach(var tokenMetadata in this._queuesByAccessToken.Values)
            {
                if(tokenMetadata.Unregister(queue))
                {
                    break;
                }
            }
        }


        /// <summary>
        /// Handles the <see cref="E:RollbarEvent" /> event.
        /// </summary>
        /// <param name="e">The <see cref="RollbarEventArgs"/> instance containing the event data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3458:Empty \"case\" clauses that fall through to the \"default\" should be omitted", Justification = "Prefer to listall cases explicitly.")]
        internal void OnRollbarEvent(RollbarEventArgs e)
        {
            Assumption.AssertNotNull(this._config, nameof(this._config));
            Assumption.AssertNotNull(e, nameof(e));

            EventHandler<RollbarEventArgs>? handler = InternalEvent;

            if(handler != null)
            {
                handler(this, e);
            }

            (e.Logger as RollbarLogger)?.OnRollbarEvent(e);

            const string category = nameof(this.OnRollbarEvent);
            const int id = 0;
            switch(e)
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
            lock(this._syncLock)
            {
                Assumption.AssertNotNull(this._config, nameof(this._config));

                int count = 0;
                foreach(var queue in this._allQueues)
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
        public int GetPayloadCount(string? accessToken)
        {
            if(string.IsNullOrWhiteSpace(accessToken))
            {
                return 0;
            }

            int counter = 0;
            lock(this._syncLock)
            {
                Assumption.AssertNotNull(this._config, nameof(this._config));

                if(this._queuesByAccessToken.TryGetValue(accessToken!, out AccessTokenQueuesMetadata? tokenMetadata))
                {
                    foreach(var queue in tokenMetadata.GetPayloadQueues())
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
            Assumption.AssertNotNull(this._config, nameof(this._config));

            return this.GetPayloadCount(rollbar.Config.RollbarDestinationOptions.AccessToken);
        }

        /// <summary>
        /// Gets the recommended timeout.
        /// </summary>
        /// <param name="accessToken">The Rollbar access token.</param>
        /// <returns>TimeSpan.</returns>
        public TimeSpan GetRecommendedTimeout(string? accessToken)
        {
            TimeSpan payloadTimeout = TimeSpan.Zero;
            if(string.IsNullOrWhiteSpace(accessToken))
            {
                return payloadTimeout;
            }

            int totalPayloads = 0;
            lock(this._syncLock)
            {
                Assumption.AssertNotNull(this._config, nameof(this._config));

                if(this._queuesByAccessToken.TryGetValue(accessToken!, out AccessTokenQueuesMetadata? tokenMetadata))
                {
                    foreach(var queue in tokenMetadata.GetPayloadQueues())
                    {
                        totalPayloads += queue.GetPayloadCount();
                        TimeSpan queueTimeout =
                            this._config!.RollbarInfrastructureOptions.MaxReportsPerMinute.HasValue ?
                            TimeSpan.FromTicks(TimeSpan.FromMinutes(1).Ticks / this._config.RollbarInfrastructureOptions.MaxReportsPerMinute.Value)
                            : TimeSpan.Zero;
                        if(payloadTimeout < queueTimeout)
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
        public TimeSpan GetRecommendedTimeout(IRollbar? rollbar)
        {
            Assumption.AssertNotNull(this._config, nameof(this._config));

            return this.GetRecommendedTimeout(rollbar?.Config.RollbarDestinationOptions.AccessToken);
        }

        /// <summary>
        /// Gets the recommended timeout.
        /// </summary>
        /// <returns>TimeSpan.</returns>
        public TimeSpan GetRecommendedTimeout()
        {
            TimeSpan timeout = TimeSpan.Zero;
            string[] accessTokens;
            lock(this._syncLock)
            {
                Assumption.AssertNotNull(this._config, nameof(this._config));

                accessTokens = this._queuesByAccessToken.Keys.ToArray();
            }

            if(accessTokens == null)
            {
                return timeout;
            }

            foreach(var token in accessTokens)
            {
                TimeSpan tokenTimeout = this.GetRecommendedTimeout(token);
                if(timeout < tokenTimeout)
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
            lock(this._syncLock)
            {
                Assumption.AssertNotNull(this._config, nameof(this._config));

                foreach(var queue in this._allQueues)
                {
                    foreach(var flushedBundle in queue.Flush())
                    {
                        Payload? payload = null;
                        try
                        {
                            payload = flushedBundle?.GetPayload();
                        }
                        catch
                        {
                            payload = null;
                        }
                        finally
                        {
                            this.OnRollbarEvent(
                                new PayloadDropEventArgs(
                                    queue.Logger,
                                    payload,
                                    PayloadDropEventArgs.DropReason.RollbarQueueControllerFlushedQueues
                                    )
                                );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The cancellation token source
        /// </summary>
        private CancellationTokenSource? _cancellationTokenSource;

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            Assumption.AssertNotNull(this._config, nameof(this._config));

            if(this._rollbarCommThread == null)
            {
#if NETFX
                HostingEnvironment.RegisterObject(this);
#endif
                this._rollbarCommThread = new Thread(new ParameterizedThreadStart(this.KeepProcessingAllQueues))
                {
                    IsBackground = true,
                    Name = "RollbarProcessor",
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
            Debug.WriteLine("Entering " + this.GetType().FullName + "." + nameof(this.CompleteProcessing) + "() method...");
#if NETFX
            HostingEnvironment.UnregisterObject(this);
#endif
            if(this._cancellationTokenSource != null)
            {
                this._cancellationTokenSource.Dispose();
                this._cancellationTokenSource = null;
                this._rollbarCommThread = null;
            }

            if(this._storeRepository != null)
            {
                this._storeRepository.Dispose();
                this._storeRepository = null;
            }
        }

        /// <summary>
        /// Stops the queues processing.
        /// </summary>
        /// <param name="immediately"></param>
        public void Stop(bool immediately)
        {
            Assumption.AssertNotNull(this._config, nameof(this._config));

            if(!immediately && this._cancellationTokenSource != null)
            {
                this._cancellationTokenSource.Cancel();
                return;
            }

            this._cancellationTokenSource?.Cancel();
            if(this._rollbarCommThread != null)
            {

                if(!this._rollbarCommThread.Join(TimeSpan.FromSeconds(60)))
                {
#pragma warning disable SYSLIB0006 // Type or member is obsolete
                    this._rollbarCommThread.Abort();
#pragma warning restore SYSLIB0006 // Type or member is obsolete
                }

                CompleteProcessing();
            }
        }


        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    // dispose managed state (managed objects).
                    CompleteProcessing();
                    if(this._config != null)
                    {
                        this._config.Reconfigured -= _config_Reconfigured;
                    }
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>This code added to correctly implement the disposable pattern.</remarks>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
