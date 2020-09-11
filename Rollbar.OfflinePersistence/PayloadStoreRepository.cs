namespace Rollbar.OfflinePersistence
{
    using Rollbar.DTOs;
    using Rollbar.PayloadStore;
    using Rollbar.Serialization.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class PayloadStoreRepository.
    /// Implements the <see cref="Rollbar.PayloadStore.IPayloadStoreRepository" />
    /// </summary>
    /// <seealso cref="Rollbar.PayloadStore.IPayloadStoreRepository" />
    public class PayloadStoreRepository 
        : IPayloadStoreRepository
    {
        /// <summary>
        /// The store context (the payload persistence infrastructure)
        /// </summary>
        private StoreContext _storeContext = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadStoreRepository"/> class.
        /// </summary>
        public PayloadStoreRepository()
        {
            this._storeContext = new StoreContext();
        }
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _storeContext.Dispose();
        }

        /// <summary>
        /// Makes the sure database exists and ready.
        /// </summary>
        public void MakeSureDatabaseExistsAndReady() => this._storeContext.MakeSureDatabaseExistsAndReady();

        /// <summary>
        /// Gets the destinations.
        /// </summary>
        /// <returns>IDestination[].</returns>
        public IDestination[] GetDestinations() => this._storeContext.Destinations.ToArray();

        /// <summary>
        /// Gets the destination.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <param name="accessToken">The access token.</param>
        /// <returns>Destination.</returns>
        Destination GetDestination(string endPoint, string accessToken)
        {
            var destination = this._storeContext.Destinations.SingleOrDefault(d =>
                d.Endpoint == endPoint &&
                d.AccessToken == accessToken
                );
            return destination;
        }

        /// <summary>
        /// Gets the stale records.
        /// </summary>
        /// <param name="staleRecordsLimit">The stale records limit.</param>
        /// <returns>IPayloadRecord[].</returns>
        public IPayloadRecord[] GetStaleRecords(DateTime staleRecordsLimit) => this._storeContext.PayloadRecords.Where(pr => pr.Timestamp < staleRecordsLimit).ToArray();

        /// <summary>
        /// Deletes the records.
        /// </summary>
        /// <param name="staleRecords">The stale records.</param>
        public void DeleteRecords(IPayloadRecord[] staleRecords)
        {
            this._storeContext.PayloadRecords.RemoveRange(staleRecords.Cast<PayloadRecord>());
            this._storeContext.SaveChanges();
        }

        /// <summary>
        /// Gets the oldest records.
        /// </summary>
        /// <param name="destinationId">The destination identifier.</param>
        /// <returns>IPayloadRecord.</returns>
        public IPayloadRecord GetOldestRecords(Guid destinationId)
        {
            return this._storeContext.PayloadRecords
                .Where(r => r.DestinationID == destinationId)
                .OrderBy(r => r.Timestamp)
                .FirstOrDefault();
        }

        /// <summary>
        /// Creates the payload record.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="payloadContent">Content of the payload.</param>
        /// <returns>IPayloadRecord.</returns>
        public IPayloadRecord CreatePayloadRecord(Payload payload, string payloadContent)
        {
            return new PayloadRecord()
            {
                Timestamp = payload.TimeStamp,
                ConfigJson = JsonUtil.SerializeAsJsonString(payload.Data.Notifier.Configuration),
                PayloadJson = payloadContent,
            };
        }

        /// <summary>
        /// Saves the payloads.
        /// </summary>
        /// <param name="destinationEndPoint">The destination end point.</param>
        /// <param name="destinationAccessToken">The destination access token.</param>
        /// <param name="payloads">The payloads.</param>
        public void SavePayloads(string destinationEndPoint, string destinationAccessToken, List<IPayloadRecord> payloads)
        {
            var destination = GetDestination(destinationEndPoint, destinationAccessToken);
            if (destination == null)
            {
                destination = new Destination() { Endpoint = destinationEndPoint, AccessToken = destinationAccessToken, };
                this._storeContext.Destinations.Add(destination);
            }

            foreach (var p in payloads.Cast<PayloadRecord>())
            {
                destination.PayloadRecords.Add(p);
            }

            this._storeContext.SaveChanges();
        }

        /// <summary>
        /// Gets the full name of the rollbar store database.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetRollbarStoreDbFullName() => StoreContext.RollbarStoreDbFullName;

        /// <summary>
        /// Sets the full name of the rollbar store database.
        /// </summary>
        /// <param name="newStorePath">The new store path.</param>
        public void SetRollbarStoreDbFullName(string newStorePath)
        {
            StoreContext.RollbarStoreDbFullName = newStorePath;
        }
    }
}
