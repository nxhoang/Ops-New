using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OPS_DAL;

namespace OPS.Controllers
{
    [SessionTimeout]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //CCLT.test();
            return View("Test");
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
    }
}