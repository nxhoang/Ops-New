using System;
using System.Web.Mvc;
using OPS.Models;
using OPS_DAL.Business;
using OPS_Utils;
using OPS_DAL.Entities;
using System.Web;
using System.Linq;
using OPS.GenericClass;

namespace OPS.Controllers
{
    public class OpsMasterDataController : Controller
    {
        public string MenuId => ConstantGeneric.ManageToolId;
        public string SystemOpsId => ConstantGeneric.OpsSystemId;
        Usmt UserInf => (Usmt)Session["LoginUser"];
        string screenId = ConstantGeneric.ScreenToolMaster;
        Srmt Role => SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, MenuId);

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Test()
        {
            return View();
        }

        //Author: Son Nguyen Cao
        public JsonResult GetOpMaster(GridSettings gridRequest, string styleCode, string styleSize, string styleColor, string revNo)
        {
            try
            {
                int pageIndex = gridRequest.pageIndex;
                int pageSize = gridRequest.pageSize;

                var opMaster = OpmtBus.GetOpMaster(styleCode, styleSize, styleColor, revNo, pageIndex, pageSize);
                decimal totalRecords = 0;
                if (opMaster.Count > 0) { totalRecords = opMaster[0].TotalRecords; }

                int modes = totalRecords % pageSize == 0 ? 0 : 1;
                var totalPage = totalRecords > 0 ? (totalRecords / pageSize) + modes : 1;
                var pagingOpMaster = new
                {
                    total = totalPage,
                    page = pageIndex,
                    records = totalRecords,
                    rows = opMaster.ToArray()
                };

                return Json(pagingOpMaster, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new object(), JsonRequestBehavior.AllowGet);
            }

        }

        #region Linking and tools

        public ActionResult Tools()
        {
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            //
            return View();
        }

        public ActionResult Machine()
        {
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        public JsonResult GetOtmtsMc(GridSettings gridRequest, string gId, int machine)
        {
            try
            {
                var otmts = OtmtBus.GetOtmtsByCateGid(gId, machine);
                var lisDataQ = otmts.AsQueryable();
                if (null != gridRequest.where && gridRequest.where.rules.Length > 0)
                {
                    string strWhere = LinqExtensionsMethod.FilterNullExpression(gridRequest);
                    if (string.IsNullOrEmpty(strWhere) == false)
                    {
                        var data = lisDataQ.Where(strWhere).ToList();
                        lisDataQ = data.AsQueryable();
                    }
                    strWhere = LinqExtensionsMethod.GetAllStringFiltersExpression(gridRequest);
                    if (string.IsNullOrEmpty(strWhere) == false)
                    {
                        lisDataQ = lisDataQ.Where(strWhere);
                    }
                }
                lisDataQ = lisDataQ?.OrderBy(gridRequest.sortColumn, gridRequest.sortOrder);
                return Json(lisDataQ.ToArray(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        public int UpdateMachine(Otmt otmt, string type, string newSrc)
        {
            try
            {
                if (!CommonMethod.CheckRole(Role.IsUpdate))
                {
                    return ReportAction.RoleFail;
                }
                otmt.Machine = type;
                if (!string.IsNullOrEmpty(newSrc))
                {
                    otmt.ImagePath = newSrc;
                }
                OtmtBus.Update(otmt);
                CommonUtility.InsertLogActivity(otmt.ItemCode, UserInf, SystemOpsId, screenId, ConstantGeneric.EventEdit, "Delete OTMT.", "1");
                return ReportAction.Success;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventEdit);
                throw new Exception(msg);
                //return ReportAction.Error;
            }

        }

        public int AddMachine(Otmt otmt, string type, string newSrc, HttpPostedFileBase ImagePath)
        {
            try
            {
                if (!CommonMethod.CheckRole(Role.IsAdd))
                {
                    return ReportAction.RoleFail;
                }
                otmt.Machine = type;
                otmt.ImagePath = newSrc;
                otmt.Registrar = UserInf.UserName;
                OtmtBus.AddMachine(otmt);
                CommonUtility.InsertLogActivity(otmt.ItemCode, UserInf, SystemOpsId, screenId, ConstantGeneric.EventAdd, "Add machine.", "1");
                return ReportAction.Success;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventAdd);
                throw new Exception(msg);
            }
        }

        public JsonResult GetAutomaticCode(int isMachine)
        {
            try
            {
                return Json(OtmtBus.GetAutomaticCode(isMachine), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        public int DeleteMachine(string itemcode)
        {
            try
            {
                if (!CommonMethod.CheckRole(Role.IsDelete))
                {
                    return ReportAction.RoleFail;
                }

                OtmtBus.DeleteMachine(itemcode);
                CommonUtility.InsertLogActivity(itemcode, UserInf, SystemOpsId, screenId, ConstantGeneric.EventDelete, "Delete OTMT.", "1");
                return ReportAction.Success;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        public int CheckReferenceKey(string itemcode)
        {
            if (OtmtBus.CheckReferenceKey(itemcode))
            {
                return 1;
            }
            return 0;
        }

        public JsonResult GetMasterCodeByCode(int isMachine)
        {
            try
            {
                var arrMasterCode = OtmtBus.GetCategroy(isMachine);
                return Json(arrMasterCode, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        public JsonResult GetSuppiers()
        {
            return Json(SsCmBus.GetSuppiers(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBrand()
        {
            return Json(McmtBus.GetMasterCode("MachineBrand"), JsonRequestBehavior.AllowGet);
        }
        #endregion Linking and tools

        public JsonResult GetOperationGroup(string groupLevel, string parentId)
        {
            //Set parent Id is -1 if group level is difference with 0 and parent id is empty
            if (groupLevel != "0") parentId = string.IsNullOrEmpty(parentId) ? "-1" : parentId;

            return Json(OpnmBus.GetOperationGroup(groupLevel, parentId), JsonRequestBehavior.AllowGet);
        }
    }
}