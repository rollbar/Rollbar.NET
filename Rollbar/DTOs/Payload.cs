namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Rollbar.Diagnostics;
    using System;
    using System.Threading;

    /// <summary>
    /// Models Rollbar Payload DTO.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.DtoBase" />
    public class Payload
        : DtoBase
    {
        private readonly DateTime? _timeoutAt = null;
        private readonly SemaphoreSlim _signal = null;

        [JsonIgnore]
        internal DateTime? TimeoutAt
        {
            get { return this._timeoutAt; }
        }

        [JsonIgnore]
        internal SemaphoreSlim Signal
        {
            get { return this._signal; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Payload" /> class.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="data">The data.</param>
        /// <param name="timeoutAt">The timeout at.</param>
        /// <param name="signal">The signal.</param>
        public Payload(
            string accessToken, 
            Data data, 
            DateTime? timeoutAt = null,
            SemaphoreSlim signal = null
            )
        {
            this._timeoutAt = timeoutAt;
            this._signal = signal;

            AccessToken = accessToken;
            Data = data;
            Validate();
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        [JsonProperty("access_token", Required = Required.Always)]
        public string AccessToken { get; private set; }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [JsonProperty("data", Required = Required.Always)]
        public Data Data { get; private set; }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        public override void Validate()
        {
            Assumption.AssertNotNullOrWhiteSpace(this.AccessToken, nameof(this.AccessToken));
            Assumption.AssertNotNull(this.Data, nameof(this.Data));

            this.Data.Validate();
        }
    }
}
