# Rollbar.NET Notifier SDK

A .NET Rollbar Client/Notifier that can be used in any application built on the following .NET versions: .NET Core 2.0+, .NET Standard 2.0+, .NET Full Framework 4.5+, Mono, Xamarin, and, in general, any implementation of the .NET Standard 2.0+.
It simplifies building data payloads based on exception data, tracing data, informational messages and telemetry data and sends the payloads to the Rollbar API for remote monitoring and analysis of the hosting application's behavior.'

It also includes a collection of Rollbar.NET plug-ins into most popular .NET logging and exception handling libraries/frameworks, like:
*   Serilog
*   log4net
*   NLog
*   Microsoft Enterprise Library's Exception Handling block
*   etc.
*   as well as RollbarTraceListener and ASP.NET Core Rollbar middleware. 

These plug-ins simplify integration of the Rollbar.NET Notifier into codebases that are already using any of these libraries/frameworks using the libraries' native extensions mechanisms.

## CI builds status

### [Rollbar.NET](https://github.com/rollbar/Rollbar.NET) repo
[![Build Status](https://dev.azure.com/rollbar/Rollbar.NET/_apis/build/status/rollbar.Rollbar.NET?branchName=master)](https://dev.azure.com/rollbar/Rollbar.NET/_build/latest?definitionId=1&branchName=master)

### [Rollbar.NET @ Wide Spectrum Computing](https://github.com/WideSpectrumComputing/Rollbar.NET) repo (primary development fork)
[![Build Status](https://dev.azure.com/wsc0610/wsc/_apis/build/status/WideSpectrumComputing.Rollbar.NET?branchName=master)](https://dev.azure.com/wsc0610/wsc/_build/latest?definitionId=1?branchName=master)

## Code quality status

### [Rollbar.NET](https://github.com/rollbar/Rollbar.NET) repo

### [Rollbar.NET @ Wide Spectrum Computing](https://github.com/WideSpectrumComputing/Rollbar.NET) repo (primary development fork)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/9adf26abd5694ebb8fecf6d4925f1da7)](https://www.codacy.com/app/WideSpectrumComputing/WideSpectrumComputing-Rollbar.NET?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=WideSpectrumComputing/Rollbar.NET&amp;utm_campaign=Badge_Grade)

## Install

Using Nuget Package Manager:

    ```Install-Package Rollbar```

## Blocking vs Non-Blocking Use

The SDK is designed to have as little impact on the hosting system or application as possible. It takes an async "fire and forget" approach to logging. Normally, you want to use fully asynchronous logging, since it has virtually no instrumentational overhead on your application execution performance at runtime (especially when running on a multi-core/multi-processor system).

In v1.x.x versions of the SDK, the asynchronous logging calls were still performing some of their data processing functions (like packaging the data objects to log into a proper Rollbar Payload DTO instance) on the calling thread before asynchronously placing the payloads into a transmission queue. Hence, the duration of the logging method calls was proportional to the size and complexity of the data object to package and log.  

In v2.x.x versions of the SDK, we moved the packaging of the data-to-log one level deeper and now it is handled in a context of a worker thread that is responsible for packaging of a proper payload DTO and queuing it for transmission to the Rollbar API Server.
As the result, the logging method calls are extremely quick now (under 20 microseconds) regardless of complexity and size of the data-to-log. All the methods now return a `Task` instance (instead of an `ILogger` instance as in v1.x.x) that could be either ignored in true "fire-and-forget" logging scenarios or could be waited (or awaited) to complete packaging and queuing of the payloads in some scenarios.

However, in some specific situations (such as while logging right before exiting an application), you may want to use a logger fully synchronously so that the application does not quit before the logging completes (including subsequent delivery of the corresponding payload to the Rollbar API).

That is why every instance of the Rollbar asynchronous logger (implementing `IAsyncLogger` interface) defines the `AsBlockingLogger(TimeSpan timeout)` method that returns a fully synchronous implementation of the `ILogger` interface that defines signatures of its logging methods that are very similar to the `IAsyncLogger`'s logging methods but returning the same `ILogger` instance instead os a `Task`. This approach allows for easier code refactoring when switching between asynchronous and synchronous uses of the logger.

Therefore, this call will perform the quickest possible asynchronous logging (true "fire-and-forget" logging):

```csharp
RollbarLocator.RollbarInstance.Log(ErrorLevel.Error, "test message");
```

while the following call will perform somewhat quick asynchronous logging (only having its payload packaging and queuing complete by the end of the call):

```csharp
RollbarLocator.RollbarInstance.Log(ErrorLevel.Error, "test message").Wait();
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

More detailed information about Rollbar.NET usage and API reference are available at [https://docs.rollbar.com/docs/dotnet](https://docs.rollbar.com/docs/dotnet)

## Where you can find the SDK performance benchmarking results

The benchmarking results are stored within the [Rollbar.Benchmarks folder](https://github.com/rollbar/Rollbar.NET/tree/master/Rollbar.Benchmarks) in a subfolder per an SDK release.
You,probably, want to start by reviewing the result/Rollbar.Benchmarker.RollbarLoggerBenchmark-report.html file within each of the subfolders and, then, to drill down for more details within the rest of the
BenchmarkDotNet generated artifacts.

## Help / Support

If you run into any issues, please email us at [support@rollbar.com](mailto:support@rollbar.com)

For bug reports, please [open an issue on GitHub](https://github.com/rollbar/Rollbar.NET/issues/new).

## Contributing

1.  [Fork it](https://github.com/rollbar/Rollbar.NET)
2.  Create your feature branch (```git checkout -b my-new-feature```).
3.  Commit your changes (```git commit -am 'Added some feature'```)
4.  Push to the branch (```git push origin my-new-feature```)
5.  Create new Pull Request
