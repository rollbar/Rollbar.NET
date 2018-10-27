# Rollbar.NET

A .NET Rollbar Client that can be used in any application built on the following .NET versions: .NET Core 2.0+, .NET Standard 2.0+, and .NET Full Framework 4.5+.

## Install

Nuget Package Manager:

    Install-Package Rollbar

## Blocking vs Non-Blocking Use

The SDK is designed to have as little impact on the hosting system or application as possible. Normally, you want to use asynchronous logging, since it has virtually no instrumentational overhead on your application execution performance at runtime. It has a "fire and forget" approach to logging. 

However, in some specific situations (such as while logging right before exiting an application), you may want to use it synchronously so that the application does not quit before the logging completes.

That is why all the logging methods of the `ILogger` interface imply asynchronous/non-blocking implementation. However, the `ILogger` interface defines the `AsBlockingLogger(TimeSpan timeout)` method that returns a synchronous implementation of the same `ILogger`. This approach allows for easier code refactoring when switching between asynchronous and synchronous uses of the logger.

Therefore, the following call will perform async logging:

```csharp
RollbarLocator.RollbarInstance.Log(ErrorLevel.Error, "test message");
```

While this call will perform blocking/synchronous logging with a timeout of 1 second:

```csharp
RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(5)).Log(ErrorLevel.Error, "test message");
```

In case of a timeout, all the blocking log methods throw `System.TimeoutException` instead of gracefully completing the call. Therefore you might want to make all the blocking log calls within a try-catch block while catching `System.TimeoutException` specifically to handle a timeout case.

## Basic Usage

* Configure Rollbar with `RollbarLocator.RollbarInstance.Configure(new RollbarConfig("POST_SERVER_ITEM_ACCESS_TOKEN"))`
* Send errors (asynchronously) to Rollbar with `RollbarLocator.RollbarInstance.Error(Exception)`
* Send messages (synchronously) to Rollbar with `RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(5)).Info(string)`

## Upgrading to v1.0.0+ from earlier versions

In order to upgrade to v1.0.0+ from an earlier version, you should change your config from the old version

```csharp
Rollbar.Init(new RollbarConfig("POST_SERVER_ITEM_ACCESS_TOKEN"));
```

to 

```csharp
const string postServerItemAccessToken = "POST_SERVER_ITEM_ACCESS_TOKEN";
RollbarLocator.RollbarInstance
.Configure(new RollbarConfig(postServerItemAccessToken) { Environment = "proxyTest" }) ;
```

Additionally, anywhere in your code that you were sending error reports via `Rollbar.Report(Exception)` or `Rollbar.Report(string)` will need to be replaced with either something like `RollbarLocator.RollbarInstance.Error(new Exception("trying out the TraceChain", new NullReferenceException()))` or `RollbarLocator.RollbarInstance.Info("Basic info log example.")`.

## More Information about the SDK

More detailed information about Rollbar.NET usage and API reference are available at [https://docs.rollbar.com/docs/dotnet](https://docs.rollbar.com/docs/dotnet)


## Where can you find the SDK performance benchmarking results?

The benchmarking results are stored within the [Rollbar.Benchmarks folder](https://github.com/rollbar/Rollbar.NET/tree/master/Rollbar.Benchmarks) in a subfolder per an SDK release.
You,probably, want to start by reviewing the result/Rollbar.Benchmarker.RollbarLoggerBenchmark-report.html file within each of the subfolders and, then, to drill down for more details within the rest of the
BenchmarkDotNet generated artifacts.

## Help / Support

If you run into any issues, please email us at [support@rollbar.com](mailto:support@rollbar.com)

For bug reports, please [open an issue on GitHub](https://github.com/rollbar/Rollbar.NET/issues/new).


## Contributing

1. [Fork it](https://github.com/rollbar/Rollbar.NET)
2. Create your feature branch (```git checkout -b my-new-feature```).
3. Commit your changes (```git commit -am 'Added some feature'```)
4. Push to the branch (```git push origin my-new-feature```)
5. Create new Pull Request
