using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using OPS_Utils;
using PKERP.Base.Domain.Interface.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MES.Controllers
{
    [SessionTimeout]
    public class PackageReadinessController : Controller
    {
        public string MenuMesId => ConstantGeneric.MesPkgReadinessId;
        public string SystemMesId => ConstantGeneric.MesSystemId;
        public Usmt UserInf => (Usmt)Session["LoginUser"];
        public Srmt Role => SrmtBus.GetUserRoleInfoMySql(UserInf.RoleID, SystemMesId, MenuMesId);

        // GET: PackageReadiness
        public ActionResult PackageReadiness()
        {
            ViewBag.PageTitle = "Package Readiness";
            return View();
        }

        public JsonResult SendJigRequest(Prrd prrd)
        {
            try
            {
                if(prrd == null)
                {
                    return Json(new FailedTaskResult<string>("Request cannot be empty"), JsonRequestBehavior.AllowGet);
                }

                var resIns = PrrdBus.InsertPackageReadiness(prrd);

                //Insert Log
                InsertActionLog(resIns, "SendRequestJig()", ConstantGeneric.ActionCreate, prrd.PRDPKG, "Insert package readiness");

                return Json(new SuccessTaskResult<string>("Sent request"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //Insert Log
                InsertActionLog(false, "SendRequestJig()", ConstantGeneric.ActionCreate, prrd.PRDPKG, ex.Message);

                return Json(new FailedTaskResult<string>(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetPackageReadiness(string prdPkg)
        {
            var pkgReadiness = PrrdBus.GetPackageReadiness(prdPkg);

            return Json(new SuccessTaskResult<Prrd>(pkgReadiness), JsonRequestBehavior.AllowGet);
        }

        private void InsertActionLog(bool actStatus, string functionId, string operationId, string refNo, string remark)
        {
            var isSuccess = actStatus ? "1" : "0";

            ActlBus.AddTransactionLog(UserInf.UserName, UserInf.RoleID, functionId, operationId, isSuccess, MenuMesId, SystemMesId, refNo, remark);

        }
    }
}