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

## Reference

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
<dd>Environment name, e.g. `"production"` or `"development"`, defaults to `"production"`
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
<dd>Allows you to specify a transformation function to modify the payload before it is sent to Rollbar. Use this function to add any value that is in [Request.cs](https://github.com/rollbar/Rollbar.NET/blob/master/Rollbar/DTOs/Request.cs), such as the user's IP address, query string, and URL of the request. 

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
<dt>CheckIgnore
</dt>
<dd>Function called before sending payload to Rollbar. Return `true` to stop the error from being sent to Rollbar.
</dd>
<dt>Truncate
</dt>
<dd>Truncates the payload before sending it to Rollbar.
</dd>
<dt>Server
</dt>
<dd>An object with the following properties: `Host`, `Root`, `Branch`, and `CodeVersion`. 
</dd>
<dt>Person
</dt>
<dd>You can set the current person data like so

```csharp
private void SetRollbarReportingUser(string id, string email, string userName)
       {
           Person person = new Person(id);
           person.Email = email;
           person.UserName = userName;
           RollbarLocator.RollbarInstance.Config.Person = person;
       }
```

and this person will be attached to all future Rollbar calls.
</dd>
<dt>MaxReportsPerMinute
</dt>
<dd>The maximum reports sent to Rollbar per minute, as an integer.
</dd>
<dt>ReportingQueueDepth
</dt>
<dd>The reporting queue depth, as an integer. The reporting queue depth can be used to limit error report bursts when connectivity to the Rollbar API server is poor.
</dd>
<dt>ScrubFields
</dt>
<dd> Array of field names to scrub out of `_POST` and `_SESSION`. Values will be replaced with asterisks. If overriding, make sure to list all fields you want to scrub, not just fields you want to add to the default. Param names are case-sensitive when comparing against the scrub list. Default: `{'passwd', 'password', 'secret', 'confirm_password', 'password_confirmation'}`

```csharp
var config = new RollbarConfig(rollbarAccessToken) // minimally required Rollbar configuration
    {
        Environment = rollbarEnvironment,
        ScrubFields = new string[]
        { 
            "password",
            "username",
        }
    };
```
</dd>

</dl>

## Caught Exceptions

To send a caught exception to Rollbar, you must call `RollbarLocator.RollbarInstance.Log()`. You can set an item's level when you call this function. The level can be `'Debug'`, `'Info'`, `'Warning'`, `'Error'`, or `'Critical'`.

In addition, there other conveninence methods for logging messages using different error levels that are named after the levels.

```csharp
RollbarLocator.RollbarInstance.Error(exception);
```

## Logging Messages Using the Notifier

### When Using the Singleton-like Instance of the Notifier

1.	Get a reference to the singleton-like instance of the Notifier by calling `RollbarLocator.RollbarInstance`.
2.	Configure the instance (before any attempts to use it for logging) by calling its `Configure(…)` method while supplying valid configuration parameters.
3.	Call any of the `ILogger`’s methods on the instance.

```csharp
            const string postServerItemAccessToken = "POST_SERVER_ITEM_ACCESS_TOKEN";

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

1.	Get a reference to a newly created instance of the Notifier by calling the `RollbarFactory.CreateNew()` helper method.
2.	Properly configure the instance (before any attempts to use it for logging) by calling its `Configure(…)` method while supplying valid configuration parameters.
3.	Call any of the `ILogger`’s methods on the instance.
4.	Dispose of the Notifier instance at the end of its scope by casting it to `IDisposable` and calling `Dispose()` on the cast.

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

## Monitoring the Notifier’s Internal Events

To monitor any internal event regardless of the specific instance of the Notifier:

1.	Get a reference to the `RollbarQueueController.Instance` singleton.
2.	Subscribe to its `InternalEvent` event.
3.	Implement the event handler as you desire. As the result of this subscription, at runtime, all the Rollbar internal events generated while using any instance of the Notifier will be reported into the event handler. 

To monitor internal events within any specific instance of the Notifier:

1.	Get a reference to a specific instance of the Notifier. 
2.	Subscribe to its `InternalEvent` event.
3.	Implement the event handler as you desire. As the result of this subscription, at runtime, all the Rollbar internal events generated while using this specific instance of the Notifier will be reported into the event handler. 

Below is an example using both levels of monitoring at the same time:

```csharp
        static void Main(string[] args)
        {
            ConfigureRollbarSingleton();

            RollbarLocator.RollbarInstance
                .Info("ConsoleApp sample: Basic info log example.")
                .Debug("ConsoleApp sample: First debug log.")
                .Error(new NullReferenceException("ConsoleApp sample: null reference exception."))
                .Error(new System.Exception("ConsoleApp sample: trying out the TraceChain", new NullReferenceException()))
                ;

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10));

        }

        /// <summary>
        /// Configures the Rollbar singleton-like notifier.
        /// </summary>
        private static void ConfigureRollbarSingleton()
        {
            const string rollbarAccessToken = "POST_SERVER_ITEM_ACCESS_TOKEN";
            const string rollbarEnvironment = "RollbarNetSamples";

            var config = new RollbarConfig(rollbarAccessToken) // minimally required Rollbar configuration
            {
                Environment = rollbarEnvironment,
                ScrubFields = new string[]
                {
                    "pw",
                    "username",
                }
            };
            RollbarLocator.RollbarInstance
                // minimally required Rollbar configuration:
                .Configure(config)
                // optional step if you would like to monitor Rollbar internal events within your application:
                .InternalEvent += OnRollbarInternalEvent
                ;

            // Optional info about reporting Rollbar user:
            SetRollbarReportingUser("007", "jbond@mi6.uk", "JBOND");
        }

        /// <summary>
        /// Sets the rollbar reporting user.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="email">The email.</param>
        /// <param name="userName">Name of the user.</param>
        private static void SetRollbarReportingUser(string id, string email, string userName)
        {
            Person person = new Person(id);
            person.Email = email;
            person.UserName = userName;
            RollbarLocator.RollbarInstance.Config.Person = person;
        }

        /// <summary>
        /// Called when rollbar internal event is detected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RollbarEventArgs"/> instance containing the event data.</param>
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
                //      Rollbar API service... 
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

## Advanced Usage

If you want more control over sending data to Rollbar, there is one interesting class
to worry about: `Rollbar.DTOs.Payload`. The class and the classes that compose the class cannot be
constructed without all mandatory arguments, and mandatory fields cannot be set.
Therefore, if you can construct a payload, then it is valid for the purposes of
sending to Rollbar. You can have read/write access to instance reference of this type 
within your own functions/actions defined as `CheckIgnore`, `Transform`, and `Truncate` delegates of
a `RollbarConfig` instance.

There are two other *particularly* interesting classes to worry about:
`Rollbar.DTOs.Data` and `Rollbar.DTOs.Body`. `Rollbar.DTOs.Data` can be filled out as completely
or incompletely as you want, except for the `Environment` (`"debug"`,
`"production"`, `"test"`, etc) and `Body` fields. The `Body` is
where what you're actually posting to Rollbar lives. All the other fields on
`Rollbar.DTOs.Data` answer contextual questions about the bug, such as "who saw this
error?" (`RollbarPerson`), "what HTTP request data can you give me about the
error (if it happened during an HTTP Request)?" (`Rollbar.DTOs.Request`),
and "how severe was the error?" (`Level`). Anything you see in the
[Rollbar API docs](https://rollbar.com/docs/api/items_post/) can be found in
`Rollbar.NET`.

`Rollbar.DTOs.Body` can be constructed one of 5 ways:

 1. With a class extending from `Exception`, which will automatically produce a
 `Rollbar.DTOs.Trace` object, assigning it to the `Trace` field of the `Rollbar.DTOs.Body`.
 2. With a class extending from `AggregateException`, which will automatically
 produce an array of `Rollbar.DTOs.Trace` objects for each inner exception, assigning
 it to the `TraceChain` field of `Rollbar.DTOs.Body`.
 3. With an actual array of `Exception` objects, which will automatically
 produce an array of `Rollbar.DTOs.Trace` objects for each exception, assigning
 it to the `TraceChain` field of `RollbarBody`.
 4. With a `Rollbar.DTOs.Message` object, which consists of a string and any
 additional keys you wish to send along. It will be assigned to the `Message`
 field of `Rollbar.DTOs.Body`.
 5. With a string, which should be formatted like an iOS crash report. This
 library has no way to verify if you've done this correctly, but if you pass in
 a string it will be wrapped in a dictionary and assigned to the `"raw"` key and
 assigned to the `CrashReport` field of `Rollbar.DTOs.Body`

None of the fields on `Rollbar.DTOs.Body` are updatable, and all null fields in
`Rollbar.NET` are left off of the final JSON payload.

## Examples

### ASP.NET Core 2

The SDK can be integrated into an ASP.NET Core 2 application on two levels:

1.	Each ASP.NET Core controllers’ method implementation could be surrounded by a try-catch block where, within the `catch(…){…}` block, any caught exception is passed to a common exception handling routine which in turn reports the exception via the SDK.
2.	A request processing pipeline of the application is extended with the Rollbar.NET middleware component that monitors all the inner middleware components of the pipeline for unhandled exceptions and reports them via the Rollbar.NET Notifier singleton instance and then rethrows the exceptions while wrapping them with a new Exception object.

You can check out a sample ASP.NET Core 2 based application that demonstrates a proper use of the middleware component [here](https://github.com/rollbar/Rollbar.NET/tree/master/Sample.AspNetCore2.WebApi).

First, the singleton component needs to be properly configured. There are two approaches to doing this. If both approaches are used, the second option always overrides the first option.

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
    "AccessToken": "POST_SERVER_ITEM_ACCESS_TOKEN",
    "Environment": "AspNetCoreMiddlewareTest"
  }
}
```

#### Option 2

Within the `ConfigureServices(…)` method of `Startup.cs`, add proper Rollbar configuration and register the Rollbar logger with the application services:

```csharp
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureRollbarSingleton();

            services.AddRollbarLogger(loggerOptions =>
            {
                loggerOptions.Filter = (loggerName, loglevel) => loglevel >= LogLevel.Trace;
            });
        
            services.AddMvc();
        }

        /// <summary>
        /// Configures the Rollbar singleton-like notifier.
        /// </summary>
        private void ConfigureRollbarSingleton()
        {
            const string rollbarAccessToken = "POST_SERVER_ITEM_ACCESS_TOKEN";
            const string rollbarEnvironment = "RollbarNetSamples";

            RollbarLocator.RollbarInstance
                // minimally required Rollbar configuration:
                .Configure(new RollbarConfig(rollbarAccessToken) { Environment = rollbarEnvironment })
                // optional step if you would like to monitor Rollbar internal events within your application:
                .InternalEvent += OnRollbarInternalEvent
                ;
        }
```


Next, add the Rollbar middleware to the application pipeline. This is normally done by adding its usage within the `Configure(…)` method of `Startup.cs`.

```csharp
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRollbarMiddleware();

            // Any other middleware component intended to be "monitored" by Rollbar.NET middleware
            // go below this line:

            app.UseMvc();
        }
```

That's it! Now every unhandled exception within the middleware pipeline under the Rollbar middleware component monitoring will be reported to Rollbar.


### ASP.NET MVC

To use inside an ASP.NET application, first in your `global.asax.cs` and `Application_Start` method initialize Rollbar

```csharp
protected void Application_Start()
{
    ...
    RollbarLocator.RollbarInstance.Configure(new RollbarConfig
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

### Winforms

To use inside a Winforms Application, do the following inside your main method:

```csharp
[STAThread]
static void Main()
{
    RollbarLocator.RollbarInstance.Configure(new RollbarConfig
    {
        AccessToken = "POST_SERVER_ITEM_ACCESS_TOKEN",
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
        RRollbarLocator.RollbarInstance.Error(args.ExceptionObject as System.Exception);
    };
    
    TaskScheduler.UnobservedTaskException += (sender, args) =>
    {
        RollbarLocator.RollbarInstance.Error(args.Exception);
    };

    Application.Run(new Form1());
}
```

### WPF

It is optional to set the user for Rollbar, and this can be reset to a different user at any time. This example includes a default user being set with `MainWindow.xml` loads by calling the `SetRollbarReportingUser` function. Gist example code [here](https://gist.github.com/cdesch/e08275e85a3f27a7b1b481430e12f308).

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
            RollbarLocator.RollbarInstance.Configure(new RollbarConfig
            {
                AccessToken = "POST_SERVER_ITEM_ACCESS_TOKEN",
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
            RollbarLocator.RollbarInstance.Config.Person = person;
        }
    }
}
```

### WebForms

#### Application Level Error Handling

From the application error handler in the `Global.asax` file: 

```csharp
       void Application_Error(object sender, EventArgs e)
       {
           Exception exception = Server.GetLastError();
          
           // Let's report to Rollbar on the Application/Global Level:
           var metaData = new Dictionary<string, object>();
           metaData.Add("reportLevel", "GlobalLevel");
           RollbarLocator.RollbarInstance.Error(exception, metaData);
           
           if (exception is HttpUnhandledException)
           {
               // Pass the error on to the error page.
               Server.Transfer("ErrorPage.aspx?handler=Application_Error%20-%20Global.asax", true);
           }
       }
```

#### Page Level Error Handling

From a page error handler in its code-behind file/class:

```csharp
       private void Page_Error(object sender, EventArgs e)
       {
           // Get last error from the server.
           Exception exception = Server.GetLastError();
           
           // Let's report to Rollbar on the Page Level:
           var metaData = new Dictionary<string, object>();
           metaData.Add("reportLevel", "PageLevel");
           metaData.Add("failedPage", this.AppRelativeVirtualPath);
           RollbarLocator.RollbarInstance.Error(exception, metaData);
           
           // Handle specific exception.
           if (exception is InvalidOperationException)
           {
               // Pass the error on to the error page.
               Server.Transfer("ErrorPage.aspx?handler=Page_Error%20-%20Default.aspx", true);
           }
       }
```

#### Code Level Error Handling

From the catch block:

```csharp
          try
           {
               // Let's simulate an error:
               throw new NullReferenceException("WebForms.Site sample: simulated exception");
           }
           catch (Exception exception)
           {
               // Let's report to Rollbar on the Code Level:
               var metaData = new Dictionary<string, object>();
               metaData.Add("reportLevel", "CodeLevel");
               metaData.Add("failedPage", this.AppRelativeVirtualPath);
               RollbarLocator.RollbarInstance.Error(exception, metaData);
               throw exception;
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
