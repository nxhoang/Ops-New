using OPS_DAL.MesBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MES.Controllers
{
    [SessionTimeout]
    public class SchedulingViewerController : Controller
    {
        // GET: SchedulingViewer
        public ActionResult SchedulingViewer()
        {
            return View();
        }

        public JsonResult GetMESPackage(string mesFac, string startDate, string endDate, string ppFactory, string aoNo, string buyer, string styleInf)
        {
            try
            {
                if (string.IsNullOrEmpty(mesFac) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
                {
                    return Json(new List<Mpmt>(), JsonRequestBehavior.AllowGet);
                }

                var listMpmt = MpmtBus.GetPackageGroup(mesFac, startDate, endDate, ppFactory, aoNo, buyer, styleInf);
                foreach (var mpmt in listMpmt)
                {
                    //mpmt.ListMpdt = MpdtBus.GetMesPackagesByPackageGroup(mpmt.PackageGroup);
                    mpmt.ListMpdt = MpdtBus.GetMesPackagesByPackageGroup(mpmt.PackageGroup).FindAll(x => int.Parse(x.PlnStartDate) >= int.Parse(startDate));
                    mpmt.ListPpkg = PpkgBus.GetProPackages(mpmt.PackageGroup);
                }

                return Json(listMpmt, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }
    }
}