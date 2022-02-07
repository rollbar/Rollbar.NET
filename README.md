![Rollbar Logo](https://github.com/rollbar/rollbar.net/blob/master/rollbar-logo.png)

# Rollbar.NET Notifier SDK

A .NET Rollbar Client/Notifier SDK that can be used in any application built on the following .NET versions: .NET Core 2.0+, .NET Standard 2.0+, .NET Full Framework 4.5+, Mono, Xamarin, and, in general, any implementation of the .NET Standard 2.0+.
It simplifies building data payloads based on exception data, tracing data, informational messages and telemetry data and sends the payloads to the [Rollbar API](https://www.rollbar.com) for remote monitoring and analysis of the hosting application's behavior and health.

It also includes a collection of adapters and helpers for many .NET application frameworks as well as a collection of Rollbar.NET plug-ins into most popular .NET logging and exception handling libraries/frameworks, like:
*   Serilog
*   log4net
*   NLog
*   Microsoft Enterprise Library's Exception Handling block
*   etc.
*   as well as RollbarTraceListener and ASP.NET Core Rollbar middleware. 

These plug-ins simplify integration of the Rollbar.NET Notifier into codebases that are already using any of these libraries/frameworks using the libraries' native extensions mechanisms.

## NEW MAJOR RELEASE (v5) ANNOUNCEMENT

A new major release v5 of the SDK is available.
The primary new feature of this release is the support of Blazor Client/Webassembly/WASM.

For detailed instructions see our [v5 specific documentation](https://docs.rollbar.com/docs/net-v5).

## Codebase status (code quality and CI build)

![CI workflow](https://github.com/rollbar/Rollbar.NET/workflows/CI%20workflow/badge.svg)

[![Build Status](https://dev.azure.com/rollbar-account/SDKs/_apis/build/status/rollbar.Rollbar.NET?branchName=master)](https://dev.azure.com/rollbar-account/SDKs/_build/latest?definitionId=1&branchName=master)

[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=rollbar-net&metric=security_rating)](https://sonarcloud.io/dashboard?id=rollbar-net)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=rollbar-net&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=rollbar-net)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=rollbar-net&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=rollbar-net)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=rollbar-net&metric=vulnerabilities)](https://sonarcloud.io/dashboard?id=rollbar-net)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=rollbar-net&metric=coverage)](https://sonarcloud.io/dashboard?id=rollbar-net)

## Available as NuGet packages
- `Rollbar (the core)....................`
[![NuGet version](http://img.shields.io/nuget/v/Rollbar.svg)](http://www.nuget.org/packages/Rollbar/) 
[![Nuget downloads](http://img.shields.io/nuget/dt/Rollbar.svg)](http://www.nuget.org/packages/Rollbar/)
- `Rollbar.Deploys.......................`
[![NuGet version](http://img.shields.io/nuget/v/Rollbar.svg)](http://www.nuget.org/packages/Rollbar.Deploys/) 
[![Nuget downloads](http://img.shields.io/nuget/dt/Rollbar.Deploys.svg)](http://www.nuget.org/packages/Rollbar.Deploys/)
- `Rollbar.NetPlatformExtensions.........`
[![NuGet version](http://img.shields.io/nuget/v/Rollbar.NetPlatformExtensions.svg)](http://www.nuget.org/packages/Rollbar.NetPlatformExtensions/) 
[![Nuget downloads](http://img.shields.io/nuget/dt/Rollbar.NetPlatformExtensions.svg)](http://www.nuget.org/packages/Rollbar.NetPlatformExtensions/)
- `Rollbar.NetCore.AspNet................`
[![NuGet version](http://img.shields.io/nuget/v/Rollbar.NetCore.AspNet.svg)](http://www.nuget.org/packages/Rollbar.NetCore.AspNet/) 
[![Nuget downloads](http://img.shields.io/nuget/dt/Rollbar.NetCore.AspNet.svg)](http://www.nuget.org/packages/Rollbar.NetCore.AspNet/)
- `Rollbar.Net.AspNet....................`
[![NuGet version](http://img.shields.io/nuget/v/Rollbar.Net.AspNet.svg)](http://www.nuget.org/packages/Rollbar.Net.AspNet/) 
[![Nuget downloads](http://img.shields.io/nuget/dt/Rollbar.Net.AspNet.svg)](http://www.nuget.org/packages/Rollbar.Net.AspNet/)
- `Rollbar.Net.AspNet.Mvc................`
[![NuGet version](http://img.shields.io/nuget/v/Rollbar.Net.AspNet.Mvc.svg)](http://www.nuget.org/packages/Rollbar.Net.AspNet.Mvc/) 
[![Nuget downloads](http://img.shields.io/nuget/dt/Rollbar.Net.AspNet.Mvc.svg)](http://www.nuget.org/packages/Rollbar.Net.AspNet.Mvc/)
- `Rollbar.Net.AspNet.WebApi.............`
[![NuGet version](http://img.shields.io/nuget/v/Rollbar.Net.AspNet.WebApi.svg)](http://www.nuget.org/packages/Rollbar.Net.AspNet.WebApi/) 
[![Nuget downloads](http://img.shields.io/nuget/dt/Rollbar.Net.AspNet.WebApi.svg)](http://www.nuget.org/packages/Rollbar.Net.AspNet.WebApi/)
- `Rollbar.PlugIns.Log4net...............`
[![NuGet version](http://img.shields.io/nuget/v/Rollbar.PlugIns.Log4net.svg)](http://www.nuget.org/packages/Rollbar.PlugIns.Log4net/) 
[![Nuget downloads](http://img.shields.io/nuget/dt/Rollbar.PlugIns.Log4net.svg)](http://www.nuget.org/packages/Rollbar.PlugIns.Log4net/)
- `Rollbar.PlugIns.MSEnterpriseLibrary...`
[![NuGet version](http://img.shields.io/nuget/v/Rollbar.PlugIns.MSEnterpriseLibrary.svg)](http://www.nuget.org/packages/Rollbar.PlugIns.MSEnterpriseLibrary/) 
[![Nuget downloads](http://img.shields.io/nuget/dt/Rollbar.PlugIns.MSEnterpriseLibrary.svg)](http://www.nuget.org/packages/Rollbar.PlugIns.MSEnterpriseLibrary/)
- `Rollbar.PlugIns.NLog..................`
[![NuGet version](http://img.shields.io/nuget/v/Rollbar.PlugIns.NLog.svg)](http://www.nuget.org/packages/Rollbar.PlugIns.NLog/) 
[![Nuget downloads](http://img.shields.io/nuget/dt/Rollbar.PlugIns.NLog.svg)](http://www.nuget.org/packages/Rollbar.PlugIns.NLog/)
- `Rollbar.PlugIns.Serilog...............`
[![NuGet version](http://img.shields.io/nuget/v/Rollbar.PlugIns.Serilog.svg)](http://www.nuget.org/packages/Rollbar.PlugIns.Serilog/) 
[![Nuget downloads](http://img.shields.io/nuget/dt/Rollbar.PlugIns.Serilog.svg)](http://www.nuget.org/packages/Rollbar.PlugIns.Serilog/)

## Install

Using [Nuget Package Manager](https://www.nuget.org/packages/Rollbar/):

```
Install-Package Rollbar
```

## Blocking vs Non-Blocking Use

The SDK is designed to have as little impact on the hosting system or application as possible. It takes an async "fire and forget" approach to logging. Normally, you want to use fully asynchronous logging, since it has virtually no instrumentational overhead on your application execution performance at runtime (especially when running on a multi-core/multi-processor system).

In v1.x.x versions of the SDK, the asynchronous logging calls were still performing some of their data processing functions (like packaging the data objects to log into a proper Rollbar Payload DTO instance) on the calling thread before asynchronously placing the payloads into a transmission queue. Hence, the duration of the logging method calls was proportional to the size and complexity of the data object to package and log.  

In v2.x.x versions of the SDK, we moved the packaging of the data-to-log one level deeper and now it is handled in a context of a worker thread that is responsible for packaging of a proper payload DTO and queuing it for transmission to the Rollbar API Server.
As the result, the logging method calls are extremely quick now (under 20 microseconds) regardless of complexity and size of the data-to-log. All the methods now return a `Task` instance (instead of an `ILogger` instance as in v1.x.x) that could be either ignored in true "fire-and-forget" logging scenarios or could be waited (or awaited) to complete packaging and queuing of the payloads in some scenarios.

While it was a nice flexible and easy to use solution from API point of view, the tasks did not perform well (as we learned it the hard way) under EXTREMELY high AND sustained rate of load. 
So, in v3.x.x, we went away from the Tasks and removed `IAsynLogger` all together. We are now back to having only `ILogger` and we have a substitute for the eliminated Tasks in the form of `IRollbarPackage`.
Think of the `IRollbarPackage` as a basis for implementing arbitrary data packaging strategies with explicit flag (named as `MustApplySynchronously`) that signifies need to apply the packaging (steps 1 and 2)
on the calling thread before returning from a logging method. We also provide with abstract base classes like `RollbarPackageBase` and `RollbarPackageDecoratorBase` for implementing custom packaging strategies and their decorators.
We used these abstraction to implement our own collection of packagers and their decorators. All of them are available to the SDK users as well. 
In addition to helping us in getting away from the Tasks usage, these new abstractions allow for very flexible and powerful ways to bundle a lot specific types of data into a single payload as needed 
while encapsulating and reusing the packaging rules of any custom type.  
In v3.x.x, you can either throw into a logging method a data object to log (exactly the way it was in v2) or you can wrap in an `ObjectPackage` while setting the `MustApplySynchronously` flag if you want the logger to behave 
the way IAsyncLogger used to when you had to block-wait on its Task to complete.

However, in some specific situations (such as while logging right before exiting an application), you may want to use a logger fully synchronously so that the application does not quit before the logging completes (including subsequent delivery of the corresponding payload to the Rollbar API).

That is why every instance of the Rollbar logger (implementing `ILogger` interface) defines the `AsBlockingLogger(TimeSpan timeout)` method that returns a fully synchronous implementation of the `ILogger` interface. This approach allows for easier code refactoring when switching between asynchronous and synchronous uses of the logger.

Therefore, this call will perform the quickest possible asynchronous logging (true "fire-and-forget" logging):

```csharp
RollbarLocator.RollbarInstance.Log(ErrorLevel.Error, "test message");
// which is equivalent:
// RollbarLocator.RollbarInstance.Log(ErrorLevel.Error, new ObjectPackage("test message", false));
```

while the following call will perform somewhat quick asynchronous logging (only having its payload packaging and queuing complete by the end of the call):

```csharp
var package = new ObjectPackage("test message", true);
// OR to make it a bit leaner:
// var package = new MessagePackage("test message", true);
RollbarLocator.RollbarInstance.Log(ErrorLevel.Error, package);
```

while next call will perform fully blocking/synchronous logging with a timeout of 5 seconds (including the payload delivery to the Rollbar API either complete or failed due to the timeout by the end of the call):

```csharp
RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(5)).Log(ErrorLevel.Error, "test message");
```

In case of a timeout, all the blocking log methods throw `System.TimeoutException` instead of gracefully completing the call. Therefore you might want to make all the blocking log calls within a try-catch block while catching `System.TimeoutException` specifically to handle a timeout case.

## Basic Usage

*   Configure Rollbar with `RollbarLocator.RollbarInstance.Configure(new RollbarConfig("POST_SERVER_ITEM_ACCESS_TOKEN"))`
*   Send errors (asynchronously) to Rollbar with `RollbarLocator.RollbarInstance.Error(Exception)`
*   Send messages (synchronously) to Rollbar with `RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(5)).Info(string)`

## Upgrading to 4.x.x from v3.x.x versions

The only area of the SDK APIs that changed between v3 and v4 is the one related to the file-based configuration of Rollbar infrastructure.

All the public types related to configuring the SDK based on either app.config or web.config files were moved to their own dedicated module/package `Rollbar.App.Config`. The types namespace changed from `Rollbar.NetFramework` to `Rollbar.App.Config`.
 
All the public types related to configuring the SDK based on appsettings.json file were moved to their own dedicated module/package `Rollbar.AppSettings.Json`. The types namespace changed from `Rollbar.NetCore` to `Rollbar.AppSettings.Json`.

Both new modules are optional alternatives. When either is needed, just reference a corresponding module/package from the application already hosting the Rollbar core module. Assuming you are already using the application config file in your application for other reasons than just configuring Rollbar, all the dependencies needed for accessing the file should be already established by your application codebase.

## Upgrading to v3.x.x from v2.x.x versions

Some Rollbar functionality and API types related to specific more narrow .NET sub-technology
were moved into separate dedicated modules. For example, Rollbar Asp.Net Core middleware moved into 
the Rollbar.NetCore.AspNet module while changing its namespace to follow the module naming.
So, might need to add extra reference to a relevant module and update namespaces when upgrading to v3.

The `IAsyncLogger` is gone. There is only `ILogger`.
So, if you had some code relying on the `IAsyncLogger` where you used to wait on a Task like:

```csharp
RollbarLocator.RollbarInstance.Log(ErrorLevel.Error, "test message").Wait();
```

now, the easiest fix is to rework it into:

```csharp
RollbarLocator.RollbarInstance.Log(ErrorLevel.Error, new ObjectPackage("test message", true));
```

to achieve the same behavior of the logger.

For more details about the v3 specific changes, please, refer to the `ReleaseNotes.md` in the root of this repository.

## Upgrading to v2.x.x from v1.x.x versions

All Rollbar notifier instances of `IRollbar` (that used to be expanding `ILogger` in v1.x.x versions) now implement `IAsyncLogger` instead of `ILogger`. As the result of this change, the rollbar instances lost support of fluent API, hence, they can not be cascaded like so:

```csharp
RollbarLocator.RollbarInstance.Log(ErrorLevel.Error, "error message")
                              .Log(ErrorLevel.Info, "info message")
                              .Log(ErrorLevel.Debug, "debug message");
```

and now have to be reworked as:

```csharp
RollbarLocator.RollbarInstance.Log(ErrorLevel.Error, "error message");
RollbarLocator.RollbarInstance.Log(ErrorLevel.Info, "info message");
RollbarLocator.RollbarInstance.Log(ErrorLevel.Debug, "debug message");
```

while the instance of the blocking Rollbar logger are still capable of supporting the fluent/cascading calls.

We think that convenience of the fluent APIs is less important than an ability to support completely "fire-and-forget" logging calls that could be waited upon if needed.

One more significant change in v2.x.x SDK API that should be fully backward compatible with the v1.x.x SDK API (hence, should not require any client code changes) but is important enough to be mentioned here:

You will notice that the `ILogger` interface was significantly simplified by getting rid of a large amount of logging methods' overloads based on types of the data-to-log (an `Exception` vs a `String` vs an `Object` vs a `Data` DTO, etc). Now, we only have the overloads that take in an `Object` (a good enough reason for backward compatibility of the latest API). The newly introduced `IAsyncLogger` interface defines the same set of methods as `ILogger` with the only difference between the equivalent method signatures - method return type: `IAsyncLogger`'s methods return `Task` while `ILogger`'s methods return `ILogger`.

## Upgrading to v1.x.x from earlier versions

In order to upgrade to v1.0.0+ from an earlier version, you should change your config from the old version

```csharp
Rollbar.Init(new RollbarConfig("POST_SERVER_ITEM_ACCESS_TOKEN"));
```

to

```csharp
    const string postServerItemAccessToken = "POST_SERVER_ITEM_ACCESS_TOKEN";
    RollbarLocator.RollbarInstance.Configure(
        new RollbarConfig(postServerItemAccessToken) { Environment = "proxyTest" }
        ) ;
```

Additionally, anywhere in your code that you were sending error reports via `Rollbar.Report(Exception)` or `Rollbar.Report(string)` will need to be replaced with either something like `RollbarLocator.RollbarInstance.Error(new Exception("trying out the TraceChain", new NullReferenceException()))` or `RollbarLocator.RollbarInstance.Info("Basic info log example.")`.

## Rollbar Plug-ins

We are adding extra projects to the Rollbar.sln solution that simplify integration of Rollbar services with existing popular .NET logging and tracing libraries like Serilog, log4net, NLog, etc.
These plug-in projects are named using following pattern `Rollbar.PlugIns.<LoggingLibraryToIntegrateWith>` and implement similarly named namespaces for the plug-ins.
Each plug-in maintains its own versioning schema and is distributed as its own NuGet package using the naming pattern mentioned above.

## More Information about the SDK

More details about Rollbar.NET usage and API reference are available at [Rollbar.NET SDK Documentation](https://docs.rollbar.com/docs/dotnet).
 v5 specific documentation is available [here](https://docs.rollbar.com/docs/net-v5).

## Help / Support

If you run into any issues, please email us at [support@rollbar.com](mailto:support@rollbar.com)

For bug reports, please [open an issue on GitHub](https://github.com/rollbar/Rollbar.NET/issues/new).

## Contributing

1.  [Fork it](https://github.com/rollbar/Rollbar.NET)
2.  Create your feature branch (```git checkout -b my-new-feature```).
3.  Commit your changes (```git commit -am 'Added some feature'```)
4.  Push to the branch (```git push origin my-new-feature```)
5.  Create new Pull Request
