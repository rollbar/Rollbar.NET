namespace Rollbar.Common
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface ICollector
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICollector<in T>
    {
        /// <summary>
        /// Adds the specified collection item.
        /// </summary>
        /// <param name="collectionItem">The collection item.</param>
        void Add(T collectionItem);

        /// <summary>
        /// Adds the specified collection items.
        /// </summary>
        /// <param name="collectionItems">The collection items.</param>
        void Add(IEnumerable<T> collectionItems);
    }
}
