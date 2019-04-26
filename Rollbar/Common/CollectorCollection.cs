namespace Rollbar.Common
{
    using System.Collections;
    using System.Collections.Generic;

    public class CollectorCollection<T>
        : ICollector<T>
        , IReadOnlyCollection<T>
    {
        private readonly ICollection<T> _collection;

        public static int DefaultCollectionCapacity { get; set; } = 10;

        public CollectorCollection()
            : this(CollectorCollection<T>.DefaultCollectionCapacity)
        {
        }

        public CollectorCollection(int capacity)
            : this(new List<T>(capacity))
        {
        }

        public CollectorCollection(IEnumerable<T> items)
            : this(new List<T>(items))
        {
        }

        public CollectorCollection(ICollection<T> collection)
        {
            this._collection = collection;

            if (this._collection == null)
            {
                this._collection = new List<T>(CollectorCollection<T>.DefaultCollectionCapacity);
            }
        }

        public int Count
        {
            get { return this._collection.Count; }
        }

        public void Add(T collectionItem)
        {
            this._collection.Add(collectionItem);
        }

        public void Add(IEnumerable<T> collectionItems)
        {
            foreach(var item in collectionItems)
            {
                this.Add(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this._collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
