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
            var traceListeners = Trace.Listeners;
            Trace.WriteLine("Tracing something...");
            Trace.TraceError(new Exception("Some EXCEPTION").ToString());
            Trace.TraceWarning("WARNING!");
            Trace.TraceInformation("INFO...");
            Trace.Fail("FAIL!!!");
            Trace.Assert(false, "ASSERT");

            Console.ReadLine();
        }
    }
}
