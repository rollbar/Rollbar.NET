namespace Sample.RollbarTraceListener
{
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DoSomthing();
            }
            catch(Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }

            Console.ReadLine();
        }

        static void DoSomthing()
        {
            var traceListeners = Trace.Listeners;
            Trace.WriteLine("Tracing something...");
            Trace.TraceError(new Exception("Some EXCEPTION").ToString());
            Trace.TraceWarning("WARNING!");
            Trace.TraceInformation("INFO...");
            Trace.Fail("FAIL!!!");
            Trace.Assert(false, "ASSERT");

            throw new ApplicationException("Oy way!");
        }
    }
}
