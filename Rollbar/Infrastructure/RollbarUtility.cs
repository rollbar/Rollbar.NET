namespace Rollbar.Infrastructure
{
    using Rollbar.Common;
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class RollbarUtility.
    /// Aids in packaging logged objects as Rollbar DTO data structures (i.e. payloads).
    /// </summary>
    internal static class RollbarUtility
    {
        /// <summary>
        /// Packages as payload data.
        /// </summary>
        /// <param name="utcTimestamp">The UTC timestamp of when the object-to-log was captured.</param>
        /// <param name="rollbarConfig">The rollbar configuration.</param>
        /// <param name="level">The level.</param>
        /// <param name="obj">The object.</param>
        /// <param name="custom">The custom.</param>
        /// <returns>Data.</returns>
        public static Data? PackageAsPayloadData(
            DateTime utcTimestamp,
            IRollbarLoggerConfig rollbarConfig, 
            ErrorLevel level, 
            object obj, 
            IDictionary<string, object?>? custom = null
            )
        {
            if (!rollbarConfig.RollbarDeveloperOptions.Enabled)
            {
                // nice shortcut:
                return null;
            }

            if (level < rollbarConfig.RollbarDeveloperOptions.LogLevel)
            {
                // nice shortcut:
                return null;
            }

            Data? data = obj as Data;
            if (data != null)
            {
                //we do not have to update the timestamp of the data here
                //because we already have the incoming object to log of DTOs.Data type
                //so the timestamp value assigned during its construction should work better:
                data.Level = level;
                return data;
            }

            IRollbarPackage? rollbarPackagingStrategy = obj as IRollbarPackage;
            if (rollbarPackagingStrategy != null)
            {
                data = rollbarPackagingStrategy.PackageAsRollbarData();
                if (data != null)
                {
                    data.Environment = rollbarConfig?.RollbarDestinationOptions.Environment;
                    data.Level = level;
                    //update the data timestamp from the data creation timestamp to the passed
                    //object-to-log capture timestamp:
                    data.Timestamp = DateTimeUtil.ConvertToUnixTimestampInSeconds(utcTimestamp);
                }
                return data;
            }

            Body? body = obj as Body;
            if (body == null)
            {
                body = RollbarUtility.PackageAsPayloadBody(obj, ref custom);
            }
            
            data = new Data(rollbarConfig, body, custom);
            data.Level = level;
            //update the data timestamp from the data creation timestamp to the passed
            //object-to-log capture timestamp:
            data.Timestamp = DateTimeUtil.ConvertToUnixTimestampInSeconds(utcTimestamp);
            return data;
        }

        /// <summary>
        /// Packages as payload body.
        /// </summary>
        /// <param name="bodyObject">The body object.</param>
        /// <param name="custom">The custom.</param>
        /// <returns>Body.</returns>
        public static Body PackageAsPayloadBody(object bodyObject, ref IDictionary<string, object?>? custom)
        {
            System.Exception? exception = bodyObject as System.Exception;
            if (exception != null)
            {
                RollbarUtility.SnapExceptionDataAsCustomData(exception, ref custom);
                return new Body(exception);
            }

            string? messageString = bodyObject as string;
            if (messageString != null)
            {
                return new Body(new Message(messageString));
            }

            ITraceable? traceable = bodyObject as ITraceable;
            if (traceable != null)
            {
                return new Body(traceable.TraceAsString());
            }

            return new Body(new Message(bodyObject.ToString()));
        }

        /// <summary>
        /// Snaps the exception data as custom data.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="custom">The custom.</param>
        public static void SnapExceptionDataAsCustomData(
            System.Exception e,
            ref IDictionary<string, object?>? custom
            )
        {
            if (custom == null)
            {
                custom =
                    new Dictionary<string, object?>(capacity: e.Data != null ? e.Data.Count : 0);
            }

            const string nullObjPresentation = "<null>";
            if (e.Data != null)
            {
                string customKeyPrefix = $"{e.GetType().Name}.Data.";
                foreach (var key in e.Data.Keys)
                {
                    if(key == null)
                    {
                        continue;
                    }
                    object? valueObj = e.Data[key];
                    string keyName = key.ToString() ?? nullObjPresentation;
                    string customKey = $"{customKeyPrefix}{keyName}";
                    custom[customKey] = valueObj ?? nullObjPresentation;
                }
            }

            if (e.InnerException != null)
            {
                SnapExceptionDataAsCustomData(e.InnerException, ref custom);
            }

            // there could be more Data to capture in case of an AggregateException:
            AggregateException? aggregateException = e as AggregateException;
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
