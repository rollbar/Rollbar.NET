# Rollbar.NET

A .NET Rollbar Client that is not ASP.NET specific.

## Install

Nuget Package Manager:

    Install-Package Rollbar

## Basic Usage

* Initialize Rollbar with `Rollbar.Init(new RollbarConfig("POST_SERVER_ACCESS_TOKEN"))`
* Send errors to Rollbar with `Rollbar.Report(Exception)`
* Send messages to Rollbar with `Rollbar.Report(string)`

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

To send a caught exception to Rollbar, you must call Rollbar.Report(). You can set an item's level when you call this function. The level can be 'Debug', 'Info', 'Warning', 'Error' and 'Critical'.

```csharp
Rollbar.Report(exception, ErrorLevel.Critical);
```


## Advanced Usage

If you want more control over sending the data to Rollbar, you can fire up a `RollbarClient`
yourself, and make the calls directly. To get started you've got exactly one interesting class
to worry about: `RollbarPayload`. The class and the classes that compose the class cannot be
constructed without all mandatory arguments, and mandatory fields cannot be set.
Therefore, if you can construct a payload then it is valid for the purposes of
sending to Rollbar. To get the JSON to send to Rollbar just call
`RollbarPayload.ToJson` and stick it in the request body.

There are two other *particularly* interesting classes to worry about:
`RollbarData` and `RollbarBody`. `RollbarData` can be filled out as completely
or incompletely as you want, except for the `Environment` ("debug",
"production", "test", etc) and and `RollbarBody` fields. The `RollbarBody` is
where "what you're actually posting to Rollbar" lives. All the other fields on
`RollbarData` answer contextual questions about the bug like "who saw this
error" (`RollbarPerson`), "What HTTP request data can you give me about the
error (if it happened during an HTTP Request, of course)" (`RollbarRequest`),
"How severe was the error?" (`Level`). Anything you see on the
[rollbar api website](https://rollbar.com/docs/api/items_post/) can be found in
`Rollbar.NET`.

`RollbarBody` can be constructed one of 5 ways:

 1. With a class extending from `Exception`, which will automatically produce a
 `RollbarTrace` object, assigning it to the `Trace` field of the `RollbarBody`.
 2. With a class extending from `AggregateException`, which will automatically
 produce an array of `RollbarTrace` objects for each inner exception, assigning
 it to the `TraceChain` field of `RollbarBody`.
 3. With an actual array of `Exception` objects, which will automatically
 produce an array of `RollbarTrace` objects for each exception, assigning
 it to the `TraceChain` field of `RollbarBody`.
 4. With a `RollbarMessage` object, which consists of a string and any
 additional keys you wish to send along. It will be assigned to the `Message`
 field of `RollbarBody`.
 5. With a string, which should be formatted like an iOS crash report. This
 library has no way to verify if you've done this correctly, but if you pass in
 a string it will be wrapped in a dictionary and assigned to the `"raw"` key and
 assigned to the `CrashReport` field of `RollbarBody`

None of the fields on `RollbarBody` are updatable, and all null fields in
`Rollbar.NET` are left off of the final JSON payload.

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

        Rollbar.Report(filterContext.Exception);
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
    Rollbar.Init(new RollbarConfig
    {
        AccessToken = "POST_SERVER_ACCESS_TOKEN",
        Environment = "production"
    });
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

    Application.ThreadException += (sender, args) =>
    {
        Rollbar.Report(args.Exception);
    };

    AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
    {
        Rollbar.Report(args.ExceptionObject as System.Exception);
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
            Rollbar.Init(new RollbarConfig
            {
                AccessToken = "<your rollbar token>",
                Environment = "production"          
            });
            // Setup Exception Handler
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Rollbar.Report(args.ExceptionObject as System.Exception);
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

#### Page Level Error Handling

From a page error handler in its code-behind file/class:

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


#### Code Level Error Handling

From the catch block:

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


