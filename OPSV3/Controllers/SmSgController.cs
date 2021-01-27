using OPS.GenericClass;
using OPS.Models;
using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_Utils;
using System;
using System.Linq;
using System.Web.Mvc;

namespace OPS.Controllers
{
    public class SmSgController : Controller
    {
        public string MenuId => ConstantGeneric.OpSmSgMenuId;
        public string SystemOpsId => ConstantGeneric.OpsSystemId;
        Usmt UserInf => (Usmt)Session["LoginUser"];
        Srmt Role => SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, MenuId);
        SmSgBus _smSg = new SmSgBus();
        // GET: SmSg
        public ActionResult Index()
        {
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        public ActionResult GetListSmSg(GridSettings gridRequest)
        {
            try
            {
                decimal totalRecords = 0;
                var lisData = _smSg.GetAll().Where(d=>d.Status == 1);
                var lisDataQ = lisData.AsQueryable();
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
                totalRecords = lisDataQ.Count();
                lisDataQ = lisDataQ?.OrderBy(gridRequest.sortColumn, gridRequest.sortOrder);
                int pageIndex = gridRequest.pageIndex;
                int pageSize = gridRequest.pageSize;
                int modes = totalRecords % pageSize == 0 ? 0 : 1;
                var intTotalPage = totalRecords > 0 ? (totalRecords / pageSize) + modes : 1;
                if (lisDataQ != null)
                {
                    var result = new
                    {
                        total = intTotalPage,
                        page = pageIndex,
                        records = totalRecords,
                        rows = lisDataQ.ToArray()
                    };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        public int AddSmSg(SmSg smSg)
        {
            try
            {
                if (!CommonMethod.CheckRole(Role.IsAdd))
                {
                    return ReportAction.RoleFail;
                }
                //osysId, omenuId, oeven, otype
                string maxSerial = _smSg.GetMaxId(smSg);
                smSg.ContextSerial = maxSerial;
                smSg.RegisterId = UserInf.UserName;
                var result = _smSg.Add(smSg);
                CommonUtility.InsertLogActivity(smSg.ContextSerial, UserInf, SystemOpsId, MenuId, ConstantGeneric.EventAdd, "Add SMSG.", result.ToString());

                return ReportAction.Success;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventAdd);
                return ReportAction.Error;
            }
        }

        public int UpdateSmSg(SmSg smSg, int id)
        {
            try
            {
                //smSg.ContextSerial = id;
                if (!CommonMethod.CheckRole(Role.IsUpdate))
                {
                    return ReportAction.RoleFail;
                }
                //if (!CompareSmSg(smSg, osysId, omenuId, oeven, otype, ocontext))
                //{
                //    var smsS = _smSg.GetByItem(smSg);
                //    if (smsS != null)
                //    {
                //        return ReportAction.Duplicate;
                //    }
                //}
                smSg.UpdateId = UserInf.UserName;
                var result = _smSg.Update(smSg);
                CommonUtility.InsertLogActivity(smSg.ContextSerial.ToString(), UserInf, SystemOpsId, MenuId, ConstantGeneric.EventEdit, "Edit SMSG.", result.ToString());
                return ReportAction.Success;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventEdit);
                throw new Exception(msg);
            }
        }

        bool CompareSmSg(SmSg smSg, string sysId, string menuId, string funt, string type, string ocontext)
        {
            return smSg.SystemId == sysId && smSg.MenuId == menuId && smSg.Function == funt && smSg.MessageType == type && smSg.MessageContext == ocontext;
        }

        public int DelSmSg(SmSg smSg)
        {
            try
            {
                if (!CommonMethod.CheckRole(Role.IsDelete))
                {
                    return ReportAction.RoleFail;
                }
                var result = _smSg.Delete(smSg);
                string idLog = smSg.ContextSerial + smSg.SystemId + smSg.MenuId +smSg.Function+ smSg.MessageType + smSg.MessageContext;
                CommonUtility.InsertLogActivity(idLog, UserInf, SystemOpsId, MenuId, ConstantGeneric.EventDelete, "Delete SMSG.", result.ToString());
                return ReportAction.Success;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventDelete);
                return ReportAction.Error;
            }
        }

        public ActionResult GetMsgById(int id)
        {
            var smSgs = _smSg.GetByID(id.ToString());
            return Json(smSgs, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMsgByItem(string contextSerial, string sysId, string menuId, string funt, string type, string context)
        {
            try
            {
                var smSgs = _smSg.GetByItem(new SmSg() { ContextSerial = contextSerial, SystemId = sysId, MenuId = menuId, Function = funt, MessageType = type, MessageContext = context });
                if(smSgs == null)
                {
                    smSgs = new SmSg();
                }
                return Json(smSgs, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventDelete);
                return Json(new SmSg(), JsonRequestBehavior.AllowGet);
            }
        }
        #region Load combobox
        public ActionResult GetSystem()
        {
            return Json(_smSg.GetAllSystem(), JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetMenu()
        {
            return Json(_smSg.GetAllMenu(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFunction()
        {
            return Json(_smSg.GetFunction(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSmSgType()
        {
            return Json(McmtBus.GetSmSgTyPe(), JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult GetContext()
        {
            return Json(McmtBus.GetSmSgContext(), JsonRequestBehavior.AllowGet);
        }
        #endregion Load combobox
    }
}