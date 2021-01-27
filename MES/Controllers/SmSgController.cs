using MES.GenericClass;
using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_Utils;
using System;
using System.Web.Mvc;

namespace MES.Controllers
{
    public class SmSgController : Controller
    {
        public string MenuId => ConstantGeneric.OpSmSgMenuId;
        public string SystemOpsId => ConstantGeneric.OpsSystemId;
        Usmt UserInf => (Usmt)Session["LoginUser"];
        Srmt Role => SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, MenuId);
        private readonly SmSgBus _smSg = new SmSgBus();

        public ActionResult GetMsgById(int id)
        {
            var smSgs = _smSg.GetByID(id.ToString());
            return Json(smSgs, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMsgByItem(string contextSerial, string sysId, string menuId, string funt, string type,
            string context)
        {
            try
            {
                var smSgs = _smSg.GetByItem(new SmSg { ContextSerial = contextSerial, SystemId = sysId, MenuId = menuId,
                                Function = funt, MessageType = type, MessageContext = context }) ?? new SmSg();
                return Json(smSgs, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventDelete);
                return Json(new SmSg(), JsonRequestBehavior.AllowGet);
            }
        }
    }
}