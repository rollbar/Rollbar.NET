namespace Rollbar.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;
    using Rollbar.Diagnostics;

    /// <summary>
    /// Models Rollbar Frame DTO.
    /// </summary>
    /// <seealso cref="Rollbar.DTOs.DtoBase" />
    public class Frame
        : DtoBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Frame"/> class.
        /// </summary>
        public Frame()
        {
            this.FileName = @"(unknown)";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Frame" /> class.
        /// </summary>
        /// <param name="frameString">The frame string.</param>
        public Frame(string? frameString)
        {
            if (string.IsNullOrWhiteSpace(frameString))
            {
                return;
            }

            string token = @"at ";
            int tokenIndex = frameString!.IndexOf(token, StringComparison.InvariantCulture);
            frameString = frameString.Remove(tokenIndex, token.Length);
            frameString = frameString.Trim();
            string[] components = frameString.Split(new [] { " in ", }, StringSplitOptions.None);
            if (components.Length > 0)
            {
                this.Method = components[0];
            }
            if (components.Length > 1)
            {
                components = components[1].Split(new [] { ":line ", }, StringSplitOptions.None);
                if (components.Length > 0)
                {
                    this.FileName = components[0];
                }
                if (components.Length > 1 
                    && int.TryParse(components[1], out int lineNumber)
                    )
                {
                    this.LineNo = lineNumber;
                }
            }

            if (string.IsNullOrWhiteSpace(this.FileName))
            {
                this.FileName = @"(unknown)";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Frame"/> class.
        /// </summary>
        /// <param name="frame">The frame.</param>
        public Frame(StackFrame? frame)
        {
            if (frame == null)
            {
                return;
            }

            Method = GetMethod(frame);
            FileName = GetFileName(frame);
            LineNo = GetLineNumber(frame);
            ColNo = LineNo.HasValue ? GetFileColumnNumber(frame) : null;
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        [JsonProperty("filename", Required = Required.Always)]
        public string? FileName { get; private set; }

        /// <summary>
        /// Gets or sets the line number.
        /// </summary>
        /// <value>
        /// The line number.
        /// </value>
        [JsonProperty("lineno", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? LineNo { get; set; }

        /// <summary>
        /// Gets or sets the column number.
        /// </summary>
        /// <value>
        /// The column number.
        /// </value>
        [JsonProperty("colno", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? ColNo { get; set; }

        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        [JsonProperty("method", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Method { get; set; }

        #region Unautomatable

        // These properties cannot be automated w/ normal C# StackFrames.
        // You may be able to fill this out from another .NET language.
        // They're there in case you're awesome.

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        [JsonProperty("code", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Code { get; set; }

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        [JsonProperty("context", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CodeContext? Context { get; set; }

        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        /// <value>
        /// The arguments.
        /// </value>
        [JsonProperty("args", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[]? Args { get; set; }

        /// <summary>
        /// Gets or sets the kwargs.
        /// </summary>
        /// <value>
        /// The kwargs.
        /// </value>
        [JsonProperty("kwargs", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IDictionary<string, object>? Kwargs { get; set; }

        #endregion Unautomatable

        private const string defaultFileName = @"(unknown)";

        private static string GetFileName(StackFrame frame)
        {
            Assumption.AssertNotNull(frame, nameof(frame));

            string? returnVal = defaultFileName;

            if (frame == null)
            {
                return returnVal;
            }

            returnVal = frame.GetFileName();
            if (!string.IsNullOrWhiteSpace(returnVal))
            {
                return returnVal;
            }

            MethodBase? method = frame.GetMethod();
            if (method != null && method.ReflectedType != null)
            {
                returnVal = method.ReflectedType.FullName ;
            }
            if (!string.IsNullOrWhiteSpace(returnVal))
            {
                return returnVal;
            }

            return defaultFileName;
        }

        private static int? GetLineNumber(StackFrame frame)
        {
            Assumption.AssertNotNull(frame, nameof(frame));

            var lineNo = frame.GetFileLineNumber();
            if (lineNo != 0)
            {
                return lineNo;
            }

            lineNo = frame.GetILOffset();
            if (lineNo != -1)
            {
                return lineNo;
            }

            lineNo = frame.GetNativeOffset();
            return lineNo == -1 ? (int?)null : lineNo;
        }

        private static int? GetFileColumnNumber(StackFrame frame)
        {
            Assumption.AssertNotNull(frame, nameof(frame));

            return frame.GetFileColumnNumber();
        }

        private static string GetMethod(StackFrame frame)
        {
            Assumption.AssertNotNull(frame, nameof(frame));

            MethodBase? method = frame.GetMethod();
            if (method == null)
            {
                return frame.ToString();
            }

            var methodName = method.Name;
            if (method.ReflectedType != null)
            {
                methodName = string.Format("{0}.{1}", method.ReflectedType.FullName, methodName);
            }

            var parameters = method.GetParameters();

            if (parameters.Length > 0)
            {
                return string.Format("{0}({1})", methodName, string.Join(", ", parameters.Select(p => string.Format("{0} {1}", p.ParameterType, p.Name))));
            }

            return string.Format("{0}()", methodName);
        }
    }
}
