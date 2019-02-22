using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sample.AspNet.MvcApp.Controllers
{
    public class HomeController : Controller
    {
        //public HomeController()
        //{

        //}

        public ActionResult Index()
        {
            throw new NotImplementedException("Simulated HomeController exception!");

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);
        }
    }
}