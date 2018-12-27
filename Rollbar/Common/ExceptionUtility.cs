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
            var exceptionStackTrace = new StackTrace(exception.InnerException, true);
            var stackTrace = new StackTrace(true);
            return SnapLocalVariables(stackTrace);
        }

        /// <summary>
        /// Snaps the local variables.
        /// </summary>
        /// <param name="stackTrace">The stack trace.</param>
        /// <returns>System.String.</returns>
        public static string SnapLocalVariables(StackTrace stackTrace)
        {
            StackFrame[] stackFrames = stackTrace.GetFrames();
            StringBuilder sb = new StringBuilder();
            foreach (StackFrame frame in stackFrames)
            {
                MethodInfo method = frame.GetMethod() as MethodInfo;
                sb.AppendLine("----------------");
                sb.AppendLine($"Method: {method.Name}");
                sb.AppendLine(" Parameters:");
                ParameterInfo[] pis = method.GetParameters();
                foreach (ParameterInfo pi in pis)
                {
                    sb.AppendLine($" Name:{pi.Name} Type:{pi.ParameterType.ToString()}");
                }
                sb.AppendLine(" Local Variables:");

                MethodBody method_body = method.GetMethodBody();
                if (method_body != null)
                {
                    foreach (LocalVariableInfo lvi in method_body.LocalVariables)
                    {
                        sb.AppendLine($" Index:{lvi.LocalIndex} Type:{lvi.LocalType.ToString()}");
                    }
                }
            }
            sb.AppendLine("----------------");

            string result = sb.ToString();

            return result;
        }

    }
}
