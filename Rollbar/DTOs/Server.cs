namespace Rollbar.DTOs
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Rollbar.Serialization.Json;

    /// <summary>
    /// Models Rollbar Server DTO.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.ExtendableDtoBase" />
    /// <remarks>
    /// Optional: server
    /// Data about the server related to this event.
    /// Can contain any arbitrary keys.
    /// </remarks>
    public class Server 
        : HostBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        public Server()
            : this(null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        public Server(IDictionary<string, object?>? arbitraryKeyValuePairs) 
            : base(arbitraryKeyValuePairs)
        {
        }

        /// <summary>
        /// The DTO reserved properties.
        /// </summary>
        public new static class ReservedProperties
        {
            /// <summary>
            /// The host
            /// </summary>
            public static readonly string Host = "host";
            /// <summary>
            /// The root
            /// </summary>
            public static readonly string Root = "root";
            /// <summary>
            /// The branch
            /// </summary>
            public static readonly string Branch = "branch";
            /// <summary>
            /// The code version
            /// </summary>
            public static readonly string CodeVersion = "code_version";
        }

        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        /// <value>
        /// The host.
        /// </value>
        /// <remarks>
        /// host: The server hostname. Will be indexed.
        /// </remarks>
        public string? Host
        {
            get { return this[ReservedProperties.Host] as string; }
            set { this[ReservedProperties.Host] = value; }
        }

        /// <summary>
        /// Gets or sets the root.
        /// </summary>
        /// <value>
        /// The root.
        /// </value>
        /// <remarks>
        /// root: Path to the application code root, not including the final slash.
        /// Used to collapse non-project code when displaying tracebacks.
        /// </remarks>
        public string? Root
        {
            get { return this[ReservedProperties.Root] as string; }
            set { this[ReservedProperties.Root] = value; }
        }

        /// <summary>
        /// Gets or sets the branch.
        /// </summary>
        /// <value>
        /// The branch.
        /// </value>
        /// <remarks>
        /// branch: Name of the checked-out source control branch. Defaults to "master"
        /// </remarks>
        public string? Branch
        {
            get { return this[ReservedProperties.Branch] as string; }
            set { this[ReservedProperties.Branch] = value; }
        }

        /// <summary>
        /// Gets or sets the code version (OPTIONAL).
        /// </summary>
        /// <value>
        /// The code version.
        /// </value>
        /// <remarks>
        /// Optional: code_version
        /// String describing the running code version on the server
        /// </remarks>
        public string? CodeVersion
        {
            get { return this[ReservedProperties.CodeVersion] as string; }
            set { this[ReservedProperties.CodeVersion] = value; }
        }
    }
}
