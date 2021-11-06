namespace Rollbar.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    using Rollbar.Diagnostics;
    using Rollbar.Infrastructure;

    /// <summary>
    /// A utility class aiding with .NET Reflection.
    /// </summary>
    internal static class ReflectionUtility
    {
        #region data fields and properties discovery

        /// <summary>
        /// Gets all data fields.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>FieldInfo[].</returns>
        [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "By design.")]
        public static FieldInfo[] GetAllDataFields(Type type)
        {
            if (type == null)
            {
                return ArrayUtility.GetEmptyArray<FieldInfo>();
            }

            List<Type> relevantTypes = new();
            Type? relevantType = type;
            while (relevantType != null)
            {
                relevantTypes.Add(relevantType);
                relevantType = relevantType.BaseType;
            }

            List<FieldInfo> fieldInfos = new();
            relevantTypes.Reverse(); // eventually, we want following order: most base type's data fields first...
            foreach (var t in relevantTypes)
            {
                fieldInfos.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic));
            }

            return fieldInfos.ToArray();
        }

        /// <summary>
        /// Gets all public static fields.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>All discovered FieldInfos.</returns>
        [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "By design.")]
        public static FieldInfo[] GetAllStaticDataFields(Type type)
        {
            var memberInfos =
                type.GetFields(BindingFlags.Static | BindingFlags.NonPublic);

            return memberInfos;
        }

        /// <summary>
        /// Gets all public static fields.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>All discovered FieldInfos.</returns>
        public static FieldInfo[] GetAllPublicStaticFields(Type type)
        {
            var memberInfos =
                type.GetFields(BindingFlags.Static | BindingFlags.Public);

            return memberInfos;
        }

        /// <summary>
        /// Gets all public instance properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static PropertyInfo[] GetAllPublicInstanceProperties(Type type)
        {
            var memberInfos = 
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            return memberInfos;
        }

        /// <summary>
        /// Gets all public static properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>FieldInfo[].</returns>
        public static PropertyInfo[] GetAllPublicStaticProperties(Type type)
        {
            var memberInfos =
                type.GetProperties(BindingFlags.Static | BindingFlags.Public);

            return memberInfos;
        }

        /// <summary>
        /// Gets the static field value.
        /// </summary>
        /// <typeparam name="TFieldDataType">The type of the field data type.</typeparam>
        /// <param name="staticField">The static field.</param>
        /// <returns></returns>
        public static TFieldDataType? GetStaticFieldValue<TFieldDataType>(FieldInfo staticField)
        {
            Assumption.AssertTrue(staticField.IsStatic, nameof(staticField.IsStatic));

            object? valueObject = staticField.GetValue(null);
            if(valueObject != null)
            {
                TFieldDataType result = (TFieldDataType) valueObject;

                return result;
            }

            return default;
        }

        /// <summary>
        /// Gets all public static field values.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="type">The type.</param>
        /// <returns>All the field values.</returns>
        public static TField?[] GetAllPublicStaticFieldValues<TField>(Type type)
        {
            var memberInfos =
                type.GetFields(BindingFlags.GetField | BindingFlags.Static | BindingFlags.Public);

            var values = new List<TField?>(memberInfos.Length);
            foreach (var memberInfo in memberInfos)
            {
                object? valueObject = memberInfo.GetValue(null);
                TField? value = valueObject != null ? (TField) valueObject : default;
                values.Add(value);
            }
            return values.ToArray();
        }

        #endregion data fields and properties discovery

        /// <summary>
        /// Gets the nested types.
        /// </summary>
        /// <param name="hostType">Type of the host.</param>
        /// <param name="nestedTypesBindingFlags">The nested types binding flags.</param>
        /// <returns></returns>
        public static Type[] GetNestedTypes(Type hostType, BindingFlags nestedTypesBindingFlags = BindingFlags.Public)
        {
            return hostType.GetNestedTypes(nestedTypesBindingFlags);
        }

        /// <summary>
        /// Gets the nested type by its name.
        /// </summary>
        /// <param name="hostType">Type of the host.</param>
        /// <param name="nestedTypeName">Name of the nested type (without its namespace).</param>
        /// <param name="nestedTypeBindingFlags">The nested type binding flags.</param>
        /// <returns></returns>
        public static Type? GetNestedTypeByName(Type hostType, string nestedTypeName, BindingFlags nestedTypeBindingFlags = BindingFlags.Public)
        {
            return hostType.GetNestedType(nestedTypeName, nestedTypeBindingFlags);
        }

        /// <summary>
        /// Gets the sub classes of a given type 
        /// from the same assembly where the base type is defined.
        /// </summary>
        /// <param name="baseType">Type of the base.</param>
        /// <returns>Array of the derived types</returns>
        public static Type[] GetSubClassesOf(Type baseType)
        {
            var assembly = baseType.Assembly;
            return GetSubClassesOf(baseType, assembly);
        }

        /// <summary>
        /// Gets the sub classes of.
        /// </summary>
        /// <param name="baseType">Type of the base.</param>
        /// <param name="searchAssembly">The search assembly.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetSubClassesOf(Type baseType, Assembly searchAssembly)
        {
            List<Type> types = searchAssembly.GetTypes()
                .Where(t => t.IsSubclassOf(baseType))
                .ToList();
            if (!types.Any())
            {
                types.Add(baseType);
            }
            return types.ToArray();
        }

        /// <summary>
        /// Does the type implement interface.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <returns><c>true</c> if implements, <c>false</c> otherwise.</returns>
        public static bool DoesTypeImplementInterface(Type type, Type interfaceType)
        {
            Assumption.AssertNotNull(type, nameof(type));
            Assumption.AssertNotNull(interfaceType, nameof(interfaceType));
            Assumption.AssertTrue(interfaceType.IsInterface, nameof(interfaceType));

            return type.GetInterfaces().Any(i => i.FullName == interfaceType.FullName);
        }

        /// <summary>
        /// Gets the types hierarchy including the provided type (if any) as the first one in the array.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetTypesHierarchy(Type? type)
        {
            if(type == null)
            {
                return ArrayUtility.GetEmptyArray<Type>();
            }

            var baseTypes = ReflectionUtility.GetBaseTypesHierarchy(type);
            List<Type> types = new(baseTypes.Length + 1);
            types.Add(type);
            types.AddRange(baseTypes);
            return types.ToArray();
        }

        /// <summary>
        /// Gets the base types hierarchy for the provided type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetBaseTypesHierarchy(Type type)
        {
            Type? baseType = type?.BaseType;
            if(type == null || baseType == null)
            {
                return ArrayUtility.GetEmptyArray<Type>();
            }

            List<Type> baseTypesList = new();
            baseTypesList.Add(baseType);
            baseTypesList.AddRange(ReflectionUtility.GetBaseTypesHierarchy(baseType));
            return baseTypesList.ToArray();
        }

        /// <summary>
        /// Gets the common hierarchy types.
        /// </summary>
        /// <param name="lType">Type of the l.</param>
        /// <param name="rType">Type of the r.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetCommonHierarchyTypes(Type lType, Type rType)
        {
            var bestCommonType = ReflectionUtility.GetTopCommonSuperType(lType, rType);
            return ReflectionUtility.GetTypesHierarchy(bestCommonType);
        }

        /// <summary>
        /// Gets the type of the top common super.
        /// </summary>
        /// <param name="lType">Type of the left type.</param>
        /// <param name="rType">Type of the right type</param>
        /// <returns>Type.</returns>
        public static Type? GetTopCommonSuperType(Type lType, Type rType)
        {
            var lBases = ReflectionUtility.GetTypesHierarchy(lType);
            var rBases = ReflectionUtility.GetTypesHierarchy(rType);

            foreach(var lBase in lBases)
            {
                foreach(var rBase in rBases)
                {
                    if (lBase == rBase)
                    {
                        return lBase;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the implemented interface types.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetImplementedInterfaceTypes(Type type)
        {
            if (type == null)
            {
                return ArrayUtility.GetEmptyArray<Type>();
            }

            return type.GetInterfaces();
        }

        /// <summary>
        /// Gets the common implemented interfaces among provided types.
        /// </summary>
        /// <param name="lType">Type of the left type.</param>
        /// <param name="rType">Type of the right type.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetCommonImplementedInterfaces(Type lType, Type rType)
        {
            return ReflectionUtility.GetCommonImplementedInterfaces(new Type[] { lType, rType});
        }

        /// <summary>
        /// Gets the common implemented interfaces among provided types.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetCommonImplementedInterfaces(Type[] types)
        {
            if (types == null || types.Length == 0)
            {
                return ArrayUtility.GetEmptyArray<Type>();
            }

            switch (types.Length)
            {
                case 1:
                    return ReflectionUtility.GetImplementedInterfaceTypes(types.First());
                default:
                    Type[] commonInterfaceTypes = 
                        ReflectionUtility.GetImplementedInterfaceTypes(types.First());
                    int i = 0;
                    while ((commonInterfaceTypes.Length > 0) && (++i < types.Length))
                    {
                        commonInterfaceTypes = 
                            commonInterfaceTypes
                            .Intersect(ReflectionUtility.GetImplementedInterfaceTypes(types[i]))
                            .ToArray();
                    }
                    return commonInterfaceTypes.ToArray();
            }
        }

        /// <summary>
        /// Loads the SDK module assembly.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns>System.Nullable&lt;Assembly&gt;.</returns>
        [SuppressMessage("Major Code Smell", "S3885:\"Assembly.Load\" should be used", Justification = "Good for now for what we need. NOTE: we may want to reconsider it in the future.")]
        public static Assembly? LoadSdkModuleAssembly(string? assemblyName)
        {
            if (string.IsNullOrWhiteSpace(assemblyName))
            {
                return null;
            }

            try
            {
                var assembly = Assembly.LoadFrom(assemblyName);

                return assembly;
            }
            catch (Exception ex)
            {
                RollbarErrorUtility.Report(
                    rollbarLogger: null, 
                    dataObject: null, 
                    rollbarError: InternalRollbarError.SdkModuleLoaderError, 
                    message: $"Couldn't load `{assemblyName}` assembly module!", 
                    exception: ex, 
                    errorCollector: null
                    );

                return null;
            }
        }
    }
}
