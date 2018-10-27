# Rollbar.NET Notifier Performance Benchmarking Notes

## Where can you find the SDK performance benchmarking results?

The benchmarking results are stored within the [Rollbar.Benchmarks folder](https://github.com/rollbar/Rollbar.NET/tree/master/Rollbar.Benchmarks) in a subfolder per an SDK release.
You,probably, want to start by reviewing the result/Rollbar.Benchmarker.RollbarLoggerBenchmark-report.html file within each of the subfolders and, then, to drill down for more details within the rest of the
BenchmarkDotNet generated artifacts.

## What exactly do we benchmark?

As of now we benchmark performance of the key Notifier’s Log(...) method, since mostly all of the other logging API are extremely thin convenience wrappers around a few overloads of this method.

At this point we are only benchmarking the async variant of the SDK (i.e. logging methods that are not exposed via the `ILogger.AsBlockingLogger(TimeSpan timeout)` call). The reason is that the
blocking calls have to be always considered as very slow alternative to equivalent async call that should only be used when the client component needs to make shure that a log method either succeeds or 
hopelessly fails before proceeding with the next logical step. Duration of a blocking call depends not only on the current state and performance of the Intenet connection but on the fact how many payloads are already 
waiting in the in-memory transport queues and what is the currently configured Rollbar reporting rate. However, if the queues are empty a blocking logging call normally takes somewhere from 70 msec to 200 msec.

On the other hand, async variants of the logging calls are essential to defining performance of the SDK since they are the calls that should be normally used for on going logging (unless the hosting process is about to quit right after a logging call and you want to make sure all the logging payloads are delivered to the Rollbar API before that).

As of now, we are benchmarking the async Log(...) method overloads for the following payload types:
..* a string message;
..* an exception.

Each of the simulated payload types has three variants based on their size. For now we support forllowing payload size categories: Small, Medium, and Large.

Here is content of the small message used by the harness: “Small message 1234 56789! ”. 
The medium message is just a concatenation of the small message 10 times. 
The large message is a concatenation of the medium one 20 times.

Size variations of the exception like payload are built similarly as derivative of the smallest base. 
The small exception includes 5 frames, the medium one includes 17 frames, the large one - 227 frames.

Here are some examples of such payloads:

[A small message payload.](https://rollbar.com/Rollbar/Rollbar.Net/items/409/occurrences/55989509691/)
[A medium message payload.](https://rollbar.com/Rollbar/Rollbar.Net/items/412/occurrences/55989598411/)
[A larges message payload.](https://rollbar.com/Rollbar/Rollbar.Net/items/413/occurrences/55989692814/)

[A small one](https://rollbar.com/Rollbar/Rollbar.Net/items/416/occurrences/55989554430/)
[A medium one.](https://rollbar.com/Rollbar/Rollbar.Net/items/415/occurrences/55989645792/)
[A large one.](https://rollbar.com/Rollbar/Rollbar.Net/items/417/occurrences/55989726254/)

## What is our benchmarking engine?

To track the performance of the SDK, we use a very fine benchmarking library for .NET codebases called [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet).

We implement a benchmarking harness in the form of a console application. The harness provides the simulated test payloads and calls the benchmarked SDK APIs.

The BenchmarkDotNet uses the harness' codebase to generate its instances based on the following .NET Standard implementations:
..* .NET Framework;
..* .NET Core; 
..* Mono.

When running on Windows platform, BenchmarkDotNet executes the harness within each of the .NET implementation runtimes (for now we have it configuret to use the latest version of each runtime).
It collects, analyses and summarizes benchmarking data per runtime.  


