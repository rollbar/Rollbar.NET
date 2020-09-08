using Rollbar.DTOs;
using Rollbar.PayloadStore;
using Rollbar.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rollbar.OfflinePersistence
{
    public class PayloadStoreRepository : IPayloadStoreRepository
    {
        /// <summary>
        /// The store context (the payload persistence infrastructure)
        /// </summary>
        private StoreContext _storeContext = null;

        public PayloadStoreRepository()
        {
            this._storeContext = new StoreContext();
        }
        public void Dispose()
        {
            _storeContext.Dispose();
        }


        public void MakeSureDatabaseExistsAndReady() => this._storeContext.MakeSureDatabaseExistsAndReady();

        public IDestination[] GetDestinations() => this._storeContext.Destinations.ToArray();

        Destination GetDestination(string endPoint, string accessToken)
        {
            var destination = this._storeContext.Destinations.SingleOrDefault(d =>
                d.Endpoint == endPoint &&
                d.AccessToken == accessToken
                );
            return destination;
        }

        public IPayloadRecord[] GetStaleRecords(DateTime staleRecordsLimit) => this._storeContext.PayloadRecords.Where(pr => pr.Timestamp < staleRecordsLimit).ToArray();

        public void DeleteRecords(IPayloadRecord[] staleRecords)
        {
            this._storeContext.PayloadRecords.RemoveRange(staleRecords.Cast<PayloadRecord>());
            this._storeContext.SaveChanges();
        }

        public IPayloadRecord GetOldestRecords(Guid destinationId)
        {
            return this._storeContext.PayloadRecords
                .Where(r => r.DestinationID == destinationId)
                .OrderBy(r => r.Timestamp)
                .FirstOrDefault();
        }

        public IPayloadRecord CreatePayloadRecord(Payload payload, string payloadContent)
        {
            return new PayloadRecord()
            {
                Timestamp = payload.TimeStamp,
                ConfigJson = JsonUtil.SerializeAsJsonString(payload.Data.Notifier.Configuration),
                PayloadJson = payloadContent,
            };
        }

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

        public string GetRollbarStoreDbFullName() => StoreContext.RollbarStoreDbFullName;

        public void SetRollbarStoreDbFullName(string newStorePath)
        {
            StoreContext.RollbarStoreDbFullName = newStorePath;
        }
    }
}
