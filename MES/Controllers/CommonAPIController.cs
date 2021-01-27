using System; 
using System.Web.Mvc; 

using OPS_DAL.QCOBus; 
 
namespace MES.Controllers
{
    [SessionTimeout]
    public class CommonAPIController : Controller
    { 
        public ActionResult GetWarehouseList()
        { 
            return Json(WHMTBus.GetWarehouseMasterList("PatternWarehouse"), JsonRequestBehavior.AllowGet);
        }
         
        public ActionResult GetFactoryList(string vstrFactory)
        {
            return Json(OPS_DAL.MesBus.FcmtBus.GetFactories(vstrFactory), JsonRequestBehavior.AllowGet);
        }
         
        public ActionResult GetBuyerList(string vstrFactory)

        {
            return Json(OPS_DAL.MesBus.FcmtBus.GetFactories(vstrFactory), JsonRequestBehavior.AllowGet);
        }


#region Used Internally
        //Author: Son Nguyen Cao
        public ActionResult GetCstpByServerNo()
        {
            return Json(OPS_DAL.MesBus.CstpBus.GetCstpByServerNo(OPS_Utils.ConstantGeneric.ServerNo), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetWorkingTimeSheet(string factoryId, string planYear, string planMonth, string planDay)
        {
            return Json(OPS_DAL.MesBus.FwtsBus.GetWorkingTimeOraMES(factoryId, planYear, planMonth, planDay), JsonRequestBehavior.AllowGet);
        }

         
        //Author: Tai Le(Thomas)
        public JsonResult GetURLM()
        {
            return Json(OPS_DAL.Business.UrlmBus.GetMasterRoleList(), JsonRequestBehavior.AllowGet);
        }
#endregion

    }
}