namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Rollbar.Diagnostics;

    /// <summary>
    /// Models Rollbar Payload DTO.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.DtoBase" />
    public class Payload
        : DtoBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Payload"/> class.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="data">The data.</param>
        public Payload(string accessToken, Data data)
        {
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
