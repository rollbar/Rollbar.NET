namespace Rollbar.Common
{
    using System.Collections;
    using System.Collections.Generic;

    using Rollbar.Common;

    /// <summary>
    /// Class CollectorCollectionBase.
    /// </summary>
    public abstract class CollectorCollectionBase
    {
        /// <summary>
        /// Gets or sets the default collection capacity.
        /// </summary>
        /// <value>The default collection capacity.</value>
        public static int DefaultCollectionCapacity { get; set; } = 10;

    }

    /// <summary>
    /// Class CollectorCollection.
    /// Implements the <see cref="Rollbar.Common.ICollector{T}" />
    /// Implements the <see cref="System.Collections.Generic.IReadOnlyCollection{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Rollbar.Common.ICollector{T}" />
    /// <seealso cref="System.Collections.Generic.IReadOnlyCollection{T}" />
    public class CollectorCollection<T>
        : CollectorCollectionBase
        , ICollector<T>
        , IReadOnlyCollection<T>
    {
        /// <summary>
        /// The collection
        /// </summary>
        private readonly ICollection<T> _collection;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectorCollection{T}"/> class.
        /// </summary>
        public CollectorCollection()
            : this(CollectorCollection<T>.DefaultCollectionCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectorCollection{T}"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public CollectorCollection(int capacity)
            : this(new List<T>(capacity))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectorCollection{T}"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        public CollectorCollection(IEnumerable<T> items)
            : this(new List<T>(items))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectorCollection{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public CollectorCollection(ICollection<T> collection)
        {
            this._collection = collection;

            if (this._collection == null)
            {
                this._collection = new List<T>(CollectorCollection<T>.DefaultCollectionCapacity);
            }
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get { return this._collection.Count; }
        }

        /// <summary>
        /// Adds the specified collection item.
        /// </summary>
        /// <param name="collectionItem">The collection item.</param>
        public void Add(T collectionItem)
        {
            this._collection.Add(collectionItem);
        }

        /// <summary>
        /// Adds the specified collection items.
        /// </summary>
        /// <param name="collectionItems">The collection items.</param>
        public void Add(IEnumerable<T> collectionItems)
        {
            foreach(var item in collectionItems)
            {
                this.Add(item);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this._collection.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }


}
