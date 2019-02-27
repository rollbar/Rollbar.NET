namespace Rollbar.DTOs
{
    using Newtonsoft.Json;
    using Rollbar.Diagnostics;
    using System;

    /// <summary>
    /// Models Rollbar Payload DTO.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.DtoBase" />
    public class Payload
        : DtoBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Payload" /> class.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="data">The data.</param>
        public Payload(
            string accessToken, 
            Data data
            )
        {
            Assumption.AssertNotNullOrWhiteSpace(accessToken, nameof(accessToken));
            Assumption.AssertNotNull(data, nameof(data));

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
