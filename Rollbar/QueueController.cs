namespace Rollbar
{
    using Rollbar.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;

    internal class QueueController
    {
        private readonly Thread _rollbarCommThread = null;

        #region singleton implementation

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static QueueController Instance
        {
            get
            {
                return NestedSingleInstance.Instance;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="QueueController"/> class from being created.
        /// </summary>
        private QueueController()
        {
            this._rollbarCommThread = new Thread(this.KeepProcessingAllQueues);
            this._rollbarCommThread.IsBackground = true;
            this._rollbarCommThread.Name = "Rollbar Communication Thread";

            this._rollbarCommThread.Start();
        }

        private sealed class NestedSingleInstance
        {
            static NestedSingleInstance()
            {
            }

            internal static readonly QueueController Instance = new QueueController();
        }

        #endregion singleton implementation

        private readonly object _syncLock = new object();

        private readonly HashSet<PayloadQueue> _allQueues =
            new HashSet<PayloadQueue>();

        private readonly Dictionary<string, AccessTokenQueuesMetadata> _queuesByAccessToken = 
            new Dictionary<string, AccessTokenQueuesMetadata>();

        private void KeepProcessingAllQueues()
        {
            TimeSpan sleepInterval = TimeSpan.FromSeconds(1);

            while(true)
            {
                try
                {
                    //TODO:

                    Thread.Sleep(sleepInterval);
                }
                catch(Exception ex)
                {
                    //TODO: do we want to direct the exception 
                    //      to some kind of Rollbar notifier maintenance "access token"?
                }
            }
        }

        /// <summary>
        /// Registers the specified queue.
        /// </summary>
        /// <param name="queue">The queue.</param>
        public void Register(PayloadQueue queue)
        {
            lock(this._syncLock)
            {
                Assumption.AssertTrue(!this._allQueues.Contains(queue), nameof(queue));

                this._allQueues.Add(queue);
                this.IndexByToken(queue);
                queue.Logger.Config.Reconfigured += Config_Reconfigured;
                Debug.WriteLine(this.GetType().Name + ": Registered a queue. Total queues count: " + this._allQueues.Count + ".");
            }
        }

        /// <summary>
        /// Unregisters the specified queue.
        /// </summary>
        /// <param name="queue">The queue.</param>
        public void Unregister(PayloadQueue queue)
        {
            lock (this._syncLock)
            {
                Assumption.AssertTrue(!queue.Logger.IsSingleton, nameof(queue.Logger.IsSingleton));
                Assumption.AssertTrue(this._allQueues.Contains(queue), nameof(queue));

                this.DropIndexByToken(queue);
                this._allQueues.Remove(queue);
                queue.Logger.Config.Reconfigured -= Config_Reconfigured;
                Debug.WriteLine(this.GetType().Name + ": Unregistered a queue. Total queues count: " + this._allQueues.Count + ".");
            }
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

            AccessTokenQueuesMetadata tokenMetadata = null;
            if (!this._queuesByAccessToken.TryGetValue(queueToken, out tokenMetadata))
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

    }
}
