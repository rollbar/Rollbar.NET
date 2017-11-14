using Rollbar;
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
                .Configure(new RollbarConfig(postServerItemAccessToken) { Environment = "proxyTest" })
                .InternalEvent += OnRollbarInternalEvent
                ;

            RollbarLocator.RollbarInstance
                .Info("Basic info log example.")
                .Debug("First debug log.")
                .Error(new NullReferenceException())
                .Error(new Exception("trying out the TraceChain", new NullReferenceException()))
                ;

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10));
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
        }
    }
}
