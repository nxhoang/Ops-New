using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MES.CommonClass;
namespace MES.Controllers
{
	[AutologArribute]
    public class QCOParameterController : Controller
    {
        // GET: QCOParameter
        public ActionResult Management()
        {
            ViewBag.PageTitle = "<i class=\"fa fa-cog\"></i>&nbsp;QCO";
            ViewBag.SubPageTitle = "&nbsp;<span>> QCO-Factory Setup (New)</span>";
            return View();
        }
    }
}