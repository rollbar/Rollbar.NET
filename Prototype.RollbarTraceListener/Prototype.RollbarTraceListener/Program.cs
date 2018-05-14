using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Prototype.RollbarTraceListener
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace.WriteLine("Tracing something...");
            Trace.TraceError(new Exception().ToString());

            Console.ReadLine();
        }
    }
}
