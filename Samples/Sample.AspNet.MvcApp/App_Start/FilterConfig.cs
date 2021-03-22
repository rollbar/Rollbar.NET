using Rollbar.Net.AspNet.Mvc;
using System.Web;
using System.Web.Mvc;

namespace Sample.AspNet.MvcApp
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new RollbarExceptionFilter("SOURCE: Sample.AspNet.MvcApp's RollbarExceptionFilter"));
        }
    }
}
