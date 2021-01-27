using OPS_DAL.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OPSV3.Controllers
{
    public class DataMasterController : Controller
    {
        public JsonResult GetSaleTeams()
        {
            //get list of sale team
            var listTeams = UrlmBus.GetSaleTeams();
            return Json(listTeams, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProductTeams()
        {
            //get list of sale team
            var listTeams = FactoryBus.GetDevelopmentTeam();
            return Json(listTeams, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBuyers(string teamId)
        {
            var listAcc = McmtBus.GetMasterCode("Buyer");
            //If team id is null or empty then get all buyer
            if (string.IsNullOrEmpty(teamId)) return Json(listAcc, JsonRequestBehavior.AllowGet);

            //filter buyer by team id
            var listAccByTeam = listAcc.FindAll(x => x.CodeDesc == teamId);
            return Json(listAccByTeam, JsonRequestBehavior.AllowGet);
        }
    }
}