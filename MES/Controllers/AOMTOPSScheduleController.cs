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
    public class AOMTOPSScheduleController : Controller
    {
        // GET: AOMTOPSSchedule
        public ActionResult AOMTOPSSchedule()
        {
            return View();
        }

        public JsonResult GetLinesByProPackage(string factoryId, string startDate, string endDate, string buyer, string styleInfo, string aoNo)
        {
            try
            {
                if (string.IsNullOrEmpty(factoryId) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate) )
                {
                    return Json(new List<Vepp>(), JsonRequestBehavior.AllowGet);
                }

                //Get production package through view
                var lstVeep = VeppBus.GetFactoryLinesMtop(factoryId, startDate, endDate, buyer, styleInfo, aoNo);

                return Json(lstVeep, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult GetProductionPackage(string factoryId, string startDate, string endDate, string buyer, string styleInfo, string aoNo, string searchType)
        {
            try
            {
                if (string.IsNullOrEmpty(factoryId) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
                {
                    return Json(new List<Vepp>(), JsonRequestBehavior.AllowGet);
                }

                //Get production package through view
                var lstVeep = VeppBus.GetProductionPackageMtop(factoryId, startDate, endDate, buyer, styleInfo, aoNo, searchType);

                return Json(lstVeep, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }
    }
}