using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;
using Rollbar.PlugIns.MSEnterpriseLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        public static readonly ExceptionManager ExceptionManager;
        public const string ExceptionShieldingPolicyID = "ExceptionShieldingPolicy";

        static Program()
        {
            ExceptionManager = BuildExceptionManager();
        }

        static void Main(string[] args)
        {
            SalaryCalculator calc = new SalaryCalculator();
            var result = Program.ExceptionManager.Process(() =>
                           calc.GetWeeklySalary("jsmith", 0), Program.ExceptionShieldingPolicyID);
            Console.WriteLine("Result is: {0}", result);
        }

        private static ExceptionManager BuildExceptionManager()
        {
            // The following code sample shows how to define a policy named 
            // according to the ExceptionShieldingPolicyID programmatically.
            // This policy handles the three exception types—FormatException, 
            // Exception, and InvalidCastException—and contains a mix of handlers 
            // for each exception type.
            // You must configure a LogWriter instance before you add a Logging 
            // exception handler to your exception handling.

            // Listeners
            var flatFileTraceListener = new FlatFileTraceListener(fileName: "SalaryCalculator.log");

            // Build Configuration
            var config = new LoggingConfiguration();
            config.AddLogSource("General", SourceLevels.All, true);
            config.LogSources["General"].AddTraceListener(flatFileTraceListener);

            LogWriter logWriter = new LogWriter(config);

            var policies = new List<ExceptionPolicyDefinition>();

            const string rollbarAccessToken = "17965fa5041749b6bf7095a190001ded";
            const string rollbarEnvironment = "RollbarNetSamples";

            var exceptionShielding = new List<ExceptionPolicyEntry>
            {
                {
                    new ExceptionPolicyEntry(
                        typeof (Exception),
                        PostHandlingAction.ThrowNewException,
                        new IExceptionHandler[]
                        {
                            new LoggingExceptionHandler(
                                "General", 
                                9000, 
                                TraceEventType.Error,
                                "Salary Calculations Service", 
                                5,
                                typeof(TextExceptionFormatter), 
                                logWriter
                                ),
                            new WrapHandler(
                            "Rolbar Plug-in for MSEntLib ExceptionHandler Sample Application Error. Please contact your administrator.",
                            typeof(Exception)),
                            new RollbarExceptionHandler(
                                rollbarAccessToken,
                                rollbarEnvironment, 
                                TimeSpan.FromSeconds(3)
                                ),
                        }
                    )
                },

            };

            policies.Add(
                new ExceptionPolicyDefinition(Program.ExceptionShieldingPolicyID, exceptionShielding)
                );

            ExceptionManager manager = new ExceptionManager(policies);
            return manager;
        }
    }
}
