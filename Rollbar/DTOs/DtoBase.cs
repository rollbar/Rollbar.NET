namespace Rollbar.DTOs
{
    using Rollbar.Common;
    using Rollbar.Diagnostics;
    using Rollbar.Utils;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Xamarin.iOS.Foundation;

    /// <summary>
    /// Implements an abstract DTO type base.
    /// </summary>
    [Preserve]
    public abstract class DtoBase
        : ITraceable
    {

        #region strings truncation support

        private static readonly IReadOnlyDictionary<Type, PropertyInfo[]> stringPropertiesByType = null;
        private static readonly IReadOnlyDictionary<Type, PropertyInfo[]> dictionaryPropertiesByType = null;
        private static readonly IReadOnlyDictionary<Type, PropertyInfo[]> dtoPropertiesByType = null;
        private static readonly IReadOnlyDictionary<Type, PropertyInfo[]> enumerablePropertiesByType = null;

        internal PropertyInfo[] StringProperties
        {
            get { return DtoBase.stringPropertiesByType[this.GetType()]; }
        }

        internal PropertyInfo[] DictionaryProperties
        {
            get { return DtoBase.dictionaryPropertiesByType[this.GetType()]; }
        }

        internal PropertyInfo[] DtoProperties
        {
            get { return DtoBase.dtoPropertiesByType[this.GetType()]; }
        }

        internal PropertyInfo[] EnumerableProperties
        {
            get { return DtoBase.enumerablePropertiesByType[this.GetType()]; }
        }

        public virtual DtoBase TruncateStrings(Encoding encoding, int stringBytesLimit)
        {
            foreach(var property in this.StringProperties)
            {
                string originalString = property.GetValue(this) as string;
                string truncatedString = StringUtility.Truncate(originalString, encoding, stringBytesLimit);
                if (!object.ReferenceEquals(originalString, truncatedString))
                {
                    property.SetValue(this, truncatedString);
                }
            }

            foreach (var property in this.DictionaryProperties)
            {
                var objectDictionary = property.GetValue(this) as IDictionary<string, object>;
                if (objectDictionary != null)
                {
                    TruncateStringValues(objectDictionary, encoding, stringBytesLimit);
                    property.SetValue(this, objectDictionary);
                    continue;
                }

                var stringDictionary = property.GetValue(this) as IDictionary<string, string>;
                if (stringDictionary != null)
                {
                    TruncateStringValues(stringDictionary, encoding, stringBytesLimit);
                    property.SetValue(this, stringDictionary);
                    continue;
                }
            }

            foreach (var property in this.DtoProperties)
            {
                DtoBase dto = property.GetValue(this) as DtoBase;
                if (dto != null)
                {
                    dto.TruncateStrings(encoding, stringBytesLimit);
                }
            }

            foreach (var property in this.EnumerableProperties)
            {
                var enumerableItemTypes = property.PropertyType.GetInterfaces()
                    .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .Select(t => t.GetGenericArguments()[0])
                    .ToArray()
                    ;

                if (enumerableItemTypes == null || enumerableItemTypes.Length != 1)
                {
                    continue;
                }
                else if (DtoBase.dtoPropertiesByType.Keys.Contains(enumerableItemTypes[0]))
                {
                    var dtos = property.GetValue(this) as IEnumerable<DtoBase>;
                    if (dtos == null)
                    {
                        continue;
                    }
                    foreach(var dto in dtos)
                    {
                        dto.TruncateStrings(encoding, stringBytesLimit);
                    }
                }
                else if (enumerableItemTypes[0] == typeof(string))
                {
                    var stringArray = property.GetValue(this) as string[];
                    if (stringArray == null)
                    {
                        continue;
                    }
                    List<string> truncatedStrings = new List<string>(); 
                    foreach(var str in stringArray)
                    {
                        truncatedStrings.Add(StringUtility.Truncate(str, encoding, stringBytesLimit));
                    }
                    property.SetValue(this, truncatedStrings.ToArray());
                }
            }

            return this;
        }

        protected void TruncateStringValues(IDictionary<string, object> dictionary, Encoding encoding, int stringBytesLimit)
        {
            Assumption.AssertNotNull(dictionary, nameof(dictionary));

            var keys = dictionary.Keys.ToArray();
            foreach (var key in keys)
            {
                string originalString = dictionary[key] as string;
                if (originalString != null)
                {
                    string truncatedString = StringUtility.Truncate(originalString, encoding, stringBytesLimit);
                    if (!object.ReferenceEquals(originalString, truncatedString))
                    {
                        dictionary[key] = truncatedString;
                    }
                    continue;
                }

                var objectDictionary = dictionary[key] as IDictionary<string, object>;
                if (objectDictionary != null)
                {
                    TruncateStringValues(objectDictionary, encoding, stringBytesLimit);
                    continue;
                }

                var stringDictionary = dictionary[key] as IDictionary<string, string>;
                if (stringDictionary != null)
                {
                    TruncateStringValues(stringDictionary, encoding, stringBytesLimit);
                    continue;
                }

            }
        }

        protected void TruncateStringValues(IDictionary<string, string> dictionary, Encoding encoding, int stringBytesLimit)
        {
            Assumption.AssertNotNull(dictionary, nameof(dictionary));

            var keys = dictionary.Keys.ToArray();
            foreach (var key in keys)
            {
                string original = dictionary[key];
                if (original != null)
                {
                    string truncated = StringUtility.Truncate(original, encoding, stringBytesLimit);
                    if (!object.ReferenceEquals(original, truncated))
                    {
                        dictionary[key] = truncated;
                    }
                }
            }
        }

        static DtoBase()
        {
            DtoBase.stringPropertiesByType = DtoBase.ReflectStringProperies();
            DtoBase.dictionaryPropertiesByType = DtoBase.ReflectDictionaryProperies();
            DtoBase.dtoPropertiesByType = DtoBase.ReflectDtoProperies();
            DtoBase.enumerablePropertiesByType = DtoBase.ReflectEnumerableProperies();
        }

        private static IReadOnlyDictionary<Type, PropertyInfo[]> ReflectStringProperies()
        {
            Type[] derivedTypes =
                ReflectionUtil.GetSubClassesOf(typeof(DtoBase));

            var reflectedMetadata = new Dictionary<Type, PropertyInfo[]>(derivedTypes.Length - 1);
            Type stringType = typeof(string);

            foreach (var type in derivedTypes)
            {
                if (type == typeof(ExtendableDtoBase))
                {
                    continue;
                }

                var stringProperties =
                    type.GetProperties().Where(p => p.PropertyType == stringType).ToArray();
                reflectedMetadata.Add(type, stringProperties);
            }

            return reflectedMetadata;
        }

        private static IReadOnlyDictionary<Type, PropertyInfo[]> ReflectDictionaryProperies()
        {
            Type[] derivedTypes =
                ReflectionUtil.GetSubClassesOf(typeof(DtoBase));

            var reflectedMetadata = new Dictionary<Type, PropertyInfo[]>(derivedTypes.Length - 1);
            Type dictionaryType = typeof(Dictionary<,>);

            foreach (var type in derivedTypes)
            {
                if (type == typeof(ExtendableDtoBase))
                {
                    continue;
                }

                Type[] typesOfInterest = new Type[] {
                    typeof(IDictionary<string, object>),
                    typeof(IDictionary<string, string>),
                };

                var properties =
                    type.GetProperties()
                    .Where(p => !derivedTypes.Contains(p.PropertyType) &&
                        typesOfInterest.Contains(p.PropertyType)
                        )
                    .ToArray();
                reflectedMetadata.Add(type, properties);

            }

            return reflectedMetadata;
        }

        private static IReadOnlyDictionary<Type, PropertyInfo[]> ReflectDtoProperies()
        {
            Type[] derivedTypes =
                ReflectionUtil.GetSubClassesOf(typeof(DtoBase));

            var reflectedMetadata = new Dictionary<Type, PropertyInfo[]>(derivedTypes.Length - 1);

            foreach (var type in derivedTypes)
            {
                var properties =
                    type.GetProperties().Where(p => derivedTypes.Contains(p.PropertyType)).ToArray();
                reflectedMetadata.Add(type, properties);
            }

            return reflectedMetadata;
        }

        private static IReadOnlyDictionary<Type, PropertyInfo[]> ReflectEnumerableProperies()
        {
            Type[] dtoTypes =
                ReflectionUtil.GetSubClassesOf(typeof(DtoBase));

            var reflectedMetadata = new Dictionary<Type, PropertyInfo[]>(dtoTypes.Length - 1);

            foreach (var type in dtoTypes)
            {
                var properties = type.GetProperties()
                    .Where(p => p.PropertyType.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition()).Contains(typeof(IEnumerable<>))
                    )
                    .ToArray();
                reflectedMetadata.Add(type, properties);
            }

            return reflectedMetadata;
        }
        #endregion strings truncation support

        /// <summary>
        /// Traces as string.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <returns>
        /// String rendering of this instance.
        /// </returns>
        public virtual string TraceAsString(string indent = "")
        {
            return this.ToString();
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        public virtual void Validate()
        {
        }

    }
}
