namespace UnitTest.RollbarTestCommon
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;

    using global::Rollbar;

    using UnitTest.Rollbar;

    public static class RollbarUnitTestEnvironmentUtil
    {
        public static RollbarDestinationOptions GetLiveTestRollbarDestinationOptions()
        {
            var options =
                new RollbarDestinationOptions(
                    RollbarUnitTestSettings.AccessToken, 
                    RollbarUnitTestSettings.Environment
                    );

            return options;
        }

        public static RollbarInfrastructureConfig GetLiveTestRollbarInfrastructureConfig()
        {
            var config = 
                new RollbarInfrastructureConfig();

            var destinationOptions =
                RollbarUnitTestEnvironmentUtil.GetLiveTestRollbarDestinationOptions();
            config
                .RollbarLoggerConfig
                .RollbarDestinationOptions
                .Reconfigure(destinationOptions);

            var infrastructureOptions = new RollbarInfrastructureOptions();
            infrastructureOptions.PayloadPostTimeout = TimeSpan.FromSeconds(3);
            config
                .RollbarInfrastructureOptions
                .Reconfigure(infrastructureOptions);

            return config;
        }


        public static void SetupLiveTestRollbarInfrastructure()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            var config = 
                RollbarUnitTestEnvironmentUtil.GetLiveTestRollbarInfrastructureConfig();

            if(!RollbarInfrastructure.Instance.IsInitialized)
            {
                RollbarInfrastructure.Instance.Init(config);
            }
            else
            {
                RollbarInfrastructure.Instance.Config.Reconfigure(config);
            }

            RollbarUnitTestEnvironmentUtil.TraceCurrentRollbarInfrastructureConfig();
        }

        public static void TraceCurrentRollbarInfrastructureConfig()
        {
            RollbarUnitTestEnvironmentUtil.Trace(
                RollbarInfrastructure.Instance.Config, 
                "ROLLBAR_INFRASTRUCTURE_CONFIG:"
                );
        }

        public static void Trace(object content, string title = null)
        {
            string contentAsString;

            ITraceable tracable = content as ITraceable;
            if(tracable != null)
            {
                contentAsString = tracable.TraceAsString();
            }
            else if(content != null)
            {
                contentAsString = content.ToString();
            }
            else
            {
                contentAsString = "<null>";
            }

            RollbarUnitTestEnvironmentUtil.Trace(contentAsString, title);
        }

        public static void Trace(string content, string title = null)
        {
            Debug.WriteLine(RollbarUnitTestEnvironmentUtil.Decorate(content, title));
            //Debug.WriteLine(string.Empty);
        }
        private static string Decorate(string content, string title = null)
        {
            if(title == null)
            {
                return content; // no title - no decoration!
            }

            StringBuilder builder = new StringBuilder(Environment.NewLine);
            builder.AppendLine(title);
            builder.AppendLine(RollbarUnitTestEnvironmentUtil.TraceSeparator);
            builder.AppendLine(content);
            builder.AppendLine(RollbarUnitTestEnvironmentUtil.TraceSeparator);

            return builder.ToString();
        }

        private readonly static string TraceSeparator = "==============================================";
    }
}
