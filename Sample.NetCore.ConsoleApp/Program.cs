using Rollbar;
using Rollbar.DTOs;
using System;

namespace Sample.NetCore.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            RollbarQueueController.Instance.InternalEvent += OnRollbarInternalEvent;

            const string postServerItemAccessToken = "17965fa5041749b6bf7095a190001ded";

            RollbarLocator.RollbarInstance
                .Configure(new RollbarConfig(postServerItemAccessToken) { Environment = "RollbarNetSamples" })
                .InternalEvent += OnRollbarInternalEvent
                ;

            SetRollbarReportingUser("007", "jbond@mi6.uk", "JBOND");

            RollbarLocator.RollbarInstance
                .Info("ConsoleApp sample: Basic info log example.")
                .Debug("ConsoleApp sample: First debug log.")
                .Error(new NullReferenceException("ConsoleApp sample: null reference exception."))
                .Error(new System.Exception("ConsoleApp sample: trying out the TraceChain", new NullReferenceException()))
                ;

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10));

        }

        private static void SetRollbarReportingUser(string id, string email, string userName)
        {
            Person person = new Person(id);
            person.Email = email;
            person.UserName = userName;
            RollbarLocator.RollbarInstance.Config.Person = person;
        }

        private static void OnRollbarInternalEvent(object sender, RollbarEventArgs e)
        {
            Console.WriteLine(e.TraceAsString());

            RollbarApiErrorEventArgs apiErrorEvent = e as RollbarApiErrorEventArgs;
            if (apiErrorEvent != null)
            {
                //TODO: handle/report Rollbar API communication error event...
                return;
            }
            CommunicationEventArgs commEvent = e as CommunicationEventArgs;
            if (commEvent != null)
            {
                //TODO: handle/report Rollbar API communication event...
                return;
            }
            CommunicationErrorEventArgs commErrorEvent = e as CommunicationErrorEventArgs;
            if (commErrorEvent != null)
            {
                //TODO: handle/report basic communication error while attempting to reach Rollbar API service... 
                return;
            }
            InternalErrorEventArgs internalErrorEvent = e as InternalErrorEventArgs;
            if (internalErrorEvent != null)
            {
                //TODO: handle/report basic internal error while using the Rollbar Notifier... 
                return;
            }
        }
    }
}
