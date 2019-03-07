using Rollbar;
using Rollbar.Net.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Sample.WebForms.Site
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Let's simulate an error:
                throw new ApplicationException("WebForms.Site sample: simulated exception");
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
        }

        private void Page_Error(object sender, EventArgs e)
        {
            // Get last error from the server. 
            Exception exception = Server.GetLastError();

            // Let's report to Rollbar on the Page Level: 
            var metaData = new Dictionary<string, object>();
            metaData.Add("reportLevel", "PageLevel");
            metaData.Add("failedPage", this.AppRelativeVirtualPath);
            IRollbarPackage package = new ExceptionPackage(exception, "Page_Error", true);
            package = new CustomKeyValuePackageDecorator(package, metaData);
            package = new HttpContextPackageDecorator(package, this.Context);
            RollbarLocator.RollbarInstance.Error(package);

            // Handle specific exception. 
            if (exception is InvalidOperationException)
            {
                // Pass the error on to the error page. 
                Server.Transfer("ErrorPage.aspx?handler=Page_Error%20-%20Default.aspx", true);
            }
        }
    }
}