namespace Rollbar.Common
{
    using Rollbar.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A utility class aiding with .NET Reflection.
    /// </summary>
    internal static class ReflectionUtility
    {
        /// <summary>
        /// Gets all data fields.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>FieldInfo[].</returns>
        public static FieldInfo[] GetAllDataFields(Type type)
        {
            if (type == null)
            {
                return new FieldInfo[0];
            }

            List<Type> relevantTypes = new List<Type>();
            Type relevantType = type;
            while (relevantType != null)
            {
                relevantTypes.Add(relevantType);
                relevantType = relevantType.BaseType;
            }

            List<FieldInfo> fieldInfos = new List<FieldInfo>();
            relevantTypes.Reverse(); // eventually, we want following order: most base type's data fields first...
            foreach (var t in relevantTypes)
            {
                fieldInfos.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic));
            }

            return fieldInfos.ToArray();
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
        /// Gets all public static fields.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>All discovered FieldInfos.</returns>
        public static FieldInfo[] GetAllPublicStaticFields(Type type)
        {
            var memberInfos =
                type.GetFields(BindingFlags.GetField | BindingFlags.Static | BindingFlags.Public);

            return memberInfos;
        }

        /// <summary>
        /// Gets the static field value.
        /// </summary>
        /// <typeparam name="TFieldDataType">The type of the field data type.</typeparam>
        /// <param name="staticField">The static field.</param>
        /// <returns></returns>
        public static TFieldDataType GetStaticFieldValue<TFieldDataType>(FieldInfo staticField)
        {
            Assumption.AssertTrue(staticField.IsStatic, nameof(staticField.IsStatic));

            TFieldDataType result  = (TFieldDataType)staticField.GetValue(null);

            return result;
        }

        /// <summary>
        /// Gets all public static field values.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="type">The type.</param>
        /// <returns>All the field values.</returns>
        public static TField[] GetAllPublicStaticFieldValues<TField>(Type type)
        {
            var memberInfos =
                type.GetFields(BindingFlags.GetField | BindingFlags.Static | BindingFlags.Public);

            var values = new List<TField>(memberInfos.Length);
            foreach (var memberInfo in memberInfos)
            {
                values.Add((TField)memberInfo.GetValue(null));
            }
            return values.ToArray();
        }

        /// <summary>
        /// Gets the nested types.
        /// </summary>
        /// <param name="hostType">Type of the host.</param>
        /// <param name="nestedTypesBindingFlags">The nested types binding flags.</param>
        /// <returns></returns>
        public static Type[] GetNestedTypes(Type hostType, BindingFlags nestedTypesBindingFlags)
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
        public static Type GetNestedTypeByName(Type hostType, string nestedTypeName, BindingFlags nestedTypeBindingFlags)
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

    }
}
