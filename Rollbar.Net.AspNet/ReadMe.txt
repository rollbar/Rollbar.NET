This component implements Rollbar Notifier integration with .NET Framework ASP.NET as its HTTP Module:

https://docs.microsoft.com/en-us/previous-versions/ms227673(v=vs.140)



An HTTP module is called on every request in response to the BeginRequest and EndRequest events. 
As a result, the module runs before and after a request is processed.

If the ASP.NET application is running under IIS 6.0, you can use HTTP modules to customize requests for resources that are serviced by ASP.NET. 
This includes ASP.NET Web pages (.aspx files), Web services (.asmx files), ASP.NET handlers (.ashx files), and any file types that you have mapped to ASP.NET. 

If the ASP.NET application is running under IIS 7.0, you can use HTTP modules to customize requests for any resources that are served by IIS. 
This includes not just ASP.NET resources, but HTML files (.htm or .html files), graphics files, and so on. 
For more information, see ASP.NET Application Life Cycle Overview for IIS 5.0 and 6.0 and ASP.NET Application Life Cycle Overview for IIS 7.0.

This RollbarHttpModule: 
- subscribes (during the module initialization) to some relevant events of the HttpApplication object.
- has each event handler written as a private method of the module. 
- when the registered events are raised, ASP.NET calls the appropriate handler in the module, which forwards the event data to the Rollbar API service.


Registering the HTTP Module in IIS 6.0 and IIS 7.0 Classic Mode
===============================================================

After you have created the HelloWorldModule class, you register the module by creating an entry in the Web.config file. 
Registering the HTTP module enables it to subscribe to request-pipeline notifications.

In IIS 7.0, an application can run in either Classic or Integrated mode. 
In Classic mode, requests are processed basically the same as they are in IIS 6.0. 
In Integrated mode, IIS 7.0 manages requests by using a pipeline that enables it to share requests, modules, and other features with ASP.NET.

The procedure for registering a module is different in IIS 7.0 Classic mode and IIS 7.0 Integrated mode. 
This section describes the procedure for IIS 6.0 and IIS 7.0 Classic mode. 
The procedure for registering a module that is running in IIS 7.0 Integrated mode is described in the next section.

To register the module for IIS 6.0 and IIS 7.0 running in Classic mode
If the Web site does not already have a Web.config file, create one under the root of the site.

Add the following highlighted code to the Web.config file:

<configuration>
  <system.web>
	<httpModules>
		<add name="RollbarHttpModule" type="RollbarHttpModule"/>
	</httpModules>
  </system.web>
</configuration>

The code registers the module with the class name and the module name of RollbarHttpModule.

Registering the HTTP Module in IIS 7.0 Integrated Mode
======================================================

The process for registering a module in IIS 7.0 Integrated mode is slightly different than the process for IIS 7.0 Classic mode.

To register the module for IIS 7.0 running in Integrated mode
If the Web site does not already have a Web.config file, create one under the root of the site.

Add the following highlighted code to the Web.config file:


<configuration>
	<system.webServer>
		<modules>
			<add name="RollbarHttpModule" type="RollbarHttpModule"/>
		</modules>
	</system.webServer>
</configuration>
 
 NOTE

You can also register the module by using IIS Manager. For more information, see Configuring Modules in IIS 7.0.

The code registers the module with the class name and the module name of RollbarHttpModule.

Testing the Custom HTTP Module
After you have created and registered your custom HTTP module, you can test it.


The module automatically runs during any request for a file whose extension is assigned to ASP.NET. For more information, see HTTP Handlers and HTTP Modules Overview.

