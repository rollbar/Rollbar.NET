namespace Rollbar.DTOs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    public abstract class ExtendableDtoBase
        : DtoBase,
        IEnumerable<KeyValuePair<string, object>>,
        IDictionary<string, object>
    {
        protected readonly IDictionary<string, object> _keyedValues = new Dictionary<string, object>();

        public object this[string key] { get => this._keyedValues[key]; set => this._keyedValues[key] = value; }

        public ICollection<string> Keys => this._keyedValues.Keys;

        public ICollection<object> Values => this._keyedValues.Values;

        public int Count => this._keyedValues.Count;

        public bool IsReadOnly => this._keyedValues.IsReadOnly;

        public void Add(string key, object value)
        {
            this._keyedValues[key] = value;
        }

        public void Add(KeyValuePair<string, object> item)
        {
            this._keyedValues[item.Key] = item.Value;
        }

        public void Clear()
        {
            this._keyedValues.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return this._keyedValues.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return this._keyedValues.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            this._keyedValues.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this._keyedValues.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return this._keyedValues.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return this._keyedValues.Remove(item);
        }

        public bool TryGetValue(string key, out object value)
        {
            return this._keyedValues.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Extend(Dictionary<string, object> extraProperties)
        {
            foreach (var kvp in extraProperties)
            {
                this._keyedValues[kvp.Key] = kvp.Value;
            }
        }

    }
}
