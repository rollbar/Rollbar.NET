using Rollbar.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rollbar
{
    internal class QueueController
    {
        #region singleton implementation

        public static QueueController Instance
        {
            get
            {
                return NestedSingleInstance.Instance;
            }
        }

        private QueueController()
        {

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

        private readonly Dictionary<string, HashSet<PayloadQueue>> _queuesByAccessToken = 
            new Dictionary<string, HashSet<PayloadQueue>>();

        public void Register(PayloadQueue queue)
        {
            lock(this._syncLock)
            {
                Assumption.AssertTrue(!this._allQueues.Contains(queue), nameof(queue));

                this._allQueues.Add(queue);
                this.IndexByToken(queue);
                queue.Logger.Config.Reconfigured += Config_Reconfigured;
            }
        }

        public void Unregister(PayloadQueue queue)
        {
            lock (this._syncLock)
            {
                Assumption.AssertTrue(!queue.Logger.IsSingleton, nameof(queue.Logger.IsSingleton));
                Assumption.AssertTrue(this._allQueues.Contains(queue), nameof(queue));

                this.DropIndexByToken(queue);
                this._allQueues.Remove(queue);
                queue.Logger.Config.Reconfigured -= Config_Reconfigured;
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
            }
        }

        private void IndexByToken(PayloadQueue queue)
        {
            HashSet<PayloadQueue> tokenQueues = null;
            string queueToken = queue.Logger.Config.AccessToken;
            if (!this._queuesByAccessToken.TryGetValue(queueToken, out tokenQueues))
            {
                tokenQueues = new HashSet<PayloadQueue>();
                this._queuesByAccessToken.Add(queueToken, tokenQueues);
            }
            tokenQueues.Add(queue);
        }

        private void DropIndexByToken(PayloadQueue queue)
        {
            foreach (var set in this._queuesByAccessToken.Values)
            {
                if (set.Contains(queue))
                {
                    set.Remove(queue);
                    break;
                }
            }
        }

    }
}
