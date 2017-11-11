﻿using Rollbar;
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


            bool check = RollbarLocator.RollbarInstance is IDisposable;

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
            //CommunicationEventArgs commEvent = e as CommunicationEventArgs;
            //if (commEvent != null)
            //{
            //    Console.WriteLine(commEvent.Trace());
            //    return;
            //}
            //CommunicationErrorEventArgs commErrorEvent = e as CommunicationErrorEventArgs;
            //if (commErrorEvent != null)
            //{
            //    Console.WriteLine(commErrorEvent.Trace());
            //    return;
            //}

            Console.WriteLine(e.TraceAsString());
        }
    }
}
