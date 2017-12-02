namespace Rollbar.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A simple Tracer utility class.
    /// </summary>
    public static class Tracer
    {
        /// <summary>
        /// Initializes the <see cref="Tracer"/> class.
        /// </summary>
        static Tracer()
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.AutoFlush = true;
        }

        /// <summary>
        /// Traces the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="sourceLineNumber">The source line number.</param>
        public static void TraceMessage(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0
            )
        {
            Trace.WriteLine("Message: " + message);
            Trace.Indent();
            Trace.WriteLine("member name: " + memberName);
            Trace.WriteLine("source file path: " + sourceFilePath);
            Trace.WriteLine("source line number: " + sourceLineNumber);
            Trace.Unindent();
        }
    }
}
