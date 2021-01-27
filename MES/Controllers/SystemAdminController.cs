using System;
using System.Data;
using System.Web.Mvc;
using System.Web;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using MES.Models;
using OPS_Utils;

using Oracle.ManagedDataAccess.Client;
using Newtonsoft.Json;


namespace MES.Controllers
{
    public class SystemAdminController: Controller
    {
        // GET: SystemAdmin
        public ActionResult Home()
        {
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //if (Session["LoginRole"].ToString() != "5000")
            //    return View("Read");

            ViewBag.PageTitle = "KPI Setting";
            ViewBag.SubPageTitle = "<span>&gt; KPI Setting</span>";

            return View("Home");
        }

        public ActionResult Export()
        {
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //if (Session["LoginRole"].ToString() != "5000")
            //    return View("Read");

            ViewBag.PageTitle = "KPI Setting";
            ViewBag.SubPageTitle = "<span>&gt; KPI Export</span>";

            return View("Export");
        }

        public ActionResult Read()
        {
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //if (Session["LoginRole"].ToString() != "5000")
            //    return View("Read");

            ViewBag.PageTitle = "KPI Setting";
            ViewBag.SubPageTitle = "<span>&gt; KPI Setting (Grid)</span>";

            var KPISettings = OPS_DAL.SystemBus.KPITeamBus.GetKPISettings(""); 
            return View(KPISettings);
        }

        #region GET THE SETTINGS 
        //[AllowAnonymous]
        public ActionResult GetBuyerList()
        {
            var objBuyer = new OPS_DAL.SystemBus.BuyerBus();
            var DBType = OPS_Utils.CommonMethod.GetXMLNodeValue(Server.MapPath("~/AppSetting.xml"), "/applicationSetting/DBType");

            var arrBuyer = objBuyer.GetBuyerWithAny(DBType).ToArray();
            return Json(arrBuyer, JsonRequestBehavior.AllowGet);
        }

        //[AllowAnonymous]
        public ActionResult GetSystemList()
        {
            var DBType = OPS_Utils.CommonMethod.GetXMLNodeValue(Server.MapPath("~/AppSetting.xml"), "/applicationSetting/DBType");

            var arrSystem = OPS_DAL.SystemBus.SystemBus.GetSystemList(DBType).ToArray();
            return Json(arrSystem, JsonRequestBehavior.AllowGet);
        }

        //[AllowAnonymous]
        public ActionResult GetSystemMenuList()
        {
            var DBType = OPS_Utils.CommonMethod.GetXMLNodeValue(Server.MapPath("~/AppSetting.xml"), "/applicationSetting/DBType");

            var arrSystemMenu = OPS_DAL.SystemBus.MenuBus.GetSystemMenuList(DBType).ToArray();
            return Json(arrSystemMenu, JsonRequestBehavior.AllowGet);
        }

        //[AllowAnonymous]
        public ActionResult GetUsers()
        {
            var keyword = Request.QueryString["keyword"] == null ? "" : Request.QueryString["keyword"].ToString(); //Url.RequestContext.RouteData.Values["keyword"] == null ? "" : Url.RequestContext.RouteData.Values["keyword"].ToString();
            var DBType = OPS_Utils.CommonMethod.GetXMLNodeValue(Server.MapPath("~/AppSetting.xml"), "/applicationSetting/DBType");

            //if (!String.IsNullOrEmpty(keyword))
            //{
            var arrUsers = OPS_DAL.Business.UsmtBus.GetUserList(keyword, DBType).ToArray();
            return Json(arrUsers, JsonRequestBehavior.AllowGet);
            //}
            //else
            //    return Json(new List<OPS_DAL.Entities.Usmt>(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetHRMEmployees()
        {
            var keyword = Request.QueryString["keyword"] == null ? "" : Request.QueryString["keyword"].ToString(); //Url.RequestContext.RouteData.Values["keyword"] == null ? "" : Url.RequestContext.RouteData.Values["keyword"].ToString();
            var DBType = OPS_Utils.CommonMethod.GetXMLNodeValue(Server.MapPath("~/AppSetting.xml"), "/applicationSetting/DBType");

            if (!String.IsNullOrEmpty(keyword))
            {
                var arrUsers = OPS_DAL.Business.UsmtBus.GetHRMEmployeeList(keyword, DBType); 
                return Json(arrUsers.ToArray() , JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new List<OPS_DAL.Entities.Usmt>(), JsonRequestBehavior.AllowGet);
        }
         
        public ActionResult GetCorporation()
        {
            var arrCorp = OPS_DAL.SystemBus.CorporationBus.GetCorporationList().ToArray();
            return Json(arrCorp, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSaleTeam()
        {
            var DBType = OPS_Utils.CommonMethod.GetXMLNodeValue(Server.MapPath("~/AppSetting.xml"), "/applicationSetting/DBType"); 
            var arrCorp = OPS_DAL.SystemBus.SaleTeamBus.GetSaleTeamList(DBType).ToArray();
            return Json(arrCorp, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFactoryList()
        {
            var DBType = OPS_Utils.CommonMethod.GetXMLNodeValue(Server.MapPath("~/AppSetting.xml"), "/applicationSetting/DBType");

            return Json(OPS_DAL.QCOBus.FcmtBus.GetFactoriesWithAny(DBType), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetKPITeamList()
        {
            var DBType = OPS_Utils.CommonMethod.GetXMLNodeValue(Server.MapPath("~/AppSetting.xml"), "/applicationSetting/DBType");

            return Json(OPS_DAL.SystemBus.KPITeamBus.GetKPITeamList(DBType), JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region ADD KPI SETTING
        /*Add New KPI Setting*/
        [HttpPost]
        public ActionResult AddNewKPISetting(string selected_System, string selected_Corporation, string selected_KPITeam, string KPISeniorData,
            string selected_Buyer, string selected_Factory, string KPIJuniorData, string KPILocalMgrData,
            string selected_Menu, string KPIPrimaryData, string KPIStaffData)
        {
            string Message = "";
            if (OPS_DAL.SystemBus.KPITeamBus.AddKPISetting("ByUI", selected_System, selected_Corporation, selected_KPITeam, KPISeniorData,
              selected_Buyer, selected_Factory, KPIJuniorData, KPILocalMgrData,
              selected_Menu, KPIPrimaryData, KPIStaffData, out Message))
                return Json(new { retResult = true, retMsg = Message }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { retResult = false, retMsg = Message }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region HANDLE UPLOAD FILE "KPI TEMPLATE"
        [HttpPost]
        public ActionResult AddKPIByTemplate()
        {
            string fName = "";
            var originalDirectory = new DirectoryInfo(string.Format("{0}", Server.MapPath(@"\"))); // ::Application Root 
            string fNameCollection = "" ,
                pathString = System.IO.Path.Combine(originalDirectory.ToString(), "TempKPIFile");
            
            int fCounter = 0;

            try
            {
                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    //Save file content goes here
                    fName = file.FileName;
                    if (file != null && file.ContentLength > 0)
                    {
                        fCounter += 1;
                         
                        //pathString = System.IO.Path.Combine(originalDirectory.ToString(), "TempKPIFile");  // ::Application Root\\TempKPIFile

                        bool isExists = System.IO.Directory.Exists(pathString);

                        if (!isExists)
                            System.IO.Directory.CreateDirectory(pathString);

                        var path = string.Format("{0}\\{1}", pathString, Session["LoginUserID"].ToString() + "-" + file.FileName);  // ::Application Root\\TempKPIFile\\FileName << File Location
                        file.SaveAs(path);

                        if (String.IsNullOrEmpty(fNameCollection))
                            fNameCollection = Session["LoginUserID"].ToString() + "-" + file.FileName;
                        else
                            fNameCollection = fNameCollection + ";" + Session["LoginUserID"].ToString() + "-" + file.FileName;
                    }
                }

                //var importKPITask = ImportKPITemplateFile(fNameCollection); 
                //var importKPI_Status = await importKPITask;

                Task.Run(() => MES.CommonClass.ImportExcel.ImportKPITemplateFile (pathString , fNameCollection)   );

                return Json(new { retResult = true, retMessage = "Total File " + fCounter + "<br/>" });
            }
            catch (Exception ex)
            {
                return Json(new { retResult = false, retMessage = "Error in saving file: " + ex.Message });
            }
             
        }
        #endregion
         
        public string ShowQCPM(GridSettings gridRequest)
        {
            /*
             * Creator: Tai Le Huu (Thomas)
             * Create Time: 2018-12-11
             * Purpose: Based on the PO Delivery Schedule (PO DO) distributed the Receiving PO Qty to RANKED Package
             * Modified Track:
             *      2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column : Quantity_A (100%) ; Quantity_B (50%) ; Quantity_C (30%) ; Quantity_D (10%)
             */

            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(ConstantGeneric.ConnectionStr))
                {
                    oracleConnection.Open();

                    //Variable Declare
                    string strSQL = "";
                    decimal totalPages = 0, totalRecords = 0;
                    decimal intRowPerPage = gridRequest.pageSize;

                    var strSortColumn = gridRequest.sortColumn != String.Empty ? gridRequest.sortColumn : "QCORANK";
                    var strSortOrder = gridRequest.sortColumn != String.Empty ? gridRequest.sortOrder : "ASC";
                    var strOrderSQL = strSortColumn + " " + strSortOrder;

                    strSQL =
                        " SELECT ROW_NUMBER() OVER(ORDER BY NVL(CHANGEQCORANK, QCORANK) ) AS RANKING , " +
                        " T_00_STMT.STYLENAME , T_00_STMT.BUYERSTYLECODE , T_00_STMT.BUYERSTYLENAME , T_00_SCMT.STYLECOLORWAYS , " +
                        " T_QC_QUEUE.* , " +
                        " TO_DATE(T_QC_QUEUE.PRDSDAT , 'yyyyMMdd') AOPRDSDAT , " +
                        " TO_DATE(T_QC_QUEUE.PRDEDAT , 'yyyyMMdd') AOPRDEDAT , " +
                        " VIEW_ERP_PSRSNP_PLAN.DELIVERYDATE as AODELIVERYDATE , " +
                        " VIEW_ERP_PSRSNP_PLAN.PLANQTY as AOPLANQTY , " +
                        " VIEW_ERP_PSRSNP_PLAN.FACTORY AS ChangeFactory " +
                        " FROM PKMES.T_QC_QUEUE " +
                        "   INNER JOIN PKERP.T_00_SCMT  ON " +
                        "       T_QC_QUEUE.STYLECODE = T_00_SCMT.STYLECODE " +
                        "       AND T_QC_QUEUE.STYLECOLORSERIAL = T_00_SCMT.STYLECOLORSERIAL " +
                        "   INNER JOIN PKERP.T_00_STMT  ON " +
                        "       T_QC_QUEUE.STYLECODE = T_00_STMT.STYLECODE " +
                        "   LEFT JOIN PKERP.VIEW_ERP_PSRSNP_PLAN ON " +
                        "       T_QC_QUEUE.AONO = VIEW_ERP_PSRSNP_PLAN.AONO " +
                        "       AND T_QC_QUEUE.STYLECODE = VIEW_ERP_PSRSNP_PLAN.STYLECODE " +
                        "       AND T_QC_QUEUE.STYLESIZE = VIEW_ERP_PSRSNP_PLAN.STYLESIZE " +
                        "       AND T_QC_QUEUE.STYLECOLORSERIAL = VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL " +
                        "       AND T_QC_QUEUE.REVNO = VIEW_ERP_PSRSNP_PLAN.REVNO " +
                        "       AND T_QC_QUEUE.PRDPKG = VIEW_ERP_PSRSNP_PLAN.PRDPKG " +
                        " WHERE T_QC_QUEUE.QCOFACTORY = :FACTORY  " +
                        "   AND T_QC_QUEUE.QCOYEAR = :YEAR " +
                        "   AND T_QC_QUEUE.QCOWEEKNO = :WEEKNO ";

                    DataTable dt = new DataTable();
                    //dt = OracleDbManager.Query(strSQL, new List<OracleParameter> { new OracleParameter("FACTORY", idFactory), new OracleParameter("YEAR", vstrYear), new OracleParameter("WEEKNO", vstrWeekNo) }.ToArray());

                    if (dt != null)
                    {
                        if (dt.Rows.Count > 0)
                        {
                            totalRecords = dt.Rows.Count;

                            if (intRowPerPage <= 0)
                                intRowPerPage = totalRecords;

                            totalPages = Math.Ceiling(totalRecords / intRowPerPage);
                        }
                        dt.Dispose();
                    }

                    decimal StartingIndex = 1 + intRowPerPage * (gridRequest.pageIndex - 1);
                    if (StartingIndex <= 0)
                        StartingIndex = 1;

                    decimal EndIndex = intRowPerPage * gridRequest.pageIndex;
                    if (EndIndex <= 0)
                        EndIndex = totalRecords;

                    var strMainSQL = " SELECT * " +
                                     " FROM (" + strSQL + ") MainData " +
                                     " WHERE RANKING >= " + StartingIndex + " " +
                                     " AND   RANKING <= " + EndIndex +
                                     " ORDER BY RANKING ";

                    //dt = OracleDbManager.Query(strMainSQL, new List<OracleParameter> { new OracleParameter("FACTORY", idFactory), new OracleParameter("YEAR", vstrYear), new OracleParameter("WEEKNO", vstrWeekNo) }.ToArray());

                    var tmpResult = JsonConvert.SerializeObject(new
                    {
                        total = totalPages,
                        page = gridRequest.pageIndex,
                        records = totalRecords,
                        rows = dt
                    }, new JsonSerializerSettings { DateFormatString = "yyyy/MM/dd" });
                    dt.Dispose();

                    oracleConnection.Close();
                    oracleConnection.Dispose();

                    return tmpResult;
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { retResult = false, dataRow = ex.Message });
            }
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