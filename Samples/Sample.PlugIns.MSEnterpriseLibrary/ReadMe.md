# Sample: Rollbar.NET via MS Enterprise Library's Exception Handling Block

This example demonstrates how to integrate Rollbar.NET within an application that already uses MS Enterprise Library's Exception Handling Block for its exception logging purposes 
and have all (or some of) its exception logs/traces forwarded/reported to the Rollbar API Service/Dashboard.

## Starting Point

In this example, we have a very simple .NET console application that already makes use of MS Enterprise Library's Exception Handling Block.

## Adding Error Forwarding to the Rollbar Dashboard
- add an instance of RollbarExceptionHandler (using proper Rollbar token, environment and blocking timeout as needed) into the array of existing IExceptionHandler instances.

## Done!

Have fun taking advantages for Rollbar Dashboard while monitoring behavior of your products!

Here is an example of an error log from this sample rendered within the Rollbar Dashboard:
[https://rollbar.com/Rollbar/Rollbar.Net/items/1188/](https://rollbar.com/Rollbar/Rollbar.Net/items/1188/)

