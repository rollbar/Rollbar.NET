namespace Rollbar.Classification
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Interface Identifiable
    /// </summary>
    public interface Identifiable
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        object ID { get; }
        /// <summary>
        /// Gets all known IDs.
        /// </summary>
        /// <value>All known IDs.</value>
        IEnumerable<object> GetAllKnownIDs();
    }

    /// <summary>
    /// Interface Identifiable
    /// Implements the <see cref="Rollbar.Classification.Identifiable" />
    /// </summary>
    /// <typeparam name="TID">The type of the identifier.</typeparam>
    /// <seealso cref="Rollbar.Classification.Identifiable" />
    public interface Identifiable<out TID>
        : Identifiable
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        new TID ID { get; }

        /// <summary>
        /// Gets all known IDs.
        /// </summary>
        /// <value>All known IDs.</value>
        new IEnumerable<TID> GetAllKnownIDs();
    }
}
