using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_Utils;
using PKERP.Base.Domain.Interface.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MES.Controllers
{
    [SessionTimeout]
    public class StyleGroupMachineController : Controller
    {
        public Usmt UserInf => (Usmt)Session["LoginUser"];

        // GET: StyleGroupMachine
        public ActionResult StyleGroupMachine()
        {
            return View();
        }

        public JsonResult GetMachines(string categoryId)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
                return Json(new FailedTaskResult<List<Otmt>>("Category cannot empty"), JsonRequestBehavior.AllowGet);

            var listMachines = OtmtBus.GetOtmtsByCateGid(categoryId, 1);

            return Json(new SuccessTaskResult<IEnumerable<Otmt>>(listMachines), JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddStyleGroupMachines(List<Sgmc> listSgmc)
        {
            try
            {
                if (listSgmc == null || listSgmc.Count() == 0)
                    return Json(new FailedTaskResult<string>("List of machine cannot empty"), JsonRequestBehavior.AllowGet);

                //Check existing machines
                var styleGroup = listSgmc[0].STYLEGROUP;
                var subGroup = listSgmc[0].SUBGROUP;
                var subSubGroup = listSgmc[0].SUBSUBGROUP;
                //Get list of existing style group machine
                var listExistSgmc = SgmcBus.GetStyleGroupMachines(styleGroup, subGroup, subGroup);
                foreach (var sgmc in listSgmc)
                {
                    var existSgmc = listExistSgmc.Find(s => s.STYLEGROUP == styleGroup && s.SUBGROUP == subGroup && s.SUBSUBGROUP == subSubGroup && s.MACHINEID == sgmc.MACHINEID);
                    if (existSgmc != null)
                    {
                        return Json(new FailedTaskResult<string>("Machine was existing: " + existSgmc.MACHINEID + " - " + existSgmc.MACHINENAME), JsonRequestBehavior.AllowGet);
                    }

                }

                var resAdd = SgmcBus.InsertListStlGroupMachine(listSgmc);

                //Insert log: CSMC - Create style group machine
                InsertActionLog(resAdd, "CSMC", ConstantGeneric.ActionUpdate, listSgmc[0].STYLEGROUP + listSgmc[0].SUBGROUP + listSgmc[0].SUBSUBGROUP, "Create style group machine");

                if (resAdd)
                    return Json(new SuccessTaskResult<string>("Added"), JsonRequestBehavior.AllowGet);

                return Json(new FailedTaskResult<string>("Adding machines fail"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new FailedTaskResult<string>(ex.Message), JsonRequestBehavior.AllowGet);
            }


        }

        //Get list of machines by style group
        public JsonResult GetStyleGroupMachines(string styleGroup, string subGroup, string subSubGroup)
        {

            if (string.IsNullOrWhiteSpace(styleGroup))
                return Json(new FailedTaskResult<List<Sgmc>>("Style group cannot empty"), JsonRequestBehavior.AllowGet);

            var listSgmc = SgmcBus.GetStyleGroupMachines(styleGroup, subGroup, subSubGroup);

            return Json(listSgmc, JsonRequestBehavior.AllowGet);
        }

        private void InsertActionLog(bool actStatus, string functionId, string operationId, string refNo, string remark)
        {
            var isSuccess = actStatus ? "1" : "0";

            ActlBus.AddTransactionLog(UserInf.UserName, UserInf.RoleID, functionId, operationId, isSuccess, ConstantGeneric.MesPplMenuId, ConstantGeneric.MesSystemId, refNo, remark);

        }

    }
}