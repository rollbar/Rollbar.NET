namespace Rollbar.Deploys
{
    using System;

    /// <summary>
    /// Defines a deployment details/data interface
    /// </summary>
    /// <seealso cref="Rollbar.Deploys.IDeployment" />
    public interface IDeploymentDetails
        : IDeployment
    {
        /// <summary>
        /// Gets the deploy identifier.
        /// </summary>
        /// <value>
        /// The deploy identifier.
        /// </value>
        string DeployID { get; }

        /// <summary>
        /// Gets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        string ProjectID { get; }

        /// <summary>
        /// Gets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        DateTimeOffset StartTime { get; }

        /// <summary>
        /// Gets the end time.
        /// </summary>
        /// <value>
        /// The end time.
        /// </value>
        DateTimeOffset EndTime { get; }
    }
}
