namespace Rollbar
{
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal static class RollbarUtil
    {
        //public static IAsyncLogger LogUsingProperObjectDiscovery(IRollbar rollbar, ErrorLevel level, object obj, IDictionary<string, object> custom = null)
        //{
        //    if (rollbar.Config.LogLevel.HasValue && level < rollbar.Config.LogLevel.Value)
        //    {
        //        // nice shortcut:
        //        return rollbar;
        //    }

        //    Data data = obj as Data;
        //    if (data != null)
        //    {
        //        data.Level = level;
        //        return rollbar.Log(data);
        //    }
        //    System.Exception exception = obj as System.Exception;
        //    if (exception != null)
        //    {
        //        Body exceptionBody = new Body(exception);
        //        Data exceptionData = new Data(rollbar.Config, exceptionBody, custom);
        //        rollbar.Log(exceptionData);
        //        return rollbar;
        //    }
        //    ITraceable traceable = obj as ITraceable;
        //    if (traceable != null)
        //    {
        //        return rollbar.Log(level, traceable.TraceAsString(), custom);
        //    }

        //    return rollbar.Log(level, obj.ToString(), custom);
        //}

        public static Data PackageAsPayloadData(IRollbarConfig rollbarConfig, ErrorLevel level, object obj, IDictionary<string, object> custom = null)
        {
            if (rollbarConfig.LogLevel.HasValue && level < rollbarConfig.LogLevel.Value)
            {
                // nice shortcut:
                return null;
            }

            Data data = obj as Data;
            if (data != null)
            {
                data.Level = level;
                return data;
            }

            Body body = obj as Body;
            if (body == null)
            {
                body = RollbarUtil.PackageAsPayloadBody(obj, ref custom);
            }
            
            data = new Data(rollbarConfig, body, custom);
            data.Level = level;
            return data;
        }

        public static Body PackageAsPayloadBody(object bodyObject, ref IDictionary<string, object> custom)
        {
            System.Exception exception = bodyObject as System.Exception;
            if (exception != null)
            {
                RollbarUtil.SnapExceptionDataAsCustomData(exception, ref custom);
                return new Body(exception);
            }

            string messageString = bodyObject as string;
            if (messageString != null)
            {
                return new Body(new Message(messageString));
            }

            ITraceable traceable = bodyObject as ITraceable;
            if (traceable != null)
            {
                return new Body(traceable.TraceAsString());
            }

            return new Body(new Message(bodyObject.ToString()));
        }

        public static void SnapExceptionDataAsCustomData(
            System.Exception e,
            ref IDictionary<string, object> custom
            )
        {
            if (custom == null)
            {
                custom =
                    new Dictionary<string, object>(capacity: e.Data != null ? e.Data.Count : 0);
            }

            const string nullObjPresentation = "<null>";
            if (e.Data != null)
            {
                string customKeyPrefix = $"{e.GetType().Name}.Data.";
                foreach (var key in e.Data.Keys)
                {
                    // Some of the null-checks here may look unnecessary for the way an IDictionary
                    // is implemented today. 
                    // But the things could change tomorrow and we want to stay safe always:
                    object valueObj = e.Data[key];
                    if (valueObj == null && key == null)
                    {
                        continue;
                    }
                    string keyName = (key != null) ? key.ToString() : nullObjPresentation;
                    string customKey = $"{customKeyPrefix}{keyName}";
                    custom[customKey] = valueObj ?? nullObjPresentation;
                }
            }

            if (e.InnerException != null)
            {
                SnapExceptionDataAsCustomData(e.InnerException, ref custom);
            }

            // there could be more Data to capture in case of an AggregateException:
            AggregateException aggregateException = e as AggregateException;
            if (aggregateException != null && aggregateException.InnerExceptions != null)
            {
                foreach (var aggregatedException in aggregateException.InnerExceptions)
                {
                    SnapExceptionDataAsCustomData(aggregatedException, ref custom);
                }
            }
        }

    }
}
