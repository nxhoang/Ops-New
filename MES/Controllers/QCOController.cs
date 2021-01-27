using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Web;
using System.Reflection;
using MES.Models;
using MES.CommonClass;
using OPS_DAL.Entities;
using OPS_DAL.Business;
using OPS_DAL.DAL;
using OPS_DAL.QCOBus;
using OPS_DAL.QCOEntities;
using OPS_Utils;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using OfficeOpenXml;
using PKQCO;
using System.Windows;
using System.Text;
namespace MES.Controllers
{
    [SessionTimeout]
    [AutologArribute]
    public class QCOController : Controller
    {
        /* Class Object  REGION */
        public OPS_DAL.Entities.Usmt UserInf => (OPS_DAL.Entities.Usmt)Session["LoginUser"];
        public Srmt Role => SrmtBus.GetUserRoleInfoMySql(UserInf.RoleID, "MES", "QCO");
        public string cTableName = " PKMES.T_QC_QUEUE";
        #region VIEW
        [SysActionFilter(SystemID = "MES", MenuID = "QCO", Action = "")]
        public ActionResult QCOManagement()
        {
            ViewBag.PageTitle = "<i class=\"fa fa-list-alt\"></i>&nbsp;QCO";
            ViewBag.SubPageTitle = "&nbsp;<span>> QCO Ranking</span>";
            return View();
        }
        public ActionResult QCODetail()
        {
            return View("QCODetail");
        }
        public ActionResult POPStyleSummary()
        {
            string StyleCode =
                Url.RequestContext.HttpContext.Request["STYLECODE"] != null
                ? Url.RequestContext.HttpContext.Request["STYLECODE"]
                : "";
            string StyleSize =
                Url.RequestContext.HttpContext.Request["STYLESIZE"] != null
                ? Url.RequestContext.HttpContext.Request["STYLESIZE"]
                : "";
            string StyleColorSerial =
                Url.RequestContext.HttpContext.Request["STYLECOLORSERIAL"] != null
                ? Url.RequestContext.HttpContext.Request["STYLECOLORSERIAL"]
                : "";
            string RevNo =
                Url.RequestContext.HttpContext.Request["REVNO"] != null
                ? Url.RequestContext.HttpContext.Request["REVNO"]
                : "";
            string PRDPKG =
                Url.RequestContext.HttpContext.Request["PRDPKG"] != null
                ? Url.RequestContext.HttpContext.Request["PRDPKG"]
                : "";
            string RemainQTY =
                Url.RequestContext.HttpContext.Request["RemainQty"] != null
                ? Url.RequestContext.HttpContext.Request["RemainQty"]
                : "0";
            //2020-03-26 Tai Le(Thomas)
            //Handle Style Info from PRDPKG when OR(StyleCode, StyleSize, StyleColorSerial, RevNo) = ""
            var arrPRDPKG = PRDPKG.Split('_');
            if (arrPRDPKG.Length > 0)
            {
                var fullStyleInfo = arrPRDPKG[3];
                StyleCode = fullStyleInfo.Substring(0, 7);
                StyleSize = fullStyleInfo.Substring(7, 3);
                StyleColorSerial = fullStyleInfo.Substring(10, 3);
                RevNo = fullStyleInfo.Substring(13, 3);
            }
            //var Hyperlink = "http://203.113.151.204:8080/PKPDM/style/";
            //var strSQL = @"
            //Select  VIEW_ERP_PSRSNP_PLAN.StyleCode ,  VIEW_ERP_PSRSNP_PLAN.StyleSize ,   VIEW_ERP_PSRSNP_PLAN.StyleColorSerial , T_00_SCMT.StyleColorWays , VIEW_ERP_PSRSNP_PLAN.RevNo , T_00_STMT.BuyerStyleCode , T_00_STMT.BuyerStyleName , 'http://203.113.151.204:8080/PKPDM/style/' || Substr(T_00_STMT.StyleCode,1,3) ||'/' || VIEW_ERP_PSRSNP_PLAN.StyleCode || '/Images/' || T_00_SFDT.FileName ImgLink, 
            //VIEW_ERP_PSRSNP_PLAN.PlanQty , VIEW_ERP_PSRSNP_PLAN.ORDQTY  , VIEW_ERP_PSRSNP_PLAN.PRDPKG
            //From PKERP.VIEW_ERP_PSRSNP_PLAN
            //INNER JOIN PKERP.T_00_STMT ON 
            //    VIEW_ERP_PSRSNP_PLAN.StyleCode = T_00_STMT.StyleCode  
            //INNER JOIN PKERP.T_00_SCMT ON 
            //    VIEW_ERP_PSRSNP_PLAN.StyleCode = T_00_SCMT.StyleCode  
            //    AND VIEW_ERP_PSRSNP_PLAN.StyleColorSerial = T_00_SCMT.StyleColorSerial 
            //LEFT JOIN PKERP.T_00_SFDT ON 
            //    VIEW_ERP_PSRSNP_PLAN.StyleCode = T_00_SFDT.StyleCode AND T_00_SFDT.is_main = 'Y'
            //Where VIEW_ERP_PSRSNP_PLAN.STYLECODE = :STYLECODE
            //AND VIEW_ERP_PSRSNP_PLAN.STYLESIZE = :STYLESIZE
            //AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = :STYLECOLORSERIAL
            //AND VIEW_ERP_PSRSNP_PLAN.REVNO = :REVNO
            //AND VIEW_ERP_PSRSNP_PLAN.PRDPKG = :PRDPKG
            //";
            var strSQL =
                $" Select  " +
                $" VIEW_ERP_PSRSNP_PLAN.StyleCode ,  VIEW_ERP_PSRSNP_PLAN.StyleSize ,   VIEW_ERP_PSRSNP_PLAN.StyleColorSerial , T_00_SCMT.StyleColorWays ,  " +
                $" VIEW_ERP_PSRSNP_PLAN.RevNo , T_00_STMT.BuyerStyleCode , T_00_STMT.BuyerStyleName ," +
                $"  'http://203.113.151.204:8080/PKPDM/style/' || Substr(T_00_STMT.StyleCode,1,3) ||'/' || VIEW_ERP_PSRSNP_PLAN.StyleCode || '/Images/' || T_00_SFDT.FileName ImgLink, " +
                $" VIEW_ERP_PSRSNP_PLAN.PlanQty , VIEW_ERP_PSRSNP_PLAN.ORDQTY  , VIEW_ERP_PSRSNP_PLAN.PRDPKG ,  {RemainQTY} as RemainQty " +
                $" From " +
                $"  PKERP.VIEW_ERP_PSRSNP_PLAN " +
                $"  INNER JOIN PKERP.T_00_STMT ON " +
                $"      VIEW_ERP_PSRSNP_PLAN.StyleCode = T_00_STMT.StyleCode " +
                $"  INNER JOIN PKERP.T_00_SCMT ON " +
                $"      VIEW_ERP_PSRSNP_PLAN.StyleCode = T_00_SCMT.StyleCode AND " +
                $"      VIEW_ERP_PSRSNP_PLAN.StyleColorSerial = T_00_SCMT.StyleColorSerial " +
                $"  LEFT JOIN PKERP.T_00_SFDT ON " +
                $"      VIEW_ERP_PSRSNP_PLAN.StyleCode = T_00_SFDT.StyleCode AND " +
                $"      T_00_SFDT.is_main = 'Y' " +
                $" Where " +
                $"      VIEW_ERP_PSRSNP_PLAN.STYLECODE = '{StyleCode}' AND " +
                $"      VIEW_ERP_PSRSNP_PLAN.STYLESIZE = '{StyleSize}' AND " +
                $"      VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = '{StyleColorSerial}' AND " +
                $"      VIEW_ERP_PSRSNP_PLAN.REVNO = '{RevNo}' AND " +
                $"      VIEW_ERP_PSRSNP_PLAN.PRDPKG = '{PRDPKG}'  " +
                $" ";
            //List<OracleParameter> parameters = new List<OracleParameter>();
            //parameters.Add(new OracleParameter("STYLECODE", StyleCode));
            //parameters.Add(new OracleParameter("STYLESIZE", StyleSize));
            //parameters.Add(new OracleParameter("STYLECOLORSERIAL", StyleColorSerial));
            //parameters.Add(new OracleParameter("REVNO", RevNo));
            //parameters.Add(new OracleParameter("PRDPKG", PRDPKG)); 
            //var objStyleCodeSummary = OPS_DAL.DAL.OracleDbManager.GetObjectsByType<StyleCodeSummary>(strSQL, CommandType.Text, parameters.ToArray(), OPS_Utils.ConstantGeneric.ConnectionStrMes);
            var objStyleCodeSummary = OPS_DAL.DAL.OracleDbManager.GetObjectsByType<StyleCodeSummary>(strSQL, CommandType.Text, null, OPS_Utils.ConstantGeneric.ConnectionStrMes);
            return PartialView("POPStyleSummary", objStyleCodeSummary.FirstOrDefault());
        }
        public ActionResult POPStyleSummaryMESPackage()
        {
            string StyleCode = Url.RequestContext.HttpContext.Request["STYLECODE"] != null ? Url.RequestContext.HttpContext.Request["STYLECODE"] : "";
            string StyleSize = Url.RequestContext.HttpContext.Request["STYLESIZE"] != null ? Url.RequestContext.HttpContext.Request["STYLESIZE"] : "";
            string StyleColorSerial = Url.RequestContext.HttpContext.Request["STYLECOLORSERIAL"] != null ? Url.RequestContext.HttpContext.Request["STYLECOLORSERIAL"] : "";
            string RevNo = Url.RequestContext.HttpContext.Request["REVNO"] != null ? Url.RequestContext.HttpContext.Request["REVNO"] : "";
            string MxPackage = Url.RequestContext.HttpContext.Request["MxPackage"] != null ? Url.RequestContext.HttpContext.Request["MxPackage"] : "";
            //2019-11-18 Tai Le (Thomas)
            string MxTarget = Url.RequestContext.HttpContext.Request["MxTarget"] != null ? Url.RequestContext.HttpContext.Request["MxTarget"] : "";
            //var Hyperlink = "http://203.113.151.204:8080/PKPDM/style/";
            //MESPackage = OPS_DAL.MesBus.MpdtBus.GetByMxPackage(MxPackage);
            var strSQL = @"
Select  T_SD_DORM.StyleCode ,  T_SD_DORM.StyleSize ,   T_SD_DORM.StyleColorSerial , T_00_SCMT.StyleColorWays , T_SD_DORM.RevNo , T_00_STMT.BuyerStyleCode , T_00_STMT.BuyerStyleName , 
'http://203.113.151.204:8080/PKPDM/style/' || Substr(T_00_STMT.StyleCode,1,3) ||'/' || T_SD_DORM.StyleCode || '/Images/' || T_00_SFDT.FileName ImgLink, 
:PLANQTY  PlanQty , 0 ORDQTY  , :PRDPKG  PRDPKG
From PKERP.T_00_STMT 
INNER JOIN PKERP.T_SD_DORM ON 
    T_00_STMT.StyleCode = T_SD_DORM.StyleCode  
INNER JOIN PKERP.T_00_SCMT ON 
    T_SD_DORM.StyleCode = T_00_SCMT.StyleCode  
    AND T_SD_DORM.StyleColorSerial = T_00_SCMT.StyleColorSerial 
LEFT JOIN PKERP.T_00_SFDT ON 
    T_00_STMT.StyleCode = T_00_SFDT.StyleCode AND T_00_SFDT.is_main = 'Y'
Where T_SD_DORM.StyleCode = :StyleCode
And T_SD_DORM.StyleSize = :StyleSize
And T_SD_DORM.StyleColorSerial = :StyleColorSerial
And T_SD_DORM.RevNo = :RevNo 
";
            List<OracleParameter> parameters = new List<OracleParameter>();
            //parameters.Add(new OracleParameter("PLANQTY", MESPackage.MxTarget));
            parameters.Add(new OracleParameter("PLANQTY", MxTarget));
            parameters.Add(new OracleParameter("PRDPKG", MxPackage));
            parameters.Add(new OracleParameter("StyleCode", StyleCode));
            parameters.Add(new OracleParameter("StyleSize", StyleSize));
            parameters.Add(new OracleParameter("StyleColorSerial", StyleColorSerial));
            parameters.Add(new OracleParameter("RevNo", RevNo));
            var objStyleCodeSummary = OPS_DAL.DAL.OracleDbManager.GetObjectsByType<StyleCodeSummary>(strSQL, CommandType.Text, parameters.ToArray(), OPS_Utils.ConstantGeneric.ConnectionStrMes);
            return PartialView("POPStyleSummary", objStyleCodeSummary.FirstOrDefault());
        }
        public ActionResult QCODetailPop()
        {
            string Factory = Url.RequestContext.HttpContext.Request["Factory"] != null ? Url.RequestContext.HttpContext.Request["Factory"] : "";
            string LineNo = Url.RequestContext.HttpContext.Request["LINENO"] != null ? Url.RequestContext.HttpContext.Request["LINENO"] : "";
            string AONo = Url.RequestContext.HttpContext.Request["AONO"] != null ? Url.RequestContext.HttpContext.Request["AONO"] : "";
            string StyleCode = Url.RequestContext.HttpContext.Request["STYLECODE"] != null ? Url.RequestContext.HttpContext.Request["STYLECODE"] : "";
            string StyleSize = Url.RequestContext.HttpContext.Request["STYLESIZE"] != null ? Url.RequestContext.HttpContext.Request["STYLESIZE"] : "";
            string StyleColorSerial = Url.RequestContext.HttpContext.Request["STYLECOLORSERIAL"] != null ? Url.RequestContext.HttpContext.Request["STYLECOLORSERIAL"] : "";
            string RevNo = Url.RequestContext.HttpContext.Request["REVNO"] != null ? Url.RequestContext.HttpContext.Request["REVNO"] : "";
            string PP = Url.RequestContext.HttpContext.Request["PRDPKG"] != null ? Url.RequestContext.HttpContext.Request["PRDPKG"] : "";
            string QCOYEAR = Url.RequestContext.HttpContext.Request["QCOYEAR"] != null ? Url.RequestContext.HttpContext.Request["QCOYEAR"] : "";
            string QCOWEEK = Url.RequestContext.HttpContext.Request["QCOWEEKNO"] != null ? Url.RequestContext.HttpContext.Request["QCOWEEKNO"] : "";
            string QCOSOURCE = Url.RequestContext.HttpContext.Request["QCOSOURCE"] != null ? Url.RequestContext.HttpContext.Request["QCOSOURCE"] : "QCO";
            //string strSQL = "SELECT ROW_NUMBER() OVER(ORDER BY T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL ) AS SEQNO ,  " +
            //				" '" + Factory + "' AS QCOFACTORY , " +
            //				" " + QCOYEAR + " as QCOYEAR , " +
            //				" '" + QCOWEEK + "' as QCOWEEKNO , " +
            //				" '" + LineNo + "' AS LINE , " +
            //				" VIEW_ERP_PSRSNP_PLAN.FACTORY ,  VIEW_ERP_PSRSNP_PLAN.LINENO ,  VIEW_ERP_PSRSNP_PLAN.AONO ,  " +
            //				" VIEW_ERP_PSRSNP_PLAN.STYLECODE ,  T_00_STMT.STYLENAME , T_00_STMT.BUYERSTYLENAME ,  VIEW_ERP_PSRSNP_PLAN.STYLESIZE ,  " +
            //				" VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL , T_00_SCMT.STYLECOLORWAYS, " +
            //				" VIEW_ERP_PSRSNP_PLAN.REVNO ,  VIEW_ERP_PSRSNP_PLAN.PRDPKG ,  " +
            //				" T_SD_BOMT.ITEMCODE ,  T_00_ICMT.ITEMNAME ,  T_SD_BOMT.ITEMCOLORSERIAL || ' - ' ||  T_00_ICCM.ITEMCOLORWAYS as ITEMCOLORSERIAL , " +
            //				" VIEW_ERP_PSRSNP_PLAN.PLANQTY * T_SD_BOMT.UNITCONSUMPTION as REQUESTQTY , " +
            //				" NVL(T_QC_QCPM.QUANTITY_A, 0) QUANTITY_A ,  NVL(T_QC_QCPM.QUANTITY_B, 0) QUANTITY_B ,  NVL(T_QC_QCPM.QUANTITY_C, 0) QUANTITY_C ,  NVL(T_QC_QCPM.QUANTITY_D, 0) QUANTITY_D , " +
            //				" NVL(T_QC_QCPM.PLANQUANTITY, 0)  PLANQUANTITY , " +
            //				" V_MRP_PP_WO.WONO , " +
            //				" T_QC_QCPM.PDNO " +
            //				" FROM PKERP.VIEW_ERP_PSRSNP_PLAN " +
            //				" INNER JOIN PKERP.T_SD_BOMT ON " +
            //				"   VIEW_ERP_PSRSNP_PLAN.STYLECODE = T_SD_BOMT.STYLECODE " +
            //				"   AND VIEW_ERP_PSRSNP_PLAN.STYLESIZE = T_SD_BOMT.STYLESIZE " +
            //				"   AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = T_SD_BOMT.STYLECOLORSERIAL " +
            //				"   AND VIEW_ERP_PSRSNP_PLAN.REVNO = T_SD_BOMT.REVNO " +
            //				" INNER JOIN PKERP.T_00_ICCM  ON " +
            //				"        T_SD_BOMT.ITEMCODE = T_00_ICCM.ITEMCODE " +
            //				"        AND T_SD_BOMT.ITEMCOLORSERIAL = T_00_ICCM.ITEMCOLORSERIAL " +
            //				" INNER JOIN PKERP.T_00_SCMT  ON " +
            //				"        VIEW_ERP_PSRSNP_PLAN.STYLECODE = T_00_SCMT.STYLECODE " +
            //				"        AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = T_00_SCMT.STYLECOLORSERIAL " +
            //				" INNER JOIN PKERP.T_00_ICMT  ON " +
            //				"        T_SD_BOMT.ITEMCODE = T_00_ICMT.ITEMCODE " +
            //				" INNER JOIN PKERP.T_00_STMT  ON " +
            //				"        VIEW_ERP_PSRSNP_PLAN.STYLECODE = T_00_STMT.STYLECODE " +
            //				" LEFT JOIN " +
            //				" ( SELECT T_QC_QCPM.RANGKING , T_QC_QCPM.FACTORY, T_QC_QCPM.LINENO, T_QC_QCPM.AONO, " +
            //				"           T_QC_QCPM.STYLECODE,  T_QC_QCPM.STYLESIZE,  T_QC_QCPM.STYLECOLORSERIAL , T_QC_QCPM.REVNO, T_QC_QCPM.PRDPKG, " +
            //				"           T_QC_QCPM.ITEMCODE,  T_QC_QCPM.ITEMCOLORSERIAL , T_QC_QCPM.REQUESTQTY as REQUESTQTY_A , " +
            //				"           T_QC_QCPM.PDNO ,  " +
            //				"           SUM( nvl(T_QC_QCPM.QUANTITY_A,0 ) ) QUANTITY_A , SUM( nvl(T_QC_QCPM.QUANTITY_B,0 ) ) QUANTITY_B ,  SUM( nvl(T_QC_QCPM.QUANTITY_C,0 ) ) QUANTITY_C ,   SUM( nvl(T_QC_QCPM.QUANTITY_D,0 ) ) QUANTITY_D ,   " +
            //				"           SUM( nvl(T_QC_QCPM.PLANQUANTITY,0 ) ) PLANQUANTITY " +
            //				"   FROM {TableName} T_QC_QCPM " +
            //				"   WHERE T_QC_QCPM.FACTORY = '" + Factory + "' " +
            //				"       AND T_QC_QCPM.LINENO = '" + LineNo + "' " +
            //				"       AND T_QC_QCPM.AONO = '" + AONo + "' " +
            //				"       AND T_QC_QCPM.STYLECODE = '" + StyleCode + "' " +
            //				"       AND T_QC_QCPM.STYLESIZE = '" + StyleSize + "' " +
            //				"       AND T_QC_QCPM.STYLECOLORSERIAL = '" + StyleColorSerial + "' " +
            //				"       AND T_QC_QCPM.REVNO = '" + RevNo + "' " +
            //				"       AND T_QC_QCPM.PRDPKG = '" + PP + "' " +
            //				"       AND T_QC_QCPM.QCOWEEKNO = '" + QCOWEEK + "' " +
            //				"       AND T_QC_QCPM.QCOYEAR = '" + QCOYEAR + "' " +
            //				"   GROUP BY T_QC_QCPM.RANGKING , T_QC_QCPM.FACTORY, T_QC_QCPM.LINENO, T_QC_QCPM.AONO, T_QC_QCPM.STYLECODE, T_QC_QCPM.STYLESIZE, T_QC_QCPM.STYLECOLORSERIAL, T_QC_QCPM.REVNO, T_QC_QCPM.PRDPKG, " +
            //				"   T_QC_QCPM.ITEMCODE, T_QC_QCPM.ITEMCOLORSERIAL , T_QC_QCPM.REQUESTQTY  ,T_QC_QCPM.PDNO " +
            //				" ) T_QC_QCPM ON " +
            //				"       VIEW_ERP_PSRSNP_PLAN.FACTORY = T_QC_QCPM.FACTORY " +
            //				"       AND VIEW_ERP_PSRSNP_PLAN.LINENO = T_QC_QCPM.LINENO " +
            //				"       AND VIEW_ERP_PSRSNP_PLAN.AONO = T_QC_QCPM.AONO " +
            //				"       AND T_SD_BOMT.STYLECODE = T_QC_QCPM.STYLECODE " +
            //				"       AND T_SD_BOMT.STYLESIZE = T_QC_QCPM.STYLESIZE " +
            //				"       AND T_SD_BOMT.STYLECOLORSERIAL = T_QC_QCPM.STYLECOLORSERIAL " +
            //				"       AND T_SD_BOMT.REVNO = T_QC_QCPM.REVNO " +
            //				"       AND VIEW_ERP_PSRSNP_PLAN.PRDPKG = T_QC_QCPM.PRDPKG " +
            //				"       AND T_SD_BOMT.ITEMCODE = T_QC_QCPM.ITEMCODE " +
            //				"       AND T_SD_BOMT.ITEMCOLORSERIAL = T_QC_QCPM.ITEMCOLORSERIAL " +
            //				"   LEFT JOIN PKMES.V_MRP_PP_WO ON " +
            //				"      V_MRP_PP_WO. AONO =  VIEW_ERP_PSRSNP_PLAN.AONO AND " +
            //				"      V_MRP_PP_WO. STLCD =  VIEW_ERP_PSRSNP_PLAN.STYLECODE AND " +
            //				"      V_MRP_PP_WO. STLSIZ =  VIEW_ERP_PSRSNP_PLAN.STYLESIZE  AND " +
            //				"      V_MRP_PP_WO. STLCOSN =  VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL  AND " +
            //				"      V_MRP_PP_WO. STLREVN =  VIEW_ERP_PSRSNP_PLAN.REVNO  AND " +
            //				"      V_MRP_PP_WO. FACTORY =  VIEW_ERP_PSRSNP_PLAN.FACTORY  AND " +
            //				"      V_MRP_PP_WO. PRODPACKAGE =   VIEW_ERP_PSRSNP_PLAN.PRDPKG  " +
            //				"   WHERE VIEW_ERP_PSRSNP_PLAN.FACTORY = '" + Factory + "' " +
            //				"       AND VIEW_ERP_PSRSNP_PLAN.LINENO = '" + LineNo + "' " +
            //				"       AND VIEW_ERP_PSRSNP_PLAN.AONO = '" + AONo + "' " +
            //				"       AND VIEW_ERP_PSRSNP_PLAN.STYLECODE = '" + StyleCode + "' " +
            //				"       AND VIEW_ERP_PSRSNP_PLAN.STYLESIZE = '" + StyleSize + "' " +
            //				"       AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = '" + StyleColorSerial + "' " +
            //				"       AND VIEW_ERP_PSRSNP_PLAN.REVNO = '" + RevNo + "' " +
            //				"       AND VIEW_ERP_PSRSNP_PLAN.PRDPKG = '" + PP + "' " +
            //				"       AND (T_SD_BOMT.ITEMCODE NOT LIKE 'PKG%' AND T_SD_BOMT.ITEMCODE NOT LIKE 'TRE%' ) " +
            //				" ORDER BY T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL ";
            //2020-08-11 Tai Le(Thomas): remove comparing [LineNo]
            string strSQL = "SELECT ROW_NUMBER() OVER(ORDER BY T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL ) AS SEQNO ,  " +
                         " '" + Factory + "' AS QCOFACTORY , " +
                         " " + QCOYEAR + " as QCOYEAR , " +
                         " '" + QCOWEEK + "' as QCOWEEKNO , " +
                         " '" + LineNo + "' AS LINE , " +
                         " VIEW_ERP_PSRSNP_PLAN.FACTORY ,  VIEW_ERP_PSRSNP_PLAN.LINENO ,  VIEW_ERP_PSRSNP_PLAN.AONO ,  " +
                         " VIEW_ERP_PSRSNP_PLAN.STYLECODE ,  T_00_STMT.STYLENAME , T_00_STMT.BUYERSTYLENAME ,  VIEW_ERP_PSRSNP_PLAN.STYLESIZE ,  " +
                         " VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL , T_00_SCMT.STYLECOLORWAYS, " +
                         " VIEW_ERP_PSRSNP_PLAN.REVNO ,  VIEW_ERP_PSRSNP_PLAN.PRDPKG ,  " +
                         " T_SD_BOMT.ITEMCODE ,  T_00_ICMT.ITEMNAME ,  T_SD_BOMT.ITEMCOLORSERIAL || ' - ' ||  T_00_ICCM.ITEMCOLORWAYS as ITEMCOLORSERIAL , " +
                         " VIEW_ERP_PSRSNP_PLAN.PLANQTY * T_SD_BOMT.UNITCONSUMPTION as REQUESTQTY , " +
                         " NVL(T_QC_QCPM.QUANTITY_A, 0) QUANTITY_A ,  NVL(T_QC_QCPM.QUANTITY_B, 0) QUANTITY_B ,  NVL(T_QC_QCPM.QUANTITY_C, 0) QUANTITY_C ,  NVL(T_QC_QCPM.QUANTITY_D, 0) QUANTITY_D , " +
                         " NVL(T_QC_QCPM.PLANQUANTITY, 0)  PLANQUANTITY , " +
                         " V_MRP_PP_WO.WONO , " +
                         " T_QC_QCPM.PDNO " +
                         " FROM PKERP.VIEW_ERP_PSRSNP_PLAN " +
                         " INNER JOIN PKERP.T_SD_BOMT ON " +
                         "   VIEW_ERP_PSRSNP_PLAN.STYLECODE = T_SD_BOMT.STYLECODE " +
                         "   AND VIEW_ERP_PSRSNP_PLAN.STYLESIZE = T_SD_BOMT.STYLESIZE " +
                         "   AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = T_SD_BOMT.STYLECOLORSERIAL " +
                         "   AND VIEW_ERP_PSRSNP_PLAN.REVNO = T_SD_BOMT.REVNO " +
                         " INNER JOIN PKERP.T_00_ICCM  ON " +
                         "        T_SD_BOMT.ITEMCODE = T_00_ICCM.ITEMCODE " +
                         "        AND T_SD_BOMT.ITEMCOLORSERIAL = T_00_ICCM.ITEMCOLORSERIAL " +
                         " INNER JOIN PKERP.T_00_SCMT  ON " +
                         "        VIEW_ERP_PSRSNP_PLAN.STYLECODE = T_00_SCMT.STYLECODE " +
                         "        AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = T_00_SCMT.STYLECOLORSERIAL " +
                         " INNER JOIN PKERP.T_00_ICMT  ON " +
                         "        T_SD_BOMT.ITEMCODE = T_00_ICMT.ITEMCODE " +
                         " INNER JOIN PKERP.T_00_STMT  ON " +
                         "        VIEW_ERP_PSRSNP_PLAN.STYLECODE = T_00_STMT.STYLECODE " +
                         " LEFT JOIN " +
                         " ( SELECT T_QC_QCPM.RANGKING , T_QC_QCPM.FACTORY, T_QC_QCPM.LINENO, T_QC_QCPM.AONO, " +
                         "           T_QC_QCPM.STYLECODE,  T_QC_QCPM.STYLESIZE,  T_QC_QCPM.STYLECOLORSERIAL , T_QC_QCPM.REVNO, T_QC_QCPM.PRDPKG, " +
                         "           T_QC_QCPM.ITEMCODE,  T_QC_QCPM.ITEMCOLORSERIAL , T_QC_QCPM.REQUESTQTY as REQUESTQTY_A , " +
                         "           T_QC_QCPM.PDNO ,  " +
                         "           SUM( nvl(T_QC_QCPM.QUANTITY_A,0 ) ) QUANTITY_A , SUM( nvl(T_QC_QCPM.QUANTITY_B,0 ) ) QUANTITY_B ,  SUM( nvl(T_QC_QCPM.QUANTITY_C,0 ) ) QUANTITY_C ,   SUM( nvl(T_QC_QCPM.QUANTITY_D,0 ) ) QUANTITY_D ,   " +
                         "           SUM( nvl(T_QC_QCPM.PLANQUANTITY,0 ) ) PLANQUANTITY " +
                         "   FROM {TableName} T_QC_QCPM " +
                         "   WHERE T_QC_QCPM.FACTORY = '" + Factory + "' " +
                         "       AND T_QC_QCPM.AONO = '" + AONo + "' " +
                         "       AND T_QC_QCPM.STYLECODE = '" + StyleCode + "' " +
                         "       AND T_QC_QCPM.STYLESIZE = '" + StyleSize + "' " +
                         "       AND T_QC_QCPM.STYLECOLORSERIAL = '" + StyleColorSerial + "' " +
                         "       AND T_QC_QCPM.REVNO = '" + RevNo + "' " +
                         "       AND T_QC_QCPM.PRDPKG = '" + PP + "' " +
                         "       AND T_QC_QCPM.QCOWEEKNO = '" + QCOWEEK + "' " +
                         "       AND T_QC_QCPM.QCOYEAR = '" + QCOYEAR + "' " +
                         "   GROUP BY T_QC_QCPM.RANGKING , T_QC_QCPM.FACTORY, T_QC_QCPM.LINENO, T_QC_QCPM.AONO, T_QC_QCPM.STYLECODE, T_QC_QCPM.STYLESIZE, T_QC_QCPM.STYLECOLORSERIAL, T_QC_QCPM.REVNO, T_QC_QCPM.PRDPKG, " +
                         "   T_QC_QCPM.ITEMCODE, T_QC_QCPM.ITEMCOLORSERIAL , T_QC_QCPM.REQUESTQTY  ,T_QC_QCPM.PDNO " +
                         " ) T_QC_QCPM ON " +
                         "       VIEW_ERP_PSRSNP_PLAN.FACTORY = T_QC_QCPM.FACTORY " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.LINENO = T_QC_QCPM.LINENO " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.AONO = T_QC_QCPM.AONO " +
                         "       AND T_SD_BOMT.STYLECODE = T_QC_QCPM.STYLECODE " +
                         "       AND T_SD_BOMT.STYLESIZE = T_QC_QCPM.STYLESIZE " +
                         "       AND T_SD_BOMT.STYLECOLORSERIAL = T_QC_QCPM.STYLECOLORSERIAL " +
                         "       AND T_SD_BOMT.REVNO = T_QC_QCPM.REVNO " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.PRDPKG = T_QC_QCPM.PRDPKG " +
                         "       AND T_SD_BOMT.ITEMCODE = T_QC_QCPM.ITEMCODE " +
                         "       AND T_SD_BOMT.ITEMCOLORSERIAL = T_QC_QCPM.ITEMCOLORSERIAL " +
                         "   LEFT JOIN PKMES.V_MRP_PP_WO ON " +
                         "      V_MRP_PP_WO. AONO =  VIEW_ERP_PSRSNP_PLAN.AONO AND " +
                         "      V_MRP_PP_WO. STLCD =  VIEW_ERP_PSRSNP_PLAN.STYLECODE AND " +
                         "      V_MRP_PP_WO. STLSIZ =  VIEW_ERP_PSRSNP_PLAN.STYLESIZE  AND " +
                         "      V_MRP_PP_WO. STLCOSN =  VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL  AND " +
                         "      V_MRP_PP_WO. STLREVN =  VIEW_ERP_PSRSNP_PLAN.REVNO  AND " +
                         "      V_MRP_PP_WO. FACTORY =  VIEW_ERP_PSRSNP_PLAN.FACTORY  AND " +
                         "      V_MRP_PP_WO. PRODPACKAGE =   VIEW_ERP_PSRSNP_PLAN.PRDPKG  " +
                         "   WHERE VIEW_ERP_PSRSNP_PLAN.FACTORY = '" + Factory + "' " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.AONO = '" + AONo + "' " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.STYLECODE = '" + StyleCode + "' " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.STYLESIZE = '" + StyleSize + "' " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = '" + StyleColorSerial + "' " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.REVNO = '" + RevNo + "' " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.PRDPKG = '" + PP + "' " +
                         "       AND (T_SD_BOMT.ITEMCODE NOT LIKE 'PKG%' AND T_SD_BOMT.ITEMCODE NOT LIKE 'TRE%' ) " +
                         " ORDER BY T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL ";
            if ("QCO" == QCOSOURCE)
            {
                strSQL = strSQL.Replace("{TableName}", "PKMES.T_QC_QCPM");
            }
            else if ("QCOSim" == QCOSOURCE)
            {
                strSQL = strSQL.Replace("{TableName}", "PKMES.T_QC_QCPMSIM");
            }
            DataTable dt = new DataTable();
            dt = OracleDbManager.Query(strSQL, null);
            return PartialView("QCODetailPop", dt);
        }
        public ActionResult MTOPPPCuttingReadinessPop()
        {
            string PP = Url.RequestContext.HttpContext.Request["PRDPKG"] != null ? Url.RequestContext.HttpContext.Request["PRDPKG"] : "";

            //string strSQL = $@"
            //SELECT V_PCM_MTOPS_PP_CUTREADINESS.PRDPKG, V_PCM_MTOPS_PP_CUTREADINESS.MODULENAME , 
            //V_PCM_MTOPS_PP_CUTREADINESS.ITEMCODE, V_PCM_MTOPS_PP_CUTREADINESS.ITEMNAME, V_PCM_MTOPS_PP_CUTREADINESS.ITEMCOLORSERIAL, V_PCM_MTOPS_PP_CUTREADINESS.ITEMCOLORWAYS, 
            //V_PCM_MTOPS_PP_CUTREADINESS.PIECEUNIQUE, V_PCM_MTOPS_PP_CUTREADINESS.PIECE, 
            //V_PCM_MTOPS_PP_CUTREADINESS.TOTALPIECEQTY , V_PCM_MTOPS_PP_CUTREADINESS.ALLOCATEDQTY , V_PCM_MTOPS_PP_CUTREADINESS.CUTTINGREADINESS,
            //VIEW_ERP_PSRSNP_PLAN.Factory , VIEW_ERP_PSRSNP_PLAN.LineNo , VIEW_ERP_PSRSNP_PLAN.AONO, 
            //VIEW_ERP_PSRSNP_PLAN.StyleCode, VIEW_ERP_PSRSNP_PLAN.StyleSize, VIEW_ERP_PSRSNP_PLAN.StyleColorSerial , VIEW_ERP_PSRSNP_PLAN.RevNo , 
            //VIEW_ERP_PSRSNP_PLAN.PlanQty ,
            //(Select BuyerStyleName From T_00_STMT Where StyleCode = VIEW_ERP_PSRSNP_PLAN.StyleCode ) BuyerStyleName ,
            //(Select StyleColorWays From T_00_SCMT Where StyleCode = VIEW_ERP_PSRSNP_PLAN.StyleCode And StyleColorSerial = VIEW_ERP_PSRSNP_PLAN.StyleColorSerial) StyleColorWays 
            //FROM PKERP.V_PCM_MTOPS_PP_CUTREADINESS 
            //JOIN PKERP.VIEW_ERP_PSRSNP_PLAN ON 
            //    V_PCM_MTOPS_PP_CUTREADINESS.PRDPKG = VIEW_ERP_PSRSNP_PLAN.PRDPKG
            //WHERE V_PCM_MTOPS_PP_CUTREADINESS.PRDPKG = '{PP}'
            //";
            string strSQL = $@"
SELECT VIEW_ERP_PSRSNP_PLAN.Factory ,  VIEW_ERP_PSRSNP_PLAN.LineNo ,  VIEW_ERP_PSRSNP_PLAN.AONO , 
VIEW_ERP_PSRSNP_PLAN.PRDPKG , VIEW_ERP_PSRSNP_PLAN.STYLECODE , VIEW_ERP_PSRSNP_PLAN.STYLESIZE , VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL , VIEW_ERP_PSRSNP_PLAN.REVNO , 
VIEW_ERP_PSRSNP_PLAN.PLANQTY,
(Select BuyerStyleName From T_00_STMT Where StyleCode = VIEW_ERP_PSRSNP_PLAN.StyleCode ) BuyerStyleName ,
(Select StyleColorWays From T_00_SCMT Where StyleCode = VIEW_ERP_PSRSNP_PLAN.StyleCode And StyleColorSerial = VIEW_ERP_PSRSNP_PLAN.StyleColorSerial) StyleColorWays ,
T_00_ICMT.ITEMCODE AS MODULEITEMCODE ,  
NVL(T_00_ICMT.ITEMNAME,'Ungroup') AS MODULENAME , 
T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL ,
T_SD_PTMT.PIECEUNIQUE , T_SD_PTMT.PIECE ,
VIEW_ERP_PSRSNP_PLAN.PLANQTY  * T_SD_PTMT.PIECEQTY  AS TOTALPIECEQTY , 
CASE WHEN NVL(T_CT_CDPT.ALLOCATEDQTY,0) > (VIEW_ERP_PSRSNP_PLAN.PLANQTY  * T_SD_PTMT.PIECEQTY) THEN (VIEW_ERP_PSRSNP_PLAN.PLANQTY * T_SD_PTMT.PIECEQTY)
ELSE NVL(T_CT_CDPT.ALLOCATEDQTY,0) 
END ALLOCATEDQTY, 
NVL(T_CT_CDPT.ALLOCATEDQTY,0) AS ALLOCATEDQTYSHOW,
(Select ItemName From PKERP.T_00_ICMT Where ItemCode = NVL(T_SD_BOMT.ItemCode , T_SD_PTMT.ItemCode) ) as ItemName,
(Select ItemName From PKERP.T_00_ICCM Where ItemCode = NVL(T_SD_BOMT.ItemCode , T_SD_PTMT.ItemCode) and ItemColorSerial= NVL(T_SD_BOMT.ItemColorSerial , T_SD_PTMT.ItemColorSerial) ) as ItemColorWays
FROM PKERP.VIEW_ERP_PSRSNP_PLAN
LEFT JOIN PKERP.T_SD_BOMT ON 
    VIEW_ERP_PSRSNP_PLAN.STYLECODE = T_SD_BOMT.STYLECODE
    AND VIEW_ERP_PSRSNP_PLAN.STYLESIZE= T_SD_BOMT.STYLESIZE
    AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = T_SD_BOMT.STYLECOLORSERIAL
    AND VIEW_ERP_PSRSNP_PLAN.REVNO = T_SD_BOMT.REVNO
JOIN PKERP.T_SD_PTMT ON 
    T_SD_BOMT.STYLECODE = T_SD_PTMT.STYLECODE
    AND T_SD_BOMT.STYLESIZE= T_SD_PTMT.STYLESIZE
    AND T_SD_BOMT.STYLECOLORSERIAL = T_SD_PTMT.STYLECOLORSERIAL
    AND T_SD_BOMT.REVNO = T_SD_PTMT.REVNO
    AND T_SD_BOMT.MAINITEMCODE = T_SD_PTMT.MAINITEMCODE
    AND T_SD_BOMT.MAINITEMCOLORSERIAL = T_SD_PTMT.MAINITEMCOLORSERIAL
    AND T_SD_BOMT.ITEMCODE = T_SD_PTMT.ITEMCODE
    AND T_SD_BOMT.ITEMCOLORSERIAL = T_SD_PTMT.ITEMCOLORSERIAL
LEFT JOIN PKPCM.T_CT_CDPT ON 
    VIEW_ERP_PSRSNP_PLAN.PRDPKG = T_CT_CDPT.PRDPKG    
    AND T_SD_BOMT.MAINITEMCODE = T_CT_CDPT.MAINITEMCODE
    AND T_SD_BOMT.MAINITEMCOLORSERIAL = T_CT_CDPT.MAINITEMCOLORSERIAL
    AND T_SD_BOMT.ITEMCODE = T_CT_CDPT.ITEMCODE
    AND T_SD_BOMT.ITEMCOLORSERIAL = T_CT_CDPT.ITEMCOLORSERIAL
    AND T_SD_PTMT.PIECEUNIQUE = T_CT_CDPT.PIECEUNIQUE
LEFT JOIN PKERP.T_SD_MPTN ON 
    T_SD_PTMT.STYLECODE =T_SD_MPTN.STYLECODE
    AND T_SD_PTMT.STYLESIZE =T_SD_MPTN.STYLESIZE
    AND T_SD_PTMT.STYLECOLORSERIAL =T_SD_MPTN.STYLECOLORSERIAL
    AND T_SD_PTMT.REVNO =T_SD_MPTN.REVNO
    AND T_SD_PTMT.MAINITEMCODE =T_SD_MPTN.MAINITEMCODE
    AND T_SD_PTMT.MAINITEMCOLORSERIAL =T_SD_MPTN.MAINITEMCOLORSERIAL
    AND T_SD_PTMT.ITEMCODE =T_SD_MPTN.ITEMCODE
    AND T_SD_PTMT.ITEMCOLORSERIAL =T_SD_MPTN.ITEMCOLORSERIAL
    AND T_SD_PTMT.PIECEUNIQUE  =T_SD_MPTN.PIECEUNIQUE
LEFT JOIN PKERP.T_SD_MBOM  ON 
    T_SD_BOMT.STYLECODE =T_SD_MBOM.STYLECODE
    AND T_SD_BOMT.STYLESIZE =T_SD_MBOM.STYLESIZE
    AND T_SD_BOMT.STYLECOLORSERIAL =T_SD_MBOM.STYLECOLORSERIAL
    AND T_SD_BOMT.REVNO =T_SD_MBOM.REVNO
    AND T_SD_BOMT.MAINITEMCODE =T_SD_MBOM.MAINITEMCODE
    AND T_SD_BOMT.MAINITEMCOLORSERIAL =T_SD_MBOM.MAINITEMCOLORSERIAL
    AND T_SD_BOMT.ITEMCODE =T_SD_MBOM.ITEMCODE
    AND T_SD_BOMT.ITEMCOLORSERIAL =T_SD_MBOM.ITEMCOLORSERIAL
LEFT JOIN PKERP.T_00_ICMT ON 
    NVL(T_SD_MPTN.MODULEITEMCODE , T_SD_MBOM.MODULEITEMCODE ) = T_00_ICMT.ITEMCODE   
WHERE 1=1  
AND  VIEW_ERP_PSRSNP_PLAN.PRDPKG = '{PP}'
AND T_SD_BOMT.ITEMCODE NOT LIKE 'BNF%'
";

            DataTable dt = new DataTable();
            dt = OracleDbManager.Query(strSQL, null);
            System.Data.DataColumn newdc = new System.Data.DataColumn("PPTotalPiece", typeof(System.Int32));
            dt.Columns.Add(newdc);
            int TotalPcs = 0;
            foreach (DataRow dr in dt.Rows)
            {
                TotalPcs += Int32.Parse(dr["TOTALPIECEQTY"].ToString());
            }
            dt.Rows[0]["PPTotalPiece"] = TotalPcs;
            return PartialView("MTOPPPCuttingReadinessPop", dt);
        }
        public ActionResult ThreeWeekQCO()
        {
            return View();
        }
        #endregion

        #region GRID
        public string ShowQCPS(GridSettings gridRequest)
        {
            DateTime dtStart = DateTime.Now;
            string idFactory = Url.RequestContext.RouteData.Values["id"].ToString();
            string vstrQCORankingFilter = Url.RequestContext.HttpContext.Request["QCORankingFilter"];
            string vstrQCOSource = Url.RequestContext.HttpContext.Request["QCOSource"];
            //2019-11-19 Tai Le (Thomas)
            string vstrIncSample = Url.RequestContext.HttpContext.Request["IncSample"];
            if (String.IsNullOrEmpty(idFactory))
                return JsonConvert.SerializeObject(new { retResult = false, dataRow = "Please Choose Factory; Year; WeekNo To Visualize The QCO." });
            try
            {
                string strSQL = "",
                    strSQLWhere = "";
                strSQL =
                    " SELECT ROW_NUMBER() OVER(ORDER BY  NVL(CHANGEQCORANK, QCORANK) ) AS RANKING , '" + vstrQCOSource + "' QCOSource , " +
                    " T_00_STMT.STYLENAME , T_00_STMT.BUYERSTYLECODE , T_00_STMT.BUYERSTYLENAME , T_00_SCMT.STYLECOLORWAYS , " +
                    " T_QC_QUEUE.* , " +
                    " VIEW_ERP_PSRSNP_PLAN.ADTYPENAME , VIEW_ERP_PSRSNP_PLAN.ADTYPE , " +
                    " TO_DATE(T_QC_QUEUE.PRDSDAT , 'yyyyMMdd') AOPRDSDAT , " +
                    " TO_DATE(T_QC_QUEUE.PRDEDAT , 'yyyyMMdd') AOPRDEDAT , " +
                    " VIEW_ERP_PSRSNP_PLAN.DELIVERYDATE as AODELIVERYDATE , " +
                    " VIEW_ERP_PSRSNP_PLAN.PLANQTY as AOPLANQTY , " +
                    " VIEW_ERP_PSRSNP_PLAN.FACTORY AS ChangeFactory , " +
                    " NVL(CHANGEQCORANK, QCORANK) as CHANGEQCORANKSHOW , " +
                    " CASE FINSOREADINESS WHEN '1' THEN 'Yes' Else 'No' End As FINSOREADINESSSHOW ," +
                    " CASE JIGREADINESS WHEN '1' THEN 'Yes' Else 'No' End As JIGREADINESSSHOW ," +
                    " CASE SOPREADINESS WHEN '1' THEN 'Yes' Else 'No' End As SOPREADINESSSHOW " +
                    " FROM {TableName}   T_QC_QUEUE " +
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
                    "       AND T_QC_QUEUE.PRDPKG = VIEW_ERP_PSRSNP_PLAN.PRDPKG ";
                strSQLWhere = " T_QC_QUEUE.QCOFACTORY = '" + idFactory + "'  ";
                if (!String.IsNullOrEmpty(vstrQCORankingFilter))
                {
                    switch (vstrQCORankingFilter)
                    {
                        case "Neg":
                            strSQLWhere += " AND T_QC_QUEUE.QCORANK <0 ";
                            break;
                        case "Pos":
                            strSQLWhere += " AND T_QC_QUEUE.QCORANK >=0 ";
                            break;
                    }
                }
                //if (!String.IsNullOrEmpty(vstrIncSample)) {   if (vstrIncSample == "N") { strSQLWhere += " AND VIEW_ERP_PSRSNP_PLAN.ADTYPE <> 'S' "; } }
                var RealTableName = "";
                if (vstrQCOSource == "QCO")
                    RealTableName = " PKMES.T_QC_QUEUE ";
                else if (vstrQCOSource == "QCOSim")
                    RealTableName = " PKMES.T_QC_QUEUESIM ";
                strSQL = strSQL.Replace("{TableName}", RealTableName);
                var _Result = GridData.GetGridData(ConstantGeneric.ConnectionStr, strSQL, strSQLWhere, gridRequest, "dd MMM, yyyy");
                return _Result;
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { retResult = false, dataRow = ex.Message });
            }
        }
        public string ShowCoprQCO(GridSettings gridRequest)
        {
            string CorpID = Url.RequestContext.RouteData.Values["id"].ToString();

            if (String.IsNullOrEmpty(CorpID))
                return JsonConvert.SerializeObject(new { retResult = false, dataRow = "Please Choose Factory; Year; WeekNo To Visualize The QCO." });

            var RealTableName = " PKMES.T_QC_QUEUE ";
            string vstrQCORankingFilter = Url.RequestContext.HttpContext.Request["QCORankingFilter"];

            string QCOYEAR = Url.RequestContext.HttpContext.Request["QCOYEAR"];
            string QCOWEEKNO = Url.RequestContext.HttpContext.Request["QCOWEEKNO"];

            string SortingPara = Url.RequestContext.HttpContext.Request["SortPara"];
            var arrSortingPara = SortingPara.Split(',');
            SortingPara = "";
              
            foreach (string item in arrSortingPara)
            {
                var seperator = String.IsNullOrEmpty(SortingPara) ? "" : ",";

                var arrItem = item.Split(';');
                SortingPara += $"{seperator} {arrItem[0]} {arrItem[1]}";
            }

            //string vstrQCOSource = Url.RequestContext.HttpContext.Request["QCOSource"];
            //2019-11-19 Tai Le (Thomas)
            //string vstrIncSample = Url.RequestContext.HttpContext.Request["IncSample"];

            try
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                string strSQL = 
                    " SELECT " +
                    " T_00_STMT.STYLENAME , T_00_STMT.BUYERSTYLECODE , T_00_STMT.BUYERSTYLENAME , T_00_SCMT.STYLECOLORWAYS , " +
                    " T_QC_QUEUE.* , " +
                    " VIEW_ERP_PSRSNP_PLAN.ADTYPENAME , VIEW_ERP_PSRSNP_PLAN.ADTYPE , " +
                    " TO_DATE(T_QC_QUEUE.PRDSDAT , 'yyyyMMdd') AOPRDSDAT , " +
                    " TO_DATE(T_QC_QUEUE.PRDEDAT , 'yyyyMMdd') AOPRDEDAT , " +
                    " VIEW_ERP_PSRSNP_PLAN.DELIVERYDATE as AODELIVERYDATE , " +
                    " VIEW_ERP_PSRSNP_PLAN.PLANQTY as AOPLANQTY , " +
                    " VIEW_ERP_PSRSNP_PLAN.FACTORY AS ChangeFactory , " +
                    " NVL(CHANGEQCORANK, QCORANK) as CHANGEQCORANKSHOW , " +
                    " CASE FINSOREADINESS WHEN '1' THEN 'Yes' Else 'No' End As FINSOREADINESSSHOW ," +
                    " CASE JIGREADINESS WHEN '1' THEN 'Yes' Else 'No' End As JIGREADINESSSHOW ," +
                    " CASE SOPREADINESS WHEN '1' THEN 'Yes' Else 'No' End As SOPREADINESSSHOW " +
                    " , T_QC_QUEUE.NORMALIZEDPERCENT as MATNORNALRATE , T_QC_QUEUE.MATPRIORITYA as MATPRIORITYLEV1 , T_QC_QUEUE.MATPRIORITYB as MATPRIORITYLEV2 , T_QC_QUEUE.MATPRIORITYC as MATPRIORITYLEV3 " +
                    " FROM {TableName}  T_QC_QUEUE " +
                    "   INNER JOIN PKERP.T_CM_FCMT ON T_QC_QUEUE.QCOFACTORY = T_CM_FCMT.FACTORY " +
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
                    $"      AND VIEW_ERP_PSRSNP_PLAN.FACTORY = T_QC_QUEUE.QCOFACTORY " +
                    $" WHERE T_CM_FCMT.CORPORATION = '{CorpID}' " +
                    $"      AND T_QC_QUEUE.QCOYear= {QCOYEAR}  "+
                    $"      AND T_QC_QUEUE.QCOWEEKNO = '{QCOWEEKNO}'  "
                    ;

                if (!String.IsNullOrEmpty(vstrQCORankingFilter))
                {
                    switch (vstrQCORankingFilter)
                    {
                        case "Neg":
                            strSQL += " AND T_QC_QUEUE.QCORANK <0 ";
                            break;
                        case "Pos":
                            strSQL += " AND T_QC_QUEUE.QCORANK >=0 ";
                            break;
                    }
                }

                strSQL = strSQL.Replace("{TableName}", RealTableName);

                sb.AppendLine($@"
SELECT ROW_NUMBER() OVER( ORDER BY {SortingPara} ) AS RANKING , 
Main.* 
FROM ({strSQL} 
) MAIN
");

                var _Result = GridData.GetGridData(ConstantGeneric.ConnectionStr, sb.ToString(), String.Empty, gridRequest, "dd MMM, yyyy");
                return _Result;
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { retResult = false, dataRow = ex.Message });
            }
        }

        public string ShowQCOReadiness(GridSettings gridRequest)
        {
            DateTime dtStart = DateTime.Now;
            string idFactory = Url.RequestContext.RouteData.Values["id"].ToString();
            string vstrQCORankingFilter = Url.RequestContext.HttpContext.Request["QCORankingFilter"];
            string vstrQCOSource = Url.RequestContext.HttpContext.Request["QCOSource"];
            //2019-11-19 Tai Le (Thomas)
            string vstrIncSample = Url.RequestContext.HttpContext.Request["IncSample"];
            if (String.IsNullOrEmpty(idFactory))
                return JsonConvert.SerializeObject(new { retResult = false, dataRow = "Please Choose Factory; Year; WeekNo To Visualize The QCO." });
            try
            {
                string strSQL = "",
                    strSQLWhere = "";
                strSQL =
                    " SELECT ROW_NUMBER() OVER(ORDER BY  NVL(T_QC_QUEUE.CHANGEQCORANK, T_QC_QUEUE.QCORANK) ) AS RANKING , '" + vstrQCOSource + "' QCOSource , " +
                    " T_QC_QUEUE.CHANGEQCORANK, T_QC_QUEUE.QCORANK , T_QC_QUEUE.QCOFactory , T_QC_QUEUE.QCOYear , T_QC_QUEUE.QCOWeekNo , " +
                    " T_QC_QUEUE.AONO, T_QC_QUEUE.PRDPKG, T_QC_QUEUE.PLANQTY, " +
                    " T_00_STMT.STYLENAME , T_00_STMT.BUYERSTYLECODE , T_00_STMT.BUYERSTYLENAME , T_00_SCMT.STYLECOLORWAYS , " +
                    " T_QC_QUEUE.StyleSize," +
                    " T_QC_QUEUE.StyleColorSerial, " +
                    " T_QC_QUEUE.RevNo , " +
                    " TO_DATE(T_QC_QUEUE.PRDSDAT , 'yyyyMMdd') AOPRDSDAT , " +
                    " TO_DATE(T_QC_QUEUE.PRDEDAT , 'yyyyMMdd') AOPRDEDAT , " +
                    " T_QC_QUEUE.DELIVERYDATE , " +
                    " FINSOREADINESS , " +
                    " JIGREADINESS , " +
                    " SOPREADINESS " +
                    " FROM {TableName}   T_QC_QUEUE " +
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
                    "       AND T_QC_QUEUE.PRDPKG = VIEW_ERP_PSRSNP_PLAN.PRDPKG ";
                strSQLWhere = " T_QC_QUEUE.QCOFACTORY = '" + idFactory + "'  ";
                if (!String.IsNullOrEmpty(vstrQCORankingFilter))
                {
                    switch (vstrQCORankingFilter)
                    {
                        case "Neg":
                            strSQLWhere += " AND T_QC_QUEUE.QCORANK <0 ";
                            break;
                        case "Pos":
                            strSQLWhere += " AND T_QC_QUEUE.QCORANK >=0 ";
                            break;
                    }
                }
                //if (!String.IsNullOrEmpty(vstrIncSample)) {   if (vstrIncSample == "N") { strSQLWhere += " AND VIEW_ERP_PSRSNP_PLAN.ADTYPE <> 'S' "; } }
                var RealTableName = "";
                if (vstrQCOSource == "QCO")
                    RealTableName = " PKMES.T_QC_QUEUE ";
                else if (vstrQCOSource == "QCOSim")
                    RealTableName = " PKMES.T_QC_QUEUESIM ";
                strSQL = strSQL.Replace("{TableName}", RealTableName);
                var _Result = GridData.GetGridData(ConstantGeneric.ConnectionStr, strSQL, strSQLWhere, gridRequest, "dd MMM, yyyy");
                return _Result;
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { retResult = false, dataRow = ex.Message });
            }
        }
        [SysActionFilter(SystemID = "MES", MenuID = "QCO", Action = "EXPORT")]
        public ActionResult QCPSToExcel(GridSettings gridRequest)
        {
            DateTime dtStart = DateTime.Now;
            //Debug.Print("ShowQCPS(): START " + dtStart.ToString("s"));
            string idFactory = Url.RequestContext.RouteData.Values["id"].ToString();
            string vstrQCORankingFilter = Url.RequestContext.HttpContext.Request["QCORankingFilter"];
            string QCOSource = Url.RequestContext.HttpContext.Request["QCOSource"];   //2019-11-06 Tai Le (Thomas) 
            string ExcelType =
                Url.RequestContext.HttpContext.Request["Type"] == null
                    ? "A"
                    : Url.RequestContext.HttpContext.Request["Type"].ToString();   //2020-04-08 Tai Le (Thomas) 
            if (String.IsNullOrEmpty(idFactory))
            {
                //new EmptyResult();
                return Json(new { retResult = false, retMsg = "\"Factory\" is required." }, JsonRequestBehavior.AllowGet);
            }
            if (String.IsNullOrEmpty(QCOSource))
            {
                //new EmptyResult();
                return Json(new { retResult = false, retMsg = "\"QCO Source\" is required." }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                string strSQL = "",
                    strSQLWhere = "";
                if (QCOSource.ToUpper().Trim() == "QCOSIM")
                    cTableName = "PKMES.T_QC_QUEUESIM";
                strSQL =
                    $" SELECT '" + QCOSource + "' QCOSOURCE , ROW_NUMBER() OVER(ORDER BY NVL(T_QC_QUEUE.CHANGEQCORANK, T_QC_QUEUE.QCORANK) ) AS RANKING ," +
                    $" T_00_STMT.STYLENAME , T_00_STMT.BUYERSTYLECODE , T_00_STMT.BUYERSTYLENAME , T_00_SCMT.STYLECOLORWAYS , " +
                    $" T_QC_QUEUE.QCOFACTORY, " +
                    $" T_QC_QUEUE.QCOYEAR, T_QC_QUEUE.QCOWEEKNO, T_QC_QUEUE.QCORANK, T_QC_QUEUE.CHANGEQCORANK As CustomRanking,  T_QC_QUEUE.PRECUSTOMRANK, " +
                    $" T_QC_QUEUE.FACTORY, T_QC_QUEUE.LINENO, " +
                    $" T_QC_QUEUE.AONO, VIEW_ERP_PSRSNP_PLAN.ADTYPENAME , T_QC_QUEUE.BUYER, T_QC_QUEUE.STYLECODE, T_QC_QUEUE.STYLESIZE, T_QC_QUEUE.STYLECOLORSERIAL, T_QC_QUEUE.REVNO, " +
                    $" T_QC_QUEUE.PRDPKG, TO_CHAR(T_QC_QUEUE.CREATEDATE, 'yyyy/MM/dd') CREATEDATE, T_QC_QUEUE.NORMALIZEDPERCENT, '' as QCOSTARTDATE, '' as CHANGEQCORANK, '' as REASON, T_QC_QUEUE.CHANGEBY as PreviousCHANGEBY, T_QC_QUEUE.CHANGEQCORANK as PreviousCHANGEQCORANK , T_QC_QUEUE.REASON as PreviousReason , " +
                    $" T_QC_QUEUE.PLANQTY,  TO_CHAR(T_QC_QUEUE.DELIVERYDATE , 'yyyy/MM/dd') DELIVERYDATE , T_QC_QUEUE.PRDSDAT, T_QC_QUEUE.PRDEDAT, T_QC_QUEUE.ORDQTY, T_QC_QUEUE.QCORANKINGNEW, T_QC_QUEUE.LATESTQCOTIME , " +
                    $" TO_DATE(T_QC_QUEUE.PRDSDAT , 'yyyyMMdd') AOPRDSDAT , " +
                    $" TO_DATE(T_QC_QUEUE.PRDEDAT , 'yyyyMMdd') AOPRDEDAT , " +
                    $" TO_CHAR(VIEW_ERP_PSRSNP_PLAN.DELIVERYDATE , 'yyyy/MM/dd' ) as AODELIVERYDATE , " +
                    $" VIEW_ERP_PSRSNP_PLAN.PLANQTY as AOPLANQTY , " +
                    $" VIEW_ERP_PSRSNP_PLAN.FACTORY AS ChangeFactory , " +
                    $" NVL(CHANGEQCORANK, QCORANK) as CHANGEQCORANKSHOW , T_QC_QUEUE.QCOVERSION " +
                    $" FROM {cTableName} T_QC_QUEUE " +
                    $"   INNER JOIN PKERP.T_00_SCMT  ON " +
                    $"       T_QC_QUEUE.STYLECODE = T_00_SCMT.STYLECODE " +
                    $"       AND T_QC_QUEUE.STYLECOLORSERIAL = T_00_SCMT.STYLECOLORSERIAL " +
                    $"   INNER JOIN PKERP.T_00_STMT  ON " +
                    $"       T_QC_QUEUE.STYLECODE = T_00_STMT.STYLECODE " +
                    $"   LEFT JOIN PKERP.VIEW_ERP_PSRSNP_PLAN ON " +
                    $"       T_QC_QUEUE.AONO = VIEW_ERP_PSRSNP_PLAN.AONO " +
                    $"       AND T_QC_QUEUE.STYLECODE = VIEW_ERP_PSRSNP_PLAN.STYLECODE " +
                    $"       AND T_QC_QUEUE.STYLESIZE = VIEW_ERP_PSRSNP_PLAN.STYLESIZE " +
                    $"       AND T_QC_QUEUE.STYLECOLORSERIAL = VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL " +
                    $"       AND T_QC_QUEUE.REVNO = VIEW_ERP_PSRSNP_PLAN.REVNO " +
                    $"       AND T_QC_QUEUE.PRDPKG = VIEW_ERP_PSRSNP_PLAN.PRDPKG " +
                    $" ";
                if (ExcelType == "B")
                    /// 2020-05-07 Tai Le(Thomas)
                    /// Add 2 columns: QCOSTARTDATE; CurrentQCOSTARTDATE
                    strSQL = $@"
SELECT '{QCOSource}' QCOSOURCE , ROW_NUMBER() OVER(ORDER BY NVL(T_QC_QUEUE.CHANGEQCORANK, T_QC_QUEUE.QCORANK) ) AS RANKING ,
	T_00_STMT.STYLENAME , T_00_STMT.BUYERSTYLECODE , T_00_STMT.BUYERSTYLENAME , T_00_SCMT.STYLECOLORWAYS , 
	T_QC_QUEUE.QCOFACTORY, 
	T_QC_QUEUE.QCOYEAR, T_QC_QUEUE.QCOWEEKNO, T_QC_QUEUE.QCORANK, T_QC_QUEUE.CHANGEQCORANK As CustomRanking,  T_QC_QUEUE.PRECUSTOMRANK, 
	T_QC_QUEUE.FACTORY, T_QC_QUEUE.LINENO, 
	T_QC_QUEUE.AONO, VIEW_ERP_PSRSNP_PLAN.ADTYPENAME , T_QC_QUEUE.BUYER, T_QC_QUEUE.STYLECODE, T_QC_QUEUE.STYLESIZE, T_QC_QUEUE.STYLECOLORSERIAL, T_QC_QUEUE.REVNO, 
	T_QC_QUEUE.PRDPKG, T_QC_QUEUE.NORMALIZEDPERCENT, T_QC_QUEUE.PLANQTY,  TO_CHAR(T_QC_QUEUE.DELIVERYDATE, 'yyyy/MM/dd') DELIVERYDATE , 
	TO_CHAR(VIEW_ERP_PSRSNP_PLAN.DELIVERYDATE, 'yyyy/MM/dd') as AODELIVERYDATE , T_QC_QUEUE.QCOVERSION , 
	T_QC_QUEUE.FINSOREADINESS , T_QC_QUEUE.JIGREADINESS , T_QC_QUEUE.SOPREADINESS , '' as QCOSTARTDATE , To_char(T_QC_QUEUE.QCOSTARTDATE, 'yyyy-MM-dd') as CurrentQCOSTARTDATE
FROM { cTableName} T_QC_QUEUE
INNER JOIN PKERP.T_00_SCMT ON
	T_QC_QUEUE.STYLECODE = T_00_SCMT.STYLECODE
	AND T_QC_QUEUE.STYLECOLORSERIAL = T_00_SCMT.STYLECOLORSERIAL
INNER JOIN PKERP.T_00_STMT ON
	T_QC_QUEUE.STYLECODE = T_00_STMT.STYLECODE
LEFT JOIN PKERP.VIEW_ERP_PSRSNP_PLAN ON
	T_QC_QUEUE.AONO = VIEW_ERP_PSRSNP_PLAN.AONO
	AND T_QC_QUEUE.STYLECODE = VIEW_ERP_PSRSNP_PLAN.STYLECODE
	AND T_QC_QUEUE.STYLESIZE = VIEW_ERP_PSRSNP_PLAN.STYLESIZE
	AND T_QC_QUEUE.STYLECOLORSERIAL = VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL
	AND T_QC_QUEUE.REVNO = VIEW_ERP_PSRSNP_PLAN.REVNO
	AND T_QC_QUEUE.PRDPKG = VIEW_ERP_PSRSNP_PLAN.PRDPKG 
";
                strSQLWhere = " T_QC_QUEUE.QCOFACTORY = '" + idFactory + "'  ";
                if (!String.IsNullOrEmpty(vstrQCORankingFilter))
                {
                    switch (vstrQCORankingFilter)
                    {
                        case "Neg":
                            strSQLWhere += " AND T_QC_QUEUE.QCORANK <0 ";
                            break;
                        case "Pos":
                            strSQLWhere += " AND T_QC_QUEUE.QCORANK >=0 ";
                            break;
                    }
                }
                var _Result = GridData.GetGridDataTable(ConstantGeneric.ConnectionStr, strSQL, strSQLWhere, gridRequest);
                if (_Result != null)
                {
                    if (_Result.Rows.Count > 0)
                    {
                        int intI = 1, intJ = 0;
                        var excelPackage = new ExcelPackage();
                        excelPackage.Workbook.Worksheets.Add("QCO");
                        ExcelWorksheet excelWorkSheet = excelPackage.Workbook.Worksheets[1];
                        /*Excel Column and Row is  ONE_BASED  , NOT ZERO_BASED  */
                        //Header
                        for (intJ = 1; intJ <= _Result.Columns.Count; intJ++)
                        {
                            excelWorkSheet.Cells[1, intJ].Value = _Result.Columns[intJ - 1].ColumnName;
                        }
                        //Row_data 
                        for (intI = 0; intI < _Result.Rows.Count; intI++)
                            for (intJ = 0; intJ < _Result.Columns.Count; intJ++)
                                excelWorkSheet.Cells[intI + 2, intJ + 1].Value = _Result.Rows[intI][intJ] != null ? _Result.Rows[intI][intJ] : DBNull.Value;
                        var fileName = $"QCO-{_Result.Rows[0]["QCOFACTORY"].ToString()}";
                        if (QCOSource.ToUpper().Trim() == "QCOSIM")
                            fileName = $"QCOLive-{_Result.Rows[0]["QCOFACTORY"].ToString()}";
                        //2020-04-08 QCO Readiness only avaible on the QCO , not Live QCO
                        if (ExcelType == "B")
                            fileName = $"QCOReadiness-{_Result.Rows[0]["QCOFACTORY"].ToString()}";
                        //Using Stream
                        System.IO.MemoryStream stream = new System.IO.MemoryStream();
                        excelPackage.SaveAs(stream);
                        excelPackage.Dispose();
                        byte[] xlsInBytes = stream.ToArray();
                        if (xlsInBytes.Length > 0)
                            return File(xlsInBytes, "application/octet-stream", fileName + ".xlsx");
                        else
                            return new EmptyResult();
                        //var _filePath = System.IO.Path.Combine(Server.MapPath("~"), $@"{fileName}.xlsx");
                        //if (System.IO.File.Exists(_filePath))
                        //    System.IO.File.Delete(_filePath);  
                        //excelPackage.SaveAs(new System.IO.FileInfo(_filePath)); //Save File to Hard-disk  
                        //if (System.IO.File.Exists(System.IO.Path.Combine(Server.MapPath("~"), $@"{fileName}.pdf")))
                        //    System.IO.File.Delete(System.IO.Path.Combine(Server.MapPath("~"), $@"{fileName}.pdf")); 
                        //EPPlusExcelToPDF ePPlusExcelToPDF = new EPPlusExcelToPDF(); 
                        //var _res= ePPlusExcelToPDF.CreateExcelToPdfReport(_filePath, "QCO"); 
                        //if (System.IO.File.Exists(System.IO.Path.Combine(Server.MapPath("~"), $@"{fileName}.pdf")))
                        //{
                        //    //Temporary disable
                        //    //System.IO.File.Delete(System.IO.Path.Combine(Server.MapPath("~"), $@"{fileName}.xlsx")); 
                        //    var stream = System.IO.File.OpenRead(System.IO.Path.Combine(Server.MapPath("~"), $@"{fileName}.pdf"));
                        //    System.IO.MemoryStream data = new System.IO.MemoryStream();
                        //    stream.CopyTo(data); 
                        //    byte[] xlsInBytes = data.ToArray();
                        //    if (xlsInBytes.Length > 0)
                        //    {
                        //        //return File(xlsInBytes, "application/octet-stream", fileName + ".pdf");
                        //        return File(xlsInBytes, "application/pdf", fileName + ".pdf");
                        //    }
                        //    else
                        //        return new EmptyResult();
                        //}
                        //else
                        //    return new EmptyResult(); 
                    }
                    else
                        return new EmptyResult();
                }
                else
                    return new EmptyResult();
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return new EmptyResult();
            }
        }
        public string BindQCODetail(GridSettings gridRequest)
        {
            string Factory = Url.RequestContext.HttpContext.Request["Factory"],
                LineNo = Url.RequestContext.HttpContext.Request["LineNo"],
                AONo = Url.RequestContext.HttpContext.Request["AONo"],
                StyleCode = Url.RequestContext.HttpContext.Request["StyleCode"],
                StyleSize = Url.RequestContext.HttpContext.Request["StyleSize"],
                StyleColorSerial = Url.RequestContext.HttpContext.Request["StyleColorSerial"],
                RevNo = Url.RequestContext.HttpContext.Request["RevNo"],
                PP = Url.RequestContext.HttpContext.Request["PrdPkg"],
                QCOYEAR = Url.RequestContext.HttpContext.Request["QCOYEAR"],
                QCOWEEK = Url.RequestContext.HttpContext.Request["QCOWEEK"]
                ;
            try
            {
                //Variable Declare
                string strSQL = "";
                decimal totalPages = 0, totalRecords = 0;
                decimal intRowPerPage = gridRequest.pageSize;
                //strSQL = "SELECT ROW_NUMBER() OVER(ORDER BY T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL ) AS SEQNO ,  " +
                //         " VIEW_ERP_PSRSNP_PLAN.FACTORY ,  " +
                //         " VIEW_ERP_PSRSNP_PLAN.LINENO ,  " +
                //         " VIEW_ERP_PSRSNP_PLAN.AONO ,  " +
                //         " VIEW_ERP_PSRSNP_PLAN.STYLECODE , " +
                //         " T_00_STMT.STYLENAME , " +
                //         " VIEW_ERP_PSRSNP_PLAN.STYLESIZE ,  " +
                //         " VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL , " +
                //         " T_00_SCMT.STYLECOLORWAYS, " +
                //         " VIEW_ERP_PSRSNP_PLAN.REVNO ,  " +
                //         " VIEW_ERP_PSRSNP_PLAN.PRDPKG ,  " +
                //         " T_SD_BOMT.ITEMCODE , " +
                //         " T_00_ICMT.ITEMNAME ,  " +
                //         " T_SD_BOMT.ITEMCOLORSERIAL || ' - ' ||  T_00_ICCM.ITEMCOLORWAYS as ITEMCOLORSERIAL , " +
                //         " VIEW_ERP_PSRSNP_PLAN.PLANQTY * T_SD_BOMT.UNITCONSUMPTION as REQUESTQTY , " +
                //         " NVL(T_QC_QCPM.QUANTITY_A, 0) QUANTITY_A , " +
                //         " NVL(T_QC_QCPM.QUANTITY_B, 0) QUANTITY_B , " +
                //         " NVL(T_QC_QCPM.QUANTITY_C, 0) QUANTITY_C , " +
                //         " NVL(T_QC_QCPM.QUANTITY_D, 0) QUANTITY_D , " +
                //         " NVL(T_QC_QCPM.PLANQUANTITY, 0)  PLANQUANTITY " +
                //         " FROM PKERP.VIEW_ERP_PSRSNP_PLAN " +
                //         " INNER JOIN PKERP.T_SD_BOMT ON " +
                //         "   VIEW_ERP_PSRSNP_PLAN.STYLECODE = T_SD_BOMT.STYLECODE " +
                //         "   AND VIEW_ERP_PSRSNP_PLAN.STYLESIZE = T_SD_BOMT.STYLESIZE " +
                //         "   AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = T_SD_BOMT.STYLECOLORSERIAL " +
                //         "   AND VIEW_ERP_PSRSNP_PLAN.REVNO = T_SD_BOMT.REVNO " +
                //         " INNER JOIN PKERP.T_00_ICCM  ON " +
                //         "        T_SD_BOMT.ITEMCODE = T_00_ICCM.ITEMCODE " +
                //         "        AND T_SD_BOMT.ITEMCOLORSERIAL = T_00_ICCM.ITEMCOLORSERIAL " +
                //         " INNER JOIN PKERP.T_00_SCMT  ON " +
                //         "        VIEW_ERP_PSRSNP_PLAN.STYLECODE = T_00_SCMT.STYLECODE " +
                //         "        AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = T_00_SCMT.STYLECOLORSERIAL " +
                //         " INNER JOIN PKERP.T_00_ICMT  ON " +
                //         "        T_SD_BOMT.ITEMCODE = T_00_ICMT.ITEMCODE " +
                //         " INNER JOIN PKERP.T_00_STMT  ON " +
                //         "        VIEW_ERP_PSRSNP_PLAN.STYLECODE = T_00_STMT.STYLECODE " +
                //         " LEFT JOIN " +
                //         " ( SELECT T_QC_QCPM.RANGKING , T_QC_QCPM.FACTORY, T_QC_QCPM.LINENO, T_QC_QCPM.AONO, " +
                //         "           T_QC_QCPM.STYLECODE,  T_QC_QCPM.STYLESIZE, " +
                //         "           T_QC_QCPM.STYLECOLORSERIAL , T_QC_QCPM.REVNO, T_QC_QCPM.PRDPKG, " +
                //         "           T_QC_QCPM.ITEMCODE, " +
                //         "           T_QC_QCPM.ITEMCOLORSERIAL , T_QC_QCPM.REQUESTQTY as REQUESTQTY_A ,  " +
                //         "           SUM( nvl(T_QC_QCPM.QUANTITY_A,0 ) ) QUANTITY_A ,   " +
                //         "           SUM( nvl(T_QC_QCPM.QUANTITY_B,0 ) ) QUANTITY_B ,   " +
                //         "           SUM( nvl(T_QC_QCPM.QUANTITY_C,0 ) ) QUANTITY_C ,   " +
                //         "           SUM( nvl(T_QC_QCPM.QUANTITY_D,0 ) ) QUANTITY_D ,   " +
                //         "           SUM( nvl(T_QC_QCPM.PLANQUANTITY,0 ) ) PLANQUANTITY " +
                //         "   FROM PKMES.T_QC_QCPM " +
                //         "   WHERE T_QC_QCPM.FACTORY = '" + Factory + "' " +
                //         "       AND T_QC_QCPM.LINENO = '" + LineNo + "' " +
                //         "       AND T_QC_QCPM.AONO = '" + AONo + "' " +
                //         "       AND T_QC_QCPM.STYLECODE = '" + StyleCode + "' " +
                //         "       AND T_QC_QCPM.STYLESIZE = '" + StyleSize + "' " +
                //         "       AND T_QC_QCPM.STYLECOLORSERIAL = '" + StyleColorSerial + "' " +
                //         "       AND T_QC_QCPM.REVNO = '" + RevNo + "' " +
                //         "       AND T_QC_QCPM.PRDPKG = '" + PP + "' " +
                //         "       AND T_QC_QCPM.QCOYEAR = '" + QCOYEAR + "' " +
                //         "       AND T_QC_QCPM.QCOWEEKNO = '" + QCOWEEK + "' " +
                //         "   GROUP BY T_QC_QCPM.RANGKING , T_QC_QCPM.FACTORY, T_QC_QCPM.LINENO, T_QC_QCPM.AONO, T_QC_QCPM.STYLECODE, T_QC_QCPM.STYLESIZE, T_QC_QCPM.STYLECOLORSERIAL, T_QC_QCPM.REVNO, T_QC_QCPM.PRDPKG, " +
                //         "   T_QC_QCPM.ITEMCODE, T_QC_QCPM.ITEMCOLORSERIAL , T_QC_QCPM.REQUESTQTY   " +
                //         " ) T_QC_QCPM ON " +
                //         "       VIEW_ERP_PSRSNP_PLAN.FACTORY = T_QC_QCPM.FACTORY " +
                //         "       AND VIEW_ERP_PSRSNP_PLAN.LINENO = T_QC_QCPM.LINENO " +
                //         "       AND VIEW_ERP_PSRSNP_PLAN.AONO = T_QC_QCPM.AONO " +
                //         "       AND T_SD_BOMT.STYLECODE = T_QC_QCPM.STYLECODE " +
                //         "       AND T_SD_BOMT.STYLESIZE = T_QC_QCPM.STYLESIZE " +
                //         "       AND T_SD_BOMT.STYLECOLORSERIAL = T_QC_QCPM.STYLECOLORSERIAL " +
                //         "       AND T_SD_BOMT.REVNO = T_QC_QCPM.REVNO " +
                //         "       AND VIEW_ERP_PSRSNP_PLAN.PRDPKG = T_QC_QCPM.PRDPKG " +
                //         "       AND T_SD_BOMT.ITEMCODE = T_QC_QCPM.ITEMCODE " +
                //         "       AND T_SD_BOMT.ITEMCOLORSERIAL = T_QC_QCPM.ITEMCOLORSERIAL " +
                //         "   WHERE VIEW_ERP_PSRSNP_PLAN.FACTORY = '" + Factory + "' " +
                //         "       AND VIEW_ERP_PSRSNP_PLAN.LINENO = '" + LineNo + "' " +
                //         "       AND VIEW_ERP_PSRSNP_PLAN.AONO = '" + AONo + "' " +
                //         "       AND VIEW_ERP_PSRSNP_PLAN.STYLECODE = '" + StyleCode + "' " +
                //         "       AND VIEW_ERP_PSRSNP_PLAN.STYLESIZE = '" + StyleSize + "' " +
                //         "       AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = '" + StyleColorSerial + "' " +
                //         "       AND VIEW_ERP_PSRSNP_PLAN.REVNO = '" + RevNo + "' " +
                //         "       AND VIEW_ERP_PSRSNP_PLAN.PRDPKG = '" + PP + "' " +
                //         "       AND (T_SD_BOMT.ITEMCODE NOT LIKE 'PKG%' OR T_SD_BOMT.ITEMCODE NOT LIKE 'TRE%' ) " +
                //         " ORDER BY T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL ";
                strSQL = "SELECT ROW_NUMBER() OVER(ORDER BY T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL ) AS SEQNO ,  " +
                          " VIEW_ERP_PSRSNP_PLAN.FACTORY ,  " +
                          " VIEW_ERP_PSRSNP_PLAN.LINENO ,  " +
                          " VIEW_ERP_PSRSNP_PLAN.AONO ,  " +
                          " VIEW_ERP_PSRSNP_PLAN.STYLECODE , " +
                          " T_00_STMT.STYLENAME , " +
                          " VIEW_ERP_PSRSNP_PLAN.STYLESIZE ,  " +
                          " VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL , " +
                          " T_00_SCMT.STYLECOLORWAYS, " +
                          " VIEW_ERP_PSRSNP_PLAN.REVNO ,  " +
                          " VIEW_ERP_PSRSNP_PLAN.PRDPKG ,  " +
                          " T_SD_BOMT.ITEMCODE , " +
                          " T_00_ICMT.ITEMNAME ,  " +
                          " T_SD_BOMT.ITEMCOLORSERIAL || ' - ' ||  T_00_ICCM.ITEMCOLORWAYS as ITEMCOLORSERIAL , " +
                          " VIEW_ERP_PSRSNP_PLAN.PLANQTY * T_SD_BOMT.UNITCONSUMPTION as REQUESTQTY , " +
                          " NVL(T_QC_QCPM.QUANTITY_A, 0) QUANTITY_A , " +
                          " NVL(T_QC_QCPM.QUANTITY_B, 0) QUANTITY_B , " +
                          " NVL(T_QC_QCPM.QUANTITY_C, 0) QUANTITY_C , " +
                          " NVL(T_QC_QCPM.QUANTITY_D, 0) QUANTITY_D , " +
                          " NVL(T_QC_QCPM.PLANQUANTITY, 0)  PLANQUANTITY " +
                          " FROM PKERP.VIEW_ERP_PSRSNP_PLAN " +
                          " INNER JOIN PKERP.T_SD_BOMT ON " +
                          "   VIEW_ERP_PSRSNP_PLAN.STYLECODE = T_SD_BOMT.STYLECODE " +
                          "   AND VIEW_ERP_PSRSNP_PLAN.STYLESIZE = T_SD_BOMT.STYLESIZE " +
                          "   AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = T_SD_BOMT.STYLECOLORSERIAL " +
                          "   AND VIEW_ERP_PSRSNP_PLAN.REVNO = T_SD_BOMT.REVNO " +
                          " INNER JOIN PKERP.T_00_ICCM  ON " +
                          "        T_SD_BOMT.ITEMCODE = T_00_ICCM.ITEMCODE " +
                          "        AND T_SD_BOMT.ITEMCOLORSERIAL = T_00_ICCM.ITEMCOLORSERIAL " +
                          " INNER JOIN PKERP.T_00_SCMT  ON " +
                          "        VIEW_ERP_PSRSNP_PLAN.STYLECODE = T_00_SCMT.STYLECODE " +
                          "        AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = T_00_SCMT.STYLECOLORSERIAL " +
                          " INNER JOIN PKERP.T_00_ICMT  ON " +
                          "        T_SD_BOMT.ITEMCODE = T_00_ICMT.ITEMCODE " +
                          " INNER JOIN PKERP.T_00_STMT  ON " +
                          "        VIEW_ERP_PSRSNP_PLAN.STYLECODE = T_00_STMT.STYLECODE " +
                          " LEFT JOIN " +
                          " ( SELECT T_QC_QCPM.RANGKING , T_QC_QCPM.FACTORY, T_QC_QCPM.LINENO, T_QC_QCPM.AONO, " +
                          "           T_QC_QCPM.STYLECODE,  T_QC_QCPM.STYLESIZE, " +
                          "           T_QC_QCPM.STYLECOLORSERIAL , T_QC_QCPM.REVNO, T_QC_QCPM.PRDPKG, " +
                          "           T_QC_QCPM.ITEMCODE, " +
                          "           T_QC_QCPM.ITEMCOLORSERIAL , T_QC_QCPM.REQUESTQTY as REQUESTQTY_A ,  " +
                          "           SUM( nvl(T_QC_QCPM.QUANTITY_A,0 ) ) QUANTITY_A ,   " +
                          "           SUM( nvl(T_QC_QCPM.QUANTITY_B,0 ) ) QUANTITY_B ,   " +
                          "           SUM( nvl(T_QC_QCPM.QUANTITY_C,0 ) ) QUANTITY_C ,   " +
                          "           SUM( nvl(T_QC_QCPM.QUANTITY_D,0 ) ) QUANTITY_D ,   " +
                          "           SUM( nvl(T_QC_QCPM.PLANQUANTITY,0 ) ) PLANQUANTITY " +
                          "   FROM PKMES.T_QC_QCPM " +
                          "   WHERE T_QC_QCPM.FACTORY = '" + Factory + "' " +
                          "       AND T_QC_QCPM.LINENO = '" + LineNo + "' " +
                          "       AND T_QC_QCPM.AONO = '" + AONo + "' " +
                          "       AND T_QC_QCPM.STYLECODE = '" + StyleCode + "' " +
                          "       AND T_QC_QCPM.STYLESIZE = '" + StyleSize + "' " +
                          "       AND T_QC_QCPM.STYLECOLORSERIAL = '" + StyleColorSerial + "' " +
                          "       AND T_QC_QCPM.REVNO = '" + RevNo + "' " +
                          "       AND T_QC_QCPM.PRDPKG = '" + PP + "' " +
                          "       AND T_QC_QCPM.QCOWEEKNO = '" + QCOWEEK + "' " +
                          "       AND T_QC_QCPM.QCOYEAR = '" + QCOYEAR + "' " +
                          "   GROUP BY T_QC_QCPM.RANGKING , T_QC_QCPM.FACTORY, T_QC_QCPM.LINENO, T_QC_QCPM.AONO, T_QC_QCPM.STYLECODE, T_QC_QCPM.STYLESIZE, T_QC_QCPM.STYLECOLORSERIAL, T_QC_QCPM.REVNO, T_QC_QCPM.PRDPKG, " +
                          "   T_QC_QCPM.ITEMCODE, T_QC_QCPM.ITEMCOLORSERIAL , T_QC_QCPM.REQUESTQTY   " +
                          " ) T_QC_QCPM ON " +
                          "       VIEW_ERP_PSRSNP_PLAN.FACTORY = T_QC_QCPM.FACTORY " +
                          "       AND VIEW_ERP_PSRSNP_PLAN.LINENO = T_QC_QCPM.LINENO " +
                          "       AND VIEW_ERP_PSRSNP_PLAN.AONO = T_QC_QCPM.AONO " +
                          "       AND T_SD_BOMT.STYLECODE = T_QC_QCPM.STYLECODE " +
                          "       AND T_SD_BOMT.STYLESIZE = T_QC_QCPM.STYLESIZE " +
                          "       AND T_SD_BOMT.STYLECOLORSERIAL = T_QC_QCPM.STYLECOLORSERIAL " +
                          "       AND T_SD_BOMT.REVNO = T_QC_QCPM.REVNO " +
                          "       AND VIEW_ERP_PSRSNP_PLAN.PRDPKG = T_QC_QCPM.PRDPKG " +
                          "       AND T_SD_BOMT.ITEMCODE = T_QC_QCPM.ITEMCODE " +
                          "       AND T_SD_BOMT.ITEMCOLORSERIAL = T_QC_QCPM.ITEMCOLORSERIAL " +
                          "   WHERE VIEW_ERP_PSRSNP_PLAN.FACTORY = '" + Factory + "' " +
                          "       AND VIEW_ERP_PSRSNP_PLAN.LINENO = '" + LineNo + "' " +
                          "       AND VIEW_ERP_PSRSNP_PLAN.AONO = '" + AONo + "' " +
                          "       AND VIEW_ERP_PSRSNP_PLAN.STYLECODE = '" + StyleCode + "' " +
                          "       AND VIEW_ERP_PSRSNP_PLAN.STYLESIZE = '" + StyleSize + "' " +
                          "       AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = '" + StyleColorSerial + "' " +
                          "       AND VIEW_ERP_PSRSNP_PLAN.REVNO = '" + RevNo + "' " +
                          "       AND VIEW_ERP_PSRSNP_PLAN.PRDPKG = '" + PP + "' " +
                          "       AND (T_SD_BOMT.ITEMCODE NOT LIKE 'PKG%' OR T_SD_BOMT.ITEMCODE NOT LIKE 'TRE%' ) " +
                          " ORDER BY T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL ";
                DataTable dt = new DataTable();
                dt = OracleDbManager.Query(strSQL, null);
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
                                 " WHERE SEQNO >= " + StartingIndex + " " +
                                 " AND   SEQNO <= " + EndIndex +
                                 " ORDER BY SEQNO ";
                dt = OracleDbManager.Query(strMainSQL, null);
                var tmpResult = JsonConvert.SerializeObject(new
                {
                    total = totalPages,
                    page = gridRequest.pageIndex,
                    records = totalRecords,
                    rows = dt
                }, new JsonSerializerSettings { DateFormatString = "yyyy/MM/dd" });
                dt.Dispose();
                return tmpResult;
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new { retResult = false, retRowData = e.Message });
            }
        }
        #endregion

        #region Bind Data to DOM 
        public ActionResult GetFactorySettings(string vstrFactoryId)
        {
            string idDataValue = vstrFactoryId; //Url.RequestContext.RouteData.Values["id"].ToString();
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
        public ActionResult GetFactoryList(string vstrFactory)
        {
            return Json(OPS_DAL.MesBus.FcmtBus.GetCentralFactories(""), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetAOTypeList(string vstrFactory)
        {
            return Json(OPS_DAL.QCOBus.McmtBus.GetAOTypeList(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadThreeWeekChart(string pFactory, string pYear, string pWeekNo, string pTopRecords)
        {
            //Handle Previous Week No
            int PreYear = 0, PPreYear = 0, PreWeekNo = 0, PPreWeekNo = 0;
            string strPreWeekNo = "", strPPreWeekNo = "";
            if (pWeekNo == "W01")
            {
                PreYear = Convert.ToInt32(pYear) - 1;
                PPreYear = PreYear;
                strPreWeekNo = MaxQCOWeekNo(PreYear);
                PreWeekNo = QCOWeekToInt(strPreWeekNo);
                PPreWeekNo = PreWeekNo - 1;
                strPPreWeekNo = PPreWeekNo < 10 ? "W0" + PPreWeekNo : "W" + PPreWeekNo;
            }
            else
            {
                PreYear = Convert.ToInt32(pYear);
                PPreYear = PreYear;
                PreWeekNo = QCOWeekToInt(pWeekNo) - 1;
                strPreWeekNo = PreWeekNo < 10 ? "W0" + PreWeekNo : "W" + PreWeekNo;
                PPreWeekNo = PreWeekNo - 1;
                strPPreWeekNo = PPreWeekNo < 10 ? "W0" + PPreWeekNo : "W" + PPreWeekNo;
            }
            int TopRecords = 10;
            if (!String.IsNullOrEmpty(pTopRecords)) TopRecords = Convert.ToInt32(pTopRecords);
            var sql = $@"
    Select T_QC_QUEUE.PRDPKG , 
    NVL(NVL( T_QC_QUEUE.CHANGEQCORANK , T_QC_QUEUE.QCORANK),0) QCORANK , 
    NVL(NVL( PREA.CHANGEQCORANK , PREA.QCORANK),0) AS QCORANK_A , 
    NVL(NVL( PREB.CHANGEQCORANK , PREB.QCORANK),0) AS QCORANK_B
    From T_QC_QUEUE
        LEFT JOIN  T_QC_QUEUE  PREA ON 
            PREA.PRDPKG = T_QC_QUEUE.PRDPKG 
            AND PREA.QCOFactory = T_QC_QUEUE.QCOFactory 
            AND PREA.QCOYear = {PreYear}
            AND PREA.QCOWEEKNO = '{strPreWeekNo}'
        LEFT JOIN  T_QC_QUEUE  PREB ON 
            PREB.PRDPKG = T_QC_QUEUE.PRDPKG 
            AND PREB.QCOFactory = T_QC_QUEUE.QCOFactory 
            AND PREB.QCOYear = {PPreYear}
            AND PREB.QCOWEEKNO = '{strPPreWeekNo}'
    Where T_QC_QUEUE.QCOFactory = '{pFactory}'
    And T_QC_QUEUE.QCOYear = {pYear}
    And T_QC_QUEUE.QCOWEEKNO = '{pWeekNo}' 
    And NVL( T_QC_QUEUE.CHANGEQCORANK , T_QC_QUEUE.QCORANK) >=1 and NVL( T_QC_QUEUE.CHANGEQCORANK , T_QC_QUEUE.QCORANK) <= {TopRecords} 
    Order By NVL(NVL( T_QC_QUEUE.CHANGEQCORANK , T_QC_QUEUE.QCORANK),0) 
";
            var _labels = new List<string>();
            _labels.Add($"{pYear}/{pWeekNo}");
            _labels.Add($"{PreYear}/{strPreWeekNo}");
            _labels.Add($"{PPreYear}/{strPPreWeekNo}");
            var _series = new List<string>();
            var _data1 = new List<int>();
            var _data2 = new List<int>();
            var _data3 = new List<int>();
            var _rawData = new List<string[]>();
            //var _rawDataDetail = new List<string>();
            using (OracleConnection oracleCnn = new OracleConnection(OPS_Utils.ConstantGeneric.ConnectionStrMes))
            {
                oracleCnn.Open();
                using (OracleCommand cmd = new OracleCommand())
                {
                    cmd.Connection = oracleCnn;
                    cmd.CommandText = sql;
                    var _reader = cmd.ExecuteReader();
                    if (_reader.HasRows)
                    {
                        while (_reader.Read())
                        {
                            if (_reader["PRDPKG"] != null)
                            {
                                _series.Add(_reader["PRDPKG"].ToString());
                                _data1.Add(Convert.ToInt32(_reader["QCORANK_B"]));
                                _data2.Add(Convert.ToInt32(_reader["QCORANK_A"]));
                                _data3.Add(Convert.ToInt32(_reader["QCORANK"]));
                                //Detail:
                                //_rawDataDetail.Clear();
                                //_rawDataDetail.Add(_reader["PRDPKG"].ToString());
                                //_rawDataDetail.Add(_reader["QCORANK_B"].ToString());
                                //_rawDataDetail.Add(_reader["QCORANK_A"].ToString());
                                //_rawDataDetail.Add((Convert.ToInt32(_reader["QCORANK_A"]) - Convert.ToInt32(_reader["QCORANK_B"])).ToString());
                                //_rawDataDetail.Add(_reader["QCORANK"].ToString());
                                //                        _rawDataDetail.Add((Convert.ToInt32(_reader["QCORANK"]) - Convert.ToInt32(_reader["QCORANK_A"])).ToString());
                                //Master Raw Data:
                                _rawData.Add(new string[] {
                                    _reader["PRDPKG"].ToString()
                                , _reader["QCORANK_B"].ToString()
                                , _reader["QCORANK_A"].ToString()
                                , (Convert.ToInt32(_reader["QCORANK_A"]) - Convert.ToInt32(_reader["QCORANK_B"])).ToString()
                                , _reader["QCORANK"].ToString()
                                ,(Convert.ToInt32(_reader["QCORANK"]) - Convert.ToInt32(_reader["QCORANK_A"])).ToString()
                                });
                            }
                        }
                        _reader.Close();
                    }
                }
                oracleCnn.Close();
            }
            return Json(new { labels = _labels, series = _series, data1 = _data1, data2 = _data2, data3 = _data3, rawData = _rawData }, JsonRequestBehavior.AllowGet);
        }
        private string MaxQCOWeekNo(int pYear)
        {
            string Result = "";
            var sql = $"Select MAX(QCOWeekNo) From T_QC_QUEUE Where QCOYear = {pYear} ";
            using (OracleConnection oracleCnn = new OracleConnection(OPS_Utils.ConstantGeneric.ConnectionStrMes))
            {
                oracleCnn.Open();
                using (OracleCommand cmd = new OracleCommand())
                {
                    cmd.Connection = oracleCnn;
                    cmd.CommandText = sql;
                    var _reader = cmd.ExecuteReader();
                    if (_reader.HasRows)
                    {
                        while (_reader.Read())
                        {
                            Result = _reader[0] != null ? _reader[0].ToString() : "";
                        }
                        _reader.Close();
                    }
                }
                oracleCnn.Close();
            }
            return Result;
        }
        private int QCOWeekToInt(string pWeekNo)
        {
            if (String.IsNullOrEmpty(pWeekNo))
                return Int32.MinValue;
            else
            {
                return pWeekNo.Contains("W")
                    ? (Convert.ToInt32(pWeekNo.Substring(1, 2)))
                    : Int32.MinValue;
            }
        }
        #endregion

        #region EXcel Export
        [SysActionFilter(SystemID = "MES", MenuID = "QCO", Action = "EXPORT")]
        //[SysActionFilter(RoleID = "5000;5100;5110;5111;5113;5114")]
        public ActionResult QCPMToExcel()
        {
            //2020-02-04 Tai Le(Thomas):
            string Type = Url.RequestContext.HttpContext.Request["Type"] != null ? Url.RequestContext.HttpContext.Request["Type"] : "";
            string Factory = Url.RequestContext.HttpContext.Request["Factory"] != null ? Url.RequestContext.HttpContext.Request["Factory"] : "";
            string LineNo = Url.RequestContext.HttpContext.Request["LINENO"] != null ? Url.RequestContext.HttpContext.Request["LINENO"] : "";
            string AONo = Url.RequestContext.HttpContext.Request["AONO"] != null ? Url.RequestContext.HttpContext.Request["AONO"] : "";
            string StyleCode = Url.RequestContext.HttpContext.Request["STYLECODE"] != null ? Url.RequestContext.HttpContext.Request["STYLECODE"] : "";
            string StyleSize = Url.RequestContext.HttpContext.Request["STYLESIZE"] != null ? Url.RequestContext.HttpContext.Request["STYLESIZE"] : "";
            string StyleColorSerial = Url.RequestContext.HttpContext.Request["STYLECOLORSERIAL"] != null ? Url.RequestContext.HttpContext.Request["STYLECOLORSERIAL"] : "";
            string RevNo = Url.RequestContext.HttpContext.Request["REVNO"] != null ? Url.RequestContext.HttpContext.Request["REVNO"] : "";
            string PP = Url.RequestContext.HttpContext.Request["PRDPKG"] != null ? Url.RequestContext.HttpContext.Request["PRDPKG"] : "";
            string QCOFACTORY = Url.RequestContext.HttpContext.Request["QCOFACTORY"] != null ? Url.RequestContext.HttpContext.Request["QCOFACTORY"] : "";
            string QCOYEAR = Url.RequestContext.HttpContext.Request["QCOYEAR"] != null ? Url.RequestContext.HttpContext.Request["QCOYEAR"] : "";
            string QCOWEEK = Url.RequestContext.HttpContext.Request["QCOWEEKNO"] != null ? Url.RequestContext.HttpContext.Request["QCOWEEKNO"] : "";
            string strSQL = "SELECT ROW_NUMBER() OVER(ORDER BY T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL ) AS SEQNO ,  " +
                         " VIEW_ERP_PSRSNP_PLAN.FACTORY ,  " +
                         " VIEW_ERP_PSRSNP_PLAN.LINENO ,  " +
                         " VIEW_ERP_PSRSNP_PLAN.AONO ,  " +
                         " VIEW_ERP_PSRSNP_PLAN.STYLECODE , " +
                         " T_00_STMT.STYLENAME , T_00_STMT.BUYERSTYLENAME , " +
                         " VIEW_ERP_PSRSNP_PLAN.STYLESIZE ,  " +
                         " VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL , " +
                         " T_00_SCMT.STYLECOLORWAYS, " +
                         " VIEW_ERP_PSRSNP_PLAN.REVNO ,  " +
                         " VIEW_ERP_PSRSNP_PLAN.PRDPKG ,  " +
                         " T_SD_BOMT.ITEMCODE , " +
                         " T_00_ICMT.ITEMNAME ,  " +
                         " T_SD_BOMT.ITEMCOLORSERIAL || ' - ' ||  T_00_ICCM.ITEMCOLORWAYS as ITEMCOLORSERIAL , " +
                         " VIEW_ERP_PSRSNP_PLAN.PLANQTY * T_SD_BOMT.UNITCONSUMPTION as REQUESTQTY , " +
                         " NVL(T_QC_QCPM.QUANTITY_A, 0) QUANTITY_A , " +
                         " NVL(T_QC_QCPM.QUANTITY_B, 0) QUANTITY_B , " +
                         " NVL(T_QC_QCPM.QUANTITY_C, 0) QUANTITY_C , " +
                         " NVL(T_QC_QCPM.QUANTITY_D, 0) QUANTITY_D , " +
                         " NVL(T_QC_QCPM.PLANQUANTITY, 0)  PLANQUANTITY ," +
                         " T_QC_QCPM.PDNO " +
                         " FROM PKERP.VIEW_ERP_PSRSNP_PLAN " +
                         " INNER JOIN PKERP.T_SD_BOMT ON " +
                         "   VIEW_ERP_PSRSNP_PLAN.STYLECODE = T_SD_BOMT.STYLECODE " +
                         "   AND VIEW_ERP_PSRSNP_PLAN.STYLESIZE = T_SD_BOMT.STYLESIZE " +
                         "   AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = T_SD_BOMT.STYLECOLORSERIAL " +
                         "   AND VIEW_ERP_PSRSNP_PLAN.REVNO = T_SD_BOMT.REVNO " +
                         " INNER JOIN PKERP.T_00_ICCM  ON " +
                         "        T_SD_BOMT.ITEMCODE = T_00_ICCM.ITEMCODE " +
                         "        AND T_SD_BOMT.ITEMCOLORSERIAL = T_00_ICCM.ITEMCOLORSERIAL " +
                         " INNER JOIN PKERP.T_00_SCMT  ON " +
                         "        VIEW_ERP_PSRSNP_PLAN.STYLECODE = T_00_SCMT.STYLECODE " +
                         "        AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = T_00_SCMT.STYLECOLORSERIAL " +
                         " INNER JOIN PKERP.T_00_ICMT  ON " +
                         "        T_SD_BOMT.ITEMCODE = T_00_ICMT.ITEMCODE " +
                         " INNER JOIN PKERP.T_00_STMT  ON " +
                         "        VIEW_ERP_PSRSNP_PLAN.STYLECODE = T_00_STMT.STYLECODE " +
                         " LEFT JOIN " +
                         " ( SELECT T_QC_QCPM.RANGKING , T_QC_QCPM.FACTORY, T_QC_QCPM.LINENO, T_QC_QCPM.AONO, " +
                         "           T_QC_QCPM.STYLECODE,  T_QC_QCPM.STYLESIZE, " +
                         "           T_QC_QCPM.STYLECOLORSERIAL , T_QC_QCPM.REVNO, T_QC_QCPM.PRDPKG, " +
                         "           T_QC_QCPM.ITEMCODE, " +
                         "           T_QC_QCPM.ITEMCOLORSERIAL , T_QC_QCPM.REQUESTQTY as REQUESTQTY_A ,  " +
                         "           SUM( nvl(T_QC_QCPM.QUANTITY_A,0 ) ) QUANTITY_A ,   " +
                         "           SUM( nvl(T_QC_QCPM.QUANTITY_B,0 ) ) QUANTITY_B ,   " +
                         "           SUM( nvl(T_QC_QCPM.QUANTITY_C,0 ) ) QUANTITY_C ,   " +
                         "           SUM( nvl(T_QC_QCPM.QUANTITY_D,0 ) ) QUANTITY_D ,   " +
                         "           SUM( nvl(T_QC_QCPM.PLANQUANTITY,0 ) ) PLANQUANTITY ," +
                         "   CASE " +
                         "      WHEN '" + Type + "' = 'ByItem' Then '' " +
                         "      WHEN '" + Type + "' = 'ByPOItem' Then T_QC_QCPM.PDNO " +
                         "   END as PDNO " +
                         "   FROM PKMES.T_QC_QCPM " +
                         "   WHERE T_QC_QCPM.FACTORY = '" + QCOFACTORY + "' " +
                         "       AND T_QC_QCPM.LINENO = '" + LineNo + "' " +
                         "       AND T_QC_QCPM.AONO = '" + AONo + "' " +
                         "       AND T_QC_QCPM.STYLECODE = '" + StyleCode + "' " +
                         "       AND T_QC_QCPM.STYLESIZE = '" + StyleSize + "' " +
                         "       AND T_QC_QCPM.STYLECOLORSERIAL = '" + StyleColorSerial + "' " +
                         "       AND T_QC_QCPM.REVNO = '" + RevNo + "' " +
                         "       AND T_QC_QCPM.PRDPKG = '" + PP + "' " +
                         "       AND T_QC_QCPM.QCOWEEKNO = '" + QCOWEEK + "' " +
                         "       AND T_QC_QCPM.QCOYEAR = '" + QCOYEAR + "' " +
                         "   GROUP BY T_QC_QCPM.RANGKING , T_QC_QCPM.FACTORY, T_QC_QCPM.LINENO, T_QC_QCPM.AONO, T_QC_QCPM.STYLECODE, T_QC_QCPM.STYLESIZE, T_QC_QCPM.STYLECOLORSERIAL, T_QC_QCPM.REVNO, T_QC_QCPM.PRDPKG, " +
                         "   T_QC_QCPM.ITEMCODE, T_QC_QCPM.ITEMCOLORSERIAL , T_QC_QCPM.REQUESTQTY ," +
                         "   CASE " +
                         "      WHEN '" + Type + "' = 'ByItem' Then '' " +
                         "      WHEN '" + Type + "' = 'ByPOItem' Then T_QC_QCPM.PDNO " +
                         "   END  " +
                         " ) T_QC_QCPM ON " +
                         "       VIEW_ERP_PSRSNP_PLAN.FACTORY = T_QC_QCPM.FACTORY " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.LINENO = T_QC_QCPM.LINENO " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.AONO = T_QC_QCPM.AONO " +
                         "       AND T_SD_BOMT.STYLECODE = T_QC_QCPM.STYLECODE " +
                         "       AND T_SD_BOMT.STYLESIZE = T_QC_QCPM.STYLESIZE " +
                         "       AND T_SD_BOMT.STYLECOLORSERIAL = T_QC_QCPM.STYLECOLORSERIAL " +
                         "       AND T_SD_BOMT.REVNO = T_QC_QCPM.REVNO " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.PRDPKG = T_QC_QCPM.PRDPKG " +
                         "       AND T_SD_BOMT.ITEMCODE = T_QC_QCPM.ITEMCODE " +
                         "       AND T_SD_BOMT.ITEMCOLORSERIAL = T_QC_QCPM.ITEMCOLORSERIAL " +
                         "   WHERE VIEW_ERP_PSRSNP_PLAN.FACTORY = '" + Factory + "' " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.LINENO = '" + LineNo + "' " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.AONO = '" + AONo + "' " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.STYLECODE = '" + StyleCode + "' " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.STYLESIZE = '" + StyleSize + "' " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = '" + StyleColorSerial + "' " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.REVNO = '" + RevNo + "' " +
                         "       AND VIEW_ERP_PSRSNP_PLAN.PRDPKG = '" + PP + "' " +
                         "       AND (T_SD_BOMT.ITEMCODE NOT LIKE 'PKG%' AND T_SD_BOMT.ITEMCODE NOT LIKE 'TRE%' ) " +
                         " ORDER BY T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL ";
            var _Result = OPS_DAL.DAL.OracleDbManager.Query(strSQL, null, OPS_Utils.ConstantGeneric.ConnectionStrMes);
            if (_Result != null)
            {
                if (_Result.Rows.Count > 0)
                {
                    int intI = 1, intJ = 0;
                    var excelPackage = new ExcelPackage();
                    excelPackage.Workbook.Worksheets.Add("QCO");
                    ExcelWorksheet excelWorkSheet = excelPackage.Workbook.Worksheets[1];
                    /*Excel Column and Row is  ONE_BASED  , NOT ZERO_BASED  */
                    //Header
                    for (intJ = 1; intJ <= _Result.Columns.Count; intJ++)
                        excelWorkSheet.Cells[1, intJ].Value = _Result.Columns[intJ - 1].ColumnName;
                    //Row_data 
                    for (intI = 0; intI < _Result.Rows.Count; intI++)
                        for (intJ = 0; intJ < _Result.Columns.Count; intJ++)
                            excelWorkSheet.Cells[intI + 2, intJ + 1].Value = _Result.Rows[intI][intJ] != null ? _Result.Rows[intI][intJ] : DBNull.Value;
                    System.IO.MemoryStream stream = new System.IO.MemoryStream();
                    excelPackage.SaveAs(stream);
                    excelPackage.Dispose();
                    //var fileName = "QCODetail-" + UserInf.UserName;
                    var fileName = $"QCODetail-{_Result.Rows[0]["FACTORY"].ToString()}";
                    byte[] xlsInBytes = stream.ToArray();
                    if (xlsInBytes.Length > 0)
                        return File(xlsInBytes, "application/octet-stream", fileName + ".xlsx");
                    else
                        return new EmptyResult();
                }
                else
                    return new EmptyResult();
            }
            else
                return new EmptyResult();
            //[SysActionFilter(RoleID = "5000;5100;5110;5111;5113;5114")]
            //[SysActionFilter(RoleID = "5000;5100;5110;5111;5113;5114")]
        }
        //[SysActionFilter(RoleID = "5000;5100;5110;5111;5113;5114")]
        [SysActionFilter(SystemID = "MES", MenuID = "QCO", Action = "EXPORT")]
        public ActionResult QCOCapaToExcel(GridSettings gridRequest)
        {
            try
            {
                string strSQL = "",
                    strSQLWhere = "";
                strSQL =
                    " SELECT ROW_NUMBER() OVER(ORDER BY NVL(T_QC_QUEUE.CHANGEQCORANK, T_QC_QUEUE.QCORANK) ) AS RANKING , " +
                    " T_00_STMT.STYLENAME , T_00_STMT.BUYERSTYLECODE , T_00_STMT.BUYERSTYLENAME , T_00_SCMT.STYLECOLORWAYS , " +
                    " T_QC_QUEUE.QCOFACTORY, T_QC_QUEUE.QCOYEAR, T_QC_QUEUE.QCOWEEKNO, T_QC_QUEUE.QCORANK, NVL(T_QC_QUEUE.CHANGEQCORANK, T_QC_QUEUE.QCORANK) CustomRanking , T_QC_QUEUE.FACTORY, T_QC_QUEUE.LINENO, " +
                    " T_QC_QUEUE.OPTIME , T_QC_QUEUE.MANCOUNT , T_QC_QUEUE.BEGINCAPA as AvailableCapa , T_QC_QUEUE.USAGECAPA as ReqManHours , T_QC_QUEUE.BALANCECAPA , T_QC_QUEUE.WEEKCAPA as AssignWeek , T_QC_QUEUE.EFFICIENCY,  " +
                    " T_QC_QUEUE.AONO, VIEW_ERP_PSRSNP_PLAN.ADTYPENAME , T_QC_QUEUE.BUYER, T_QC_QUEUE.STYLECODE, T_QC_QUEUE.STYLESIZE, T_QC_QUEUE.STYLECOLORSERIAL, T_QC_QUEUE.REVNO, " +
                    " T_QC_QUEUE.PRDPKG, T_QC_QUEUE.NORMALIZEDPERCENT, " +
                    " T_QC_QUEUE.PLANQTY, T_QC_QUEUE.DELIVERYDATE " +
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
                    "       AND T_QC_QUEUE.PRDPKG = VIEW_ERP_PSRSNP_PLAN.PRDPKG ";
                strSQLWhere = " T_QC_QUEUE.USAGECAPA IS NOT NULL  ";
                var _Result = GridData.GetGridDataTable(ConstantGeneric.ConnectionStr, strSQL, strSQLWhere, gridRequest);
                if (_Result != null)
                {
                    if (_Result.Rows.Count > 0)
                    {
                        int intI = 1, intJ = 0;
                        var excelPackage = new ExcelPackage();
                        excelPackage.Workbook.Worksheets.Add("QCO");
                        ExcelWorksheet excelWorkSheet = excelPackage.Workbook.Worksheets[1];
                        /*Excel Column and Row is  ONE_BASED  , NOT ZERO_BASED  */
                        //Header
                        for (intJ = 1; intJ <= _Result.Columns.Count; intJ++)
                            excelWorkSheet.Cells[1, intJ].Value = _Result.Columns[intJ - 1].ColumnName;
                        //Row_data 
                        for (intI = 0; intI < _Result.Rows.Count; intI++)
                            for (intJ = 0; intJ < _Result.Columns.Count; intJ++)
                                excelWorkSheet.Cells[intI + 2, intJ + 1].Value = _Result.Rows[intI][intJ] != null ? _Result.Rows[intI][intJ] : DBNull.Value;
                        System.IO.MemoryStream stream = new System.IO.MemoryStream();
                        excelPackage.SaveAs(stream);
                        excelPackage.Dispose();
                        var fileName = "QCOCapa-" + UserInf.UserName + "-" + DateTime.Today.ToString("yyyyMMdd");
                        byte[] xlsInBytes = stream.ToArray();
                        if (xlsInBytes.Length > 0)
                            return File(xlsInBytes, "application/octet-stream", fileName + ".xlsx");
                        else
                            return new EmptyResult();
                    }
                    else
                        return new EmptyResult();
                }
                else
                    return new EmptyResult();
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return new EmptyResult();
            }
        }
        [SysActionFilter(SystemID = "MES", MenuID = "QCO", Action = "EXPORT")]
        public ActionResult QCOCapaBOMToExcel(GridSettings gridRequest)
        {
            try
            {
                JsonResult _result = new JsonResult();
                string FormatOutput = Request.QueryString["FormatOutput"] != null ?
                    Request.QueryString["FormatOutput"].ToString() :
                    "Excel";
                string strSQL = "",
                    strSQLWhere = "";
                strSQL =
                    " SELECT " +
                    " T_QC_QUEUE.QCOFACTORY, T_QC_QUEUE.QCOYEAR, T_QC_QUEUE.QCOWEEKNO, T_QC_QUEUE.QCORANK, NVL(T_QC_QUEUE.CHANGEQCORANK, T_QC_QUEUE.QCORANK) as CUSTOMRANKING , " +
                    " T_QC_QUEUE.FACTORY, T_QC_QUEUE.LINENO, " +
                    " T_QC_QUEUE.STYLECODE || '/' ||  T_00_STMT.BUYERSTYLECODE ||'/' || T_00_STMT.STYLENAME AS STYLECODE, " +
                    " T_QC_QUEUE.STYLESIZE, " +
                    " T_QC_QUEUE.STYLECOLORSERIAL || ' - ' ||T_00_SCMT.STYLECOLORWAYS AS STYLECOLORSERIAL, " +
                    " T_QC_QUEUE.REVNO, T_00_STMT.BUYERSTYLENAME , " +
                    " T_QC_QUEUE.OPTIME , " +
                    " T_QC_QUEUE.MANCOUNT , " +
                    " T_QC_QUEUE.BEGINCAPA as AvailableCapa , " +
                    " T_QC_QUEUE.USAGECAPA as ReqManHours , " +
                    " T_QC_QUEUE.BALANCECAPA , " +
                    " T_QC_QUEUE.WEEKCAPA as AssignWeek , " +
                    " T_QC_QUEUE.EFFICIENCY,  " +
                    " T_QC_QUEUE.AONO, VIEW_ERP_PSRSNP_PLAN.ADTYPENAME , T_QC_QUEUE.BUYER, " +
                    " T_QC_QUEUE.PRDPKG, T_QC_QUEUE.NORMALIZEDPERCENT, " +
                    " T_QC_QUEUE.PLANQTY, T_QC_QUEUE.DELIVERYDATE , " +
                    " T_QC_QCPM.ITEMCODE , T_QC_QCPM.ITEMCOLORSERIAL , " +
                    " T_QC_QCPM.ITEMCOLORSERIAL || ' - '  || T_00_ICCM.ItemColorWays as  ItemColorWays , " +
                    " T_QC_QCPM.REQUESTQTY , " +
                    " SUM(T_QC_QCPM.QUANTITY_A) as WMS_QTY , " +
                    " SUM(T_QC_QCPM.QUANTITY_B) AS RECEIVING_WITHIN_5DAYS , " +
                    " SUM(T_QC_QCPM.QUANTITY_C) AS RECEIVING_WITHIN_10DAYS , " +
                    " SUM(T_QC_QCPM.QUANTITY_D) AS RECEIVING_AFTER_DUEDATE , " +
                    " CASE " +
                    "   WHEN T_QC_QCPM.REQUESTQTY - (SUM(T_QC_QCPM.QUANTITY_A) + SUM(T_QC_QCPM.QUANTITY_B) + SUM(T_QC_QCPM.QUANTITY_C) + SUM(T_QC_QCPM.QUANTITY_D) ) > 0 " +
                    "       THEN T_QC_QCPM.REQUESTQTY - (SUM(T_QC_QCPM.QUANTITY_A) + SUM(T_QC_QCPM.QUANTITY_B) + SUM(T_QC_QCPM.QUANTITY_C) + SUM(T_QC_QCPM.QUANTITY_D) ) " +
                    "   ELSE 0  " +
                    " END as BALANCEQTY " +
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
                    "   LEFT JOIN PKMES.T_QC_QCPM ON " +
                    "       T_QC_QUEUE.QCOFACTORY = T_QC_QCPM.QCOFACTORY " +
                    "       AND T_QC_QUEUE.QCOYEAR = T_QC_QCPM.QCOYEAR " +
                    "       AND T_QC_QUEUE.QCOWEEKNO = T_QC_QCPM.QCOWEEKNO " +
                    "       AND T_QC_QUEUE.AONO = T_QC_QCPM.AONO " +
                    "       AND T_QC_QUEUE.STYLECODE = T_QC_QCPM.STYLECODE " +
                    "       AND T_QC_QUEUE.STYLESIZE = T_QC_QCPM.STYLESIZE " +
                    "       AND T_QC_QUEUE.STYLECOLORSERIAL = T_QC_QCPM.STYLECOLORSERIAL " +
                    "       AND T_QC_QUEUE.REVNO = T_QC_QCPM.REVNO " +
                    "       AND T_QC_QUEUE.PRDPKG = T_QC_QCPM.PRDPKG " +
                    "       AND T_QC_QUEUE.FACTORY = T_QC_QCPM.FACTORY " +
                    "       AND T_QC_QUEUE.LINENO = T_QC_QCPM.LINENO " +
                    "   LEFT JOIN PKERP.T_00_ICCM ON " +
                    "       T_QC_QCPM.ITEMCODE = T_00_ICCM.ITEMCODE " +
                    "       AND T_QC_QCPM.ITEMCOLORSERIAL = T_00_ICCM.ITEMCOLORSERIAL " +
                    "";
                strSQLWhere =
                    $" T_QC_QUEUE.USAGECAPA IS NOT NULL " +
                    $" AND T_QC_QUEUE.QCOFACTORY = '{Request["_searchFieldQCOFACTORY"].ToString()}' " +
                    $" AND T_QC_QUEUE.QCOYEAR = {Request["_searchFieldQCOYEAR"].ToString()} " +
                    $" AND T_QC_QUEUE.QCOWEEKNO = '{Request["_searchFieldQCOWEEKNO"].ToString()}' ";
                if (Request["_searchFieldStrBeginASSIGNWEEK"] != null)
                    if (Request["_searchFieldStrBeginASSIGNWEEK"].ToString().Length > 0)
                    {
                        strSQLWhere = strSQLWhere + $" AND  T_QC_QUEUE.WEEKCAPA >= '{Request["_searchFieldStrBeginASSIGNWEEK"].ToString()}' ";
                        //var _temp = Request["_searchFieldStrBeginASSIGNWEEK"].ToString().Split('/');
                        //var _week = _temp[1].Substring(2, 1) == "0" ? _temp[1].Substring(_temp[1].Length - 1, 1) : _temp[1].Substring(_temp[1].Length - 2, 2); 
                        //strSQLWhere = strSQLWhere + $" AND  T_QC_QUEUE.WEEKCAPA >= '{_temp[0].Trim()} / W{_week.Trim()}' ";
                    }
                if (Request["_searchFieldStrEndASSIGNWEEK"] != null)
                    if (Request["_searchFieldStrEndASSIGNWEEK"].ToString().Length > 0)
                    {
                        strSQLWhere = strSQLWhere + $" AND  T_QC_QUEUE.WEEKCAPA <= '{Request["_searchFieldStrEndASSIGNWEEK"].ToString()}' ";
                        //var _temp = Request["_searchFieldStrEndASSIGNWEEK"].ToString().Split('/');
                        //var _week = _temp[1].Substring(2, 1) == "0" ? _temp[1].Substring(_temp[1].Length - 1, 1) : _temp[1].Substring(_temp[1].Length - 2, 2); 
                        //strSQLWhere = strSQLWhere + $" AND  T_QC_QUEUE.WEEKCAPA <= '{_temp[0].Trim()} / W{_week.Trim()}' ";
                    }
                string strSQLGroupBy =
                    " T_00_STMT.STYLENAME , T_00_STMT.BUYERSTYLECODE , T_00_STMT.BUYERSTYLENAME , T_00_SCMT.STYLECOLORWAYS , " +
                    " T_QC_QUEUE.QCOFACTORY, T_QC_QUEUE.QCOYEAR, T_QC_QUEUE.QCOWEEKNO, T_QC_QUEUE.QCORANK, NVL(T_QC_QUEUE.CHANGEQCORANK, T_QC_QUEUE.QCORANK) , T_QC_QUEUE.FACTORY, T_QC_QUEUE.LINENO, " +
                    " T_QC_QUEUE.OPTIME , T_QC_QUEUE.MANCOUNT , T_QC_QUEUE.BEGINCAPA , T_QC_QUEUE.USAGECAPA , T_QC_QUEUE.BALANCECAPA , T_QC_QUEUE.WEEKCAPA , T_QC_QUEUE.EFFICIENCY,  " +
                    " T_QC_QUEUE.AONO, VIEW_ERP_PSRSNP_PLAN.ADTYPENAME , T_QC_QUEUE.BUYER, T_QC_QUEUE.STYLECODE, T_QC_QUEUE.STYLESIZE, T_QC_QUEUE.STYLECOLORSERIAL, T_QC_QUEUE.REVNO, " +
                    " T_QC_QUEUE.PRDPKG, T_QC_QUEUE.NORMALIZEDPERCENT, " +
                    " T_QC_QUEUE.PLANQTY, T_QC_QUEUE.DELIVERYDATE , " +
                    " T_QC_QCPM.ITEMCODE , T_QC_QCPM.ITEMCOLORSERIAL , T_QC_QCPM.ITEMCOLORSERIAL || ' - '  || T_00_ICCM.ItemColorWays , T_QC_QCPM.REQUESTQTY ";
                strSQL = $@"
SELECT  ROW_NUMBER() OVER(ORDER BY CustomRanking , ItemCode , ITEMCOLORSERIAL ) AS RANKING ,
MainT.* 
FROM (
    {strSQL}
    Where {strSQLWhere}
    Group By {strSQLGroupBy}
) MainT
";
                //2019-12-21 Tai Le (Thomas):prevent the duplicated Where since already set inside "strSQLWhere"
                gridRequest.extraWhere = "";
                strSQLWhere = "";
                var _Result = GridData.GetGridDataTable(ConstantGeneric.ConnectionStr, strSQL, strSQLWhere, gridRequest);
                if (_Result != null)
                {
                    if (_Result.Rows.Count > 0)
                    {
                        //Remove some column of _Result 
                        if (FormatOutput.ToLower() == "pdf")
                        {
                            List<string> RemovedColumns = new List<string>();
                            List<string> AllColumns = new List<string>();
                            List<string> tableColumns = new List<string>();
                            if (Request.Form != null)
                            {
                                var FormKeys = Request.Form.AllKeys;
                                foreach (var item in FormKeys)
                                    if (item.Contains("col"))
                                    {
                                        AllColumns.Add(item.Replace("col", ""));
                                        if (Request.Form[item].ToString() == "false")
                                            RemovedColumns.Add(item.Replace("col", ""));
                                    }
                            }
                            //2020-02-03 Tai Le(Thomas): Remove the deselect columns
                            //Step 1: only keep the Columns on the list
                            foreach (DataColumn dc in _Result.Columns)
                            {
                                tableColumns.Add(dc.ColumnName);
                            }
                            var lst = (from lst1 in tableColumns
                                       where !AllColumns.Any(x => x.ToUpper() == lst1.ToUpper())
                                       select lst1).ToList();
                            var _temp = lst.ToList();
                            if (_temp.Count > 0)
                                foreach (string item in _temp)
                                {
                                    if (_Result.Columns[item] != null)
                                        _Result.Columns.Remove(item.ToUpper());
                                }
                            //Step 2: remove the deselected Columns on the list
                            if (RemovedColumns.Count > 0)
                                foreach (string item in RemovedColumns)
                                {
                                    if (_Result.Columns[item] != null)
                                        _Result.Columns.Remove(item.ToUpper());
                                }
                            //END   2020-02-03 Tai Le(Thomas): Remove the deselect columns
                        }
                        int intI = 1, intJ = 0;
                        var excelPackage = new ExcelPackage();
                        excelPackage.Workbook.Worksheets.Add("QCO");
                        ExcelWorksheet excelWorkSheet = excelPackage.Workbook.Worksheets[1];
                        /*Excel Column and Row is  ONE_BASED  , NOT ZERO_BASED  */
                        //Header
                        for (intJ = 1; intJ <= _Result.Columns.Count; intJ++)
                            excelWorkSheet.Cells[1, intJ].Value = _Result.Columns[intJ - 1].ColumnName;
                        //Row_data 
                        for (intI = 0; intI < _Result.Rows.Count; intI++)
                            for (intJ = 0; intJ < _Result.Columns.Count; intJ++)
                                excelWorkSheet.Cells[intI + 2, intJ + 1].Value = _Result.Rows[intI][intJ] != null ? _Result.Rows[intI][intJ] : DBNull.Value;
                        //var fileName = "QCOCapaBOM-" + UserInf.UserName + "-" + DateTime.Today.ToString("yyyyMMdd");
                        var fileName = $"QCOBOMCapa" +
                            $"-{_Result.Rows[0]["QCOFACTORY"].ToString()}" +
                            $"-{_Result.Rows[0]["QCOYEAR"].ToString()}" +
                            $"-{_Result.Rows[0]["QCOWEEKNO"].ToString()}" +
                            $"-{UserInf.UserName}";
                        switch (FormatOutput.ToLower())
                        {
                            case "excel":
                                {
                                    /*Excel Part */
                                    System.IO.MemoryStream stream = new System.IO.MemoryStream();
                                    excelPackage.SaveAs(stream);
                                    excelPackage.Dispose();
                                    byte[] xlsInBytes = stream.ToArray();
                                    //if (xlsInBytes.Length > 0)
                                    //    return File(xlsInBytes, "application/octet-stream", fileName + ".xlsx");
                                    //else
                                    //    return new EmptyResult(); 
                                    //return Json(
                                    //    JsonConvert.SerializeObject(new 
                                    //    { 
                                    //        fileName = fileName + (FormatOutput == "PDF" ? ".pdf" : ".xlsx"), 
                                    //        fileByte = stream.ToArray()
                                    //    }), 
                                    //    JsonRequestBehavior.AllowGet);
                                    _result = Json(new
                                    {
                                        fileName = fileName + (FormatOutput == "PDF" ? ".pdf" : ".xlsx"),
                                        fileExtension = FormatOutput == "PDF" ? "pdf" : "xlsx",
                                        fileByte = Convert.ToBase64String(xlsInBytes)
                                    }, JsonRequestBehavior.AllowGet);
                                }
                                break;
                            case "pdf":
                                {
                                    /*PDF Part */
                                    var _filePath = System.IO.Path.Combine(Server.MapPath("~"), $@"{fileName}.xlsx");
                                    if (System.IO.File.Exists(_filePath))
                                        System.IO.File.Delete(_filePath);
                                    excelPackage.SaveAs(new System.IO.FileInfo(_filePath)); //Save File to Hard-disk  
                                    if (System.IO.File.Exists(System.IO.Path.Combine(Server.MapPath("~"), $@"{fileName}.pdf")))
                                        System.IO.File.Delete(System.IO.Path.Combine(Server.MapPath("~"), $@"{fileName}.pdf"));
                                    EPPlusExcelToPDF ePPlusExcelToPDF = new EPPlusExcelToPDF();
                                    //Default PageOrientation ; PdfPageSize
                                    var pageOrientation = PdfRpt.Core.Contracts.PageOrientation.Landscape;
                                    var pageSize = PdfRpt.Core.Contracts.PdfPageSize.A4;
                                    if (_Result.Columns.Count >= 30) { pageSize = PdfRpt.Core.Contracts.PdfPageSize.A0; }
                                    else if (_Result.Columns.Count >= 25) { pageSize = PdfRpt.Core.Contracts.PdfPageSize.A1; }
                                    else if (_Result.Columns.Count >= 20) { pageSize = PdfRpt.Core.Contracts.PdfPageSize.A2; }
                                    else if (_Result.Columns.Count >= 15) { pageSize = PdfRpt.Core.Contracts.PdfPageSize.A3; }
                                    else if (_Result.Columns.Count >= 10) { pageSize = PdfRpt.Core.Contracts.PdfPageSize.A4; }
                                    ePPlusExcelToPDF.CreateExcelToPdfReport(_filePath, "QCO", pageOrientation, pageSize);
                                    if (System.IO.File.Exists(System.IO.Path.Combine(Server.MapPath("~"), $@"{fileName}.pdf")))
                                    {
                                        //System.IO.File.Delete(System.IO.Path.Combine(Server.MapPath("~"), $@"{fileName}.xlsx"));
                                        //var stream = System.IO.File.OpenRead(System.IO.Path.Combine(Server.MapPath("~"), $@"{fileName}.pdf"));
                                        //System.IO.MemoryStream data = new System.IO.MemoryStream();
                                        //stream.CopyTo(data);
                                        //byte[] xlsInBytes = data.ToArray();
                                        _result = Json(new
                                        {
                                            fileName = fileName + (FormatOutput == "PDF" ? ".pdf" : ".xlsx"),
                                            fileExtension = FormatOutput == "PDF" ? "pdf" : "xlsx",
                                            fileByte = ""
                                        }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                        return new EmptyResult();
                                }
                                break;
                        }
                        return _result;
                    }
                    else
                        return new EmptyResult();
                }
                else
                    return new EmptyResult();
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return new EmptyResult();
            }
        }
        #endregion

        #region  FEATURES
        [HttpPost]
        //[SysActionFilter(RoleID = "5000;5100;5110;5111;5113;5114")]
        [SysActionFilter(SystemID = "MES", MenuID = "QCO", Action = "UPDATE")]
        public ActionResult SaveQCPSRanking(string data, string Reason)
        {
            string strMsg = "";
            bool blResult = false;
            //Check Role Permission (SRMT)
            if (Role == null || Role.IsUpdate != "1")
            {
                strMsg = "No Authority.";
                goto HE_END;
            }
            try
            {
                var Qcops = new JavaScriptSerializer().Deserialize<List<Qcops>>(data);
                CultureInfo cul = CultureInfo.CurrentCulture;
                int weekNum = cul.Calendar.GetWeekOfYear(DateTime.Today, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                if (Qcops[0].QCOWEEKNO != "W" + PCMGeneralFunctions.GetRight("00" + weekNum, 2))
                {
                    return Json(new
                    {
                        retResult = false,
                        retMsg = "Only Week [" + weekNum + "] is allowed to change. Your week data is [" + Qcops[0].QCOWEEKNO + "]"
                    }, JsonRequestBehavior.AllowGet);
                }
                blResult = SaveQCPSRankingMain(Qcops, Reason, out strMsg);
            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
            }
        HE_END:
            return Json(new { retResult = blResult, retMsg = strMsg }, JsonRequestBehavior.AllowGet);
        }
        public bool SaveQCPSRankingMain(List<Qcops> Qcops, string Reason, out string pMsg)
        {
            pMsg = "";
            var Check_ = Qcops.Where(f => f.QCORANK < 0).ToList();
            if (Check_.Count > 0)
            {
                pMsg = "Please DON'T Include Negative QCO_RANK.<BR/>Use \"QCO Display\" to Filter Positive Ranking.";
                return false;
                //goto HE_END;
            }
            if (Qcops.Count > 0)
            {
                List<Qcops> HandleList = new List<Qcops>();
                int intNewRanking = 0;
                var minQCORANKINGNEW = Qcops.Min(x => x.QCORANKINGNEW);
                var maxQCORANKINGNEW = Qcops.Max(x => x.QCORANKINGNEW);
                var minRANKING = Qcops.Min(x => x.RANKING);
                var maxRANKING = Qcops.Max(x => x.RANKING);
                int MAXQCORANKING = T_QC_QUEUE_MAXQCORANK(Qcops[0].QCOFACTORY, Qcops[0].QCOYEAR, Qcops[0].QCOWEEKNO);
                intNewRanking = minRANKING - 1;
                Qcops _temp = null;
                bool blStartFromTop = false;
                for (int intI = 0; intI < Qcops.Count; intI++)
                {
                    intNewRanking += 1;
                    Qcops[intI].intNewRanking = intNewRanking;
                    if (intNewRanking != Qcops[intI].RANKING)
                    {
                        if (intI > 0)
                        {
                            if (_temp == null)
                                _temp = Qcops[intI - 1]; //Get Previous Row
                        }
                        else if (intI == 0)
                            blStartFromTop = true;
                        MAXQCORANKING += 1;
                        Qcops[intI].MAXQCORANKING = MAXQCORANKING;
                        HandleList.Add(Qcops[intI]);
                    }
                }
                bool blIsBasedOnCustomRank = false;
                if (_temp != null)
                    if (!String.IsNullOrEmpty(_temp.CHANGEQCORANK))
                    {
                        blIsBasedOnCustomRank = true;
                        intNewRanking = Convert.ToInt32(_temp.CHANGEQCORANK);
                    }
                if (blStartFromTop)
                    intNewRanking = 0;
                foreach (Qcops item in HandleList)
                {
                    /// Step 2: Update QCORanking = intNewRanking ,
                    ///             ChangeBy , ChangeOn
                    ///             Reason
                    ///             Where QCORANKING = MAXQCORANKING Debug.Print("intNewRanking= " + item.intNewRanking + "; QCORANKINGNEW = " + item.QCORANKINGNEW);
                    if (blStartFromTop)
                    {
                        intNewRanking = intNewRanking + 1;
                        //UpdateQCORanking_2(item, intNewRanking, Reason);
                        UpdateQCORanking_2(item, intNewRanking, !String.IsNullOrEmpty(Reason) ? Reason : item.REASON);
                    }
                    else
                    {
                        if (!blIsBasedOnCustomRank)
                        {
                            var origPP = Qcops.Where(f => f.RANKING == item.intNewRanking).Select(n => n.QCORANK).First();
                            //UpdateQCORanking_2(item, origPP, Reason);
                            UpdateQCORanking_2(item, origPP, !String.IsNullOrEmpty(Reason) ? Reason : item.REASON);
                        }
                        else if (blIsBasedOnCustomRank)
                        {
                            intNewRanking = intNewRanking + 1;
                            //UpdateQCORanking_2(item, intNewRanking, Reason);
                            UpdateQCORanking_2(item, intNewRanking, !String.IsNullOrEmpty(Reason) ? Reason : item.REASON);
                        }
                    }
                }
                /// Step 3: 
                /// Update QCORANKINGNEW Based On Is_NULL(CHANGEQCORANK, QCORANK), 
                /// QCORANKINGNEW starts from 1 to n 
                UpdateQCORanking_3(HandleList[0]);
                /// Step 4: Handle the CAPA
                string msg = "";
                PKQCO.PCMQCOCalculation pCMQCOCalculation = new PCMQCOCalculation(OPS_Utils.ConstantGeneric.ConnectionStr, OPS_Utils.ConstantGeneric.ConnectionStrMes);
                pCMQCOCalculation.mEnviroment = "";
                pCMQCOCalculation.mQCOSource = "QCO";
                pCMQCOCalculation.mFactory = HandleList[0].QCOFACTORY;
                pCMQCOCalculation.CalculateCAPA(pCMQCOCalculation.mFactory, HandleList[0].QCOYEAR, PKQCO.PCMGeneralFunctions.WeekStringToInt(HandleList[0].QCOWEEKNO), true, out msg);
                //blResult = true;
                pMsg = "Custom Ranking Saved!";
                return true;
            }
            else
            {
                pMsg = "No Data to adjust Custom Rank.";
                return false;
            }
        }
        private void UpdateQCORanking_2(Qcops item, int origQCORANKING, string Reason)
        {
            using (OracleConnection OracleConn = new OracleConnection(OPS_Utils.ConstantGeneric.ConnectionStrMes))
            {
                OracleConn.Open();
                //var strSQL =
                //      " UPDATE PKMES.T_QC_QUEUE " +
                //      " SET CHANGEQCORANK = :QCORANKNEW , " +
                //      " CHANGEBY = :CHANGEBY , " +
                //      " CHANGEON = :CHANGEON , " +
                //      " REASON = :REASON " +
                //      " WHERE QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO AND QCORANK= :QCORANK ";
                var strSQL =
                      " UPDATE {TableName} " +
                      " SET CHANGEQCORANK = :QCORANKNEW , " +
                      " CHANGEBY = :CHANGEBY , " +
                      " CHANGEON = :CHANGEON , " +
                      " REASON = :REASON " +
                      " WHERE QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO AND QCORANK= :QCORANK ";
                //** 2019-11-07 Tai Le (Thomas)
                if (item.QCOSOURCE.ToUpper() == "QCO")
                    strSQL = strSQL.Replace("{TableName}", "PKMES.T_QC_QUEUE");
                else if (item.QCOSOURCE.ToUpper() == "QCOSIM")
                    strSQL = strSQL.Replace("{TableName}", "PKMES.T_QC_QUEUESIM");
                OracleCommand oracleCommand = new OracleCommand(strSQL, OracleConn);
                oracleCommand.Parameters.Add("QCORANKNEW", origQCORANKING);
                oracleCommand.Parameters.Add("CHANGEBY", UserInf.UserName);
                oracleCommand.Parameters.Add("CHANGEON", DateTime.Now);
                oracleCommand.Parameters.Add("REASON", Reason);
                oracleCommand.Parameters.Add("QCOFACTORY", item.QCOFACTORY);
                oracleCommand.Parameters.Add("QCOYEAR", item.QCOYEAR);
                oracleCommand.Parameters.Add("QCOWEEKNO", item.QCOWEEKNO);
                oracleCommand.Parameters.Add("QCORANK", item.QCORANK);
                oracleCommand.ExecuteNonQuery();
                OracleConn.Close();
                OracleConn.Dispose();
            }
        }
        private void UpdateQCORanking_3(Qcops item)
        {
            using (OracleConnection OracleConn = new OracleConnection(OPS_Utils.ConstantGeneric.ConnectionStrMes))
            {
                OracleConn.Open();
                //var strSQL =
                //  " SELECT * FROM PKMES.T_QC_QUEUE " +
                //  " WHERE QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO " +
                //  " ORDER BY NVL(CHANGEQCORANK, QCORANK) ";
                var strSQL =
                  " SELECT * FROM {TableName} " +
                  " WHERE QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO " +
                  " ORDER BY NVL(CHANGEQCORANK, QCORANK) ";
                //** 2019-11-07 Tai Le (Thomas)
                if (item.QCOSOURCE.ToUpper() == "QCO")
                    strSQL = strSQL.Replace("{TableName}", "PKMES.T_QC_QUEUE");
                else if (item.QCOSOURCE.ToUpper() == "QCOSIM")
                    strSQL = strSQL.Replace("{TableName}", "PKMES.T_QC_QUEUESIM");
                OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, OracleConn);
                oracleDataAdapter.SelectCommand.Parameters.Add("QCOFACTORY", item.QCOFACTORY);
                oracleDataAdapter.SelectCommand.Parameters.Add("QCOYEAR", item.QCOYEAR);
                oracleDataAdapter.SelectCommand.Parameters.Add("QCOWEEKNO", item.QCOWEEKNO);
                DataTable dt = new DataTable();
                oracleDataAdapter.Fill(dt);
                int intQCORANKINGNEW = 0;
                if (dt != null)
                    foreach (DataRow dr in dt.Rows)
                    {
                        intQCORANKINGNEW += 1;
                        dr["QCORANKINGNEW"] = intQCORANKINGNEW;
                    }
                OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                oracleDataAdapter.Update(dt);
                oracleCommandBuilder.Dispose();
                if (dt != null)
                    dt.Dispose();
                oracleDataAdapter.Dispose();
                OracleConn.Close();
                OracleConn.Dispose();
            }
        }
        [SysActionFilter(SystemID = "MES", MenuID = "QCO", Action = "UPDATE")]
        public ActionResult RunQCO(QCOQueue qcoQueue)
        {
            if (String.IsNullOrEmpty(qcoQueue.FACTORY))
                return Json("Factory is not Specified.", JsonRequestBehavior.AllowGet);
            var UserID = UserInf.UserName;
            PCMQCOCalculation pcmQCOCalculation = new PCMQCOCalculation(OPS_Utils.ConstantGeneric.ConnectionStr, OPS_Utils.ConstantGeneric.ConnectionStrMes);
            pcmQCOCalculation.mEnviroment = "";
            pcmQCOCalculation.mFactory = qcoQueue.FACTORY;
            pcmQCOCalculation.mQCOSource = "QCOSim"; //QCOSim or QCO 
            pcmQCOCalculation.mUserID = UserID;
            if (qcoQueue != null)
            {
                //string QCOResult_2 = "";
                //Task.Run(() => pcmQCOCalculation.QCOCalculation(System.Configuration.ConfigurationManager.ConnectionStrings["PKPCM"].ConnectionString, qcoQueue.FACTORY, UserInf.UserName, Role.OwnerId, String.Empty, false, "", "", "", "", "", "", out QCOResult_2));
                //2020-02-10 Tai Le(Thomas): change the function to new QCO including PDNo
                Task.Run(() => pcmQCOCalculation.QCOCalculationNew(qcoQueue.FACTORY, UserInf.UserName, Role.OwnerId, String.Empty, false, "", "", "", "", "", "", false));
                return Json("QCO in " + qcoQueue.FACTORY + " is under process. It takes a while depending on the number of MTOPS Package under " + qcoQueue.FACTORY, JsonRequestBehavior.AllowGet);
            }
            else
                return Json("No Input Data", JsonRequestBehavior.AllowGet);
        }
        [SysActionFilter(SystemID = "MES", MenuID = "QCO", Action = "UPDATE")]
        public ActionResult RunSimulateQCO(QCOQueue qcoQueue)
        {
            if (String.IsNullOrEmpty(qcoQueue.FACTORY))
                return Json("Factory is not Specified.", JsonRequestBehavior.AllowGet);
            if (UserInf == null)
                return Json("Couldn't get User information.", JsonRequestBehavior.AllowGet);
            if (String.IsNullOrEmpty(UserInf.UserName))
                return Json("Couldn't get \"UserID\" information.", JsonRequestBehavior.AllowGet);
            var UserID = UserInf.UserName;
            var UserRole = Role.OwnerId;
            PCMQCOCalculation pcmQCOCalculation = new PCMQCOCalculation(OPS_Utils.ConstantGeneric.ConnectionStr, OPS_Utils.ConstantGeneric.ConnectionStrMes);
            pcmQCOCalculation.mEnviroment = "";
            pcmQCOCalculation.mUserID = UserID;
            pcmQCOCalculation.mQCOSource = "QCOSim"; //QCOSim or QCO 
            pcmQCOCalculation.mFactory = qcoQueue.FACTORY;
            if (qcoQueue != null)
            {
                string QCOResult_2 = "";
                Task.Run(() => pcmQCOCalculation.QCOCalculationSIM(qcoQueue.FACTORY, UserID, UserRole, out QCOResult_2));
                //pcmQCOCalculation.QCOCalculationSIM(OPS_Utils.ConstantGeneric.ConnectionStrMes, qcoQueue.FACTORY, UserID, UserRole, out QCOResult_2);
                return Json("QCO in " + qcoQueue.FACTORY + " is under process. It takes a while depending on the number of MTOPS Package under " + qcoQueue.FACTORY, JsonRequestBehavior.AllowGet);
            }
            else
                return Json("No Input Data", JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Upload Excel 
        [SysActionFilter(SystemID = "MES", MenuID = "QCO", Action = "UPDATE")]
        public ActionResult QCOExcelUpload()
        {
            /* Created by Tai Le (Thomas)
             * Create on 2019-09-26
             */
            try
            {
                string strAccumMsg = "", strMsg = "";
                bool blResult = false;
                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    System.IO.Stream documentConverted = file.InputStream;
                    ExcelPackage package = new ExcelPackage(documentConverted);
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    // get number of rows and columns in the sheet
                    int rows = worksheet.Dimension.Rows; // 20
                    int columns = worksheet.Dimension.Columns; // 7
                    List<Qcops> PCMQCOQueues = new List<Qcops>();
                    //Data
                    // loop through the worksheet rows and columns 
                    for (int i = 2; i <= rows; i++)
                    {
                        var pcmQueue = new Qcops();
                        for (int j = 1; j <= columns; j++)
                            if (worksheet.Cells[i, j].Value != null)
                            {
                                var properties = typeof(Qcops).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                                var property = properties.FirstOrDefault(x => string.Equals(x.Name, worksheet.Cells[1, j].Value.ToString(), StringComparison.OrdinalIgnoreCase));
                                if (property != null)
                                {
                                    //System.Diagnostics.Debug.WriteLine("property.PropertyType.Name= " + property.PropertyType.Name);
                                    var content = worksheet.Cells[i, j].Value.ToString();
                                    switch (property.PropertyType.Name)
                                    {
                                        case "Int32":
                                            property.SetValue(pcmQueue, Convert.ToInt32(content), null);
                                            break;
                                        case "String":
                                            property.SetValue(pcmQueue, content, null);
                                            break;
                                        case "DateTime":
                                            property.SetValue(pcmQueue, DateTime.Parse(content), null);
                                            break;
                                    }
                                }
                            }
                        PCMQCOQueues.Add(pcmQueue);
                    }
                    var REASONCounter = PCMQCOQueues.Where(f => !String.IsNullOrEmpty(f.REASON)).Count();
                    if (REASONCounter == 0)
                    {
                        return Json(new { retResult = false, retMsg = "Please input the Reason." }, JsonRequestBehavior.AllowGet);
                    }
                    var QCOSource = PCMQCOQueues[0].QCOSOURCE;
                    if (QCOSource.ToUpper() == "QCOSIM")
                        cTableName = cTableName + "SIM ";
                    //2019-11-27 Tai Le (Thomas)
                    var ExcelQCOVersion = PCMQCOQueues[0].QCOVERSION;
                    var CurrentQCOVersion = PKQCO.PCMQCOCalculation.GetQCOVersion(OPS_Utils.ConstantGeneric.ConnectionStrMes, PCMQCOQueues[0].QCOFACTORY, PCMQCOQueues[0].QCOYEAR, PCMQCOQueues[0].QCOWEEKNO, cTableName);
                    if (ExcelQCOVersion != CurrentQCOVersion)
                    {
                        return Json(new
                        {
                            retResult = false,
                            retMsg = "QCO Excel file is not the latest version. Please export QCO to get the latest one."
                        }, JsonRequestBehavior.AllowGet);
                    }
                    //2019-10-03 Tai Le (Thomas) more validation
                    var MinQCOWeek = PCMQCOQueues.Min(f => f.QCOWEEKNO);
                    var MaxQCOWeek = PCMQCOQueues.Max(f => f.QCOWEEKNO);
                    var MinQCOYear = PCMQCOQueues.Min(f => f.QCOYEAR);
                    var MaxQCOYear = PCMQCOQueues.Max(f => f.QCOYEAR);
                    if (MinQCOWeek != MaxQCOWeek || MinQCOYear != MaxQCOYear)
                    {
                        return Json(new
                        {
                            retResult = false,
                            retMsg = "Custom Ranking feature only work in one FACTORY-YEAR-WEEK. Upload Excel contains different Year or WeekNo."
                        }, JsonRequestBehavior.AllowGet);
                    }
                    CultureInfo cul = CultureInfo.CurrentCulture;
                    int weekNum = cul.Calendar.GetWeekOfYear(DateTime.Today, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                    if (MaxQCOWeek != "W" + PCMGeneralFunctions.GetRight("00" + weekNum, 2))
                    {
                        return Json(new
                        {
                            retResult = false,
                            retMsg = "Only Week [" + weekNum + "] is allowed to change. Your week data [" + MaxQCOWeek + "] is old. Please get the latest QCO Excel."
                        }, JsonRequestBehavior.AllowGet);
                    }
                    //Update  QCORANKINGNEW  (Requirement) which is KEY to correct the Priority 
                    UpdateQCORanking_3(PCMQCOQueues[0]);
                    var CHANGEQCORANK = PCMQCOQueues.Where(f => !String.IsNullOrEmpty(f.CHANGEQCORANK));
                    var _CHANGEQCORANKCounter = CHANGEQCORANK.Select(q => q.CHANGEQCORANK).Distinct().Count();
                    //var QCOSTARTDATE = PCMQCOQueues.Where(f => !String.IsNullOrEmpty(f.QCOSTARTDATE));//2020-04-25 Tai Le(Thomas)
                    if (CHANGEQCORANK.Count() != _CHANGEQCORANKCounter)
                        return Json(new { retResult = false, retMsg = "ChangeQCORanking Duplicated.<br/>Progress quit." }, JsonRequestBehavior.AllowGet);
                    if (REASONCounter == CHANGEQCORANK.Count() && REASONCounter == 1)
                    {
                        /* 
                         * Simple case: just SWITCH the  CHANGEQCORANK each other */
                        var HandleList = PCMQCOQueues.Where(f => f.REASON != String.Empty && f.CHANGEQCORANK != String.Empty && Convert.ToInt32(f.CHANGEQCORANK) > 0).ToList();
                        if (HandleList.Count == 1)
                        {
                            for (int intI = 0; intI < HandleList.Count; intI++)
                            {
                                var _item = HandleList[intI];
                                strMsg = _item.UpdateCustomRanking(OPS_Utils.ConstantGeneric.ConnectionStrMes, UserInf.UserName);
                                if (String.IsNullOrEmpty(strAccumMsg)) strAccumMsg = strMsg;
                                else strAccumMsg = strAccumMsg + "<BR/>" + strMsg;
                            }
                        }
                    }
                    else
                    {
                        /* 
                         * Complex case:
                         *      Step 1./ From PCMQCOQueues, WHERE( f => !String.IsNullOrEmpty(f.CHANGEQCORANK) && !String.IsNullOrEmpty(f.REASON) )
                         *      Step 2./ From PCMQCOQueues, WHERE( f => String.IsNullOrEmpty(f.REASON) )
                         *      Step 3./ From (1.) find the Max (QCORank) ; Filter out the Range need to Handle [...] >> Count it 
                         *      Step 4./ Run the For from 1 to (3).Counter 
                         *               Update the CHANGE_QCORANK
                         */
                        //Step 1./
                        var ManualChangeQCORankList = PCMQCOQueues.Where(f => f.REASON != String.Empty && f.CHANGEQCORANK != String.Empty && Convert.ToInt32(f.CHANGEQCORANK) > 0).ToList();
                        var EmptyManualChangeQCORank = PCMQCOQueues.Where(f => String.IsNullOrEmpty(f.CHANGEQCORANK) || String.IsNullOrEmpty(f.REASON)).ToList().OrderBy(f => f.QCORANK).ToList();
                        //var ManualChangeQCORank_MinCHANGEQCORANK = ManualChangeQCORankList.Min(f => Convert.ToInt32(f.CHANGEQCORANK));
                        var ManualChangeQCORank_MaxCHANGEQCORANK = ManualChangeQCORankList.Max(f => Convert.ToInt32(f.CHANGEQCORANK));
                        //var ManualChangeQCORank_MinQCORANK = ManualChangeQCORankList.Min(f => f.QCORANK);
                        var ManualChangeQCORank_MaxQCORANK = ManualChangeQCORankList.Max(f => f.QCORANK);
                        //var LowerBound = Math.Min(ManualChangeQCORank_MinCHANGEQCORANK, ManualChangeQCORank_MinQCORANK);
                        var UpperBound = Math.Max(ManualChangeQCORank_MaxCHANGEQCORANK, ManualChangeQCORank_MaxQCORANK);
                        //System.Diagnostics.Debug.WriteLine("Only the QCORANKING between "+ ManualChangeQCORank_MinQCORANK + " and "+ ManualChangeQCORank_MaxQCORANK + " affected.");
                        List<Qcops> _HandleList = new List<Qcops>();
                        int J = 0;
                        for (int I = 1; I <= UpperBound; I++)
                        {
                            /*Case 1: loop "I" inside  [ManualChangeQCORankList] */
                            if (ManualChangeQCORankList.Where(f => Convert.ToInt32(f.CHANGEQCORANK) == I).ToList().Count > 0)
                            {
                                _HandleList.Add(ManualChangeQCORankList.Where(f => Convert.ToInt32(f.CHANGEQCORANK) == I).FirstOrDefault());
                                J += 1;
                                continue;
                            }
                            /* Case 2: loop "I" inside [PCMQCOQueues] */
                            _HandleList.Add(EmptyManualChangeQCORank[I - 1 - J]);
                        }
                        blResult = SaveQCPSRankingMain(_HandleList, "", out strAccumMsg);
                    }
                    /// Step 3: 
                    /// Update QCORANKINGNEW Based On Is_NULL(CHANGEQCORANK, QCORANK), 
                    /// QCORANKINGNEW starts from 1 to n 
                    UpdateQCORanking_3(PCMQCOQueues[0]);
                    //Update the capacity 
                    PCMQCOCalculation pcmQCOCalculation = new PCMQCOCalculation("");
                    pcmQCOCalculation.mEnviroment = "";
                    pcmQCOCalculation.mFactory = PCMQCOQueues[0].QCOFACTORY;
                    pcmQCOCalculation.mUserID = Session["LoginUserID"].ToString();
                    pcmQCOCalculation.mQCOSource = QCOSource;
                    Task.Run(() => pcmQCOCalculation.CalculateCapaAll());
                }
                return Json(new { retResult = true, retMsg = "Change QCO Ranking Upload Result<br/>" + strAccumMsg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { retResult = false, retMsg = "ERROR:<BR/>" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [SysActionFilter(SystemID = "MES", MenuID = "QCO", Action = "UPDATE")]
        public ActionResult ExcelUploadReadiness()
        {
            /* Created by Tai Le (Thomas)
             * Create on 2020-03-31
             */
            try
            {
                //string strAccumMsg = "", strMsg = "";
                //bool blResult = false;
                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    System.IO.Stream documentConverted = file.InputStream;
                    ExcelPackage package = new ExcelPackage(documentConverted);
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    // get number of rows and columns in the sheet
                    int rows = worksheet.Dimension.Rows; // 20
                    int columns = worksheet.Dimension.Columns; // 7
                    List<Qcops> PCMQCOQueues = new List<Qcops>();
                    // Deserialize excel to List<Qcops>
                    // loop through the worksheet rows and columns 
                    for (int i = 2; i <= rows; i++)
                    {
                        var pcmQueue = new Qcops();
                        for (int j = 1; j <= columns; j++)
                            if (worksheet.Cells[i, j].Value != null)
                            {
                                var properties = typeof(Qcops).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                                var property = properties.FirstOrDefault(x => string.Equals(x.Name, worksheet.Cells[1, j].Value.ToString(), StringComparison.OrdinalIgnoreCase));
                                if (property != null)
                                {
                                    //System.Diagnostics.Debug.WriteLine("property.PropertyType.Name= " + property.PropertyType.Name);
                                    var content = worksheet.Cells[i, j].Value.ToString();
                                    switch (property.PropertyType.Name)
                                    {
                                        case "Int32":
                                            property.SetValue(pcmQueue, Convert.ToInt32(content), null);
                                            break;
                                        case "String":
                                            property.SetValue(pcmQueue, content, null);
                                            break;
                                        case "DateTime":
                                            property.SetValue(pcmQueue, DateTime.Parse(content), null);
                                            break;
                                    }
                                }
                            }
                        PCMQCOQueues.Add(pcmQueue);
                    }
                    if (PCMQCOQueues.Count == 0)
                        continue;
                    //2019-11-27 Tai Le (Thomas)
                    var ExcelQCOVersion = PCMQCOQueues[0].QCOVERSION;
                    var CurrentQCOVersion = PKQCO.PCMQCOCalculation.GetQCOVersion(OPS_Utils.ConstantGeneric.ConnectionStrMes, PCMQCOQueues[0].QCOFACTORY, PCMQCOQueues[0].QCOYEAR, PCMQCOQueues[0].QCOWEEKNO, cTableName);
                    if (ExcelQCOVersion != CurrentQCOVersion)
                    {
                        return Json(new
                        {
                            retResult = false,
                            retMsg = "QCO Excel file is not the latest version. Please export QCO to get the latest one."
                        }, JsonRequestBehavior.AllowGet);
                    }
                    //bool isHaveUpdate = false;
                    for (int I = 0; I < PCMQCOQueues.Count; I++)
                    {
                        //Reset the flag
                        //isHaveUpdate = false;
                        //Update FINSOREADINESS ; JIGREADINESS ; SOPREADINESS in [T_QC_QUEUE]
                        var currQCO = PCMQCOQueues[I];
                        string FINSOREADINESS = "0", JIGREADINESS = "0", SOPREADINESS = "0";
                        StringBuilder spUpdate = new StringBuilder();
                        spUpdate.Append($" LASTUPDATEDATE = Sysdate ");
                        if (!String.IsNullOrEmpty(currQCO.FINSOREADINESS))
                        {
                            FINSOREADINESS = currQCO.FINSOREADINESS == "Y" ? "1" : "0";
                            //isHaveUpdate = true;
                            spUpdate.Append($" ,FINSOREADINESS =  '{FINSOREADINESS}'  ");
                        }
                        if (!String.IsNullOrEmpty(currQCO.JIGREADINESS))
                        {
                            JIGREADINESS = currQCO.JIGREADINESS == "Y" ? "1" : "0";
                            //isHaveUpdate = true;
                            spUpdate.Append($" , JIGREADINESS = '{JIGREADINESS}'  ");
                        }
                        if (!String.IsNullOrEmpty(currQCO.SOPREADINESS))
                        {
                            SOPREADINESS = currQCO.SOPREADINESS == "Y" ? "1" : "0";
                            //isHaveUpdate = true;
                            spUpdate.Append($" , SOPREADINESS = '{SOPREADINESS}'  ");
                        }
                        //2020-05-07 Tai Le(Thomas)
                        if (!String.IsNullOrEmpty(currQCO.QCOSTARTDATE))
                        {
                            DateTime dt = DateTime.Today;
                            if (DateTime.TryParse(currQCO.QCOSTARTDATE, out dt))
                            {
                                //isHaveUpdate = true;
                                spUpdate.Append($" , QCOSTARTDATE = To_date('{dt:yyyyMMdd}', 'yyyyMMdd')  ");
                            }
                            else
                            {
                                if (DateTime.TryParseExact(currQCO.QCOSTARTDATE, "yyyy-MM-dd", new CultureInfo(""), DateTimeStyles.None, out dt))
                                {
                                    spUpdate.Append($" , QCOSTARTDATE = To_date('{dt:yyyyMMdd}', 'yyyyMMdd')  ");
                                }
                                else
                                {
                                    //2020-10-16 Tai Le(Thomas)
                                    if (DateTime.TryParseExact(currQCO.QCOSTARTDATE, "yyyyMMdd", new CultureInfo(""), DateTimeStyles.None, out dt))
                                    {
                                        spUpdate.Append($" , QCOSTARTDATE = To_date('{dt:yyyyMMdd}', 'yyyyMMdd')  ");
                                    }
                                }
                            }
                        }
                        var sql =
                                $" Update T_QC_QUEUE " +
                                $" SET {spUpdate.ToString()}" +
                                $" Where QCOFACTORY = '{currQCO.QCOFACTORY}' " +
                                $" And QCOYEAR = {currQCO.QCOYEAR} " +
                                $" And QCOWEEKNO = '{currQCO.QCOWEEKNO}' " +
                                $" And PRDPKG = '{currQCO.PRDPKG}' ";
                        OPS_DAL.DAL.OracleDbManager.ExecuteQuery(sql, null, CommandType.Text, OPS_Utils.ConstantGeneric.ConnectionStrMes);
                    }
                }
                return Json(new { retResult = true, retMsg = "QCO Readiness Upload Result<br/>" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { retResult = false, retMsg = "ERROR:<BR/>" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        //[SysActionFilter(RoleID = "5113;5**0")]
        [SysActionFilter(SystemID = "MES", MenuID = "QCO", Action = "UPDATE")]
        public ActionResult CentralizeSIMQCO(string qCOFACTORY, int qCOYEAR, string qCOWEEKNO)
        {
            var UserID = UserInf.UserName;
            PCMQCOCalculation pcmQCOCalculation = new PCMQCOCalculation("");
            pcmQCOCalculation.mUserID = UserID;
            pcmQCOCalculation.mFactory = qCOFACTORY;
            pcmQCOCalculation.mYear = qCOYEAR;
            pcmQCOCalculation.mWeekNo = qCOWEEKNO;
            Task.Run(() => pcmQCOCalculation.CentralizeSIMQCO(OPS_Utils.ConstantGeneric.ConnectionStrMes));
            //pcmQCOCalculation.CentralizeSIMQCO(OPS_Utils.ConstantGeneric.ConnectionStrMes);
            return Json("Centralize QCO in " + qCOFACTORY + " is under process. It takes a while", JsonRequestBehavior.AllowGet);
        }
        
       
        private int T_QC_QUEUE_MAXQCORANK(string qCOFACTORY, int qCOYEAR, string qCOWEEKNO)
        {
            int intT_QC_QUEUE_MAXQCORANK = 0;
            var strSQL =
                " Select MAX(QCORANK) QCORANK From PKMES.T_QC_QUEUE " +
                " Where QCOFactory = :QCOFactory and QCOWeekNo = :QCOWeekNo and QCOYear = :QCOYear ";
            OracleConnection oracleConnection = new OracleConnection(OPS_Utils.ConstantGeneric.ConnectionStrMes);
            oracleConnection.Open();
            OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
            oracleDataAdapter.SelectCommand.Parameters.Add("QCOFactory", qCOFACTORY);
            oracleDataAdapter.SelectCommand.Parameters.Add("QCOWeekNo", qCOWEEKNO);
            oracleDataAdapter.SelectCommand.Parameters.Add("QCOYear", qCOYEAR);
            DataTable dt = new DataTable();
            oracleDataAdapter.Fill(dt);
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    intT_QC_QUEUE_MAXQCORANK = Convert.ToInt32(dt.Rows[0]["QCORANK"].ToString());
                }
                dt.Dispose();
            }
            oracleDataAdapter.Dispose();
            oracleConnection.Close();
            oracleConnection.Dispose();
            return intT_QC_QUEUE_MAXQCORANK;
        }
        private bool FactoryHasMaterialParameter(string vstrFactory, out List<OPS_DAL.QCOEntities.Qcfo> lsFactoryParameters)
        {
            lsFactoryParameters = null;
            bool blHasMaterialQtyPara = false;
            //Return an empty list of opmt if keys code is empty. 
            if (string.IsNullOrEmpty(vstrFactory))
                return false;
            //GET FACTORY SORTING SETTING;
            string strSQL = "";
            strSQL = "SELECT T_CM_QCOP.DBFIELDNAME , T_00_QCFO.FACTORY , T_CM_QCOP.PARAMETERNAME, NVL(T_00_QCFO.SORTDIRECTION, 'ASC')  as SORTDIRECTION " +
                     " FROM PKMES.T_00_QCFO " +
                     " INNER JOIN PKMES.T_CM_QCOP ON " +
                     "   T_00_QCFO.PARAMETERNAME = T_CM_QCOP.PARAMETERNAME " +
                     " WHERE T_00_QCFO.FACTORY = '" + vstrFactory + "' " +
                     " ORDER BY T_00_QCFO.SORTINGSEQ ";
            DataTable dt = new DataTable();
            dt = OracleDbManager.Query(strSQL, null);
            if (dt != null)
            {
                List<OPS_DAL.QCOEntities.Qcfo> lsTempFactoryParameters = new List<OPS_DAL.QCOEntities.Qcfo>();
                int intTemp = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    if (!blHasMaterialQtyPara)
                        intTemp += 1;
                    if (dr["PARAMETERNAME"].ToString() == "Material Readiness")
                    {
                        blHasMaterialQtyPara = true;
                    }
                    lsTempFactoryParameters.Add(new OPS_DAL.QCOEntities.Qcfo(dr["FACTORY"].ToString(), dr["PARAMETERNAME"].ToString(), dr["DBFIELDNAME"].ToString(), dr["SORTDIRECTION"].ToString()));
                }
                if (intTemp == dt.Rows.Count)
                    blHasMaterialQtyPara = false;
                lsFactoryParameters = lsTempFactoryParameters;
                dt.Dispose();
            }
            return blHasMaterialQtyPara;
        }
        public ActionResult GetYearWeeks(string vstrYear)
        {
            string idDataValue = vstrYear; //Url.RequestContext.RouteData.Values["id"].ToString();
            if (String.IsNullOrEmpty(idDataValue))
                idDataValue = DateTime.Today.Year.ToString();
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            DateTime date1 = new DateTime(Convert.ToInt16(idDataValue), 12, 31);
            Calendar cal = dfi.Calendar;
            var NumberOfWeek = cal.GetWeekOfYear(date1, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
            //2019-11-29 Tai Le
            CultureInfo cul = CultureInfo.CurrentCulture;
            int weekNum = cul.Calendar.GetWeekOfYear(DateTime.Today, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            return Json(new { NumberOfWeeks = NumberOfWeek, CurrentWeek = weekNum }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetLatestQCOMaterialReadiness(QCOQueue qcoQueue)
        {
            /** Function Description
             * Creator : Tai Le (Thomas)
             * Create Time: 2019-07-22
             * Purpose: Re-calculate the Material Readiness based on current Date
             * Input:   
             *          AONo
             *          Factory
             *          StyleCode
             *          StyleSize
             *          StyleColorSerial
             *          RevNo
             *          MTOPS Production Package
             * Ouput:   QCO Material Readiness
             */
            /* Tai Le(Thomas): 2019-06-17: Handle Check New Material Readiness */
            try
            {
                //Check Role Permission (SRMT)
                if (Role == null || Role.IsUpdate != "1")
                    return Json(ConstantGeneric.NoAuthority, JsonRequestBehavior.AllowGet);
                //string QCOResult_1 = "", QCOResult_2;
                //string QCOResult_2;
                PCMQCOCalculation pcmQCOCalculation = new PCMQCOCalculation("");
                pcmQCOCalculation.mEnviroment = "";
                //Fire the QCO Calculation for Single Selected Package 
                //QCOResult_1 = pcmQCOCalculation.QCOCalculation(OPS_Utils.ConstantGeneric.ConnectionStr, qcoQueue.FACTORY, UserInf.UserName, Role.OwnerId, String.Empty, true, qcoQueue.AONO , qcoQueue.STYLECODE, qcoQueue.STYLESIZE, qcoQueue.STYLECOLORSERIAL, qcoQueue.REVNO, qcoQueue.PRDPKG, out QCOResult_2);
                //var strRes = QCOResult_1 == String.Empty ? ConstantGeneric.Success : ConstantGeneric.Fail;
                //Task.Run(() => pcmQCOCalculation.QCOCalculation(OPS_Utils.ConstantGeneric.ConnectionStr, qcoQueue.FACTORY, UserInf.UserName, Role.OwnerId, String.Empty, true, qcoQueue.AONO, qcoQueue.STYLECODE, qcoQueue.STYLESIZE, qcoQueue.STYLECOLORSERIAL, qcoQueue.REVNO, qcoQueue.PRDPKG));
                return Json("Material Readiness is under process. It takes a while to finish.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("GetLatestQCOMaterialReadiness() ERROR: " + ex.Message, JsonRequestBehavior.AllowGet);
            }
            //return Json("", JsonRequestBehavior.AllowGet);
        }
        
        #region QCO CAPA 
        /* Method  REGION */
        //[SysActionFilter(RoleID = "5000;5100;5110;5111;5113")]
        [SysActionFilter(SystemID = "MES", MenuID = "QCO", Action = "")]
        public ActionResult QCOCAPA()
        {
            ViewBag.PageTitle = "<i class=\"fa fa-list\"></i>&nbsp;QCO";
            ViewBag.SubPageTitle = "&nbsp;<span>> QCO Capacity</span>";
            return View();
        }
        public string ShowQCOCapa(GridSettings gridRequest)
        {
            try
            {
                string strSQL = "";
                strSQL =
                    " SELECT ROW_NUMBER() OVER(ORDER BY NVL(CHANGEQCORANK, QCORANK) ) AS RANKING , " +
                    " T_00_STMT.STYLENAME , T_00_STMT.BUYERSTYLECODE , T_00_STMT.BUYERSTYLENAME , T_00_SCMT.STYLECOLORWAYS , " +
                    " T_QC_QUEUE.* , " +
                    " VIEW_ERP_PSRSNP_PLAN.ADTypeName , " +
                    " VIEW_ERP_PSRSNP_PLAN.DELIVERYDATE as AODELIVERYDATE , " +
                    " NVL(CHANGEQCORANK, QCORANK) as CHANGEQCORANKSHOW " +
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
                    "       AND T_QC_QUEUE.PRDPKG = VIEW_ERP_PSRSNP_PLAN.PRDPKG ";
                var _Result = GridData.GetGridData(ConstantGeneric.ConnectionStr, strSQL, " T_QC_QUEUE.USAGECAPA IS NOT NULL ", gridRequest, "dd MMM, yyyy");
                return _Result;
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { retResult = false, dataRow = ex.Message });
            }
        }
        //[SysActionFilter(RoleID = "5000;5100;5110;5111;5113")]
        [SysActionFilter(SystemID = "MES", MenuID = "QCO", Action = "")]
        public string CapaAllocation(string StartWeek)
        //public string CapaAllocation(string Factory)
        {
            if (Url.RequestContext.RouteData.Values["id"] == null)
                return "Factory is required.";
            string idFactory = Url.RequestContext.RouteData.Values["id"].ToString();
            //if (String.IsNullOrEmpty(StartWeek))
            //	return "Factory is required.";
            //PCMQCOCalculation pcmQCOCalculation = new PCMQCOCalculation("");
            PCMQCOCalculation pcmQCOCalculation = new PCMQCOCalculation("", OPS_Utils.ConstantGeneric.ConnectionStrMes);
            pcmQCOCalculation.mEnviroment = "";
            //pcmQCOCalculation.mFactory = Factory;
            pcmQCOCalculation.mFactory = idFactory == "_all_" ? "%" : idFactory;
            pcmQCOCalculation.mUserID = UserInf.UserName;
            pcmQCOCalculation.mQCOSource = "QCO";
            Task.Run(() => pcmQCOCalculation.CalculateCapaAll(StartWeek));
            //pcmQCOCalculation.CalculateCapaAll(OPS_Utils.ConstantGeneric.ConnectionStrMes, StartWeek);
            return "Capacity Distribution is processing in background. It takes a moment to finish.";
        }
        #endregion
        
        #region Production Weekly Efficiency
        public ActionResult WeeklyEfficiencySetting()
        {
            /* Created on: 2019-10-10
             * Creator: Tai Le (Thomas) */
            ViewBag.PageTitle = "<i class=\"fa fa-home\"></i>&nbsp;Factory";
            ViewBag.SubPageTitle = "&nbsp;<span>> Weekly Efficiency Setting</span>";
            return View();
        }
        public ActionResult FWESEdit(OPS_DAL.QCOEntities.FWES objFWES)
        {
            //return View();
            return PartialView("FWESEdit", objFWES);
        }
        public string GetFWES(GridSettings gridRequest)
        {
            var strSQL =
                " SELECT  ROW_NUMBER() OVER(ORDER BY T_CM_FWES.FACTORY, T_CM_FWES.YEAR , T_CM_FWES.WEEKNO) AS RANKING , " +
                " T_CM_FWES.* , " +
                " '/QCO/FWESEdit?Id=' || T_CM_FWES.FACTORY ||';' || T_CM_FWES.YEAR || ';' || T_CM_FWES.WEEKNO as ACTEDIT " +
                " FROM PKMES.T_CM_FWES ";
            var strSQLWhere = "";
            var _Result = GridData.GetGridData(ConstantGeneric.ConnectionStrMes, strSQL, strSQLWhere, gridRequest);
            return _Result;
        }
        public ActionResult NewFWES(OPS_DAL.QCOEntities.FWES objFWES)
        {
            var Result = OPS_DAL.QCOEntities.FWESBus.SaveFWES(OPS_Utils.ConstantGeneric.ConnectionStrMes, objFWES);
            if (Result)
                return Json(new { retResult = Result, retMessage = "New Weekly Efficiency Added" }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { retResult = Result, retMessage = "New Weekly Efficiency FAILED" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveFWES(OPS_DAL.QCOEntities.FWES objFWES)
        {
            var Result = OPS_DAL.QCOEntities.FWESBus.SaveFWES(OPS_Utils.ConstantGeneric.ConnectionStrMes, objFWES);
            if (Result)
                return Json(new { retResult = Result, retMessage = "Weekly Efficiency Updated" }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { retResult = Result, retMessage = "Update Weekly Efficiency FAILED" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ConfirmFWES(List<OPS_DAL.QCOEntities.FWES> FWESList)
        {
            for (int I = 0; I < FWESList.Count; I++)
            {
                var _item = FWESList[I];
                OPS_DAL.QCOEntities.FWESBus.ConfirmFWES(OPS_Utils.ConstantGeneric.ConnectionStrMes, _item);
            }
            return Json(new { retResult = true, retMessage = "New Weekly Efficiency Added" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult RemoveFWES(List<OPS_DAL.QCOEntities.FWES> FWESList)
        {
            string Msg = ""
                , AccumMsg = "";
            for (int I = 0; I < FWESList.Count; I++)
            {
                var _item = FWESList[I];
                Msg = "";
                var res = OPS_DAL.QCOEntities.FWESBus.RemoveFWES(OPS_Utils.ConstantGeneric.ConnectionStrMes, _item).Result;
                Msg = res ?
                    $"Factory[{_item.FACTORY}], Year[{_item.YEAR}], Week[{_item.WEEKNO}] Removed: SUCCEED." :
                    $"Factory[{_item.FACTORY}], Year[{_item.YEAR}], Week[{_item.WEEKNO}] Removed: FAILED.";
                if (String.IsNullOrEmpty(AccumMsg))
                {
                    AccumMsg = Msg;
                }
                else
                    AccumMsg = $"{AccumMsg}<BR/>{Msg}";
            }
            return Json(new { retMessage = AccumMsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region RETIRED FUNCTIONS 
        [SysActionFilter(SystemID = "MES", MenuID = "QCO", Action = "")]
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
                DateTime dtStart = DateTime.Now;
                //Debug.Print("ShowQCPM(): START " + dtStart.ToString("s"));
                string idFactory = Url.RequestContext.RouteData.Values["id"].ToString();
                string IsCleanData = Url.RequestContext.HttpContext.Request["IsCleanData"] != null ? Url.RequestContext.HttpContext.Request["IsCleanData"] : "N";
                List<OPS_DAL.QCOEntities.Qcfo> lsFactoryParameters;
                bool IsFactoryHasMaterialPara = FactoryHasMaterialParameter(idFactory, out lsFactoryParameters);
                if (lsFactoryParameters.Count == 0)
                    return JsonConvert.SerializeObject(new { retResult = false, dataRow = "Please Set Up Factory Paramater" });
                using (OracleConnection oracleConnection = new OracleConnection(ConstantGeneric.ConnectionStr))
                {
                    oracleConnection.Open();
                    ////Return an empty list of opmt if keys code is empty. 
                    //if (string.IsNullOrEmpty(idFactory))
                    //    return JsonConvert.SerializeObject(new { retResult = false, dataRow = "Factory missed" });
                    //Variable Declare
                    string strSQL = "", strSorting = "";
                    if (IsCleanData == "N")
                        goto HE_ShowQCPMCon;
                    // Clean data of T_QC_QCPM and Recalculate...
                    strSQL = "Delete PKMES.T_QC_QCPM Where Factory = :FACTORY ";
                    OracleDbManager.ExecuteQuery(strSQL, new List<OracleParameter> { new OracleParameter("FACTORY", idFactory) }.ToArray(), CommandType.Text, 180);
                    // *****************************************************************************
                    //Get Data Of Sorted Package with BOM and prepare for Distribution
                    //strSQL = " SELECT T_QC_QCFP.* , V_MRP_PP_WO.WONO , " +
                    //         " T_SD_BOMT.MAINITEMCODE , T_SD_BOMT.MAINITEMCOLORSERIAL , " +
                    //         " T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL , " +
                    //         " T_QC_QCFP.PLANQTY * T_SD_BOMT.UNITCONSUMPTION AS REQUESTQTY " +
                    //         " FROM PKMES.T_QC_QCFP " +
                    //         " INNER JOIN PKMES.V_MRP_PP_WO ON " +
                    //         "      T_QC_QCFP.FACTORY = V_MRP_PP_WO.FACTORY " +
                    //         "      AND T_QC_QCFP.AONO = V_MRP_PP_WO.AONO " +
                    //         "      AND T_QC_QCFP.STYLECODE = V_MRP_PP_WO.STLCD " +
                    //         "      AND T_QC_QCFP.STYLESIZE = V_MRP_PP_WO.STLSIZ " +
                    //         "      AND T_QC_QCFP.STYLECOLORSERIAL = V_MRP_PP_WO.STLCOSN " +
                    //         "      AND T_QC_QCFP.REVNO = V_MRP_PP_WO.STLREVN " +
                    //         "      AND T_QC_QCFP.PRDPKG = V_MRP_PP_WO.PRODPACKAGE " +
                    //         " INNER JOIN T_SD_BOMT ON " +
                    //         "      T_QC_QCFP.STYLECODE = T_SD_BOMT.STYLECODE " +
                    //         "      AND T_QC_QCFP.STYLESIZE = T_SD_BOMT.STYLESIZE " +
                    //         "      AND T_QC_QCFP.STYLECOLORSERIAL = T_SD_BOMT.STYLECOLORSERIAL " +
                    //         "      AND T_QC_QCFP.REVNO = T_SD_BOMT.REVNO " +
                    //         " WHERE T_QC_QCFP.FACTORY = :FACTORY " +
                    //         " AND T_SD_BOMT.ITEMCODE NOT LIKE 'PKG%' " +
                    //         " ORDER BY NVL(T_QC_QCFP.CHANGEOVERRANKING,T_QC_QCFP.RANGKING), " +
                    //         "          T_QC_QCFP.PRDPKG, " +
                    //         "          T_SD_BOMT.ITEMCODE, " +
                    //         "          T_SD_BOMT.UNITCONSUMPTION ";
                    strSQL = " SELECT ROW_NUMBER() OVER(PARTITION BY T_QC_QCFP.FACTORY ORDER BY T_QC_QCFP.FACTORY, T_QC_QCFP.DELIVERYDATE , T_QC_QCFP.ORDQTY ,  T_QC_QCFP.PLANQTY , T_QC_QCFP.AONO , T_QC_QCFP.STYLECODE , T_QC_QCFP.STYLESIZE , T_QC_QCFP.STYLECOLORSERIAL , T_QC_QCFP.REVNO , T_QC_QCFP.PRDPKG ) AS RowSeqNo , " +
                             " T_QC_QCFP.* , V_MRP_PP_WO.WONO , " +
                             " T_SD_BOMT.MAINITEMCODE , T_SD_BOMT.MAINITEMCOLORSERIAL , " +
                             " T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL , " +
                             " T_QC_QCFP.PLANQTY * T_SD_BOMT.UNITCONSUMPTION AS REQUESTQTY " +
                             " FROM PKERP.VIEW_ERP_PSRSNP_PLAN  T_QC_QCFP " +
                             " LEFT JOIN PKERP.V_AO_PPDP ON " +
                             "      T_QC_QCFP.FACTORY = V_AO_PPDP.FACTORY " +
                             "      AND T_QC_QCFP.AONO = V_AO_PPDP.AONO " +
                             "      AND T_QC_QCFP.STYLECODE = V_AO_PPDP.STYLECODE " +
                             "      AND T_QC_QCFP.STYLESIZE = V_AO_PPDP.STYLESIZE " +
                             "      AND T_QC_QCFP.STYLECOLORSERIAL = V_AO_PPDP.STYLECOLORSERIAL " +
                             "      AND T_QC_QCFP.REVNO = V_AO_PPDP.REVNO " +
                             "      AND T_QC_QCFP.PRDPKG = V_AO_PPDP.PRDPKG " +
                             " INNER JOIN PKMES.V_MRP_PP_WO ON " +
                             "      T_QC_QCFP.FACTORY = V_MRP_PP_WO.FACTORY " +
                             "      AND T_QC_QCFP.AONO = V_MRP_PP_WO.AONO " +
                             "      AND T_QC_QCFP.STYLECODE = V_MRP_PP_WO.STLCD " +
                             "      AND T_QC_QCFP.STYLESIZE = V_MRP_PP_WO.STLSIZ " +
                             "      AND T_QC_QCFP.STYLECOLORSERIAL = V_MRP_PP_WO.STLCOSN " +
                             "      AND T_QC_QCFP.REVNO = V_MRP_PP_WO.STLREVN " +
                             "      AND T_QC_QCFP.PRDPKG = V_MRP_PP_WO.PRODPACKAGE " +
                             " INNER JOIN T_SD_BOMT ON " +
                             "      T_QC_QCFP.STYLECODE = T_SD_BOMT.STYLECODE " +
                             "      AND T_QC_QCFP.STYLESIZE = T_SD_BOMT.STYLESIZE " +
                             "      AND T_QC_QCFP.STYLECOLORSERIAL = T_SD_BOMT.STYLECOLORSERIAL " +
                             "      AND T_QC_QCFP.REVNO = T_SD_BOMT.REVNO " +
                             " WHERE T_QC_QCFP.FACTORY = :FACTORY " +
                             " AND (T_SD_BOMT.ITEMCODE NOT LIKE 'PKG%' OR T_SD_BOMT.ITEMCODE NOT LIKE 'TRE%' )  " +
                             " AND T_QC_QCFP.STATUS NOT IN ( '**' , 'AC', 'F-' , 'GD' , 'PS' , 'R-' , 'WC' ) " +
                             " AND T_QC_QCFP.FACCLOSE = 'N' " +
                             " AND NVL(T_QC_QCFP.ORDQTY,0) - NVL(V_AO_PPDP.PRDQTY,0) > 0 " +
                             " ORDER BY T_QC_QCFP.FACTORY ";
                    //" AND T_QC_QCFP.DELIVERYDATE > TO_DATE('20181231' , 'yyyyMMdd') "
                    //" AND T_QC_QCFP.DELIVERYDATE BETWEEN TO_DATE('20181201' ,'yyyyMMdd') AND TO_DATE('20181231' ,'yyyyMMdd') "
                    DataTable dt_QCFP = new DataTable();
                    dt_QCFP = OracleDbManager.Query(strSQL, new List<OracleParameter> { new OracleParameter("FACTORY", idFactory) }.ToArray());
                    /* 
                     * if Parameter has Material Qty, Distribute the Received Qty for each Package first
                     *      after that, distribute the Qty not coming
                     *      
                     * if Parameter NOT has Material Qty, Distribute Receive Qty and Comming Delivery Qty at the same time.
                     */
                    strSQL = " SELECT WO , ITEM_CD , COLOR_SERIAL , ETA , SUM(SHIP_QTY) PLAN_DOQTY " +
                             " FROM KMS_PSRSHP_TBL@AOMTOPS " +
                             " WHERE DELFLG = 'N' " +
                             " AND ETA IS NOT NULL " +
                             " AND Length(ETA) = 8 " +
                             " GROUP BY WO , ITEM_CD , COLOR_SERIAL , ETA  ";
                    OracleCommand oracleCommand = new OracleCommand(strSQL, oracleConnection);
                    var dtReader = oracleCommand.ExecuteReader();
                    if (dtReader.HasRows)
                    {
                        //#Noted: Write the Data Into T_QC_QCPM 
                        DataTable tmpdt_T_QC_QCPM = new DataTable();
                        OracleDataAdapter oracleDataAdapter = new OracleDataAdapter("Select * From PKMES.T_QC_QCPM WHERE 1=2", oracleConnection);
                        oracleDataAdapter.Fill(tmpdt_T_QC_QCPM);
                        int intTemp = 1;
                        if (IsFactoryHasMaterialPara)
                            intTemp = 2;
                        DataView dataView = new DataView();
                        DataColumn newColumn = new DataColumn("ASSIGNEDQTY", typeof(System.Double)) { DefaultValue = 0.0 };
                        dt_QCFP.Columns.Add(newColumn);
                        newColumn = new DataColumn("MATNORNALRATE", typeof(System.Double)) { DefaultValue = 0.0 };
                        dt_QCFP.Columns.Add(newColumn);
                        int intSeqNo = 0;
                        double decAssignQty = 0;
                        for (int i = 0; i < intTemp; i++)
                        {
                            //#Noted: Resort  "dt_QCFP" again based on the Material , Material Rate = SUM(ASSIGNEDQTY) / SUM(REQUESTQTY)  ForEach  PP 
                            dataView = dt_QCFP.DefaultView;
                            strSorting = "";
                            if (!IsFactoryHasMaterialPara || (i == 1 && IsFactoryHasMaterialPara))
                            {
                                foreach (OPS_DAL.QCOEntities.Qcfo FactoryParameter in lsFactoryParameters)
                                {
                                    if (String.IsNullOrEmpty(strSorting))
                                        strSorting = FactoryParameter.DBFIELDNAME + " " + FactoryParameter.SORTDIRECTION;
                                    else
                                        strSorting = strSorting + ", " + FactoryParameter.DBFIELDNAME + " " + FactoryParameter.SORTDIRECTION;
                                }
                                //dataView.Sort = strSorting;
                            }
                            else if (IsFactoryHasMaterialPara && i == 0)
                            {
                                foreach (OPS_DAL.QCOEntities.Qcfo FactoryParameter in lsFactoryParameters)
                                {
                                    if (FactoryParameter.DBFIELDNAME == "ASSIGNEDQTY")
                                        break;
                                    if (String.IsNullOrEmpty(strSorting))
                                        strSorting = FactoryParameter.DBFIELDNAME + " " + FactoryParameter.SORTDIRECTION;
                                    else
                                        strSorting = strSorting + ", " + FactoryParameter.DBFIELDNAME + " " + FactoryParameter.SORTDIRECTION;
                                }
                                //dataView.Sort = strSorting;
                            }
                            strSorting = strSorting + ", ITEMCODE , ITEMCOLORSERIAL ";
                            dataView.Sort = strSorting;
                            //Sort dt_QCFP
                            dt_QCFP = dataView.ToTable();
                            bool blIsInsert = false;
                            while (dtReader.Read())
                            {
                                var DOQTY = Convert.ToDouble(dtReader["PLAN_DOQTY"].ToString());
                                if (DOQTY <= 0)
                                    continue;
                                string expression = " WONO = '" + dtReader["WO"] + "' " +
                                                    " AND ITEMCODE = '" + dtReader["ITEM_CD"] + "'  " +
                                                    " AND ITEMCOLORSERIAL = '" + dtReader["COLOR_SERIAL"] + "' ";
                                //#Noted: First Loop: Handle the DO Qty with Matched WO
                                //#Noted: Second Loop: Handle the DO Qty with Unmatched WO : WO == PO
                                DataRow[] foundRows = dt_QCFP.Select(expression);
                                DateTime dtPRDSDAT = DateTime.Today; //DateTime.ParseExact(PRDSDAT, "yyyyMMdd", new CultureInfo(""));
                                foreach (DataRow dr in foundRows)
                                {
                                    var ETA = dtReader["ETA"].ToString();
                                    DateTime dtETA = DateTime.ParseExact(ETA, "yyyyMMdd", new CultureInfo(""));
                                    //Debug.Print("Found " + foundRows.Length + " Row(s) From Sorted PP-BOM to Distribute with Req. Qty= " + dr["REQUESTQTY"] + " ; DO Qty =  " + DOQTY);
                                    //Reset
                                    //decAssignQty = 0;
                                    blIsInsert = true;
                                    if (DOQTY > 0)
                                    {
                                        decAssignQty = Convert.ToDouble(dr["REQUESTQTY"].ToString()) - Convert.ToDouble(dr["ASSIGNEDQTY"].ToString());
                                        if (decAssignQty > 0)
                                        {
                                            //#Noted: If T_QC_QCPM not Distrbute yet, create new records
                                            DataRow drNew_tmp_T_QC_QCPM = tmpdt_T_QC_QCPM.NewRow();
                                            intSeqNo = intSeqNo + 1;
                                            drNew_tmp_T_QC_QCPM["ID"] = dr["FACTORY"] + "-" + CommonMethod.GetRight("000000000000000" + intSeqNo, 15);
                                            drNew_tmp_T_QC_QCPM["RANGKING"] = 0; // dr["RANGKING"];
                                            drNew_tmp_T_QC_QCPM["FACTORY"] = dr["FACTORY"];
                                            drNew_tmp_T_QC_QCPM["LINENO"] = dr["LINENO"];
                                            drNew_tmp_T_QC_QCPM["AONO"] = dr["AONO"];
                                            drNew_tmp_T_QC_QCPM["STYLECODE"] = dr["STYLECODE"];
                                            drNew_tmp_T_QC_QCPM["STYLESIZE"] = dr["STYLESIZE"];
                                            drNew_tmp_T_QC_QCPM["STYLECOLORSERIAL"] = dr["STYLECOLORSERIAL"];
                                            drNew_tmp_T_QC_QCPM["REVNO"] = dr["REVNO"];
                                            drNew_tmp_T_QC_QCPM["PRDPKG"] = dr["PRDPKG"];
                                            drNew_tmp_T_QC_QCPM["MAINITEMCODE"] = dr["MAINITEMCODE"];
                                            drNew_tmp_T_QC_QCPM["MAINITEMCOLORSERIAL"] = dr["MAINITEMCOLORSERIAL"];
                                            drNew_tmp_T_QC_QCPM["ITEMCODE"] = dr["ITEMCODE"];
                                            drNew_tmp_T_QC_QCPM["ITEMCOLORSERIAL"] = dr["ITEMCOLORSERIAL"];
                                            drNew_tmp_T_QC_QCPM["REQUESTQTY"] = dr["REQUESTQTY"];
                                            if (DOQTY < decAssignQty)
                                            {
                                                decAssignQty = DOQTY;
                                            }
                                            /*
                                            * Nếu ETA nhỏ hơn ngày exec QCO Ranking (thường là thu7) 
                                            *      SHIP_QTY = SHIP_QTY * 100% >> Quantity_A
                                            * Nếu ETA trong 5 ngày của ngày exec QCO Ranking ; tức  Calc_Date  < ETA < Exec_Date + 5.Days
                                            *      SHIP_QTY = SHIP_QTY * 50%>> Quantity_B
                                            * Nếu ETA trong 10 ngày của ngày exec QCO Ranking ; tức  Calc_Date  < ETA < Exec_Date + 10.Days
                                            *      SHIP_QTY = SHIP_QTY * 30%>> Quantity_C
                                            * Nếu ETA > 10 ngày của ngày exec QCO Ranking ; tức  Calc_Date + 10.Days < ETA  
                                            *      SHIP_QTY = SHIP_QTY * 10%>> Quantity_D
                                            */
                                            if (!IsFactoryHasMaterialPara)
                                            {
                                                dr["ASSIGNEDQTY"] = Convert.ToDouble(dr["ASSIGNEDQTY"].ToString()) + decAssignQty;
                                                if (dtETA < dtPRDSDAT)
                                                {
                                                    drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty;
                                                    //2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column
                                                    drNew_tmp_T_QC_QCPM["QUANTITY_A"] = decAssignQty;
                                                    //Debug.Print("dtPRDSDAT > dtETA");
                                                }
                                                else if (dtPRDSDAT < dtETA && dtETA >= dtPRDSDAT.AddDays(5))
                                                {
                                                    drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty * 0.5;
                                                    //2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column
                                                    drNew_tmp_T_QC_QCPM["QUANTITY_B"] = decAssignQty * 0.5;
                                                    //Debug.Print("dtPRDSDAT < dtETA && dtETA >= dtPRDSDAT.AddDays(5)");
                                                }
                                                else if (dtPRDSDAT.AddDays(5) < dtETA && dtETA >= dtPRDSDAT.AddDays(10))
                                                {
                                                    drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty * 0.3;
                                                    //2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column
                                                    drNew_tmp_T_QC_QCPM["QUANTITY_C"] = decAssignQty * 0.3;
                                                    //Debug.Print("dtPRDSDAT.AddDays(5) < dtETA && dtETA >= dtPRDSDAT.AddDays(10)");
                                                }
                                                else
                                                {
                                                    drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty * 0.1;
                                                    //2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column
                                                    drNew_tmp_T_QC_QCPM["QUANTITY_D"] = decAssignQty * 0.1;
                                                    //Debug.Print("CASE : ELSE");
                                                }
                                            }
                                            else
                                            {
                                                switch (i)
                                                {
                                                    case 0:
                                                        if (dtETA < dtPRDSDAT)
                                                        {
                                                            dr["ASSIGNEDQTY"] = Convert.ToDouble(dr["ASSIGNEDQTY"].ToString()) + decAssignQty;
                                                            drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty;
                                                            //2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column
                                                            drNew_tmp_T_QC_QCPM["QUANTITY_A"] = decAssignQty;
                                                            //Debug.Print("dtPRDSDAT > dtETA");
                                                        }
                                                        break;
                                                    case 1:
                                                        if (dtETA < dtPRDSDAT)
                                                        {
                                                            decAssignQty = 0;
                                                            blIsInsert = false;
                                                        }
                                                        else if (dtPRDSDAT < dtETA && dtETA >= dtPRDSDAT.AddDays(5))
                                                        {
                                                            drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty * 0.5;
                                                            //2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column
                                                            drNew_tmp_T_QC_QCPM["QUANTITY_B"] = decAssignQty * 0.5;
                                                            //Debug.Print("dtPRDSDAT < dtETA && dtETA >= dtPRDSDAT.AddDays(5)");
                                                        }
                                                        else if (dtPRDSDAT.AddDays(5) < dtETA && dtETA >= dtPRDSDAT.AddDays(10))
                                                        {
                                                            drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty * 0.3;
                                                            //2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column
                                                            drNew_tmp_T_QC_QCPM["QUANTITY_C"] = decAssignQty * 0.3;
                                                            //Debug.Print("dtPRDSDAT.AddDays(5) < dtETA && dtETA >= dtPRDSDAT.AddDays(10)");
                                                        }
                                                        else
                                                        {
                                                            drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty * 0.1;
                                                            //2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column
                                                            drNew_tmp_T_QC_QCPM["QUANTITY_D"] = decAssignQty * 0.1;
                                                            //Debug.Print("CASE : ELSE");
                                                        }
                                                        dr["ASSIGNEDQTY"] = Convert.ToDouble(dr["ASSIGNEDQTY"].ToString()) + decAssignQty;
                                                        break;
                                                }
                                            }
                                            DOQTY = DOQTY - decAssignQty;
                                            if (blIsInsert)
                                                tmpdt_T_QC_QCPM.Rows.Add(drNew_tmp_T_QC_QCPM);
                                            //if (intSeqNo % 500 == 0)
                                            //Debug.Print("500 records INSERTED: " + DateTime.Now.ToString("s"));
                                        }
                                    }
                                }
                            }
                        }
                        //Debug.Print("Commit 'T_QC_QCPM' " + DateTime.Now.ToString("s"));
                        OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                        oracleDataAdapter.Update(tmpdt_T_QC_QCPM);
                        oracleCommandBuilder.Dispose();
                        tmpdt_T_QC_QCPM.Dispose();
                        oracleDataAdapter.Dispose();
                    }
                    dtReader.Dispose();
                    dt_QCFP.Dispose();
                    DateTime dtEnd = DateTime.Now;
                //Debug.Print("Commit 'T_QC_QCPM' Complete " + dtEnd.ToString("s"));
                //Debug.Print("TOTAL TIME: " + ":" + (dtEnd - dtStart).TotalMinutes.ToString("#,##0") + ":" + (dtEnd - dtStart).Seconds.ToString("#,##0"));
                HE_ShowQCPMCon:
                    //Debug.Print("Query 'T_QC_QCFP' to Bind JQGrid: START" + DateTime.Now.ToString("s"));
                    decimal totalPages = 0, totalRecords = 0;
                    int intRowPerPage = gridRequest.pageSize;
                    if (intRowPerPage <= 0)
                        intRowPerPage = 20;
                    strSQL = "SELECT " +
                             " ROW_NUMBER() OVER(ORDER BY RANGKING) AS SEQNO " +
                             " , T_QC_QCPM.* " +
                             " FROM PKMES.T_QC_QCPM " +
                             " WHERE T_QC_QCPM.FACTORY = :FACTORY ";
                    DataTable dt = new DataTable();
                    dt = OracleDbManager.Query(strSQL, new List<OracleParameter> { new OracleParameter("FACTORY", idFactory) }.ToArray());
                    if (dt != null)
                    {
                        if (dt.Rows.Count > 0)
                        {
                            totalRecords = dt.Rows.Count;
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
                                     " WHERE SEQNO >= " + StartingIndex + " " +
                                     " AND   SEQNO <= " + EndIndex +
                                     " ORDER BY SEQNO ";
                    dt = OracleDbManager.Query(strMainSQL, new List<OracleParameter> { new OracleParameter("FACTORY", idFactory) }.ToArray());
                    //Debug.Print("ShowQCPM(): COMPLETE " + DateTime.Now.ToString("s"));
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
        private void UpdateQCORanking_1(Qcops item)
        {
            //Function Retired: 2019-05-03
            return;
            //Update QCORANK to avoid the duplicated Primary Key on Table "T_QC_QUEUE"
            //using (OracleConnection OracleConn = new OracleConnection(OPS_Utils.ConstantGeneric.ConnectionStrMes))
            //{
            //    OracleConn.Open();
            //    var strSQL =
            //          " UPDATE PKMES.T_QC_QUEUE " +
            //          " SET CHANGEQCORANK = :QCORANKNEW " +
            //          " WHERE QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO AND QCORANK= :QCORANK ";
            //    OracleCommand oracleCommand = new OracleCommand(strSQL, OracleConn);
            //    oracleCommand.Parameters.Add("QCORANKNEW", item.MAXQCORANKING);
            //    oracleCommand.Parameters.Add("QCOFACTORY", item.QCOFACTORY);
            //    oracleCommand.Parameters.Add("QCOYEAR", item.QCOYEAR);
            //    oracleCommand.Parameters.Add("QCOWEEKNO", item.QCOWEEKNO);
            //    oracleCommand.Parameters.Add("QCORANK", item.QCORANK);
            //    oracleCommand.ExecuteNonQuery();
            //    OracleConn.Close();
            //    OracleConn.Dispose();
            //}
        }
        #endregion
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
    public class StyleCodeSummary
    {
        public string StyleCode { get; set; }
        public string StyleSize { get; set; }
        public string StyleColorSerial { get; set; }
        public string StyleColorWays { get; set; }
        public string StyleRevNo { get; set; }
        public string ImgLink { get; set; }
        public string BuyerStyleCode { get; set; }
        public string BuyerStyleName { get; set; }
        public decimal PlanQty { get; set; }
        public decimal PlanQtyBal { get; set; }
        public decimal OrdQty { get; set; }
        public string PRDPKG { get; set; }
        public decimal RemainQty { get; set; } //2019-12-13 Tai Le (Thomas)
        public StyleCodeSummary() { }
    }
}