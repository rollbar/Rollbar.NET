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
                .LogInfo("Basic info log example.")
                .LogDebug("First debug log.")
                .LogError(new NullReferenceException())
                ;
        }
    }
}
