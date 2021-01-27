using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using PKERP.Base.Domain.Interface.Dto;

namespace MES.Controllers
{
    [SessionTimeout]
    public class ActiveLineMesController : Controller
    {
        // GET: ActiveLineMes
        public ActionResult ActiveLine()
        {
            return View();
        }

        public ActionResult MesActiveLine()
        {
            //ViewBag.PageTitle = "Line Chart Dashboard";
            //var lstFactories = OPS_DAL.MesBus.FcmtBus.GetFactories(string.Empty);
            //ViewBag.LstFactories = lstFactories;

            return View("~/Views/ActiveLineMes/MesActiveLine.cshtml");
        }

        public JsonResult GetMesPackagesByDate(string factoryId, string scheDate)
        {
            if (string.IsNullOrWhiteSpace(factoryId) && string.IsNullOrWhiteSpace(scheDate))
                return Json(new FailedTaskResult<List<Mpdt>>("Factory cannot empty"), JsonRequestBehavior.AllowGet);

            var listMpdt = MpdtBus.GetMesPackagesScheduled(factoryId, scheDate);
            
            return Json(new SuccessTaskResult<IEnumerable<Mpdt>>(listMpdt), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLinesByFactoryId(string factoryId)
        {
            if (string.IsNullOrWhiteSpace(factoryId) )
                return Json(new FailedTaskResult<List<Mpdt>>("Factory cannot empty"), JsonRequestBehavior.AllowGet);

            var listLines = LineBus.GetLinesByFactoryId(factoryId);

            return Json(new SuccessTaskResult<IEnumerable<LineEntity>>(listLines), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMappingSeats(string factoryId, string plnStartDate)
        {
            if (string.IsNullOrWhiteSpace(factoryId) || string.IsNullOrWhiteSpace(plnStartDate))
                return Json(new FailedTaskResult<List<LineEntity>>("Factory or Plan Start Date cannot empty"), JsonRequestBehavior.AllowGet);

            var listLines = LineBus.GetMappingSeats(factoryId, plnStartDate);

            var connectedIot = LineBus.GetConnectedIot(factoryId, plnStartDate);

            foreach (var lin in listLines)
            {
                foreach (var iot in connectedIot)
                {
                    if(iot.LineSerial == lin.LineSerial && iot.Factory == lin.Factory)
                    {
                        lin.ConnectedIot = iot.ConnectedIot;

                        var timespan = DateTime.Now - iot.LAST_IOT_DATA_RECEIVE_TIME;

                        //If data receiving less than 30 minutes then set line is deactive
                        if(timespan.TotalMinutes > 30)
                        {
                            lin.IsActive = "N";
                        }
                        else
                        {
                            lin.IsActive = "Y";
                        }
                    }
                }
            }

            return Json(new SuccessTaskResult<IEnumerable<LineEntity>>(listLines), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetListMesPkg(string factoryId, string yyyyMMdd, string isLast30Min)
        {
            if (string.IsNullOrWhiteSpace(factoryId) || string.IsNullOrWhiteSpace(yyyyMMdd))
                return Json(new FailedTaskResult<List<Mpdt>>("Factory and date cannot empty"), JsonRequestBehavior.AllowGet);
            //Get list of mes package
            var listMesPkg = MpdtBus.GetListMesPkg(factoryId, yyyyMMdd);

            if (isLast30Min == "1")
            {
                //Get list mes packages which have data was send from Iot in last 30 minutes
                var pkgIn30 = MpdtBus.GetMesPackageInLast30Mins(factoryId, yyyyMMdd);
                foreach (var mpdt in listMesPkg)
                {
                    //Find package to update active status
                    var pkg = pkgIn30.Find(p => p.MxPackage == mpdt.MxPackage && p.LineSerial == mpdt.LineSerial);
                    mpdt.IsActive = pkg != null ? pkg.IsActive : "N";
                   
                }
            }
          
            return Json(new SuccessTaskResult<IEnumerable<Mpdt>>(listMesPkg), JsonRequestBehavior.AllowGet);
        }
    }
}