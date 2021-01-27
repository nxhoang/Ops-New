using OPS_DAL.Business;
using OPS_DAL.Entities;
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
    public class DefectCodeController : Controller
    {
        // GET: DefectCode
        public ActionResult DefectCode()
        {
            ViewBag.PageTitle = "Defect Codes";
            return View();
        }

        public ActionResult GetListDefectCodeByCat(string defectCat)
        {            
            var listDfcd = DfmtBus.GetListDefectCodeByCategory(defectCat);

            return Json(listDfcd, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBuyerDefect(string pkDefectCode)
        {
            var listBuyerDef = DfmpBus.GetBuyerDefectMySql(pkDefectCode);

            return Json(listBuyerDef, JsonRequestBehavior.AllowGet);
        }
    }
}