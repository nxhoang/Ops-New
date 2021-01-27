using System; 
using System.Web.Mvc; 

using MES.Models; 
using OPS_Utils;

using Newtonsoft.Json;
using MES.CommonClass;

namespace MES.Controllers
{
    [SessionTimeout]
	[AutologArribute]
    public class WHTransactionController : Controller
    {
        // GET: WHTransaction
        public ActionResult SuperMarket()
        {
            ViewBag.PageTitle = "<i class=\"fa fa-home\"></i>&nbsp;WMS";
            ViewBag.SubPageTitle = "&nbsp;<span>> Super Market</span>";

            return View();
        }


        public string RealTimeStockGrid(GridSettings gridRequest)
        { 
            try
            {
                string strSQL = "";

                strSQL =
                    " SELECT ROW_NUMBER() OVER( ORDER BY T_WH_TPRS.WAREHOUSE, T_WH_TPRS.AONO , T_WH_TPRS.STYLECODE , T_WH_TPRS.STYLECOLORSERIAL , T_WH_TPRS.ITEMCODE , T_WH_TPRS.ITEMCOLORSERIAL , T_WH_TPRS.PIECEUNIQUE ) AS RANKING , " +
                    " T_00_STMT.STYLENAME , T_00_STMT.BUYERSTYLECODE , T_00_STMT.BUYERSTYLENAME , T_00_SCMT.STYLECOLORWAYS , " +
                    " T_00_ICMT.ITEMNAME , T_00_ICCM.ITEMCOLORWAYS , " +
                    " T_WH_TPRS.* " + 
                    " FROM PKMES.T_WH_TPRS " +
                    "   INNER JOIN PKERP.T_00_SCMT  ON " +
                    "       T_WH_TPRS.STYLECODE = T_00_SCMT.STYLECODE " +
                    "       AND T_WH_TPRS.STYLECOLORSERIAL = T_00_SCMT.STYLECOLORSERIAL " +
                    "   INNER JOIN PKERP.T_00_STMT  ON " +
                    "       T_WH_TPRS.STYLECODE = T_00_STMT.STYLECODE " +
                    "   INNER JOIN PKERP.T_00_ICCM  ON " +
                    "       T_WH_TPRS.ITEMCODE = T_00_ICCM.ITEMCODE " +
                    "       AND T_WH_TPRS.ITEMCOLORSERIAL = T_00_ICCM.ITEMCOLORSERIAL " +
                    "   INNER JOIN PKERP.T_00_ICMT  ON " +
                    "       T_WH_TPRS.ITEMCODE = T_00_ICMT.ITEMCODE " +
                    "";
                 
                var _Result = GridData.GetGridData(ConstantGeneric.ConnectionStr, strSQL, "", gridRequest);
                return _Result; 
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { retResult = false, dataRow = ex.Message });
            }
        }

    }
}