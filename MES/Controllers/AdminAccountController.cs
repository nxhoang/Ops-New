using MES.CommonClass;
using OPS_DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MES.Controllers
{
    public class AdminAccountController : Controller
    {
        // GET: AdminAccount
		[SysActionFilter(SystemID = "MES", MenuID = "AAS", Action = "")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult NewUser(Usmt objUsmt)
        {
            var res = OPS_DAL.Business.UsmtBus.NewUser(objUsmt);
            return Json(new { res }, JsonRequestBehavior.AllowGet);
        }
    }
}