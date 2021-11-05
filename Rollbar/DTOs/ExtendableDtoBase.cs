namespace Rollbar.DTOs
{
    using Rollbar.Diagnostics;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Common;

    /// <summary>
    /// Implements an abstract base for defining expendable DTO types.
    /// These types of DTOs can be extended with arbitrary extra 
    /// key-value pairs as needed.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.DtoBase" />
    /// <seealso cref="System.Collections.Generic.IDictionary{TKey,TValue}" />
    public abstract class ExtendableDtoBase
        : DtoBase,
        IDictionary<string, object?>
    {
        internal const string reservedPropertiesNestedTypeName = "ReservedProperties";

        private static readonly IReadOnlyDictionary<Type, ExtendableDtoMetadata> metadataByDerivedType = ExtendableDtoMetadata.BuildAll();

        private readonly ExtendableDtoMetadata? _metadata;

        /// <summary>
        /// The keyed values
        /// </summary>
        private readonly IDictionary<string, object?> _keyedValues = 
            new Dictionary<string, object?>();

        private ExtendableDtoBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendableDtoBase"/> class.
        /// </summary>
        /// <param name="arbitraryKeyValuePairs">The arbitrary key value pairs.</param>
        protected ExtendableDtoBase(IDictionary<string, object?>? arbitraryKeyValuePairs)
        {
            this._metadata = ExtendableDtoBase.metadataByDerivedType[this.GetType()];
            Assumption.AssertNotNull(this._metadata, nameof(this._metadata));

            if (arbitraryKeyValuePairs != null)
            {
                foreach (var key in arbitraryKeyValuePairs.Keys)
                {
                    this[key] = arbitraryKeyValuePairs[key];
                }
            }
        }


        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2589:Boolean expressions should not be gratuitous", Justification = "It is better to explicitly list all the relevant assumptions.")]
        public object? this[string key]
        {
            get
            {
                if (this._keyedValues.TryGetValue(key, out var result))
                {
                    return result;
                }

                var concreteDtoMetadata = metadataByDerivedType[this.GetType()];
                if(concreteDtoMetadata.ReservedPropertyInfoByReservedKey == null)
                {
                    return null;
                }
                if (concreteDtoMetadata
                    .ReservedPropertyInfoByReservedKey
                    .TryGetValue(key, out var reservedPropertyInfo) 
                    && reservedPropertyInfo.PropertyType.IsValueType
                    )
                {
                    return Activator.CreateInstance(reservedPropertyInfo.PropertyType);
                }

                return null;
            }
            set
            {
                bool isReservedProperty = 
                    this._metadata?.ReservedPropertyInfoByReservedKey?.Keys.Contains(key) 
                    ?? false;

                Assumption.AssertTrue(
                    !this._keyedValues.ContainsKey(key)                                         // no such key preset yet
                    || this._keyedValues[key] == null                                           // OR its not initialized yet
                    || value != null                                                            // OR no-null value
                    || !isReservedProperty,                                                     // OR not about reserved property/key
                    "conditional " + nameof(value) + " assessment"
                    );

                var lowCaseKey = key.ToLower();
                var concreteDtoMetadata = metadataByDerivedType[this.GetType()];
                if (concreteDtoMetadata.ReservedPropertyInfoByReservedKey != null 
                    && concreteDtoMetadata
                    .ReservedPropertyInfoByReservedKey
                    .TryGetValue(key, out PropertyInfo? reservedPropertyInfo))
                {
                    var reservedPropertyType = reservedPropertyInfo.PropertyType;
                    var valueType = value?.GetType();
                    if(valueType != null)
                    {
                        Assumption.AssertTrue(
                            //we are not dealing with a reserved property, hence, anything works:
                            !(concreteDtoMetadata.ReservedPropertyInfoByReservedKey.ContainsKey(key) || concreteDtoMetadata.ReservedPropertyInfoByReservedKey.ContainsKey(lowCaseKey))
                            //OR we are dealing with a reserved property and the value and its type should make sense:  
                            || value == null
                            || reservedPropertyType == valueType
                            || (reservedPropertyType.IsInterface
                                && ReflectionUtility.DoesTypeImplementInterface(valueType, reservedPropertyType))
                            || (reservedPropertyType.IsGenericType                                // dealing with nullable type
                                && reservedPropertyType.GenericTypeArguments.Length == 1
                                && reservedPropertyType.GenericTypeArguments[0] == valueType)
                            || valueType.IsSubclassOf(reservedPropertyType),
                            nameof(value)
                        );
                    }
                }

                if(concreteDtoMetadata.ReservedPropertyInfoByReservedKey != null
                    && concreteDtoMetadata.ReservedPropertyInfoByReservedKey.ContainsKey(lowCaseKey))
                {
                    // we are setting a reserved key when calling Bind(...) on an IConfigurationSection 
                    // that treats this instance of ExtendableDtoBase as a dictionary while binding to the targeted deserialization object:
                    this._keyedValues[lowCaseKey] = value;
                }
                else
                {
                    this._keyedValues[key] = value;
                }
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> 
        /// containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        public ICollection<string> Keys => this._keyedValues.Keys;

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> 
        /// containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        public ICollection<object?> Values => this._keyedValues.Values;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        public int Count => this._keyedValues.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> 
        /// is read-only.
        /// </summary>
        public bool IsReadOnly => this._keyedValues.IsReadOnly;

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        public void Add(string key, object? value)
        {
            this._keyedValues[key] = value;
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">
        /// The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </param>
        public void Add(KeyValuePair<string, object?> item)
        {
            this._keyedValues[item.Key] = item.Value;
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        public void Clear()
        {
            this._keyedValues.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> 
        /// contains a specific value.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </param>
        /// <returns>
        /// true if <paramref name="item">item</paramref> is found in 
        /// the <see cref="T:System.Collections.Generic.ICollection`1"></see>; 
        /// otherwise, false.
        /// </returns>
        public bool Contains(KeyValuePair<string, object?> item)
        {
            return this._keyedValues.Contains(item);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"></see> 
        /// contains an element with the specified key.
        /// </summary>
        /// <param name="key">
        /// The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2"></see> 
        /// contains an element with the key; otherwise, false.
        /// </returns>
        public bool ContainsKey(string key)
        {
            return this._keyedValues.ContainsKey(key);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> 
        /// to an <see cref="T:System.Array"></see>, starting at a particular 
        /// <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="T:System.Array"></see> that is the destination of 
        /// the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>. 
        /// The <see cref="T:System.Array"></see> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
        {
            this._keyedValues.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            return this._keyedValues.GetEnumerator();
        }

        /// <summary>
        /// Removes the element with the specified key from 
        /// the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; 
        /// otherwise, false.  
        /// This method also returns false if <paramref name="key">key</paramref> was not found 
        /// in the original <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </returns>
        public bool Remove(string key)
        {
            return this._keyedValues.Remove(key);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from 
        /// the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">
        /// The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </param>
        /// <returns>
        /// true if <paramref name="item">item</paramref> was successfully removed from 
        /// the <see cref="T:System.Collections.Generic.ICollection`1"></see>; 
        /// otherwise, false. 
        /// This method also returns false if <paramref name="item">item</paramref> is not found 
        /// in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </returns>
        public bool Remove(KeyValuePair<string, object?> item)
        {
            return this._keyedValues.Remove(item);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified key, 
        /// if the key is found; otherwise, the default value 
        /// for the type of the value parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// true if the object that implements 
        /// <see cref="T:System.Collections.Generic.IDictionary`2"></see> contains 
        /// an element with the specified key; otherwise, false.
        /// </returns>
        public bool TryGetValue(string key, out object? value)
        {
            return this._keyedValues.TryGetValue(key, out value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object 
        /// that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Extends this instance with the specified extra key-value pairs.
        /// </summary>
        /// <param name="extraProperties">The extra key-value pairs.</param>
        public void Extend(Dictionary<string, object> extraProperties)
        {
            foreach (var kvp in extraProperties)
            {
                this._keyedValues[kvp.Key] = kvp.Value;
            }
        }

        #region strings truncation support

        /// <summary>
        /// Truncates the strings.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <param name="stringBytesLimit">The string bytes limit.</param>
        /// <returns></returns>
        public override DtoBase TruncateStrings(Encoding encoding, int stringBytesLimit)
        {
            base.TruncateStrings(encoding, stringBytesLimit);

            this.TruncateStringValues(this, encoding, stringBytesLimit);

            return this;
        }

        #endregion strings truncation support
    }
}
