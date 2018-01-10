# Rollbar.NET

A .NET Rollbar Client that can be hosted by any application built based on following .NET versions:
1. .NET Core 2.0 and newer;
2. .NET Standard 2.0 and newer;
3. .NET Full Framework 4.5 and newer.

## Install

Nuget Package Manager:

    Install-Package Rollbar

## Basic Usage

* Configure Rollbar with `RollbarLocator.RollbarInstance.Configure(new RollbarConfig("POST_SERVER_ACCESS_TOKEN"))`
* Send errors to Rollbar with `RollbarLocator.RollbarInstance.Error(Exception)`
* Send messages to Rollbar with `RollbarLocator.RollbarInstance.Info(string)`

## RollbarConfig

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

To send a caught exception to Rollbar, you must call RollbarLocator.RollbarInstance.Log(). You can set an item's level when you call this function. The level can be 'Debug', 'Info', 'Warning', 'Error' and 'Critical'.

In addition there other conveninence methods for logging messages using different error levels that are named after the levels.

```csharp
RollbarLocator.RollbarInstance.Error(exception);
```

## Logging Messages Using the Notifier

### When Using the Singleton-like Instance of the Notifier

1.	Get a reference to the singleton-like instance of the Notifier by calling RollbarLocator.RollbarInstance.
2.	Properly configure the instance (before any attempts to use it for logging) by calling its Configure(…) method while supplying valid configuration parameters.
3.	Call any of the ILogger’s methods on the instance in order to log messages towards Rollbar API service.

For example:
```csharp
            const string postServerItemAccessToken = "…65fa5041749b6bf7095a190001…";

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

1.	Get reference to a newly created instance of the Notifier by calling RollbarFactory.CreateNew() helper method.
2.	Properly configure the instance (before any attempts to use it for logging) by calling its Configure(…) method while supplying valid configuration parameters.
3.	Call any of the ILogger’s methods on the instance in order to log messages towards Rollbar API service.
4.	Dispose of the Notifier instance at the end of its scope by casting it to IDisposable and calling Dispose() on the cast.

For example, here the scoped instance of the Notifier is disposed of with the help of the using(…){…} block:

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

## Monitoring Notifier’s Internal Events

To monitor any Notifier library internal event regardless of specific instance of the Notifier:

1.	Get a reference to the RollbarQueueController.Instnace singleton.
2.	Subscribe to its InternalEvent event.
3.	Implement the event handler the way you see it.
4.	As the result of this subscription, at runtime, all the Rollbar internal events generated while using any instance of the Notifier will be reported into the event handler. 

To monitor internal events within any specific instance of the Notifier:

1.	Get a reference to a specific instance of the Notifier. 
2.	Subscribe to its InternalEvent event.
3.	Implement the event handler the way you see it.
4.	As the result of this subscription, at runtime, all the Rollbar internal events generated while using this specific instance of the Notifier will be reported into the event handler. 

For example, to demonstrate both levels of monitoring at the same time:

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
            const string rollbarAccessToken = "17965fa5041749b6bf7095a190001ded";
            const string rollbarEnvironment = "RollbarNetSamples";

            var config = new RollbarConfig(rollbarAccessToken) // minimally required Rollbar configuration
            {
                Environment = rollbarEnvironment,
                ScrubFields = new string[]
                {
                    "access_token", // normally, you do not want scrub this specific field (it is operationally critical), but it just proves safety net built into the notifier... 
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

If you want more control over sending the data to Rollbar, there is one interesting class
to worry about: `Rollbar.DTOs.Payload`. The class and the classes that compose the class cannot be
constructed without all mandatory arguments, and mandatory fields cannot be set.
Therefore, if you can construct a payload then it is valid for the purposes of
sending to Rollbar. You can have a read/write access to instance reference of this type 
within your own functions/actions defined as CheckIgnore, Transform and Truncate delegates of
a `RollbarConfig` instance.

There are two other *particularly* interesting classes to worry about:
`Rollbar.DTOs.Data` and `Rollbar.DTOs.Body`. `Rollbar.DTOs.Data` can be filled out as completely
or incompletely as you want, except for the `Environment` ("debug",
"production", "test", etc) and and `Body` fields. The `Body` is
where "what you're actually posting to Rollbar" lives. All the other fields on
`Rollbar.DTOs.Data` answer contextual questions about the bug like "who saw this
error" (`RollbarPerson`), "What HTTP request data can you give me about the
error (if it happened during an HTTP Request, of course)" (`Rollbar.DTOs.Request`),
"How severe was the error?" (`Level`). Anything you see on the
[rollbar api website](https://rollbar.com/docs/api/items_post/) can be found in
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

### Asp.Net Core 2

Rollbar.NET Notifier can be integrated into an Asp.Net Core 2 application on two levels:

1.	Each Asp.Net Core controllers’ method implementation could be surrounded by a try-catch block where, within the catch(…){…} block, any caught  exception is passed to a common exception handling routine which, in turn, reports the exception via the Rollbar.NET Notifier (possibly, among other exception processing steps). 
2.	A request processing pipeline of the application is extended with the Rollbar.NET middleware component that “monitors” all the “inner” middleware components of the pipeline for unhandled exceptions and reports them via the Rollbar.NET Notifier singleton instance and, then, rethrows the exceptions while wrapping them with a new Exception object.
The codebase repository has a sample Asp.Net Core 2 based application (Sample.AspNetCore2.WebApi) that demonstrates a proper use of the middleware component. 

Here is how it works:

#### As usually, the Rollbar.NET Notifier singleton component needs to be properly configured. 

There are two ways of doing that:

1.	Add at least minimum required configuration parameters to the hosting application’s appsettings.json file. For example:   

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
    "AccessToken": "17965fa5041749b6bf7095a190001ded",
    "Environment": "AspNetCoreMiddlewareTest"
  }
}
```
Note: any of the properties of the RollbarConfig class that has public setter can be set using this approach. 

2.	Add proper Rollbar configuration within the ConfigureServices(…) method of Startup.cs like so:  

```csharp
        // This method gets called by the runtime. Use this method to add services to the container.
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
            const string rollbarAccessToken = "17965fa5041749b6bf7095a190001ded";
            const string rollbarEnvironment = "RollbarNetSamples";

            RollbarLocator.RollbarInstance
                // minimally required Rollbar configuration:
                .Configure(new RollbarConfig(rollbarAccessToken) { Environment = rollbarEnvironment })
                // optional step if you would like to monitor Rollbar internal events within your application:
                .InternalEvent += OnRollbarInternalEvent
                ;
        }
```

Note: if for some reason both approaches are used, the in-code one always “overrides” the app-settings one.

#### Next, we need to add the Rollbar middleware to the application pipeline. 

This is normally done by adding its usage within the Configure(…) method of the Startup.cs, like so: 

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

That is, it! At this point every unhandled exception within the middleware pipeline under the Rollbar middleware component monitoring will be reported to the Rollbar API service.

### ASP.Net MVC

To use inside an ASP.Net Application, first in your global.asax.cs and Application_Start method
initialize Rollbar

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
        RRollbarLocator.RollbarInstance.Error(args.ExceptionObject as System.Exception);
    };

    Application.Run(new Form1());
}
```

### WPF

Example of using Rollbar.NET inside of a WPF application. It is optional to set the user for Rollbar and can be reset to a different user at any time. This example includes a default user being set with `MainWindow.xml` loads by calling the `SetRollbarReportingUser` function. [Gist example code here](https://gist.github.com/cdesch/e08275e85a3f27a7b1b481430e12f308).

`App.cs`:
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
                AccessToken = "<your rollbar token>",
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

`MainWindow.cs`:
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


