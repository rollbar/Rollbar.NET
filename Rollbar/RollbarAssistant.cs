namespace Rollbar
{
    using Rollbar.Diagnostics;
    using Rollbar.Utils;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// This is a utility class assisting in collecting extra information for logging.
    /// </summary>
    public static class RollbarAssistant
    {
        /// <summary>
        /// Captures the state (all the data fields' values of the provided instance).
        /// 
        /// It captures all the static and instance data fields (public and non-public) 
        /// including the inherited ones (if there is any).
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="instanceName">Name of the instance.</param>
        /// <param name="stateCapture">a instance of a state capture bag.</param>
        /// <returns>
        /// either the provided instance or a new instance of a state capture bag that is a dictionary of data field name/value pairs representing 
        /// the captured state of the supplied instance object or null whenever the state capture is not applicable 
        /// (for example, when instance argument happened to be uninitialized)
        /// </returns>
        public static IDictionary<string, object> CaptureState(object instance, string instanceName = null, IDictionary<string, object> stateCapture = null)
        {
            if (instance == null)
            {
                return null;
            }

            Type instanceType = instance.GetType();
            if (instanceType == null)
            {
                return null;
            }
            if (instanceType.IsInterface || instanceType.IsEnum)
            {
                return null;
            }
            if (instanceType == typeof(Type) && instanceType.IsAbstract && instanceType.IsSealed)
            {
                return RollbarAssistant.CaptureState((Type)instance);
            }

            string dataMemberNamePrefix = $"{instanceName ?? string.Empty}[{instanceType.FullName}].";
            var memberInfos = ReflectionUtil.GetAllDataFields(instanceType);

            if (stateCapture == null)
            {
                stateCapture = new Dictionary<string, object>(memberInfos.Length);
            }
            foreach (var memberInfo in memberInfos)
            {
                object value = memberInfo.GetValue(instance);
                if (value != null && value.GetType().IsEnum)
                {
                    value = value.ToString();
                }
                stateCapture[dataMemberNamePrefix + memberInfo.Name] = value;
            }

            return stateCapture;
        }

        /// <summary>
        /// Captures the state of a static type (all the data fields' values of the provided static type).
        /// </summary>
        /// <param name="staticType">Type of a static class.</param>
        /// <param name="stateCapture">a instance of a state capture bag.</param>
        /// <returns>
        /// either the provided instance or a new instance of a state capture bag that is a dictionary of data field name/value pairs representing 
        /// the captured state of the supplied static type or null whenever the state capture is not applicable 
        /// (for example, when static type argument happened to represent an Enum or an interface).
        /// </returns>
        public static IDictionary<string, object> CaptureState(Type staticType, IDictionary<string, object> stateCapture = null)
        {
            //Assumption.AssertTrue(
            //    staticType.IsAbstract && staticType.IsSealed 
            //    && !staticType.IsInterface && !staticType.IsEnum
            //    , nameof(staticType)
            //    );

            if (staticType.IsInterface || staticType.IsEnum)
            {
                return null;
            }

            string dataMemberNamePrefix = $"[{staticType.FullName}].";
            var memberInfos = ReflectionUtil.GetAllDataFields(staticType);

            if (stateCapture == null)
            {
                stateCapture = new Dictionary<string, object>(memberInfos.Length);
            }
            foreach (var memberInfo in memberInfos)
            {
                stateCapture[dataMemberNamePrefix + memberInfo.Name] = memberInfo.GetValue(null);
            }

            return stateCapture;
        }
    }
}
