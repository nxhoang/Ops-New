using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using TestingMES.CommonClass;

using OPS_DAL.QCOBus;

namespace TestingMES.Controllers
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
            return Json(OPS_DAL.MesBus.FcmtBus.GetFactories(""), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBuyerList(string vstrFactory)
        {
            return Json(OPS_DAL.MesBus.FcmtBus.GetFactories(""), JsonRequestBehavior.AllowGet);
        }

        //Author: Son Nguyen Cao
        public ActionResult GetCstpByServerNo()
        {
            return Json(OPS_DAL.MesBus.CstpBus.GetCstpByServerNo(OPS_Utils.ConstantGeneric.ServerNo), JsonRequestBehavior.AllowGet);
        }
    }


}