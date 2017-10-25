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
            Username = "rollbar",
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

## Caught Exceptions

To send a caught exception to Rollbar, you must call Rollbar.Report(). You can set an item's level when you call this function. The level can be 'Debug', 'Info', 'Warning', 'Error' and 'Critical'.

```csharp
Rollbar.Report(exception, ErrorLevel.Critical);
```

## Person Data

You can set the current person data with a call to

```csharp
Rollbar.PersonData(() => new Person
{
    Id = 123,
    Username = "rollbar",
    Email = "user@rollbar.com"
});
```

and this person will be attached to all future Rollbar calls.

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
