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
        /// Captures the state.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="instanceName">Name of the instance.</param>
        /// <returns></returns>
        public static IDictionary<string, object> CaptureState(object instance, string instanceName = null)
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

            IDictionary<string, object> stateCapture = new Dictionary<string, object>(memberInfos.Length);
            foreach(var memberInfo in memberInfos)
            {
                stateCapture[dataMemberNamePrefix + memberInfo.Name] = memberInfo.GetValue(instance);
            }

            return stateCapture;
        }

        /// <summary>
        /// Captures the state.
        /// </summary>
        /// <param name="staticType">Type of a static class.</param>
        /// <returns></returns>
        public static IDictionary<string, object> CaptureState(Type staticType)
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

            string dataMemberNamePrefix = $"{staticType.FullName}.";
            var memberInfos = ReflectionUtil.GetAllDataFields(staticType);

            IDictionary<string, object> stateCapture = new Dictionary<string, object>(memberInfos.Length);
            foreach (var memberInfo in memberInfos)
            {
                stateCapture[memberInfo.Name] = memberInfo.GetValue(null);
            }

            return stateCapture;
        }
    }
}
