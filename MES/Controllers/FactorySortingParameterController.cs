using System;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Mvc;

using MES.CommonClass;
using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_DAL.QCOBus;


namespace MES.Controllers
{
    [SessionTimeout]
    [AutologArribute]
    public class FactorySortingParameterController : Controller
    {
        public OPS_DAL.Entities.Usmt UserInf => (OPS_DAL.Entities.Usmt)Session["LoginUser"];
        public Srmt Role => SrmtBus.GetUserRoleInfoMySql(UserInf.RoleID, "MES", "QCO");

        // GET: FactorySortingParameter
        //[SysActionFilter(RoleID = "5000;5100;5110;5111;5113")]
        [SysActionFilter(SystemID = "MES", MenuID = "QCO", Action = "UPDATE")]
        public ActionResult Index()
        {
            ViewBag.PageTitle = "<i class=\"fa fa-cog\"></i>&nbsp;QCO";
            ViewBag.SubPageTitle = "&nbsp;<span>> QCO-Factory Setup</span>";

            return View();
        }

        //[AllowAnonymous]
        public ActionResult GetMasterSettings()
        {
            return Json(QcopBus.GetMasterSettings(), JsonRequestBehavior.AllowGet);
        }

        //[AllowAnonymous]
        public ActionResult GetFactoryList()
        {
            return Json(OPS_DAL.QCOBus.FcmtBus.GetFactories(""), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        //[AllowAnonymous]
        public ActionResult SaveFactorySettings(string vstrFactory, string vstrpostData)
        {
            string[] arrSelectedParameters = Regex.Split(vstrpostData, "_A2LF_");

            if (String.IsNullOrEmpty(vstrpostData) || String.IsNullOrEmpty(vstrFactory))
            {
                return Json(new
                {
                    retMsg = "Invalid Input<br/>" +
                             " Factory = \"" + vstrFactory + "\"<br/>" +
                             " Parameter List = \"" + vstrpostData + "\""
                }, JsonRequestBehavior.AllowGet);
            }

            if (!(Role.OwnerId == "9100"))
                return Json(new { retMsg = "Unauthorized." }, JsonRequestBehavior.AllowGet);


            if (QcfoBus.ClearAll(vstrFactory))
            {
                string strReturnMsg = "";

                for (int i = 0; i < arrSelectedParameters.Length; i++)
                {
                    if (QcfoBus.AddNew(vstrFactory, arrSelectedParameters[i], Session["LoginUserID"].ToString()))
                    {
                        if (strReturnMsg == "") strReturnMsg = "Setting \"" + arrSelectedParameters[i] + "\" For Factory \"" + vstrFactory + "\" Saved Successfully.";
                        else strReturnMsg = strReturnMsg + "<br/>Setting \"" + arrSelectedParameters[i] + "\" For Factory \"" + vstrFactory + "\" Saved Successfully.";
                    }
                    else
                    {
                        if (strReturnMsg == "") strReturnMsg = "Setting \"" + arrSelectedParameters[i] + "\" For Factory \"" + vstrFactory + "\" Saved : FAILED.";
                        else strReturnMsg = strReturnMsg + "<br/>Setting \"" + arrSelectedParameters[i] + "\" For Factory \"" + vstrFactory + "\" Saved : FAILED.";
                    }
                }

                return Json(new { retMsg = strReturnMsg }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { retMsg = "Clean the old Settings : FAILED." }, JsonRequestBehavior.AllowGet);
        }

        //[AllowAnonymous]
        public ActionResult EditFactorySetting()
        {
            string idDataValue = Url.RequestContext.RouteData.Values["id"].ToString();

            if (String.IsNullOrEmpty(idDataValue))
            {
                return Json(new
                {
                    retMsg = "Invalid Input:<br/>Factory = \"" + idDataValue + "\"<br/>"
                }, JsonRequestBehavior.AllowGet);
            }

            if (idDataValue != Session["LoginFactory"].ToString())
            {
                return Json(new { retResult = false, retMsg = "Selected Factory [" + idDataValue + "] Is Not Same Your Login Factory [" + Session["LoginFactory"].ToString() + "]." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { retResult = true, retMsg = "" }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult GetFactorySettings()
        {
            string idDataValue = Url.RequestContext.RouteData.Values["id"].ToString();

            if (String.IsNullOrEmpty(idDataValue))
            {
                return Json(new
                {
                    retMsg = "Invalid Input<br/>" +
                             " Factory = \"" + idDataValue + "\"<br/>"
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(QcfoBus.GetMasterSettings(idDataValue), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult FactorySettingsEdit()
        {
            string idDataValue = Url.RequestContext.RouteData.Values["id"].ToString();

            if (String.IsNullOrEmpty(idDataValue))
            {
                return Json(new
                {
                    retMsg = "Invalid Input<br/>" +
                             " Factory = \"" + idDataValue + "\"<br/>"
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(QcfoBus.GetMasterSettings(idDataValue), JsonRequestBehavior.AllowGet);
        }

        public ActionResult OpenFactorySettingEdit()
        {
            string idDataValue = Url.RequestContext.RouteData.Values["id"].ToString();
            ViewBag.id = idDataValue;
            return View("FactorySettingEdit");
        }




        /*Handle Crazy Action */
        protected override void HandleUnknownAction(string actionName)
        {
            try
            {
                this.View(actionName).ExecuteResult(this.ControllerContext);
            }
            catch
            {
                Response.Redirect("/SysPage/PageNotFound");
            }
        }

    }
}