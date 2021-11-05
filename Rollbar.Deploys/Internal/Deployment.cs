namespace Rollbar.Deploys
{
    using System;

    /// <summary>
    /// Models data used for registering a deployment instance.
    /// </summary>
   internal class Deployment 
        : IDeployment
    { 
        /// <summary>
        /// Prevents a default instance of the <see cref="Deployment"/> class from being created.
        /// </summary>
        private Deployment()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Deployment"/> class.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <param name="revision">The revision.</param>
        public Deployment(string environment, string revision)
            : this(null, environment, revision)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Deployment"/> class.
        /// </summary>
        /// <param name="writeAccessToken">The write access token.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="revision">The revision.</param>
        public Deployment(string? writeAccessToken, string? environment, string? revision)
        {
            this.WriteAccessToken = writeAccessToken;
            this.Environment = environment;
            this.Revision = revision;
        }

        /// <summary>
        /// Gets the write access token.
        /// </summary>
        /// <value>
        /// The write access token.
        /// </value>
        public string? WriteAccessToken { get; private set; }

        /// <summary>
        /// Gets the environment.
        /// </summary>
        /// <value>
        /// The environment.
        /// </value>
        public string? Environment { get; private set; }

        /// <summary>
        /// Gets the revision.
        /// </summary>
        /// <value>
        /// The revision.
        /// </value>
        public string? Revision { get; private set; }

        /// <summary>
        /// Gets the rollbar username.
        /// </summary>
        /// <value>
        /// The rollbar username.
        /// </value>
        public string? RollbarUsername { get; set; }

        /// <summary>
        /// Gets the local username.
        /// </summary>
        /// <value>
        /// The local username.
        /// </value>
        public string? LocalUsername { get; set; }

        /// <summary>
        /// Gets the comment.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        public string? Comment { get; set; }

    }
}
