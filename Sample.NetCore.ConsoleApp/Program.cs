using Rollbar;
using System;

namespace Sample.NetCore.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            const string postServerItemAccessToken = "17965fa5041749b6bf7095a190001ded";

            RollbarLocator.RollbarInstance
                .Configure(new RollbarConfig(postServerItemAccessToken) { Environment = "proxyTest" })
                .Info("Basic info log example.")
                .Debug("First debug log.")
                .Error(new NullReferenceException())
                .Error(new Exception("trying out the TraceChain", new NullReferenceException()))
                ;
        }
    }
}
