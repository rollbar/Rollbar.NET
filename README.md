# Rollbar.NET

A .NET Rollbar Client that is not ASP.NET specific.

## Why?

[RollbarSharp](https://github.com/mroach/RollbarSharp) is great, works nicely,
even with Nancy if you choose. But the problem is that it depends on being in an
ASP.NET environment. You can't use it in a console application, or a windows
phone application, or any of the dozens of other web frameworks you might want
to take advantage of.

This library handles all of that by making it easy to construct a valid rollbar
occurence leaving two important things up to you, the implementer:

 1. How do you send this to Rollbar? (Do you use a `WebClient`? `RestSharp`?
 (if you don't mind the lisense), raw `HttpClient`?)
 2. How do you get all the information out of your environment and into the
 payload you're about to send Rollbar?

## Install

Nuget Package Manager:

    install-package Rollbar

## What's inside?

To get started you've got exactly one interesting class to worry about:
`RollbarPayload`. The class and the classes that compose the class cannot be
constructed without all mandatory arguments, and mandatory fields cannot be set.
Therefore, if you can constrcut a payload then it is valid for the purposes of
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
 assigned tothe `CrashReport` field of `RollbarBody`

None of the fields on `RollbarBody` are updatable, and all null fields in
`Rollbar.NET` are left off of the final JSON payload.

## Examples

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
