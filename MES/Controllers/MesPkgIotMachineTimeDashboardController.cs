using DocumentFormat.OpenXml.Vml.Spreadsheet;
using MES.Repositories;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OPS_DAL.Business;
using OPS_DAL.DgsBus;
using OPS_DAL.DgsEntities;
using OPS_DAL.Entities;
using OPS_DAL.MesEntities;
using PKERP.Base.Domain.Interface.Dto;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MES.Controllers
{
    /*----- Create by Dinh Van 2020-09-22 -----*/
    [SessionTimeout]
    public class MesPkgIotMachineTimeDashboardController : Controller
    {
        private readonly IMpdtRepository _mpdtRepo;

        public MesPkgIotMachineTimeDashboardController(IMpdtRepository mpdtRepo)
        {
            _mpdtRepo = mpdtRepo;
        }

        // GET: MesPkgIotMachineTimeDashboard
        public ActionResult Index()
        {
            return View();
        }

        //Action get mes package
        public async Task<JsonResult> GetMesPkgByDate(string factory, string date)
        {
            if (string.IsNullOrWhiteSpace(factory))
                return Json(new FailedTaskResult<List<Mpdt>>("Factory cannot empty"), JsonRequestBehavior.AllowGet);

            var lstMpdt = await _mpdtRepo.GetMesPackagesByDateAsync(factory, date);

            return Json(new SuccessTaskResult<IEnumerable<Mpdt>>(lstMpdt), JsonRequestBehavior.AllowGet);
        }

        //Action return data to draw chart
        public async Task<JsonResult> DisplayDashboard(string mesPkg, string date)
        { 
            //Get scanned iot of MES PP
            var lstMpdt = await _mpdtRepo.GetIotOfMesPackage(mesPkg);
            if(lstMpdt.Count() == 0)
            {
                return Json(new FailedTaskResult<List<Mpdt>>("Mes Package not has Iot"), JsonRequestBehavior.AllowGet);
            }
            
            //Get DGS iot machine time
            var lstIot = IotMachineTimeBus.GetDGSIotMachineTime(date);

            //join list iot of mes and dgs
            var query = from lst_mpdt in lstMpdt
                        join lst_iot in lstIot 
                        on lst_mpdt.IOT_MODULE_MAC equals lst_iot.MacAddress into lf
                        from subpet in lf.DefaultIfEmpty()
                        select new
                        {
                            ExcDttm = subpet?.ExcDttm ?? String.Empty,
                            PowerTime = subpet?.PowerTime ?? 0,
                            MotoTime = subpet?.MotoTime ?? 0,
                            ActTime = subpet?.ActTime ?? 0,
                            //lst_iot.MacAddress,
                            //lst_iot.ExcDttm,
                            //lst_iot.PowerTime,
                            //lst_iot.MotoTime,
                            //lst_iot.MachineId,
                            lst_mpdt.IOT_MODULE_MAC,
                            lst_mpdt.MCID,
                            lst_mpdt.MXPACKAGE,
                            lst_mpdt.OPNAME,
                            lst_mpdt.OPGROUPNAME,
                            lst_mpdt.LAST_IOT_DATA,
                            lst_mpdt.LAST_IOT_DATA_DGS,
                            lst_mpdt.VAN_COUNT,
                            lst_mpdt.EMPLOYEENAME,
                            lst_mpdt.CORPORATIONCODE,
                            lst_mpdt.IMAGENAME
                        };

            //Get max and min time
            List<dynamic> list = new List<dynamic>();
            foreach(var item in lstMpdt)
            {
                var el = query
                            .Where(x => x.IOT_MODULE_MAC == item.IOT_MODULE_MAC)
                            .GroupBy(x => x.IOT_MODULE_MAC)
                            .Select(x => new {
                                MacAddress = item.IOT_MODULE_MAC,
                                MinDate = x.Min(z => z.ExcDttm),
                                MaxDate = x.Max(z => z.ExcDttm),
                                MinPowerTime = x.Min(z => z.PowerTime),
                                MaxPowerTime = x.Max(z => z.PowerTime),
                                MaxMotoTime = x.Max(z => z.MotoTime),
                                MaxActTime = x.Max(z => z.ActTime),
                                MachineId = item.MCID,
                                OpName = item.OPNAME,
                                OpGroupName = item.OPGROUPNAME,
                                LastIotData = item.LAST_IOT_DATA,
                                LastIotDataDgs = item.LAST_IOT_DATA_DGS,
                                VanCount = item.VAN_COUNT,
                                EmployeeName = item.EMPLOYEENAME,
                                CorporationCode = item.CORPORATIONCODE,
                                ImageName = item.IMAGENAME

                            }).ToList().FirstOrDefault();
                if(el != null && item.MCID.Length < 10)
                {
                    list.Add(el);
                }
            }
            
            return Json(new SuccessTaskResult<IEnumerable<dynamic>>(list), JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetInfoDetailChart(string mesPkg, string date)
        {
            var lstMpdt = await _mpdtRepo.GetInfoChartByMesDate(mesPkg, date);

            if (lstMpdt.Count() == 0)
            {
                return Json(new FailedTaskResult<List<Mpdt>>("Mes Package not has Iot"), JsonRequestBehavior.AllowGet);
            }

            return Json(new SuccessTaskResult<IEnumerable<dynamic>>(lstMpdt), JsonRequestBehavior.AllowGet);
        }

        //public JsonResult GetImageByMesPacKage(string mesPkg)
        //{
        //    try
        //    {
        //        var lstMpdt = StmtBus.GetStyleInfoByMxPackage(mesPkg);
        //        return Json(lstMpdt, JsonRequestBehavior.AllowGet);

        //    } catch (Exception ex)
        //    {
        //        var mes = ex.Message;
        //        return Json(mes, JsonRequestBehavior.AllowGet);
        //    }

        //}

        public ActionResult GetImageByMesPacKage(string mesPkg)
        {
            try
            {
                var lstMpdt = StmtBus.GetStyleInfoByMxPackage(mesPkg);
                return PartialView("ParImageMesPkg", lstMpdt);
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
                return Json(mes, JsonRequestBehavior.AllowGet);
            }

        }

        //get LOB rate dashboard Efficeticy Operaion
        public async Task<JsonResult> getLobRate(string mesPkg)
        {
            var lstMpdt = await _mpdtRepo.GetLobRateByMesPackage(mesPkg);

            if (lstMpdt.Count() == 0)
            {
                return Json(new FailedTaskResult<List<Mpdt>>("Mes Package not has Iot"), JsonRequestBehavior.AllowGet);
            }

            return Json(new SuccessTaskResult<IEnumerable<dynamic>>(lstMpdt), JsonRequestBehavior.AllowGet);
        }


        
        public async Task<JsonResult> DisplayOperationPlan(string mesPkg, string date)
        {
            //Get list operation plan 
            var lstOpe= await _mpdtRepo.GetOperationPlan(mesPkg);

            return Json(new SuccessTaskResult<IEnumerable<dynamic>>(lstOpe), JsonRequestBehavior.AllowGet);
        }
    }
}