using RollbarDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            const string postServerItemAccessToken = "17965fa5041749b6bf7095a190001ded";
            Rollbar.Init(new RollbarConfig(postServerItemAccessToken) { Environment = "proxyTest"});
            Rollbar.Report("Solving proxy issues...");
        }
    }
}
