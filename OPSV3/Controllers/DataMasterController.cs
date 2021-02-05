﻿using OPS_DAL.Business;
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
            //remove works "Manager of"
            foreach (var team in listTeams)
            {
                team.RoleDesc = team.RoleDesc.Replace("Manager of ", "");
            }
            return Json(listTeams, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProductTeams()
        {
            //get list of sale team
            var listTeams = FactoryBus.GetDevelopmentTeam();
            return Json(listTeams, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBuyersByTeam(string teamId)
        {
            var listAcc = McmtBus.GetBuyersBySaleTeam(teamId);
            //If team id is null or empty then get all buyer
            if (string.IsNullOrEmpty(teamId)) return Json(listAcc, JsonRequestBehavior.AllowGet);

            //filter buyer by team id
            var listAccByTeam = listAcc.FindAll(x => x.CodeDesc == teamId);
            return Json(listAccByTeam, JsonRequestBehavior.AllowGet);
        }
      
        public JsonResult GetBuyersByFactory(string factoryId)
        {
            var listAcc = McmtBus.GetBuyersByFactory(factoryId);

            return Json(listAcc, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetModuleColors()
        {
            var listModuleColors = OpColorBus.GetColour();

            return Json(listModuleColors, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetReasonOperationPlan()
        {
            var listReason = McmtBus.GetMasterCode("OPReason");

            return Json(listReason, JsonRequestBehavior.AllowGet);
        }
    }
}