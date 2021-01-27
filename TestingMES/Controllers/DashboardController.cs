using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using OPS_Utils;
using PKERP.Base.Domain.Interface.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

using TestingMES.CommonClass;

namespace TestingMES.Controllers
{
    [SessionTimeout]
    public class DashboardController : Controller
    {
        public Usmt UserInf => (Usmt)Session["LoginUser"];

        // GET: Dashboard
        public ActionResult Dashboard()
        {
            ViewBag.PageTitle = "Machine Opeartion";
            ViewBag.SubPageTitle = "<span>> Machine Opeartion</span>";

            return View();
        }

        public ActionResult Default()
        {
            ViewBag.PageTitle = "Defautl";
            ViewBag.SubPageTitle = "<span>> Default</span>";

            return View();
        }

        public JsonResult GetMaxMachineCountByMonth(string yyyy, string mm, string dd, string weekNo, string showType)
        {
            var tasks = new[]
            {
                Task.Run(() => UpdateMachineCountDaily(yyyy, mm, dd, weekNo, false))
            };

            var listMaxMc = MccnBus.GetMaxMachineCountByMonth(ConstantGeneric.ServerNo, yyyy, mm);
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
                }
            }

            return Json(new SuccessTaskResult<List<Mccn>>(listMaxMc), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMaxMachineCountByWeek(string yyyy, string mm, string dd, string weekNo, string showType)
        {
            var tasks = new[]
            {
                Task.Run(() => UpdateMachineCountDaily(yyyy, mm, dd, weekNo, false))
            };

            var listMaxMc = MccnBus.GetMaxMachineCountByWeek(ConstantGeneric.ServerNo, yyyy, weekNo);

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
                }
            }

            return Json(new SuccessTaskResult<List<Mccn>>(listMaxMc), JsonRequestBehavior.AllowGet);
        }

        public bool UpdateMachineCountDaily(string yyyy, string mm, string dd, string weekNo, bool isUpdate)
        {
            return true;

            //try
            //{
            //    //Get list of factories by server no
            //    var listFac = CsdtBus.GetListFactories(ConstantGeneric.ServerNo);

            //    //Check if it is not a update function then checking machine count was updated or not
            //    if (!isUpdate)
            //    {
            //        //There is no factory
            //        if (listFac.Count() == 0) return false;

            //        //Get list machine count in MES
            //        var listMc = MccnBus.GetListMachineCount(listFac[0].Factory, yyyy, mm, dd, ConstantGeneric.ServerNo);
            //        //If machine count have updated then do not need to update
            //        if (listMc.Count() > 0 && listMc[0].MACHINE_COUNT_DGS != 0)
            //        {
            //            return false;
            //        }
            //    }

            //    //Get list of machine sent data to DGS
            //    var listMcDgs = OhisBus.GetListMachineDGS(yyyy, mm, dd);
            //    //if (listMcDgs.Count == 0)
            //    //{
            //    //    return false;
            //    //}

            //    //Create new list factory machine
            //    var listMccn = new List<Mccn>();

            //    //Iterate list of factory
            //    foreach (var fac in listFac)
            //    {
            //        //Get list machine by factory from TPM database
            //        var listMcTPM = MchnDtlBus.GetListMachine(fac.Factory);
            //        //Create new list of history machine.
            //        var listMcDgsFac = new List<Ohis>();
            //        foreach (var mc in listMcDgs)
            //        {
            //            //Get machine in DGS by comparing with machine id on TPM side
            //            var mcTPM = listMcTPM.Where(x => x.MCHN_DTL_CD == mc.MCHN_ID);
            //            if (mcTPM.Count() > 0 && mcTPM != null)
            //            {
            //                listMcDgsFac.Add(mc);
            //            }
            //        }

            //        var pkgMc = OpdtMcBus.GetMachineScanMesPkg(fac.Factory, yyyy, mm, dd);

            //        var mccn = new Mccn()
            //        {
            //            FACTORY = fac.Factory,
            //            YEAR_COUNT = yyyy,
            //            MONTH_COUNT = mm,
            //            DAY_COUNT = dd,
            //            WEEKNO = weekNo,
            //            MACHINE_COUNT_DGS = listMcDgsFac == null ? 0 : listMcDgsFac.Count(),
            //            MACHINE_COUNT_MES = listMcTPM == null ? 0 : listMcTPM.Count(),
            //            MACHINE_COUNT_PKG = pkgMc.Count()
            //        };
            //        listMccn.Add(mccn);
            //    }

            //    //Insert list of machine count
            //    var updSta = MccnBus.InsertListMachineCount(listMccn);

            //    return updSta;
            //}
            //catch (Exception ex)
            //{
            //    //Record action
            //    //Insert log: Update machine count
            //    InsertActionLog(false, "UMC", ConstantGeneric.ActionUpdate, yyyy + mm + dd + weekNo, ex.Message);

            //    return false;
            //}

        }

        public JsonResult UpdateMachineCount(string yyyy, string mm, string dd, string weekNo)
        {
            try
            {
                //Update machine count daily
                var updSta = UpdateMachineCountDaily(yyyy, mm, dd, weekNo, true);

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

        private void InsertActionLog(bool actStatus, string functionId, string operationId, string refNo, string remark)
        {
            var isSuccess = actStatus ? "1" : "0";

            ActlBus.AddTransactionLog(UserInf.UserName, UserInf.RoleID, functionId, operationId, isSuccess, ConstantGeneric.MesMachineDBId, ConstantGeneric.MesSystemId, refNo, remark);

        }
    }


}