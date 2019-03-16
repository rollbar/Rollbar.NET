# v3.0.2 Rollbar.NET Notifier SDK Release Notes

## Upgrade Notes

Depending on how you were integrating the Notifier into your application prior to v3, you might have to perform some or all of the following changes when moving to v3 of our SDK:
-   in addition to Rollbar assembly reference, add references to relevant .NET Specific Technology/Application Integration Modules (these modules are described in the next sections);
-   if you did have to add one or more references to the .NET Specific Technology/Application Integration Modules, you will have to update namespaces of the types that were moved from Rollbar to these new modules;
-   if you have any calls to IAsyncLogger's methods where you are waiting on the returned Task object to complete, you will now have to wrap your data (originally passed into the logging method) into an `ObjectPackage` instance while setting its `MustApplySynchronously` flag and pass the wrapper instead of the original data into the same logging method.

All of these changes are very straightforward and the compiler will help you and guide you along the way as needed. 
Most of it is just a "find-and-replace" with a few "compile-and-correct-the-build-issues".

## General SDK-wide Notes

### Overview

Starting with v3 we split the Rollbar.NET Notifier SDK into more components or modules.

These components can be logically grouped into the following categories:
-   Core Modules
-   .NET Specific Technology/Application Integration Modules
-   Third-party Logging Libraries/Frameworks' Integration Plug-ins

Core Modules implement the very basic stand-alone core functionality of the Rollbar.NET Notifier. 
They have least possible amount of dependencies and only rely on functionality provided by most basic .NET implementation types 
that are common across all of the supported .NET Standard implementations. So, if you only care about using core SDK concepts like:
IRollbar, ILogger, IRollbarConfig, RollbarFactory, RollbarLocator,IRollbarPackage, Rollbar.DTOs, RollbarQueueController, ITelemetryCollector, ITelemetryConfig, some basic Rollbar utility classes
and you are planning to implement all the integration of these classes into your application on your own - use the Core Modules only.

If you care about functionality that we implemented to simplify integration of the Notifier core functionality into a specific .NET application technology/framework, like 
.NET Framework ASP.NET or .NET Core ASP.NET etc. - you may want to pull in some of the .NET Specific Technology/Application Integration Modules.

If you want to have the Notifier integrated as a simple plug-in expected by a commonly used third-party logging library/framework, 
like Serilog, log4net, etc. (because you are already relying on one of these) - look for proper plug-in among the Third-party Logging Libraries/Frameworks' Integration Plug-ins.

Here are examples of currently available modules in each category:

-   Core Modules: 
1.   Rollbar.

-   .NET Specific Technology/Application Integration Modules: 
1.   Rollbar.Net.AspNet, 
2.   Rollbar.Net.AspNet.Mvc, 
3.   Rollbar.Net.AspNet.WebApi, 
4.   Rollbar.NetCore.AspNet.

-   Third-party Logging Libraries/Frameworks' Integration Plug-ins:
 1.   Rollbar.PlugIns.Log4net,
 2.   Rollbar.PlugIns.MSEnterpriseLibrary,
 3.   Rollbar.PlugIns.NLog,
 4.   Rollbar.PlugIns.Serilog.

Each of the modules is available via NuGet as stand-alone package. 

We are also unifying versioning all of the modules to follow common SDK versioning. 

### Fixes and Improvements

#### v3.0.0-preview
-   resolve #287: Add more seamless integration with ASP.NET (Full Framework) based applications.
-   resolve #288: Add sample of Rollbar.NET within ASP.NET (Full Framework) based applications.
-   resolve #289: Improve flexibility composing Data DTO.
-   resolve #290: Improve comments related to meaning of DTOs properties.
-   resolve #291: Define IRollbarPackage
-   resolve #296: Out of memory exception with high exceptions rate in multi threaded environment.
-   resolve #297: Assumption failure when setting nullable value for a key in ExtendableDtoBase
-   resolve #298: Make initialization of RollbarConfig via config files or explicitly specified access token as mutually exclusive.
-   resolve #300: Implement packaging strategy abstraction.
-   resolve #301: Implement packaging strategy decorator abstraction.
-   resolve #316: Implement packaging strategy for arbitrary object payload.
-   resolve #302: Implement packaging strategy for messages.
-   resolve #303: Implement packaging strategy for exceptions.
-   resolve #304: Implement packaging strategy for ExceptionContext.
-   resolve #312: Implement packaging strategy for Data DTO
-   resolve #313: Implement packaging strategy for Body DTO
-   resolve #305: Implement packaging strategy decorator for Person info.
-   resolve #314: Implement packaging strategy decorator for custom Key Value Pairs
-   resolve #315: Implement packaging strategy decorator for custom RollbarConfig
-   resolve #306: Implement packaging strategy decorator for HttpRequest info.
-   resolve #307: Implement packaging strategy decorator for HttpContext info.
-   resolve #311: Rename packaging strategies and their decorators into "Package"s and "PackageDecorator"s
-   resolve #310: Implement PayloadBundle
-   resolve #308: Implement automatic strategy/Data time-stamping.
-   resolve #309: Complete integration of packaging strategies
-   resolve #317: Consolidate dependencies versions.

#### v3.0.1-preview
-   resolve #322: General codebase cleanup.
-   resolve #321: Address latest Codacy review results.
-   resolve #320: Update CI build scripts/environment.
-   resolve #325: Extract integration with .NET Framework ASP.NET MVC into separate project (Rollbar.Net.AspNet.Mvc).
-   resolve #326: Extract integration with .NET Framework ASP.NET Web API into separate project (Rollbar.Net.AspNet.WebApi).

#### v3.0.2
-   resolve #322: General codebase cleanup.
-   resolve #321: Address latest Codacy review results.
-   resolve #320: Update CI build scripts/environment.
-   resolve #325: Extract integration with .NET Framework ASP.NET MVC into a separate module/Nuget-package Rollbar.Net.AspNet.Mvc.
-   resolve #326: Extract integration with .NET Framework ASP.NET Web API into a separate module/Nuget-package Rollbar.Net.AspNet.WebApi.
-   resolve #327: Extract integration with .NET Core ASP.NET (middleware) into a separate module/Nuget-package Rollbar.NetCore.AspNet.
-   resolve #332: Complete cleanup of the Request DTO constructors that are based on specialized framework types
-   resolve #330: Implement RollbarHttpModule as part of Rollbar.Net.AspNet integration module/Nuget-package.
-   resolve #334: Fix Azure build pipeline
-   resolve #329: ASP.NET code middleware not reporting request data.
-   resolve #331: Verify and correct sample apps to adopt latest SDK changes related to multiple integration modules
-   resolve #336: Extract SdkCommon.csproj and clean-up SDK projects' settings
-   resolve #337: Unify/sync-up SDK components versioning
-   resolve #318: Add explicit build target for .NET Core 2.2
-   resolve #338: Fix all the build warnings across the SDK
-   resolve #263: PostBody is always null
-   resolve #335: Update documentation regarding v3 changes

## Notes per SDK Module/Nuget-Package

### Core Modules

The most fundamental API of the Notifier is `ILogger`. It defines a collection of convenience methods for sending different kinds of data payloads using different logging levels flags.
Internally, any implementation of the `ILogger` that we have performs these three distinct steps when any of its logging methods is called:
1.   enqueue the data object(s) to log for future transmission to the Rollbar API;
2.   package/snap-shot the incoming/enqueued data object(s) into proper Rollbar data structure while applying some rules specified as part of relevant RollbarConfig instance;
3.   transmit the enqueued items to the Rollbar API according to relevant RollbarConfig settings.

To minimize impact of logging on the calling thread, ideally, it would be nice to perform all the steps on auxiliary background thread(s). 

However, in cases when highly mutable data is about to be logged it is essential to have step 2 performed on the calling thread before returning from the logging method.
In v2 we introduced `IAsynLogger` to help handle such cases. Its methods were a complete copy of `ILogger`'s methods with one main difference - they all used to return a Task, 
so that the client code could wait for it to complete steps 1 and 2 before proceeding further if that is what needed in a specific case. 
While it was a nice flexible and easy to use solution from API point of view, the tasks did not perform well (as we learned it the hard way) under EXTREMELY high AND sustained rate of load. 
So, in v3, we went away from the Tasks and removed `IAsynLogger` all together. We are now back to having only `ILogger` and we have a substitute for the eliminated Tasks in the form of `IRollbarPackage`.
Think of the `IRollbarPackage` as a basis for implementing arbitrary data packaging strategies with explicit flag (named as `MustApplySynchronously`) that signifies need to apply the packaging (steps 1 and 2)
on the calling thread before returning from a logging method. We also provide with abstract base classes like `RollbarPackageBase` and `RollbarPackageDecoratorBase` for implementing custom packaging strategies and their decorators.
We used these abstraction to implement our own collection of packagers and their decorators. All of them are available to the SDK users as well. 
In addition to helping us in getting away from the Tasks usage, these new abstractions allow for very flexible and powerful ways to bundle a lot specific types of data into a single payload as needed 
while encapsulating and reusing the packaging rules of any custom type.  
In v3, you can either throw into a logging method a data object to log (exactly the way it was in v2) or you can wrap in an `ObjectPackage` while setting the `MustApplySynchronously` flag if you want the logger to behave 
the way IAsyncLogger used to when you had to block-wait on its Task to complete.

Also, in some scenarios a Notifier client may want to make sure that the data was actually transmitted to the Rollbar API before it can proceed further (like exiting the application/process). 
That is why we still support `ILogger AsBlockingLogger(TimeSpan timeout)` method on the `ILogger`. It makes sure all of the 3 internal processing steps are performed before its logging methods return or it times out with an exception. 

#### Rollbar

`IAsyncLogger` is gone.

All the Asp.Net Core middleware related classes were moved to the new Rollbar.NetCore.AspNet module.
All the .NET Framework's HttpRequest and HttpContext capture functionality was moved to Rollbar.Net.AspNet module.

New abstractions:
-   `IRollbarPackage`
-   `RollbarPackageBase` 
-   `RollbarPackageDecoratorBase`

New useful types:
-   `ObjectPackage`
-   `ExceptionPackage`
-   `MessagePackage`
-   `DataPackage`
-   `BodyPackage`
-   `PersonPackageDecorator`
-   `CustomKeyValuePackageDecorator`
-   `ConfigAttributesPackageDecorator`
-   `HttpRequestMessagePackageDecorator`

### .NET Specific Technology/Application Integration Modules

#### Rollbar.Net.AspNet

New useful types:
-   `HttpContextPackageDecorator`
-   `HttpRequestPackageDecorator`
-   `RollbarHttpModule`

#### Rollbar.Net.AspNet.Mvc

New useful types:
-   `ExceptionContextPackageDecorator`
-  `RollbarExceptionFilter`

#### Rollbar.Net.AspNet.WebApi

New useful types:
-   `RollbarExceptionFilterAttribute`

#### Rollbar.NetCore.AspNet

All the Rollbar middleware for Asp.Net Core is implemented in this module: 
Rollbar middleware, logger factory, logger provider, etc.

New useful types:
-   `HttpRequestPackageDecorator`
-   `RollbarHttpContextPackageDecorator`

### Third-party Logging Libraries/Frameworks' Integration Plug-ins

#### Rollbar.PlugIns.Log4Net

Implements Rollbar.NET Notifier as a log4net Appender.

#### Rollbar.PlugIns.MSEnterpriseLibrary

Implements Rollbar.NET Notifier as an IExceptionHandler.

#### Rollbar.PlugIns.NLog

Implements Rollbar.NET Notifier as a nlog Target.

#### Rollbar.PlugIns.Serilog

Implements Rollbar.NET Notifier as a Serilog Sink.

## More Information about the SDK

More detailed information about Rollbar.NET usage and API reference are available at [https://docs.rollbar.com/docs/dotnet](https://docs.rollbar.com/docs/dotnet)

## Help / Support

If you run into any issues, please email us at [support@rollbar.com](mailto:support@rollbar.com)

For bug reports, please [open an issue on GitHub](https://github.com/rollbar/Rollbar.NET/issues/new).
