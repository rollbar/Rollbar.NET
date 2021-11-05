
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "<Pending>", Scope = "member", Target = "~M:Rollbar.AspNetCore.RollbarLogger.BeginScope``1(``0)~System.IDisposable")]
[assembly: SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "<Pending>", Scope = "member", Target = "~M:Rollbar.AspNetCore.RollbarLogger.Log``1(Microsoft.Extensions.Logging.LogLevel,Microsoft.Extensions.Logging.EventId,``0,System.Exception,System.Func{``0,System.Exception,System.String})")]
[assembly: SuppressMessage("Potential Code Quality Issues", "RECS0021:Warns about calls to virtual member functions occuring in the constructor", Justification = "<Pending>", Scope = "member", Target = "~M:Rollbar.DTOs.Telemetry.#ctor(Rollbar.DTOs.TelemetrySource,Rollbar.DTOs.TelemetryLevel,Rollbar.DTOs.TelemetryBody,System.Collections.Generic.IDictionary{System.String,System.Object})")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>", Scope = "type", Target = "~T:Rollbar.DTOs.Telemetry.ReservedProperties")]
[assembly: SuppressMessage("Usage", "CA2217:Do not mark enums with FlagsAttribute", Justification = "<Pending>", Scope = "type", Target = "~T:Rollbar.Telemetry.TelemetryAutoCollectionSettings")]
[assembly: SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = "<Pending>", Scope = "member", Target = "~M:Rollbar.RollbarBlazorClient.ScrubHttpMessageBodyContentString(System.String,System.String,System.String,System.String[],System.String[])~System.String")]
[assembly: SuppressMessage("Major Code Smell", "S3267:Loops should be simplified with \"LINQ\" expressions", Justification = "<Pending>", Scope = "member", Target = "~M:Rollbar.Classification.Classification.GenerateID(System.Collections.Generic.IEnumerable{Rollbar.Classification.Classifier})~System.String")]
