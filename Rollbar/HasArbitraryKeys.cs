using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RollbarDotNet 
{
    public abstract class HasArbitraryKeys : IEnumerable<KeyValuePair<string, object>> 
    {
        protected HasArbitraryKeys() 
        {
            AdditionalKeys = new Dictionary<string, object>();
        }

        /// <summary>
        /// Note: this is ugly state altering madness :(.
        /// On the other hand it's pretty formal: this method should look for 
        /// 'Rollbar Knows about this' keys, and update the actual C# value with
        /// the value in the dict, then delete the key from the AdditionalKeys. 
        /// Any time you add a key this will be called.
        /// </summary>
        protected abstract void Normalize();

        /// <summary>
        /// Given a dictionary, add any keys that are stored on the C# object instead of the
        /// dictionary.
        /// </summary>
        /// <param name="dict">guaranteed to be a copy of the Additional Keys</param>
        /// <returns>the input `dict` with additional keys</returns>
        protected abstract Dictionary<string, object> Denormalize(Dictionary<string, object> dict);

        protected Dictionary<string, object> AdditionalKeys { get; private set; }

        public Dictionary<string, object> Denormalize() 
        {
            var dictionary = AdditionalKeys.ToDictionary(k => k.Key, k => k.Value);
            return Denormalize(dictionary);
        }

        public void Add(string key, object value) 
        {
            AdditionalKeys[key] = value;
            Normalize();
        }

        public void Extend(Dictionary<string, object> extraKeys) 
        {
            foreach (var kvp in extraKeys) 
            {
                AdditionalKeys[kvp.Key] = kvp.Value;
            }

            Normalize();
        }

        public object this[string key] 
        {
            get { return Denormalize()[key]; }
            set { Add(key, value); }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() 
        {
            return Denormalize().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() 
        {
            return GetEnumerator();
        }
    }
}