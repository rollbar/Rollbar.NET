This sample app demonstrates simple use of the Rollbar Notifier configured via the app.config file
within an existing .NET console application.

1. Add references to the Rollbar.App.Config SDK module 
   (or add its Nuget package to the application project).

2. Add references to the Rollbar SDK module 
   (or add its Nuget package to the application project).

3. Make sure the application references System.Configuration.ConfigurationManager Nuget package.

4. Update the app.config file of the application with the Rollbar relevant settings.

5. Start using the Rollbar Notifier within the console application to log desired payloads.

6. Build, run, verify the payloads show up within the Project Dashboard on Rollbar.com