using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using PKERP.Base.Domain.Interface.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MES.Controllers
{
    [SessionTimeout]
    public class CorporationDashboardController : Controller
    {
        // GET: CorporationDashboard

        public ActionResult CorporationDashboard()
        {
            var lstFactories = OPS_DAL.MesBus.FcmtBus.GetFactories(string.Empty);
            ViewBag.LstFactories = lstFactories;
            return View("~/Views/CorporationDashboard/CorporationDashboard.cshtml");
        }

        public JsonResult GetListFactory()
        {
            var lstFactories = OPS_DAL.MesBus.FcmtBus.GetFactories(string.Empty);

            return Json(lstFactories);
        }

        public JsonResult GetListActiveMesPkgHaveNameFactory(string factoryId, string yyyyMMdd)
        {
            if (string.IsNullOrWhiteSpace(factoryId) || string.IsNullOrWhiteSpace(yyyyMMdd))
                return Json(new FailedTaskResult<List<Mpdt>>("Factory and date cannot empty"), JsonRequestBehavior.AllowGet);
            //Get list of mes package
            var listMesPkg = MpdtBus.GetListMesPkgHaveNameFactory(factoryId, yyyyMMdd);

            //Get list line which have mes package
            var newListMesPkg = listMesPkg.Where(p => !string.IsNullOrEmpty(p.MxPackage)).ToList();

            return Json(new SuccessTaskResult<IEnumerable<Mpdt>>(newListMesPkg), JsonRequestBehavior.AllowGet);
        }
    }
}