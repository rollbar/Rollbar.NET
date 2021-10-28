namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Xamarin.iOS.Foundation;
    using Rollbar.Common;
    using Rollbar.Diagnostics;

    /// <summary>
    /// Implements an abstract DTO type base.
    /// </summary>
    [Preserve]
    public abstract class DtoBase
        : ITraceable
        , IValidatable
    {

        #region strings truncation support

        private static readonly IReadOnlyDictionary<Type, PropertyInfo[]> stringPropertiesByType = DtoBase.ReflectStringProperies();
        private static readonly IReadOnlyDictionary<Type, PropertyInfo[]> dictionaryPropertiesByType = DtoBase.ReflectDictionaryProperies();
        private static readonly IReadOnlyDictionary<Type, PropertyInfo[]> dtoPropertiesByType = DtoBase.ReflectDtoProperies();
        private static readonly IReadOnlyDictionary<Type, PropertyInfo[]> enumerablePropertiesByType = DtoBase.ReflectEnumerableProperies();

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


        /// <summary>
        /// Truncates the strings.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <param name="stringBytesLimit">The string bytes limit.</param>
        /// <returns></returns>
        public virtual DtoBase TruncateStrings(Encoding encoding, int stringBytesLimit)
        {
            foreach(var property in this.StringProperties)
            {
                string? originalString = property.GetValue(this) as string;
                string? truncatedString = StringUtility.Truncate(originalString, encoding, stringBytesLimit);
                if (!object.ReferenceEquals(originalString, truncatedString))
                {
                    property.SetValue(this, truncatedString);
                }
            }

            foreach (var property in this.DictionaryProperties)
            {
                if (property.GetValue(this) is IDictionary<string, object?> objectDictionary)
                {
                    TruncateStringValues(objectDictionary, encoding, stringBytesLimit);
                    property.SetValue(this, objectDictionary);
                    continue;
                }

                if (property.GetValue(this) is IDictionary<string, string?> stringDictionary)
                {
                    TruncateStringValues(stringDictionary, encoding, stringBytesLimit);
                    property.SetValue(this, stringDictionary);
                    continue;
                }
            }

            foreach (var property in this.DtoProperties)
            {
                if (property.GetValue(this) is DtoBase dto)
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
                    if (property.GetValue(this) is not IEnumerable<DtoBase> dtos)
                    {
                        continue;
                    }
                    foreach (var dto in dtos)
                    {
                        dto.TruncateStrings(encoding, stringBytesLimit);
                    }
                }
                else if (enumerableItemTypes[0] == typeof(string))
                {
                    if (property.GetValue(this) is not string[] stringArray)
                    {
                        continue;
                    }
                    List<string?> truncatedStrings = new(); 
                    foreach(var str in stringArray)
                    {
                        truncatedStrings.Add(StringUtility.Truncate(str, encoding, stringBytesLimit));
                    }
                    property.SetValue(this, truncatedStrings.ToArray());
                }
            }

            return this;
        }


        /// <summary>
        /// Truncates the string values.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="stringBytesLimit">The string bytes limit.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3626:Jump statements should not be redundant", Justification = "Better communicates intent of the code.")]
        protected void TruncateStringValues(IDictionary<string, object?> dictionary, Encoding encoding, int stringBytesLimit)
        {
            Assumption.AssertNotNull(dictionary, nameof(dictionary));

            var keys = dictionary.Keys.ToArray();
            foreach (var key in keys)
            {
                if (dictionary[key] is string originalString)
                {
                    string? truncatedString = StringUtility.Truncate(originalString, encoding, stringBytesLimit);
                    if (!object.ReferenceEquals(originalString, truncatedString))
                    {
                        dictionary[key] = truncatedString;
                    }
                    continue;
                }

                if (dictionary[key] is IDictionary<string, object?> objectDictionary)
                {
                    TruncateStringValues(objectDictionary, encoding, stringBytesLimit);
                    continue;
                }

                if (dictionary[key] is IDictionary<string, string?> stringDictionary)
                {
                    TruncateStringValues(stringDictionary, encoding, stringBytesLimit);
                    continue;
                }
            }
        }

        /// <summary>
        /// Truncates the string values.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="stringBytesLimit">The string bytes limit.</param>
        protected static void TruncateStringValues(IDictionary<string, string?> dictionary, Encoding encoding, int stringBytesLimit)
        {
            Assumption.AssertNotNull(dictionary, nameof(dictionary));

            var keys = dictionary.Keys.ToArray();
            foreach (var key in keys)
            {
                string? original = dictionary[key];
                if (original != null)
                {
                    string? truncated = StringUtility.Truncate(original, encoding, stringBytesLimit);
                    if (!object.ReferenceEquals(original, truncated))
                    {
                        dictionary[key] = truncated;
                    }
                }
            }
        }

        private static IReadOnlyDictionary<Type, PropertyInfo[]> ReflectStringProperies()
        {
            Type[] derivedTypes =
                ReflectionUtility.GetSubClassesOf(typeof(DtoBase));

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
                ReflectionUtility.GetSubClassesOf(typeof(DtoBase));

            var reflectedMetadata = new Dictionary<Type, PropertyInfo[]>(derivedTypes.Length - 1);

            foreach (var type in derivedTypes)
            {
                if (type == typeof(ExtendableDtoBase))
                {
                    continue;
                }

                Type[] typesOfInterest = new [] {
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
                ReflectionUtility.GetSubClassesOf(typeof(DtoBase));

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
                ReflectionUtility.GetSubClassesOf(typeof(DtoBase));

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
        /// <returns>System.String.</returns>
        public virtual string TraceAsString()
        {
            return this.TraceAsString(string.Empty);
        }

        /// <summary>
        /// Traces as string.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <returns>
        /// String rendering of this instance.
        /// </returns>
        public virtual string TraceAsString(string indent)
        {
            return this.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns>IReadOnlyCollection&lt;ValidationResult&gt; containing failed validation rules.</returns>
        public IReadOnlyCollection<ValidationResult> Validate()
        {
            var validator = this.GetValidator();

            var failedValidations = validator?.Validate(this);

            if (failedValidations == null)
            {
                // it is always better to return an empty collection instead of null:
                failedValidations = ArrayUtility.GetEmptyArray<ValidationResult>();
            }

            return failedValidations;
        }

        /// <summary>
        /// Gets the proper validator.
        /// </summary>
        /// <returns>Validator.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual Validator? GetValidator()
        {
            return null;
        }
    }
}
