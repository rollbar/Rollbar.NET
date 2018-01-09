To add Rollbar.NET to a WebForms application:

1. update your application NuGet packages to the latest possible version.
2. add Rollbar NuGet package;

3. Application Level Error Handling.
   =================================

From the application error handler in the Global.asax file: 

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

4. Page Level Error Handling.
   ==========================

From a page error handler in its code-behind file/class, for example:

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


5. Code Level Error Handling.
   ==========================

From the catch block. For example:

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
