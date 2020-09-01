using Rollbar.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rollbar.PayloadStore
{
    public interface IPayloadStoreRepository : IDisposable
    {
        void MakeSureDatabaseExistsAndReady();
        IDestination[] GetDestinations();
        //IDestination GetOrCreateDestination(string endPoint, string accessToken);
        IPayloadRecord[] GetStaleRecords(DateTime staleRecordsLimit);
        IPayloadRecord GetOldestRecords(Guid destinationId);
        void DeleteRecords(params IPayloadRecord[] staleRecords);
        IPayloadRecord CreatePayloadRecord(Payload payload, string payloadContent);
        void SavePayloads(string destinationEndPoint, string destinationAccessToken, List<IPayloadRecord> payloads);
        string GetRollbarStoreDbFullName();
        void SetRollbarStoreDbFullName(string newStorePath);
    }
}
