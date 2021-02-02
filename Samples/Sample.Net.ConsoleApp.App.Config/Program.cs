using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rollbar;

namespace Sample.Net.ConsoleApp.App.Config
{
    class Program
    {
        static void Main(string[] args)
        {
            // The Rollbar Notifier is configured within the app.config.
            // Let's just start using it:
            RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(3))
                .Info($"{typeof(Program).Namespace} sample: Rollbar is alive based on the app.config settings!.");
        }
    }
}
