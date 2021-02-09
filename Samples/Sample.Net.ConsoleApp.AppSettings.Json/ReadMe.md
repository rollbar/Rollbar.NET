This sample demonstrates basic integration of the RollbarNotifier with a .NET console application based on appsettings.json 

1. From the console application project, reference the Rollbar SDK module (or its Nuget package).
2. From the console application project, reference the Rollbar.AppSettings.Json SDK module (or its Nuget package).
3. Make sure your application project also refrences: 
	- Microsoft.Extensions.Configuration.Abstractions,
	- Microsoft.Extensions.Configuration,
	- Microsoft.Extensions.Configuration.Binder,
	- Microsoft.Extensions.Configuration.Json,
	- Microsoft.Extensions.Configuration.FileExtensions.
4. Add properly composed appsettings.json to the application project and set it to "Copy always"
5. Add test call to log a message via the Rollbar Notifier.
5. Build, run and verify the expected paylads show up on the Project Dashboard on Rollbar.com
