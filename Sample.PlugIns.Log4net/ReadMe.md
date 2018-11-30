# Sample: Rollbar.NET via log4net

This example demonstrates how to integrate Rollbar.NET within an application that already uses log4net for its logging/tracing purposes 
and have all (or some of) its logs/traces forwarded/reported to the Rollbar API Service/Dashboard.

This example updates an existing .NET Core console application to support Rollbar capabilities (with no code changes or product re-build required). 
However, very similar approach can be taken with any .NET Full Framework based application. You will just have to configure Rollbar.NET notifier within 
the app.config file instead of the appsettings.config file.

## Starting Point

In this example we have a very simple .NET Core console application that already made use of log4net to log informational and error traces.
The log4net was custom configured using log4net.config that included use of log4net Console and RollingFile appenders.

## Adding Logs Forwarding to the Rollbar Dashboard

- Add/modify the appsettings.json application configuration file to include Rollbar specific configuration options.
- Drop Rollbar.dll and Rollbar.PlugIns.Log4net.dll side by side with the application executable file.
- Modify the  log4net.config to include the RollbarAppender.
- Restart your application to have log4net logs forwarded to the Rollbar Dashboard. 

NOTE:
In this example, we actually included the Rollbar and Rollbar.PlugIns.Log4net projects within the sample solution and referenced the projects from the ConsoleApp project.
It was done to facilitate debugging of the sample and can be omitted if you just drop expected Rollbar.dll and Rollbar.PlugIns.Log4net.dll files side by side with the 
application executable file as described in the steps above.

## Done!

Have fun taking advantages for Rollbar Dashboard while monitoring behavior of your products without having to recompile them (assuming you already used log4net prior to 
adding the Rollbar support to your products)!

Here is an example of an error log from this sample rendered within the Rollbar Dashboard:
https://rollbar.com/Rollbar/Rollbar.Net/items/612/occurrences/59671813374/

