namespace Rollbar.Common
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Class ExceptionUtility.
    /// </summary>
    public static class ExceptionUtility
    {
        /// <summary>
        /// Snaps the local variables.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>System.String.</returns>
        public static string SnapLocalVariables(Exception exception)
        {
            var stackTrace = new StackTrace(exception, true);
            return SnapLocalVariables(stackTrace);
        }

        /// <summary>
        /// Snaps the local variables.
        /// </summary>
        /// <param name="stackTrace">The stack trace.</param>
        /// <returns>System.String.</returns>
        public static string SnapLocalVariables(StackTrace stackTrace)
        {
            StackFrame?[] stackFrames = stackTrace.GetFrames();
            if(stackFrames == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new();
            foreach (StackFrame? frame in stackFrames)
            {
                if(frame == null)
                {
                    continue;
                }

                MethodInfo? method = frame.GetMethod() as MethodInfo;
                if(method != null)
                {
                    sb.AppendLine("----------------");
                    sb.AppendLine($"Method: {method.Name}");
                    sb.AppendLine(" Parameters:");
                    ParameterInfo[] pis = method.GetParameters();
                    foreach(ParameterInfo pi in pis)
                    {
                        sb.AppendLine($" Name:{pi.Name} Type:{pi.ParameterType}");
                    }
                    sb.AppendLine(" Local Variables:");

                    MethodBody? method_body = method.GetMethodBody();
                    if(method_body != null)
                    {
                        foreach(LocalVariableInfo lvi in method_body.LocalVariables)
                        {
                            sb.AppendLine($" Index:{lvi.LocalIndex} Type:{lvi.LocalType}");
                        }
                    }
                }
            }
            sb.AppendLine("----------------");

            string result = sb.ToString();

            return result;
        }

    }
}
