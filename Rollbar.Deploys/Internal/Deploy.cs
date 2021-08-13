namespace Rollbar.Deploys
{
    using Newtonsoft.Json;
    using Rollbar.Common;
    using System;

    /// <summary>
    /// Models a deploy data record.
    /// </summary>
    /// <seealso cref="Rollbar.Deploys.IDeployment" />
    internal class Deploy
        : IDeploymentDetails
    {
        /// <summary>
        /// Gets or sets the deploy identifier.
        /// </summary>
        /// <value>
        /// The deploy identifier.
        /// </value>
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? DeployID { get; set; }

        /// <summary>
        /// Gets or sets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        [JsonProperty("project_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? ProjectID { get; set; }

        /// <summary>
        /// Gets the revision.
        /// </summary>
        /// <value>
        /// The revision.
        /// </value>
        [JsonProperty("revision", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Revision { get; set; }

        /// <summary>
        /// Gets the environment.
        /// </summary>
        /// <value>
        /// The environment.
        /// </value>
        [JsonProperty("environment", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Environment { get; set; }

        /// <summary>
        /// Gets the comment.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        [JsonProperty("comment", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Comment { get; set; }

        /// <summary>
        /// Gets the local username.
        /// </summary>
        /// <value>
        /// The local username.
        /// </value>
        [JsonProperty("local_username", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? LocalUsername { get; set; }

        /// <summary>
        /// Gets the rollbar username.
        /// </summary>
        /// <value>
        /// The rollbar username.
        /// </value>
        [JsonProperty("user_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? RollbarUsername { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        [JsonProperty("start_time", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        /// <value>
        /// The end time.
        /// </value>
        [JsonProperty("finish_time", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long EndTime { get; set; }

        /// <summary>
        /// Gets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        [JsonIgnore]
        DateTimeOffset IDeploymentDetails.StartTime
        {
            get
            {
                return DateTimeUtil.ConvertFromUnixTimestampInSeconds(this.StartTime);
            }
        }

        /// <summary>
        /// Gets the end time.
        /// </summary>
        /// <value>
        /// The end time.
        /// </value>
        [JsonIgnore]
        DateTimeOffset IDeploymentDetails.EndTime
        {
            get
            {
                return DateTimeUtil.ConvertFromUnixTimestampInSeconds(this.EndTime);
            }
        }
    }
}
