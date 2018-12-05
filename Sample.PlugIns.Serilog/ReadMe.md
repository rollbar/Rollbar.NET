# Sample: Rollbar.NET via Serilog

This example demonstrates how to integrate Rollbar.NET within an application that already uses Serilog for its logging/tracing purposes 
and have all (or some of) its logs/traces forwarded/reported to the Rollbar API Service/Dashboard.

## Starting Point

In this example, we have a very simple .NET Core console application that already makes use of Serilog to log informational and error traces using a Console sink.

## Adding Logs Forwarding to the Rollbar Dashboard

- Initialize and configure an instance of RollbarConfig.
- Add use of the RollbarSink (while supplying the RollbarConfig instance above) when creating an instance of the Serilog Logger.


## Done!

Have fun taking advantages for Rollbar Dashboard while monitoring behavior of your products!

Here is an example of an error log from this sample rendered within the Rollbar Dashboard:
https://rollbar.com/Rollbar/Rollbar.Net/items/610/occurrences/60084806017/


