using System;
using System.Data;
using System.Web.Mvc;

using MES.Models;
using MES.CommonClass;
using OPS_Utils;
using OPS_DAL.CuttingPlanEntity;
using OPS_DAL.CuttingPlanService; 

//3rd party
using Newtonsoft.Json;

namespace MES.Controllers
{
    [SessionTimeout]
    [AutologArribute]
    public class CuttingManagementController : Controller
    { 
        private readonly ICutTicketService _cutTicketService;
         
        public CuttingManagementController(ICutTicketService cutTicketService)
        {
            _cutTicketService = cutTicketService;
        }
         

        //[SysActionFilter(RoleID = "5**0;6**0")]
        [SysActionFilter(SystemID = "MES", MenuID = "CTP", Action = "")]
        public ActionResult CutPackageManagement()
        {
            return View();
        }


        //[SysActionFilter(RoleID = "5**0;6**0")]
        [SysActionFilter(SystemID = "MES", MenuID = "CTM", Action = "")]
        public ActionResult CutTicketManagement()
        {
            return View();
        }

        public ActionResult CutTicketEdit()
		{
            var ticket = _cutTicketService.FindIDAsync("P1B1");
            return View(ticket.Result); 
        }

        //[SysActionFilter(RoleID = "5**0;6**0")]
        [SysActionFilter(SystemID = "MES", MenuID = "CTR", Action = "")]
        public ActionResult CuttingReport()
        { 
            return View();
        }



#region Test
        //[SysActionFilter(SystemID = "MES", MenuID = "CTR", Action = "")]
        public ActionResult Test()
        {
            return View();
        }
        public string TestGrid(GridSettings gridRequest)
        {
            try
            {
                string strSQL = "";
                string strSQLWhere1 = " T_MX_MPDT.PLNSTARTDATE >='20191125' and T_MX_MPDT.PLNSTARTDATE <='20191130' ";
                string strSQLWhere2 = " PRDSDAT >='20191125' and PRDSDAT <='20191130' ";
                //string strSQLOrderBy = " ORDER BY T_MX_MPDT.PACKAGEGROUP , T_MX_MPDT.MXPACKAGE ";
                
                /**strSQL =
                //    $" SELECT @rownum := @rownum + 1 AS RANKING , MPDTCount.MESPCount , " +
                //    $" PPKG.LISTMTOPSPP, CONCAT(T_MX_MPMT.PACKAGEGROUP , ': Qty= ', T_MX_MPMT.TARGETQTY) PACKAGEGROUP , T_MX_MPMT.STYLECODE , T_MX_MPMT.STYLESIZE , T_MX_MPMT.STYLECOLORSERIAL , T_MX_MPMT.REVNO , " +
                //    $" T_MX_MPDT.MXPACKAGE ,  STR_TO_DATE(T_MX_MPDT.PLNSTARTDATE, '%Y%m%d') PLNSTARTDATE , T_MX_MPDT.MXTARGET , T_MX_MPDT.FACTORY , T_MX_MPDT.LINESERIAL " +
                //    $" FROM T_MX_MPDT " +
                //    $" JOIN T_MX_MPMT ON T_MX_MPDT.PACKAGEGROUP = T_MX_MPMT.PACKAGEGROUP " +
                //    $" JOIN ( " +
                //    $"      SELECT PACKAGEGROUP, " +
                //    $"          GROUP_CONCAT(DISTINCT CONCAT(PPACKAGE , ': Qty= ', PLANQTY) ORDER BY PPACKAGE DESC SEPARATOR '<BR/>') AS LISTMTOPSPP " +
                //    $"      FROM T_MX_PPKG GROUP BY PACKAGEGROUP ) " +
                //    $" PPKG ON T_MX_MPMT.PACKAGEGROUP = PPKG.PACKAGEGROUP " +
                //    $" JOIN ( " +
                //    $"      SELECT T_MX_MPDT.PACKAGEGROUP, COUNT(*) AS MESPCOUNT " +
                //    $"      FROM T_MX_MPDT " +
                //    $"      WHERE 1=1 AND {strSQLWhere} " +
                //    $"      GROUP BY  T_MX_MPDT.PACKAGEGROUP ) " +
                //    $" MPDTCount on T_MX_MPDT.PACKAGEGROUP = MPDTCount.PACKAGEGROUP" +
                //    $" , (SELECT @rownum := 0) r ";
                */

                strSQL =
                    $" SELECT ROW_NUMBER() OVER(ORDER BY RowPriority , ORIPACKAGEGROUP , MXPACKAGE ) as RANKING , " +
                    $" MAIN.* , T_SD_BOMT.MainItemCode , T_SD_BOMT.MainItemColorSerial , T_SD_BOMT.ItemCode , T_SD_BOMT.ItemColorSerial , T_SD_BOMT.UNITCONSUMPTION ,  T_SD_BOMT.UNITCONSUMPTION * Main.MXTARGET as TotalCONSUMPTION " +
                    $" FROM " +
                    $" ( " +
                    $" SELECT 0 as RowPriority , " +
                    $" MPDTCount.MESPCount , " +
                    $" PPKG.LISTMTOPSPP, T_MX_MPMT.PACKAGEGROUP as ORIPACKAGEGROUP, T_MX_MPMT.PACKAGEGROUP || ': Qty= ' || T_MX_MPMT.TARGETQTY as PACKAGEGROUP , T_MX_MPMT.STYLECODE , T_MX_MPMT.STYLESIZE , T_MX_MPMT.STYLECOLORSERIAL , T_MX_MPMT.REVNO , " +
                    $" T_MX_MPDT.MXPACKAGE ,  TO_DATE(T_MX_MPDT.PLNSTARTDATE, 'yyyymmdd') PLNSTARTDATE , T_MX_MPDT.MXTARGET , T_MX_MPDT.FACTORY ,  TO_CHAR(T_MX_MPDT.LINESERIAL ) LINESERIAL " +
                    $" FROM T_MX_MPDT " +
                    $" JOIN T_MX_MPMT ON T_MX_MPDT.PACKAGEGROUP = T_MX_MPMT.PACKAGEGROUP " +
                    $" JOIN ( " +
                    $"      SELECT PACKAGEGROUP, " +
                    $"          LISTAGG(PPACKAGE ||  ': Qty= ' || PLANQTY, '<BR/>') WITHIN GROUP ( ORDER BY PPACKAGE,PLANQTY) AS LISTMTOPSPP  " +
                    $"      FROM T_MX_PPKG " +
                    $"      GROUP BY PACKAGEGROUP " +
                    $" ) PPKG ON T_MX_MPMT.PACKAGEGROUP = PPKG.PACKAGEGROUP " +
                    $" JOIN ( " +
                    $"      SELECT T_MX_MPDT.PACKAGEGROUP, COUNT(*) AS MESPCOUNT " +
                    $"      FROM T_MX_MPDT " +
                    $"      WHERE 1=1 AND FACTORY = 'P2B1' AND {strSQLWhere1} " +
                    $"      GROUP BY  T_MX_MPDT.PACKAGEGROUP ) " +
                    $" MPDTCount on T_MX_MPDT.PACKAGEGROUP = MPDTCount.PACKAGEGROUP" +
                    $" UNION " +
                    $" SELECT  1 as RowPriority , 1 MESPCount , " +
                    $" PRDPKG as LISTMTOPSPP, PRDPKG as ORIPACKAGEGROUP ,PRDPKG || ': Qty= ' || PLANQTY as PACKAGEGROUP , STYLECODE , STYLESIZE , STYLECOLORSERIAL , REVNO , " +
                    $" PRDPKG as MXPACKAGE , TO_DATE(PRDSDAT, 'yyyymmdd') PLNSTARTDATE , PLANQTY as MXTARGET , FACTORY , LINENO as LINESERIAL " +
                    $" FROM PKERP.VIEW_ERP_PSRSNP_PLAN " +
                    $" WHERE FACCLOSE = 'N' AND {strSQLWhere2} " +
                    $" AND FACTORY = 'P2B1' " +
                    $" ) MAIN " +
                    $" LEFT JOIN PKERP.T_SD_BOMT ON " +
                    $" Main.StyleCode = T_SD_BOMT.StyleCode " +
                    $" And Main.StyleSize =  T_SD_BOMT.StyleSize " +
                    $" And Main.StyleColorSerial = T_SD_BOMT.StyleColorSerial  " +
                    $" And Main.RevNo = T_SD_BOMT.RevNo  ";

                //var _GridResult = GridData.GetGridResultMySQL(ConstantGeneric.ConnectionStrMesMySql, strSQL, strSQLWhere, strSQLOrderBy, gridRequest, "dd MMM, yyyy");
                var _GridResult = GridData.GetGridResultOracle(ConstantGeneric.ConnectionStrMes, strSQL, "", gridRequest, "dd MMM, yyyy");

                //Add New Column 
                DataColumn newColumn = new DataColumn("STARTCOLUMNSPAN", typeof(System.String)) { DefaultValue = "N" };
                _GridResult.rows.Columns.Add(newColumn);

                string PrePACKAGEGROUP = "", CurrPACKAGEGROUP = "";
                foreach (DataRow dr in _GridResult.rows.Rows)
                {
                    CurrPACKAGEGROUP = dr["PACKAGEGROUP"].ToString();

                    if (PrePACKAGEGROUP != CurrPACKAGEGROUP)
                        dr["STARTCOLUMNSPAN"] = "NewGroup";
                    else
                        dr["STARTCOLUMNSPAN"] = "SameGroup";

                    PrePACKAGEGROUP = CurrPACKAGEGROUP;
                }


                var _GridDataResult = new GridDataResult
                {
                    page = gridRequest.pageIndex,
                    records = gridRequest.pageSize,
                    total = 0,
                    rows = _GridResult.rows
                };

                var _dtTest = _GridResult.rows;

                return JsonConvert.SerializeObject(_GridResult);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { retResult = false, dataRow = ex.Message });
            }
        }
#endregion
         
        
    }
}