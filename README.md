# Rollbar.NET

A .NET Rollbar Client that is not ASP.NET specific.

## Installation

Nuget Package Manager:

    Install-Package Rollbar

## Upgrading to v1.0.0 from earlier versions

In order to upgrade to v1 from an earlier version, you should change your config from the old version

```csharp
Rollbar.Init(new RollbarConfig("POST_SERVER_ACCESS_TOKEN"));
```

to 

```csharp
const string postServerItemAccessToken = "POST_SERVER_ACCESS_TOKEN";
RollbarLocator.RollbarInstance
.Configure(new RollbarConfig(postServerItemAccessToken) { Environment = "proxyTest" }) ;
```

Additionally, anywhere in your code that you were sending error reports via `Rollbar.Report(Exception)` or `Rollbar.Report(string)` will need to be replaced with either something like `RollbarLocator.RollbarInstance.Error(new Exception("trying out the TraceChain", new NullReferenceException()))` or `RollbarLocator.RollbarInstance.Info("Basic info log example.")`.

## Quick Start

### When Using the Singleton-like Instance of the Notifier

1. Get a reference to the singleton-like instance of the Notifier by calling `RollbarLocator.RollbarInstance`.
2. Properly configure the instance (before any attempts to use it for logging) by calling its `Configure(…)` method while supplying valid configuration parameters.
3. Call any of the `ILogger`’s methods on the instance.

```csharp
const string postServerItemAccessToken = "POST_SERVER_ACCESS_TOKEN";

RollbarLocator.RollbarInstance
    .Configure(new RollbarConfig(postServerItemAccessToken) { Environment = "proxyTest" })
    ;

RollbarLocator.RollbarInstance
    .Info("Basic info log example.")
    .Debug("First debug log.")
    .Error(new NullReferenceException())
    .Error(new Exception("trying out the TraceChain", new NullReferenceException()))
    ;
```

### When Using a Scoped Instance of the Notifier

1. Get reference to a newly created instance of the Notifier by calling the `RollbarFactory.CreateNew()` helper method.
2. Properly configure the instance (before any attempts to use it for logging) by calling its `Configure(…)` method while supplying valid configuration parameters.
3. Call any of the `ILogger`’s methods on the instance.
4. Dispose of the Notifier instance at the end of its scope by casting it to `IDisposable` and calling `Dispose()` on the cast.

Here the scoped instance of the Notifier is disposed of with the help of the `using(…){…}` block:

```csharp
 RollbarConfig loggerConfig = new RollbarConfig(RollbarUnitTestSettings.AccessToken)
{
    Environment = RollbarUnitTestSettings.Environment,
};

using (var logger = RollbarFactory.CreateNew().Configure(loggerConfig))
{
    logger.Log(ErrorLevel.Error, "test message");
        .Info("Basic info log example.")
        .Debug("First debug log.")
        .Error(new NullReferenceException())
        .Error(new Exception("trying out the TraceChain", new NullReferenceException()))
        ;
}
```


## Reference

The SDK supports instantiation of multiple instances of the Notifier component as needed. The lifetime of an instance can be scoped and the instance should be disposed of at the end of its scope. All the instances implement and obey the `IDisposable` interface and support on-the-fly reconfiguration.  

There is one instance of the Notifier that implements the `IDisposable` interface but does not dispose of itself even if the `Dispose()` method is called on it. This instance has a singleton-like behavior and should be treated as an application-wide service of global scope with its lifetime matching the lifetime of the hosting application.

Each instance of the Notifier component can be configured independently and differently from any other instance, and more than one Notifier instance can point to the same endpoint and use the same access token. The SDK instantiates one payload queue per Notifier instance. The queues are indexed by an access token instance that is used by the corresponding Notifier instance. This allows the dedicated worker thread to process and retry the queues while complying with specified rate limits per access token. All these queues-organizing and queues-processing responsibilities are performed by the internal RollbarQueueController singleton type. 

All the interfaces and types listed below are defined within the Rollbar namespace.

### IReconfigurable<T>

This interface models any generic type T that implements its own reconfiguration based on another provided instance that serves as a prototype for the new configuration of a given object of the type.

```csharp
/// <summary>
/// Defines generic IReconfigurable interface.
/// 
/// Any type that supports its own reconfiguration based on a provided original
/// configuration should implement this interface.
/// </summary>
/// <typeparam name="T">A type that supports its reconfiguration.</typeparam>
public interface IReconfigurable<T>
{
    /// <summary>
    /// Reconfigures this object similar to the specified one.
    /// </summary>
    /// <param name="likeMe">
    /// The pre-configured instance to be cloned in terms of its configuration/settings.
    /// </param>
    /// <returns>Reconfigured instance.</returns>
    T Reconfigure(T likeMe);

    /// <summary>
    /// Occurs when this instance reconfigured.
    /// </summary>
    event EventHandler Reconfigured;
}
```

### RollbarConfig

The `RollbarConfig` object allows you to configure the Rollbar library.

  <dl>
<dt>AccessToken
</dt>
<dd>The access token for your project, allows you access to the Rollbar API
</dd>
<dt>Endpoint
</dt>
<dd>The Rollbar API endpoint, defaults to https://api.rollbar.com/api/1/
</dd>
<dt>Environment
</dt>
<dd>Environment name, e.g. `"production"` or `"development"` defaults to `"production"`
</dd>
<dt>Enabled
</dt>
<dd>If set to false, errors will not be sent to Rollbar, defaults to `true`
</dd>
<dt>LogLevel
</dt>
<dd>The default level of the messages sent to Rollbar
</dd>
<dt>Transform
</dt>
<dd>Allows you to specify a transformation function to modify the payload before it is sent to Rollbar. Use this function to add any value that is in [Request.cs](https://github.com/rollbar/Rollbar.NET/blob/master/Rollbar/Request.cs), such as the user's IP address, query string, and URL of the request. 

```csharp
new RollbarConfig
{
    Transform = payload =>
    {
        payload.Data.Person = new Person
        {
            Id = 123,
            UserName = "rollbar",
            Email = "user@rollbar.com"
        };
        payload.Data.CodeVersion = "2";
        payload.Data.Request = new Request()
        {
            Url = "http://rollbar.com",
            UserIp = "192.121.222.92"
        };
    }
}
```

</dd>

<dt>ProxyAddress
</dt>
<dd>A string URI which will be used as the WebClient proxy by passing to the WebProxy constructor.
</dd>

</dl>

###  ILogger

This interfaces models all the logging methods supported by the SDK.

```csharp
public interface ILogger
{
    ILogger Log(ErrorLevel level, object obj, IDictionary<string, object> custom = null);
    ILogger Log(ErrorLevel level, string msg, IDictionary<string, object> custom = null);

    ILogger Critical(string msg, IDictionary<string, object> custom = null);
    ILogger Error(string msg, IDictionary<string, object> custom = null);
    ILogger Warning(string msg, IDictionary<string, object> custom = null);
    ILogger Info(string msg, IDictionary<string, object> custom = null);
    ILogger Debug(string msg, IDictionary<string, object> custom = null);

    ILogger Critical(Exception error, IDictionary<string, object> custom = null);
    ILogger Error(Exception error, IDictionary<string, object> custom = null);
    ILogger Warning(Exception error, IDictionary<string, object> custom = null);
    ILogger Info(Exception error, IDictionary<string, object> custom = null);
    ILogger Debug(Exception error, IDictionary<string, object> custom = null);

    ILogger Critical(ITraceable traceableObj, IDictionary<string, object> custom = null);
    ILogger Error(ITraceable traceableObj, IDictionary<string, object> custom = null);
    ILogger Warning(ITraceable traceableObj, IDictionary<string, object> custom = null);
    ILogger Info(ITraceable traceableObj, IDictionary<string, object> custom = null);
    ILogger Debug(ITraceable traceableObj, IDictionary<string, object> custom = null);

    ILogger Critical(object obj, IDictionary<string, object> custom = null);
    ILogger Error(object obj, IDictionary<string, object> custom = null);
    ILoger Warning(object obj, IDictionary<string, object> custom = null);
        ILogger Info(object obj, IDictionary<string, object> custom = null);
    ILogger Debug(object obj, IDictionary<string, object> custom = null);
}
```

### IRollbar

This interface models the complete public interface of a Notifier component.

```csharp
/// <summary>
/// Defines IRollbar notifier interface.
/// </summary>
/// <seealso cref="Rollbar.ILogger" />
/// <seealso cref="System.IDisposable" />
public interface IRollbar
    : ILogger
    , IDisposable
{
    /// <summary>
    /// Configures the using specified settings.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// <returns></returns>
    IRollbar Configure(RollbarConfig settings);

    /// <summary>
    /// Configures using the specified access token.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    /// <returns></returns>
    IRollbar Configure(string accessToken);

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    /// <value>
    /// The configuration.
    /// </value>
    RollbarConfig Config { get; }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    /// <value>
    /// The logger.
    /// </value>
    ILogger Logger { get; }

    /// <summary>
    /// Occurs when a Rollbar internal event happens.
    /// </summary>
    event EventHandler<RollbarEventArgs> InternalEvent;
}
```

### RollbarEventArgs

This is an abstract base for implementing concrete derived types representing different internal events happening within the SDK.
Currently, the following concrete events are supported:

```csharp
InternalErrorEventArgs
RollbarApiErrorEventArgs
CommunicationErrorEventArgs
CommunicationEventArgs
```

All the events are reported as an abstract RollbarEventArgs base instance. Once you subscribe to the event, cast the received instances to the concrete type of interest.

### RollbarFactory

This is a static utility class that is used to create any scoped instance of the Notifier (which is an implementation of `IRollbar`) via its `CreateNew()` method.

```csharp
/// <summary>
/// RollbarFactory utility class.
/// </summary>
public static class RollbarFactory
{
    /// <summary>
    /// Creates the new instance of IRollbar.
    /// </summary>
    /// <returns></returns>
    public static IRollbar CreateNew()
    {
        return RollbarFactory.CreateNew(false);
    }

    /// <summary>
    /// Creates the new instance of IRollbar.
    /// </summary>
    /// <param name="isSingleton">if set to <c>true</c> [is singleton].</param>
    /// <returns></returns>
    internal static IRollbar CreateNew(bool isSingleton)
    {
        return new RollbarLogger(isSingleton);
    }
} 
```

All the scoped instances of the Notifier component are `IDisposable`, so use the `using(…){…}` block while dealing with short-lived instances or call the `Dispose()` method manually when controlling the instance lifetime manually.

### RollbarLocator

This is a helper singleton-like type used to gain access and instantiate the special singleton-like instance of the Notifier on the first access. Call its `RollbarLocator.RollbarInstance` property to get reference to the special Notifier singleton instance.

```csharp
RollbarLocator.RollbarInstance
    .Configure(new RollbarConfig("POST_SERVER_ACCESS_TOKEN") { Environment = "production" })
    ;
```

### RollbarQueueController

This is a singleton type used to gain access to the SDK-wide internal events at runtime. 

To monitor any Notifier library internal event, regardless of the specific instance of the Notifier:

1. Get a reference to the `RollbarQueueController.Instance` singleton.
2. Subscribe to its `InternalEvent` event.
3. Implement the event handler as you see fit.
At runtime, all the Rollbar internal events generated while using any instance of the Notifier will be reported into the event handler. 

To monitor internal events within any specific instance of the Notifier:
1. Get a reference to a specific instance of the Notifier. 
2. Subscribe to its `InternalEvent` event.
3, Implement the event handler as you see fit.
At runtime, all the Rollbar internal events generated while using this specific instance of the Notifier will be reported into the event handler. 

An example demonstrating both levels of monitoring at the same time:

```csharp
static void Main(string[] args)
{
    RollbarQueueController.Instance.InternalEvent += OnRollbarInternalEvent;

    const string postServerItemAccessToken = "POST_SERVER_ACCESS_TOKEN";

    RollbarLocator.RollbarInstance
        .Configure(new RollbarConfig(postServerItemAccessToken) { Environment = "proxyTest" })
        .InternalEvent += OnRollbarInternalEvent
        ;

    RollbarLocator.RollbarInstance
        .Info("Basic info log example.")
        .Debug("First debug log.")
        .Error(new NullReferenceException())
        .Error(new Exception("trying out the TraceChain", new NullReferenceException()))
        ;

    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10));
}

private static void OnRollbarInternalEvent(object sender, RollbarEventArgs e)
{
    Console.WriteLine(e.TraceAsString());

    RollbarApiErrorEventArgs apiErrorEvent = e as RollbarApiErrorEventArgs;
    if (apiErrorEvent != null)
    {
        //TODO: handle/report Rollbar API communication error event...
        return;
    }
    CommunicationEventArgs commEvent = e as CommunicationEventArgs;
    if (commEvent != null)
    {
        //TODO: handle/report Rollbar API communication event...
        return;
    }
    CommunicationErrorEventArgs commErrorEvent = e as CommunicationErrorEventArgs;
    if (commErrorEvent != null)
    {
        //TODO: handle/report basic communication error while attempting to reach 
        // Rollbar API service... 
        return;
    }
    InternalErrorEventArgs internalErrorEvent = e as InternalErrorEventArgs;
    if (internalErrorEvent != null)
    {
        //TODO: handle/report basic internal error while using the Rollbar Notifier... 
        return;
    }
}
```

## Person Data

You can set the current person data with a call to

```csharp
Rollbar.PersonData(() => new Person
{
    Id = 123,
    UserName = "rollbar",
    Email = "user@rollbar.com"
});
```

and this person will be attached to all future Rollbar calls.

## Examples

### ASP.Net MVC

To use inside an ASP.Net Application, first in your global.asax.cs and Application_Start method
initialize Rollbar

```csharp
protected void Application_Start()
{
    ...
    Rollbar.Init(new RollbarConfig
    {
        AccessToken = ConfigurationManager.AppSettings["Rollbar.AccessToken"],
        Environment = ConfigurationManager.AppSettings["Rollbar.Environment"]
    });
    ...
}
```

Then create a global action filter

```csharp
public class RollbarExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext filterContext)
    {
        if (filterContext.ExceptionHandled)
            return;

        RollbarLocator.RollbarInstance.Error(filterContext.Exception);
    }
}
```

and finally add it to the global filters collection

```csharp
private static void RegisterGlobalFilters(GlobalFilterCollection filters)
{
    ...
    filters.Add(new RollbarExceptionFilter());
}
```


### ASP.Net Core 2

The SDK can be integrated into an Asp.Net Core 2 application on two levels:
1. Each ASP.Net Core controllers’ method implementation could be surrounded by a try-catch block where, within the `catch(…){…}` block, any caught exception is passed to a common exception handling routine which in turn reports the exception via the SDK. 

2. A request processing pipeline of the application is extended with the Rollbar.NET middleware component that monitors all the inner middleware components of the pipeline for unhandled exceptions and reports them via the Rollbar.NET Notifier singleton instance and then rethrows the exceptions while wrapping them with a new Exception object.

You can check out a sample ASP.Net Core 2 based application that demonstrates a proper use of the middleware component [here](https://github.com/rollbar/Rollbar.NET/tree/master/Sample.AspNetCore2.WebApi).

1. First, the singleton component needs to be properly configured. There are two approaches to doing this. If both approaches are used, the second option always overrides the first option.

#### Option 1
Add at least the minimum required configuration parameters to the hosting application’s `appsettings.json` file. Any of the properties of the RollbarConfig class that has public setter can be set using this approach. 

```json
{
  "Logging": {
    "IncludeScopes": false,
    "Debug": {
      "LogLevel": {
        "Default": "Warning"
      }
    },
    "Console": {
      "LogLevel": {
        "Default": "Warning"
      }
    }
  },

  "Rollbar": {
    "AccessToken": "POST_SERVER_ACCESS_TOKEN",
    "Environment": "AspNetCoreMiddlewareTest"
  }
}
```

#### Option 2
Add proper Rollbar configuration within the `ConfigureServices(…)` method of `Startup.cs`.

```csharp
/// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{
    ConfigureRollbarSingleton();
    services.AddMvc();
}

/// <summary>
/// Configures the Rollbar singleton-like notifier.
/// </summary>
private void ConfigureRollbarSingleton()
{
    const string postServerItemAccessToken = "POST_SERVER_ACCESS_TOKEN";

    RollbarLocator.RollbarInstance
        // minimally required Rollbar configuration:
        .Configure(new RollbarConfig(postServerItemAccessToken) { Environment = "AspNetCoreMiddlewareTest" })
        // optional step if you would like to monitor Rollbar internal events within your application:
        InternalEvent += OnRollbarInternalEvent
        ;
}
```


2. Next, add the Rollbar middleware to the application pipeline. This is normally done by adding its usage within the `Configure(…)` method of `Startup.cs`. 

```csharp
// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseRollbarMiddleware();

    // Any other middleware component intended to be "monitored" by Rollbar.NET middleware should go below this line

    app.UseMvc();
}
```


That is, it! Now every unhandled exception within the middleware pipeline under the Rollbar middleware component monitoring will be reported to Rollbar.

### Winforms

To use inside a Winforms Application, do the following inside your main method:

```csharp
[STAThread]
static void Main()
{
    Rollbar.Init(new RollbarConfig
    {
        AccessToken = "POST_SERVER_ACCESS_TOKEN",
        Environment = "production"
    });
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

    Application.ThreadException += (sender, args) =>
    {
        RollbarLocator.RollbarInstance.Error(args.Exception);
    };

    AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
    {
        RollbarLocator.RollbarInstance.Error(args.ExceptionObject as System.Exception);
    };

    Application.Run(new Form1());
}
```

### WPF

It is optional to set the user for Rollbar, and this can be reset to a different user at any time. This example includes a default user being set with `MainWindow.xml` loads by calling the `SetRollbarReportingUser` function.

In `App.cs`:

```csharp
namespace Sample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            System.Diagnostics.Debug.WriteLine("App Start Up");

            //Initialize Rollbar
            Rollbar.Init(new RollbarConfig
            {
                AccessToken = "POST_SERVER_ACCESS_TOKEN",
                Environment = "production"          
            });
            // Setup Exception Handler
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                RollbarLocator.RollbarInstance.Error(args.ExceptionObject as System.Exception);
            };
        }
    }
}
```

In `MainWindow.cs`:

```csharp
namespace Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            System.Diagnostics.Debug.Write("Starting MainWindow");

            InitializeComponent();

            //Set Default User for RollbarReporting
            //  -- Reset if user logs in or wait to call SetRollbarReportingUser until user logs in 
            SetRollbarReportingUser("id", "myEmail@example.com", "default");
        }

        private void SetRollbarReportingUser(string id, string email, string userName)
        {
            Person person = new Person(id);
            person.Email = email;
            person.UserName = userName;
            Rollbar.PersonData(() => person);
        }
    }
}
```

## Help / Support

If you run into any issues, please email us at [support@rollbar.com](mailto:support@rollbar.com)

You can also find us in IRC: [#rollbar on chat.freenode.net](irc://chat.freenode.net/rollbar)

For bug reports, please [open an issue on GitHub](https://github.com/rollbar/Rollbar.NET/issues/new).


## Contributing

1. [Fork it](https://github.com/rollbar/Rollbar.NET)
2. Create your feature branch (```git checkout -b my-new-feature```).
3. Commit your changes (```git commit -am 'Added some feature'```)
4. Push to the branch (```git push origin my-new-feature```)
5. Create new Pull Request
