namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Rollbar.Diagnostics;
    using System;
    using System.Net.Http;
    using System.Threading;

    /// <summary>
    /// Models Rollbar Payload DTO.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.DtoBase" />
    public class Payload
        : DtoBase
    {
        private readonly DateTime? _timeoutAt;
        private readonly SemaphoreSlim _signal;
        private StringContent _asHttpContentToSend;

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
        /// Gets or sets this payload rendered as HTTP content to send.
        /// We may need it as optimization cache for re-tries.
        /// </summary>
        /// <value>An HTTP content to send.</value>
        [JsonIgnore]
        internal StringContent AsHttpContentToSend
        {
            get
            {
                return this._asHttpContentToSend;
            }
            set
            {
                this._asHttpContentToSend = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Payload"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        //public Payload(
        //    Data data
        //    )
        //    : this(null, data)
        //{
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="Payload"/> class.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="data">The data.</param>
        public Payload(
            string accessToken,
            Data data
            )
            : this(accessToken, data, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Payload" /> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="timeoutAt">The timeout at.</param>
        /// <param name="signal">The signal.</param>
        //public Payload(
        //    Data data,
        //    DateTime? timeoutAt,
        //    SemaphoreSlim signal
        //    )
        //    : this(null, data, timeoutAt, signal)
        //{
        //}

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
            DateTime? timeoutAt,
            SemaphoreSlim signal
            )
        {
            Assumption.AssertNotNullOrWhiteSpace(accessToken, nameof(accessToken));
            Assumption.AssertNotNull(data, nameof(data));

            this._timeoutAt = timeoutAt;
            this._signal = signal;

            AccessToken = accessToken;
            Data = data;
        }

        /// <summary>
        /// Gets the access token (REQUIRED).
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        /// <remarks>
        /// Required: access_token
        /// An access token with scope "post_server_item" or "post_client_item".
        /// A post_client_item token must be used if the "platform" is "browser", "android", "ios", "flash", or "client"
        /// A post_server_item token should be used for other platforms.
        /// </remarks>
        [JsonProperty("access_token", Required = Required.Always)]
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets the data (REQUIRED).
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        /// <remarks>
        /// Required: data
        /// </remarks>
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

            base.Validate();
        }
    }
}
