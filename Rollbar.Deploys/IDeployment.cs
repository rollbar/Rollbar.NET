namespace Rollbar.Deploys
{
    /// <summary>
    /// Models deployment related information/data interface. 
    /// </summary>
    public interface IDeployment
    {
        /// <summary>
        /// Gets the revision.
        /// </summary>
        /// <value>
        /// The revision.
        /// </value>
        string? Revision { get; }

        /// <summary>
        /// Gets the environment.
        /// </summary>
        /// <value>
        /// The environment.
        /// </value>
        string? Environment { get; }

        /// <summary>
        /// Gets the comment.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        string? Comment { get; }

        /// <summary>
        /// Gets the local username.
        /// </summary>
        /// <value>
        /// The local username.
        /// </value>
        string? LocalUsername { get; }

        /// <summary>
        /// Gets the rollbar username.
        /// </summary>
        /// <value>
        /// The rollbar username.
        /// </value>
        string? RollbarUsername { get; }
    }
}