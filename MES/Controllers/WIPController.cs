using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MES.Controllers
{
    [SessionTimeout]
    public class WIPController : Controller
    {
        // GET: WIP
        public ActionResult WIP()
        {
            ViewBag.PageTitle = "Work In Process";
            return View();
        }

        public JsonResult GetWorkInProcessByMesPkg(string mesPkg, string getType)
        {
            try
            {
                //Check MES package whether is empty or not
                if (string.IsNullOrEmpty(mesPkg))
                {
                    return Json(new List<Mpdt>(), JsonRequestBehavior.AllowGet);
                }

                //Get style information in mes package
                var styleInf = mesPkg.Split('_')[3];
                var styleCode = styleInf.Substring(0, 7);
                var styleSize = styleInf.Substring(7, 3);
                var styleColorSerial = styleInf.Substring(10, 3);
                var revNo = styleInf.Substring(13, 3);

                //Get list work in process
                var listWIP = MpdtBus.GetMesPackageWorkingProcess(mesPkg, styleCode, styleSize, styleColorSerial, revNo, getType);

                return Json(listWIP, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new List<Mpdt>(), JsonRequestBehavior.AllowGet); ;
            }
        }
    }
}