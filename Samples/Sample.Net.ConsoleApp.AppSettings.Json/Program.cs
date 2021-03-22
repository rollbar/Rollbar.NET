using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rollbar;

namespace Sample.Net.ConsoleApp.AppSettings.Json
{
    class Program
    {
        static void Main(string[] args)
        {
            RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(3))
                .Info($"{typeof(Program).Namespace} sample: Rollbar Notifier is alive based on appsetting.json!");
        }
    }
}
