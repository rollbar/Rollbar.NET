namespace Rollbar.DTOs
{
    using Rollbar.Utils;
    using System;
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
        private static readonly IReadOnlyDictionary<Type, PropertyInfo[]> stringPropertiesByType = null;

        static DtoBase()
        {
            DtoBase.stringPropertiesByType = DtoBase.ReflectDtoStringProperies();
        }

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

        private static IReadOnlyDictionary<Type, PropertyInfo[]> ReflectDtoStringProperies()
        {
            Type[] derivedTypes =
                ReflectionUtil.GetSubClassesOf(typeof(DtoBase));

            var reflectedMetadata = new Dictionary<Type, PropertyInfo[]>(derivedTypes.Length - 1);
            Type stringType = typeof(string);

            foreach(var type in derivedTypes)
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

    }
}
