using OPS_DAL.Business;
using OPS_DAL.DgsBus;
using OPS_DAL.DgsEntities;
using OPS_DAL.Entities;
using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using OPS_DAL.TpmBus;
using OPS_DAL.TpmEntities;
using OPS_Utils;
using PKERP.Base.Domain.Interface.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MES.Controllers
{
    [SessionTimeout]
    public class DashboardController : Controller
    {
        public Usmt UserInf => (Usmt)Session["LoginUser"];

        // GET: Dashboard
        public ActionResult Dashboard()
        {
            ViewBag.PageTitle = "Machine Opeartions";
            ViewBag.SubPageTitle = "<span>> Machine Opeartions</span>";

            return View();
        }

        public ActionResult Default()
        {
            return View();
        }

        public JsonResult GetMaxMachineCountByDay(string yyyy, string mm, string dd, string weekNo, string showType)
        {
            //var tasks = new[]
            //{
            //    Task.Run(() => UpdateMachineCountDaily(yyyy, mm, dd, weekNo))
            //};

            //Get list of factories by server no            
            var listMc = MccnBus.GetListMachineCount(null, yyyy, mm, dd, ConstantGeneric.ServerNo);

            //Get total of Iot from DGS
            foreach (var fac in listMc)
            {
                //var facName = FactoryDgs.ListFactoryDgs().Where(f => f.FactoryId == fac.FACTORY).FirstOrDefault();
                //var listIot = IotMappingBus.GetListIotDgs(facName.FactoryName);

                var listIot = MchnDtlBus.GetListMachineTPM(fac.FACTORY, true);

                fac.TOTAL_IOT = listIot.Count();
            }

            //If show type is equal 2 then get list machine count by machines hours
            if (showType == "2")
            {
                foreach (var mccn in listMc)
                {
                    var listFwts = FwtsBus.GetWorkingTimeOraMES(mccn.FACTORY, yyyy, mm, dd);
                    float totalHours = 0;
                    foreach (var fwts in listFwts)
                    {
                        totalHours += fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                    }

                    mccn.MACHINE_COUNT_DGS *= (int)totalHours;
                    mccn.MACHINE_COUNT_MES *= (int)totalHours;
                    mccn.MACHINE_COUNT_PKG *= (int)totalHours;
                    mccn.TOTAL_IOT *= (int)totalHours;
                }
            }

            return Json(new SuccessTaskResult<List<Mccn>>(listMc), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMaxMachineCountByMonth(string yyyy, string mm, string dd, string weekNo, string showType)
        {
            //var tasks = new[]
            //{
            //    Task.Run(() => UpdateMachineCountDaily(yyyy, mm, dd, weekNo))
            //};

            var listMaxMc = MccnBus.GetMaxMachineCountByMonth(ConstantGeneric.ServerNo, yyyy, mm);

            //Get total of Iot from DGS
            foreach (var fac in listMaxMc)
            {
                //var facName = FactoryDgs.ListFactoryDgs().Where(f => f.FactoryId == fac.FACTORY).FirstOrDefault();
                //var listIot = IotMappingBus.GetListIotDgs(facName.FactoryName);

                var listIot = MchnDtlBus.GetListMachineTPM(fac.FACTORY, true);

                fac.TOTAL_IOT = listIot.Count();
            }

            //If show type is equal 2 then get list machine count by machines hours
            if (showType == "2")
            {
                foreach (var mccn in listMaxMc)
                {
                    var listFwts = FwtsBus.GetLineWorkingTimeFactoryOraMES(mccn.FACTORY, yyyy, mm);
                    float totalHours = 0;
                    foreach (var fwts in listFwts)
                    {
                        totalHours += fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                    }

                    mccn.MACHINE_COUNT_DGS *= (int)totalHours;
                    mccn.MACHINE_COUNT_MES *= (int)totalHours;
                    mccn.MACHINE_COUNT_PKG *= (int)totalHours;
                    mccn.TOTAL_IOT *= (int)totalHours;
                }
            }

            return Json(new SuccessTaskResult<List<Mccn>>(listMaxMc), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMaxMachineCountByWeek(string yyyy, string mm, string dd, string weekNo, string showType)
        {
            //var tasks = new[]
            //{
            //    Task.Run(() => UpdateMachineCountDaily(yyyy, mm, dd, weekNo))
            //};

            var listMaxMc = MccnBus.GetMaxMachineCountByWeek(ConstantGeneric.ServerNo, yyyy, weekNo);

            //Get total of Iot from DGS
            foreach (var fac in listMaxMc)
            {
                //var facName = FactoryDgs.ListFactoryDgs().Where(f => f.FactoryId == fac.FACTORY).FirstOrDefault();
                //var listIot = IotMappingBus.GetListIotDgs(facName.FactoryName);

                //Get list of Iot base on MAC address
                var listIot = MchnDtlBus.GetListMachineTPM(fac.FACTORY, true);
                fac.TOTAL_IOT = listIot.Count();
            }

            //If show type is equal 2 then get list machine count by machines hours
            if (showType == "2")
            {
                foreach (var mccn in listMaxMc)
                {
                    var listFwts = FwtsBus.GetWorkingTimeByWeekOraMES(mccn.FACTORY, yyyy, weekNo);
                    float totalHours = 0;
                    foreach (var fwts in listFwts)
                    {
                        totalHours += fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                    }

                    mccn.MACHINE_COUNT_DGS *= (int)totalHours;
                    mccn.MACHINE_COUNT_MES *= (int)totalHours;
                    mccn.MACHINE_COUNT_PKG *= (int)totalHours;
                    mccn.TOTAL_IOT *= (int)totalHours;
                }
            }

            return Json(new SuccessTaskResult<List<Mccn>>(listMaxMc), JsonRequestBehavior.AllowGet);
        }

        public bool UpdateMachineCountDaily(string yyyy, string mm, string dd, string weekNo)
        {
            try
            {
                //Check date update. 
                //Let update in last 2 date, because dgs database keep only 2 last date data
                var last2date = DateTime.Now.AddDays(-2).Date;
                var searchingDate = DateTime.ParseExact(yyyy + "-" + mm + "-" + dd, "yyyy-MM-dd",
                                       System.Globalization.CultureInfo.InvariantCulture).Date;

                if (searchingDate < last2date) return false;

                //Get list of factories by server no
                //var listFac = CsdtBus.GetListFactories(ConstantGeneric.ServerNo);
                var listFac = OPS_DAL.MesBus.FcmtBus.GetFactories(null);

                //Get list machine count in MES
                var listMcCount = MccnBus.GetListMachineCount(null, yyyy, mm, dd, ConstantGeneric.ServerNo);

                //Get list of machine sent data to DGS
                var listIotDgs = OhisBus.GetListMachineDGS(yyyy, mm, dd);
                var isUpdate = false;

                //Create new list factory machine
                var listMccn = new List<Mccn>();

                //Iterate list of factory
                foreach (var fac in listFac)
                {
                    //Get list of IoT from TPM
                    var listIotTpm = MchnDtlBus.GetListMachineTPM(fac.Factory, true);

                    //Get list runing Iot
                    var listRunningIot = MappingIotDgsAndTpm(listIotTpm, listIotDgs);

                    //Get list of machine are scanned by process
                    var pkgMc = OpdtMcBus.GetMachineScanMesPkg(fac.Factory, yyyy, mm, dd);

                    //Get list machine by factory from TPM database
                    var listMcTPM = MchnDtlBus.GetListMachineTPM(fac.Factory, false);

                    //Check difference data to update
                    var mesMcn = listMcCount.Find(m => m.FACTORY == fac.Factory);
                    isUpdate = isUpdate == true ? true : CheckMachineCountForUpdate(mesMcn, listIotDgs, pkgMc, listMcTPM);

                    var mccn = new Mccn()
                    {
                        FACTORY = fac.Factory,
                        YEAR_COUNT = yyyy,
                        MONTH_COUNT = mm,
                        DAY_COUNT = dd,
                        WEEKNO = weekNo,
                        MACHINE_COUNT_DGS = listRunningIot == null ? 0 : listRunningIot.Count(),
                        MACHINE_COUNT_MES = listMcTPM == null ? 0 : listMcTPM.Count(),
                        MACHINE_COUNT_PKG = pkgMc.Count()
                    };
                    listMccn.Add(mccn);
                }

                //Don't need to update
                if (!isUpdate) return false;

                //Insert list of machine count
                var updSta = MccnBus.InsertListMachineCount(listMccn);

                return updSta;
            }
            catch (Exception ex)
            {
                //Record action
                //Insert log: Update machine count
                InsertActionLog(false, "UMC", ConstantGeneric.ActionUpdate, yyyy + mm + dd + weekNo, ex.Message);

                return false;
            }

        }

        public JsonResult UpdateMachineCount(string yyyy, string mm, string dd, string weekNo)
        {
            try
            {
                //Update machine count daily
                var updSta = UpdateMachineCountDaily(yyyy, mm, dd, weekNo);

                //Record action
                //Insert log: Update machine count
                InsertActionLog(updSta, "UMC", ConstantGeneric.ActionUpdate, yyyy + mm + dd + weekNo, "Update machine count");

                return Json(new SuccessTaskResult<string>("Updated"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //Insert log: Update machine count
                InsertActionLog(false, "UMC", ConstantGeneric.ActionUpdate, yyyy + mm + dd + weekNo, "Update machine count");

                return Json(new FailedTaskResult<string>(ex.Message), JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult GetIotDataFromDgsTpm(string yyyy, string mm, string dd, string weekNo, string showType)
        {
            //Get list of factories
            //var listFac = CsdtBus.GetListFactories(ConstantGeneric.ServerNo);
            var listFac = OPS_DAL.MesBus.FcmtBus.GetFactories(null);

            //Get list of machine sent data to DGS
            var listIotDgs = OhisBus.GetListMachineDGS(yyyy, mm, dd);

            //Create new list factory machine
            var listMccn = new List<Mccn>();

            foreach (var fac in listFac)
            {
                //Get factory name DGS
                //var facName = FactoryDgs.ListFactoryDgs().Where(f => f.FactoryId == fac.Factory).FirstOrDefault().FactoryName;

                //Get list of Iot in each factory
                //var listIot = IotMappingBus.GetListIotDgs(facName);

                var listIotTpm = MchnDtlBus.GetListMachineTPM(fac.Factory, true);

                //Get list machine by factory from TPM database
                var listMcTPM = MchnDtlBus.GetListMachineTPM(fac.Factory, false);

                //Get running Iot which send data to DGS by factory
                //var listRunningIot = listMcDgs.FindAll(i => i.Factory == facName);
                var listRunningIot = MappingIotDgsAndTpm(listIotTpm, listIotDgs);

                //Get scanned iot
                var pkgMc = OpdtMcBus.GetMachineScanMesPkg(fac.Factory, yyyy, mm, dd);

                //Count machine
                var totalMcTPM = listMcTPM == null ? 0 : listMcTPM.Count(); //Total machine in TPM
                var totalIot = listIotTpm.Count(); //Total IoT in DGS database: t_dg_mchn_iot_mapping
                var runningIot = listRunningIot == null ? 0 : listRunningIot.Count(); //Iot send data to DGS: t_dg_iot_event_counter
                var scannedIot = pkgMc.Count(); //List Iot are scaned by operation

                if (showType == "2")
                {
                    //Get working time sheet
                    var listFwts = FwtsBus.GetWorkingTimeByWeekOraMES(fac.Factory, yyyy, weekNo);
                    float totalHours = 0;
                    foreach (var fwts in listFwts)
                    {
                        totalHours += fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                    }

                    totalMcTPM *= (int)totalHours;
                    totalIot *= (int)totalHours;
                    runningIot *= (int)totalHours;
                    scannedIot *= (int)totalHours;
                }

                var mccn = new Mccn()
                {
                    FACTORY = fac.Factory,
                    YEAR_COUNT = yyyy,
                    MONTH_COUNT = mm,
                    DAY_COUNT = dd,
                    WEEKNO = weekNo,
                    MACHINE_COUNT_DGS = runningIot,
                    MACHINE_COUNT_MES = totalMcTPM,
                    MACHINE_COUNT_PKG = scannedIot,
                    TOTAL_IOT = totalIot
                };
                listMccn.Add(mccn);
            }

            return Json(new SuccessTaskResult<List<Mccn>>(listMccn), JsonRequestBehavior.AllowGet);
        }

        private bool CheckMachineCountForUpdate(Mccn mesMccn, List<Ohis> listIotDgs, List<OpdtMc> listIotScanned, List<MchnDtl> listMcTpm)
        {
            //If MES machine count is null then return true.
            if (mesMccn == null) return true;

            //If the number Iot in dgs database, scanned IoT or total of machine in TPM are different with database then update it
            if (mesMccn.MACHINE_COUNT_DGS != listIotDgs.Count()
                || mesMccn.MACHINE_COUNT_PKG != listIotScanned.Count()
                || mesMccn.MACHINE_COUNT_MES != listMcTpm.Count())
                return true;

            return false;
        }

        private List<Ohis> MappingIotDgsAndTpm(List<MchnDtl> listIotTpm, List<Ohis> listIotDgs)
        {
            var listMapIot = new List<Ohis>();

            foreach (var iotTpm in listIotTpm)
            {
                var iotDgs = listIotDgs.Find(i => i.MacAddress == iotTpm.MAC);

                if (iotDgs != null) listMapIot.Add(iotDgs);
            }

            return listMapIot;
        }

        private void InsertActionLog(bool actStatus, string functionId, string operationId, string refNo, string remark)
        {
            var isSuccess = actStatus ? "1" : "0";

            ActlBus.AddTransactionLog(UserInf.UserName, UserInf.RoleID, functionId, operationId, isSuccess, ConstantGeneric.MesMachineDBId, ConstantGeneric.MesSystemId, refNo, remark);

        }
    }
}