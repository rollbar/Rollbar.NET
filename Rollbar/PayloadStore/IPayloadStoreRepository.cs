namespace Rollbar.PayloadStore
{
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface IPayloadStoreRepository
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IPayloadStoreRepository 
        : IDisposable
    {
        /// <summary>
        /// Makes the sure database exists and ready.
        /// </summary>
        void MakeSureDatabaseExistsAndReady();

        /// <summary>
        /// Gets the destinations.
        /// </summary>
        /// <returns>IDestination[].</returns>
        IDestination[] GetDestinations();

        /// <summary>
        /// Gets the stale records.
        /// </summary>
        /// <param name="staleRecordsLimit">The stale records limit.</param>
        /// <returns>IPayloadRecord[].</returns>
        IPayloadRecord[] GetStaleRecords(DateTime staleRecordsLimit);

        /// <summary>
        /// Gets the oldest records.
        /// </summary>
        /// <param name="destinationId">The destination identifier.</param>
        /// <returns>IPayloadRecord.</returns>
        IPayloadRecord? GetOldestRecords(Guid destinationId);

        /// <summary>
        /// Deletes the records.
        /// </summary>
        /// <param name="staleRecords">The stale records.</param>
        void DeleteRecords(params IPayloadRecord[] staleRecords);

        /// <summary>
        /// Creates the payload record.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="payloadContent">Content of the payload.</param>
        /// <returns>IPayloadRecord.</returns>
        IPayloadRecord CreatePayloadRecord(Payload payload, string payloadContent);

        /// <summary>
        /// Saves the payloads.
        /// </summary>
        /// <param name="destinationEndPoint">The destination end point.</param>
        /// <param name="destinationAccessToken">The destination access token.</param>
        /// <param name="payloads">The payloads.</param>
        void SavePayloads(string destinationEndPoint, string destinationAccessToken, List<IPayloadRecord> payloads);

        /// <summary>
        /// Gets the full name of the rollbar store database.
        /// </summary>
        /// <returns>System.String.</returns>
        string GetRollbarStoreDbFullName();

        /// <summary>
        /// Sets the full name of the rollbar store database.
        /// </summary>
        /// <param name="newStorePath">The new store path.</param>
        void SetRollbarStoreDbFullName(string newStorePath);
    }
}
