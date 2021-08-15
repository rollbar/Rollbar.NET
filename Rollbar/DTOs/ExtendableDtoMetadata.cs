namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Rollbar.Common;
    using Rollbar.Diagnostics;

    internal class ExtendableDtoMetadata
    {
        public Type? ExtendableDtoType { get; private set; }

        public IReadOnlyDictionary<string, PropertyInfo>? ReservedPropertyInfoByReservedKey { get; private set; }

        public static IReadOnlyDictionary<Type, ExtendableDtoMetadata> BuildAll()
        {
            Type[] derivedTypes = 
                ReflectionUtility.GetSubClassesOf(typeof(ExtendableDtoBase));

            Dictionary<Type, ExtendableDtoMetadata> result = 
                new Dictionary<Type, ExtendableDtoMetadata>(derivedTypes.Length);

            foreach(var type in derivedTypes)
            {
                result.Add(type, Build(type));
            }

            return result;
        }

        private static ExtendableDtoMetadata Build(Type extendableDtoType)
        {
            Assumption.AssertNotNull(extendableDtoType, nameof(extendableDtoType));

            ExtendableDtoMetadata result = new ExtendableDtoMetadata();
            result.ExtendableDtoType = extendableDtoType;

            List<Type> reservedPropertiesNestedTypes = new List<Type>();
            Type extendableDtoHierarchyType = extendableDtoType;
            while (extendableDtoHierarchyType != null)
            {
                Type? reservedPropertiesNestedType = ReflectionUtility.GetNestedTypeByName(
                    extendableDtoHierarchyType,
                    ExtendableDtoBase.reservedPropertiesNestedTypeName,
                    BindingFlags.Public | BindingFlags.Static
                    );
                if (reservedPropertiesNestedType != null)
                {
                    reservedPropertiesNestedTypes.Add(reservedPropertiesNestedType);
                }
                if (extendableDtoHierarchyType.BaseType == typeof(ExtendableDtoBase))
                {
                    break;
                }
                if (extendableDtoHierarchyType.BaseType != null)
                {
                    extendableDtoHierarchyType = extendableDtoHierarchyType.BaseType;
                }
            }

            List<FieldInfo> reservedAttributes = new List<FieldInfo>();
            foreach (Type reservedPropertiesNestedType in reservedPropertiesNestedTypes)
            {
                reservedAttributes.AddRange(
                    ReflectionUtility.GetAllPublicStaticFields(reservedPropertiesNestedType)
                    );
            }

            Dictionary<string, PropertyInfo> reservedPropertyInfoByName = 
                new Dictionary<string, PropertyInfo>(reservedAttributes.Count);
            result.ReservedPropertyInfoByReservedKey = reservedPropertyInfoByName;

            foreach(var reservedAttribue in reservedAttributes)
            {
                var property = 
                    extendableDtoType?
                    .GetProperty(
                        reservedAttribue.Name, 
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy
                        );
                Assumption.AssertNotNull(property, nameof(property));

                string? reservedKey = ReflectionUtility.GetStaticFieldValue<string>(reservedAttribue);
                Assumption.AssertNotNullOrWhiteSpace(reservedKey, nameof(reservedKey));

                if(reservedKey != null)
                {
                    reservedPropertyInfoByName.Add(reservedKey, property!);
                }
            }

            return result;
        }
    }
}
