using MES.CommonClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MES.Controllers
{
	[AutologArribute]
    public class SysPageController : Controller
    {
        // GET: SysPage
        public ActionResult Index()
        {
            return View();
        }

        protected override void HandleUnknownAction(string actionName)
        {
            try
            {
                this.View(actionName).ExecuteResult(this.ControllerContext);
            }
            catch {
                Response.Redirect("PageNotFound");
            }
        }

    }
}