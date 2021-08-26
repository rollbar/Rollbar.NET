namespace Rollbar.PayloadTruncation
{
    using Newtonsoft.Json;
    using Rollbar.DTOs;
    using System.Text;

    /// <summary>
    /// An abstract base for implementing Payload truncation strategies.
    /// </summary>
    /// <seealso cref="Rollbar.PayloadTruncation.IPayloadTruncationStrategy" />
    internal abstract class PayloadTruncationStrategyBase
        : IPayloadTruncationStrategy
    {
        /// <summary>
        /// Truncates the specified payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        /// Payload size (in bytes) after the truncation.
        /// </returns>
        public abstract int Truncate(Payload? payload);

        /// <summary>
        /// Gets the size in bytes.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        protected static int GetSizeInBytes(Payload? payload)
        {
            int result = 0;

            if(payload == null)
            {
                return result;
            }

            var jsonData = JsonConvert.SerializeObject(payload);

            Encoding stringEncoding = Encoding.UTF8;

            result = stringEncoding.GetByteCount(jsonData);

            return result;
        }
    }
}
