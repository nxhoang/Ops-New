using System;
using System.Globalization;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Serilog;
using System.Threading.Tasks;
using System.Linq;
using Dapper;
namespace PKQCO
{
    /* Standardalone QCO */
    public class PCMQCOCalculation : IPCMQCOCalculation
    {
        //public static StreamWriter log;
        public string mFactory { get; set; }
        public int mYear { get; set; }
        public string mWeekNo { get; set; }
        public string mEnviroment { get; set; }
        public string mUserID { get; set; }
        public string mRoleID { get; set; }
        public string mQCOSource { get; set; }
        //Class Global Variable.
        public int intNegativeRank;
        public string strQCOVersion = "";
        public string strTableName = "";
        public string strPath = "";
        public string TeleTokenID = "1031789091:AAElsz7zm8vSdvdIJqzA07Q_Pvxfs-1Ikag";
        //2020-03-11 Tai Le(Thomas)
        public readonly string _PKERPConnString = "";
        public readonly string _PKMESConnString = "";
        public PCMQCOCalculation(string PKERPConnString, string PKMESConnString)
        {
            strQCOVersion = this.GenerateQCOVersion();
            //2020-03-11 Tai Le(Thomas)
            _PKERPConnString = PKERPConnString;
            _PKMESConnString = PKMESConnString;
        }
        public PCMQCOCalculation(string Factory)
        {
            strQCOVersion = this.GenerateQCOVersion();
            mFactory = Factory;
            mEnviroment = "";
        }

        #region  QCO Related Methods
        private string GetYearMaxWeekNum(string pQCOFactory, int pYear)
        {
            string strWeekNo = "";
            var strSQL = $"Select MAX(QCOWEEKNO) From PKMES.T_QC_QUEUE Where QCOYEAR = {pYear} And QCOFACTORY = '{pQCOFactory}' ";
            using (OracleConnection oracleCnn = new OracleConnection(_PKMESConnString))
            {
                oracleCnn.Open();
                DataTable dt = new DataTable();
                PCMOracleLibrary.StatementToDataTable(oracleCnn, strSQL, null, out dt, out strSQL, 60);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        if (dt.Rows[0][0] != null)
                            strWeekNo = dt.Rows[0][0].ToString();
                    }
                    dt.Dispose();
                }
                oracleCnn.Close();
            }
            return strWeekNo;
        }
        public bool RemoveDuplicateRowQCFP(ref DataTable pDt_QCFP)
        {
            try
            {
                DataRow[] foundRows = pDt_QCFP.Select("MATNORNALRATE = -1 ");
                if (foundRows.Length > 0)
                {
                    foreach (DataRow dr in foundRows)
                    {
                        pDt_QCFP.Rows.Remove(dr);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return false;
            }
        }
        public bool UpdateMaterialRateT_QC_QCFP(ref DataTable pDt_QCFP, string pFactory, int pQCOYear, string pWeekNum)
        {
            try
            {
                var strSQLMaterialRating =
                    $@"
SELECT
	QCOFactory, QCOYear, QCOWEEKNO, PRDPKG ,
	TO_NUMBER( TO_CHAR(SUM( CASE WHEN MATRating = '3' THEN MATCOUNTERS / MATRatingC ELSE 0 END ) * 100 , '999.99' ))  AS LEV3 ,
	TO_NUMBER( TO_CHAR(SUM( CASE WHEN MATRating = '2' THEN MATCOUNTERS / MATRatingC ELSE 0 END ) * 100 , '999.99' ))  AS LEV2 ,
	TO_NUMBER( TO_CHAR(SUM( CASE WHEN MATRating = '1' THEN MATCOUNTERS / MATRatingC ELSE 0 END ) * 100 , '999.99' ))  AS LEV1
FROM
	(
	SELECT
		Main.QCOWEEKNO,
		Main.QCOFactory,
		Main.QCOYear,
		Main.PRDPKG ,
		MATRating ,
		COUNT(MATRating) MATRatingC ,
		SUM(MATCOUNTER) MATCOUNTERS
	FROM
		(
		SELECT
			T_00_ICMT.MainLevel ,
			NVL(T_SD_BOMT.PRIORITY , NVL(T_00_ICMT.PRIORITY , NVL(T_00_ILHM.PRIORITY,  '1')  )  ) as MATRating,
			CASE
				WHEN QUANTITY_A = REQUESTQTY THEN 1
				ELSE 0
			END AS MATCOUNTER ,
			T_QC_QCPM.*
		FROM
			PKMES.T_QC_QCPM
		INNER JOIN PKERP.T_00_ICMT ON
			T_QC_QCPM.ITEMCODE = T_00_ICMT.ITEMCODE 
		INNER JOIN PKERP.T_00_ILHM ON 
			T_00_ICMT.MAINLEVEL  = T_00_ILHM.MAINLEVEL
		LEFT JOIN PKERP.T_SD_BOMT ON 
			T_QC_QCPM.StyleCode = T_SD_BOMT.StyleCode 
			And T_QC_QCPM.StyleSize= T_SD_BOMT.StyleSize 
			And T_QC_QCPM.StyleColorSerial = T_SD_BOMT.StyleColorSerial 
			And T_QC_QCPM.RevNo = T_SD_BOMT.RevNo 
			And T_QC_QCPM.MAINITEMCODE = T_SD_BOMT.MAINITEMCODE 
			And T_QC_QCPM.MAINITEMCOLORSERIAL = T_SD_BOMT.MAINITEMCOLORSERIAL 
			And T_QC_QCPM.ITEMCODE = T_SD_BOMT.ITEMCODE 
			And T_QC_QCPM.ITEMCOLORSERIAL = T_SD_BOMT.ITEMCOLORSERIAL 
		WHERE QCOFACTORY = '{pFactory}' AND  QCOYEAR = {pQCOYear} AND QCOWEEKNO = '{pWeekNum}'
		  ) MAIN
	GROUP BY Main.PRDPKG , MATRating , QCOFactory, QCOYear, QCOWEEKNO
  ) MAINM
GROUP BY QCOFactory, QCOYear, QCOWEEKNO, PRDPKG 
";
                using (OracleConnection oracleCnn = new OracleConnection(_PKMESConnString))
                {
                    oracleCnn.Open();
                    using (OracleCommand cmd = new OracleCommand(strSQLMaterialRating, oracleCnn))
                    {
                        cmd.CommandTimeout = 90;
                        var dtReader = cmd.ExecuteReader();
                        if (dtReader.HasRows)
                            while (dtReader.Read())
                            {
                                var PRDPKG = dtReader["PRDPKG"].ToString();
                                DataRow[] foundRows = pDt_QCFP.Select($"PRDPKG = '{PRDPKG}' ");
                                if (foundRows.Length > 0)
                                {
                                    foreach (DataRow dr in foundRows)
                                    {
                                        dr["MATPRIORITYLEV3"] = dtReader["LEV3"];
                                        dr["MATPRIORITYLEV2"] = dtReader["LEV2"];
                                        dr["MATPRIORITYLEV1"] = dtReader["LEV1"];
                                    }
                                }
                            }
                    }
                    oracleCnn.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return false;
            }
        }
        public string DistributeMaterial(bool IsSinglePP, string pOracleCnnString, string pFactory, int pQCOYear, string pWeekNum, ref DataTable dt_T_QC_QCPM, ref DataTable dt_QCFP)
        {
            string msg = "";
            try
            {
                StringBuilder sb = new StringBuilder();
                /* 2019-11-15  Main Distribute Material  */
                using (OracleConnection oracleConnection = new OracleConnection(pOracleCnnString))
                {
                    oracleConnection.Open();
                    int intSeqNo = 0;
                    //::: Distribute WMS Qty (Received Material Qty) 
                    //::: WMS based on WONO (MRP_OLD data)
                    string strSQL =
                        " SELECT * " +
                        " FROM PKERP.V_WO_RECWMS " +
                        " WHERE " +
                        " (WO Not Like 'AD%' ) AND ( WO IN " +
                        "   (Select V_MRP_PP_WO.WONO  " +
                        "    From PKMES.T_QC_QCFP " +
                        "    Inner Join PKMES.V_MRP_PP_WO On  " +
                        "       T_QC_QCFP.FACTORY = V_MRP_PP_WO.FACTORY " +
                        "       AND T_QC_QCFP.AONO = V_MRP_PP_WO.AONO " +
                        "       AND T_QC_QCFP.STYLECODE = V_MRP_PP_WO.STLCD " +
                        "       AND T_QC_QCFP.STYLESIZE = V_MRP_PP_WO.STLSIZ " +
                        "       AND T_QC_QCFP.STYLECOLORSERIAL = V_MRP_PP_WO.STLCOSN " +
                        "       AND T_QC_QCFP.REVNO = V_MRP_PP_WO.STLREVN " +
                        "       AND T_QC_QCFP.PRDPKG = V_MRP_PP_WO.PRODPACKAGE " +
                        "    WHERE T_QC_QCFP.QCOFACTORY = '" + pFactory + "' AND T_QC_QCFP.QCOYEAR = " + pQCOYear + " AND T_QC_QCFP.QCOWEEKNO = '" + pWeekNum + "' " +
                        "    Group By V_MRP_PP_WO.WONO) " +
                        " ) " +
                        " ORDER BY WO , ITEM_CD , COLOR_SERIAL , PLAN_DOQTY ";
                    OracleCommand command = new OracleCommand(strSQL, oracleConnection);
                    var dr_WMS = command.ExecuteReader();
                    DistributeMaterial_T_QC_QCPM(pFactory, pQCOYear, pWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, dr_WMS, "W", ref intSeqNo);
                    dr_WMS.Close();
                    dr_WMS.Dispose();
                    command.Dispose();
                    if (mEnviroment == "Console")
                        Console.WriteLine("DistributeMaterial_T_QC_QCPM() for W: PASSED");
                    sb.AppendLine($"DistributeMaterial_T_QC_QCPM() for W: PASSED");
                    //::: Distribute WMS Qty (Received Material Qty) 
                    //::: WMS based on AONO (MRP2 data)
                    strSQL =
                        " SELECT * " +
                        " FROM PKERP.V_WO_RECWMS " +
                        " WHERE " +
                        " WO LIKE 'AD%' " +
                        " AND WO IN (Select AONO From PKMES.T_QC_QCFP  Where T_QC_QCFP.QCOFACTORY = '" + pFactory + "' AND T_QC_QCFP.QCOYEAR = " + pQCOYear + " AND T_QC_QCFP.QCOWEEKNO = '" + pWeekNum + "' Group By AONO) " +
                        " ORDER BY WO , ITEM_CD , COLOR_SERIAL , PLAN_DOQTY ";
                    command = new OracleCommand(strSQL, oracleConnection);
                    var dr_WMS2 = command.ExecuteReader();
                    DistributeMaterial_T_QC_QCPM(pFactory, pQCOYear, pWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, dr_WMS2, "W2", ref intSeqNo);
                    dr_WMS.Close();
                    dr_WMS.Dispose();
                    command.Dispose();
                    if (mEnviroment == "Console")
                        Console.WriteLine("DistributeMaterial_T_QC_QCPM() for W2: PASSED");
                    sb.AppendLine($"DistributeMaterial_T_QC_QCPM() for W2: PASSED");
                    //::: Distribute KMS Qty (Incoming Qty)
                    //2019-06-15:  Get the Monday based on Year and WeekNo 
                    int weekNum = 0;
                    weekNum = Convert.ToInt32(pWeekNum.Replace("W", ""));
                    if (IsSinglePP)
                    {
                        CultureInfo cul = CultureInfo.CurrentCulture;
                        weekNum = cul.Calendar.GetWeekOfYear(DateTime.Today, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                    }
                    DateTime dtMonday = PCMGeneralFunctions.GetDateFromWeekNumberAndDayOfWeek(pQCOYear, weekNum, 0);
                    strSQL = " SELECT WO , ITEM_CD , COLOR_SERIAL , ETA , SUM(SHIP_QTY) PLAN_DOQTY " +
                             " FROM KMS_PSRSHP_TBL@AOMTOPS " +
                             " WHERE DELFLG = 'N' " +
                             " AND ETA IS NOT NULL " +
                             " AND Length(ETA) = 8 " +
                             " AND ETA >= '" + dtMonday.ToString("yyyyMMdd") + "' " +
                             " GROUP BY WO , ITEM_CD , COLOR_SERIAL , ETA  ";
                    command = new OracleCommand(strSQL, oracleConnection);
                    var dr_KMS = command.ExecuteReader();
                    DistributeMaterial_T_QC_QCPM(pFactory, pQCOYear, pWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, dr_KMS, "K", ref intSeqNo);
                    dr_KMS.Close();
                    dr_KMS.Dispose();
                    command.Dispose();
                    if (!String.IsNullOrEmpty(mEnviroment))
                        if (mEnviroment.ToLower() == "console")
                            Console.WriteLine("DistributeMaterial_T_QC_QCPM for K: PASSED");
                    sb.AppendLine($"DistributeMaterial_T_QC_QCPM() for K: PASSED");
                    //::: Distribute KMS Qty (Received Material Qty) 
                    /*2019-11-01 Tai Le (Thomas) add part of KMS from MRP2*/
                    strSQL = " SELECT PRDPKG , ITEM_CD , COLOR_SERIAL , ETA , SUM(ORD_CNF_QTY) PLAN_DOQTY " +
                             " FROM KMS_PSRSHP2_TBL@AOMTOPS " +
                             " WHERE ETA IS NOT NULL " +
                             " AND Length(ETA) = 8 " +
                             " AND ETA >= '" + dtMonday.ToString("yyyyMMdd") + "' " +
                             " GROUP BY PRDPKG , ITEM_CD , COLOR_SERIAL , ETA ";
                    command = new OracleCommand(strSQL, oracleConnection);
                    var dr_KMS2 = command.ExecuteReader();
                    DistributeMaterial_T_QC_QCPM(pFactory, pQCOYear, pWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, dr_KMS2, "K2", ref intSeqNo);
                    dr_KMS2.Close();
                    dr_KMS2.Dispose();
                    command.Dispose();
                    if (!String.IsNullOrEmpty(mEnviroment))
                        if (mEnviroment.ToLower() == "console")
                            Console.WriteLine("DistributeMaterial_T_QC_QCPM for K2: PASSED");
                    sb.AppendLine($"DistributeMaterial_T_QC_QCPM() for K2: PASSED");
                    /*::END  2019-11-01 Tai Le (Thomas) add part of KMS from MRP2*/
                    oracleConnection.Close();
                }
                msg = sb.ToString();
                sb.Clear();
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return msg;
        }
        public void DistributeMaterial_T_QC_QCPM(string vstrFactory, int vintQCOYear, string vstrWeekNo, ref DataTable vdtT_QC_QCPM, ref DataTable vdt_T_QC_QCFP, OracleDataReader vDrMaterialSource, string vType, ref int vintSeqNo)
        {
            string ETA = "";
            try
            {
                /* vType 
					 *  - "W" stand for WMS >> it's Received Qty
					 *  - "K" stand for KMS >> it's ETA Qty
					 */
                if (vDrMaterialSource.HasRows)
                {
                    double decAssignQty = 0;
                    bool blInsert = true;
                    bool isETA_Issue = false;
                    while (vDrMaterialSource.Read())
                    {
                        isETA_Issue = false;
                        var DOQTY = Convert.ToDouble(vDrMaterialSource["PLAN_DOQTY"].ToString());
                        if (DOQTY <= 0)
                            continue;
                        //2019-11-16 
                        DateTime dtPRDSDAT = DateTime.MinValue; //DateTime.ParseExact(PRDSDAT, "yyyyMMdd", new CultureInfo(""));
                        DateTime dtETA = dtPRDSDAT;
                        if (vType == "K" || vType == "K2")
                        {
                            ETA = vDrMaterialSource["ETA"].ToString().Trim();
                            isETA_Issue = !DateTime.TryParseExact(ETA, "yyyyMMdd", new CultureInfo(""), DateTimeStyles.None, out dtETA);
                        }
                        if (isETA_Issue == false)
                        {
                            //string expression = " WONO = '" + vDrMaterialSource["WO"] + "' " +
                            //                    " AND ITEMCODE = '" + vDrMaterialSource["ITEM_CD"] + "'  " +
                            //                    " AND ITEMCOLORSERIAL = '" + vDrMaterialSource["COLOR_SERIAL"] + "' "; 
                            //if (vType == "K2")
                            //{
                            //    expression = " PRDPKG = '" + vDrMaterialSource["PRDPKG"] + "' " +
                            //                 " AND ITEMCODE = '" + vDrMaterialSource["ITEM_CD"] + "'  " +
                            //                 " AND ITEMCOLORSERIAL = '" + vDrMaterialSource["COLOR_SERIAL"] + "' ";
                            //} 
                            string expression = "";
                            if (vType == "K2")
                            {
                                expression = " PRDPKG = '" + vDrMaterialSource["PRDPKG"] + "' " +
                                             " AND ITEMCODE = '" + vDrMaterialSource["ITEM_CD"] + "'  " +
                                             " AND ITEMCOLORSERIAL = '" + vDrMaterialSource["COLOR_SERIAL"] + "' ";
                            }
                            else if (vType == "W2")
                            {
                                expression = " ( AONO = '" + vDrMaterialSource["WO"] + "' ) " +
                                             " AND ( ITEMCODE = '" + vDrMaterialSource["ITEM_CD"] + "' ) " +
                                             " AND ( ITEMCOLORSERIAL = '" + vDrMaterialSource["COLOR_SERIAL"] + "' ) ";
                            }
                            else
                                expression = " ( WONO = '" + vDrMaterialSource["WO"] + "' ) " +
                                             " AND ( ITEMCODE = '" + vDrMaterialSource["ITEM_CD"] + "' ) " +
                                             " AND ( ITEMCOLORSERIAL = '" + vDrMaterialSource["COLOR_SERIAL"] + "' ) ";
                            DataRow[] foundRows = vdt_T_QC_QCFP.Select(expression);
                            foreach (DataRow dr in foundRows)
                            {
                                if (DOQTY > 0)
                                {
                                    decAssignQty = Convert.ToDouble(dr["REQUESTQTY"].ToString()) - Convert.ToDouble(dr["ASSIGNEDQTY"].ToString());
                                    if (decAssignQty > 0)
                                    {
                                        //#Noted: If T_QC_QCPM not Distrbute yet, create new records
                                        DataRow drNew_tmp_T_QC_QCPM = vdtT_QC_QCPM.NewRow();
                                        vintSeqNo = vintSeqNo + 1;
                                        drNew_tmp_T_QC_QCPM["ID"] = dr["ID"]; //; dr["FACTORY"] + "-" + PCMGeneralFunctions.GetRight("0000000000000000" + vintSeqNo, 15);
                                                                              //drNew_tmp_T_QC_QCPM["RANGKING"] = intSeqNo; // dr["RANGKING"];
                                        drNew_tmp_T_QC_QCPM["FACTORY"] = dr["FACTORY"];
                                        //2019-02-28
                                        drNew_tmp_T_QC_QCPM["QCOFACTORY"] = dr["FACTORY"];
                                        drNew_tmp_T_QC_QCPM["QCOYEAR"] = vintQCOYear;
                                        drNew_tmp_T_QC_QCPM["QCOWEEKNO"] = vstrWeekNo;
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
                                        //2019-11-01 Tai Le (Thomas)
                                        drNew_tmp_T_QC_QCPM["SOURCEDATA"] = vType;
                                        drNew_tmp_T_QC_QCPM["QUANTITY_A"] = 0;
                                        drNew_tmp_T_QC_QCPM["QUANTITY_B"] = 0;
                                        drNew_tmp_T_QC_QCPM["QUANTITY_C"] = 0;
                                        drNew_tmp_T_QC_QCPM["QUANTITY_D"] = 0;
                                        //:: END    2019-11-01 Tai Le (Thomas)
                                        dr["ASSIGNEDQTY"] = Convert.ToDouble(dr["ASSIGNEDQTY"].ToString()) + decAssignQty;
                                        if (vType == "W" || vType == "W2")
                                        {
                                            /* When vType = "W" >> it means the RECEIVED QTY
											 *::: ONLY Data for QUANTITY_A   INCLUDED
											 */
                                            //dr["ASSIGNEDQTY"] = Convert.ToDouble(dr["ASSIGNEDQTY"].ToString()) + decAssignQty;
                                            drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty;
                                            drNew_tmp_T_QC_QCPM["QUANTITY_A"] = decAssignQty;
                                            blInsert = true;
                                        }
                                        else if (vType == "K" || vType == "K2")
                                        {
                                            /* When vType = "K" or "K2" >> it means the INCOMING QTY
											 *::: ONLY Data for QUANTITY_A   EXCLUDED
											 * "K"  >> MRP (old)
											 * "K2" >> MRP-2
											 */
                                            blInsert = false;
                                            /* 
											* Nếu ETA trong 5 ngày của ngày exec QCO Ranking ; tức  Calc_Date  < ETA < Exec_Date + 5.Days
											*      SHIP_QTY = SHIP_QTY * 50%>> Quantity_B
											* Nếu ETA trong 10 ngày của ngày exec QCO Ranking ; tức  Calc_Date  < ETA < Exec_Date + 10.Days
											*      SHIP_QTY = SHIP_QTY * 30%>> Quantity_C
											* Nếu ETA > 10 ngày của ngày exec QCO Ranking ; tức  Calc_Date + 10.Days < ETA  
											*      SHIP_QTY = SHIP_QTY * 10%>> Quantity_D
											*/
                                            //dr["ASSIGNEDQTY"] = Convert.ToDouble(dr["ASSIGNEDQTY"].ToString()) + decAssignQty;
                                            if (dtETA < dtPRDSDAT)
                                            {
                                                drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = 0;
                                                drNew_tmp_T_QC_QCPM["QUANTITY_A"] = 0;
                                                blInsert = true;
                                            }
                                            else if (dtPRDSDAT < dtETA && dtETA <= dtPRDSDAT.AddDays(5))
                                            {
                                                drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty * 0.5;
                                                //2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column
                                                drNew_tmp_T_QC_QCPM["QUANTITY_B"] = decAssignQty * 0.5;
                                                blInsert = true;
                                            }
                                            else if (dtPRDSDAT.AddDays(5) < dtETA && dtETA <= dtPRDSDAT.AddDays(10))
                                            {
                                                drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty * 0.3;
                                                //2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column
                                                drNew_tmp_T_QC_QCPM["QUANTITY_C"] = decAssignQty * 0.3;
                                                blInsert = true;
                                            }
                                            else
                                            {
                                                drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty * 0.1;
                                                //2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column
                                                drNew_tmp_T_QC_QCPM["QUANTITY_D"] = decAssignQty * 0.1;
                                                blInsert = true;
                                            }
                                        }
                                        else
                                        {
                                            blInsert = false;
                                        }
                                        DOQTY = DOQTY - decAssignQty;
                                        if (blInsert)
                                            vdtT_QC_QCPM.Rows.Add(drNew_tmp_T_QC_QCPM);
                                        if (vintSeqNo % 500 == 0)
                                        {
                                            Debug.Print("Finished " + vintSeqNo + " Material Rows T_QC_QCPM");
                                            if (mEnviroment.ToLower() == "console")
                                                Console.WriteLine("Finished " + vintSeqNo + " Material Rows T_QC_QCPM");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                if (!String.IsNullOrEmpty(mEnviroment))
                    if (mEnviroment.ToLower() == "console")
                        Console.WriteLine("ERROR at DistributeMaterial_T_QC_QCPM(): " + Msg + "; Type= " + vType + ", ETA (string)=" + ETA);
            }
        }
        public async Task<bool> DeleteQCOData(string vstrQCOFactory, int vintQCOYear, string vstrWeekNo, bool IsSinglePP)
        {
            try
            {
                //Clean the temporary physical Tables { PKMES.T_QC_QCFP ; PKMES.T_QC_QCPM ; PKMES.T_QC_QCPS ; PKMES.T_QC_QUEUE } 
                using (OracleConnection oracleConnection = new OracleConnection(_PKMESConnString))
                {
                    oracleConnection.Open();
                    //T_QC_QCFP : Original MTOPS Packages
                    OracleCommand command = new OracleCommand("DELETE PKMES.T_QC_QCFP WHERE QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO", oracleConnection);
                    command.Parameters.Add(new OracleParameter("QCOFACTORY", vstrQCOFactory));
                    command.Parameters.Add(new OracleParameter("QCOYEAR", vintQCOYear));
                    command.Parameters.Add(new OracleParameter("QCOWEEKNO", vstrWeekNo));
                    if (IsSinglePP == false)
                    {
                        //command.ExecuteNonQuery();
                        await command.ExecuteNonQueryAsync();
                    }
                    command.Dispose();
                    //T_QC_QCPM : QCO Material Distribution
                    command = new OracleCommand("DELETE PKMES.T_QC_QCPM WHERE QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO ", oracleConnection);
                    command.Parameters.Add(new OracleParameter("QCOFACTORY", vstrQCOFactory));
                    command.Parameters.Add(new OracleParameter("QCOYEAR", vintQCOYear));
                    command.Parameters.Add(new OracleParameter("QCOWEEKNO", vstrWeekNo));
                    if (IsSinglePP == false)
                    {
                        //command.ExecuteNonQuery();
                        await command.ExecuteNonQueryAsync();
                    }
                    command.Dispose();
                    //T_QC_QCPS : QCO Package Scheduling
                    command = new OracleCommand("DELETE PKMES.T_QC_QCPS WHERE QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO ", oracleConnection);
                    command.Parameters.Add(new OracleParameter("QCOFACTORY", vstrQCOFactory));
                    command.Parameters.Add(new OracleParameter("QCOYEAR", vintQCOYear));
                    command.Parameters.Add(new OracleParameter("QCOWEEKNO", vstrWeekNo));
                    if (IsSinglePP == false)
                    {
                        //command.ExecuteNonQuery();
                        await command.ExecuteNonQueryAsync();
                    }
                    command.Dispose();
                    //T_QC_QUEUE : QCO Ranking 
                    command = new OracleCommand("DELETE PKMES.T_QC_QUEUE WHERE QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO ", oracleConnection);
                    command.Parameters.Add(new OracleParameter("QCOFACTORY", vstrQCOFactory));
                    command.Parameters.Add(new OracleParameter("QCOYEAR", vintQCOYear));
                    command.Parameters.Add(new OracleParameter("QCOWEEKNO", vstrWeekNo));
                    if (IsSinglePP == false)
                    {
                        //command.ExecuteNonQuery();
                        await command.ExecuteNonQueryAsync();
                    }
                    command.Dispose();
                    //T_QC_QCPC : QCO Package Complete Cutting
                    command = new OracleCommand("DELETE PKMES.T_QC_QCPC WHERE QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO ", oracleConnection);
                    command.Parameters.Add(new OracleParameter("QCOFACTORY", vstrQCOFactory));
                    command.Parameters.Add(new OracleParameter("QCOYEAR", vintQCOYear));
                    command.Parameters.Add(new OracleParameter("QCOWEEKNO", vstrWeekNo));
                    if (IsSinglePP == false)
                    {
                        //command.ExecuteNonQuery();
                        await command.ExecuteNonQueryAsync();
                    }
                    command.Dispose();
                    oracleConnection.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> Insert_T_QC_QCFR(string vstrFactory, string vstrCurrentUserID, string executing)
        {
            try
            {
                bool isExe = true;
                //#if DEBUG
                //                isExe = false;
                //#endif
                if (isExe)
                    using (OracleConnection oracleConnection = new OracleConnection(_PKMESConnString))
                    {
                        oracleConnection.Open();
                        //int intI = 0;
                        OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(
                            "SELECT * FROM PKMES.T_QC_QCFR WHERE FACTORY = '" + vstrFactory + "' ", oracleConnection);
                        DataTable dt_T_QC_QCFR = new DataTable();
                        oracleDataAdapter.Fill(dt_T_QC_QCFR);
                        if (dt_T_QC_QCFR.Rows.Count > 0)
                        {
                            /*Update Existing Factory dt_T_QC_QCFR.Rows[0]["STATUS"] = "RUNNING"; dt_T_QC_QCFR.Rows[0]["EXECUTINGBY"] = vstrCurrentUserID; dt_T_QC_QCFR.Rows[0]["EXECUTINGDATE"] = DateTime.Now;*/
                            using (OracleCommand cmd = new OracleCommand())
                            {
                                cmd.Connection = oracleConnection;
                                cmd.CommandText =
                                    $"  Update PKMES.T_QC_QCFR " +
                                    $"  Set STATUS = 'RUNNING' , " +
                                    $"  EXECUTINGBY = '{vstrCurrentUserID}' , " +
                                    $"  EXECUTINGDATE = sysdate " +
                                    $"  Where FACTORY = '{vstrFactory}' ";
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                        else
                        {
                            using (OracleCommand cmd = new OracleCommand())
                            {
                                cmd.Connection = oracleConnection;
                                cmd.CommandText =
                                    $"  INSERT INTO PKMES.T_QC_QCFR (FACTORY , STATUS , EXECUTINGBY , EXECUTINGDATE) " +
                                    $"  Values ('{vstrFactory}' , 'RUNNING' , '{vstrCurrentUserID}' , sysdate ) ";
                                await cmd.ExecuteNonQueryAsync();
                            }
                            /*Insert For Non-existing Factory DataRow drNew_T_QC_QCFR = dt_T_QC_QCFR.NewRow(); drNew_T_QC_QCFR["FACTORY"] = vstrFactory; drNew_T_QC_QCFR["STATUS"] = "RUNNING"; drNew_T_QC_QCFR["EXECUTINGBY"] = vstrCurrentUserID; drNew_T_QC_QCFR["EXECUTINGDATE"] = DateTime.Now; dt_T_QC_QCFR.Rows.Add(drNew_T_QC_QCFR);*/
                        }
                        //OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter); //oracleDataAdapter.Update(dt_T_QC_QCFR); //oracleCommandBuilder.Dispose();
                        dt_T_QC_QCFR.Dispose();
                        oracleDataAdapter.Dispose();
                        oracleConnection.Close();
                    }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                return false;
            }
            return true;
        }
        public async Task Complete_T_QC_QCFR(string vstrQCOFactory, int vintQCOYear, string vstrWeekNo, string vstrCurrentUserID, string vstrResult, bool blHasError)
        {
            try
            {
                bool isExe = true;
                //#if DEBUG
                //	isExe = false;
                //#endif
                if (isExe)
                    using (OracleConnection oracleConnection = new OracleConnection(_PKMESConnString))
                    {
                        oracleConnection.Open();
                        var msg = blHasError ? vstrResult : $"QCO Calculation Success In Year[{vintQCOYear}], Week[{vstrWeekNo}]";
                        using (OracleCommand cmd = new OracleCommand())
                        {
                            cmd.CommandText = $@"
		Update PKMES.T_QC_QCFR 
		Set STATUS = 'COMPLETE' , LASTDONEBY = :LASTDONEBY , LASTCOMPLETEDATE = :LASTCOMPLETEDATE , FAILMESSAGE = :FAILMESSAGE
		Where FACTORY = :FACTORY
";
                            cmd.Connection = oracleConnection;
                            cmd.Parameters.Add("LASTDONEBY", vstrCurrentUserID);
                            cmd.Parameters.Add("LASTCOMPLETEDATE", DateTime.Now);
                            cmd.Parameters.Add("FAILMESSAGE", msg);
                            cmd.Parameters.Add("FACTORY", vstrQCOFactory);
                            await cmd.ExecuteNonQueryAsync();
                        }
                        ////int intI = 0; //OracleDataAdapter oracleDataAdapter = new OracleDataAdapter("SELECT * FROM PKMES.T_QC_QCFR WHERE FACTORY = '" + vstrQCOFactory + "' AND STATUS = 'RUNNING' ", oracleConnection); //DataTable dt_T_QC_QCFR = new DataTable(); //oracleDataAdapter.Fill(dt_T_QC_QCFR);  //if (dt_T_QC_QCFR.Rows.Count > 0) //{ //    /*Update Existing Factory */ //    dt_T_QC_QCFR.Rows[0]["STATUS"] = "COMPLETE"; //Important Flag  //    dt_T_QC_QCFR.Rows[0]["LASTDONEBY"] = vstrCurrentUserID; //    dt_T_QC_QCFR.Rows[0]["LASTCOMPLETEDATE"] = DateTime.Now;  //    if (blHasError) //        dt_T_QC_QCFR.Rows[0]["FAILMESSAGE"] = vstrResult; //    else //        dt_T_QC_QCFR.Rows[0]["FAILMESSAGE"] = "QCO Calculation Success In Year[" + vintQCOYear + "], Week[" + vstrWeekNo + "]"; //}  //OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter); //oracleDataAdapter.Update(dt_T_QC_QCFR); //oracleCommandBuilder.Dispose();  //dt_T_QC_QCFR.Dispose(); //oracleDataAdapter.Dispose(); //oracleConnection.Close(); //oracleConnection.Dispose();
                    }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
            }
        }
        public bool FactoryHasMaterialParameter(string vstrFactory, out List<Qcfo> lsAllFactoryParameters, out List<Qcfo> lsNoMaterialFactoryParameters)
        {
            lsAllFactoryParameters = null;
            lsNoMaterialFactoryParameters = null;
            try
            {
                bool blHasMaterialQtyPara = false;
                using (OracleConnection oracleConnection = new OracleConnection(_PKMESConnString))
                {
                    oracleConnection.Open();
                    //Return an empty list of opmt if keys code is empty. 
                    if (string.IsNullOrEmpty(vstrFactory))
                        return false;
                    //GET FACTORY SORTING SETTING;
                    string strSQL = "";
                    strSQL =
                             " SELECT T_CM_QCOP.DBFIELDNAME , T_00_QCFO.FACTORY , T_CM_QCOP.PARAMETERNAME, NVL(T_00_QCFO.SORTDIRECTION, 'ASC')  as SORTDIRECTION ,  T_CM_QCOP.SETTINGTYPE  " +
                             " FROM PKMES.T_00_QCFO " +
                             " INNER JOIN PKMES.T_CM_QCOP ON " +
                             "   T_00_QCFO.PARAMETERNAME = T_CM_QCOP.PARAMETERNAME " +
                             " WHERE T_00_QCFO.FACTORY = '" + vstrFactory + "' " +
                             " ORDER BY T_00_QCFO.SORTINGSEQ ";
                    DataTable dt = new DataTable();
                    PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, null, out dt, out strSQL);
                    if (dt != null)
                    {
                        List<Qcfo> lsTempFactoryParameters = new List<Qcfo>();
                        List<Qcfo> lsTempNoMaterialFactoryParameters = new List<Qcfo>();
                        int intTemp = 0;
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (!blHasMaterialQtyPara)
                                intTemp += 1;
                            //if (dr["PARAMETERNAME"].ToString().ToUpper() == "MATERIAL READINESS")
                            //{
                            //    blHasMaterialQtyPara = true;
                            //}
                            if (dr["SETTINGTYPE"].ToString().ToUpper() == "COMPUTED")
                            {
                                blHasMaterialQtyPara = true;
                            }
                            //2019-12-11 Tai Le (Thomas): get the Sorting Parameters which are before "Computed" field for 1st time sorting
                            if (!blHasMaterialQtyPara)
                                lsTempNoMaterialFactoryParameters.Add(new Qcfo(dr["FACTORY"].ToString(), dr["PARAMETERNAME"].ToString(), dr["DBFIELDNAME"].ToString(), dr["SORTDIRECTION"].ToString()));
                            //2019-12-11 Tai Le (Thomas): All sorting parameters for the last sorting.
                            lsTempFactoryParameters.Add(new Qcfo(dr["FACTORY"].ToString(), dr["PARAMETERNAME"].ToString(), dr["DBFIELDNAME"].ToString(), dr["SORTDIRECTION"].ToString()));
                        }
                        if (intTemp == dt.Rows.Count)
                            blHasMaterialQtyPara = false;
                        lsAllFactoryParameters = lsTempFactoryParameters;
                        lsNoMaterialFactoryParameters = lsTempNoMaterialFactoryParameters;
                        dt.Dispose();
                    }
                    oracleConnection.Close();
                    oracleConnection.Dispose();
                }
                return blHasMaterialQtyPara;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return false;
            }
        }
        public DataTable GetMTOPSPackage(string vstrFactory, out string vstrRetMsg, int pPreYear = 0, string pPreWeekNum = "")
        {
            vstrRetMsg = "";
            string strSQL = "";
            try
            {
                DataTable dt = new DataTable();
                using (OracleConnection oracleConnection = new OracleConnection(_PKMESConnString))
                {
                    oracleConnection.Open();
                    /**
					//                string strSQL = @" 
					//                     SELECT ROW_NUMBER() OVER(ORDER BY VIEW_ERP_PSRSNP_PLAN.PRDPKG) as RowSeq,  
					//                     VIEW_ERP_PSRSNP_PLAN.*   
					//                     FROM PKERP.VIEW_ERP_PSRSNP_PLAN  
					//                     LEFT JOIN PKERP.V_AO_PPDP ON  
					//                           VIEW_ERP_PSRSNP_PLAN.FACTORY = V_AO_PPDP.FACTORY  
					//                           AND VIEW_ERP_PSRSNP_PLAN.AONO = V_AO_PPDP.AONO 
					//                           AND VIEW_ERP_PSRSNP_PLAN.STYLECODE = V_AO_PPDP.STYLECODE  
					//                           AND VIEW_ERP_PSRSNP_PLAN.STYLESIZE = V_AO_PPDP.STYLESIZE  
					//                           AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = V_AO_PPDP.STYLECOLORSERIAL  
					//                           AND VIEW_ERP_PSRSNP_PLAN.REVNO = V_AO_PPDP.REVNO  
					//                           AND VIEW_ERP_PSRSNP_PLAN.PRDPKG = V_AO_PPDP.PRDPKG  
					//                      WHERE VIEW_ERP_PSRSNP_PLAN.FACTORY = :FACTORY  
					//                      AND VIEW_ERP_PSRSNP_PLAN.STATUS NOT IN ( '**' , 'AC', 'F-' , 'GD' , 'PS' , 'R-' , 'WC' ) 
					//                      AND VIEW_ERP_PSRSNP_PLAN.FACCLOSE = 'N'  
					//                      AND NVL(VIEW_ERP_PSRSNP_PLAN.PLANQTY,0) - NVL(V_AO_PPDP.PRDQTY,0) > 0 
					//AND rownum<=50
					//AND VIEW_ERP_PSRSNP_PLAN.DeliveryDate >= To_date('20191101', 'yyyyMMdd')
					//";
					//var paras = new List<OracleParameter> { new OracleParameter("FACTORY", vstrFactory) };
					//var _FAOT = PCMOracleLibrary.QueryToObject<FAOT>(oracleConnection, $"SELECT * FROM PKMES.T_QC_FAOT WHERE FACTORY = :FACTORY AND ISACTIVE = 'Y'  ", paras, out Msg);
					*/
                    /* Change Log:
					 * 2020-02-27 Tai Le(Thomas): Add new fields VIEW_MTOPSPP_MXPPKG.AllocatStatus ; 
					 * 2020-30-31 Tai Le(Thomas): Add FIN SO READINESS ; JIG READINESS  ; SOP READINESS
					 * 2020-05-04 Tai Le(Thomas): Add T_QC_QUEUE.QCOSTARTDATE , REFQCOSTARTDATE
					 * 2020-06-16 Tal Le(Thomas): Prevent the duplicate T_QC_QUEUE
					 */
                    /**
//					strSQL = $@" 
//    SELECT ROW_NUMBER() OVER(ORDER BY VIEW_ERP_PSRSNP_PLAN.PRDPKG) as RowSeq,  
//    NVL(VIEW_ERP_PSRSNP_PLAN.PLANQTY,0) - NVL(V_AO_PPDP.PRDQTY,0) as PLANQTYBAL  ,
//    VIEW_ERP_PSRSNP_PLAN.* 
//    , NVL(VIEW_MTOPSPP_MXPPKG.AllocateStatus , 'Not Scheduled' ) MESAllocateStatus 
//    , NVL(VIEW_MTOPSPP_MXPPKG.MINPLNSTARTDATE , '{DateTime.Now.AddYears(2).ToString("yyyyMMdd")}') as MESSTARTDATE
//    , VIEW_MTOPSPP_MXPPKG.MINPLNSTARTDATE
//    , NVL(T_QC_QUEUE.CHANGEQCORANK , T_QC_QUEUE.QCORANK ) PRECUSTOMRANK 
//	, NVL(T_QC_QUEUE.FINSOREADINESS, '0') as FINSOREADINESS 
//	, NVL(T_QC_QUEUE.JIGREADINESS, '0') as JIGREADINESS 
//	, NVL(T_QC_QUEUE.SOPREADINESS, '0') as SOPREADINESS 
//	, NVL(T_QC_QUEUE.QCOSTARTDATE , TO_DATE('{DateTime.Now.AddYears(2).ToString("yyyyMMdd")}' , 'yyyyMMdd')  ) as REFQCOSTARTDATE
//	, T_QC_QUEUE.QCOSTARTDATE
//	FROM PKERP.VIEW_ERP_PSRSNP_PLAN  
//    LEFT JOIN PKERP.V_AO_PPDP ON  
//        VIEW_ERP_PSRSNP_PLAN.FACTORY = V_AO_PPDP.FACTORY  
//        AND VIEW_ERP_PSRSNP_PLAN.AONO = V_AO_PPDP.AONO 
//        AND VIEW_ERP_PSRSNP_PLAN.STYLECODE = V_AO_PPDP.STYLECODE  
//        AND VIEW_ERP_PSRSNP_PLAN.STYLESIZE = V_AO_PPDP.STYLESIZE  
//        AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = V_AO_PPDP.STYLECOLORSERIAL  
//        AND VIEW_ERP_PSRSNP_PLAN.REVNO = V_AO_PPDP.REVNO  
//        AND VIEW_ERP_PSRSNP_PLAN.PRDPKG = V_AO_PPDP.PRDPKG 
//    LEFT JOIN PKMES.T_QC_QUEUE ON   
//        VIEW_ERP_PSRSNP_PLAN.FACTORY = T_QC_QUEUE.FACTORY   
//        AND VIEW_ERP_PSRSNP_PLAN.AONO = T_QC_QUEUE.AONO   
//        AND VIEW_ERP_PSRSNP_PLAN.STYLECODE = T_QC_QUEUE.STYLECODE   
//        AND VIEW_ERP_PSRSNP_PLAN.STYLESIZE = T_QC_QUEUE.STYLESIZE   
//        AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = T_QC_QUEUE.STYLECOLORSERIAL   
//        AND VIEW_ERP_PSRSNP_PLAN.REVNO = T_QC_QUEUE.REVNO   
//        AND VIEW_ERP_PSRSNP_PLAN.PRDPKG = T_QC_QUEUE.PRDPKG   
//        AND T_QC_QUEUE.QCOFACTORY =  '{vstrFactory}'    
//        AND T_QC_QUEUE.QCOYEAR =  {pPreYear}
//        AND T_QC_QUEUE.QCOWEEKNO = '{pPreWeekNum}' 
//    LEFT JOIN PKMES.VIEW_MTOPSPP_MXPPKG ON 
//        VIEW_ERP_PSRSNP_PLAN.PRDPKG = View_MTOPSPP_MXPPKG.PPACKAGE
//    WHERE 
//    ( VIEW_ERP_PSRSNP_PLAN.FACTORY = '{vstrFactory}'  )
//    AND ( VIEW_ERP_PSRSNP_PLAN.STATUS NOT IN ( '**' , 'AC', 'F-' , 'GD' , 'PS' , 'R-' , 'WC' )  )
//    AND ( VIEW_ERP_PSRSNP_PLAN.FACCLOSE = 'N'  )
//    AND ( NVL(VIEW_ERP_PSRSNP_PLAN.PLANQTY,0) - NVL(V_AO_PPDP.PRDQTY,0) > 0  )   
//";
*/
                    strSQL = $@" 
	SELECT ROW_NUMBER() OVER(ORDER BY VIEW_ERP_PSRSNP_PLAN.PRDPKG) as RowSeq,  
	NVL(VIEW_ERP_PSRSNP_PLAN.PLANQTY,0) - NVL(V_AO_PPDP.PRDQTY,0) as PLANQTYBAL  ,
	VIEW_ERP_PSRSNP_PLAN.* 
	, NVL(VIEW_MTOPSPP_MXPPKG.AllocateStatus , 'Not Scheduled' ) MESAllocateStatus 
	, NVL(VIEW_MTOPSPP_MXPPKG.MINPLNSTARTDATE , '{DateTime.Now.AddYears(2):yyyyMMdd}') as MESSTARTDATE
	, VIEW_MTOPSPP_MXPPKG.MINPLNSTARTDATE
	, NVL(T_QC_QUEUE.CHANGEQCORANK , T_QC_QUEUE.QCORANK ) PRECUSTOMRANK 
	, NVL(T_QC_QUEUE.FINSOREADINESS, '0') as FINSOREADINESS 
	, NVL(T_QC_QUEUE.JIGREADINESS, '0') as JIGREADINESS 
	, NVL(T_QC_QUEUE.SOPREADINESS, '0') as SOPREADINESS 
	, NVL(T_QC_QUEUE.QCOSTARTDATE , TO_DATE('{DateTime.Now.AddYears(2):yyyyMMdd}' , 'yyyyMMdd')  ) as REFQCOSTARTDATE
	, T_QC_QUEUE.QCOSTARTDATE
	FROM PKERP.VIEW_ERP_PSRSNP_PLAN  
	LEFT JOIN PKERP.V_AO_PPDP ON  
		VIEW_ERP_PSRSNP_PLAN.FACTORY = V_AO_PPDP.FACTORY  
		AND VIEW_ERP_PSRSNP_PLAN.AONO = V_AO_PPDP.AONO 
		AND VIEW_ERP_PSRSNP_PLAN.STYLECODE = V_AO_PPDP.STYLECODE  
		AND VIEW_ERP_PSRSNP_PLAN.STYLESIZE = V_AO_PPDP.STYLESIZE  
		AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = V_AO_PPDP.STYLECOLORSERIAL  
		AND VIEW_ERP_PSRSNP_PLAN.REVNO = V_AO_PPDP.REVNO  
		AND VIEW_ERP_PSRSNP_PLAN.PRDPKG = V_AO_PPDP.PRDPKG 
	LEFT JOIN ( SELECT
    T_QC_QUEUE.FACTORY,
    T_QC_QUEUE.AONO ,
    T_QC_QUEUE.STYLECODE,
    T_QC_QUEUE.STYLESIZE,
    T_QC_QUEUE.STYLECOLORSERIAL,
    T_QC_QUEUE.REVNO ,
    T_QC_QUEUE.PRDPKG ,
    T_QC_QUEUE.CHANGEQCORANK ,
    T_QC_QUEUE.QCORANK ,
    T_QC_QUEUE.FINSOREADINESS ,
    T_QC_QUEUE.JIGREADINESS ,
    T_QC_QUEUE.SOPREADINESS ,
    T_QC_QUEUE.QCOSTARTDATE
FROM
    PKMES.T_QC_QUEUE
JOIN (
	SELECT
		FACTORY, AONO , STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO , PRDPKG , MIN (nvl(CHANGEQCORANK , QCORANK)) QCORANK
	FROM
		PKMES.T_QC_QUEUE
	WHERE
		1 = 1
		AND T_QC_QUEUE.QCOFACTORY = '{vstrFactory}'
		AND T_QC_QUEUE.QCOYEAR = {pPreYear}
		AND T_QC_QUEUE.QCOWEEKNO = '{pPreWeekNum}'
	GROUP BY
		FACTORY, AONO , STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO , PRDPKG ) Submain ON
	T_QC_QUEUE.FACTORY = Submain.FACTORY 
	AND T_QC_QUEUE.AONO = Submain.AONO 
	AND T_QC_QUEUE.STYLECODE = Submain.STYLECODE 
	AND T_QC_QUEUE.STYLESIZE = Submain.STYLESIZE 
	AND T_QC_QUEUE.STYLECOLORSERIAL = Submain.STYLECOLORSERIAL 
	AND T_QC_QUEUE.REVNO = Submain.REVNO 
	AND T_QC_QUEUE.PRDPKG = Submain.PRDPKG 
	AND nvl(T_QC_QUEUE.CHANGEQCORANK , T_QC_QUEUE.QCORANK) = Submain.QCORANK
	WHERE
		1 = 1
		AND T_QC_QUEUE.QCOFACTORY = '{vstrFactory}'
		AND T_QC_QUEUE.QCOYEAR = {pPreYear}
		AND T_QC_QUEUE.QCOWEEKNO = '{pPreWeekNum}'
) T_QC_QUEUE ON   
		VIEW_ERP_PSRSNP_PLAN.FACTORY = T_QC_QUEUE.FACTORY   
		AND VIEW_ERP_PSRSNP_PLAN.AONO = T_QC_QUEUE.AONO   
		AND VIEW_ERP_PSRSNP_PLAN.STYLECODE = T_QC_QUEUE.STYLECODE   
		AND VIEW_ERP_PSRSNP_PLAN.STYLESIZE = T_QC_QUEUE.STYLESIZE   
		AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = T_QC_QUEUE.STYLECOLORSERIAL   
		AND VIEW_ERP_PSRSNP_PLAN.REVNO = T_QC_QUEUE.REVNO   
		AND VIEW_ERP_PSRSNP_PLAN.PRDPKG = T_QC_QUEUE.PRDPKG   
	LEFT JOIN PKMES.VIEW_MTOPSPP_MXPPKG ON 
		VIEW_ERP_PSRSNP_PLAN.PRDPKG = View_MTOPSPP_MXPPKG.PPACKAGE
	WHERE 
	( VIEW_ERP_PSRSNP_PLAN.FACTORY = '{vstrFactory}'  )
	AND ( VIEW_ERP_PSRSNP_PLAN.STATUS NOT IN ( '**' , 'AC', 'F-' , 'GD' , 'PS' , 'R-' , 'WC' )  )
	AND ( VIEW_ERP_PSRSNP_PLAN.FACCLOSE = 'N'  )
	AND ( NVL(VIEW_ERP_PSRSNP_PLAN.PLANQTY,0) - NVL(V_AO_PPDP.PRDQTY,0) > 0  )   
";
                    //#if DEBUG
                    //                    var PRDPKG = " P_COH-4969_2_COH1211RGL001014_02','P_COH-4950_1_COH1807RGL001001_01','P_COH-4950_1_COH1807RGL002001_01','P_COH-4950_1_COH1808RGL001001_01','P_COH-4950_1_COH1808RGL002001_01','P_COH-4950_1_COH1809RGL001001_01 ";
                    //                    strSQL += $" AND ( VIEW_ERP_PSRSNP_PLAN.PRDPKG IN ('{PRDPKG}') )";
                    //#endif
                    //Get Factory AO Type Filter from PKMES.T_QC_FAOT
                    string Msg = "";
                    var _FAOT = PCMOracleLibrary.QueryToObject<FAOT>(oracleConnection, $"SELECT * FROM PKMES.T_QC_FAOT WHERE FACTORY = '{vstrFactory}' AND ISACTIVE = 'Y'  ", null, out Msg);
                    if (_FAOT != null)
                        if (_FAOT.Count > 0)
                        {
                            Msg = "";
                            var seperator = " OR ";
                            for (int i = 0; i < _FAOT.Count; i++)
                            {
                                if (Msg.Length == 0)
                                    Msg = $" VIEW_ERP_PSRSNP_PLAN.ADTYPE = '{_FAOT[i].ADTYPE}'  ";
                                else
                                    Msg = Msg + seperator + $" VIEW_ERP_PSRSNP_PLAN.ADTYPE = '{_FAOT[i].ADTYPE}'  ";
                            }
                            if (Msg.Length > 0)
                                strSQL = strSQL + $" AND ( {Msg} )";
                        }
                    PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, null, out dt, out strSQL);
                    //oracleConnection.Close();
                    //oracleConnection.Dispose();
                }
                return dt;
            }
            catch (Exception ex)
            {
                vstrRetMsg = ex.Message + "Query= " + strSQL;
                return null;
            }
        }
        public void Sort_T_QC_QCFP(ref DataTable vdt_QCFP, List<Qcfo> vQCOFactorySortingParameters, string vType)
        {
            try
            {
                bool blHandleQCOSTARTDATE = false;
                if (vQCOFactorySortingParameters.Where(x => x.DBFIELDNAME == "QCOSTARTDATE").Count() > 0)
                    blHandleQCOSTARTDATE = true;
                if (blHandleQCOSTARTDATE)
                {
                    //2020-05-06 Tai Le(Thomas) Handle QCOStartDate , temp sorting column
                    vdt_QCFP.Columns.Add(new DataColumn("SortOrderQCOStartDate", typeof(System.Double)));
                    foreach (DataRow dr in vdt_QCFP.Rows)
                    {
                        if (dr["QCOSTARTDATE"] != null)
                        {
                            if (dr["QCOSTARTDATE"].ToString().Length > 0)
                                dr["SortOrderQCOStartDate"] = true;
                            else
                                dr["SortOrderQCOStartDate"] = false;
                        }
                        else
                            dr["SortOrderQCOStartDate"] = false;
                    }
                }
                var dv = vdt_QCFP.DefaultView;
                string strSorting = "";
                foreach (var objNonMateialFactoryParameters in vQCOFactorySortingParameters)
                {
                    if (String.IsNullOrEmpty(strSorting))
                    {
                        if (objNonMateialFactoryParameters.DBFIELDNAME == "QCOSTARTDATE")
                            strSorting = $"SortOrderQCOStartDate DESC ,{objNonMateialFactoryParameters.DBFIELDNAME} {objNonMateialFactoryParameters.SORTDIRECTION}";
                        else
                            strSorting = objNonMateialFactoryParameters.DBFIELDNAME + " " + objNonMateialFactoryParameters.SORTDIRECTION;
                    }
                    else
                    {
                        if (objNonMateialFactoryParameters.DBFIELDNAME == "QCOSTARTDATE")
                            strSorting = strSorting + ", " + $"SortOrderQCOStartDate DESC , {objNonMateialFactoryParameters.DBFIELDNAME}  {objNonMateialFactoryParameters.SORTDIRECTION} ";
                        else
                            strSorting = strSorting + ", " + objNonMateialFactoryParameters.DBFIELDNAME + " " + objNonMateialFactoryParameters.SORTDIRECTION;
                    }
                }
                //::: Extra Sorting Parameter Since has T_SD_BOMT 
                if (vType == "All")
                    strSorting = strSorting + ", ITEMCODE , ITEMCOLORSERIAL ";
                dv.Sort = strSorting;
                vdt_QCFP = dv.ToTable();
                if (blHandleQCOSTARTDATE)
                    vdt_QCFP.Columns.Remove("SortOrderQCOStartDate"); //2020-05-06 Tai Le(Thomas) : delete the temp Sorting column
            }
            catch (Exception Ex)
            {
                var Msg = Ex.Message;
            }
        }
        public bool Save_T_QC_QCFP(string vstrQCOFactory, DateTime vdtStartTime, int vintQCOYear, string vstrWeekNo, DataTable vdt_T_QC_QCFP)
        {
            try
            {
                if (vdt_T_QC_QCFP != null)
                {
                    using (OracleConnection oracleConnection = new OracleConnection(_PKMESConnString))
                    {
                        oracleConnection.Open();
                        intNegativeRank = 0;
                        int intPositiveRank = 0;
                        int intI = 0;
                        OracleDataAdapter oracleDataAdapter = new OracleDataAdapter("SELECT * FROM PKMES.T_QC_QCFP WHERE QCOFACTORY = '" + vstrQCOFactory + "'  AND QCOYEAR = " + vintQCOYear + " AND QCOWEEKNO = '" + vstrWeekNo + "' ", oracleConnection);
                        DataTable dt_T_QC_QCFP = new DataTable();
                        oracleDataAdapter.Fill(dt_T_QC_QCFP);
                        foreach (DataRow dr in vdt_T_QC_QCFP.Rows)
                        {
                            intI += 1;
                            DataRow drNew_T_QC_QCFP = dt_T_QC_QCFP.NewRow();
                            drNew_T_QC_QCFP["QCOFACTORY"] = vstrQCOFactory;
                            drNew_T_QC_QCFP["QCOYEAR"] = vintQCOYear;
                            drNew_T_QC_QCFP["QCOWEEKNO"] = vstrWeekNo;
                            //2019-04-08
                            if (DateTime.Parse(dr["DELIVERYDATE"].ToString()).Date < vdtStartTime.Date)
                            {
                                intNegativeRank = intNegativeRank - 1;
                                drNew_T_QC_QCFP["RANGKING"] = intNegativeRank;
                            }
                            else
                            {
                                intPositiveRank = intPositiveRank + 1;
                                drNew_T_QC_QCFP["RANGKING"] = intPositiveRank;
                            }
                            drNew_T_QC_QCFP["ID"] = vstrQCOFactory + "_" + PCMGeneralFunctions.GetRight("0000000000000000" + intI, 15);
                            drNew_T_QC_QCFP["FACTORY"] = dr["FACTORY"];
                            drNew_T_QC_QCFP["LINENO"] = dr["LINENO"];
                            drNew_T_QC_QCFP["AONO"] = dr["AONO"];
                            drNew_T_QC_QCFP["STYLECODE"] = dr["STYLECODE"];
                            drNew_T_QC_QCFP["STYLESIZE"] = dr["STYLESIZE"];
                            drNew_T_QC_QCFP["STYLECOLORSERIAL"] = dr["STYLECOLORSERIAL"];
                            drNew_T_QC_QCFP["REVNO"] = dr["REVNO"];
                            drNew_T_QC_QCFP["PRDPKG"] = dr["PRDPKG"];
                            drNew_T_QC_QCFP["PLANQTY"] = dr["PLANQTY"];
                            drNew_T_QC_QCFP["PLANQTYBAL"] = dr["PLANQTYBAL"]; //2019-11-04 Tai Le (Thomas): Add to show the Remain PP instead of original Production QTY
                            drNew_T_QC_QCFP["DELIVERYDATE"] = dr["DELIVERYDATE"];
                            drNew_T_QC_QCFP["PRDSDAT"] = dr["PRDSDAT"];
                            drNew_T_QC_QCFP["PRDEDAT"] = dr["PRDEDAT"];
                            /*2019-04-22 Tai Le(Thomas): Add 1 Original Data from AOMTOPS Package {ORDQTY} */
                            drNew_T_QC_QCFP["ORDQTY"] = dr["ORDQTY"];
                            /*2020-01-13 Tai Le(Thomas): Add Previous Custom Ranking*/
                            if (dr["PRECUSTOMRANK"] != null)
                                drNew_T_QC_QCFP["PRECUSTOMRANK"] = dr["PRECUSTOMRANK"];
                            /*2020-02-27 Tai Le(Thomas)*/
                            if (dr["MESALLOCATESTATUS"] != null)
                                drNew_T_QC_QCFP["MESALLOCATESTATUS"] = dr["MESALLOCATESTATUS"];
                            /*2020-02-29 Tai Le(Thomas)*/
                            //if (dr["MINPLNSTARTDATE"] != null)
                            //    drNew_T_QC_QCFP["MESSTARTDATE"] = dr["MINPLNSTARTDATE"];
                            if (dr["MESSTARTDATE"] != null)
                                drNew_T_QC_QCFP["MESSTARTDATE"] = dr["MESSTARTDATE"];
                            //2020-03-31 Tai Le(Thomas)
                            if (dr["FINSOREADINESS"] != null)
                                drNew_T_QC_QCFP["FINSOREADINESS"] = dr["FINSOREADINESS"];
                            if (dr["JIGREADINESS"] != null)
                                drNew_T_QC_QCFP["JIGREADINESS"] = dr["JIGREADINESS"];
                            if (dr["SOPREADINESS"] != null)
                                drNew_T_QC_QCFP["SOPREADINESS"] = dr["SOPREADINESS"];
                            //::END		2020-03-31 Tai Le(Thomas)
                            //2020-05-06 Tai Le(Thomas): Custom QCO Start Date
                            if (dr["QCOSTARTDATE"] != null)
                                drNew_T_QC_QCFP["QCOSTARTDATE"] = dr["QCOSTARTDATE"];
                            dt_T_QC_QCFP.Rows.Add(drNew_T_QC_QCFP);
                        }
                        OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                        oracleDataAdapter.Update(dt_T_QC_QCFP);
                        oracleCommandBuilder.Dispose();
                        dt_T_QC_QCFP.Dispose();
                        oracleDataAdapter.Dispose();
                        oracleConnection.Close();
                        oracleConnection.Dispose();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                return false;
            }
        }
        public void Update_T_QC_QCFP_MaterialReadiness(ref DataTable vdt_QCFP, DataTable vdt_T_QC_QCPM)
        {
            try
            {
                double dbRequestQty = 0, dbReceivedQty = 0;
                string ID = "", Next_ID = "";
                foreach (DataRow dr_QCFP in vdt_QCFP.Rows)
                {
                    ID = "";
                    Next_ID = "";
                    //Debug.Print("Updating Material Readiness For: " +
                    //			"AONo[" + dr_QCFP["AONO"] + "] ;  " +
                    //			"StyleCode[" + dr_QCFP["STYLECODE"] + "] ; " +
                    //			"StyleSize[" + dr_QCFP["STYLESIZE"] + "] ; " +
                    //			"StyleColorSerial[" + dr_QCFP["STYLECOLORSERIAL"] + "]; " +
                    //			"RevNo[" + dr_QCFP["REVNO"] + "]; " +
                    //			"PrdPkg[" + dr_QCFP["PRDPKG"] + "]; " +
                    //			"ItemCode[" + dr_QCFP["ITEMCODE"] + "]; " +
                    //			"ItemColorSerial[" + dr_QCFP["ITEMCOLORSERIAL"] + "] ; " +
                    //			"dbRequestQty= " + dr_QCFP["REQUESTQTY"]);
                    ID = dr_QCFP["ID"].ToString();
                    if (dr_QCFP["NEXT_ID"] != null)
                        Next_ID = dr_QCFP["NEXT_ID"].ToString();
                    //Keep accumlating the Request Qty
                    if (dr_QCFP["REQUESTQTY"] != null)
                        dbRequestQty = dbRequestQty + Convert.ToDouble(dr_QCFP["REQUESTQTY"].ToString());
                    string expression = " FACTORY = '" + dr_QCFP["FACTORY"] + "' " +
                                        " AND LINENO = '" + dr_QCFP["LINENO"] + "'  " +
                                        " AND AONO = '" + dr_QCFP["AONO"] + "' " +
                                        " AND STYLECODE = '" + dr_QCFP["STYLECODE"] + "' " +
                                        " AND STYLESIZE = '" + dr_QCFP["STYLESIZE"] + "' " +
                                        " AND STYLECOLORSERIAL = '" + dr_QCFP["STYLECOLORSERIAL"] + "' " +
                                        " AND REVNO = '" + dr_QCFP["REVNO"] + "' " +
                                        " AND PRDPKG = '" + dr_QCFP["PRDPKG"] + "' " +
                                        " AND ITEMCODE = '" + dr_QCFP["ITEMCODE"] + "' " +
                                        " AND ITEMCOLORSERIAL = '" + dr_QCFP["ITEMCOLORSERIAL"] + "'  " +
                                        " ";
                    //DataRow[] FoundRows_T_QC_QCPM = vdt_T_QC_QCPM.Select(expression);
                    foreach (DataRow dr_FoundRow_T_QC_QCPM in vdt_T_QC_QCPM.Select(expression))
                    //foreach (DataRow dr_FoundRow_T_QC_QCPM in FoundRows_T_QC_QCPM)
                    {
                        if (dr_FoundRow_T_QC_QCPM["QUANTITY_A"] != null)
                            dbReceivedQty = dbReceivedQty + Convert.ToDouble(dr_FoundRow_T_QC_QCPM["QUANTITY_A"].ToString());
                        if (dr_FoundRow_T_QC_QCPM["QUANTITY_B"] != null)
                            dbReceivedQty = dbReceivedQty + Convert.ToDouble(dr_FoundRow_T_QC_QCPM["QUANTITY_B"].ToString());
                        if (dr_FoundRow_T_QC_QCPM["QUANTITY_C"] != null)
                            dbReceivedQty = dbReceivedQty + Convert.ToDouble(dr_FoundRow_T_QC_QCPM["QUANTITY_C"].ToString());
                        if (dr_FoundRow_T_QC_QCPM["QUANTITY_D"] != null)
                            dbReceivedQty = dbReceivedQty + Convert.ToDouble(dr_FoundRow_T_QC_QCPM["QUANTITY_D"].ToString());
                    }
                    if (ID != Next_ID)
                    {
                        //Update the Material Readiness
                        double MaterialReadiness = 0;
                        if (dbRequestQty > 0)
                        {
                            MaterialReadiness = dbReceivedQty / dbRequestQty * 100;
                        }
                        dr_QCFP["MATNORNALRATE"] = Math.Truncate(MaterialReadiness * 100) / 100;
#if DEBUG
                        Debug.Print("Updating Material Readiness For: " +
                                    "AONo[" + dr_QCFP["AONO"] + "] ;  " +
                                    "StyleCode[" + dr_QCFP["STYLECODE"] + "] ; " +
                                    "StyleSize[" + dr_QCFP["STYLESIZE"] + "] ; " +
                                    "StyleColorSerial[" + dr_QCFP["STYLECOLORSERIAL"] + "]; " +
                                    "RevNo[" + dr_QCFP["REVNO"] + "]; " +
                                    "PrdPkg[" + dr_QCFP["PRDPKG"] + "]; " +
                                    "ItemCode[" + dr_QCFP["ITEMCODE"] + "]; " +
                                    "ItemColorSerial[" + dr_QCFP["ITEMCOLORSERIAL"] + "] ; " +
                                    "Material_Readiness= " + (Math.Truncate(MaterialReadiness * 100) / 100));
#endif
                        //Reset the value
                        dbRequestQty = 0;
                        dbReceivedQty = 0;
#if DEBUG
                        Debug.Print("dbRequestQty= " + dbRequestQty);
                        Debug.Print("dbReceivedQty= " + dbReceivedQty);
#endif
                    }
                    else
                    {
                        dr_QCFP["MATNORNALRATE"] = -1;
                    }
                    ////if (Factory != RefFactory && LineNo != RefLineNo && AONo != RefAONo && StyleCode != RefStyleCode && StyleSize != RefStyleSize && StyleColorSerial != RefStyleColorSerial && RevNo != RefRevNo && PrdPkg != RefPrdPkg)
                    //if (Factory + LineNo + AONo + StyleCode + StyleSize + StyleColorSerial + RevNo + PrdPkg != RefFactory + RefLineNo + RefAONo + RefStyleCode + RefStyleSize + RefStyleColorSerial + RefRevNo + RefPrdPkg)
                    //{  
                    //    dbReceivedQty = 0; 
                    //    string expression = " FACTORY = '" + dr_QCFP["FACTORY"] + "' " +
                    //                        " AND LINENO = '" + dr_QCFP["LINENO"] + "'  " +
                    //                        " AND AONO = '" + dr_QCFP["AONO"] + "' " +
                    //                        " AND STYLECODE = '" + dr_QCFP["STYLECODE"] + "' " +
                    //                        " AND STYLESIZE = '" + dr_QCFP["STYLESIZE"] + "' " +
                    //                        " AND STYLECOLORSERIAL = '" + dr_QCFP["STYLECOLORSERIAL"] + "' " +
                    //                        " AND REVNO = '" + dr_QCFP["REVNO"] + "' " +
                    //                        " AND PRDPKG = '" + dr_QCFP["PRDPKG"] + "' " +
                    //                        " "; 
                    //    DataRow[] FoundRows_T_QC_QCPM = vdt_T_QC_QCPM.Select(expression); 
                    //    foreach (DataRow dr_FoundRow_T_QC_QCPM in FoundRows_T_QC_QCPM)
                    //    {
                    //        if (dr_FoundRow_T_QC_QCPM["QUANTITY_A"] != null)
                    //            dbReceivedQty = dbReceivedQty + Convert.ToDouble(dr_FoundRow_T_QC_QCPM["QUANTITY_A"].ToString());
                    //    } 
                    //    var MaterialReadiness = dbReceivedQty / dbRequestQty * 100;
                    //    dr_QCFP["MATNORNALRATE"] = Math.Truncate(MaterialReadiness * 100) / 100;
                    //    Debug.Print("Update Material Readiness For: AONo[" + AONo + "] ;  StyleCode[" + StyleCode + "] ; StyleSize[" + StyleSize + "] ; StyleColorSerial[" + StyleColorSerial + "]; RevNo[" + RevNo + "]; PrdPkg[" + PrdPkg + "] , Material_Readiness= " + Math.Truncate(MaterialReadiness * 100) / 100);
                    //    RefFactory = Factory;
                    //    RefLineNo = LineNo;
                    //    RefAONo = AONo;
                    //    RefStyleCode = StyleCode;
                    //    RefStyleSize = StyleSize;
                    //    RefStyleColorSerial = StyleColorSerial;
                    //    RefRevNo = RevNo;
                    //    RefPrdPkg = PrdPkg;
                    //} 
                }
            }
            catch (Exception Ex)
            {
                var Msg = Ex.Message;
                if (mEnviroment.ToLower() == "console")
                    Console.WriteLine("ERROR at Update_T_QC_QCFP_MaterialReadiness(): " + Msg);
            }
        }
        public bool Update_T_QC_QUEUE(string vstrQCOFactory, int vintQCOYear, string vstrWeekNo, DataTable vdt_T_QC_QCFP)
        {
            int intI = 0;
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(_PKMESConnString))
                {
                    oracleConnection.Open();
                    //OracleDataAdapter oracleDataAdapter = new OracleDataAdapter("SELECT * FROM PKMES.T_QC_QUEUE WHERE FACTORY = '" + vstrQCOFactory + "'  ", oracleConnection);
                    //DataTable dt_T_QC_QUEUE = new DataTable();
                    //oracleDataAdapter.Fill(dt_T_QC_QUEUE);
                    //int intRowCount = -1 * vdt_T_QC_QCFP.Rows.Count;
                    foreach (DataRow dr in vdt_T_QC_QCFP.Rows)
                    {
                        intI += 1;
                        if (dr["MATNORNALRATE"] != null)
                        {
                            //if (dr["MATNORNALRATE"].ToString().Length >= 0)
                            //{
                            OracleCommand oracleCommand = new OracleCommand("" +
                                " UPDATE PKMES.T_QC_QUEUE " +
                                " SET CUTTINGREADINESS = 0 , " +
                                " NORMALIZEDPERCENT = :NORMALIZEDPERCENT ," +
                                " LATESTQCOTIME = :LATESTQCOTIME " +
                                " WHERE QCOFACTORY = :QCOFACTORY " +
                                " AND QCOYEAR = :QCOYEAR " +
                                " AND QCOWEEKNO = :QCOWEEKNO " +
                                " AND AONO = :AONO " +
                                " AND FACTORY = :FACTORY " +
                                " AND STYLECODE = :STYLECODE " +
                                " AND STYLESIZE = :STYLESIZE " +
                                " AND STYLECOLORSERIAL = :STYLECOLORSERIAL " +
                                " AND REVNO = :REVNO " +
                                " AND PRDPKG = :PRDPKG "
                                , oracleConnection);
                            oracleCommand.CommandTimeout = 90;
                            oracleCommand.CommandType = CommandType.Text;
                            List<OracleParameter> parameters = new List<OracleParameter>();
                            parameters.Add(new OracleParameter("NORMALIZEDPERCENT", dr["MATNORNALRATE"].ToString()));
                            parameters.Add(new OracleParameter("LATESTQCOTIME", DateTime.Now));
                            parameters.Add(new OracleParameter("QCOFACTORY", dr["QCOFACTORY"].ToString()));
                            parameters.Add(new OracleParameter("QCOYEAR", dr["QCOYEAR"].ToString()));
                            parameters.Add(new OracleParameter("QCOWEEKNO", dr["QCOWEEKNO"].ToString()));
                            parameters.Add(new OracleParameter("AONO", dr["AONO"].ToString()));
                            parameters.Add(new OracleParameter("FACTORY", dr["FACTORY"].ToString()));
                            parameters.Add(new OracleParameter("STYLECODE", dr["STYLECODE"].ToString()));
                            parameters.Add(new OracleParameter("STYLESIZE", dr["STYLESIZE"].ToString()));
                            parameters.Add(new OracleParameter("STYLECOLORSERIAL", dr["STYLECOLORSERIAL"].ToString()));
                            parameters.Add(new OracleParameter("REVNO", dr["REVNO"].ToString()));
                            parameters.Add(new OracleParameter("PRDPKG", dr["PRDPKG"].ToString()));
                            oracleCommand.Parameters.AddRange(parameters.ToArray());
                            oracleCommand.ExecuteNonQuery();
                            oracleCommand.Dispose();
                            //}
                        }
                    }
                    //OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                    //oracleDataAdapter.Update(dt_T_QC_QUEUE);
                    //oracleCommandBuilder.Dispose();
                    //dt_T_QC_QUEUE.Dispose();
                    //oracleDataAdapter.Dispose();
                    oracleConnection.Close();
                    oracleConnection.Dispose();
                }
                return true;
            }
            catch (Exception ex)
            {
                var Msg = ex.Message + "; at Row[" + intI + "] ";
                if (mEnviroment.ToLower() == "console")
                    Console.WriteLine("ERROR at Update_T_QC_QUEUE(): " + Msg);
                return false;
            }
        }
        public bool Update_CuttingReadiness(string vstrQCOFactory, int vintQCOYear, string vstrWeekNo)
        {
            decimal CutReadiness = 0;
            string query = "", msg = "";
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(_PKMESConnString))
                {
                    oracleConnection.Open();

                    query = $@"
SELECT * 
FROM PKERP.V_PKERP_CUTTING_READINESS 
WHERE PRDPKG IN ( SELECT PRDPKG FROM T_QC_QCFP WHERE QCOYEAR ={vintQCOYear} AND QCOWEEKNO = '{vstrWeekNo}' AND QCOFACTORY = '{vstrQCOFactory}' ) 
";
                    var dt = new DataTable();

                    PCMOracleLibrary.StatementToDataTable(oracleConnection, query, null, out dt, out msg);

                    if (dt == null) return false;

                    foreach (DataRow dr in dt.Rows)
                    {
                        CutReadiness = 0;
                        //Pre-hanlding
                        if (dr["CUTTINGREADINESS"] == null) continue;
                        if (dr["CUTTINGREADINESS"].ToString() == ".00") continue;

                        CutReadiness = decimal.Parse(dr["CUTTINGREADINESS"].ToString());

                        OracleCommand oracleCommand = new OracleCommand("" +
                                   " UPDATE PKMES.T_QC_QUEUE " +
                                   " SET CUTTINGREADINESS = :CUTTINGREADINESS " +
                                   " WHERE QCOFACTORY = :QCOFACTORY " +
                                   " AND QCOYEAR = :QCOYEAR " +
                                   " AND QCOWEEKNO = :QCOWEEKNO " +
                                   " AND PRDPKG = :PRDPKG "
                                   , oracleConnection);
                        oracleCommand.CommandTimeout = 90;
                        oracleCommand.CommandType = CommandType.Text;
                        List<OracleParameter> parameters = new List<OracleParameter>();
                        parameters.Add(new OracleParameter("CUTTINGREADINESS", CutReadiness));
                        parameters.Add(new OracleParameter("QCOFACTORY", vstrQCOFactory));
                        parameters.Add(new OracleParameter("QCOYEAR", vintQCOYear));
                        parameters.Add(new OracleParameter("QCOWEEKNO", vstrWeekNo));
                        parameters.Add(new OracleParameter("PRDPKG", dr["PRDPKG"]));
                        oracleCommand.Parameters.AddRange(parameters.ToArray());
                        oracleCommand.ExecuteNonQuery();
                        oracleCommand.Dispose();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                //var Msg = ex.Message + "; at Row[" + intI + "] ";
                if (string.Equals(mEnviroment, "console", StringComparison.OrdinalIgnoreCase)) Console.WriteLine($"ERROR at Update_CuttingReadiness(): {ex.Message}");
                return false;
            }
        }
        public bool Save_T_QC_QUEUE(string vstrQCOFactory, DateTime vdtStarDateTime, int vintQCOYear, string vstrWeekNo, DataTable vdt_T_QC_QCFP)
        {
            int intI = 0,
                intQCORANKINGNEW = 0;
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(_PKMESConnString))
                {
                    oracleConnection.Open();
                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(
                        selectCommandText: $" SELECT * FROM PKMES.T_QC_QUEUE " + $" WHERE QCOFACTORY = '" + vstrQCOFactory + "' AND " + $" QCOYEAR = " + vintQCOYear + " AND " + $" QCOWEEKNo = '" + vstrWeekNo + "'  ",
                        selectConnection: oracleConnection);
                    DataTable dt_T_QC_QUEUE = new DataTable();
                    oracleDataAdapter.Fill(dt_T_QC_QUEUE);
                    //int intRowCount = -1 * vdt_T_QC_QCFP.Rows.Count;
                    foreach (DataRow dr in vdt_T_QC_QCFP.Rows)
                    {
                        if (dr["MATNORNALRATE"] != null)
                            if (dr["MATNORNALRATE"].ToString().Length > 0)
                                if (Convert.ToDouble(dr["MATNORNALRATE"].ToString()) >= 0)
                                {
                                    //intI += 1;
                                    DataRow drNew_T_QC_QCFP = dt_T_QC_QUEUE.NewRow();
                                    drNew_T_QC_QCFP["QCOFACTORY"] = dr["QCOFACTORY"];
                                    drNew_T_QC_QCFP["QCOYEAR"] = dr["QCOYEAR"];
                                    drNew_T_QC_QCFP["QCOWEEKNO"] = dr["QCOWEEKNO"];
                                    //2019-03-08 Thomas: Modify the RANKING
                                    //drNew_T_QC_QCFP["QCORANK"] = intI; //dr["QCOWEEKNO"];
                                    if (dr["DELIVERYDATE"] != null)
                                    {
                                        DateTime dtTemp = DateTime.Parse(dr["DELIVERYDATE"].ToString());
                                        if (dtTemp.Date < vdtStarDateTime.Date)
                                        {
                                            drNew_T_QC_QCFP["QCORANK"] = -1 * (Convert.ToInt32(dr["RANGKING"].ToString()) + 1 + Math.Abs(intNegativeRank));
                                            drNew_T_QC_QCFP["CHANGEQCORANK"] = -1 * (Convert.ToInt32(dr["RANGKING"].ToString()) + 1 + Math.Abs(intNegativeRank));
                                        }
                                        else
                                        {
                                            intI += 1;
                                            drNew_T_QC_QCFP["QCORANK"] = intI;
                                            drNew_T_QC_QCFP["CHANGEQCORANK"] = intI;
                                        }
                                        //2019-04-27 Tai Le (Thomas)
                                        intQCORANKINGNEW += 1;
                                        drNew_T_QC_QCFP["QCORANKINGNEW"] = intQCORANKINGNEW;
                                    }
                                    drNew_T_QC_QCFP["FACTORY"] = dr["FACTORY"];
                                    drNew_T_QC_QCFP["LINENO"] = dr["LINENO"];
                                    drNew_T_QC_QCFP["AONO"] = dr["AONO"];
                                    drNew_T_QC_QCFP["STYLECODE"] = dr["STYLECODE"];
                                    drNew_T_QC_QCFP["STYLESIZE"] = dr["STYLESIZE"];
                                    drNew_T_QC_QCFP["STYLECOLORSERIAL"] = dr["STYLECOLORSERIAL"];
                                    drNew_T_QC_QCFP["REVNO"] = dr["REVNO"];
                                    drNew_T_QC_QCFP["PRDPKG"] = dr["PRDPKG"];
                                    /*2019-04-22 Tai Le(Thomas): Add 5 Original Data from AOMTOPS Package {PLANQTY, DELIVERYDATE, PRDSDAT, PRDEDAT, ORDQTY} */
                                    drNew_T_QC_QCFP["PLANQTY"] = dr["PLANQTY"];
                                    //2019-11-15 Tai Le (Thomas): Add the Remain Package Qty for Capacity Calculation
                                    drNew_T_QC_QCFP["PLANQTYBAL"] = dr["PLANQTYBAL"];
                                    drNew_T_QC_QCFP["DELIVERYDATE"] = dr["DELIVERYDATE"];
                                    drNew_T_QC_QCFP["PRDSDAT"] = dr["PRDSDAT"];
                                    drNew_T_QC_QCFP["PRDEDAT"] = dr["PRDEDAT"];
                                    drNew_T_QC_QCFP["ORDQTY"] = dr["ORDQTY"];
                                    drNew_T_QC_QCFP["NORMALIZEDPERCENT"] = dr["MATNORNALRATE"];
                                    drNew_T_QC_QCFP["CREATEDATE"] = DateTime.Now;
                                    drNew_T_QC_QCFP["LATESTQCOTIME"] = DateTime.Now; //2019-06-18 Tai Le (Thomas)
                                    //2019-11-15 Tai Le (Thomas): Handle the QCO Version 
                                    if (!String.IsNullOrEmpty(strQCOVersion))
                                        drNew_T_QC_QCFP["QCOVERSION"] = strQCOVersion;
                                    ///2019-12-12 Tai Le (Thomas): Add Material Priority Rate
                                    /// Priority Rule :   Level 3 > Level 2 > Level 1
                                    /// MATPRIORITYA  represent for Level_3 
                                    ///     MATPRIORITYB  represent for Level_2
                                    ///         MATPRIORITYC  represent for Level_1
                                    drNew_T_QC_QCFP["MATPRIORITYA"] = dr["MATPRIORITYLEV3"];
                                    drNew_T_QC_QCFP["MATPRIORITYB"] = dr["MATPRIORITYLEV2"];
                                    drNew_T_QC_QCFP["MATPRIORITYC"] = dr["MATPRIORITYLEV1"];
                                    ///:::END   2019-12-12 Tai Le (Thomas): Add Material Priority Rate
                                    //2020-01-11 Tai Le(Thomas): Previous Week QCO Ranking
                                    if (dr["PRECUSTOMRANK"] != null)
                                        drNew_T_QC_QCFP["PRECUSTOMRANK"] = dr["PRECUSTOMRANK"];
                                    //:::END    2020-01-11 Tai Le(Thomas)
                                    //2020-02-27 Tai Le(Thomas):  
                                    if (dr["MESALLOCATESTATUS"] != null)
                                        drNew_T_QC_QCFP["MESALLOCATESTATUS"] = dr["MESALLOCATESTATUS"];
                                    //:::END    2020-02-27 Tai Le(Thomas)
                                    //2020-02-29 Tai Le(Thomas):  
                                    if (dr["MESSTARTDATE"] != null)
                                        if (dr["MESALLOCATESTATUS"].ToString() != "Not Scheduled")
                                            drNew_T_QC_QCFP["MESSTARTDATE"] = dr["MESSTARTDATE"];
                                    //:::END    2020-02-29 Tai Le(Thomas)
                                    //2020-03-31 Tai Le(Thomas): 
                                    //Final Sample Order Readiness
                                    if (dr["FINSOREADINESS"] != null)
                                        drNew_T_QC_QCFP["FINSOREADINESS"] = dr["FINSOREADINESS"];
                                    //JIG Readiness
                                    if (dr["JIGREADINESS"] != null)
                                        drNew_T_QC_QCFP["JIGREADINESS"] = dr["JIGREADINESS"];
                                    var blStep_1 = false;
                                    //SOP Readiness
                                    if (dr["SOPREADINESS"] != null)
                                    {
                                        if (dr["SOPREADINESS"].ToString() == "1")
                                        {
                                            blStep_1 = true;
                                            drNew_T_QC_QCFP["SOPREADINESS"] = "1";
                                        }
                                    }
                                    if (!blStep_1)
                                    {
                                        if (dr["SOPREADINESS_DB"] == null)
                                            drNew_T_QC_QCFP["SOPREADINESS"] = "0";
                                        else
                                            drNew_T_QC_QCFP["SOPREADINESS"] = dr["SOPREADINESS_DB"].ToString() == "Y" ? "1" : "0";
                                    }
                                    //:::END    2020-03-31 Tai Le(Thomas)
                                    //2020-05-06 Tai Le(Thomas):  Previous Week QCO Start Date
                                    if (dr["QCOSTARTDATE"] != null)
                                        drNew_T_QC_QCFP["QCOSTARTDATE"] = dr["QCOSTARTDATE"];
                                    //:::END    2020-05-06 Tai Le(Thomas)

                                    //2020-12-16 Tai Le(Thomas): Cutting Readiness
                                    drNew_T_QC_QCFP["CUTTINGREADINESS"] = 0;

                                    dt_T_QC_QUEUE.Rows.Add(drNew_T_QC_QCFP);
                                }
                    }
                    if (dt_T_QC_QUEUE.Rows.Count > 0)
                    {
                        OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                        oracleDataAdapter.Update(dt_T_QC_QUEUE);
                        oracleCommandBuilder.Dispose();
                    }
                    dt_T_QC_QUEUE.Dispose();
                    oracleDataAdapter.Dispose();
                    oracleConnection.Close();
                    oracleConnection.Dispose();
                }
                return true;
            }
            catch (Exception ex)
            {
                var Msg = ex.Message + "; intI= " + intI;
                return false;
            }
        }
        public object SaveQCPSRanking(string data, string Reason)
        {
            //    string strMsg = "";
            //    bool blResult = false;
            //    //if (Session["LoginRole"].ToString() != "5000")
            //    if (false)
            //    {
            //        strMsg = "Not Authorized.<BR/>Please login with Role \"5000 - Production Director\" to proceed this Function.";
            //        goto HE_END;
            //    }
            //    try
            //    {
            //        //var Qcops = new JavaScriptSerializer().Deserialize<List<Qcops>>(data);
            //        var Qcops = JsonConvert.DeserializeObject<Qcops>(data);
            //        var Check_ = from item in Qcops where item.QCORANK < 0 select item;
            //        if (Check_.Count > 0)
            //        {
            //            strMsg = "Please DON'T Include Negative QCO_RANK.<BR/>Use \"QCO Display\" to Filter Positive Ranking.";
            //            goto HE_END;
            //        }
            //        if (Qcops.Count > 0)
            //        {
            //            List<Qcops> HandleList = new List<Qcops>();
            //            int intNewRanking = 0;
            //            var minQCORANKINGNEW = Qcops.Min(x => x.QCORANKINGNEW);
            //            var maxQCORANKINGNEW = Qcops.Max(x => x.QCORANKINGNEW);
            //            var minRANKING = Qcops.Min(x => x.RANKING);
            //            var maxRANKING = Qcops.Max(x => x.RANKING);
            //            int MAXQCORANKING = T_QC_QUEUE_MAXQCORANK(Qcops[0].QCOFACTORY, Qcops[0].QCOYEAR, Qcops[0].QCOWEEKNO);
            //            intNewRanking = minRANKING - 1;
            //            foreach (Qcops item in Qcops)
            //            {
            //                intNewRanking += 1;
            //                item.intNewRanking = intNewRanking;
            //                if (intNewRanking != item.RANKING)
            //                {
            //                    MAXQCORANKING += 1;
            //                    item.MAXQCORANKING = MAXQCORANKING;
            //                    HandleList.Add(item);
            //                }
            //            }
            //            foreach (Qcops item in HandleList)
            //            {
            //                /// Step 1: Update QCORanking = MAXQCORANKING  WHere QCORANKING = QCORANKINGNEW
            //                /// Retired
            //                UpdateQCORanking_1(item);
            //            }
            //            foreach (Qcops item in HandleList)
            //            {
            //                /// Step 2: Update QCORanking = intNewRanking ,
            //                ///             ChangeBy , ChangeOn
            //                ///             Reason
            //                ///             Where QCORANKING = MAXQCORANKING Debug.Print("intNewRanking= " + item.intNewRanking + "; QCORANKINGNEW = " + item.QCORANKINGNEW);
            //                var origPP = Qcops.Where(f => f.RANKING == item.intNewRanking).Select(n => n.QCORANK).First();
            //                UpdateQCORanking_2(item, origPP, Reason);
            //            }
            //            /// Step 3: 
            //            /// Update QCORANKINGNEW Based On Is_NULL(CHANGEQCORANK, QCORANK), 
            //            /// QCORANKINGNEW starts from 1 to n 
            //            UpdateQCORanking_3(HandleList[0]);
            //            blResult = true;
            //            strMsg = "Change-over Ranking Saved!";
            //        }
            //        else
            //        {
            //            strMsg = "No Data on 'T_QC_QCPS' to create QCO!";
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        strMsg = ex.Message;
            //    }
            //HE_END:
            return new { retResult = "", retMsg = "" };
        }
        public static QCOQueue GetLatestQCO(string pOracleCnnString, string pAONO, string pFactory, string pStyleCode, string pStyleSize, string pStyleColorSerial, string pRevNo, string pPRDPKG_ID)
        {
            QCOQueue objQCOQueue = new QCOQueue();
            using (OracleConnection OracleConn = new OracleConnection(pOracleCnnString))
            {
                OracleConn.Open();
                string strSQL =
                    " Select V_QCO_PP_LatestQCO.* , NVL(LATESTQCOTIME , CREATEDATE) as LATESTQCOTIMECONF , NVL(CHANGEQCORANK , QCORANK) as QCORANKONF  " +
                    " From PKMES.V_QCO_PP_LatestQCO " +
                    " Where AONO = :AONO " +
                    " AND FACTORY = :FACTORY " +
                    " AND STYLECODE = :STYLECODE " +
                    " AND STYLESIZE = :STYLESIZE " +
                    " AND STYLECOLORSERIAL = :STYLECOLORSERIAL " +
                    " AND REVNO = :REVNO " +
                    " AND PRDPKG = :PRDPKG " +
                    " ";
                DataTable dt = new DataTable();
                List<OracleParameter> parameters = new List<OracleParameter>();
                parameters.Add(new OracleParameter("AONO", pAONO));
                parameters.Add(new OracleParameter("FACTORY", pFactory));
                parameters.Add(new OracleParameter("STYLECODE", pStyleCode));
                parameters.Add(new OracleParameter("STYLESIZE", pStyleSize));
                parameters.Add(new OracleParameter("STYLECOLORSERIAL", pStyleColorSerial));
                parameters.Add(new OracleParameter("REVNO", pRevNo));
                parameters.Add(new OracleParameter("PRDPKG", pPRDPKG_ID));
                PCMOracleLibrary.StatementToDataTable(OracleConn, strSQL, parameters, out dt, out strSQL);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        objQCOQueue.AONO = pAONO;
                        objQCOQueue.FACTORY = pFactory;
                        objQCOQueue.STYLECODE = pStyleCode;
                        objQCOQueue.STYLESIZE = pStyleSize;
                        objQCOQueue.STYLECOLORSERIAL = pStyleColorSerial;
                        objQCOQueue.REVNO = pRevNo;
                        objQCOQueue.PRDPKG = pPRDPKG_ID;
                        objQCOQueue.NORMALIZEDPERCENT = dt.Rows[0]["NORMALIZEDPERCENT"].ToString();
                        objQCOQueue.LATESTQCOTIME = DateTime.Parse(dt.Rows[0]["LATESTQCOTIMECONF"].ToString());
                        objQCOQueue.QCOYEAR = Convert.ToInt32(dt.Rows[0]["QCOYEAR"].ToString());
                        objQCOQueue.QCOWEEKNO = dt.Rows[0]["QCOWEEKNO"].ToString();
                        objQCOQueue.QCORANK = dt.Rows[0]["QCORANKONF"].ToString();
                        objQCOQueue.LINENO = dt.Rows[0]["LINENO"].ToString();
                    }
                    dt.Dispose();
                }
                parameters.Clear();
                OracleConn.Close();
                OracleConn.Dispose();
            }
            return objQCOQueue;
        }
        public void CopySysQCOtoSIMQCO(string vstrQCOFactory, int vintQCOYear, string vstrWeekNo)
        {
            try
            {
                string[] arrTableNames = { "PKMES.T_QC_QCFP", "PKMES.T_QC_QCPM", "PKMES.T_QC_QCPS", "PKMES.T_QC_QUEUE" };
                //  Table T_QC_QCFPSIM  >  T_QC_QCFP 
                //  Table T_QC_QCPMSIM  >  T_QC_QCPM 
                //  Table T_QC_QCPSSIM  >  T_QC_QCPS 
                //  Table T_QC_QUEUESIM  >  T_QC_QUEUE
                using (OracleConnection oracleConn = new OracleConnection(_PKMESConnString))
                {
                    oracleConn.Open();
                    bool isOK = false;
                    var strSQL =
                        $" Select * " +
                        $" From PKMES.T_QC_QUEUE " +
                        $" WHERE QCOFACTORY = '{vstrQCOFactory}' AND QCOYEAR = { vintQCOYear} AND QCOWEEKNO = '{vstrWeekNo}'  ";
                    DataTable dt = new DataTable();
                    PCMOracleLibrary.StatementToDataTable(oracleConn, strSQL, null, out dt, out strSQL);
                    if (dt != null)
                    {
                        if (dt.Rows.Count > 0)
                            isOK = true;
                        dt.Dispose();
                    }
                    if (isOK)
                        for (int I = 0; I < arrTableNames.Length; I++)
                        {
                            var curTableName = arrTableNames[I];
                            //2020-02-06 Tai Le(Thomas): Clean Data 
                            strSQL =
                            $" DELETE {curTableName}SIM " +
                            $" WHERE QCOFACTORY = '{vstrQCOFactory}' AND QCOYEAR = { vintQCOYear} AND QCOWEEKNO = '{vstrWeekNo}' ";
                            using (OracleCommand command = new OracleCommand())
                            {
                                command.Connection = oracleConn;
                                command.CommandText = strSQL;
                                command.CommandType = CommandType.Text;
                                command.ExecuteNonQuery();
                            }
                            //Insert data
                            strSQL =
                                $" INSERT INTO {curTableName}SIM " +
                                $" SELECT * FROM {curTableName} " +
                                $" WHERE QCOFACTORY = '{vstrQCOFactory}' AND QCOYEAR = { vintQCOYear} AND QCOWEEKNO = '{vstrWeekNo}' ";
                            using (OracleCommand command = new OracleCommand())
                            {
                                command.Connection = oracleConn;
                                command.CommandText = strSQL;
                                command.CommandType = CommandType.Text;
                                command.ExecuteNonQuery();
                            }
                        }
                    oracleConn.Close();
                }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
            }
        }
        public async Task UpdateLastWeekCustomRanking(string pOracleCnnString, string pQCOFactory, int pQCOYear, string pWeekNo)
        {
            try
            {
                string sPreWeekNo = "";
                int PreYear = pQCOYear,
                    PreWeekNo = 0;
                if (pWeekNo == "W01")
                {
                    PreYear = PreYear - 1;
                    using (OracleConnection oracleCnn = new OracleConnection(pOracleCnnString))
                    {
                        oracleCnn.Open();
                        using (OracleCommand cmd = new OracleCommand())
                        {
                            cmd.Connection = oracleCnn;
                            cmd.CommandText = $"Select MAX(QCOWEEKNO) FROM T_QC_QUEUE WHERE QCOYEAR = '{PreYear}' AND QCOFACTORY = '{pQCOFactory}'  ";
                            var _reader = await cmd.ExecuteReaderAsync();
                            if (_reader.HasRows)
                            {
                                while (_reader.Read())
                                {
                                    sPreWeekNo = _reader[0].ToString();
                                }
                                _reader.Close();
                            }
                        }
                        oracleCnn.Close();
                    }
                }
                else
                {
                    if (this.WeekTextToNumber(pWeekNo, out PreWeekNo))
                    {
                        PreWeekNo = PreWeekNo - 1;
                        if (PreWeekNo < 10)
                            sPreWeekNo = "W0" + PreWeekNo.ToString();
                        else
                            sPreWeekNo = "W" + PreWeekNo.ToString();
                    }
                }
                if (!String.IsNullOrEmpty(sPreWeekNo))
                    await UpdateLastWeekCustomRankingSubAsync(pOracleCnnString, pQCOFactory, pQCOYear, pWeekNo, PreYear, sPreWeekNo);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
        public async Task UpdateLastWeekCustomRankingSubAsync(string pOracleCnnString, string pQCOFactory, int pQCOYear, string pWeekNo, int vPrevQCOYear, string pPrevWeekNo)
        {
            try
            {
                var SQL =
                    $" Update PKMES.T_QC_QUEUE " +
                    $" Set PRECUSTOMRANK = (Select nvl(B.CHANGEQCORANK, B.QCORANK) From PKMES.T_QC_QUEUE B Where B.PRDPKG = T_QC_QUEUE.PRDPKG And B.QCOFACTORY= '{pQCOFactory}' And QCOYEAR = {vPrevQCOYear} And QCOWEEKNO = '{pPrevWeekNo}' ) " +
                    $" WHERE QCOFACTORY = '{pQCOFactory}' " +
                    $" And QCOYEAR = {pQCOYear} " +
                    $" And QCOWEEKNO = '{pWeekNo}' " +
                    $" And PRECUSTOMRANK Is null ";
                using (OracleConnection oracleCnn = new OracleConnection(pOracleCnnString))
                {
                    oracleCnn.Open();
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        cmd.Connection = oracleCnn;
                        cmd.CommandText = SQL;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 180;
                        await cmd.ExecuteNonQueryAsync();
                    }
                    oracleCnn.Close();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
        public bool WeekTextToNumber(string pWeekNo, out int retWeek)
        {
            retWeek = 0;
            try
            {
                if (pWeekNo is string)
                {
                    if (pWeekNo.Contains("W"))
                    {
                        pWeekNo = pWeekNo.Substring(1, 2);
                        retWeek = Convert.ToInt32(pWeekNo);
                    }
                    else
                    {
                        var _ret = int.TryParse(pWeekNo, out retWeek);
                        Console.WriteLine(_ret);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region  QCO: NEW Material Distribution Logic (including PDNO)
        public async Task<string> QCOCalculationAllNew(string Factory, string UserID, string UserRole, string ServerPath, bool pCopyToRunningQCO)
        {
            try
            {
                //2019-12-19 Tai Le (Thomas)
                //Log.Logger = new LoggerConfiguration()
                //    .MinimumLevel.Debug()
                //    .WriteTo.File(System.IO.Path.Combine(ServerPath, $"Logs\\{DateTime.Today.ToString("yyyy-MM-dd")}.txt"), rollingInterval: RollingInterval.Day)
                //    .CreateLogger();
                StringBuilder sb = new StringBuilder();
                //string retMessage = "";
                using (OracleConnection oracleConnection = new OracleConnection(_PKMESConnString))
                {
                    oracleConnection.Open();
                    List<PCMQCOCalculation> Factories = new List<PCMQCOCalculation>();
                    //var strSQL = "Select FACTORY " +
                    //			 " From PKERP.T_CM_FCMT  " +
                    //			 " Where Type = 'P' " +
                    //			 " And Substr(Factory,1,1) = 'P' " +
                    //			 " And Status ='OK' " +
                    //			 " AND FACTORY like '" + Factory + "' " +
                    //			 " Order By Factory ";
                    //2020-06-06 Tai Le(Thomas)
                    var strSQL = "Select T_CM_FCMT.FACTORY " +
                                 " From PKERP.T_CM_FCMT  JOIN PKMES.T_QC_QCFR ON T_CM_FCMT.FACTORY = T_QC_QCFR.Factory " +
                                 " Where T_CM_FCMT.Type = 'P' " +
                                 " And Substr(T_CM_FCMT.Factory,1,1) = 'P' " +
                                 " And T_CM_FCMT.Status ='OK' " +
                                 " AND T_CM_FCMT.FACTORY like '" + Factory + "' " +
                                 " Order By CAST( (T_QC_QCFR.LASTCOMPLETEDATE - T_QC_QCFR.EXECUTINGDATE) as decimal(20,6)), T_CM_FCMT.Factory ";
                    DataTable dt = new DataTable();
                    PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, null, out dt, out strSQL);
                    if (dt != null)
                    {
                        if (dt.Rows.Count > 0)
                            foreach (DataRow dr in dt.Rows)
                            {
                                Factories.Add(new PCMQCOCalculation(dr["FACTORY"].ToString()));
                            }
                        dt.Dispose();
                    }
                    oracleConnection.Close();
                    oracleConnection.Dispose();
                    if (Factories.Count > 0)
                    {
                        //sb.Clear();
                        string _tempMsg = "";
                        for (int I = 0; I < Factories.Count; I++)
                        {
                            _tempMsg = "";
                            //sb.AppendLine("QCO in " + Factories[I].mFactory);
                            if (string.Equals(mEnviroment, "console", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine("================================");
                                Console.WriteLine("::QCO in " + Factories[I].mFactory);
                                Console.WriteLine("");
                            }
                            mFactory = Factories[I].mFactory;
                            mQCOSource = "QCO";

                            _tempMsg = await this.QCOCalculationNew(Factories[I].mFactory, String.IsNullOrEmpty(mUserID) ? UserID : mUserID, UserRole, ServerPath, false, "", "", "", "", "", "", pCopyToRunningQCO);
                            sb.AppendLine(_tempMsg);
                            Console.WriteLine($"Factory: {Factories[I].mFactory}: {_tempMsg}");
                        }
                    }
                }
                //return Task.FromResult(sb.ToString());
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        public async Task<string> QCOCalculationNew(string Factory, string UserID, string UserRole, string ServerPath, bool IsSinglePP, string pAONO, string pStyleCode, string pStyleSize, string pStyleColorSerial, string pRevNo, string pPRDPKD_ID, bool pCopyToRunningQCO)
        {
            /* 
			 * Purpose: Calculate the Material Readiness For MTOPS Packages In Chosen FACTORY
			 * Input:   Factory
			 * Output:  The MTOPS Package is ranked based on the Material Readiness
			 * 
			 * :: Pre-processing
			 *      1. Check the Factory QCO Running Status on Table  PKMES.T_QC_QCFR
			 *          1.1 If Status = "RUNNING"
			 *              >> Quit and Return the message "Factory is running by <EXECUTINGBY> at <EXECUTINGDATE>
			 *          1.2 If Status = "DONE"
			 *              >> Continue the Next Process
			 *      2. Check the Availability of Factory Sorting Parameters on table PKMES.T_00_QCFO
			 *          2.1 If No Factory Setting
			 *              >> Quit and Return the message "Please set up the QCO Sorting Parameters on Factory <Factory>"
			 *      3. Check, Whether QCO Factory + QCO Week   EXISTING 
			 * :: Processing
			 *      1. From View  "PKERP.VIEW_ERP_PSRSNP_PLAN", look for all the INCOMPLETE Packages
			 *      2. Save the satisfied Package into PKMES.T_QC_QCFP
			 *      3. Sort the PKMES.T_QC_QCFP based on The Factory Sorting Parameters on table PKMES.T_00_QCFO
			 *      4. Based on PKMES.T_QC_QCFP , PKERP.T_SD_BOMT ,  PKERP.V_WMS_PORC , Distribute the Received Qty for Each Package
			 *          4.1. PKMES.T_QC_QCFP  Join  PKERP.T_SD_BOMT together, and order by "Factory Sorting Parameters" , T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL
			 * :: Post-processing
			 */
            string retMessage = "";
            StringBuilder sb = new StringBuilder();
            if (String.IsNullOrEmpty(_PKERPConnString) || String.IsNullOrEmpty(_PKMESConnString))
            {
                sb.AppendLine("Connection Strings {PKERP, PKMES} Must Define.");
                retMessage = sb.ToString();
                return retMessage;
            }
            /* Prevent RUN QCO for Factory in case input "pAONO, pStyleCode, pStyleSize, pStyleColorSerial, pRevNo, pPRDPKD_ID" but IsSinglePP = FALSE */
            if (!String.IsNullOrEmpty(pAONO) && !String.IsNullOrEmpty(pStyleCode) && !String.IsNullOrEmpty(pStyleSize) && !String.IsNullOrEmpty(pStyleColorSerial) && !String.IsNullOrEmpty(pRevNo) && !String.IsNullOrEmpty(pPRDPKD_ID))
                IsSinglePP = true;
            bool isOK = true;
            string strSQL = "",
                strErrorMessage = "",
                strSQLWhere = "",
                strSQLWhereWO = ""
                ;
            bool blHasError = false;
            bool blUpdateQCOJobStatus = true;
            //::: Get WeekNo
            //2020-03-14 Tai Le(Thomas) change 36-hours to 2-days
            DateTime dtStarDateTime = DateTime.Now.AddDays(2); //DateTime.Now.AddHours(36);
                                                               //2019-06-14 Tai Le (Thomas) : Handle Single PP Material Readiness
                                                               //2019-06-14 Tai Le (Thomas): use  intYear to Replace "dtStarDateTime.Year", in this way, able to re-use for SinglePackage
            int QCOYear = dtStarDateTime.Year;
            if (!String.IsNullOrEmpty(mEnviroment) && string.Equals(mEnviroment, "console", StringComparison.OrdinalIgnoreCase))
                Console.WriteLine("dtStarDateTime= " + dtStarDateTime.ToString("s"));
            sb.AppendLine($"dtStarDateTime= {dtStarDateTime.ToString("s")}");

            CultureInfo cul = CultureInfo.CurrentCulture;
            int weekNum = cul.Calendar.GetWeekOfYear(dtStarDateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            string strWeekNum = "W" + PCMGeneralFunctions.GetRight("00" + weekNum, 2);
            /*2019-12-31 Tai Le (Thomas) Handle Year & Week when crossing to next year*/
            //Get Monday of Each Week
            var dtMonday = dtStarDateTime.StartOfWeek(DayOfWeek.Monday);
            var dtWed = dtMonday.AddDays(2);
            if (dtWed.Year > dtStarDateTime.Year)
                strWeekNum = "W01";
            QCOYear = dtWed.Year;
            /*:::END    2019-12-31 Tai Le (Thomas) Handle Year & Week when crossing to next year*/

            //2020-10-07 Tai Le(Thomas):Update Capa
            //2020-10-07 Tai Le(Thomas): Update the Working time for 2 months further
            UpdateWeeklyCapacityFromMTOPS("System", Factory, QCOYear.ToString(), strWeekNum, strWeekNum);

            /* 2020-01-11 Tai Le(Thomas): Define Previous Week */
            int PreYear = 0;
            string strPreWeekNum = "";
            if (strWeekNum == "W01")
            {
                PreYear = QCOYear - 1;
                strPreWeekNum = GetYearMaxWeekNum(Factory, PreYear);
            }
            else
            {
                PreYear = QCOYear;
                strPreWeekNum = "W" + PCMGeneralFunctions.GetRight("00" + (weekNum - 1), 2);
            }
            //retMessage += $"Passed: Defined the Year[{QCOYear}] + Week[{weekNum}] Step; ";
            sb.AppendLine($"Passed: Defined the Year[{QCOYear}], Week[{weekNum}], PreYear[{PreYear}], PreWeek[{strPreWeekNum}]");
            /*:::END    2020-01-11 Tai Le (Thomas) Handle Year & Week when crossing to next year*/
            if (UserRole == null)
            {
                isOK = false;
                strErrorMessage = "Can not find User Role to Calculate QCO Ranking.";
                sb.AppendLine($"{strErrorMessage}");
                goto HE_Exit_QCOCalculate;
            }
            if (UserRole != "5000" && IsSinglePP == false)
            {
                isOK = false;
                strErrorMessage = "Wrong Role to Calculate QCO Ranking. Please log in with Role 5000.";
                sb.AppendLine($"{strErrorMessage}");
                goto HE_Exit_QCOCalculate;
            }
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(_PKMESConnString))
                {
                    oracleConnection.Open();
                    //::: Pre - processing
                    //* 1.1 If Status = "RUNNING"
                    //*     >> Quit and Return the message "Factory is running by <EXECUTINGBY> at <EXECUTINGDATE> 
                    strSQL = " SELECT EXECUTINGBY , EXECUTINGDATE " +
                                    " FROM PKMES.T_QC_QCFR " +
                                    " WHERE FACTORY = :FACTORY " +
                                    " AND STATUS IS NOT NULL " +
                                    " AND STATUS = 'RUNNING' ";
                    DataTable dt = new DataTable();
                    PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, new List<OracleParameter> { new OracleParameter("FACTORY", Factory) }, out dt, out strErrorMessage);
                    if (dt != null)
                        if (dt.Rows.Count > 0)
                        {
                            var ExecuteDate = dt.Rows[0][1].ToString();
                            DateTime dtExecuteDate = DateTime.Parse(ExecuteDate);
                            /* 2019/02/26: Tai Le (Thomas) modify the Rule
							 * Nếu Hiện Tại (Now) <= Execute +1 Day >> coi như process QCO đang chạy; không cho phép chạy QCO Calculation mới
							 * Nếu Hiện Tại (Now) > Execute +1 Day >> coi như process QCO Expired; cho phép chạy QCO Calculation mới 
							 */
                            if (DateTime.Now <= dtExecuteDate.AddDays(1))
                            {
                                strErrorMessage = "Factory '" + Factory + "' Being Run QCO Calculation By " + dt.Rows[0][0] + " at " + dt.Rows[0][1];
                                sb.AppendLine($"{strErrorMessage}");
                                blHasError = true;
                            }
                            dt.Dispose();
                        }
                    if (blHasError && IsSinglePP == false)
                    {
                        blUpdateQCOJobStatus = false;
                        goto HE_QCOCalculate_Complete;
                    }
                    if (!String.IsNullOrEmpty(mEnviroment))
                        if (mEnviroment.ToLower() == "console")
                            Console.WriteLine("check [T_QC_QCFR]: PASSED");
                    sb.AppendLine($"check [T_QC_QCFR]: PASSED");
                    //* 2. Check the Availability of Factory Sorting Parameters on table PKMES.T_00_QCFO
                    //* 2.1 If No Factory Setting
                    //*     >> Quit and Return the message "Please set up the QCO Sorting Parameters on Factory <Factory>"
                    strSQL = " SELECT * " +
                             " FROM PKMES.T_00_QCFO " +
                             " WHERE FACTORY = :FACTORY ";
                    if (dt == null)
                        dt = new DataTable();
                    PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, new List<OracleParameter> { new OracleParameter("FACTORY", Factory) }, out dt, out strSQL);
                    if (dt != null)
                        if (dt.Rows.Count == 0)
                        {
                            strErrorMessage = "Factory QCO Sorting Setting On '" + Factory + "' Empty. Please Set Up QCO Setting.";
                            sb.AppendLine($"{strErrorMessage}");
                            blHasError = true;
                            dt.Dispose();
                        }
                    if (blHasError)
                    {
                        goto HE_QCOCalculate_Complete;
                    }
                    if (!String.IsNullOrEmpty(mEnviroment))
                        if (mEnviroment.ToLower() == "console")
                            Console.WriteLine("check [Factory QCO Sorting Setting]: PASSED");
                    sb.AppendLine($"check [Factory QCO Sorting Setting]: PASSED");
                    //* 3. Check, Whether QCO Factory + QCO Week   EXISTING  
                    strSQL = " SELECT * " +
                             " FROM PKMES.T_QC_QUEUE " +
                             " WHERE QCOFACTORY = :FACTORY " +
                             " AND QCOYEAR = :YEAR " +
                             " AND QCOWEEKNO = :WEEK " +
                             " AND rownum <=2 ";
                    if (dt == null)
                        dt = new DataTable();
                    PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, new List<OracleParameter> { new OracleParameter("FACTORY", Factory), new OracleParameter("YEAR", QCOYear), new OracleParameter("WEEK", strWeekNum) }, out dt, out strSQL);
                    if (dt != null)
                        if (dt.Rows.Count > 0)
                        {
                            strErrorMessage = $@"QCO On Factory [{ Factory}] in Year [{QCOYear}] at Week No [{strWeekNum}] already EXIST. After Saturday 12:00 PM, QCO Calculation is Ready for Next Week No.";
                            sb.AppendLine($"{strErrorMessage}");
                            blHasError = true;
                            dt.Dispose();
                        }
                    if (blHasError && IsSinglePP == false)
                    {
                        goto HE_QCOCalculate_Complete;
                    }
                    if (!String.IsNullOrEmpty(mEnviroment))
                        if (mEnviroment.ToLower() == "console")
                            Console.WriteLine("check [T_QC_QUEUE]: PASSED");
                    sb.AppendLine($"check [T_QC_QUEUE]: PASSED");
                    /*::Processing
					 * 1. From View  "PKERP.VIEW_ERP_PSRSNP_PLAN", look for all the INCOMPLETE Packages
					 * 2. Save the satisfied Package into PKMES.T_QC_QCFP
					 * 3. Sort the PKMES.T_QC_QCFP based on The Factory Sorting Parameters on table PKMES.T_00_QCFO
					 * 4. Based on PKMES.T_QC_QCFP , PKERP.T_SD_BOMT ,  PKERP.V_WMS_PORC , Distribute the Received Qty for Each Package
					 *      4.1. PKMES.T_QC_QCFP  Join  PKERP.T_SD_BOMT together, and order by "Factory Sorting Parameters" , T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL
					 */
                    if (mEnviroment.ToLower() == "console")
                        Console.WriteLine("Start Re-make QCO For Factory= " + Factory + ", Year= " + QCOYear + ",Week= " + strWeekNum);
                    sb.AppendLine($"Start Re-make QCO For Factory={Factory}, Year={QCOYear},Week={strWeekNum} ");
                    //2019-11-25 Tai Le (Thomas)
                    var WeekMonday = PCMGeneralFunctions.GetDateFromWeekNumberAndDayOfWeek(QCOYear, weekNum, 0).Date;
                    /* 2019-11-04 Tai Le (Thomas): move the Delete codes as Functions */
                    isOK = DeleteQCOData(Factory, QCOYear, strWeekNum, IsSinglePP).Result;
                    if (isOK)
                    {
                        if (!String.IsNullOrEmpty(mEnviroment))
                            if (mEnviroment.ToLower() == "console")
                                Console.WriteLine("DeleteQCOData(): PASSED");
                        sb.AppendLine($"DeleteQCOData(): PASSED");
                    }
                    else
                    {
                        blHasError = true;
                        sb.AppendLine($"DeleteQCOData(): FAIL");
                    }
                    //::: Insert the Flag to mark the Running QCO Factory
                    if (IsSinglePP == false)
                        isOK = Insert_T_QC_QCFR(Factory, UserID, "Executing").Result;
                    if (isOK)
                    {
                        if (!String.IsNullOrEmpty(mEnviroment))
                            if (mEnviroment.ToLower() == "console")
                                Console.WriteLine("Insert_T_QC_QCFR(): PASSED");
                        sb.AppendLine($"Insert_T_QC_QCFR(): PASSED");
                    }
                    else
                    {
                        blHasError = true;
                        sb.AppendLine($"Insert_T_QC_QCFR(): FAIL");
                    }
                    if (blHasError && IsSinglePP == false)
                    {
                        goto HE_QCOCalculate_Complete;
                    }
                    //::: Get Factory QCO Setting:
                    List<Qcfo> LcAllFactoryParameters = null;
                    List<Qcfo> LcNoMateialFactoryParameters = null;
                    FactoryHasMaterialParameter(Factory, out LcAllFactoryParameters, out LcNoMateialFactoryParameters);
                    //::: Get MTOPS Package From  Chosen Factory 
                    var _tempMsg = "";
                    if (IsSinglePP == false)
                        dt = GetMTOPSPackage(Factory, out _tempMsg, PreYear, strPreWeekNum);
                    if (!String.IsNullOrEmpty(mEnviroment))
                        if (mEnviroment.ToLower() == "console")
                            Console.WriteLine("GetMTOPSPackage(): PASSED");
                    if (dt == null)
                    {
                        strErrorMessage = "No AO-MTOPS Package Found Factory [" + Factory + "].";
                        sb.AppendLine($"{strErrorMessage}");
                        blHasError = true;
                    }
                    else if (dt.Rows.Count == 0)
                    {
                        isOK = false;
                        strErrorMessage = "No AO-MTOPS Package Found Factory [" + Factory + "].";
                        sb.AppendLine($"{strErrorMessage}");
                        blHasError = true;
                    }
                    if (blHasError && IsSinglePP == false)
                    {
                        goto HE_QCOCalculate_Complete;
                    }
                    //::: Sort MTOPS Package with Parameter Before Material Readiness
                    if (IsSinglePP == false)
                        Sort_T_QC_QCFP(ref dt, LcNoMateialFactoryParameters, "First");
                    if (!String.IsNullOrEmpty(mEnviroment))
                        if (mEnviroment.ToLower() == "console")
                            Console.WriteLine("Sort_T_QC_QCFP(): PASSED");
                    sb.AppendLine($"Sort_T_QC_QCFP(): PASSED");
                    //::: Save Sorted PP Into PKMES.T_QC_QCFP
                    if (IsSinglePP == false)
                    {
                        //Save_T_QC_QCFP(OracleConnectionString, Factory, dtStarDateTime, QCOYear, strWeekNum, dt);
                        Save_T_QC_QCFP(Factory, WeekMonday, QCOYear, strWeekNum, dt);
                    }
                    if (!String.IsNullOrEmpty(mEnviroment))
                        if (mEnviroment.ToLower() == "console")
                            Console.WriteLine("Save_T_QC_QCFP(): PASSED");
                    sb.AppendLine($"Save_T_QC_QCFP(): PASSED");
                    if (dt != null)
                    {
                        dt.Clear();
                        dt.Dispose();
                    }
                    if (IsSinglePP)
                    {
                        //2019-06-14 Tai Le (Thomas): Handle Update Material Readiness For Single MTOPS Production Package 
                        //Get the Lastest QCO WEEK; QCO YEAR 
                        strSQL =
                            " SELECT * FROM PKMES.T_QC_QCFP " +
                            " WHERE FACTORY = '" + Factory + "' " +
                            " AND AONO = '" + pAONO + "'  " +
                            " AND STYLECODE = '" + pStyleCode + "' " +
                            " AND STYLESIZE = '" + pStyleSize + "' " +
                            " AND STYLECOLORSERIAL = '" + pStyleColorSerial + "' " +
                            " AND REVNO = '" + pRevNo + "' " +
                            " AND PRDPKG = '" + pPRDPKD_ID + "' " +
                            " ORDER BY QCOYEAR DESC , QCOWEEKNO DESC  ";
                        DataTable dt_Temp = new DataTable();
                        PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, null, out dt_Temp, out strSQL);
                        if (dt_Temp != null)
                        {
                            if (dt_Temp.Rows.Count > 0)
                            {
                                strWeekNum = dt_Temp.Rows[0]["QCOWEEKNO"].ToString();
                                QCOYear = Convert.ToInt32(dt_Temp.Rows[0]["QCOYEAR"].ToString());
                            }
                            dt_Temp.Clear();
                            dt_Temp.Dispose();
                        }
                        //Get All WO
                        strSQL =
                            " Select WONO " +
                            "   From PKMES.V_MRP_PP_WO " +
                            "   Where FACTORY = '" + Factory + "' " +
                            "   And AONO = '" + pAONO + "'  " +
                            "   And STLCD = '" + pStyleCode + "' " +
                            "   And STLSIZ = '" + pStyleSize + "' " +
                            "   And STLCOSN = '" + pStyleColorSerial + "' " +
                            "   And STLREVN = '" + pRevNo + "' " +
                            "   And PRODPACKAGE = '" + pPRDPKD_ID + "' " +
                            "   Group By WONO " +
                            "  ";
                        dt_Temp = new DataTable();
                        PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, null, out dt_Temp, out strSQL);
                        if (dt_Temp != null)
                        {
                            if (dt_Temp.Rows.Count > 0)
                            {
                                strSQLWhereWO += "( ";
                                int intI_ = 0;
                                for (intI_ = 0; intI_ < dt_Temp.Rows.Count; intI_++)
                                {
                                    if (intI_ == dt_Temp.Rows.Count - 1)
                                        strSQLWhereWO += " '" + dt_Temp.Rows[intI_]["WONO"] + "' ";
                                    else
                                        strSQLWhereWO += " '" + dt_Temp.Rows[intI_]["WONO"] + "', ";
                                }
                                strSQLWhereWO += " )";
                            }
                            dt_Temp.Clear();
                            dt_Temp.Dispose();
                        }
                        //Get All the PP having same WO with SELECTED PRDPKG
                        strSQL =
                            " SELECT AONO, FACTORY , STLCD , STLSIZ , STLCOSN , STLREVN , PRODPACKAGE " +
                            " FROM PKMES.V_MRP_PP_WO " +
                            " WHERE WONO IN " +
                            " ( Select WONO " +
                            "   From PKMES.V_MRP_PP_WO " +
                            "   Where FACTORY = '" + Factory + "' " +
                            "   And AONO = '" + pAONO + "'  " +
                            "   And STLCD = '" + pStyleCode + "' " +
                            "   And STLSIZ = '" + pStyleSize + "' " +
                            "   And STLCOSN = '" + pStyleColorSerial + "' " +
                            "   And STLREVN = '" + pRevNo + "' " +
                            "   And PRODPACKAGE = '" + pPRDPKD_ID + "' " +
                            "   Group By WONO " +
                            " ) " +
                            " GROUP BY AONO, FACTORY , STLCD , STLSIZ , STLCOSN , STLREVN , PRODPACKAGE " +
                            "  ";
                        dt_Temp = new DataTable();
                        PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, null, out dt_Temp, out strSQL);
                        if (dt_Temp != null)
                        {
                            if (dt_Temp.Rows.Count > 0)
                            {
                                strSQLWhere += "( ";
                                int intI_ = 0;
                                for (intI_ = 0; intI_ < dt_Temp.Rows.Count; intI_++)
                                {
                                    if (intI_ == dt_Temp.Rows.Count - 1)
                                        strSQLWhere += " '" + dt_Temp.Rows[intI_]["AONO"] + dt_Temp.Rows[intI_]["FACTORY"] + dt_Temp.Rows[intI_]["STLCD"] + dt_Temp.Rows[intI_]["STLSIZ"] + dt_Temp.Rows[intI_]["STLCOSN"] + dt_Temp.Rows[intI_]["STLREVN"] + dt_Temp.Rows[intI_]["PRODPACKAGE"] + "' ";
                                    else
                                        strSQLWhere += " '" + dt_Temp.Rows[intI_]["AONO"] + dt_Temp.Rows[intI_]["FACTORY"] + dt_Temp.Rows[intI_]["STLCD"] + dt_Temp.Rows[intI_]["STLSIZ"] + dt_Temp.Rows[intI_]["STLCOSN"] + dt_Temp.Rows[intI_]["STLREVN"] + dt_Temp.Rows[intI_]["PRODPACKAGE"] + "', ";
                                }
                                strSQLWhere += " )";
                            }
                            dt_Temp.Clear();
                            dt_Temp.Dispose();
                        }
                    }
                    //2019-12-02 Tai Le(Thomas) add 3 columns {STAR_LEV3, STAR_LEV2, STAR_LEV1}
                    //2020-04-04 Tai Le(Thomas) add Table t_sd_file, t_Sd_sopm to consider the SOP Check
                    //2020-06-15 Tai Le(Thomas): modify the Query
                    //2020-08-11 Tai Le(Thomas): big change to prevent duplicate row
                    //                    strSQL = " SELECT ROW_NUMBER() OVER(PARTITION BY T_QC_QCFP.FACTORY ORDER BY T_QC_QCFP.FACTORY, T_QC_QCFP.DELIVERYDATE , T_QC_QCFP.ORDQTY ,  T_QC_QCFP.PLANQTY , " +
                    //                             " T_QC_QCFP.AONO , T_QC_QCFP.STYLECODE , T_QC_QCFP.STYLESIZE , T_QC_QCFP.STYLECOLORSERIAL , T_QC_QCFP.REVNO , T_QC_QCFP.PRDPKG ) AS RowSeqNo , " +
                    //                             " LEAD(T_QC_QCFP.ID, 1, '') OVER (ORDER BY T_QC_QCFP.ID) AS NEXT_ID , " +
                    //                             " T_QC_QCFP.* , V_MRP_PP_WO.WONO , " +
                    //                             " T_SD_BOMT.MAINITEMCODE , T_SD_BOMT.MAINITEMCOLORSERIAL , " +
                    //                             " T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL , " +
                    //                             " T_QC_QCFP.PLANQTY * T_SD_BOMT.UNITCONSUMPTION AS REQUESTQTY , " +
                    //                             " LEAD(T_QC_QCFP.ID, 1, 0) OVER (ORDER BY T_QC_QCFP.ID) AS NEXT_ID , " +
                    //                             " T_00_ICMT.MAINLEVEL , " +
                    //                             " CASE WHEN T_00_ICMT.MAINLEVEL IN ('FAB','LTR') THEN 'Y' ELSE NULL END as STAR_LEV3 , " +
                    //                             " CASE WHEN T_00_ICMT.MAINLEVEL IN ('BST','LBL', 'MTL') THEN 'Y' ELSE NULL END as STAR_LEV2 , " +
                    //                             " CASE WHEN T_00_ICMT.MAINLEVEL NOT IN ('FAB','LTR','BST','LBL', 'MTL') THEN 'Y' ELSE NULL END as STAR_LEV1 , " +
                    //" CASE WHEN A1.STYLECODE IS NOT NULL THEN 'Y' " +
                    //" ELSE  " +
                    //"     CASE WHEN A2.STYLECODE IS NOT NULL THEN 'Y' " +
                    //"     ELSE  " +
                    //"         CASE WHEN A3.STYLECODE IS NOT NULL THEN 'Y' " +
                    //"         ELSE  " +
                    //"             CASE WHEN A4.STYLECODE IS NOT NULL THEN 'Y' " +
                    //"             ELSE  " +
                    //"                 CASE WHEN B1.STYLECODE IS NOT NULL THEN 'Y' " +
                    //"                 ELSE  " +
                    //"                     CASE WHEN B2.STYLECODE IS NOT NULL THEN 'Y' " +
                    //"                     ELSE  " +
                    //"                         CASE WHEN B3.STYLECODE IS NOT NULL THEN 'Y' " +
                    //"                         ELSE  " +
                    //"                             CASE WHEN B4.STYLECODE IS NOT NULL THEN 'Y' " +
                    //"                             ELSE 'N' " +
                    //"                             END " +
                    //"                         END " +
                    //"                     END " +
                    //"                 END " +
                    //"             END " +
                    //"         END " +
                    //"     END  " +
                    //" END SOPREADINESS_DB " +
                    //                             " FROM PKMES.T_QC_QCFP " +
                    //                             " LEFT JOIN PKMES.V_MRP_PP_WO ON " +
                    //                             "      T_QC_QCFP.STYLECODE = V_MRP_PP_WO.STLCD " +
                    //                             "      AND T_QC_QCFP.STYLESIZE = V_MRP_PP_WO.STLSIZ " +
                    //                             "      AND T_QC_QCFP.STYLECOLORSERIAL = V_MRP_PP_WO.STLCOSN " +
                    //                             "      AND T_QC_QCFP.REVNO = V_MRP_PP_WO.STLREVN " +
                    //                             "      AND T_QC_QCFP.PRDPKG = V_MRP_PP_WO.PRODPACKAGE " +
                    //                             " INNER JOIN PKERP.T_SD_BOMT ON " +
                    //                             "      T_QC_QCFP.STYLECODE = T_SD_BOMT.STYLECODE " +
                    //                             "      AND T_QC_QCFP.STYLESIZE = T_SD_BOMT.STYLESIZE " +
                    //                             "      AND T_QC_QCFP.STYLECOLORSERIAL = T_SD_BOMT.STYLECOLORSERIAL " +
                    //                             "      AND T_QC_QCFP.REVNO = T_SD_BOMT.REVNO " +
                    //                             " INNER JOIN PKERP.T_00_ICMT ON " +
                    //                             "      T_SD_BOMT.ITEMCODE = T_00_ICMT.ITEMCODE " +
                    //" LEFT JOIN  PKERP.t_sd_file A1 ON " +
                    //"     T_QC_QCFP.STYLECODE = A1.STYLECODE AND T_QC_QCFP.STYLESIZE = A1.STYLESIZE AND T_QC_QCFP.STYLECOLORSERIAL = A1.STYLECOLORSERIAL AND T_QC_QCFP.REVNO = A1.REVNO " +
                    //"     AND A1.uploadcode='012'  AND A1.confirmed=1 " +
                    //" LEFT JOIN  PKERP.t_sd_file A2 ON  " +
                    //"     T_QC_QCFP.STYLECODE = A2.STYLECODE AND T_QC_QCFP.STYLESIZE = A2.STYLESIZE AND T_QC_QCFP.STYLECOLORSERIAL = A2.STYLECOLORSERIAL AND '000' = A2.REVNO " +
                    //"     AND A2.uploadcode='012'  AND A2.confirmed=1 " +
                    //" LEFT JOIN  PKERP.t_sd_file A3 ON  " +
                    //"     T_QC_QCFP.STYLECODE = A3.STYLECODE AND T_QC_QCFP.STYLESIZE = A3.STYLESIZE AND '000' = A3.STYLECOLORSERIAL AND T_QC_QCFP.REVNO = A3.REVNO " +
                    //"     AND A3.uploadcode='012'  AND A3.confirmed=1 " +
                    //" LEFT JOIN  PKERP.t_sd_file A4 ON  " +
                    //"     T_QC_QCFP.STYLECODE = A4.STYLECODE AND T_QC_QCFP.STYLESIZE = A4.STYLESIZE AND '000' = A4.STYLECOLORSERIAL AND '000' = A4.REVNO " +
                    //"     AND A4.uploadcode='012'  AND A4.confirmed=1    " +
                    //" LEFT JOIN  PKERP.t_Sd_sopm B1 ON  " +
                    //"     T_QC_QCFP.STYLECODE = B1.STYLECODE AND T_QC_QCFP.STYLESIZE = B1.STYLESIZE AND T_QC_QCFP.STYLECOLORSERIAL = B1.STYLECOLORSERIAL AND T_QC_QCFP.REVNO = B1.REVNO " +
                    //"     AND B1.confirmed='Y' " +
                    //" LEFT JOIN  PKERP.t_Sd_sopm B2 ON  " +
                    //"     T_QC_QCFP.STYLECODE = B2.STYLECODE AND T_QC_QCFP.STYLESIZE = B2.STYLESIZE AND T_QC_QCFP.STYLECOLORSERIAL = B2.STYLECOLORSERIAL AND '000' = B2.REVNO " +
                    //"     AND B2.confirmed='Y' " +
                    //" LEFT JOIN  PKERP.t_Sd_sopm B3 ON  " +
                    //"     T_QC_QCFP.STYLECODE = B3.STYLECODE AND T_QC_QCFP.STYLESIZE = B3.STYLESIZE AND '000' = B3.STYLECOLORSERIAL AND T_QC_QCFP.REVNO = B3.REVNO " +
                    //"     AND B3.confirmed='Y' " +
                    //" LEFT JOIN  PKERP.t_Sd_sopm B4 ON  " +
                    //"     T_QC_QCFP.STYLECODE = B4.STYLECODE AND T_QC_QCFP.STYLESIZE = B4.STYLESIZE AND '000' = B4.STYLECOLORSERIAL AND '000' = B4.REVNO " +
                    //"     AND B4.confirmed='Y' " +
                    //                             " WHERE " +
                    //                             " T_QC_QCFP.QCOFACTORY = '" + Factory + "'  " +
                    //                             " AND T_QC_QCFP.QCOYEAR = " + QCOYear + " " +
                    //                             " AND T_QC_QCFP.QCOWEEKNO = '" + strWeekNum + "'  " +
                    //                             " AND (T_SD_BOMT.ITEMCODE NOT LIKE 'PKG%' AND T_SD_BOMT.ITEMCODE NOT LIKE 'TRE%' )  ";
                    //                    if (IsSinglePP)
                    //                        strSQL += " AND T_QC_QCFP.AONO || T_QC_QCFP.FACTORY || T_QC_QCFP.STYLECODE || T_QC_QCFP.STYLESIZE || T_QC_QCFP.STYLECOLORSERIAL || T_QC_QCFP.REVNO || T_QC_QCFP.PRDPKG IN " + strSQLWhere;
                    //                    strSQL += " ORDER BY T_QC_QCFP.ID , T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL ";
                    //2020-08-11 Tai Le(Thomas): big change to prevent duplicate row
                    strSQL = $@"
SELECT DISTINCT  T_QC_QCFP.* , V_MRP_PP_WO.WONO , 
                              T_SD_BOMT.MAINITEMCODE , T_SD_BOMT.MAINITEMCOLORSERIAL , 
                              T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL , 
                              T_QC_QCFP.PLANQTY * T_SD_BOMT.UNITCONSUMPTION AS REQUESTQTY , 
                              T_00_ICMT.MAINLEVEL , 
                              CASE WHEN T_00_ICMT.MAINLEVEL IN ('FAB','LTR') THEN 'Y' ELSE NULL END as STAR_LEV3 , 
                              CASE WHEN T_00_ICMT.MAINLEVEL IN ('BST','LBL', 'MTL') THEN 'Y' ELSE NULL END as STAR_LEV2 , 
                              CASE WHEN T_00_ICMT.MAINLEVEL NOT IN ('FAB','LTR','BST','LBL', 'MTL') THEN 'Y' ELSE NULL END as STAR_LEV1 , 
NVL( A1.STYLECODE, 
  nvl(A2.STYLECODE, 
    nvl(A3.STYLECODE, 
      nvl(A4.STYLECODE, 
        nvl(A4.STYLECODE,  
          nvl(A5.STYLECODE,  
            nvl(A6.STYLECODE,  
              nvl(A7.STYLECODE,  
                nvl(A8.STYLECODE, 
NVL( B1.STYLECODE, 
  nvl(B2.STYLECODE, 
    nvl(B3.STYLECODE, 
      nvl(B4.STYLECODE, 
        nvl(B4.STYLECODE,  
          nvl(B5.STYLECODE,  
            nvl(B6.STYLECODE,  
              nvl(B7.STYLECODE,  
                nvl(B8.STYLECODE, 
                  nvl(B51.STYLECODE,  
                    nvl(B61.STYLECODE,  
                      nvl(B71.STYLECODE,  
                        nvl(B81.STYLECODE, 
                        'N'
                        )
                      )
                    )
                  )                
                )
              )
            )
          )
        )
      )
    )
  )
)
                )
              )
            )
          )
        )
      )
    )
  )
) as SOPREADINESS_DB 
                              FROM PKMES.T_QC_QCFP 
                              LEFT JOIN PKMES.V_MRP_PP_WO ON 
                                   T_QC_QCFP.STYLECODE = V_MRP_PP_WO.STLCD 
                                   AND T_QC_QCFP.STYLESIZE = V_MRP_PP_WO.STLSIZ 
                                   AND T_QC_QCFP.STYLECOLORSERIAL = V_MRP_PP_WO.STLCOSN 
                                   AND T_QC_QCFP.REVNO = V_MRP_PP_WO.STLREVN 
                                   AND T_QC_QCFP.PRDPKG = V_MRP_PP_WO.PRODPACKAGE 
                              INNER JOIN PKERP.T_SD_BOMT ON 
                                   T_QC_QCFP.STYLECODE = T_SD_BOMT.STYLECODE 
                                   AND T_QC_QCFP.STYLESIZE = T_SD_BOMT.STYLESIZE 
                                   AND T_QC_QCFP.STYLECOLORSERIAL = T_SD_BOMT.STYLECOLORSERIAL 
                                   AND T_QC_QCFP.REVNO = T_SD_BOMT.REVNO 
                              INNER JOIN PKERP.T_00_ICMT ON 
                                   T_SD_BOMT.ITEMCODE = T_00_ICMT.ITEMCODE  
LEFT JOIN PKERP.V_SD_FILE A1 ON 
T_QC_QCFP.STYLECODE || T_QC_QCFP.STYLESIZE || T_QC_QCFP.STYLECOLORSERIAL || T_QC_QCFP.REVNO
= A1.STYLECODE || A1.STYLESIZE || A1.STYLECOLORSERIAL || A1.REVNO
LEFT JOIN PKERP.V_SD_FILE A2 ON 
T_QC_QCFP.STYLECODE || T_QC_QCFP.STYLESIZE || T_QC_QCFP.STYLECOLORSERIAL || '000'
= A2.STYLECODE || A2.STYLESIZE || A2.STYLECOLORSERIAL || A2.REVNO
LEFT JOIN PKERP.V_SD_FILE A3 ON  
T_QC_QCFP.STYLECODE || T_QC_QCFP.STYLESIZE || '000' || T_QC_QCFP.REVNO
= A3.STYLECODE || A3.STYLESIZE || A3.STYLECOLORSERIAL || A3.REVNO
LEFT JOIN PKERP.V_SD_FILE A4 ON 
T_QC_QCFP.STYLECODE || T_QC_QCFP.STYLESIZE || '000' || '000'
= A4.STYLECODE || A4.STYLESIZE || A4.STYLECOLORSERIAL || A4.REVNO
LEFT JOIN PKERP.V_SD_FILE A5 ON 
T_QC_QCFP.STYLECODE || '000' || T_QC_QCFP.STYLECOLORSERIAL || T_QC_QCFP.REVNO
= A5.STYLECODE || A5.STYLESIZE || A5.STYLECOLORSERIAL || A5.REVNO
LEFT JOIN PKERP.V_SD_FILE A6 ON 
T_QC_QCFP.STYLECODE || '000' || T_QC_QCFP.STYLECOLORSERIAL || '000'
= A6.STYLECODE || A6.STYLESIZE || A6.STYLECOLORSERIAL || A6.REVNO
LEFT JOIN PKERP.V_SD_FILE A7 ON 
T_QC_QCFP.STYLECODE || '000' || '000' || T_QC_QCFP.REVNO
= A7.STYLECODE || A7.STYLESIZE || A7.STYLECOLORSERIAL || A7.REVNO
LEFT JOIN PKERP.V_SD_FILE A8 ON 
T_QC_QCFP.STYLECODE || '000' || '000' || '000'
= A8.STYLECODE || A8.STYLESIZE || A8.STYLECOLORSERIAL || A8.REVNO
    LEFT JOIN PKERP.V_SD_FILE B1 ON 
    T_QC_QCFP.STYLECODE || T_QC_QCFP.STYLESIZE || T_QC_QCFP.STYLECOLORSERIAL || T_QC_QCFP.REVNO
    = B1.STYLECODE || B1.STYLESIZE || B1.STYLECOLORSERIAL || B1.REVNO
    LEFT JOIN PKERP.V_SD_FILE B2 ON 
    T_QC_QCFP.STYLECODE || T_QC_QCFP.STYLESIZE || T_QC_QCFP.STYLECOLORSERIAL || '000'
    = B2.STYLECODE || B2.STYLESIZE || B2.STYLECOLORSERIAL || B2.REVNO
    LEFT JOIN PKERP.V_SD_FILE B3 ON  
    T_QC_QCFP.STYLECODE || T_QC_QCFP.STYLESIZE || '000' || T_QC_QCFP.REVNO
    = B3.STYLECODE || B3.STYLESIZE || B3.STYLECOLORSERIAL || B3.REVNO
    LEFT JOIN PKERP.V_SD_FILE B4 ON 
    T_QC_QCFP.STYLECODE || T_QC_QCFP.STYLESIZE || '000' || '000'
    = B4.STYLECODE || B4.STYLESIZE || B4.STYLECOLORSERIAL || B4.REVNO
    LEFT JOIN PKERP.V_SD_FILE B5 ON 
    T_QC_QCFP.STYLECODE || '000' || T_QC_QCFP.STYLECOLORSERIAL || T_QC_QCFP.REVNO
    = B5.STYLECODE || B5.STYLESIZE || B5.STYLECOLORSERIAL || B5.REVNO
    LEFT JOIN PKERP.V_SD_FILE B6 ON 
    T_QC_QCFP.STYLECODE || '000' || T_QC_QCFP.STYLECOLORSERIAL || '000'
    = B6.STYLECODE || B6.STYLESIZE || B6.STYLECOLORSERIAL || B6.REVNO
    LEFT JOIN PKERP.V_SD_FILE B7 ON 
    T_QC_QCFP.STYLECODE || '000' || '000' || T_QC_QCFP.REVNO
    = B7.STYLECODE || B7.STYLESIZE || B7.STYLECOLORSERIAL || B7.REVNO
    LEFT JOIN PKERP.V_SD_FILE B8 ON 
    T_QC_QCFP.STYLECODE || '000' || '000' || '000'
    = B8.STYLECODE || B8.STYLESIZE || B8.STYLECOLORSERIAL || B8.REVNO
    LEFT JOIN PKERP.V_SD_FILE B51 ON 
    T_QC_QCFP.STYLECODE || 'all' || T_QC_QCFP.STYLECOLORSERIAL || T_QC_QCFP.REVNO
    = B51.STYLECODE || LOWER(B51.STYLESIZE) || B51.STYLECOLORSERIAL || B51.REVNO
    LEFT JOIN PKERP.V_SD_FILE B61 ON 
    T_QC_QCFP.STYLECODE || 'all' || T_QC_QCFP.STYLECOLORSERIAL || '000'
    = B61.STYLECODE || LOWER(B61.STYLESIZE) || B61.STYLECOLORSERIAL || B61.REVNO
    LEFT JOIN PKERP.V_SD_FILE B71 ON 
    T_QC_QCFP.STYLECODE || 'all' || '000' || T_QC_QCFP.REVNO
    = B71.STYLECODE || LOWER(B71.STYLESIZE) || B71.STYLECOLORSERIAL || B71.REVNO
    LEFT JOIN PKERP.V_SD_FILE B81 ON 
    T_QC_QCFP.STYLECODE || 'all' || '000' || '000'
    = B81.STYLECODE || LOWER(B81.STYLESIZE) || B81.STYLECOLORSERIAL || B81.REVNO    
                              WHERE 
                              T_QC_QCFP.QCOFACTORY = '{Factory}'  
                              AND T_QC_QCFP.QCOYEAR =  {QCOYear}
                              AND T_QC_QCFP.QCOWEEKNO = '{strWeekNum}'
                              AND (T_SD_BOMT.ITEMCODE NOT LIKE 'PKG%' AND T_SD_BOMT.ITEMCODE NOT LIKE 'TRE%' )
";
                    ///:: Main Query
                    strSQL = $@"
Select ROW_NUMBER() OVER(PARTITION BY FACTORY ORDER BY ID, DELIVERYDATE , PRDPKG , ITEMCODE , ITEMCOLORSERIAL ) AS RowSeqNo
 , Main.* 
 , LEAD(ID, 1, '') OVER (ORDER BY ID) AS NEXT_ID
From ({strSQL}) Main 
ORDER BY ID , ITEMCODE , ITEMCOLORSERIAL  
";
                    DataTable dt_QCFP = new DataTable();
                    PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, new List<OracleParameter> { new OracleParameter("FACTORY", Factory) }, out dt_QCFP, out strSQL, 3600);
                    //Add column ASSIGNEDQTY for Distribution Purpose.
                    DataColumn newColumn = new DataColumn("ASSIGNEDQTY", typeof(System.Double)) { DefaultValue = 0.0 };
                    dt_QCFP.Columns.Add(newColumn);
                    newColumn.Dispose();
                    //2019-12-11 Tai Le (Thomas) add 3 more columns for Material Rating
                    newColumn = new DataColumn("MATPRIORITYLEV3", typeof(System.Double)) { DefaultValue = 0.0 };
                    dt_QCFP.Columns.Add(newColumn);
                    newColumn.Dispose();
                    newColumn = new DataColumn("MATPRIORITYLEV2", typeof(System.Double)) { DefaultValue = 0.0 };
                    dt_QCFP.Columns.Add(newColumn);
                    newColumn.Dispose();
                    newColumn = new DataColumn("MATPRIORITYLEV1", typeof(System.Double)) { DefaultValue = 0.0 };
                    dt_QCFP.Columns.Add(newColumn);
                    newColumn.Dispose();
                    //::: END   2019-12-11
                    //::: Open T_QC_QCPM to Save the Material Distribution
                    //::: 2019-04-04: Tai Le (THOMAS) Add  QCOFACTORY, QCOYEAR, QCOWEEKNO To WHERE Syntax
                    //strSQL = " SELECT * " +
                    //         " FROM PKMES.T_QC_QCPM " +
                    //         " WHERE FACTORY = '" + Factory + "' ";
                    if (IsSinglePP)
                    {
                        //Delete PKMES.T_QC_QCPM From Related Packages in same WONO
                        strSQL =
                            " DELETE PKMES.T_QC_QCPM " +
                            " WHERE AONO || FACTORY || STYLECODE || STYLESIZE || STYLECOLORSERIAL || REVNO || PRDPKG IN " + strSQLWhere;
                        OracleCommand oracleCommand = new OracleCommand(strSQL, oracleConnection);
                        oracleCommand.CommandTimeout = 90;
                        await oracleCommand.ExecuteNonQueryAsync();
                    }
                    strSQL = " SELECT * " +
                             " FROM PKMES.T_QC_QCPM " +
                             " WHERE FACTORY = '" + Factory + "' " +
                             " AND QCOFACTORY = '" + Factory + "' " +
                             " AND QCOYEAR = " + QCOYear + " " +
                             " AND QCOWEEKNO = '" + strWeekNum + "' " +
                             "";
                    if (IsSinglePP)
                        strSQL += " AND AONO || FACTORY || STYLECODE || STYLESIZE || STYLECOLORSERIAL || REVNO || PRDPKG IN " + strSQLWhere;
                    DataTable dt_T_QC_QCPM = new DataTable();
                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
                    oracleDataAdapter.Fill(dt_T_QC_QCPM);
                    if (!String.IsNullOrEmpty(mEnviroment))
                        if (mEnviroment.ToLower() == "console")
                            Console.WriteLine("Starting Distribution: PASSED");
                    sb.AppendLine($"Starting Distribution: PASSED");
                    //::: DISTRIBUTE MATERIAL
                    var DistributeMaterialNewMSG = this.DistributeMaterialNew(IsSinglePP, Factory, QCOYear, strWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP);
                    sb.AppendLine($"DistributeMaterialNew(): {DistributeMaterialNewMSG}");
                    #region Replace the following code lines with above Function "DistributeMaterialNew()"
                    /** 
					//int intSeqNo = 0;
					////::: Distribute WMS Qty (Received Material Qty) 
					////::: WMS based on WONO (MRP_OLD data)
					//strSQL =
					//    " SELECT * " +
					//    " FROM PKERP.V_WO_RECWMS " +
					//    " WHERE " +
					//    " ( WO IN " +
					//    "   (Select V_MRP_PP_WO.WONO  " +
					//    "    From PKMES.T_QC_QCFP " +
					//    "    Inner Join PKMES.V_MRP_PP_WO On  " +
					//    "       T_QC_QCFP.FACTORY = V_MRP_PP_WO.FACTORY " +
					//    "       AND T_QC_QCFP.AONO = V_MRP_PP_WO.AONO " +
					//    "       AND T_QC_QCFP.STYLECODE = V_MRP_PP_WO.STLCD " +
					//    "       AND T_QC_QCFP.STYLESIZE = V_MRP_PP_WO.STLSIZ " +
					//    "       AND T_QC_QCFP.STYLECOLORSERIAL = V_MRP_PP_WO.STLCOSN " +
					//    "       AND T_QC_QCFP.REVNO = V_MRP_PP_WO.STLREVN " +
					//    "       AND T_QC_QCFP.PRDPKG = V_MRP_PP_WO.PRODPACKAGE " +
					//    "    WHERE T_QC_QCFP.QCOFACTORY = '" + Factory + "' AND T_QC_QCFP.QCOYEAR = " + QCOYear + " AND T_QC_QCFP.QCOWEEKNO = '" + strWeekNum + "' " +
					//    "    Group By V_MRP_PP_WO.WONO) " +
					//    " ) " +
					//    " ORDER BY WO , ITEM_CD , COLOR_SERIAL , PLAN_DOQTY ";
					//OracleCommand command = new OracleCommand(strSQL, oracleConnection);
					//var dr_WMS = command.ExecuteReader();
					//DistributeMaterial_T_QC_QCPM(Factory, QCOYear, strWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, dr_WMS, "W", ref intSeqNo);
					//dr_WMS.Close();
					//dr_WMS.Dispose();
					//command.Dispose();
					//if (mEnviroment == "Console")
					//    Console.WriteLine("DistributeMaterial_T_QC_QCPM() for W: PASSED");
					////::: Distribute WMS Qty (Received Material Qty) 
					////::: WMS based on AONO (MRP2 data)
					//strSQL =
					//    " SELECT * " +
					//    " FROM PKERP.V_WO_RECWMS " +
					//    " WHERE " +
					//    " WO LIKE 'AD%' " +
					//    " AND WO IN (Select AONO From PKMES.T_QC_QCFP  Where T_QC_QCFP.QCOFACTORY = '" + Factory + "' AND T_QC_QCFP.QCOYEAR = " + QCOYear + " AND T_QC_QCFP.QCOWEEKNO = '" + strWeekNum + "' Group By AONO) " +
					//    " ORDER BY WO , ITEM_CD , COLOR_SERIAL , PLAN_DOQTY ";
					//command = new OracleCommand(strSQL, oracleConnection);
					//var dr_WMS2 = command.ExecuteReader();
					//DistributeMaterial_T_QC_QCPM(Factory, QCOYear, strWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, dr_WMS2, "W2", ref intSeqNo);
					//dr_WMS.Close();
					//dr_WMS.Dispose();
					//command.Dispose();
					//if (mEnviroment == "Console")
					//    Console.WriteLine("DistributeMaterial_T_QC_QCPM() for W2: PASSED");
					////::: Distribute KMS Qty (Incoming Qty)
					////Get the Monday based on Year and WeekNo 
					////2019-06-15
					//if (IsSinglePP)
					//    weekNum = cul.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
					//DateTime dtMonday = PCMGeneralFunctions.GetDateFromWeekNumberAndDayOfWeek(QCOYear, weekNum, 0);
					//strSQL = " SELECT WO , ITEM_CD , COLOR_SERIAL , ETA , SUM(SHIP_QTY) PLAN_DOQTY " +
					//         " FROM KMS_PSRSHP_TBL@AOMTOPS " +
					//         " WHERE DELFLG = 'N' " +
					//         " AND ETA IS NOT NULL " +
					//         " AND Length(ETA) = 8 " +
					//         " AND ETA >= '" + dtMonday.ToString("yyyyMMdd") + "' " +
					//         " GROUP BY WO , ITEM_CD , COLOR_SERIAL , ETA  ";
					//command = new OracleCommand(strSQL, oracleConnection);
					//var dr_KMS = command.ExecuteReader();
					//DistributeMaterial_T_QC_QCPM(Factory, QCOYear, strWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, dr_KMS, "K", ref intSeqNo);
					//dr_KMS.Close();
					//dr_KMS.Dispose();
					//command.Dispose();
					//if (!String.IsNullOrEmpty(mEnviroment))
					//    if (mEnviroment.ToLower() == "console")
					//        Console.WriteLine("DistributeMaterial_T_QC_QCPM for K: PASSED");
					///*2019-11-01 Tai Le (Thomas) add part of KMS from MRP2* /
					//strSQL = " SELECT PRDPKG , ITEM_CD , COLOR_SERIAL , ETA , SUM(ORD_CNF_QTY) PLAN_DOQTY " +
					//         " FROM KMS_PSRSHP2_TBL@AOMTOPS " +
					//         " WHERE ETA IS NOT NULL " +
					//         " AND Length(ETA) = 8 " +
					//         " AND ETA >= '" + dtMonday.ToString("yyyyMMdd") + "' " +
					//         " GROUP BY PRDPKG , ITEM_CD , COLOR_SERIAL , ETA ";
					//command = new OracleCommand(strSQL, oracleConnection);
					//var dr_KMS2 = command.ExecuteReader();
					//DistributeMaterial_T_QC_QCPM(Factory, QCOYear, strWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, dr_KMS2, "K2", ref intSeqNo);
					//dr_KMS2.Close();
					//dr_KMS2.Dispose();
					//command.Dispose();
					//if (!String.IsNullOrEmpty(mEnviroment))
					//    if (mEnviroment.ToLower() == "console")
					//        Console.WriteLine("DistributeMaterial_T_QC_QCPM for K2: PASSED");
					///*::END  2019-11-01 Tai Le (Thomas) add part of KMS from MRP2* / 
					//:::END    DISTRIBUTE MATERIAL
					*/
                    #endregion
                    //::: Save T_QC_QCPM
                    OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                    oracleDataAdapter.Update(dt_T_QC_QCPM);
                    oracleCommandBuilder.Dispose();
                    if (!String.IsNullOrEmpty(mEnviroment))
                        if (mEnviroment.ToLower() == "console")
                            Console.WriteLine("Save & Commit [dt_T_QC_QCPM]: PASSED");
                    sb.AppendLine($"Save & Commit [dt_T_QC_QCPM]: PASSED");
                    //2019-12-07 Tai Le (Thomas) in this Logic, when using the Material Rating, no need to use "Material Readiness"
                    //::: Update the Material Readiness back to dt_QCFP based on dt_T_QC_QCPM  { QUANTITY_A ; REQUESTQTY }
                    Update_T_QC_QCFP_MaterialReadiness(ref dt_QCFP, dt_T_QC_QCPM);
                    dt_T_QC_QCPM.Dispose();
                    oracleDataAdapter.Dispose();
                    if (!String.IsNullOrEmpty(mEnviroment))
                        if (mEnviroment.ToLower() == "console")
                            Console.WriteLine("Update_T_QC_QCFP_MaterialReadiness(): PASSED");
                    sb.AppendLine($"Update_T_QC_QCFP_MaterialReadiness(): PASSED");
                    //2019-12-11 Tai Le (Thomas): Remove Repeat row which has  ["MATNORNALRATE"] = -1 
                    RemoveDuplicateRowQCFP(ref dt_QCFP);
                    if (!String.IsNullOrEmpty(mEnviroment))
                        if (mEnviroment.ToLower() == "console")
                            Console.WriteLine("RemoveDuplicateRowQCFP(): PASSED");
                    sb.AppendLine($"RemoveDuplicateRowQCFP(): PASSED");
                    //:::END    2019-12-11 Tai Le
                    //2019-12-11 Tai Le: Add
                    //Update MATPRIORITYLEV3 ; MATPRIORITYLEV2 ; MATPRIORITYLEV1
                    UpdateMaterialRateT_QC_QCFP(ref dt_QCFP, Factory, QCOYear, strWeekNum);
                    if (!String.IsNullOrEmpty(mEnviroment))
                        if (mEnviroment.ToLower() == "console")
                            Console.WriteLine("Update UpdateMaterialRate on [dt_QCFP]: PASSED");
                    sb.AppendLine($"UpdateMaterialRateT_QC_QCFP(): PASSED");
                    //:::END    2019-12-11 Tai Le
                    //::: Sort dt_QCFP with LcAllFactoryParameters
                    if (IsSinglePP == false)
                    {
                        Sort_T_QC_QCFP(ref dt_QCFP, LcAllFactoryParameters, "All");
                        sb.AppendLine($"Sort_T_QC_QCFP(): PASSED");
                    }
                    //::: SAVE dt_T_QC_QUEUE
                    if (IsSinglePP == false)
                    {
                        //PCMGeneralFunctions.GetDateFromWeekNumberAndDayOfWeek(QCOYear, weekNum, 0)
                        //Save_T_QC_QUEUE(OracleConnectionString, Factory, dtStarDateTime.Date, QCOYear, strWeekNum, dt_QCFP);
                        Save_T_QC_QUEUE(Factory, WeekMonday.Date, QCOYear, strWeekNum, dt_QCFP);
                        Update_CuttingReadiness(Factory, QCOYear, strWeekNum); //2020-12-16 Tai Le(Thomas)
                    }
                    else
                        Update_T_QC_QUEUE(Factory, QCOYear, strWeekNum, dt_QCFP);
                    dt_QCFP.Dispose();
                    if (!String.IsNullOrEmpty(mEnviroment))
                        if (mEnviroment.ToLower() == "console")
                            Console.WriteLine("Save/Update [T_QC_QUEUE]: PASSED");
                    sb.AppendLine($"Save/Update [T_QC_QUEUE]: PASSED");
                    //Update QCORANKINGNEW  
                    OracleCommand command = new OracleCommand("Update PKMES.T_QC_QUEUE SET QCORANKINGNEW = ROWNUM WHERE QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO ", oracleConnection);
                    command.Parameters.Add(new OracleParameter("QCOFACTORY", Factory));
                    command.Parameters.Add(new OracleParameter("QCOYEAR", QCOYear));
                    command.Parameters.Add(new OracleParameter("QCOWEEKNO", strWeekNum));
                    if (IsSinglePP == false)
                        command.ExecuteNonQuery();
                    command.Dispose();
                    if (!String.IsNullOrEmpty(mEnviroment))
                        if (mEnviroment.ToLower() == "console")
                            Console.WriteLine("Update QCORANKINGNEW on [T_QC_QUEUE]: PASSED");
                    sb.AppendLine($"Update QCORANKINGNEW on [T_QC_QUEUE]: PASSED");
                HE_QCOCalculate_Complete:
                    oracleConnection.Close();
                    oracleConnection.Dispose();
                }
            }
            catch (Exception ex)
            {
                blHasError = true;
                //strResult = JsonConvert.SerializeObject(new { retResult = false, retData = "", retMsg = ex.Message + "; SQL= " + strSQL });
                strErrorMessage = $"Factory [{Factory}]: {ex.Message}; sql= {strSQL}";
                sb.AppendLine($"{strErrorMessage}");
            }
            retMessage += strErrorMessage;
            if (!String.IsNullOrEmpty(retMessage))
            {
                //2019-12-07 Tai Le: send message to Telegram Group "PCMNotify"
                var botClient = new TelegramBotClient(TeleTokenID);
                botClient.SendTextMessageAsync(-1001407116473, retMessage).Wait();
            }
        HE_Exit_QCOCalculate:
            ////2020-02-10 Tai Le(Thomas) Bring last ween Custom Ranking to New QCO
            //this.UpdateLastWeekCustomRanking(OracleConnectionString, Factory, QCOYear, strWeekNum, PreYear, strPreWeekNum);
            //::: Complete the Flag
            if (blUpdateQCOJobStatus)
            {
                Complete_T_QC_QCFR(Factory, QCOYear, strWeekNum, UserID, strErrorMessage, blHasError).Wait();
            }
            if (!blHasError)
            {
                strErrorMessage = "QCO Ranking For Factory[" + Factory + "]; Year[" + QCOYear + "]; WONo [" + strWeekNum + "] : Built Success.";
                sb.AppendLine($"{strErrorMessage}");
                retMessage += strErrorMessage;
                //2019-12-07 Tai Le: send message to Telegram Group "PCMNotify"
                var botClient = new TelegramBotClient(TeleTokenID);
                //botClient.SendTextMessageAsync(-1001407116473, strErrorMessage).Wait();
                botClient.SendTextMessageAsync(-1001407116473, retMessage).Wait();
                mFactory = Factory;
                mQCOSource = "QCO";
                //2019-11-14 Tai Le (Thomas): Run the CAPACITY Distribution
                //this.CalculateCapaAll(OracleConnectionString); 
            }
            //Run the CAPA allocation 
            //string _tempCAPAMsg = "";
            this.mFactory = Factory;
            //this.CalculateCapaAll(this._PKMESConnString);
            //sb.AppendLine($"CalculateCAPA(): PASSED");

            //Copy from QCO to Sim-QCO
            if (pCopyToRunningQCO)
            {
                //CopySysQCOtoSIMQCO(Factory, QCOYear, strWeekNum);
                //sb.AppendLine($"CopySysQCOtoSIMQCO(): PASSED");
            }
            if (!String.IsNullOrEmpty(mEnviroment))
                if (mEnviroment.ToLower() == "console")
                    Console.WriteLine("QCO under " + Factory + " finished:" + strErrorMessage);
            return sb.ToString();
        }
        public string DistributeMaterialNew(bool IsSinglePP, string pFactory, int pQCOYear, string pWeekNum, ref DataTable dt_T_QC_QCPM, ref DataTable dt_QCFP, bool IsQCOSIM = false)
        {
            /* New distribution {Material Item;PO} to PKMES.T_QC_QCPM
			 * Should use Datable since need to update the usage Qty back to Datable
			 */
            StringBuilder sb = new StringBuilder();
            try
            {
                /* 2019-11-15  Main Distribute Material  */
                using (OracleConnection oracleConnection = new OracleConnection(_PKMESConnString))
                {
                    oracleConnection.Open();
                    DataSet ds = new DataSet();
                    int intSeqNo = 0;
                    //::: Distribute WMS Qty (Received Material Qty) 
                    //::: WMS based on WONO (MRP_OLD data)
                    string strSQL =
                        " SELECT V_WO_PO_RECWMS.* " +
                        " FROM PKERP.V_WO_PO_RECWMS " +
                        " WHERE " +
                        " ( WO Not Like 'AD%' ) AND " +
                        " ( WO IN " +
                        "   (Select V_MRP_PP_WO.WONO  " +
                        "    From PKMES.T_QC_QCFP " +
                        "    Inner Join PKMES.V_MRP_PP_WO On  " +
                        "       T_QC_QCFP.FACTORY = V_MRP_PP_WO.FACTORY " +
                        "       AND T_QC_QCFP.AONO = V_MRP_PP_WO.AONO " +
                        "       AND T_QC_QCFP.STYLECODE = V_MRP_PP_WO.STLCD " +
                        "       AND T_QC_QCFP.STYLESIZE = V_MRP_PP_WO.STLSIZ " +
                        "       AND T_QC_QCFP.STYLECOLORSERIAL = V_MRP_PP_WO.STLCOSN " +
                        "       AND T_QC_QCFP.REVNO = V_MRP_PP_WO.STLREVN " +
                        "       AND T_QC_QCFP.PRDPKG = V_MRP_PP_WO.PRODPACKAGE " +
                        "    WHERE T_QC_QCFP.QCOFACTORY = '" + pFactory + "' AND T_QC_QCFP.QCOYEAR = " + pQCOYear + " AND T_QC_QCFP.QCOWEEKNO = '" + pWeekNum + "' " +
                        "    Group By V_MRP_PP_WO.WONO) " +
                        " ) " +
                        " ORDER BY WO , ITEM_CD , COLOR_SERIAL , PLAN_DOQTY ";
                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
                    oracleDataAdapter.Fill(ds, "WMS");
                    oracleDataAdapter.Dispose();
                    //Add extra column for Source Table
                    //Add column ALLOCATED for Distribution Purpose.
                    DataColumn newColumn = new DataColumn("ALLOCATED", typeof(System.Double)) { DefaultValue = 0.0 };
                    ds.Tables["WMS"].Columns.Add(newColumn);
                    newColumn.Dispose();
                    DistributeMaterial_T_QC_QCPMNew(pFactory, pQCOYear, pWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, ds.Tables["WMS"], "W", ref intSeqNo);
                    //2020-06-16 Tai Le(Thomas): Remove to prevent the Out of Memory
                    ds.Tables.Remove("WMS");
                    sb.AppendLine($"Complete WMS");
                    //::: Distribute WMS Qty (Received Material Qty) 
                    //::: WMS based on AONO (MRP2 data)
                    strSQL =
                      " SELECT V_WO_PO_RECWMS.* " +
                      " FROM PKERP.V_WO_PO_RECWMS " +
                      " WHERE " +
                      " ( WO LIKE 'AD%' ) " +
                      " AND WO IN (Select AONO From PKMES.T_QC_QCFP  Where T_QC_QCFP.QCOFACTORY = '" + pFactory + "' AND T_QC_QCFP.QCOYEAR = " + pQCOYear + " AND T_QC_QCFP.QCOWEEKNO = '" + pWeekNum + "' Group By AONO) " +
                      " ORDER BY WO , ITEM_CD , COLOR_SERIAL , PLAN_DOQTY ";
                    oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
                    oracleDataAdapter.Fill(ds, "WMSNEW");
                    oracleDataAdapter.Dispose();
                    //Add extra column for Source Table
                    newColumn = new DataColumn("ALLOCATED", typeof(System.Double)) { DefaultValue = 0.0 };
                    ds.Tables["WMSNEW"].Columns.Add(newColumn);
                    newColumn.Dispose();
                    DistributeMaterial_T_QC_QCPMNew(pFactory, pQCOYear, pWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, ds.Tables["WMSNEW"], "W2", ref intSeqNo);
                    //2020-06-16 Tai Le(Thomas): Remove to prevent the Out of Memory
                    ds.Tables.Remove("WMSNEW");
                    sb.AppendLine($"Complete WMSNew");
                    //::: Distribute KMS Qty (Incoming Qty)
                    //2019-06-15:  Get the Monday based on Year and WeekNo 
                    int weekNum = 0;
                    weekNum = Convert.ToInt32(pWeekNum.Replace("W", ""));
                    if (IsSinglePP)
                    {
                        CultureInfo cul = CultureInfo.CurrentCulture;
                        weekNum = cul.Calendar.GetWeekOfYear(DateTime.Today, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                    }
                    DateTime dtMonday = PCMGeneralFunctions.GetDateFromWeekNumberAndDayOfWeek(pQCOYear, weekNum, 0);
                    if (IsQCOSIM)
                        dtMonday = DateTime.Today;
                    strSQL = " SELECT WO ,  PO as PDNO , ITEM_CD , COLOR_SERIAL , ETA , SUM(SHIP_QTY) PLAN_DOQTY " +
                                     " FROM KMS_PSRSHP_TBL@AOMTOPS " +
                                     " WHERE DELFLG = 'N' " +
                                     " AND ETA IS NOT NULL " +
                                     " AND Length(ETA) = 8 " +
                                     " AND ETA >= '" + dtMonday.ToString("yyyyMMdd") + "' " +
                                     " GROUP BY WO , PO , ITEM_CD , COLOR_SERIAL , ETA  ";
                    oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
                    oracleDataAdapter.Fill(ds, "KMS");
                    oracleDataAdapter.Dispose();
                    //Add extra column for Source Table
                    newColumn = new DataColumn("ALLOCATED", typeof(System.Double)) { DefaultValue = 0.0 };
                    ds.Tables["KMS"].Columns.Add(newColumn);
                    newColumn.Dispose();
                    DistributeMaterial_T_QC_QCPMNew(pFactory, pQCOYear, pWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, ds.Tables["KMS"], "K", ref intSeqNo);
                    //2020-06-16 Tai Le(Thomas): Remove to prevent the Out of Memory
                    ds.Tables.Remove("KMS");
                    sb.AppendLine($"Complete KMS");
                    //::: Distribute KMS Qty (Received Material Qty) 
                    /*2019-11-01 Tai Le (Thomas) add part of KMS from MRP2*/
                    strSQL = " SELECT PRDPKG , PO_NO as PDNO ,  ITEM_CD , COLOR_SERIAL , ETA , SUM(ORD_CNF_QTY) PLAN_DOQTY " +
                                     " FROM KMS_PSRSHP2_TBL@AOMTOPS " +
                                     " WHERE ETA IS NOT NULL " +
                                     " AND Length(ETA) = 8 " +
                                     " AND ETA >= '" + dtMonday.ToString("yyyyMMdd") + "' " +
                                     " GROUP BY PRDPKG , PO_NO , ITEM_CD , COLOR_SERIAL , ETA ";
                    oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
                    oracleDataAdapter.Fill(ds, "KMSNEW");
                    oracleDataAdapter.Dispose();
                    //Add column ASSIGNEDQTY for Distribution Purpose.
                    //DataColumn newColumn = new DataColumn("ASSIGNEDQTY", typeof(System.Double)) { DefaultValue = 0.0 };
                    //Add extra column for Source Table
                    newColumn = new DataColumn("ALLOCATED", typeof(System.Double)) { DefaultValue = 0.0 };
                    ds.Tables["KMSNEW"].Columns.Add(newColumn);
                    newColumn.Dispose();
                    DistributeMaterial_T_QC_QCPMNew(pFactory, pQCOYear, pWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, ds.Tables["KMSNEW"], "K2", ref intSeqNo);
                    //2020-06-16 Tai Le(Thomas): Remove to prevent the Out of Memory
                    ds.Tables.Remove("KMSNEW");
                    sb.AppendLine($"Complete KMSNEW");
                    ///////////////////////////////////////////////////////////////////////////////////
                    //int intSeqNo = 0;
                    //oracleConnection.Close();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                sb.AppendLine($"Exception: {msg}");
            }
            return sb.ToString();
        }
        public void DistributeMaterial_T_QC_QCPMNew(string vstrFactory, int vintQCOYear, string vstrWeekNo, ref DataTable vdtT_QC_QCPM, ref DataTable vdt_T_QC_QCFP, DataTable vDrMaterialSource, string vType, ref int vintSeqNo)
        {
            string ETA = "";
            try
            {
                /* vType 
					 *  - "W" stand for WMS >> it's Received Qty
					 *  - "K" stand for KMS >> it's ETA Qty
					 */
                if (vDrMaterialSource != null)
                    if (vDrMaterialSource.Rows.Count > 0)
                    {
                        double decAvaiAssignQty = 0;
                        double decAssignQty = 0;
                        bool blInsert = true;
                        bool isETA_Issue = false;
                        //while (vDrMaterialSource.Read())
                        foreach (DataRow DrMaterialSource in vDrMaterialSource.Rows)
                        {
                            isETA_Issue = false;
                            //Source Qty , used to distribute
                            //SourceQty = Origin PO Qty - Allocated Qty
                            var DOQTY = Convert.ToDouble(DrMaterialSource["PLAN_DOQTY"]) - Convert.ToDouble(DrMaterialSource["ALLOCATED"]);
                            if (DOQTY <= 0)
                                continue;
                            //2019-11-16 
                            DateTime dtPRDSDAT = DateTime.MinValue; //DateTime.ParseExact(PRDSDAT, "yyyyMMdd", new CultureInfo(""));
                            DateTime dtETA = dtPRDSDAT;
                            if (vType == "K" || vType == "K2")
                            {
                                ETA = DrMaterialSource["ETA"].ToString().Trim();
                                isETA_Issue = !DateTime.TryParseExact(ETA, "yyyyMMdd", new CultureInfo(""), DateTimeStyles.None, out dtETA);
                            }
                            if (isETA_Issue == false)
                            {
                                //string expression = " WONO = '" + vDrMaterialSource["WO"] + "' " + //" AND ITEMCODE = '" + vDrMaterialSource["ITEM_CD"] + "'  " + //                    " AND ITEMCOLORSERIAL = '" + vDrMaterialSource["COLOR_SERIAL"] + "' "; //if (vType == "K2") //{ //    expression = " PRDPKG = '" + vDrMaterialSource["PRDPKG"] + "' " + //                 " AND ITEMCODE = '" + vDrMaterialSource["ITEM_CD"] + "'  " + //                 " AND ITEMCOLORSERIAL = '" + vDrMaterialSource["COLOR_SERIAL"] + "' "; //} 
                                string expression = "";
                                if (vType == "K2")
                                {
                                    expression = " PRDPKG = '" + DrMaterialSource["PRDPKG"] + "' " +
                                                 " AND ITEMCODE = '" + DrMaterialSource["ITEM_CD"] + "'  " +
                                                 " AND ITEMCOLORSERIAL = '" + DrMaterialSource["COLOR_SERIAL"] + "' ";
                                }
                                else if (vType == "W2")
                                {
                                    expression = " ( AONO = '" + DrMaterialSource["WO"] + "' ) " +
                                                 " AND ( ITEMCODE = '" + DrMaterialSource["ITEM_CD"] + "' ) " +
                                                 " AND ( ITEMCOLORSERIAL = '" + DrMaterialSource["COLOR_SERIAL"] + "' ) ";
                                }
                                else
                                    expression = " ( WONO = '" + DrMaterialSource["WO"] + "' ) " +
                                                 " AND ( ITEMCODE = '" + DrMaterialSource["ITEM_CD"] + "' ) " +
                                                 " AND ( ITEMCOLORSERIAL = '" + DrMaterialSource["COLOR_SERIAL"] + "' ) ";
                                DataRow[] foundRows = vdt_T_QC_QCFP.Select(expression);
                                foreach (DataRow dr in foundRows)
                                {
                                    if (DOQTY > 0)
                                    {
                                        //Material Avai.Qty for allocation
                                        decAvaiAssignQty = Convert.ToDouble(dr["REQUESTQTY"].ToString()) - Convert.ToDouble(dr["ASSIGNEDQTY"].ToString());
                                        if (decAvaiAssignQty > 0)
                                        {
                                            //#Noted: If T_QC_QCPM not Distrbute yet, create new records
                                            DataRow drNew_tmp_T_QC_QCPM = vdtT_QC_QCPM.NewRow();
                                            vintSeqNo = vintSeqNo + 1;
                                            drNew_tmp_T_QC_QCPM["ID"] = dr["ID"]; //; dr["FACTORY"] + "-" + PCMGeneralFunctions.GetRight("0000000000000000" + vintSeqNo, 15);
                                                                                  //drNew_tmp_T_QC_QCPM["RANGKING"] = intSeqNo; // dr["RANGKING"];
                                            drNew_tmp_T_QC_QCPM["FACTORY"] = dr["FACTORY"];
                                            //2019-02-28
                                            drNew_tmp_T_QC_QCPM["QCOFACTORY"] = dr["FACTORY"];
                                            drNew_tmp_T_QC_QCPM["QCOYEAR"] = vintQCOYear;
                                            drNew_tmp_T_QC_QCPM["QCOWEEKNO"] = vstrWeekNo;
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
                                            //2019-12-07 Tai Le (Thomas)
                                            drNew_tmp_T_QC_QCPM["PDNO"] = DrMaterialSource["PDNO"];
                                            decAssignQty = decAvaiAssignQty;
                                            if (DOQTY < decAvaiAssignQty)
                                            {
                                                decAssignQty = DOQTY;
                                            }
                                            DrMaterialSource["ALLOCATED"] = Convert.ToDouble(DrMaterialSource["ALLOCATED"]) + decAssignQty;
                                            //2019-11-01 Tai Le (Thomas)
                                            drNew_tmp_T_QC_QCPM["SOURCEDATA"] = vType;
                                            drNew_tmp_T_QC_QCPM["QUANTITY_A"] = 0;
                                            drNew_tmp_T_QC_QCPM["QUANTITY_B"] = 0;
                                            drNew_tmp_T_QC_QCPM["QUANTITY_C"] = 0;
                                            drNew_tmp_T_QC_QCPM["QUANTITY_D"] = 0;
                                            //:: END    2019-11-01 Tai Le (Thomas)
                                            dr["ASSIGNEDQTY"] = Convert.ToDouble(dr["ASSIGNEDQTY"]) + decAssignQty;
                                            if (vType == "W" || vType == "W2")
                                            {
                                                /* When vType = "W" >> it means the RECEIVED QTY
												 *::: ONLY Data for QUANTITY_A   INCLUDED
												 */
                                                //dr["ASSIGNEDQTY"] = Convert.ToDouble(dr["ASSIGNEDQTY"].ToString()) + decAssignQty;
                                                drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty;
                                                drNew_tmp_T_QC_QCPM["QUANTITY_A"] = decAssignQty;
                                                blInsert = true;
                                            }
                                            else if (vType == "K" || vType == "K2")
                                            {
                                                /* When vType = "K" or "K2" >> it means the INCOMING QTY
												 *::: ONLY Data for QUANTITY_A   EXCLUDED
												 * "K"  >> MRP (old)
												 * "K2" >> MRP-2
												 */
                                                blInsert = false;
                                                /* 
												* Nếu ETA trong 5 ngày của ngày exec QCO Ranking ; tức  Calc_Date  < ETA < Exec_Date + 5.Days
												*      SHIP_QTY = SHIP_QTY * 50%>> Quantity_B
												* Nếu ETA trong 10 ngày của ngày exec QCO Ranking ; tức  Calc_Date  < ETA < Exec_Date + 10.Days
												*      SHIP_QTY = SHIP_QTY * 30%>> Quantity_C
												* Nếu ETA > 10 ngày của ngày exec QCO Ranking ; tức  Calc_Date + 10.Days < ETA  
												*      SHIP_QTY = SHIP_QTY * 10%>> Quantity_D
												*/
                                                //dr["ASSIGNEDQTY"] = Convert.ToDouble(dr["ASSIGNEDQTY"].ToString()) + decAssignQty;
                                                if (dtETA < dtPRDSDAT)
                                                {
                                                    drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = 0;
                                                    drNew_tmp_T_QC_QCPM["QUANTITY_A"] = 0;
                                                    blInsert = true;
                                                }
                                                else if (dtPRDSDAT < dtETA && dtETA <= dtPRDSDAT.AddDays(5))
                                                {
                                                    drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty * 0.5;
                                                    //2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column
                                                    drNew_tmp_T_QC_QCPM["QUANTITY_B"] = decAssignQty * 0.5;
                                                    blInsert = true;
                                                }
                                                else if (dtPRDSDAT.AddDays(5) < dtETA && dtETA <= dtPRDSDAT.AddDays(10))
                                                {
                                                    drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty * 0.3;
                                                    //2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column
                                                    drNew_tmp_T_QC_QCPM["QUANTITY_C"] = decAssignQty * 0.3;
                                                    blInsert = true;
                                                }
                                                else
                                                {
                                                    drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty * 0.1;
                                                    //2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column
                                                    drNew_tmp_T_QC_QCPM["QUANTITY_D"] = decAssignQty * 0.1;
                                                    blInsert = true;
                                                }
                                            }
                                            else
                                            {
                                                blInsert = false;
                                            }
                                            DOQTY = DOQTY - decAssignQty;
                                            if (blInsert)
                                                vdtT_QC_QCPM.Rows.Add(drNew_tmp_T_QC_QCPM);
                                            if (vintSeqNo % 500 == 0)
                                            {
                                                Debug.Print($"Finished {vintSeqNo}/{vDrMaterialSource.Rows.Count} Material Rows T_QC_QCPM");
                                                if (mEnviroment.ToLower() == "console")
                                                    Console.WriteLine("Finished " + vintSeqNo + " Material Rows T_QC_QCPM");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                if (!String.IsNullOrEmpty(mEnviroment))
                    if (mEnviroment.ToLower() == "console")
                        Console.WriteLine("ERROR at DistributeMaterial_T_QC_QCPM(): " + Msg + "; Type= " + vType + ", ETA (string)=" + ETA);
            }
        }
        #endregion

        #region Distribute Factory CAPA to QC_QUEUE
        public void CalculateCapaAll(string pStartWeek = "")
        {
            try
            {
                List<PCMQCOCalculation> Factories = new List<PCMQCOCalculation>();
                using (OracleConnection oracleConnection = new OracleConnection(_PKMESConnString))
                {
                    oracleConnection.Open();
                    var strSQL = "Select FACTORY " +
                                 " From PKMES.T_QC_QCFR " +
                                 " Where 1=1 ";
                    if (!String.IsNullOrEmpty(mFactory))
                        strSQL = strSQL + " AND FACTORY like :FACTORY ";
                    List<OracleParameter> parameters = new List<OracleParameter>();
                    if (!String.IsNullOrEmpty(mFactory))
                        parameters.Add(new OracleParameter("FACTORY", mFactory));
                    else
                        parameters.Add(new OracleParameter("FACTORY", "%"));

                    strSQL = strSQL + " Order By FACTORY ";
                    DataTable dt = new DataTable();
                    PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, parameters, out dt, out strSQL);
                    parameters.Clear();
                    if (dt != null)
                    {
                        if (dt.Rows.Count > 0)
                            foreach (DataRow dr in dt.Rows)
                            {
                                Factories.Add(new PCMQCOCalculation(dr["FACTORY"].ToString()));
                            }
                        dt.Dispose();
                    }
                    //oracleConnection.Close();
                }
                string Msg = "";
                DateTime dtStarDateTime = DateTime.Now.AddHours(12);
                //Determinate the YEAR / WeekNo  
                int QCOYear = dtStarDateTime.Year;
                if (!String.IsNullOrEmpty(mEnviroment))
                    if (mEnviroment.ToLower() == "console")
                        Console.WriteLine("dtStarDateTime= " + dtStarDateTime.ToString("s"));
                CultureInfo cul = CultureInfo.CurrentCulture;
                int weekNum = cul.Calendar.GetWeekOfYear(dtStarDateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                string strWeekNum = "W" + PCMGeneralFunctions.GetRight("00" + weekNum, 2);
                if (!String.IsNullOrEmpty(mWeekNo))
                {
                    strWeekNum = mWeekNo;
                }
                if (!String.IsNullOrEmpty(pStartWeek))
                {
                    strWeekNum = pStartWeek;
                }
                weekNum = Convert.ToInt32(strWeekNum.Replace("W", ""));

                for (int I = 0; I < Factories.Count; I++)
                {
                    if (mEnviroment.ToLower() == "console")
                    {
                        Console.Write("QCO-Capa in Factory: " + Factories[I].mFactory);
                    }
                    //2020-07-07 Tai Le(Thomas): Handle the weekly Capacity
                    UpdateWeeklyCapacityFromMTOPS(
                        pUserID: "System",
                        modalWeekCapaFactory: Factories[I].mFactory,
                        modalWeekCapaYear: QCOYear.ToString(),
                        modalWeekCapaFromWeek: weekNum.ToString(),
                        modalWeekCapaToWeek: "53");

                    Msg = "";
                    CalculateCAPA(
                        pFactory: Factories[I].mFactory,
                        pYear: QCOYear,
                        pWeekNo: weekNum,
                        pIncludeNegativeRank: true,
                        pMessage: out Msg);
                    if (mEnviroment.ToLower() == "console")
                    {
                        Console.WriteLine("QCO-Capa in Factory: " + Factories[I].mFactory + ": DONE");
                        Console.WriteLine("================================");
                        Console.WriteLine("");
                    }
                }
                Factories.Clear();
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
        public bool CalculateCAPA(string pFactory, int pYear, int pWeekNo, bool pIncludeNegativeRank, out string pMessage)
        {
            pMessage = "";
            if (String.IsNullOrEmpty(mQCOSource))
            {
                pMessage = "mQCOSource NOT DEFINED.";
                return false;
            }
            StringBuilder sb = new StringBuilder();
            var _dtMain = DateTime.Now;
            int I = 0, J = 0, currentJ = 0;
            if (String.IsNullOrEmpty(mUserID))
                mUserID = "System";
            try
            {
                bool blResult = true;
                var strSQL =
                    " SELECT * " +
                    " FROM {TableName} " +
                    " WHERE QCOFACTORY = :QCOFACTORY " +
                    " AND QCOYEAR = :QCOYEAR  " +
                    " AND QCOWEEKNO = :QCOWEEKNO  " +
                    " AND QCORANK>0 " +
                    " ORDER BY NVL(CHANGEQCORANK , QCORANK) ";
                if (pIncludeNegativeRank)
                    strSQL =
                    " SELECT * " +
                    " FROM {TableName} " +
                    " WHERE QCOFACTORY = :QCOFACTORY " +
                    " AND QCOYEAR = :QCOYEAR  " +
                    " AND QCOWEEKNO = :QCOWEEKNO  " +
                    " ORDER BY NVL(CHANGEQCORANK , QCORANK) ";
                if ("QCO" == mQCOSource)
                {
                    strSQL = strSQL.Replace("{TableName}", "PKMES.T_QC_QUEUE");
                }
                else if ("QCOSim" == mQCOSource)
                {
                    strSQL = strSQL.Replace("{TableName}", "PKMES.T_QC_QUEUESIM");
                }
                else
                {
                    strSQL = strSQL.Replace("{TableName}", "PKMES.T_QC_QUEUE");
                }
                List<OracleParameter> parameters = new List<OracleParameter>();
                parameters.Add(new OracleParameter("QCOFACTORY", pFactory));
                parameters.Add(new OracleParameter("QCOYEAR", pYear));
                parameters.Add(new OracleParameter("QCOWEEKNO", "W" + PCMGeneralFunctions.GetRight("00" + pWeekNo.ToString(), 2)));
                OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, _PKMESConnString);
                oracleDataAdapter.SelectCommand.Parameters.AddRange(parameters.ToArray());
                var _dt_T_QC_QUEUE = new DataTable();
                oracleDataAdapter.Fill(_dt_T_QC_QUEUE);
                if (_dt_T_QC_QUEUE.Rows.Count > 0)
                {
                    //var StartDate = new DateTime();
                    //var EndDate = new DateTime();
                    decimal FactoryCAPA = 0; // FactoryCAPATT = 0;
                    decimal OccupiedCAPA = 0; // OccupiedCAPATT = 0;
                    decimal ProductionTime = 0;  // ProductionTimeTT = 0;
                    //bool isZeroCAPA = true;
                    FactoryCAPA = 0;
                    decimal StyleOpTime = 0;
                    decimal StyleOPHourlyRate = 0;
                    decimal StyleOPManCount = 0;
                    decimal StyleOPTAKETIME = 0;
                    string Msg = "";
                    string AccumMsg = "";
                    Dictionary<string, string> OPTIMES = new Dictionary<string, string>();
                    Dictionary<string, decimal> StyleManCount = new Dictionary<string, decimal>();
                    Dictionary<string, decimal> StyleDailyRate = new Dictionary<string, decimal>();
                    Dictionary<string, decimal> StyleTAKETIME = new Dictionary<string, decimal>();
                    Dictionary<string, decimal> StyleOPTIME = new Dictionary<string, decimal>();
                    Dictionary<string, double> FactoryLineDailyUsageHour = new Dictionary<string, double>();
                    //Dictionary<string, int> FactoryLineHandleCAPA = new Dictionary<string, int>();
                    bool blUsageFull = false;
                    //bool blUsageFullTT = false;
                    decimal EfficiencyRate = 0;
                    /// 2019-12-23 Tai Le (Thomas): Fetch the Factory Weekly Capacity, with starting Week from "pWeekNo"
                    var objFactoryCAPAs = GetAllFactoryCAPA(pFactory, pYear, pWeekNo);
                    if (objFactoryCAPAs.Count > 0)
                    {
                        foreach (var _ele in objFactoryCAPAs)
                        {
                            var _year = _ele.YEAR;
                            var _week = _ele.WEEKNO;
                            //StartDate = _ele.STARTDATE.AddHours(7);
                            //EndDate = _ele.ENDDATE.AddHours(7);
                            FactoryCAPA = _ele.CAPACITY;
                            /// Factory Weekly Efficiency
                            var objFactoryWeeklyEfficiency = GetFactoryEfficiency(pFactory, Convert.ToInt32(_year), "W" + PCMGeneralFunctions.GetRight("00" + _week.ToString(), 2));
                            if (objFactoryWeeklyEfficiency != null)
                                EfficiencyRate = objFactoryWeeklyEfficiency.EFFICIENCYPERCEN;
                            else
                                EfficiencyRate = 75;
                            decimal PackageQty = 0;
                            var rowCount = _dt_T_QC_QUEUE.Rows.Count;
                            for (J = currentJ; J < rowCount; J++)
                            {
                                currentJ = J;
                                Console.WriteLine("currentJ= " + currentJ);
                                sb.AppendLine("");
                                sb.AppendLine("currentJ= " + currentJ);
                                DataRow _dr_T_QC_QUEUE = _dt_T_QC_QUEUE.Rows[J];
                                //Reset:
                                OccupiedCAPA = 0;
                                PackageQty = 0;
                                //OccupiedCAPATT = 0;
                                //Assign 
                                var LINENO = _dr_T_QC_QUEUE["LINENO"].ToString();
                                var STYLECODE = _dr_T_QC_QUEUE["STYLECODE"].ToString();
                                var STYLESIZE = _dr_T_QC_QUEUE["STYLESIZE"].ToString();
                                var STYLECOLORSERIAL = _dr_T_QC_QUEUE["STYLECOLORSERIAL"].ToString();
                                var REVNO = _dr_T_QC_QUEUE["REVNO"].ToString();
                                //2019-11-15 Tai Le (Thomas): the Requird Capacity should be come from Remain Package Qty 
                                //var PackageQty = (decimal)_dr_T_QC_QUEUE["PLANQTY"];
                                if (_dr_T_QC_QUEUE["PLANQTYBAL"] != null)
                                {
                                    if (_dr_T_QC_QUEUE["PLANQTYBAL"].ToString().Length > 0)
                                        PackageQty = (decimal)_dr_T_QC_QUEUE["PLANQTYBAL"];
                                    else
                                        PackageQty = (decimal)_dr_T_QC_QUEUE["PLANQTY"];
                                }
                                else
                                    PackageQty = (decimal)_dr_T_QC_QUEUE["PLANQTY"];
                                sb.AppendLine("PRDPKG= " + _dr_T_QC_QUEUE["PRDPKG"].ToString() + ", PLANQTY= " + _dr_T_QC_QUEUE["PLANQTY"].ToString());
                                var KEY = STYLECODE.ToString() + STYLESIZE.ToString() + STYLECOLORSERIAL.ToString() + REVNO.ToString() + pFactory;
                                #region GET STYLE OPTIME , STYLE DAILY RATE , STYLE MANCOUNT
                                if (!OPTIMES.ContainsKey(KEY))
                                {
                                    //use Dictionary to prevent get the existing OPTIME from DB
                                    Msg = "";
                                    if (GetStyleOPTIME(STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, pFactory, out Msg))
                                    {
                                        /* [Msg] contain the OPTIME Data of Input Style
										 * Format:  OPTIME + ";" + DAILYTARGET + ";" + MANCOUNT + ";" + TAKETIME
										 */
                                        OPTIMES.Add(KEY, Msg);
                                        StyleOPTIME.Add(KEY, Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[0]));
                                        StyleDailyRate.Add(KEY, Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[1]));
                                        StyleManCount.Add(KEY, Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[2]));
                                        StyleTAKETIME.Add(KEY, Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[3]));
                                        StyleOpTime = Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                                        var OPTIME_StyleDailyRate = Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[1]);
                                        //From PKERP.T_OP_OPTIME, DAILY TARGET use 7.5 as standard
                                        StyleOPHourlyRate = Math.Floor((decimal)OPTIME_StyleDailyRate / (decimal)7.5);
                                        StyleOPManCount = Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[2]);
                                        StyleOPTAKETIME = Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[3]);
                                        #region For DEBUGGING  
                                        sb.AppendLine("KEY= " + STYLECODE.ToString() + ";" + STYLESIZE.ToString() + ";" + STYLECOLORSERIAL.ToString() + ";" + REVNO.ToString() + ";" + pFactory);
                                        sb.AppendLine("OPTIME_StyleDailyRate= " + OPTIME_StyleDailyRate);
                                        sb.AppendLine("StyleOPHourlyRate= " + StyleOPHourlyRate);
                                        sb.AppendLine("StyleOPManCount= " + StyleOPManCount);
                                        #endregion
                                        Msg = "";
                                    }
                                    else
                                        Msg = "GetStyleOPTIME(" + KEY + ") FAIL: " + Msg;
                                }
                                else
                                {
                                    StyleOpTime = StyleOPTIME[KEY];
                                    var OPTIME_StyleDailyRate = StyleDailyRate[KEY];
                                    //From PKERP.T_OP_OPTIME, DAILY TARGET use 7.5 as standard
                                    StyleOPHourlyRate = Math.Floor((decimal)OPTIME_StyleDailyRate / (decimal)7.5);
                                    StyleOPManCount = StyleManCount[KEY];
                                    StyleOPTAKETIME = StyleTAKETIME[KEY];
                                    #region For DEBUGGING  
                                    sb.AppendLine("FROM EXISTING DICTIONARY");
                                    sb.AppendLine("KEY= " + STYLECODE.ToString() + ";" + STYLESIZE.ToString() + ";" + STYLECOLORSERIAL.ToString() + ";" + REVNO.ToString() + ";" + pFactory);
                                    sb.AppendLine("OPTIME_StyleDailyRate= " + OPTIME_StyleDailyRate);
                                    sb.AppendLine("StyleOPHourlyRate= " + StyleOPHourlyRate);
                                    sb.AppendLine("StyleOPManCount= " + StyleOPManCount);
                                    #endregion
                                }
                                /* if Any ERROR, go to next QCO Ranking Row */
                                if (!String.IsNullOrEmpty(Msg))
                                {
                                    if (String.IsNullOrEmpty(AccumMsg))
                                        AccumMsg = Msg;
                                    else
                                        AccumMsg = AccumMsg + "<BR/>" + Msg;
                                    //To next QCO Rank Row
                                    continue;
                                }
                                #endregion
                                _dr_T_QC_QUEUE["MANCOUNT"] = StyleOPManCount;
                                _dr_T_QC_QUEUE["OPTIME"] = StyleOpTime; //Unit is Second 
                                                                        //Console.WriteLine("OPTIME= " + StyleOpTime / 3600);
                                                                        //sb.AppendLine("OPTIME= " + StyleOpTime / 3600);
                                _dr_T_QC_QUEUE["OPTIMEHOURLY"] = StyleOpTime / 3600; //Unit is Hour 
                                ProductionTime = Math.Round(PackageQty * (decimal)(StyleOpTime / 3600), 2, MidpointRounding.AwayFromZero); //Unit is Hour 
                                OccupiedCAPA = ProductionTime * (2 - EfficiencyRate * (decimal)0.01);
                                //2019-11-04 Tai Le (Thomas): modify the Efficiency to apply rule
                                // 50% >> 2 times of ProductionTime
                                // 100% >> same as ProductionTime 
                                OccupiedCAPA = ProductionTime + ProductionTime * (2 - (2 * EfficiencyRate * (decimal)0.01));
                                //::END     2019-11-04 Tai Le (Thomas): modify the Efficiency to apply rule
                                _dr_T_QC_QUEUE["TAKETIME"] = StyleOPTAKETIME; //Number of Bag/Hour
                                                                              //_dr_T_QC_QUEUE["TAKETIMEHOURLY"] = (decimal)(3600.00 / StyleOPTAKETIME); //Number of Bag/Hour
                                if (FactoryCAPA > OccupiedCAPA)
                                {
                                    _dr_T_QC_QUEUE["BEGINCAPA"] = FactoryCAPA;
                                    _dr_T_QC_QUEUE["USAGECAPA"] = OccupiedCAPA;
                                    FactoryCAPA = FactoryCAPA - OccupiedCAPA; //Deduct the Factory Capacity 
                                    _dr_T_QC_QUEUE["BALANCECAPA"] = FactoryCAPA;
                                    //2020-01-07 Tai Le(Thomas): Convert the WeekNo format same as QCO WeekNo
                                    _dr_T_QC_QUEUE["WEEKCAPA"] = _year.ToString() + " / W" + (_week < 10 ? "0" + _week.ToString() : _week.ToString());
                                    _dr_T_QC_QUEUE["EFFICIENCY"] = EfficiencyRate;
                                    ////2019-10-16 Tai Le (Thomas)
                                    _dr_T_QC_QUEUE["CAPAALLOCATEBY"] = mUserID;
                                    _dr_T_QC_QUEUE["CAPAALLOCATEON"] = _dtMain;
                                    ////2019-10-25 
                                    _dr_T_QC_QUEUE["WEEKWORKHOUR"] = _ele.TOTALWORKHOUR;
                                }
                                else
                                {
                                    _dr_T_QC_QUEUE["BEGINCAPA"] = DBNull.Value;
                                    _dr_T_QC_QUEUE["USAGECAPA"] = DBNull.Value;
                                    _dr_T_QC_QUEUE["BALANCECAPA"] = DBNull.Value;
                                    _dr_T_QC_QUEUE["WEEKCAPA"] = DBNull.Value;
                                    _dr_T_QC_QUEUE["EFFICIENCY"] = DBNull.Value;
                                    ////2019-10-16 Tai Le (Thomas)
                                    _dr_T_QC_QUEUE["CAPAALLOCATEBY"] = mUserID;
                                    _dr_T_QC_QUEUE["CAPAALLOCATEON"] = _dtMain;
                                    ////2019-10-25 
                                    _dr_T_QC_QUEUE["WEEKWORKHOUR"] = _ele.TOTALWORKHOUR;
                                    ////2020-01-06 Tai Le(Thomas)
                                    blUsageFull = true;
                                }
                                if (blUsageFull)
                                    break;
                            }//Loop to next QCO RANKING Row.  
                            ////2020-01-06 Tai Le(Thomas)
                            blUsageFull = false;
                        }//Loop the next Weekly Capacity
                    }
                    ///::END   2019-12-23 Tai Le (Thomas): Fetch the Factory Weekly Capacity, with starting Week from "pWeekNo"
                    /// 2019-12-23 Tai Le(Thomas) disable below codes and use above codes
                    //for (I = pWeekNo; I <= 52; I++)
                    //{  //Loop for 100 Weeks 
                    //    blUsageFull = false; 
                    //    var ProcessWeek = I; 
                    //    var objFactoryCAPA = GetFactoryCAPA(pConnectionStringMES, pFactory, pYear, ProcessWeek);
                    //    if (objFactoryCAPA != null)
                    //    {
                    //        StartDate = objFactoryCAPA.STARTDATE.AddHours(7);
                    //        EndDate = objFactoryCAPA.ENDDATE.AddHours(7); 
                    //        FactoryCAPA = objFactoryCAPA.CAPACITY;
                    //        //FactoryCAPATT = FactoryCAPA;
                    //    }
                    //    else
                    //    {
                    //        continue;
                    //    } 
                    //    var objFactoryWeeklyEfficiency = GetFactoryEfficiency(pConnectionStringMES, pFactory, pYear, "W" + PCMGeneralFunctions.GetRight("00" + ProcessWeek.ToString(), 2));
                    //    if (objFactoryWeeklyEfficiency != null)
                    //    {
                    //        EfficiencyRate = objFactoryWeeklyEfficiency.EFFICIENCYPERCEN;
                    //    }
                    //    else
                    //    {
                    //        EfficiencyRate = 75;
                    //        //2019-12-06 Tai Le (Thomas)
                    //        //change the logic to get from Previous Week
                    //        //objFactoryWeeklyEfficiency = GetFactoryEfficiency(pConnectionStringMES, pFactory, pYear, "W" + PCMGeneralFunctions.GetRight("00" + (ProcessWeek - 1).ToString(), 2));
                    //        //if (objFactoryWeeklyEfficiency == null)
                    //        //{
                    //        //    EfficiencyRate = 75;
                    //        //    //Get the lastest confirmed Production Efficiency Rate
                    //        //    objFactoryWeeklyEfficiency = GetFactoryEfficiency(pConnectionStringMES, pFactory, 0, "W00");
                    //        //    if (objFactoryWeeklyEfficiency == null)
                    //        //    {
                    //        //        EfficiencyRate = 75;
                    //        //    }
                    //        //    else EfficiencyRate = objFactoryWeeklyEfficiency.EFFICIENCYPERCEN;
                    //        //}
                    //        //else
                    //        //    EfficiencyRate = objFactoryWeeklyEfficiency.EFFICIENCYPERCEN;
                    //    }
                    //    /*2019-11-04 Tai Le (Thomas): Handle the Efficiency */
                    //    /*::END     2019-11-04 Tai Le (Thomas): Handle the Efficiency */ 
                    //    decimal PackageQty = 0; 
                    //    for (J = currentJ; J < _dt_T_QC_QUEUE.Rows.Count; J++)
                    //    //for (J = currentJ; J < 10; J++)
                    //    {
                    //        currentJ = J;
                    //        Console.WriteLine("currentJ= " + currentJ);
                    //        sb.AppendLine("");
                    //        sb.AppendLine("currentJ= " + currentJ); 
                    //        DataRow _dr_T_QC_QUEUE = _dt_T_QC_QUEUE.Rows[J];
                    //        //Reset:
                    //        OccupiedCAPA = 0;
                    //        PackageQty = 0;
                    //        //OccupiedCAPATT = 0; 
                    //        //Assign 
                    //        var LINENO = _dr_T_QC_QUEUE["LINENO"].ToString(); 
                    //        var STYLECODE = _dr_T_QC_QUEUE["STYLECODE"].ToString();
                    //        var STYLESIZE = _dr_T_QC_QUEUE["STYLESIZE"].ToString();
                    //        var STYLECOLORSERIAL = _dr_T_QC_QUEUE["STYLECOLORSERIAL"].ToString();
                    //        var REVNO = _dr_T_QC_QUEUE["REVNO"].ToString(); 
                    //        //2019-11-15 Tai Le (Thomas): the Requird Capacity should be come from Remain Package Qty 
                    //        //var PackageQty = (decimal)_dr_T_QC_QUEUE["PLANQTY"];
                    //        if (_dr_T_QC_QUEUE["PLANQTYBAL"] != null)
                    //        {
                    //            if (_dr_T_QC_QUEUE["PLANQTYBAL"].ToString().Length > 0)
                    //                PackageQty = (decimal)_dr_T_QC_QUEUE["PLANQTYBAL"];
                    //            else
                    //                PackageQty = (decimal)_dr_T_QC_QUEUE["PLANQTY"];
                    //        }
                    //        else
                    //            PackageQty = (decimal)_dr_T_QC_QUEUE["PLANQTY"]; 
                    //        sb.AppendLine("PRDPKG= " + _dr_T_QC_QUEUE["PRDPKG"].ToString() + ", PLANQTY= " + _dr_T_QC_QUEUE["PLANQTY"].ToString()); 
                    //        var KEY = STYLECODE.ToString() + STYLESIZE.ToString() + STYLECOLORSERIAL.ToString() + REVNO.ToString() + pFactory; 
                    //        #region GET STYLE OPTIME , STYLE DAILY RATE , STYLE MANCOUNT
                    //        if (!OPTIMES.ContainsKey(KEY))
                    //        {
                    //            //use Dictionary to prevent get the existing OPTIME from DB
                    //            Msg = "";
                    //            if (GetStyleOPTIME(pConnectionStringMES, STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, pFactory, out Msg))
                    //            {
                    //                /* [Msg] contain the OPTIME Data of Input Style
                    //                 * Format:  OPTIME + ";" + DAILYTARGET + ";" + MANCOUNT + ";" + TAKETIME
                    //                 */
                    //                OPTIMES.Add(KEY, Msg); 
                    //                StyleOPTIME.Add(KEY, Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[0]));
                    //                StyleDailyRate.Add(KEY, Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[1]));
                    //                StyleManCount.Add(KEY, Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[2]));
                    //                StyleTAKETIME.Add(KEY, Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[3])); 
                    //                StyleOpTime = Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                    //                var OPTIME_StyleDailyRate = Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[1]);
                    //                //From PKERP.T_OP_OPTIME, DAILY TARGET use 7.5 as standard
                    //                StyleOPHourlyRate = Math.Floor((decimal)OPTIME_StyleDailyRate / (decimal)7.5);
                    //                StyleOPManCount = Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[2]);
                    //                StyleOPTAKETIME = Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[3]); 
                    //                #region For DEBUGGING
                    //                //Console.WriteLine("KEY= " + STYLECODE.ToString() + ";" + STYLESIZE.ToString() + ";" + STYLECOLORSERIAL.ToString() + ";" + REVNO.ToString() + ";" + pFactory);
                    //                //Console.WriteLine("OPTIME_StyleDailyRate= " + OPTIME_StyleDailyRate);
                    //                //Console.WriteLine("StyleOPHourlyRate= " + StyleOPHourlyRate);
                    //                //Console.WriteLine("StyleOPManCount= " + StyleOPManCount); 
                    //                sb.AppendLine("KEY= " + STYLECODE.ToString() + ";" + STYLESIZE.ToString() + ";" + STYLECOLORSERIAL.ToString() + ";" + REVNO.ToString() + ";" + pFactory);
                    //                sb.AppendLine("OPTIME_StyleDailyRate= " + OPTIME_StyleDailyRate);
                    //                sb.AppendLine("StyleOPHourlyRate= " + StyleOPHourlyRate);
                    //                sb.AppendLine("StyleOPManCount= " + StyleOPManCount);
                    //                #endregion 
                    //                Msg = "";
                    //            }
                    //            else
                    //                Msg = "GetStyleOPTIME(" + KEY + ") FAIL: " + Msg;
                    //        }
                    //        else
                    //        {
                    //            StyleOpTime = StyleOPTIME[KEY]; 
                    //            var OPTIME_StyleDailyRate = StyleDailyRate[KEY];
                    //            //From PKERP.T_OP_OPTIME, DAILY TARGET use 7.5 as standard
                    //            StyleOPHourlyRate = Math.Floor((decimal)OPTIME_StyleDailyRate / (decimal)7.5);
                    //            StyleOPManCount = StyleManCount[KEY];
                    //            StyleOPTAKETIME = StyleTAKETIME[KEY]; 
                    //            #region For DEBUGGING
                    //            //Console.WriteLine("FROM EXISTING DICTIONARY");
                    //            //Console.WriteLine("KEY= " + STYLECODE.ToString() + ";" + STYLESIZE.ToString() + ";" + STYLECOLORSERIAL.ToString() + ";" + REVNO.ToString() + ";" + pFactory);
                    //            //Console.WriteLine("OPTIME_StyleDailyRate= " + OPTIME_StyleDailyRate);
                    //            //Console.WriteLine("StyleOPHourlyRate= " + StyleOPHourlyRate);
                    //            //Console.WriteLine("StyleOPManCount= " + StyleOPManCount); 
                    //            sb.AppendLine("FROM EXISTING DICTIONARY");
                    //            sb.AppendLine("KEY= " + STYLECODE.ToString() + ";" + STYLESIZE.ToString() + ";" + STYLECOLORSERIAL.ToString() + ";" + REVNO.ToString() + ";" + pFactory);
                    //            sb.AppendLine("OPTIME_StyleDailyRate= " + OPTIME_StyleDailyRate);
                    //            sb.AppendLine("StyleOPHourlyRate= " + StyleOPHourlyRate);
                    //            sb.AppendLine("StyleOPManCount= " + StyleOPManCount);
                    //            #endregion
                    //        } 
                    //        /* if Any ERROR, go to next QCO Ranking Row */
                    //        if (!String.IsNullOrEmpty(Msg))
                    //        {
                    //            if (String.IsNullOrEmpty(AccumMsg))
                    //                AccumMsg = Msg;
                    //            else
                    //                AccumMsg = AccumMsg + "<BR/>" + Msg;
                    //            //To next QCO Rank Row
                    //            continue;
                    //        }
                    //        #endregion 
                    //        _dr_T_QC_QUEUE["MANCOUNT"] = StyleOPManCount; 
                    //        _dr_T_QC_QUEUE["OPTIME"] = StyleOpTime; //Unit is Second 
                    //                                                //Console.WriteLine("OPTIME= " + StyleOpTime / 3600);
                    //                                                //sb.AppendLine("OPTIME= " + StyleOpTime / 3600); 
                    //        _dr_T_QC_QUEUE["OPTIMEHOURLY"] = StyleOpTime / 3600; //Unit is Hour  
                    //        ProductionTime = Math.Round(PackageQty * (decimal)(StyleOpTime / 3600), 2, MidpointRounding.AwayFromZero); //Unit is Hour 
                    //        OccupiedCAPA = ProductionTime * (2 - EfficiencyRate * (decimal)0.01);
                    //        //2019-11-04 Tai Le (Thomas): modify the Efficiency to apply rule
                    //        // 50% >> 2 times of ProductionTime
                    //        // 100% >> same as ProductionTime 
                    //        OccupiedCAPA = ProductionTime + ProductionTime * (2 - (2 * EfficiencyRate * (decimal)0.01));
                    //        //::END     2019-11-04 Tai Le (Thomas): modify the Efficiency to apply rule 
                    //        #region For DEBUGGER  
                    //        //sb.AppendLine("ProductionTime= " + ProductionTime);
                    //        //sb.AppendLine("OccupiedCAPA= " + OccupiedCAPA);
                    //        #endregion 
                    //        _dr_T_QC_QUEUE["TAKETIME"] = StyleOPTAKETIME; //Number of Bag/Hour
                    //        //_dr_T_QC_QUEUE["TAKETIMEHOURLY"] = (decimal)(3600.00 / StyleOPTAKETIME); //Number of Bag/Hour 
                    //        #region For DEBUGGER 
                    //        //sb.AppendLine("TAKETIME= " + (decimal)3600.00 / StyleOPTAKETIME);
                    //        //sb.AppendLine("TAKETIME in Second= " + StyleOPTAKETIME);
                    //        #endregion 
                    //        //ProductionTimeTT = Convert.ToDecimal(PackageQty / ((decimal)(3600.00 / StyleOPTAKETIME)));
                    //        //OccupiedCAPATT = Math.Round(ProductionTimeTT * StyleOPManCount, 2, MidpointRounding.AwayFromZero);  
                    //        #region For DEBUGGER 
                    //        //sb.AppendLine("ProductionTimeTT= " + ProductionTimeTT);
                    //        //sb.AppendLine("OccupiedCAPATT= " + OccupiedCAPATT);
                    //        #endregion 
                    //        #region TBD_Part
                    //        //_dr_T_QC_QUEUE["PKGOPTIME"] = ProductionTime;
                    //        //_dr_T_QC_QUEUE["PKGMANCOUNT"] = 0;
                    //        #endregion 
                    //        if (FactoryCAPA > OccupiedCAPA)
                    //        {  
                    //            _dr_T_QC_QUEUE["BEGINCAPA"] = FactoryCAPA;
                    //            _dr_T_QC_QUEUE["USAGECAPA"] = OccupiedCAPA; 
                    //            FactoryCAPA = FactoryCAPA - OccupiedCAPA; //Deduct the Factory Capacity 
                    //            _dr_T_QC_QUEUE["BALANCECAPA"] = FactoryCAPA; 
                    //            _dr_T_QC_QUEUE["WEEKCAPA"] = pYear.ToString() + " / W" + ProcessWeek.ToString();
                    //            _dr_T_QC_QUEUE["EFFICIENCY"] = EfficiencyRate; 
                    //            ////2019-10-16 Tai Le (Thomas)
                    //            _dr_T_QC_QUEUE["CAPAALLOCATEBY"] = mUserID;
                    //            _dr_T_QC_QUEUE["CAPAALLOCATEON"] = _dtMain; 
                    //            ////2019-10-25 
                    //            _dr_T_QC_QUEUE["WEEKWORKHOUR"] = objFactoryCAPA.TOTALWORKHOUR;
                    //        }
                    //        else
                    //            blUsageFull = true; 
                    //        /**if (FactoryCAPATT > OccupiedCAPATT)
                    //        //{
                    //        //    _dr_T_QC_QUEUE["BEGINCAPATT"] = FactoryCAPATT;
                    //        //    _dr_T_QC_QUEUE["USAGECAPATT"] = OccupiedCAPATT; 
                    //        //    FactoryCAPATT = FactoryCAPATT - OccupiedCAPATT; //Deduct the Factory Capacity 
                    //        //    _dr_T_QC_QUEUE["BALANCECAPATT"] = FactoryCAPATT; 
                    //        //}
                    //        //else blUsageFullTT = true;
                    //        */ 
                    //        if (blUsageFull)
                    //            break; 
                    //    }//Loop to next QCO RANKING.  
                    //    /*2019-12-22 Tai Le (Thomas)*/
                    //    if (I == 52)
                    //    {
                    //        I = 0;
                    //        pYear = pYear + 1;
                    //    }
                    //} //Loop to next WEEK NO.
                    OPTIMES.Clear();
                    StyleDailyRate.Clear();
                    StyleManCount.Clear();
                    OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                    oracleDataAdapter.Update(_dt_T_QC_QUEUE);
                    oracleCommandBuilder.Dispose();
                }
                _dt_T_QC_QUEUE.Dispose();
                oracleDataAdapter.Dispose();
                pMessage = sb.ToString();
                sb.Clear();
                return blResult;
            }
            catch (Exception ex)
            {
                pMessage = "Error On Factory:" + pFactory + ", Year:" + pYear.ToString() + ", Week:" + pWeekNo.ToString() + " I= [" + I + "] ; J= [" + J + "] ; currentJ= [" + currentJ + "] ; Description:" + ex.Message;
                Console.WriteLine(pMessage);
                return false;
            }
        }
        public bool DistributeCAPA(string pConnectionString, string pFactory, int pYear, int pWeekNo, bool pIncludeNegativeRank, out string pMessage)
        {
            pMessage = "";
            StringBuilder sb = new StringBuilder();
            try
            {
                bool blResult = true;
                var strSQL =
                    " SELECT * " +
                    " FROM PKMES.T_QC_QUEUE " +
                    " WHERE T_QC_QUEUE.QCOFACTORY = :QCOFACTORY " +
                    " AND T_QC_QUEUE.QCOYEAR = :QCOYEAR  " +
                    " AND T_QC_QUEUE.QCOWEEKNO = :QCOWEEKNO  " +
                    " AND QCORANK>0 " +
                    " AND LINENO IN ('LINE 01' , 'LINE 02') " +
                    " ORDER BY LINENO, QCORANKINGNEW ";
                if (pIncludeNegativeRank)
                    strSQL =
                    " SELECT * " +
                    " FROM PKMES.T_QC_QUEUE " +
                    " WHERE T_QC_QUEUE.QCOFACTORY = :QCOFACTORY " +
                    " AND T_QC_QUEUE.QCOYEAR = :QCOYEAR  " +
                    " AND T_QC_QUEUE.QCOWEEKNO = :QCOWEEKNO  " +
                    " AND LINENO IN ('LINE 01' , 'LINE 02') " +
                    " ORDER BY LINENO, QCORANKINGNEW ";
                List<OracleParameter> parameters = new List<OracleParameter>();
                parameters.Add(new OracleParameter("QCOFACTORY", pFactory));
                parameters.Add(new OracleParameter("QCOYEAR", pYear));
                parameters.Add(new OracleParameter("QCOWEEKNO", "W" + PCMGeneralFunctions.GetRight("00" + pWeekNo.ToString(), 2)));
                OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, pConnectionString);
                oracleDataAdapter.SelectCommand.Parameters.AddRange(parameters.ToArray());
                var _dt_T_QC_QUEUE = new DataTable();
                oracleDataAdapter.Fill(_dt_T_QC_QUEUE);
                var StartDate = new DateTime();
                var EndDate = new DateTime();
                decimal FactoryCAPA = 0;
                decimal OccupiedCAPA = 0;
                decimal ProductionTime = 0;
                //bool isZeroCAPA = true;
                FactoryCAPA = 0;
                decimal StyleOPHourlyRate = 0;
                int StyleOPManCount = 0;
                string Msg = "";
                string AccumMsg = "";
                Dictionary<string, string> OPTIMES = new Dictionary<string, string>();
                Dictionary<string, int> StyleManCount = new Dictionary<string, int>();
                Dictionary<string, int> StyleDailyRate = new Dictionary<string, int>();
                Dictionary<string, double> FactoryLineDailyUsageHour = new Dictionary<string, double>();
                Dictionary<string, int> FactoryLineHandleCAPA = new Dictionary<string, int>();
                int I = 0, J = 0, currentJ = 0;
                for (I = 0; I <= 100; I++)
                {  //Loop for 100 Weeks 
                    var ProcessWeek = pWeekNo + I;
                    var objFactoryCAPA = GetFactoryCAPA(pConnectionString, pFactory, pYear, ProcessWeek);
                    var objFactoryWeeklyEfficiency = GetFactoryEfficiency(pFactory, pYear, "W" + PCMGeneralFunctions.GetRight("00" + ProcessWeek.ToString(), 2));
                    var EfficiencyRate = objFactoryWeeklyEfficiency.EFFICIENCYPERCEN;
                    if (objFactoryCAPA != null)
                    {
                        StartDate = objFactoryCAPA.STARTDATE.AddHours(7);
                        EndDate = objFactoryCAPA.ENDDATE.AddHours(7);
                        FactoryCAPA = objFactoryCAPA.CAPACITY;
                    }
                    else
                    {
                        continue;
                    }
                    //var DateDiff = (EndDate - StartDate).Days;
                    string WaitForNextDate_LINE = "";
                    while (StartDate <= EndDate)
                    {
                        Console.WriteLine("StartDate= " + StartDate.ToString());
                        sb.AppendLine("");
                        sb.AppendLine("");
                        sb.AppendLine("StartDate= " + StartDate.ToString());
                        WaitForNextDate_LINE = "";
                        string LINENO = "";
                        FactoryLineDailyUsageHour.Clear();
                        var DailyWorkingHours = GetDailyWorkingHours(pFactory, StartDate.ToString("yyyyMMdd"));
                        if (!(DailyWorkingHours > 0))
                        {
                            sb.AppendLine("ZERO DailyWorkingHours on " + StartDate.ToString());
                            goto HE_ContNextDay;
                        }
                        for (J = 0; J < _dt_T_QC_QUEUE.Rows.Count; J++)
                        {
                            currentJ = J;
                            Console.WriteLine("currentJ= " + currentJ);
                            sb.AppendLine("");
                            sb.AppendLine("currentJ= " + currentJ);
                            DataRow _dr_T_QC_QUEUE = _dt_T_QC_QUEUE.Rows[J];
                            //Reset:
                            OccupiedCAPA = 0;
                            //Assign 
                            LINENO = _dr_T_QC_QUEUE["LINENO"].ToString();
                            var STYLECODE = _dr_T_QC_QUEUE["STYLECODE"].ToString();
                            var STYLESIZE = _dr_T_QC_QUEUE["STYLESIZE"].ToString();
                            var STYLECOLORSERIAL = _dr_T_QC_QUEUE["STYLECOLORSERIAL"].ToString();
                            var REVNO = _dr_T_QC_QUEUE["REVNO"].ToString();
                            sb.AppendLine("PRDPKG= " + _dr_T_QC_QUEUE["PRDPKG"].ToString() + ", PLANQTY= " + _dr_T_QC_QUEUE["PLANQTY"].ToString());
                            if (WaitForNextDate_LINE == LINENO)
                                continue;
                            else
                            {
                                if (FactoryLineHandleCAPA.ContainsKey(pFactory + LINENO))
                                {
                                    var Index = FactoryLineHandleCAPA[pFactory + LINENO];
                                    sb.AppendLine("pFactory + LINENO= " + pFactory + LINENO + ": " + Index);
                                    if (!(J > FactoryLineHandleCAPA[pFactory + LINENO]))
                                        continue;
                                }
                            }
                            var KEY = STYLECODE.ToString() + STYLESIZE.ToString() + STYLECOLORSERIAL.ToString() + REVNO.ToString() + pFactory;
                            #region GET STYLE OPTIME , STYLE DAILY RATE , STYLE MANCOUNT
                            if (!OPTIMES.ContainsKey(KEY))
                            {
                                //use Dictionary to prevent get the existing OPTIME from DB
                                Msg = "";
                                if (GetStyleOPTIME(STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, pFactory, out Msg))
                                {
                                    /* [Msg] contain the OPTIME Data of Input Style
									 * Format:  OPTIME + ";" + DAILYTARGET + ";" + MANCOUNT
									 */
                                    OPTIMES.Add(KEY, Msg);
                                    StyleDailyRate.Add(KEY, Convert.ToInt32(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[1]));
                                    StyleManCount.Add(KEY, Convert.ToInt32(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[2]));
                                    var OPTIME_StyleDailyRate = Convert.ToInt32(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[1]);
                                    //From PKERP.T_OP_OPTIME, DAILY TARGET use 7.5 as standard
                                    StyleOPHourlyRate = Math.Floor((decimal)OPTIME_StyleDailyRate / (decimal)7.5);
                                    StyleOPManCount = Convert.ToInt32(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[2]);
                                    Console.WriteLine("KEY= " + STYLECODE.ToString() + ";" + STYLESIZE.ToString() + ";" + STYLECOLORSERIAL.ToString() + ";" + REVNO.ToString() + ";" + pFactory);
                                    Console.WriteLine("OPTIME_StyleDailyRate= " + OPTIME_StyleDailyRate);
                                    Console.WriteLine("StyleOPHourlyRate= " + StyleOPHourlyRate);
                                    Console.WriteLine("StyleOPManCount= " + StyleOPManCount);
                                    sb.AppendLine("KEY= " + STYLECODE.ToString() + ";" + STYLESIZE.ToString() + ";" + STYLECOLORSERIAL.ToString() + ";" + REVNO.ToString() + ";" + pFactory);
                                    sb.AppendLine("OPTIME_StyleDailyRate= " + OPTIME_StyleDailyRate);
                                    sb.AppendLine("StyleOPHourlyRate= " + StyleOPHourlyRate);
                                    sb.AppendLine("StyleOPManCount= " + StyleOPManCount);
                                    Msg = "";
                                }
                                else
                                    Msg = "GetStyleOPTIME(" + KEY + ") FAIL: " + Msg;
                            }
                            else
                            {
                                var OPTIME_StyleDailyRate = StyleDailyRate[KEY];
                                //From PKERP.T_OP_OPTIME, DAILY TARGET use 7.5 as standard
                                StyleOPHourlyRate = Math.Floor((decimal)OPTIME_StyleDailyRate / (decimal)7.5);
                                StyleOPManCount = StyleManCount[KEY];
                                Console.WriteLine("FROM EXISTING DICTIONARY");
                                Console.WriteLine("KEY= " + STYLECODE.ToString() + ";" + STYLESIZE.ToString() + ";" + STYLECOLORSERIAL.ToString() + ";" + REVNO.ToString() + ";" + pFactory);
                                Console.WriteLine("OPTIME_StyleDailyRate= " + OPTIME_StyleDailyRate);
                                Console.WriteLine("StyleOPHourlyRate= " + StyleOPHourlyRate);
                                Console.WriteLine("StyleOPManCount= " + StyleOPManCount);
                                sb.AppendLine("FROM EXISTING DICTIONARY");
                                sb.AppendLine("KEY= " + STYLECODE.ToString() + ";" + STYLESIZE.ToString() + ";" + STYLECOLORSERIAL.ToString() + ";" + REVNO.ToString() + ";" + pFactory);
                                sb.AppendLine("OPTIME_StyleDailyRate= " + OPTIME_StyleDailyRate);
                                sb.AppendLine("StyleOPHourlyRate= " + StyleOPHourlyRate);
                                sb.AppendLine("StyleOPManCount= " + StyleOPManCount);
                            }
                            /* if Any ERROR, go to next QCO Ranking Row */
                            if (!String.IsNullOrEmpty(Msg))
                            {
                                if (String.IsNullOrEmpty(AccumMsg))
                                    AccumMsg = Msg;
                                else
                                    AccumMsg = AccumMsg + "<BR/>" + Msg;
                                //To next QCO Rank Row
                                continue;
                            }
                            #endregion
                            var PackageQty = (decimal)_dr_T_QC_QUEUE["PLANQTY"];
                            ProductionTime = PackageQty / StyleOPHourlyRate;
                            //ver 2
                            ProductionTime = PackageQty / StyleOPHourlyRate;
                            OccupiedCAPA = ProductionTime * StyleOPManCount * (2 - EfficiencyRate * (decimal)0.01);
                            Console.WriteLine("ProductionTime= " + ProductionTime);
                            Console.WriteLine("OccupiedCAPA= " + OccupiedCAPA);
                            sb.AppendLine("ProductionTime= " + ProductionTime);
                            sb.AppendLine("OccupiedCAPA= " + OccupiedCAPA);
                            if (FactoryCAPA > OccupiedCAPA)
                            {
                                if (FactoryLineDailyUsageHour.ContainsKey(pFactory + LINENO))
                                {
                                    if ((double)DailyWorkingHours < FactoryLineDailyUsageHour[pFactory + LINENO] + (double)ProductionTime)
                                    {
                                        WaitForNextDate_LINE = LINENO;
                                        sb.AppendLine("DailyWorkingHours[" + DailyWorkingHours + "] < FactoryLineDailyUsageHour[pFactory + LINENO] + (double)ProductionTime[" + FactoryLineDailyUsageHour[pFactory + LINENO] + (double)ProductionTime + "]");
                                        //continue;
                                    }
                                    _dr_T_QC_QUEUE["QCOPLANSTARTDATE"] = StartDate.AddHours(FactoryLineDailyUsageHour[pFactory + LINENO]);
                                    _dr_T_QC_QUEUE["QCOPLANENDDATE"] = StartDate.AddHours(FactoryLineDailyUsageHour[pFactory + LINENO]).AddHours(Convert.ToDouble(ProductionTime));
                                    FactoryLineDailyUsageHour[pFactory + LINENO] = FactoryLineDailyUsageHour[pFactory + LINENO] + (double)ProductionTime;
                                }
                                else
                                {
                                    if ((decimal)DailyWorkingHours < ProductionTime)
                                    {
                                        WaitForNextDate_LINE = LINENO;
                                        sb.AppendLine("DailyWorkingHours[" + DailyWorkingHours + "] < ProductionTime[" + ProductionTime + "]");
                                        //continue;
                                    }
                                    _dr_T_QC_QUEUE["QCOPLANSTARTDATE"] = StartDate;
                                    _dr_T_QC_QUEUE["QCOPLANENDDATE"] = StartDate.AddHours(Convert.ToDouble(ProductionTime)); ;
                                    FactoryLineDailyUsageHour[pFactory + LINENO] = (double)ProductionTime;
                                }
                                _dr_T_QC_QUEUE["OPTIME"] = StyleOPHourlyRate; //Unit is Hour 
                                _dr_T_QC_QUEUE["MANCOUNT"] = StyleOPManCount;
                                #region TBD_Part
                                //_dr_T_QC_QUEUE["PKGOPTIME"] = ProductionTime * ( 2 - EfficiencyRate * (decimal)0.1);
                                //_dr_T_QC_QUEUE["PKGMANCOUNT"] = 0;
                                #endregion
                                _dr_T_QC_QUEUE["BEGINCAPA"] = FactoryCAPA;
                                _dr_T_QC_QUEUE["USAGECAPA"] = OccupiedCAPA;
                                FactoryCAPA = FactoryCAPA - OccupiedCAPA; //Deduct the Factory Capacity 
                                _dr_T_QC_QUEUE["BALANCECAPA"] = FactoryCAPA;
                                _dr_T_QC_QUEUE["BALANCECAPA"] = FactoryCAPA;
                                FactoryLineHandleCAPA[pFactory + LINENO] = J;
                            }
                            else
                            {
                                sb.AppendLine("FactoryCAPA[" + FactoryCAPA + "] < OccupiedCAPA[" + OccupiedCAPA + "]");
                            }
                        }
                    HE_ContNextDay:
                        //Line-code to prevent the Endless loop
                        //Next Day inside a-week loop
                        StartDate = StartDate.AddDays(1);
                    }
                    //Loop to next WEEK NO.
                }
                OPTIMES.Clear();
                StyleManCount.Clear();
                StyleDailyRate.Clear();
                OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                oracleDataAdapter.Update(_dt_T_QC_QUEUE);
                oracleCommandBuilder.Dispose();
                _dt_T_QC_QUEUE.Dispose();
                oracleDataAdapter.Dispose();
                pMessage = sb.ToString();
                sb.Clear();
                return blResult;
            }
            catch (Exception ex)
            {
                pMessage = "Error On Factory:" + pFactory + ", Year:" + pYear.ToString() + ", Week:" + pWeekNo.ToString() + " ; Description:" + ex.Message;
                return false;
            }
        }
        private int GetDailyWorkingHours(string pFactory, string pDate)
        {
            /* 
			 * pDate has Format "yyyyMMdd"
			 */
            int WorkingTime = 0;
            using (OracleConnection oracleConn = new OracleConnection(_PKMESConnString))
            {
                oracleConn.Open();
                var strSQL =
                    " SELECT * " +
                    " FROM PKERP.VIEW_MTOP_FATOYDAILYWRKTIME " +
                    " WHERE FACTORY = :FACTORY " +
                    " AND MONTHNO || PLANDAY = :MDATE " +
                    "";
                List<OracleParameter> parameters = new List<OracleParameter>();
                parameters.Add(new OracleParameter("FACTORY", pFactory));
                parameters.Add(new OracleParameter("MDATE", pDate));
                DataTable dt = new DataTable();
                PCMOracleLibrary.StatementToDataTable(oracleConn, strSQL, parameters, out dt, out strSQL);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                        WorkingTime = Convert.ToInt32(dt.Rows[0]["MINWORKTIME"]);
                    dt.Dispose();
                }
                oracleConn.Close();
            }
            return WorkingTime;
        }
        public bool GetStyleOPTIME(string pStyleCode, string pStyleSize, string pStyleColorSerial, string pRevNo, string pFactory, out string pMsg)
        {
            pMsg = "";
            try
            {
                var strSQL =
                    " SELECT * " +
                    " FROM " +
                    " ( SELECT CASE WHEN FACTORY = '0000' THEN 1 ELSE 0 END as FPriority , " +
                    " T_OP_OPTIME.* " +
                    " FROM PKERP.T_OP_OPTIME " +
                    " WHERE STYLECODE = :STYLECODE " +
                    " AND STYLESIZE = :STYLESIZE " +
                    " AND STYLECOLORSERIAL = :STYLECOLORSERIAL " +
                    " AND REVNO = :REVNO " +
                    " AND ( FACTORY = :FACTORY OR FACTORY = '0000' ) " +
                    " ) MAIN " +
                    " ORDER BY FPriority " +
                    " ";
                using (OracleConnection oracleConn = new OracleConnection(_PKMESConnString))
                {
                    oracleConn.Open();
                    DataTable dt = new DataTable();
                    List<OracleParameter> parameters = new List<OracleParameter>();
                    parameters.Add(new OracleParameter("STYLECODE", pStyleCode));
                    parameters.Add(new OracleParameter("STYLESIZE", pStyleSize));
                    parameters.Add(new OracleParameter("STYLECOLORSERIAL", pStyleColorSerial));
                    parameters.Add(new OracleParameter("REVNO", pRevNo));
                    parameters.Add(new OracleParameter("FACTORY", pFactory));
                    PCMOracleLibrary.StatementToDataTable(oracleConn, strSQL, parameters, out dt, out pMsg);
                    if (dt != null)
                    {
                        if (dt.Rows.Count > 0)
                        {
                            var OPTIME = Convert.ToDecimal(dt.Rows[0]["OPTIME"]); //At this step, Unit is SECOND 
                            var DAILYTARGET = Convert.ToDecimal(dt.Rows[0]["HOURLYTARGET"]);
                            var MANCOUNT = Convert.ToDecimal(dt.Rows[0]["MANCOUNT"]);
                            var TAKETIME = Convert.ToDecimal(dt.Rows[0]["MAXTIME"]); //2019-10-08
                            pMsg = OPTIME + ";" + DAILYTARGET + ";" + MANCOUNT + ";" + TAKETIME;
                        }
                        dt.Dispose();
                    }
                    oracleConn.Close();
                }
                //Convert OPTIME From  Second to Hour
                //OPTIME = OPTIME / 3600;
                return true;
            }
            catch (Exception ex)
            {
                pMsg = ex.Message;
                return false;
            }
        }
        public FWCP GetFactoryCAPA(string pConnectionString, string pFactory, int pYear, int pWeekNo)
        {
            try
            {
                var strSQL =
                    " Select * From PKMES.T_CM_FWCP " +
                    " Where FACTORY=:FACTORY " +
                    " And YEAR = :YEAR " +
                    " And WEEKNO = :WEEKNO ";
                var Msg = "";
                List<OracleParameter> parameters = new List<OracleParameter>();
                parameters.Add(new OracleParameter("FACTORY", pFactory));
                parameters.Add(new OracleParameter("YEAR", pYear));
                parameters.Add(new OracleParameter("WEEKNO", pWeekNo));
                return PCMOracleLibrary.QueryToOneObject<FWCP>(pConnectionString, strSQL, parameters.ToArray(), out Msg);
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                return new FWCP();
            }
        }
        public List<FWCP> GetAllFactoryCAPA(string pFactory, int pYear, int pWeekNo)
        {
            try
            {
                var strSQL =
                    $" Select * From PKMES.T_CM_FWCP " +
                    $" Where FACTORY= '{pFactory}' " +
                    $" And YEAR || ( CASE WHEN WEEKNO <10 THEN '0' || TO_CHAR(WEEKNO) ELSE TO_CHAR(WEEKNO) END ) >= '{pYear}{pWeekNo}' " +
                    $" Order By YEAR , WEEKNO ";
                var Msg = "";
                //List<OracleParameter> parameters = new List<OracleParameter>();
                //parameters.Add(new OracleParameter("FACTORY", pFactory));
                //parameters.Add(new OracleParameter("YEAR", pYear));
                //parameters.Add(new OracleParameter("WEEKNO", pWeekNo)); 
                return PCMOracleLibrary.QueryToObject<FWCP>(_PKMESConnString, strSQL, null, out Msg);
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                return null;
            }
        }
        public FWES GetFactoryEfficiency(string pFactory, int pYear, string pWeekNo)
        {
            try
            {
                //var strSQL =
                //    " Select * " +
                //    " From PKMES.T_CM_FWES " +
                //    " Where FACTORY=:FACTORY " +
                //    " And YEAR >= :YEAR " +
                //    " And WEEKNO >= :WEEKNO " +
                //    " ORDER BY FACTORY DESC , YEAR DESC ,  WEEKNO DESC ";
                var strSQL =
                    " Select * " +
                    " From PKMES.T_CM_FWES " +
                    " Where FACTORY=:FACTORY " +
                    " And YEAR = :YEAR " +
                    " And WEEKNO = :WEEKNO " +
                    " ORDER BY FACTORY DESC , " +
                    " YEAR DESC ,  " +
                    " WEEKNO DESC ";
                var Msg = "";
                List<OracleParameter> parameters = new List<OracleParameter>();
                parameters.Add(new OracleParameter("FACTORY", pFactory));
                parameters.Add(new OracleParameter("YEAR", pYear));
                parameters.Add(new OracleParameter("WEEKNO", pWeekNo));
                return PCMOracleLibrary.QueryToOneObject<FWES>(_PKMESConnString, strSQL, parameters.ToArray(), out Msg);
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                return null;
            }
        }
        public void UpdateWeeklyCapacityFromMTOPS(string pUserID, string modalWeekCapaFactory, string modalWeekCapaYear, string modalWeekCapaFromWeek, string modalWeekCapaToWeek)
        {
            try
            {   //2020-07-07 Tai Le(Thomas)

                //2020-10-07 Tai Le(Thomas): Add clarification
                if (String.IsNullOrEmpty(modalWeekCapaFromWeek) || String.IsNullOrEmpty(modalWeekCapaToWeek))
                {
                    return;
                }

                int intmodalWeekCapaFromWeek = 0, intmodalWeekCapaToWeek = 0;
                if (!int.TryParse(modalWeekCapaFromWeek, out intmodalWeekCapaFromWeek))
                    return;

                if (!int.TryParse(modalWeekCapaToWeek, out intmodalWeekCapaToWeek))
                    return;
                //::END     2020-10-07 Tai Le(Thomas): Add clarification

                if (String.IsNullOrEmpty(modalWeekCapaYear))
                {
                    modalWeekCapaYear = DateTime.Today.Year.ToString();
                }

                var arrmodalWeekCapaFactory = modalWeekCapaFactory.Split(';');
                string Msg = "";//, AccumMsg = "";

                for (int _i = 0; _i < arrmodalWeekCapaFactory.Length; _i++)
                {
                    //2020-10-07 Tai Le(Thomas): Move 2 line codes above
                    //var intmodalWeekCapaFromWeek = Convert.ToInt32(modalWeekCapaFromWeek);
                    //var intmodalWeekCapaToWeek = Convert.ToInt32(modalWeekCapaToWeek);

                    //2020-10-07 Tai Le(Thomas): Handle across Year 
                    int LoopCounter = 1;

                    if (intmodalWeekCapaFromWeek > intmodalWeekCapaToWeek)
                        LoopCounter = 2;

                    for (int count = 1; count <= LoopCounter; count++)
                    {
                        if (count == 1)
                        {
                            intmodalWeekCapaToWeek = 53; // 1 year only has max 53 Week
                        }
                        else if (count == 2)
                        {
                            int _tempY = int.Parse(modalWeekCapaYear) + 1;
                            modalWeekCapaYear = _tempY.ToString();
                            intmodalWeekCapaFromWeek = 1;
                            intmodalWeekCapaToWeek = Convert.ToInt32(modalWeekCapaToWeek);
                        }

                        for (int I = intmodalWeekCapaFromWeek; I <= intmodalWeekCapaToWeek; I++)
                        {
                            /** Source from AO/MTOPS */
                            var strSQL = @"
    SELECT
        A.* ,
        B.WeekStartDate ,
        B.WeekEndDate
        , C.WORKER , C.SEWER , 
        A.MINWORKTIME * C.WORKER AS MAXCapacity , 
        A.MINWORKTIME * C.SEWER AS SEWCapacity
        FROM (
        SELECT
            FACTORY, MIN(MONTHNO) MONTHNO, WEEKNO , SUM(MINWORKTIME) MINWORKTIME , SUM(MAXWORKTIME) MAXWORKTIME
        FROM
            (
            SELECT
                Fatoy FACTORY, MOTHNO AS MONTHNO, WEEKNO , PLNDAY , MIN( REDTME ) AS MINWORKTIME , MAX( MORTME + ARNTME + OVRTME ) AS MAXWORKTIME
            FROM
                PKAMT.MT_CALMST_TBL@MTOPSDB
            WHERE 1=1 
                AND MOTHNO  LIKE :YEAR || '%'
                AND WEEKNO = :WEEKNO
                AND FATOY LIKE :FATOY
            GROUP BY
                Fatoy , MOTHNO , WEEKNO , PLNDAY )SubA
        GROUP BY
            FACTORY, WEEKNO )A
    LEFT JOIN (
        SELECT
            FATOY AS FACTORY , MIN(MOTHNO) AS MONTHNO , WEEKNO , MIN(MOTHNO || PLNDAY ) WeekStartDate , MAX(MOTHNO || PLNDAY) WeekEndDate
        FROM
            PKAMT.MT_CALMST_TBL@MTOPSDB
        WHERE 1=1  
                AND MOTHNO  LIKE :YEAR || '%'
                AND WEEKNO = :WEEKNO
                AND FATOY LIKE :FATOY
        GROUP BY
            FATOY , WEEKNO )B ON
        A.FACTORY = B.FACTORY
        AND A.MONTHNO = B.MONTHNO
        AND A.WEEKNO = B.WEEKNO 
    LEFT JOIN PKAMT.MT_FATWRKR_TBL@MTOPSDB C ON 
    A.FACTORY = C.FATOY 
    AND A.MONTHNO = C.MOTHNO 
    ";
                            if (String.IsNullOrEmpty(modalWeekCapaYear))
                            {
                                modalWeekCapaYear = DateTime.Today.Year.ToString();
                            }
                            List<OracleParameter> parameters = new List<OracleParameter>();
                            parameters.Add(new OracleParameter("YEAR", modalWeekCapaYear));
                            var strWeekNo = ("00" + I);
                            parameters.Add(new OracleParameter("WEEKNO", strWeekNo.Substring(strWeekNo.Length - 2, 2)));
                            parameters.Add(new OracleParameter("FATOY", arrmodalWeekCapaFactory[_i]));
                            var _dt = new DataTable();
                            PCMOracleLibrary.StatementToDataTable(
                                vOracleConnString: _PKERPConnString,
                                vstrQuery: strSQL,
                                vParameters: parameters,
                                vdtDataTable: out _dt,
                                vstrErrorMsg: out Msg);
                            //string AccMsg = "", Msg = "";
                            if (_dt != null)
                                foreach (DataRow _dr in _dt.Rows)
                                {
                                    var objFWCP = new FWCP();
                                    //objFWCP.FACTORY = _dr["FATOY"].ToString(); //pFactory;
                                    objFWCP.FACTORY = _dr["FACTORY"].ToString(); //pFactory;
                                    objFWCP.YEAR = Convert.ToInt32(modalWeekCapaYear);
                                    objFWCP.WEEKNO = I;
                                    objFWCP.STARTDATE = DateTime.ParseExact(_dr["WeekStartDate"].ToString(), "yyyyMMdd", new CultureInfo(""));
                                    objFWCP.ENDDATE = DateTime.ParseExact(_dr["WeekEndDate"].ToString(), "yyyyMMdd", new CultureInfo(""));
                                    objFWCP.TOTALWORKERS = Convert.ToInt32(_dr["WORKER"]); // Worker of Start Date
                                    objFWCP.TOTALSEWER = Convert.ToInt32(_dr["SEWER"]);// Sewer of Start Date
                                    objFWCP.CAPACITY = Convert.ToDecimal(_dr["MAXCapacity"]);
                                    objFWCP.SEWERCAPA = Convert.ToDecimal(_dr["SEWCapacity"]);
                                    objFWCP.TOTALMACHINES = 0;
                                    objFWCP.CREATOR = pUserID;
                                    objFWCP.TOTALWORKHOUR = Convert.ToDecimal(_dr["MINWORKTIME"]);

                                    //Update
                                    objFWCP.Update(_PKMESConnString, out Msg);

                                }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
        #endregion

        #region Simulation QCO Code 
        //New set of tables {T_QC_QCFPSIM ; T_QC_QCPMSIM ; T_QC_QCPSSIM ; T_QC_QUEUESIM }
        public void DeleteQCODataSIM(string pOracleCnnString, string pQCOFactory, int pQCOYear, string pQCOWeekNo)
        {
            var strSQL = @"
DELETE PKMES.T_QC_QCFPSIM 
Where QCOFactory =  :QCOFactory 
AND QCOYear = :QCOYear
AND QCOWeekNo = :QCOWeekNo 
";
            List<OracleParameter> parameters = new List<OracleParameter>();
            parameters.Add(new OracleParameter("QCOFactory", pQCOFactory));
            parameters.Add(new OracleParameter("QCOYear", pQCOYear));
            parameters.Add(new OracleParameter("QCOWeekNo", pQCOWeekNo));
            PCMOracleLibrary.StatementExecution(pOracleCnnString, strSQL, parameters, out strSQL);
            /////////////////////////////////////////////////////////////////////////////////////////////
            strSQL = @"
DELETE PKMES.T_QC_QCPMSIM 
Where QCOFactory =  :QCOFactory 
AND QCOYear = :QCOYear
AND QCOWeekNo = :QCOWeekNo 
";
            PCMOracleLibrary.StatementExecution(pOracleCnnString, strSQL, parameters, out strSQL);
            /////////////////////////////////////////////////////////////////////////////////////////////
            strSQL = @"
DELETE PKMES.T_QC_QCPSSIM 
Where QCOFactory =  :QCOFactory 
AND QCOYear = :QCOYear
AND QCOWeekNo = :QCOWeekNo 
";
            PCMOracleLibrary.StatementExecution(pOracleCnnString, strSQL, parameters, out strSQL);
            /////////////////////////////////////////////////////////////////////////////////////////////
            strSQL = @"
DELETE PKMES.T_QC_QUEUESIM 
Where QCOFactory =  :QCOFactory 
AND QCOYear = :QCOYear
AND QCOWeekNo = :QCOWeekNo 
";
            PCMOracleLibrary.StatementExecution(pOracleCnnString, strSQL, parameters, out strSQL);
        }
        public void QCOCalculationSIM(string Factory, string UserID, string UserRole, out string retMessage)
        {
            retMessage = "";
            if (String.IsNullOrEmpty(_PKERPConnString) || String.IsNullOrEmpty(_PKMESConnString))
            {
                retMessage = "Connection String NOT Defined.";
                return;
            }
            /* 
			 * Purpose: Calculate the Material Readiness For MTOPS Packages In Chosen FACTORY
			 * Input:   Factory
			 * Output:  The MTOPS Package is ranked based on the Material Readiness
			 * 
			 * :: Pre-processing
			 *      1. Check the Factory QCO Running Status on Table  PKMES.T_QC_QCFR
			 *          1.1 If Status = "RUNNING"
			 *              >> Quit and Return the message "Factory is running by <EXECUTINGBY> at <EXECUTINGDATE>
			 *          1.2 If Status = "DONE"
			 *              >> Continue the Next Process
			 *      2. Check the Availability of Factory Sorting Parameters on table PKMES.T_00_QCFO
			 *          2.1 If No Factory Setting
			 *              >> Quit and Return the message "Please set up the QCO Sorting Parameters on Factory <Factory>"
			 *      3. Check, Whether QCO Factory + QCO Week   EXISTING 
			 * :: Processing
			 *      1. From View  "PKERP.VIEW_ERP_PSRSNP_PLAN", look for all the INCOMPLETE Packages
			 *      2. Save the satisfied Package into PKMES.T_QC_QCFP
			 *      3. Sort the PKMES.T_QC_QCFP based on The Factory Sorting Parameters on table PKMES.T_00_QCFO
			 *      4. Based on PKMES.T_QC_QCFP , PKERP.T_SD_BOMT ,  PKERP.V_WMS_PORC , Distribute the Received Qty for Each Package
			 *          4.1. PKMES.T_QC_QCFP  Join  PKERP.T_SD_BOMT together, and order by "Factory Sorting Parameters" , T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL
			 * :: Post-processing
			 */
            //string strResult = "";
            string strSQL = "",
                strErrorMessage = "";
            bool blHasError = false;
            bool blUpdateQCOJobStatus = true;
            //::: Get WeekNo
            DateTime dtStarDateTime = DateTime.Now;
            //2019-06-14 Tai Le (Thomas) : Handle Single PP Material Readiness
            //2019-06-14 Tai Le (Thomas): use  intYear to Replace "dtStarDateTime.Year", in this way, able to re-use for SinglePackage
            int QCOYear = dtStarDateTime.Year;
            CultureInfo cul = CultureInfo.CurrentCulture;
            int weekNum = cul.Calendar.GetWeekOfYear(dtStarDateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            string strWeekNum = "W" + PCMGeneralFunctions.GetRight("00" + weekNum, 2);
            /*2019-12-31 Tai Le (Thomas) Handle Year & Week when crossing to next year*/
            //Get Monday of Each Week
            var dtMonday = dtStarDateTime.StartOfWeek(DayOfWeek.Monday);
            var dtWed = dtMonday.AddDays(2);
            if (dtWed.Year > dtStarDateTime.Year)
                strWeekNum = "W01";
            QCOYear = dtWed.Year;
            /*:::END    2019-12-31 Tai Le (Thomas) Handle Year & Week when crossing to next year*/
            if (UserRole == null)
            {
                strErrorMessage = "Can not find User Role to Calculate QCO Ranking.";
                goto HE_Exit_QCOCalculate;
            }
            if (UserRole != "5000")
            {
                strErrorMessage = "Wrong Role to Calculate QCO Ranking. Please log in with Role 5000.";
                goto HE_Exit_QCOCalculate;
            }
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(_PKMESConnString))
                {
                    oracleConnection.Open();
                    //::: Pre - processing
                    //* 1.1 If Status = "RUNNING"
                    //*     >> Quit and Return the message "Factory is running by <EXECUTINGBY> at <EXECUTINGDATE> 
                    if (mEnviroment == "Console")
                        Console.WriteLine("Check [PKMES.T_QC_QCFRSIM]");
                    strSQL = " SELECT EXECUTINGBY , EXECUTINGDATE " +
                                    " FROM PKMES.T_QC_QCFRSIM " +
                                    " WHERE FACTORY = :FACTORY " +
                                    " AND STATUS IS NOT NULL " +
                                    " AND STATUS = 'RUNNING' ";
                    DataTable dt = new DataTable();
                    PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, new List<OracleParameter> { new OracleParameter("FACTORY", Factory) }, out dt, out strErrorMessage);
                    if (dt != null)
                        if (dt.Rows.Count > 0)
                        {
                            var ExecuteDate = dt.Rows[0][1].ToString();
                            DateTime dtExecuteDate = DateTime.Parse(ExecuteDate);
                            /* 2019/02/26: Tai Le (Thomas) modify the Rule
							 * Nếu Hiện Tại (Now) <= Execute +1 Day >> coi như process QCO đang chạy; không cho phép chạy QCO Calculation mới
							 * Nếu Hiện Tại (Now) > Execute +1 Day >> coi như process QCO Expired; cho phép chạy QCO Calculation mới 
							 */
                            if (DateTime.Now <= dtExecuteDate.AddHours(2))
                            {
                                strErrorMessage = "Factory '" + Factory + "' Being Run QCO Calculation By " + dt.Rows[0][0] + " at " + dt.Rows[0][1];
                                blHasError = true;
                            }
                            dt.Dispose();
                        }
                    if (blHasError)
                    {
                        blUpdateQCOJobStatus = false;
                        goto HE_QCOCalculate_Complete;
                    }
                    //* 2. Check the Availability of Factory Sorting Parameters on table PKMES.T_00_QCFO
                    //* 2.1 If No Factory Setting
                    //*     >> Quit and Return the message "Please set up the QCO Sorting Parameters on Factory <Factory>"
                    strSQL = " SELECT * " +
                             " FROM PKMES.T_00_QCFO " +
                             " WHERE FACTORY = :FACTORY ";
                    if (dt == null)
                        dt = new DataTable();
                    PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, new List<OracleParameter> { new OracleParameter("FACTORY", Factory) }, out dt, out strSQL);
                    if (dt != null)
                        if (dt.Rows.Count == 0)
                        {
                            strErrorMessage = "Factory QCO Sorting Setting On '" + Factory + "' Empty. Please Set Up QCO Setting.";
                            blHasError = true;
                            dt.Dispose();
                        }
                    if (blHasError)
                    {
                        goto HE_QCOCalculate_Complete;
                    }
                    /*::Processing
					 * 1. From View  "PKERP.VIEW_ERP_PSRSNP_PLAN", look for all the INCOMPLETE Packages
					 * 2. Save the satisfied Package into PKMES.T_QC_QCFP
					 * 3. Sort the PKMES.T_QC_QCFP based on The Factory Sorting Parameters on table PKMES.T_00_QCFO
					 * 4. Based on PKMES.T_QC_QCFP , PKERP.T_SD_BOMT ,  PKERP.V_WMS_PORC , Distribute the Received Qty for Each Package
					 *      4.1. PKMES.T_QC_QCFP  Join  PKERP.T_SD_BOMT together, and order by "Factory Sorting Parameters" , T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL
					 */
                    DeleteQCODataSIM(_PKMESConnString, Factory, QCOYear, strWeekNum);
                    if (mEnviroment == "Console")
                        Console.WriteLine("DeleteSimQCOData(): PASSED");
                    var WeekMonday = PCMGeneralFunctions.GetDateFromWeekNumberAndDayOfWeek(QCOYear, weekNum, 0).Date;
                    //::: Insert the Flag to mark the Running QCO Factory 
                    Insert_T_QC_QCFRSIM(_PKMESConnString, Factory, UserID);
                    if (mEnviroment == "Console")
                        Console.WriteLine("Insert_T_QC_QCFRSIM(): PASSED");
                    //::: Get Factory QCO Setting:
                    List<Qcfo> LcAllFactoryParameters = null;
                    List<Qcfo> LcNoMateialFactoryParameters = null;
                    FactoryHasMaterialParameter(Factory, out LcAllFactoryParameters, out LcNoMateialFactoryParameters);
                    //::: Get MTOPS Package From  Chosen Factory  
                    var _tempMsg = "";
                    dt = GetMTOPSPackage(Factory, out _tempMsg);
                    if (mEnviroment == "Console")
                        Console.WriteLine("GetMTOPSPackage(): PASSED");
                    if (dt == null || dt.Rows.Count == 0)
                    {
                        strErrorMessage = "No AO-MTOPS Package Found Factory [" + Factory + "].";
                        blHasError = true;
                    }
                    if (blHasError)
                    {
                        goto HE_QCOCalculate_Complete;
                    }
                    //::: Sort MTOPS Package with Parameter Before Material Readiness 
                    Sort_T_QC_QCFP(ref dt, LcNoMateialFactoryParameters, "First");
                    if (mEnviroment == "Console")
                        Console.WriteLine("Sort_T_QC_QCFP(): PASSED");
                    //::: Save Sorted PP Into PKMES.T_QC_QCFPSIM 
                    Save_T_QC_QCFPSIM(_PKMESConnString, Factory, WeekMonday, QCOYear, strWeekNum, dt);
                    if (mEnviroment == "Console")
                        Console.WriteLine("Save_T_QC_QCFPSIM(): PASSED");
                    if (dt != null)
                    {
                        dt.Clear();
                        dt.Dispose();
                    }
                    //::: Distribute the Material into MTOPS Package of PKMES.T_QC_QCFP
                    //::: Get PP & T_SD_BOMT
                    //2020-08-11 Tai Le(Thomas):big change to prevent the duplicate row
                    //Original
                    //                    strSQL = " SELECT ROW_NUMBER() OVER(PARTITION BY T_QC_QCFP.FACTORY ORDER BY T_QC_QCFP.FACTORY, T_QC_QCFP.DELIVERYDATE , T_QC_QCFP.ORDQTY ,  T_QC_QCFP.PLANQTY , T_QC_QCFP.AONO , T_QC_QCFP.STYLECODE , T_QC_QCFP.STYLESIZE , T_QC_QCFP.STYLECOLORSERIAL , T_QC_QCFP.REVNO , T_QC_QCFP.PRDPKG ) AS RowSeqNo , " +
                    //                             " LEAD(T_QC_QCFP.ID, 1, '') OVER (ORDER BY T_QC_QCFP.ID) AS NEXT_ID , " +
                    //                             " T_QC_QCFP.* , V_MRP_PP_WO.WONO , " +
                    //                             " T_SD_BOMT.MAINITEMCODE , T_SD_BOMT.MAINITEMCOLORSERIAL , " +
                    //                             " T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL , " +
                    //                             " T_QC_QCFP.PLANQTY * T_SD_BOMT.UNITCONSUMPTION AS REQUESTQTY , " +
                    //                             " LEAD(T_QC_QCFP.ID, 1, 0) OVER (ORDER BY T_QC_QCFP.ID) AS NEXT_ID , " +
                    //                             " T_00_ICMT.MAINLEVEL , " +
                    //                             " CASE WHEN T_00_ICMT.MAINLEVEL IN ('FAB','LTR') THEN 'Y' ELSE NULL END as STAR_LEV3 , " +
                    //                             " CASE WHEN T_00_ICMT.MAINLEVEL IN ('BST','LBL', 'MTL') THEN 'Y' ELSE NULL END as STAR_LEV2 , " +
                    //                             " CASE WHEN T_00_ICMT.MAINLEVEL NOT IN ('FAB','LTR','BST','LBL', 'MTL') THEN 'Y' ELSE NULL END as STAR_LEV1 , " +
                    //" CASE WHEN A1.STYLECODE IS NOT NULL THEN 'Y' " +
                    //" ELSE  " +
                    //"     CASE WHEN A2.STYLECODE IS NOT NULL THEN 'Y' " +
                    //"     ELSE  " +
                    //"         CASE WHEN A3.STYLECODE IS NOT NULL THEN 'Y' " +
                    //"         ELSE  " +
                    //"             CASE WHEN A4.STYLECODE IS NOT NULL THEN 'Y' " +
                    //"             ELSE  " +
                    //"                 CASE WHEN B1.STYLECODE IS NOT NULL THEN 'Y' " +
                    //"                 ELSE  " +
                    //"                     CASE WHEN B2.STYLECODE IS NOT NULL THEN 'Y' " +
                    //"                     ELSE  " +
                    //"                         CASE WHEN B3.STYLECODE IS NOT NULL THEN 'Y' " +
                    //"                         ELSE  " +
                    //"                             CASE WHEN B4.STYLECODE IS NOT NULL THEN 'Y' " +
                    //"                             ELSE 'N' " +
                    //"                             END " +
                    //"                         END " +
                    //"                     END " +
                    //"                 END " +
                    //"             END " +
                    //"         END " +
                    //"     END  " +
                    //" END SOPREADINESS_DB " +
                    //                             " FROM PKMES.T_QC_QCFPSIM T_QC_QCFP " +
                    //                             " LEFT JOIN PKMES.V_MRP_PP_WO ON " +
                    //                             "      T_QC_QCFP.STYLECODE = V_MRP_PP_WO.STLCD " +
                    //                             "      AND T_QC_QCFP.STYLESIZE = V_MRP_PP_WO.STLSIZ " +
                    //                             "      AND T_QC_QCFP.STYLECOLORSERIAL = V_MRP_PP_WO.STLCOSN " +
                    //                             "      AND T_QC_QCFP.REVNO = V_MRP_PP_WO.STLREVN " +
                    //                             "      AND T_QC_QCFP.PRDPKG = V_MRP_PP_WO.PRODPACKAGE " +
                    //                             " INNER JOIN PKERP.T_SD_BOMT ON " +
                    //                             "      T_QC_QCFP.STYLECODE = T_SD_BOMT.STYLECODE " +
                    //                             "      AND T_QC_QCFP.STYLESIZE = T_SD_BOMT.STYLESIZE " +
                    //                             "      AND T_QC_QCFP.STYLECOLORSERIAL = T_SD_BOMT.STYLECOLORSERIAL " +
                    //                             "      AND T_QC_QCFP.REVNO = T_SD_BOMT.REVNO " +
                    //                             " INNER JOIN PKERP.T_00_ICMT ON " +
                    //                             "      T_SD_BOMT.ITEMCODE = T_00_ICMT.ITEMCODE " +
                    //" LEFT JOIN  PKERP.t_sd_file A1 ON " +
                    //"     T_QC_QCFP.STYLECODE = A1.STYLECODE AND T_QC_QCFP.STYLESIZE = A1.STYLESIZE AND T_QC_QCFP.STYLECOLORSERIAL = A1.STYLECOLORSERIAL AND T_QC_QCFP.REVNO = A1.REVNO " +
                    //"     AND A1.uploadcode='012'  AND A1.confirmed=1 " +
                    //" LEFT JOIN  PKERP.t_sd_file A2 ON  " +
                    //"     T_QC_QCFP.STYLECODE = A2.STYLECODE AND T_QC_QCFP.STYLESIZE = A2.STYLESIZE AND T_QC_QCFP.STYLECOLORSERIAL = A2.STYLECOLORSERIAL AND '000' = A2.REVNO " +
                    //"     AND A2.uploadcode='012'  AND A2.confirmed=1 " +
                    //" LEFT JOIN  PKERP.t_sd_file A3 ON  " +
                    //"     T_QC_QCFP.STYLECODE = A3.STYLECODE AND T_QC_QCFP.STYLESIZE = A3.STYLESIZE AND '000' = A3.STYLECOLORSERIAL AND T_QC_QCFP.REVNO = A3.REVNO " +
                    //"     AND A3.uploadcode='012'  AND A3.confirmed=1 " +
                    //" LEFT JOIN  PKERP.t_sd_file A4 ON  " +
                    //"     T_QC_QCFP.STYLECODE = A4.STYLECODE AND T_QC_QCFP.STYLESIZE = A4.STYLESIZE AND '000' = A4.STYLECOLORSERIAL AND '000' = A4.REVNO " +
                    //"     AND A4.uploadcode='012'  AND A4.confirmed=1    " +
                    //" LEFT JOIN  PKERP.t_Sd_sopm B1 ON  " +
                    //"     T_QC_QCFP.STYLECODE = B1.STYLECODE AND T_QC_QCFP.STYLESIZE = B1.STYLESIZE AND T_QC_QCFP.STYLECOLORSERIAL = B1.STYLECOLORSERIAL AND T_QC_QCFP.REVNO = B1.REVNO " +
                    //"     AND B1.confirmed='Y' " +
                    //" LEFT JOIN  PKERP.t_Sd_sopm B2 ON  " +
                    //"     T_QC_QCFP.STYLECODE = B2.STYLECODE AND T_QC_QCFP.STYLESIZE = B2.STYLESIZE AND T_QC_QCFP.STYLECOLORSERIAL = B2.STYLECOLORSERIAL AND '000' = B2.REVNO " +
                    //"     AND B2.confirmed='Y' " +
                    //" LEFT JOIN  PKERP.t_Sd_sopm B3 ON  " +
                    //"     T_QC_QCFP.STYLECODE = B3.STYLECODE AND T_QC_QCFP.STYLESIZE = B3.STYLESIZE AND '000' = B3.STYLECOLORSERIAL AND T_QC_QCFP.REVNO = B3.REVNO " +
                    //"     AND B3.confirmed='Y' " +
                    //" LEFT JOIN  PKERP.t_Sd_sopm B4 ON  " +
                    //"     T_QC_QCFP.STYLECODE = B4.STYLECODE AND T_QC_QCFP.STYLESIZE = B4.STYLESIZE AND '000' = B4.STYLECOLORSERIAL AND '000' = B4.REVNO " +
                    //"     AND B4.confirmed='Y' " +
                    //                             " WHERE " +
                    //                             " T_QC_QCFP.QCOFACTORY = '" + Factory + "'  " +
                    //                             " AND T_QC_QCFP.QCOYEAR = " + QCOYear + " " +
                    //                             " AND T_QC_QCFP.QCOWEEKNO = '" + strWeekNum + "'  " +
                    //                             " AND (T_SD_BOMT.ITEMCODE NOT LIKE 'PKG%' AND T_SD_BOMT.ITEMCODE NOT LIKE 'TRE%' )  ";
                    strSQL = " SELECT DISTINCT " +
                             " T_QC_QCFP.* , V_MRP_PP_WO.WONO , " +
                             " T_SD_BOMT.MAINITEMCODE , T_SD_BOMT.MAINITEMCOLORSERIAL , " +
                             " T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL , " +
                             " T_QC_QCFP.PLANQTY * T_SD_BOMT.UNITCONSUMPTION AS REQUESTQTY , " +
                             " T_00_ICMT.MAINLEVEL , " +
                             " CASE WHEN T_00_ICMT.MAINLEVEL IN ('FAB','LTR') THEN 'Y' ELSE NULL END as STAR_LEV3 , " +
                             " CASE WHEN T_00_ICMT.MAINLEVEL IN ('BST','LBL', 'MTL') THEN 'Y' ELSE NULL END as STAR_LEV2 , " +
                             " CASE WHEN T_00_ICMT.MAINLEVEL NOT IN ('FAB','LTR','BST','LBL', 'MTL') THEN 'Y' ELSE NULL END as STAR_LEV1 , " +
" CASE WHEN A1.STYLECODE IS NOT NULL THEN 'Y' " +
" ELSE  " +
"     CASE WHEN A2.STYLECODE IS NOT NULL THEN 'Y' " +
"     ELSE  " +
"         CASE WHEN A3.STYLECODE IS NOT NULL THEN 'Y' " +
"         ELSE  " +
"             CASE WHEN A4.STYLECODE IS NOT NULL THEN 'Y' " +
"             ELSE  " +
"                 CASE WHEN B1.STYLECODE IS NOT NULL THEN 'Y' " +
"                 ELSE  " +
"                     CASE WHEN B2.STYLECODE IS NOT NULL THEN 'Y' " +
"                     ELSE  " +
"                         CASE WHEN B3.STYLECODE IS NOT NULL THEN 'Y' " +
"                         ELSE  " +
"                             CASE WHEN B4.STYLECODE IS NOT NULL THEN 'Y' " +
"                             ELSE 'N' " +
"                             END " +
"                         END " +
"                     END " +
"                 END " +
"             END " +
"         END " +
"     END  " +
" END SOPREADINESS_DB " +
                             " FROM PKMES.T_QC_QCFPSIM T_QC_QCFP " +
                             " LEFT JOIN PKMES.V_MRP_PP_WO ON " +
                             "      T_QC_QCFP.STYLECODE = V_MRP_PP_WO.STLCD " +
                             "      AND T_QC_QCFP.STYLESIZE = V_MRP_PP_WO.STLSIZ " +
                             "      AND T_QC_QCFP.STYLECOLORSERIAL = V_MRP_PP_WO.STLCOSN " +
                             "      AND T_QC_QCFP.REVNO = V_MRP_PP_WO.STLREVN " +
                             "      AND T_QC_QCFP.PRDPKG = V_MRP_PP_WO.PRODPACKAGE " +
                             " INNER JOIN PKERP.T_SD_BOMT ON " +
                             "      T_QC_QCFP.STYLECODE = T_SD_BOMT.STYLECODE " +
                             "      AND T_QC_QCFP.STYLESIZE = T_SD_BOMT.STYLESIZE " +
                             "      AND T_QC_QCFP.STYLECOLORSERIAL = T_SD_BOMT.STYLECOLORSERIAL " +
                             "      AND T_QC_QCFP.REVNO = T_SD_BOMT.REVNO " +
                             " INNER JOIN PKERP.T_00_ICMT ON " +
                             "      T_SD_BOMT.ITEMCODE = T_00_ICMT.ITEMCODE " +
" LEFT JOIN  PKERP.t_sd_file A1 ON " +
"     T_QC_QCFP.STYLECODE = A1.STYLECODE AND T_QC_QCFP.STYLESIZE = A1.STYLESIZE AND T_QC_QCFP.STYLECOLORSERIAL = A1.STYLECOLORSERIAL AND T_QC_QCFP.REVNO = A1.REVNO " +
"     AND A1.uploadcode='012'  AND A1.confirmed=1 " +
" LEFT JOIN  PKERP.t_sd_file A2 ON  " +
"     T_QC_QCFP.STYLECODE = A2.STYLECODE AND T_QC_QCFP.STYLESIZE = A2.STYLESIZE AND T_QC_QCFP.STYLECOLORSERIAL = A2.STYLECOLORSERIAL AND '000' = A2.REVNO " +
"     AND A2.uploadcode='012'  AND A2.confirmed=1 " +
" LEFT JOIN  PKERP.t_sd_file A3 ON  " +
"     T_QC_QCFP.STYLECODE = A3.STYLECODE AND T_QC_QCFP.STYLESIZE = A3.STYLESIZE AND '000' = A3.STYLECOLORSERIAL AND T_QC_QCFP.REVNO = A3.REVNO " +
"     AND A3.uploadcode='012'  AND A3.confirmed=1 " +
" LEFT JOIN  PKERP.t_sd_file A4 ON  " +
"     T_QC_QCFP.STYLECODE = A4.STYLECODE AND T_QC_QCFP.STYLESIZE = A4.STYLESIZE AND '000' = A4.STYLECOLORSERIAL AND '000' = A4.REVNO " +
"     AND A4.uploadcode='012'  AND A4.confirmed=1    " +
" LEFT JOIN  PKERP.t_Sd_sopm B1 ON  " +
"     T_QC_QCFP.STYLECODE = B1.STYLECODE AND T_QC_QCFP.STYLESIZE = B1.STYLESIZE AND T_QC_QCFP.STYLECOLORSERIAL = B1.STYLECOLORSERIAL AND T_QC_QCFP.REVNO = B1.REVNO " +
"     AND B1.confirmed='Y' " +
" LEFT JOIN  PKERP.t_Sd_sopm B2 ON  " +
"     T_QC_QCFP.STYLECODE = B2.STYLECODE AND T_QC_QCFP.STYLESIZE = B2.STYLESIZE AND T_QC_QCFP.STYLECOLORSERIAL = B2.STYLECOLORSERIAL AND '000' = B2.REVNO " +
"     AND B2.confirmed='Y' " +
" LEFT JOIN  PKERP.t_Sd_sopm B3 ON  " +
"     T_QC_QCFP.STYLECODE = B3.STYLECODE AND T_QC_QCFP.STYLESIZE = B3.STYLESIZE AND '000' = B3.STYLECOLORSERIAL AND T_QC_QCFP.REVNO = B3.REVNO " +
"     AND B3.confirmed='Y' " +
" LEFT JOIN  PKERP.t_Sd_sopm B4 ON  " +
"     T_QC_QCFP.STYLECODE = B4.STYLECODE AND T_QC_QCFP.STYLESIZE = B4.STYLESIZE AND '000' = B4.STYLECOLORSERIAL AND '000' = B4.REVNO " +
"     AND B4.confirmed='Y' " +
                             " WHERE " +
                             " T_QC_QCFP.QCOFACTORY = '" + Factory + "'  " +
                             " AND T_QC_QCFP.QCOYEAR = " + QCOYear + " " +
                             " AND T_QC_QCFP.QCOWEEKNO = '" + strWeekNum + "'  " +
                             " AND (T_SD_BOMT.ITEMCODE NOT LIKE 'PKG%' AND T_SD_BOMT.ITEMCODE NOT LIKE 'TRE%' )  ";
                    strSQL += " ORDER BY T_QC_QCFP.ID , T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL ";
                    ///:: Main Query
                    strSQL = $@"
Select ROW_NUMBER() OVER(PARTITION BY FACTORY ORDER BY ID, DELIVERYDATE , PRDPKG , ITEMCODE , ITEMCOLORSERIAL ) AS RowSeqNo
 , Main.* 
 , LEAD(ID, 1, '') OVER (ORDER BY ID) AS NEXT_ID
From ({strSQL}) Main 
ORDER BY ID , ITEMCODE , ITEMCOLORSERIAL  ";
                    DataTable dt_QCFP = new DataTable();
                    PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, new List<OracleParameter> { new OracleParameter("FACTORY", Factory) }, out dt_QCFP, out strSQL, 3600);
                    //Add column ASSIGNEDQTY for Distribution Purpose.
                    DataColumn newColumn = new DataColumn("ASSIGNEDQTY", typeof(System.Double)) { DefaultValue = 0.0 };
                    dt_QCFP.Columns.Add(newColumn);
                    newColumn.Dispose();
                    //2019-12-11 Tai Le (Thomas) add 3 more columns for Material Rating
                    newColumn = new DataColumn("MATPRIORITYLEV3", typeof(System.Double)) { DefaultValue = 0.0 };
                    dt_QCFP.Columns.Add(newColumn);
                    newColumn.Dispose();
                    newColumn = new DataColumn("MATPRIORITYLEV2", typeof(System.Double)) { DefaultValue = 0.0 };
                    dt_QCFP.Columns.Add(newColumn);
                    newColumn.Dispose();
                    newColumn = new DataColumn("MATPRIORITYLEV1", typeof(System.Double)) { DefaultValue = 0.0 };
                    dt_QCFP.Columns.Add(newColumn);
                    newColumn.Dispose();
                    //::: END   2019-12-11
                    //::: Open T_QC_QCPM to Save the Material Distribution
                    strSQL = " SELECT * " +
                             " FROM PKMES.T_QC_QCPMSIM " +
                             " WHERE FACTORY = '" + Factory + "' " +
                             " AND QCOFACTORY = '" + Factory + "' " +
                             " AND QCOYEAR = " + QCOYear + " " +
                             " AND QCOWEEKNO = '" + strWeekNum + "' " +
                             "";
                    DataTable dt_T_QC_QCPM = new DataTable();
                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
                    oracleDataAdapter.Fill(dt_T_QC_QCPM);
                    //::: DISTRIBUTE MATERIAL
                    //this.DistributeMaterial(false, OracleConnectionString, Factory, QCOYear, strWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP);
                    var DistributeMaterialNewMSG = this.DistributeMaterialNew(false, Factory, QCOYear, strWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP);
                    if (mEnviroment == "Console")
                        Console.WriteLine("this.DistributeMaterial() : PASSED");
                    //:::END    DISTRIBUTE MATERIAL
                    //::: Save T_QC_QCPM
                    OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                    oracleDataAdapter.Update(dt_T_QC_QCPM);
                    oracleCommandBuilder.Dispose();
                    if (mEnviroment == "Console")
                        Console.WriteLine("Save dt 'T_QC_QCPM': PASSED");
                    //::: Update the Material Readiness back to dt_QCFP based on dt_T_QC_QCPM  { QUANTITY_A ; REQUESTQTY }
                    Update_T_QC_QCFP_MaterialReadiness(ref dt_QCFP, dt_T_QC_QCPM);
                    dt_T_QC_QCPM.Dispose();
                    oracleDataAdapter.Dispose();
                    //2019-12-11 Tai Le (Thomas): Remove Repeat row which has  ["MATNORNALRATE"] = -1 
                    RemoveDuplicateRowQCFP(ref dt_QCFP);
                    //2019-12-11 Tai Le: Add
                    //Update MATPRIORITYLEV3 ; MATPRIORITYLEV2 ; MATPRIORITYLEV1
                    UpdateMaterialRateT_QC_QCFP(ref dt_QCFP, Factory, QCOYear, strWeekNum);
                    //::: Sort dt_QCFP with LcAllFactoryParameters 
                    Sort_T_QC_QCFP(ref dt_QCFP, LcAllFactoryParameters, "All");
                    //::: SAVE dt_T_QC_QUEUE 
                    Save_T_QC_QUEUESIM(_PKMESConnString, Factory, WeekMonday, QCOYear, strWeekNum, dt_QCFP);
                    dt_QCFP.Dispose();
                    if (mEnviroment == "Console")
                        Console.WriteLine("Save_T_QC_QUEUESIM(): PASSED");
                    //Update QCORANKINGNEW  
                    OracleCommand command = new OracleCommand("Update PKMES.T_QC_QUEUESIM SET QCORANKINGNEW = ROWNUM WHERE QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO ", oracleConnection);
                    command.Parameters.Add(new OracleParameter("QCOFACTORY", Factory));
                    command.Parameters.Add(new OracleParameter("QCOYEAR", QCOYear));
                    command.Parameters.Add(new OracleParameter("QCOWEEKNO", strWeekNum));
                    command.ExecuteNonQuery();
                    command.Dispose();
                    if (mEnviroment == "Console")
                        Console.WriteLine("Update PKMES.T_QC_QUEUESIM : PASSED");
                    HE_QCOCalculate_Complete:
                    oracleConnection.Close();
                    //oracleConnection.Dispose();
                }
            }
            catch (Exception ex)
            {
                blHasError = true;
                strErrorMessage = ex.Message;
            }
            retMessage = strErrorMessage;
        HE_Exit_QCOCalculate:
            //::: Complete the Flag
            if (blUpdateQCOJobStatus)
            {
                Complete_T_QC_QCFRSIM(_PKMESConnString, Factory, QCOYear, strWeekNum, UserID, strErrorMessage, blHasError);
            }
            if (!blHasError)
            {
                strErrorMessage = "QCO Ranking For Factory[" + Factory + "]; Year[" + QCOYear + "]; WONo [" + strWeekNum + "] : Built Success.";
                retMessage = strErrorMessage;
            }
            //2019-11-14 Tai Le (Thomas): Run the CAPACITY Distribution
            this.CalculateCapaAll(_PKMESConnString);
            //return; // JsonConvert.SerializeObject(new { retResult = !blHasError, retData = "", retMsg = strErrorMessage });
        }
        public void Insert_T_QC_QCFRSIM(string mstrOracleCnnString, string vstrFactory, string vstrCurrentUserID)
        {
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(mstrOracleCnnString))
                {
                    oracleConnection.Open();
                    //int intI = 0;
                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter("SELECT * FROM PKMES.T_QC_QCFRSIM WHERE FACTORY = '" + vstrFactory + "'  ", oracleConnection);
                    DataTable dt_T_QC_QCFR = new DataTable();
                    oracleDataAdapter.Fill(dt_T_QC_QCFR);
                    if (dt_T_QC_QCFR.Rows.Count > 0)
                    {
                        /*Update Existing Factory */
                        dt_T_QC_QCFR.Rows[0]["STATUS"] = "RUNNING";
                        dt_T_QC_QCFR.Rows[0]["EXECUTINGBY"] = vstrCurrentUserID;
                        dt_T_QC_QCFR.Rows[0]["EXECUTINGDATE"] = DateTime.Now;
                    }
                    else
                    {
                        /*Insert For Non-existing Factory */
                        DataRow drNew_T_QC_QCFR = dt_T_QC_QCFR.NewRow();
                        drNew_T_QC_QCFR["FACTORY"] = vstrFactory;
                        drNew_T_QC_QCFR["STATUS"] = "RUNNING";
                        drNew_T_QC_QCFR["EXECUTINGBY"] = vstrCurrentUserID;
                        drNew_T_QC_QCFR["EXECUTINGDATE"] = DateTime.Now;
                        dt_T_QC_QCFR.Rows.Add(drNew_T_QC_QCFR);
                    }
                    OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                    oracleDataAdapter.Update(dt_T_QC_QCFR);
                    oracleCommandBuilder.Dispose();
                    dt_T_QC_QCFR.Dispose();
                    oracleDataAdapter.Dispose();
                    oracleConnection.Close();
                    oracleConnection.Dispose();
                }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
            }
        }
        public bool Save_T_QC_QCFPSIM(string mstrOracleCnnString, string vstrQCOFactory, DateTime vdtStartTime, int vintQCOYear, string vstrWeekNo, DataTable vdt_T_QC_QCFP)
        {
            try
            {
                if (vdt_T_QC_QCFP != null)
                {
                    using (OracleConnection oracleConnection = new OracleConnection(mstrOracleCnnString))
                    {
                        oracleConnection.Open();
                        intNegativeRank = 0;
                        int intPositiveRank = 0;
                        int intI = 0;
                        OracleDataAdapter oracleDataAdapter = new OracleDataAdapter("SELECT * FROM PKMES.T_QC_QCFPSIM WHERE QCOFACTORY = '" + vstrQCOFactory + "'  AND QCOYEAR = " + vintQCOYear + " AND QCOWEEKNO = '" + vstrWeekNo + "' ", oracleConnection);
                        DataTable dt_T_QC_QCFP = new DataTable();
                        oracleDataAdapter.Fill(dt_T_QC_QCFP);
                        foreach (DataRow dr in vdt_T_QC_QCFP.Rows)
                        {
                            intI += 1;
                            DataRow drNew_T_QC_QCFP = dt_T_QC_QCFP.NewRow();
                            drNew_T_QC_QCFP["QCOFACTORY"] = vstrQCOFactory;
                            drNew_T_QC_QCFP["QCOYEAR"] = vintQCOYear;
                            drNew_T_QC_QCFP["QCOWEEKNO"] = vstrWeekNo;
                            //2019-04-08
                            if (DateTime.Parse(dr["DELIVERYDATE"].ToString()).Date < vdtStartTime.Date)
                            {
                                intNegativeRank = intNegativeRank - 1;
                                drNew_T_QC_QCFP["RANGKING"] = intNegativeRank;
                            }
                            else
                            {
                                intPositiveRank = intPositiveRank + 1;
                                drNew_T_QC_QCFP["RANGKING"] = intPositiveRank;
                            }
                            drNew_T_QC_QCFP["ID"] = vstrQCOFactory + "_" + PCMGeneralFunctions.GetRight("0000000000000000" + intI, 15);
                            drNew_T_QC_QCFP["FACTORY"] = dr["FACTORY"];
                            drNew_T_QC_QCFP["LINENO"] = dr["LINENO"];
                            drNew_T_QC_QCFP["AONO"] = dr["AONO"];
                            drNew_T_QC_QCFP["STYLECODE"] = dr["STYLECODE"];
                            drNew_T_QC_QCFP["STYLESIZE"] = dr["STYLESIZE"];
                            drNew_T_QC_QCFP["STYLECOLORSERIAL"] = dr["STYLECOLORSERIAL"];
                            drNew_T_QC_QCFP["REVNO"] = dr["REVNO"];
                            drNew_T_QC_QCFP["PRDPKG"] = dr["PRDPKG"];
                            /*2019-11-01 Tai Le (Thomas): replace Package Qty with  Remain Package QTY instead */
                            drNew_T_QC_QCFP["PLANQTY"] = dr["PLANQTY"];
                            drNew_T_QC_QCFP["PLANQTYBAL"] = dr["PLANQTYBAL"]; //2019-11-04 Tai Le (Thomas): Add to show the Remain PP instead of original Production QTY
                            /*::END     2019-11-01 Tai Le (Thomas): replace Package Qty with  Remain Package QTY instead */
                            drNew_T_QC_QCFP["DELIVERYDATE"] = dr["DELIVERYDATE"];
                            drNew_T_QC_QCFP["PRDSDAT"] = dr["PRDSDAT"];
                            drNew_T_QC_QCFP["PRDEDAT"] = dr["PRDEDAT"];
                            /*2019-04-22 Tai Le(Thomas): Add 1 Original Data from AOMTOPS Package {ORDQTY} */
                            drNew_T_QC_QCFP["ORDQTY"] = dr["ORDQTY"];
                            /*2020-01-13 Tai Le(Thomas): Add Previous Custom Ranking*/
                            if (dr["PRECUSTOMRANK"] != null)
                                drNew_T_QC_QCFP["PRECUSTOMRANK"] = dr["PRECUSTOMRANK"];
                            /*2020-02-27 Tai Le(Thomas)*/
                            if (dr["MESALLOCATESTATUS"] != null)
                                drNew_T_QC_QCFP["MESALLOCATESTATUS"] = dr["MESALLOCATESTATUS"];
                            /*2020-02-29 Tai Le(Thomas)*/
                            //if (dr["MINPLNSTARTDATE"] != null)
                            //    drNew_T_QC_QCFP["MESSTARTDATE"] = dr["MINPLNSTARTDATE"];
                            if (dr["MESSTARTDATE"] != null)
                                drNew_T_QC_QCFP["MESSTARTDATE"] = dr["MESSTARTDATE"];
                            dt_T_QC_QCFP.Rows.Add(drNew_T_QC_QCFP);
                        }
                        OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                        oracleDataAdapter.Update(dt_T_QC_QCFP);
                        oracleCommandBuilder.Dispose();
                        dt_T_QC_QCFP.Dispose();
                        oracleDataAdapter.Dispose();
                        oracleConnection.Close();
                        oracleConnection.Dispose();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                return false;
            }
        }
        public bool Save_T_QC_QUEUESIM(string mstrOracleCnnString, string vstrQCOFactory, DateTime vdtStarDateTime, int vintQCOYear, string vstrWeekNo, DataTable vdt_T_QC_QCFP)
        {
            DataTable dt_T_QC_QUEUE = new DataTable();
            int intI = 0,
                intQCORANKINGNEW = 0;
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(mstrOracleCnnString))
                {
                    oracleConnection.Open();
                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter("SELECT * FROM PKMES.T_QC_QUEUESIM WHERE QCOFACTORY = '" + vstrQCOFactory + "' AND QCOYEAR = " + vintQCOYear + " AND QCOWEEKNo = '" + vstrWeekNo + "'  ", oracleConnection);
                    oracleDataAdapter.Fill(dt_T_QC_QUEUE);
                    //int intRowCount = -1 * vdt_T_QC_QCFP.Rows.Count;
                    foreach (DataRow dr in vdt_T_QC_QCFP.Rows)
                    {
                        if (dr["MATNORNALRATE"] != null)
                            if (dr["MATNORNALRATE"].ToString().Length > 0)
                                if (Convert.ToDouble(dr["MATNORNALRATE"].ToString()) >= 0)
                                {
                                    //intI += 1;
                                    DataRow drNew_T_QC_QCFP = dt_T_QC_QUEUE.NewRow();
                                    drNew_T_QC_QCFP["QCOFACTORY"] = dr["QCOFACTORY"];
                                    drNew_T_QC_QCFP["QCOYEAR"] = dr["QCOYEAR"];
                                    drNew_T_QC_QCFP["QCOWEEKNO"] = dr["QCOWEEKNO"];
                                    //2019-03-08 Thomas: Modify the RANKING
                                    //drNew_T_QC_QCFP["QCORANK"] = intI; //dr["QCOWEEKNO"];
                                    if (dr["DELIVERYDATE"] != null)
                                    {
                                        DateTime dtTemp = DateTime.Parse(dr["DELIVERYDATE"].ToString());
                                        if (dtTemp < vdtStarDateTime)
                                        {
                                            drNew_T_QC_QCFP["QCORANK"] = -1 * (Convert.ToInt32(dr["RANGKING"].ToString()) + 1 + Math.Abs(intNegativeRank));
                                            drNew_T_QC_QCFP["CHANGEQCORANK"] = -1 * (Convert.ToInt32(dr["RANGKING"].ToString()) + 1 + Math.Abs(intNegativeRank));
                                        }
                                        else
                                        {
                                            intI += 1;
                                            drNew_T_QC_QCFP["QCORANK"] = intI;
                                            drNew_T_QC_QCFP["CHANGEQCORANK"] = intI;
                                        }
                                        //2019-04-27 Tai Le (Thomas)
                                        intQCORANKINGNEW += 1;
                                        drNew_T_QC_QCFP["QCORANKINGNEW"] = intQCORANKINGNEW;
                                    }
                                    drNew_T_QC_QCFP["FACTORY"] = dr["FACTORY"];
                                    drNew_T_QC_QCFP["LINENO"] = dr["LINENO"];
                                    drNew_T_QC_QCFP["AONO"] = dr["AONO"];
                                    drNew_T_QC_QCFP["STYLECODE"] = dr["STYLECODE"];
                                    drNew_T_QC_QCFP["STYLESIZE"] = dr["STYLESIZE"];
                                    drNew_T_QC_QCFP["STYLECOLORSERIAL"] = dr["STYLECOLORSERIAL"];
                                    drNew_T_QC_QCFP["REVNO"] = dr["REVNO"];
                                    drNew_T_QC_QCFP["PRDPKG"] = dr["PRDPKG"];
                                    /*2019-04-22 Tai Le(Thomas): Add 5 Original Data from AOMTOPS Package {PLANQTY, DELIVERYDATE, PRDSDAT, PRDEDAT, ORDQTY} */
                                    drNew_T_QC_QCFP["PLANQTY"] = dr["PLANQTY"];
                                    //2019-11-15 Tai Le (Thomas): Add the Remain Package Qty for Capacity Calculation
                                    drNew_T_QC_QCFP["PLANQTYBAL"] = dr["PLANQTYBAL"];
                                    drNew_T_QC_QCFP["DELIVERYDATE"] = dr["DELIVERYDATE"];
                                    drNew_T_QC_QCFP["PRDSDAT"] = dr["PRDSDAT"];
                                    drNew_T_QC_QCFP["PRDEDAT"] = dr["PRDEDAT"];
                                    drNew_T_QC_QCFP["ORDQTY"] = dr["ORDQTY"];
                                    drNew_T_QC_QCFP["NORMALIZEDPERCENT"] = dr["MATNORNALRATE"];
                                    drNew_T_QC_QCFP["CREATEDATE"] = DateTime.Now;
                                    drNew_T_QC_QCFP["LATESTQCOTIME"] = DateTime.Now; //2019-06-18 Tai Le (Thomas)
                                    //2019-11-15 Tai Le (Thomas): Handle the QCO Version 
                                    if (!String.IsNullOrEmpty(strQCOVersion))
                                        drNew_T_QC_QCFP["QCOVERSION"] = strQCOVersion;
                                    ///2019-12-12 Tai Le (Thomas): Add Material Priority Rate
                                    /// Priority Rule :   Level 3 > Level 2 > Level 1
                                    /// MATPRIORITYA  represent for Level_3 
                                    ///     MATPRIORITYB  represent for Level_2
                                    ///         MATPRIORITYC  represent for Level_1
                                    drNew_T_QC_QCFP["MATPRIORITYA"] = dr["MATPRIORITYLEV3"];
                                    drNew_T_QC_QCFP["MATPRIORITYB"] = dr["MATPRIORITYLEV2"];
                                    drNew_T_QC_QCFP["MATPRIORITYC"] = dr["MATPRIORITYLEV1"];
                                    ///:::END   2019-12-12 Tai Le (Thomas): Add Material Priority Rate
                                    //2020-01-11 Tai Le(Thomas): Previous Week QCO Ranking
                                    if (dr["PRECUSTOMRANK"] != null)
                                        drNew_T_QC_QCFP["PRECUSTOMRANK"] = dr["PRECUSTOMRANK"];
                                    //:::END    2020-01-11 Tai Le(Thomas)
                                    //2020-02-27 Tai Le(Thomas):  
                                    if (dr["MESALLOCATESTATUS"] != null)
                                        drNew_T_QC_QCFP["MESALLOCATESTATUS"] = dr["MESALLOCATESTATUS"];
                                    //:::END    2020-02-27 Tai Le(Thomas)
                                    //2020-02-29 Tai Le(Thomas):  
                                    if (dr["MESSTARTDATE"] != null)
                                        if (dr["MESALLOCATESTATUS"].ToString() != "Not Scheduled")
                                            drNew_T_QC_QCFP["MESSTARTDATE"] = dr["MESSTARTDATE"];
                                    //:::END    2020-02-29 Tai Le(Thomas)
                                    dt_T_QC_QUEUE.Rows.Add(drNew_T_QC_QCFP);
                                }
                    }
                    OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                    oracleDataAdapter.Update(dt_T_QC_QUEUE);
                    oracleCommandBuilder.Dispose();
                    dt_T_QC_QUEUE.Dispose();
                    oracleDataAdapter.Dispose();
                    oracleConnection.Close();
                    oracleConnection.Dispose();
                }
                return true;
            }
            catch (Exception ex)
            {
                var Msg = ex.Message + "; intI= " + intI;
                //Export vdt_T_QC_QCFP to CSV file 
                StringBuilder sb = new StringBuilder();
                foreach (DataColumn dc in dt_T_QC_QUEUE.Columns)
                {
                    sb.Append(dc.ColumnName + ";");
                }
                sb.AppendLine("");
                foreach (DataRow dr in dt_T_QC_QUEUE.Rows)
                {
                    foreach (DataColumn dc in dt_T_QC_QUEUE.Columns)
                    {
                        sb.Append(dr[dc.ColumnName] + ";");
                    }
                    sb.AppendLine("");
                }
                var fileName = "vdt_T_QC_QCFPSIM" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                System.IO.File.WriteAllText(System.IO.Path.Combine(@"D:\\_Temp", fileName), sb.ToString());
                sb.Clear();
                //Export vdt_T_QC_QCFP to CSV file  
                foreach (DataColumn dc in vdt_T_QC_QCFP.Columns)
                {
                    sb.Append(dc.ColumnName + ";");
                }
                sb.AppendLine("");
                foreach (DataRow dr in vdt_T_QC_QCFP.Rows)
                {
                    foreach (DataColumn dc in vdt_T_QC_QCFP.Columns)
                    {
                        sb.Append(dr[dc.ColumnName] + ";");
                    }
                    sb.AppendLine("");
                }
                fileName = "vdt_T_QC_QCFP" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                System.IO.File.WriteAllText(System.IO.Path.Combine(@"D:\\_Temp", fileName), sb.ToString());
                sb.Clear();
                return false;
            }
        }
        public async Task<bool> Save_T_QC_QCFC(string mstrOracleCnnString, string vstrQCOFactory, int vintQCOYear, string vstrWeekNo) {
            try {
                string query = "";
                query = $@"
INSERT INTO T_QC_QCPC (ID,QCOFACTORY,QCOYEAR,QCOWEEKNO,PRDPKG,PLANQTY,MAINITEMCODE,MAINITEMCOLORSERIAL,ITEMCODE,ITEMCOLORSERIAL,PIECEUNIQUE,PIECEQTY,REQUIREDQTY,COMPLETEQTY,MODULEITEMCODE,COMPLETEQTYBYSET) 
SELECT rownum as ID , T_QC_QCFP.QCOFACTORY , T_QC_QCFP.QCOYEAR , T_QC_QCFP.QCOWEEKNO ,T_QC_QCFP.PRDPKG , T_QC_QCFP.PLANQTY,  
T_SD_BOMT.MAINITEMCODE , T_SD_BOMT.MAINITEMCOLORSERIAL ,
T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL ,
T_SD_PTMT.PIECEUNIQUE , T_SD_PTMT.PIECEQTY  ,
T_QC_QCFP.PLANQTY  * T_SD_PTMT.PIECEQTY  AS REQUIREDQTY , 
NVL(T_CT_CDPT.ALLOCATEDQTY,0) AS COMPLETEQTY,
NVL(T_00_ICMT.ITEMCODE,'') AS MODULEITEMCODE ,
CASE WHEN NVL(T_CT_CDPT.ALLOCATEDQTY,0) > (T_QC_QCFP.PLANQTY  * T_SD_PTMT.PIECEQTY) THEN (T_QC_QCFP.PLANQTY * T_SD_PTMT.PIECEQTY)
ELSE NVL(T_CT_CDPT.ALLOCATEDQTY,0) END COMPLETEQTYBYSET
FROM PKMES.T_QC_QCFP
LEFT JOIN PKERP.T_SD_BOMT ON 
    T_QC_QCFP.STYLECODE = T_SD_BOMT.STYLECODE
    AND T_QC_QCFP.STYLESIZE= T_SD_BOMT.STYLESIZE
    AND T_QC_QCFP.STYLECOLORSERIAL = T_SD_BOMT.STYLECOLORSERIAL
    AND T_QC_QCFP.REVNO = T_SD_BOMT.REVNO
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
    T_QC_QCFP.PRDPKG = T_CT_CDPT.PRDPKG    
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
AND  T_QC_QCFP.QCOFACTORY = '{vstrQCOFactory}'
AND  T_QC_QCFP.QCOYEAR = {vintQCOYear}
AND  T_QC_QCFP.QCOWEEKNO = '{vstrWeekNo}'
AND T_SD_BOMT.ITEMCODE NOT LIKE 'BNF%'
";

                using (OracleConnection conn = new OracleConnection(mstrOracleCnnString)) {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand()) {
                        cmd.Connection = conn;
                        cmd.CommandText = query;
                        cmd.CommandTimeout = int.MaxValue;
                        await cmd.ExecuteNonQueryAsync();
                    }
                    conn.Close();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        public void Complete_T_QC_QCFRSIM(string mstrOracleCnnString, string vstrQCOFactory, int vintQCOYear, string vstrWeekNo, string vstrCurrentUserID, string vstrResult, bool blHasError)
        {
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(mstrOracleCnnString))
                {
                    oracleConnection.Open();
                    //int intI = 0;
                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter("SELECT * FROM PKMES.T_QC_QCFRSIM WHERE FACTORY = '" + vstrQCOFactory + "' AND STATUS = 'RUNNING' ", oracleConnection);
                    DataTable dt_T_QC_QCFR = new DataTable();
                    oracleDataAdapter.Fill(dt_T_QC_QCFR);
                    if (dt_T_QC_QCFR.Rows.Count > 0)
                    {
                        /*Update Existing Factory */
                        dt_T_QC_QCFR.Rows[0]["STATUS"] = "COMPLETE"; //Important Flag
                        dt_T_QC_QCFR.Rows[0]["LASTDONEBY"] = vstrCurrentUserID;
                        dt_T_QC_QCFR.Rows[0]["LASTCOMPLETEDATE"] = DateTime.Now;
                        if (blHasError)
                            dt_T_QC_QCFR.Rows[0]["FAILMESSAGE"] = vstrResult;
                        else
                            dt_T_QC_QCFR.Rows[0]["FAILMESSAGE"] = "QCO Calculation Success In Year[" + vintQCOYear + "], Week[" + vstrWeekNo + "]";
                    }
                    OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                    oracleDataAdapter.Update(dt_T_QC_QCFR);
                    oracleCommandBuilder.Dispose();
                    dt_T_QC_QCFR.Dispose();
                    oracleDataAdapter.Dispose();
                    oracleConnection.Close();
                    oracleConnection.Dispose();
                }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
            }
        }
        public bool CentralizeSIMQCO(string mstrOracleCnnString)
        {
            try
            {
                bool isError = false;
                if (String.IsNullOrEmpty(mFactory))
                    isError = true;
                if (mYear <= 0)
                    isError = true;
                if (String.IsNullOrEmpty(mWeekNo))
                    isError = true;
                if (isError)
                    return isError;
                /*Delete central QCO*/
                DeleteQCOData(mFactory, mYear, mWeekNo, false).Wait();
                /* Prepare data From SIMQCO to transfer QCO */
                //Open 4 Central QCO tables 
                DataSet ds = new DataSet();
                OracleDataAdapter oracleDataAdapterQCFP = new OracleDataAdapter("Select * From PKMES.T_QC_QCFP Where 1=2", mstrOracleCnnString);
                oracleDataAdapterQCFP.Fill(ds, "QCFP");
                OracleDataAdapter oracleDataAdapterQCPM = new OracleDataAdapter("Select * From PKMES.T_QC_QCPM Where 1=2", mstrOracleCnnString);
                oracleDataAdapterQCPM.Fill(ds, "QCPM");
                OracleDataAdapter oracleDataAdapterQCPS = new OracleDataAdapter("Select * From PKMES.T_QC_QCPS Where 1=2", mstrOracleCnnString);
                oracleDataAdapterQCPS.Fill(ds, "QCPS");
                OracleDataAdapter oracleDataAdapterQUEUE = new OracleDataAdapter("Select * From PKMES.T_QC_QUEUE Where 1=2", mstrOracleCnnString);
                oracleDataAdapterQUEUE.Fill(ds, "QUEUE");
                string[] arrTableNames = { "T_QC_QCFPSIM", "T_QC_QCPMSIM", "T_QC_QCPSSIM", "T_QC_QUEUESIM" };
                //  Table T_QC_QCFPSIM  >  T_QC_QCFP 
                //  Table T_QC_QCPMSIM  >  T_QC_QCPM 
                //  Table T_QC_QCPSSIM  >  T_QC_QCPS 
                //  Table T_QC_QUEUESIM  >  T_QC_QUEUE
                string strMsg = "";
                for (int I = 0; I < arrTableNames.Length; I++)
                {
                    var currTable = arrTableNames[I];
                    var strSQL = "Select * From " + currTable + " Where QCOFACTORY = '" + mFactory + "' AND QCOYEAR = " + mYear + " AND QCOWEEKNO = '" + mWeekNo + "' ";
                    DataTable SourceDT = new DataTable();
                    PCMOracleLibrary.StatementToDataTable(mstrOracleCnnString, strSQL, null, out SourceDT, out strMsg, 120);
                    if (SourceDT != null)
                    {
                        if (SourceDT.Rows.Count > 0)
                            foreach (DataRow dr in SourceDT.Rows)
                            {
                                DataRow drNew = null;
                                switch (I)
                                {
                                    case 0:
                                        drNew = ds.Tables["QCFP"].NewRow();
                                        break;
                                    case 1:
                                        drNew = ds.Tables["QCPM"].NewRow();
                                        break;
                                    case 2:
                                        drNew = ds.Tables["QCPS"].NewRow();
                                        break;
                                    case 3:
                                        drNew = ds.Tables["QUEUE"].NewRow();
                                        break;
                                }
                                if (drNew != null)
                                {
                                    foreach (DataColumn dc in SourceDT.Columns)
                                    {
                                        drNew[dc.ColumnName] = dr[dc.ColumnName];
                                    }
                                    ds.Tables[I].Rows.Add(drNew);
                                }
                            }
                    }
                }
                OracleCommandBuilder oracleCommandBuilderQCFP = new OracleCommandBuilder(oracleDataAdapterQCFP);
                oracleDataAdapterQCFP.Update(ds.Tables["QCFP"]);
                oracleCommandBuilderQCFP.Dispose();
                OracleCommandBuilder oracleCommandBuilderQCPM = new OracleCommandBuilder(oracleDataAdapterQCPM);
                oracleDataAdapterQCPM.Update(ds.Tables["QCPM"]);
                oracleCommandBuilderQCPM.Dispose();
                OracleCommandBuilder oracleCommandBuilderQCPS = new OracleCommandBuilder(oracleDataAdapterQCPS);
                oracleDataAdapterQCPS.Update(ds.Tables["QCPS"]);
                oracleCommandBuilderQCPS.Dispose();
                OracleCommandBuilder oracleCommandBuilderQUEUE = new OracleCommandBuilder(oracleDataAdapterQUEUE);
                oracleDataAdapterQUEUE.Update(ds.Tables["QUEUE"]);
                oracleCommandBuilderQUEUE.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                return false;
            }
        }
        public DataTable GetSimQCOFactoryList(string mstrOracleCnnString)
        {
            DataTable dt = new DataTable();
            var strSQL = "Select * From PKMES.T_QC_SQFL Where ISACTIVE = 'Y' ";
            PCMOracleLibrary.StatementToDataTable(mstrOracleCnnString, strSQL, null, out dt, out strSQL);
            return dt;
        }
        public void QCOCalculationSimAll()
        {
            if (String.IsNullOrEmpty(_PKERPConnString) || String.IsNullOrEmpty(_PKMESConnString))
                return;
            var dt = GetSimQCOFactoryList(_PKMESConnString);
            var Msg = "";
            if (dt != null)
                foreach (DataRow dr in dt.Rows)
                {
                    var _factory = dr["FACTORY"].ToString();
                    var _applyWeekDay = dr["APPLYWEEKDAY"].ToString();
                    if (!String.IsNullOrEmpty(_factory) && !String.IsNullOrEmpty(_applyWeekDay))
                    {
                        var arrApplyWeekDay = _applyWeekDay.Split(';');
                        for (int I = 0; I < arrApplyWeekDay.Length; I++)
                        {
                            Msg = "";
                            var DayOfWeek = (int)DateTime.Today.DayOfWeek;
                            if (arrApplyWeekDay[I] == DayOfWeek.ToString())
                            {
                                var dtStart = DateTime.Now;
                                if (mEnviroment == "Console")
                                    Console.WriteLine("QCOCalculationSIM on Factory[" + _factory + "], Started at: " + dtStart.ToString("u"));
                                mQCOSource = "QCOSim";
                                QCOCalculationSIM(_factory, mUserID, mRoleID, out Msg);
                                if (Msg.Length > 0)
                                {
                                    var botClient = new TelegramBotClient(TeleTokenID);
                                    botClient.SendTextMessageAsync(-1001407116473, Msg).Wait();
                                }
                                if (mEnviroment == "Console")
                                {
                                    var dtEnd = DateTime.Now;
                                    Console.WriteLine("QCOCalculationSIM on Factory[" + _factory + "], Finished at: " + dtEnd.ToString("u") + ". Total Minute: " + (dtEnd - dtStart).TotalMinutes.ToString());
                                    Console.WriteLine("==============================================================================");
                                    Console.WriteLine("");
                                }
                                break;
                            }
                        }
                    }
                }
        }
        #endregion
        public string GenerateQCOVersion()
        {
            Guid guid = Guid.NewGuid();
            return guid.ToString("N");
        }
        public static string GetQCOVersion(string pConnString, string pQCOFactory, int pQCOYear, string pQCOWeek, string pTableName)
        {
            var CurrQCOVersion = "";
            var strSQL =
                $" Select QCOVERSION " +
                $" From {pTableName} " +
                $" Where QCOFactory = '{pQCOFactory}' " +
                $" And QCOYear = {pQCOYear} " +
                $" And QCOWeekNo = '{pQCOWeek}'  " +
                $" And rownum <=2 ";
            using (OracleConnection oracleConn = new OracleConnection(pConnString))
            {
                oracleConn.Open();
                DataTable dt = new DataTable();
                PCMOracleLibrary.StatementToDataTable(oracleConn, strSQL, null, out dt, out strSQL);
                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                        CurrQCOVersion = dt.Rows[0][0].ToString();
                    dt.Dispose();
                }
                oracleConn.Close();
            }
            return CurrQCOVersion;
        }

        #region Retired Functions
        //public void SyncMTOPSWorkingTime(string pFactory, string pYearMonth) {
        //	///this function use PKERP connection to get data from MTOPS
        //	///      /*Author: Son Nguyen Cao*/
        //	string Message = "";
        //	try
        //	{
        //		if (string.IsNullOrEmpty(pFactory) || string.IsNullOrEmpty(pYearMonth))
        //			return;
        //		var yyyy = pYearMonth.Substring(0, 4);
        //		var mm = pYearMonth.Substring(5, 2);
        //		var arrFac = pFactory.Split(',');
        //		foreach (var fac in arrFac)
        //		{
        //			//Synchronize factory worker                
        //			//var fawkMes = FawkBus.GetFactoryWorkerFromMES(fac, yyyy, mm);
        //			string strSql = @"SELECT * FROM PKMES.T_MX_FAWK
        //                              WHERE FACTORY = :P_FACTORY AND PLANYEAR = :P_PLANYEAR AND PLANMONTH = :P_PLANMONTH ";
        //			var oraParams = new List<OracleParameter>(){
        //				new OracleParameter("P_FACTORY", pFactory),
        //				new OracleParameter("PLANYEAR", yyyy),
        //				new OracleParameter("P_PLANMONTH", mm),
        //			};
        //			var fawkMes = PCMOracleLibrary.QueryToObject<Fawk>(_PKERPConnString, strSql, oraParams.ToArray(), out Message).FirstOrDefault();
        //			oraParams.Clear();
        //			if (fawkMes == null)
        //			{
        //				//Get factor woker from Mtop then inserting to MES
        //				//var fawkMtop = FawkBus.GetFactoryWorkerFromMtop(fac, yyyyMM);
        //				//Get working time of 31 days
        //				strSql = @"SELECT FATOY AS FACTORY, SUBSTR(MOTHNO, 1,4) AS PLANYEAR, SUBSTR(MOTHNO, 5, 2) AS PLANMONTH, WORKER, SEWER 
        //                            FROM MT_FATWRKR_TBL@MTOPSDB WHERE FATOY = :P_FATOY AND MOTHNO = :P_MOTHNO ";
        //				oraParams = new List<OracleParameter>(){
        //					new OracleParameter("P_FATOY", pFactory),
        //					new OracleParameter("P_MOTHNO", pYearMonth.Replace("/",""))
        //				};
        //				var fawkMtop = PCMOracleLibrary.QueryToObject<Fawk>(_PKERPConnString,strSql, oraParams.ToArray(), out Message).FirstOrDefault();
        //				//Synchronize worker factory from Mtop
        //				//FawkBus.InsertFactoryWorkerToMES(fawkMtop);
        //				strSql = @" INSERT INTO PKMES.T_MX_FAWK (FACTORY, PLANYEAR, PLANMONTH, WORKER, SEWER, DIRECTWORKER, INDIRECTWORKER)
        //                              VALUES (:P_FACTORY, :P_PLANYEAR, :P_PLANMONTH, :P_WORKER, :P_SEWER, :P_DIRECTWORKER, :P_INDIRECTWORKER) ";
        //				var param = new List<OracleParameter>(){
        //					new OracleParameter("P_FACTORY", fawkMtop.FACTORY),
        //					new OracleParameter("P_PLANYEAR", fawkMtop.PLANYEAR),
        //					new OracleParameter("P_PLANMONTH", fawkMtop.PLANMONTH),
        //					new OracleParameter("P_WORKER", fawkMtop.WORKER),
        //					new OracleParameter("P_SEWER", fawkMtop.SEWER),
        //					new OracleParameter("P_DIRECTWORKER", fawkMtop.DIRECTWORKER),
        //					new OracleParameter("P_INDIRECTWORKER", fawkMtop.INDIRECTWORKER)
        //				};
        //				var resIns = PCMOracleLibrary.StatementExecution(_PKMESConnString, strSql, param, out Message);
        //				oraParams.Clear();
        //			}
        //			//Get list working time sheet from Mtop
        //			//var listFlwsMtop = FwtsBus.GetListWorkingTimeFromMtop(fac, yyyyMM);
        //			//var listFlwsMtop = GetLineWorkingTimeFromMtop(factory, yyyyMM);
        //			strSql = @"SELECT * FROM (
        //                                  SELECT FATOY AS FACTORY, LINENO, SUBSTR(MOTHNO, 1, 4) AS PLANYEAR, SUBSTR(MOTHNO, 5, 2) AS PLANMONTH, WEEKNO,  PLNDAY AS PLANDAY, MORTME AS MORNINGTIME
        //                                         , ARNTME AS AFTERNOONTIME, REDTME - (MORTME + ARNTME) AS OVERTIME, CRTID AS CREATEID, CRTDAT AS CREATEDATE, UPTDAT AS UPDATEDATE,UPTID AS UPDATEID
        //                                  FROM MT_CALMST_TBL@MTOPSDB 
        //                                  WHERE FATOY LIKE UPPER('%'||:P_FATOY||'%')  AND MOTHNO = :P_YYYYMM 
        //                                  ORDER BY LINENO, MOTHNO, PLNDAY
        //                              ) WHERE ROWNUM <= 31";
        //			oraParams = new List<OracleParameter>()
        //			{
        //				new OracleParameter("P_FATOY", pFactory),
        //				new OracleParameter("P_YYYYMM", pYearMonth.Replace("/",""))
        //			};
        //			var listFlwsMtop = PCMOracleLibrary.QueryToObject<Fwts>(_PKERPConnString, strSql, oraParams.ToArray(), out Message);
        //			if (listFlwsMtop.Count > 0)
        //			{
        //				//Get line no
        //				var lineNo = listFlwsMtop[0].LINENO;
        //				//Get working sheet data of line no
        //				var newFlwsMtop = listFlwsMtop.Where(x => x.LINENO == lineNo);
        //				return newFlwsMtop.ToList();
        //			} 
        //			//Get list working time from MES
        //			var listFwtsMes = FwtsBus.GetLineWorkingTimeFactoryOraMES(fac, yyyyMM.Substring(0, 4), yyyyMM.Substring(5, 2));
        //			//If factory working time sheet was synchronized already then update
        //			if (listFwtsMes.Count() > 0)
        //			{
        //				//Update list factory working sheet
        //				FwtsBus.UpdateListWorkingTimeOracle(listFlwsMtop);
        //			}
        //			else
        //			{
        //				if (listFlwsMtop.Count > 0)
        //				{
        //					//Insert list of working time to MES
        //					FwtsBus.InsertListtWorkingTimeOracle(listFlwsMtop);
        //				}
        //			}
        //		}
        //		return Json(new { retResult = true, retMsg = "Sync Working Sheet" }, JsonRequestBehavior.AllowGet);
        //	}
        //	catch (Exception ex)
        //	{
        //		return Json(new { retResult = false, retMsg = ex.Message }, JsonRequestBehavior.AllowGet);
        //	}
        //}
        //public void QCOCalculationAll(string OracleConnectionString, string Factory, string UserID, string UserRole, string ServerPath, out string retMessage, bool pCopyToRunningQCO = false)
        //{
        //	retMessage = "";
        //	//2019-12-19 Tai Le (Thomas)
        //	//Log.Logger = new LoggerConfiguration()
        //	//    .MinimumLevel.Debug()
        //	//    .WriteTo.File(System.IO.Path.Combine(ServerPath, $"Logs\\{DateTime.Today.ToString("yyyy-MM-dd")}.txt"), rollingInterval: RollingInterval.Day)
        //	//    .CreateLogger();
        //	using (OracleConnection oracleConnection = new OracleConnection(OracleConnectionString))
        //	{
        //		oracleConnection.Open();
        //		List<PCMQCOCalculation> Factories = new List<PCMQCOCalculation>();
        //		var strSQL = "Select FACTORY " +
        //					 " From PKERP.T_CM_FCMT  " +
        //					 " Where Type = 'P' " +
        //					 " And Substr(Factory,1,1) = 'P' " +
        //					 " And Status ='OK' " +
        //					 " AND FACTORY like '" + Factory + "' " +
        //					 " Order By Factory ";
        //		DataTable dt = new DataTable();
        //		PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, null, out dt, out strSQL);
        //		if (dt != null)
        //		{
        //			if (dt.Rows.Count > 0)
        //				foreach (DataRow dr in dt.Rows)
        //				{
        //					Factories.Add(new PCMQCOCalculation(dr["FACTORY"].ToString()));
        //					retMessage = "";
        //				}
        //			dt.Dispose();
        //		}
        //		oracleConnection.Close();
        //		oracleConnection.Dispose();
        //		if (Factories.Count > 0)
        //		{
        //			StringBuilder sb = new StringBuilder();
        //			sb.Clear();
        //			string _tempMsg = "";
        //			for (int I = 0; I < Factories.Count; I++)
        //			{
        //				sb.AppendLine("QCO in " + Factories[I].mFactory);
        //				if (mEnviroment.ToLower() == "console")
        //				{
        //					Console.WriteLine("QCO in " + Factories[I].mFactory);
        //					Console.WriteLine("================================");
        //					Console.WriteLine("");
        //				}
        //				mFactory = Factories[I].mFactory;
        //				mQCOSource = "QCO";
        //				this.QCOCalculation(OracleConnectionString, Factories[I].mFactory, String.IsNullOrEmpty(mUserID) ? UserID : mUserID, UserRole, ServerPath, false, "", "", "", "", "", "", out _tempMsg, pCopyToRunningQCO);
        //				sb.AppendLine(_tempMsg);
        //				//Log.Information(_tempMsg);
        //				_tempMsg = "";
        //			}
        //			retMessage = sb.ToString();
        //		}
        //	}
        //	//2019-12-19 Tai Le (Thomas)
        //	//Log.CloseAndFlush();
        //}
        //public string QCOCalculation(string OracleConnectionString, string Factory, string UserID, string UserRole, string ServerPath, bool IsSinglePP, string pAONO, string pStyleCode, string pStyleSize, string pStyleColorSerial, string pRevNo, string pPRDPKD_ID, out string retMessage, bool pCopyToRunningQCO = false)
        //{
        //	retMessage = "";
        //	/* 
        //	* Purpose: Calculate the Material Readiness For MTOPS Packages In Chosen FACTORY
        //	* Input:   Factory
        //	* Output:  The MTOPS Package is ranked based on the Material Readiness
        //	* 
        //	* :: Pre-processing
        //	*      1. Check the Factory QCO Running Status on Table  PKMES.T_QC_QCFR
        //	*          1.1 If Status = "RUNNING"
        //	*              >> Quit and Return the message "Factory is running by <EXECUTINGBY> at <EXECUTINGDATE>
        //	*          1.2 If Status = "DONE"
        //	*              >> Continue the Next Process
        //	*      2. Check the Availability of Factory Sorting Parameters on table PKMES.T_00_QCFO
        //	*          2.1 If No Factory Setting
        //	*              >> Quit and Return the message "Please set up the QCO Sorting Parameters on Factory <Factory>"
        //	*      3. Check, Whether QCO Factory + QCO Week   EXISTING 
        //	* :: Processing
        //	*      1. From View  "PKERP.VIEW_ERP_PSRSNP_PLAN", look for all the INCOMPLETE Packages
        //	*      2. Save the satisfied Package into PKMES.T_QC_QCFP
        //	*      3. Sort the PKMES.T_QC_QCFP based on The Factory Sorting Parameters on table PKMES.T_00_QCFO
        //	*      4. Based on PKMES.T_QC_QCFP , PKERP.T_SD_BOMT ,  PKERP.V_WMS_PORC , Distribute the Received Qty for Each Package
        //	*          4.1. PKMES.T_QC_QCFP  Join  PKERP.T_SD_BOMT together, and order by "Factory Sorting Parameters" , T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL
        //	* :: Post-processing
        //	*/
        //	/* Prevent RUN QCO for Factory in case input "pAONO, pStyleCode, pStyleSize, pStyleColorSerial, pRevNo, pPRDPKD_ID" but IsSinglePP = FALSE */
        //	if (!String.IsNullOrEmpty(pAONO) && !String.IsNullOrEmpty(pStyleCode) && !String.IsNullOrEmpty(pStyleSize) && !String.IsNullOrEmpty(pStyleColorSerial) && !String.IsNullOrEmpty(pRevNo) && !String.IsNullOrEmpty(pPRDPKD_ID))
        //		IsSinglePP = true;
        //	StringBuilder sb = new StringBuilder();
        //	//string strResult = "";
        //	string strSQL = "",
        //		strErrorMessage = "",
        //		strSQLWhere = "",
        //		strSQLWhereWO = ""
        //		;
        //	bool blHasError = false;
        //	bool blUpdateQCOJobStatus = true;
        //	//::: Get WeekNo
        //	DateTime dtStarDateTime = DateTime.Now.AddHours(36);
        //	//2019-06-14 Tai Le (Thomas) : Handle Single PP Material Readiness
        //	//2019-06-14 Tai Le (Thomas): use  intYear to Replace "dtStarDateTime.Year", in this way, able to re-use for SinglePackage
        //	int QCOYear = dtStarDateTime.Year;
        //	if (!String.IsNullOrEmpty(mEnviroment))
        //		if (mEnviroment.ToLower() == "console")
        //			Console.WriteLine("dtStarDateTime= " + dtStarDateTime.ToString("s"));
        //	sb.AppendLine("dtStarDateTime= " + dtStarDateTime.ToString("s"));
        //	CultureInfo cul = CultureInfo.CurrentCulture;
        //	int weekNum = cul.Calendar.GetWeekOfYear(dtStarDateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        //	string strWeekNum = "W" + PCMGeneralFunctions.GetRight("00" + weekNum, 2);
        //	/*2019-12-31 Tai Le (Thomas) Handle Year & Week when crossing to next year*/
        //	//Get Monday of Each Week
        //	var dtMonday = dtStarDateTime.StartOfWeek(DayOfWeek.Monday);
        //	var dtWed = dtMonday.AddDays(2);
        //	if (dtWed.Year > dtStarDateTime.Year)
        //		strWeekNum = "W01";
        //	QCOYear = dtWed.Year;
        //	/*:::END    2019-12-31 Tai Le (Thomas) Handle Year & Week when crossing to next year*/
        //	/** 2020-01-11 Tai Le(Thomas): Define Previous Week */
        //	int PreYear = 0;
        //	string strPreWeekNum = "";
        //	if (strWeekNum == "W01")
        //	{
        //		PreYear = QCOYear - 1;
        //		strPreWeekNum = GetYearMaxWeekNum(Factory, PreYear);
        //	}
        //	else
        //	{
        //		PreYear = QCOYear;
        //		strPreWeekNum = "W" + PCMGeneralFunctions.GetRight("00" + (weekNum - 1), 2);
        //	}
        //	retMessage += "Passed: Defined the Year + Week Step; ";
        //	sb.AppendLine($"QCOYear[{QCOYear}] , weekNum[{weekNum}] , PreYear[{PreYear}] , strPreWeekNum[{strPreWeekNum}]");
        //	if (UserRole == null)
        //	{
        //		strErrorMessage = "Can not find User Role to Calculate QCO Ranking.";
        //		sb.AppendLine($"Can not find User Role to Calculate QCO Ranking.");
        //		goto HE_Exit_QCOCalculate;
        //	}
        //	if (UserRole != "5000" && IsSinglePP == false)
        //	{
        //		strErrorMessage = "Wrong Role to Calculate QCO Ranking. Please log in with Role 5000.";
        //		sb.AppendLine($"Wrong Role to Calculate QCO Ranking. Please log in with Role 5000.");
        //		goto HE_Exit_QCOCalculate;
        //	}
        //	retMessage += "Passed: Role Verification Step; ";
        //	sb.AppendLine($"Passed: Role Verification Step; ");
        //	try
        //	{
        //		using (OracleConnection oracleConnection = new OracleConnection(OracleConnectionString))
        //		{
        //			oracleConnection.Open();
        //			//::: Pre - processing
        //			//* 1.1 If Status = "RUNNING"
        //			//*     >> Quit and Return the message "Factory is running by <EXECUTINGBY> at <EXECUTINGDATE> 
        //			strSQL = " SELECT EXECUTINGBY , EXECUTINGDATE " +
        //							" FROM PKMES.T_QC_QCFR " +
        //							" WHERE FACTORY = :FACTORY " +
        //							" AND STATUS IS NOT NULL " +
        //							" AND STATUS = 'RUNNING' ";
        //			DataTable dt = new DataTable();
        //			PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, new List<OracleParameter> { new OracleParameter("FACTORY", Factory) }, out dt, out strErrorMessage);
        //			if (dt != null)
        //				if (dt.Rows.Count > 0)
        //				{
        //					var ExecuteDate = dt.Rows[0][1].ToString();
        //					DateTime dtExecuteDate = DateTime.Parse(ExecuteDate);
        //					/* 2019/02/26: Tai Le (Thomas) modify the Rule
        //					 * Nếu Hiện Tại (Now) <= Execute +1 Day >> coi như process QCO đang chạy; không cho phép chạy QCO Calculation mới
        //					 * Nếu Hiện Tại (Now) > Execute +1 Day >> coi như process QCO Expired; cho phép chạy QCO Calculation mới 
        //					 */
        //					if (DateTime.Now <= dtExecuteDate.AddDays(1))
        //					{
        //						strErrorMessage = "Factory '" + Factory + "' Being Run QCO Calculation By " + dt.Rows[0][0] + " at " + dt.Rows[0][1];
        //						blHasError = true;
        //					}
        //					dt.Dispose();
        //				}
        //			if (blHasError && IsSinglePP == false)
        //			{
        //				//strResult = JsonConvert.SerializeObject(new { retResult = !blHasError, retData = "", retMsg = strErrorMessage });
        //				blUpdateQCOJobStatus = false;
        //				goto HE_QCOCalculate_Complete;
        //			}
        //			if (!String.IsNullOrEmpty(mEnviroment))
        //				if (mEnviroment.ToLower() == "console")
        //					Console.WriteLine("check [T_QC_QCFR]: PASSED");
        //			retMessage += "Passed: check [T_QC_QCFR]; ";
        //			sb.AppendLine($"Passed: check [T_QC_QCFR]; ");
        //			//* 2. Check the Availability of Factory Sorting Parameters on table PKMES.T_00_QCFO
        //			//* 2.1 If No Factory Setting
        //			//*     >> Quit and Return the message "Please set up the QCO Sorting Parameters on Factory <Factory>"
        //			strSQL = " SELECT * " +
        //					 " FROM PKMES.T_00_QCFO " +
        //					 " WHERE FACTORY = :FACTORY ";
        //			if (dt == null)
        //				dt = new DataTable();
        //			PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, new List<OracleParameter> { new OracleParameter("FACTORY", Factory) }, out dt, out strSQL);
        //			if (dt != null)
        //				if (dt.Rows.Count == 0)
        //				{
        //					strErrorMessage = "Factory QCO Sorting Setting On '" + Factory + "' Empty. Please Set Up QCO Setting.";
        //					blHasError = true;
        //					dt.Dispose();
        //				}
        //			if (blHasError)
        //			{
        //				//strResult = JsonConvert.SerializeObject(new { retResult = !blHasError, retData = "", retMsg = strErrorMessage });
        //				goto HE_QCOCalculate_Complete;
        //			}
        //			if (!String.IsNullOrEmpty(mEnviroment))
        //				if (mEnviroment.ToLower() == "console")
        //					Console.WriteLine("check [Factory QCO Sorting Setting]: PASSED");
        //			retMessage += "Passed: check [Factory QCO Sorting Setting]; ";
        //			sb.AppendLine($"Passed: check [Factory QCO Sorting Setting];");
        //			//* 3. Check, Whether QCO Factory + QCO Week   EXISTING  
        //			strSQL = " SELECT * " +
        //					 " FROM PKMES.T_QC_QUEUE " +
        //					 " WHERE QCOFACTORY = :FACTORY " +
        //					 " AND QCOYEAR = :YEAR " +
        //					 " AND QCOWEEKNO = :WEEK " +
        //					 " AND rownum <=2 ";
        //			if (dt == null)
        //				dt = new DataTable();
        //			PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, new List<OracleParameter> { new OracleParameter("FACTORY", Factory), new OracleParameter("YEAR", QCOYear), new OracleParameter("WEEK", strWeekNum) }, out dt, out strSQL);
        //			if (dt != null)
        //				if (dt.Rows.Count > 0)
        //				{
        //					if (DateTime.Now.AddHours(36) < dtMonday)
        //					{
        //						strErrorMessage = $@"QCO On Factory [{ Factory}] in Year [{QCOYear}] at Week No [{strWeekNum}] already EXIST. After Saturday 12:00 PM, QCO Calculation is Ready for Next Week No.";
        //						sb.AppendLine($"{strErrorMessage}");
        //						blHasError = true;
        //					}
        //					dt.Dispose();
        //				}
        //			if (blHasError && IsSinglePP == false)
        //			{
        //				//strResult = JsonConvert.SerializeObject(new { retResult = !blHasError, retData = "", retMsg = strErrorMessage });
        //				goto HE_QCOCalculate_Complete;
        //			}
        //			if (!String.IsNullOrEmpty(mEnviroment))
        //				if (mEnviroment.ToLower() == "console")
        //					Console.WriteLine("check [T_QC_QUEUE]: PASSED");
        //			retMessage += "Passed: check [T_QC_QUEUE]; ";
        //			sb.AppendLine($"Passed: check [T_QC_QUEUE]; ");
        //			/*::Processing
        //			 * 1. From View  "PKERP.VIEW_ERP_PSRSNP_PLAN", look for all the INCOMPLETE Packages
        //			 * 2. Save the satisfied Package into PKMES.T_QC_QCFP
        //			 * 3. Sort the PKMES.T_QC_QCFP based on The Factory Sorting Parameters on table PKMES.T_00_QCFO
        //			 * 4. Based on PKMES.T_QC_QCFP , PKERP.T_SD_BOMT ,  PKERP.V_WMS_PORC , Distribute the Received Qty for Each Package
        //			 *      4.1. PKMES.T_QC_QCFP  Join  PKERP.T_SD_BOMT together, and order by "Factory Sorting Parameters" , T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL
        //			 */
        //			if (mEnviroment.ToLower() == "console")
        //				Console.WriteLine("Start Re-make QCO For Factory= " + Factory + ", Year= " + QCOYear + ",Week= " + strWeekNum);
        //			retMessage += "Passed: Start Re-make QCO; ";
        //			sb.AppendLine($"Start Re-make QCO For Factory={Factory}, Year={QCOYear},Week={strWeekNum}");
        //			//2019-11-25 Tai Le (Thomas)
        //			var WeekMonday = PCMGeneralFunctions.GetDateFromWeekNumberAndDayOfWeek(QCOYear, weekNum, 0).Date;
        //			/* 2019-11-04 Tai Le (Thomas): move the Delete codes as Functions */
        //			DeleteQCOData(Factory, QCOYear, strWeekNum, IsSinglePP);
        //			if (!String.IsNullOrEmpty(mEnviroment))
        //				if (mEnviroment.ToLower() == "console")
        //					Console.WriteLine("DeleteQCOData(): PASSED");
        //			retMessage += "Passed: DeleteQCOData();";
        //			sb.AppendLine($"Passed: DeleteQCOData(); ");
        //			//::: Insert the Flag to mark the Running QCO Factory
        //			if (IsSinglePP == false)
        //				Insert_T_QC_QCFR(Factory, UserID, "Executing");
        //			if (!String.IsNullOrEmpty(mEnviroment))
        //				if (mEnviroment.ToLower() == "console")
        //					Console.WriteLine("Insert_T_QC_QCFR(): PASSED");
        //			retMessage += "Passed: Insert_T_QC_QCFR();";
        //			sb.AppendLine($"Passed: Insert_T_QC_QCFR(); ");
        //			//::: Get Factory QCO Setting:
        //			List<Qcfo> LcAllFactoryParameters = null;
        //			List<Qcfo> LcNoMateialFactoryParameters = null;
        //			FactoryHasMaterialParameter(Factory, out LcAllFactoryParameters, out LcNoMateialFactoryParameters);
        //			retMessage += "Passed: FactoryHasMaterialParameter();";
        //			sb.AppendLine($"Passed: FactoryHasMaterialParameter(); ");
        //			//::: Get MTOPS Package From  Chosen Factory  
        //			retMessage += $"GetMTOPSPackage() with Factory[{Factory}], PreYear[{PreYear}], strPreWeekNum[{strPreWeekNum}];";
        //			var _tempMsg = "";
        //			if (IsSinglePP == false)
        //				dt = GetMTOPSPackage(Factory, out _tempMsg, PreYear, strPreWeekNum);
        //			if (!String.IsNullOrEmpty(_tempMsg))
        //			{
        //				retMessage += $"Error at GetMTOPSPackage(): {_tempMsg};";
        //				sb.AppendLine($"Error at GetMTOPSPackage(): {_tempMsg};");
        //			}
        //			if (!String.IsNullOrEmpty(mEnviroment))
        //				if (mEnviroment.ToLower() == "console")
        //					Console.WriteLine("GetMTOPSPackage(): PASSED");
        //			if (dt == null)
        //			{
        //				strErrorMessage = "No AO-MTOPS Package Found Factory [" + Factory + "].";
        //				sb.AppendLine($"{strErrorMessage}");
        //				blHasError = true;
        //			}
        //			else if (dt.Rows.Count == 0)
        //			{
        //				strErrorMessage = "No AO-MTOPS Package Found Factory [" + Factory + "].";
        //				sb.AppendLine($"{strErrorMessage}");
        //				blHasError = true;
        //			}
        //			retMessage += "Passed: GetMTOPSPackage(); ";
        //			sb.AppendLine($"Passed: GetMTOPSPackage(); ");
        //			if (blHasError && IsSinglePP == false)
        //			{
        //				//strResult = JsonConvert.SerializeObject(new { retResult = !blHasError, retData = "", retMsg = strErrorMessage });
        //				goto HE_QCOCalculate_Complete;
        //			}
        //			//::: Sort MTOPS Package with Parameter Before Material Readiness
        //			if (IsSinglePP == false)
        //				Sort_T_QC_QCFP(ref dt, LcNoMateialFactoryParameters, "First");
        //			if (!String.IsNullOrEmpty(mEnviroment))
        //				if (mEnviroment.ToLower() == "console")
        //					Console.WriteLine("Sort_T_QC_QCFP(): PASSED");
        //			retMessage += "Passed: Sort_T_QC_QCFP(); ";
        //			sb.AppendLine($"Passed: Sort_T_QC_QCFP(); ");
        //			//::: Save Sorted PP Into PKMES.T_QC_QCFP
        //			if (IsSinglePP == false)
        //			{
        //				//Save_T_QC_QCFP(OracleConnectionString, Factory, dtStarDateTime, QCOYear, strWeekNum, dt);
        //				Save_T_QC_QCFP(Factory, WeekMonday, QCOYear, strWeekNum, dt);
        //			}
        //			if (!String.IsNullOrEmpty(mEnviroment))
        //				if (mEnviroment.ToLower() == "console")
        //					Console.WriteLine("Save_T_QC_QCFP(): PASSED");
        //			retMessage += "Passed: Save_T_QC_QCFP(); ";
        //			sb.AppendLine($"Passed: Save_T_QC_QCFP(); ");
        //			if (dt != null)
        //			{
        //				dt.Clear();
        //				dt.Dispose();
        //			}
        //			//2019-06-14 Tai Le (Thomas): Handle Update Material Readiness For Single MTOPS Production Package 
        //			if (IsSinglePP)
        //			{
        //				//Get the Lastest QCO WEEK; QCO YEAR 
        //				strSQL =
        //					" SELECT * FROM PKMES.T_QC_QCFP " +
        //					" WHERE FACTORY = '" + Factory + "' " +
        //					" AND AONO = '" + pAONO + "'  " +
        //					" AND STYLECODE = '" + pStyleCode + "' " +
        //					" AND STYLESIZE = '" + pStyleSize + "' " +
        //					" AND STYLECOLORSERIAL = '" + pStyleColorSerial + "' " +
        //					" AND REVNO = '" + pRevNo + "' " +
        //					" AND PRDPKG = '" + pPRDPKD_ID + "' " +
        //					" ORDER BY QCOYEAR DESC , QCOWEEKNO DESC  ";
        //				DataTable dt_Temp = new DataTable();
        //				PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, null, out dt_Temp, out strSQL);
        //				if (dt_Temp != null)
        //				{
        //					if (dt_Temp.Rows.Count > 0)
        //					{
        //						strWeekNum = dt_Temp.Rows[0]["QCOWEEKNO"].ToString();
        //						QCOYear = Convert.ToInt32(dt_Temp.Rows[0]["QCOYEAR"].ToString());
        //					}
        //					dt_Temp.Clear();
        //					dt_Temp.Dispose();
        //				}
        //				//Get All WO
        //				strSQL =
        //					" Select WONO " +
        //					"   From PKMES.V_MRP_PP_WO " +
        //					"   Where FACTORY = '" + Factory + "' " +
        //					"   And AONO = '" + pAONO + "'  " +
        //					"   And STLCD = '" + pStyleCode + "' " +
        //					"   And STLSIZ = '" + pStyleSize + "' " +
        //					"   And STLCOSN = '" + pStyleColorSerial + "' " +
        //					"   And STLREVN = '" + pRevNo + "' " +
        //					"   And PRODPACKAGE = '" + pPRDPKD_ID + "' " +
        //					"   Group By WONO " +
        //					"  ";
        //				dt_Temp = new DataTable();
        //				PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, null, out dt_Temp, out strSQL);
        //				if (dt_Temp != null)
        //				{
        //					if (dt_Temp.Rows.Count > 0)
        //					{
        //						strSQLWhereWO += "( ";
        //						int intI_ = 0;
        //						for (intI_ = 0; intI_ < dt_Temp.Rows.Count; intI_++)
        //						{
        //							if (intI_ == dt_Temp.Rows.Count - 1)
        //								strSQLWhereWO += " '" + dt_Temp.Rows[intI_]["WONO"] + "' ";
        //							else
        //								strSQLWhereWO += " '" + dt_Temp.Rows[intI_]["WONO"] + "', ";
        //						}
        //						strSQLWhereWO += " )";
        //					}
        //					dt_Temp.Clear();
        //					dt_Temp.Dispose();
        //				}
        //				//Get All the PP having same WO with SELECTED PRDPKG
        //				strSQL =
        //					" SELECT AONO, FACTORY , STLCD , STLSIZ , STLCOSN , STLREVN , PRODPACKAGE " +
        //					" FROM PKMES.V_MRP_PP_WO " +
        //					" WHERE WONO IN " +
        //					" ( Select WONO " +
        //					"   From PKMES.V_MRP_PP_WO " +
        //					"   Where FACTORY = '" + Factory + "' " +
        //					"   And AONO = '" + pAONO + "'  " +
        //					"   And STLCD = '" + pStyleCode + "' " +
        //					"   And STLSIZ = '" + pStyleSize + "' " +
        //					"   And STLCOSN = '" + pStyleColorSerial + "' " +
        //					"   And STLREVN = '" + pRevNo + "' " +
        //					"   And PRODPACKAGE = '" + pPRDPKD_ID + "' " +
        //					"   Group By WONO " +
        //					" ) " +
        //					" GROUP BY AONO, FACTORY , STLCD , STLSIZ , STLCOSN , STLREVN , PRODPACKAGE " +
        //					"  ";
        //				dt_Temp = new DataTable();
        //				PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, null, out dt_Temp, out strSQL);
        //				if (dt_Temp != null)
        //				{
        //					if (dt_Temp.Rows.Count > 0)
        //					{
        //						strSQLWhere += "( ";
        //						int intI_ = 0;
        //						for (intI_ = 0; intI_ < dt_Temp.Rows.Count; intI_++)
        //						{
        //							if (intI_ == dt_Temp.Rows.Count - 1)
        //								strSQLWhere += " '" + dt_Temp.Rows[intI_]["AONO"] + dt_Temp.Rows[intI_]["FACTORY"] + dt_Temp.Rows[intI_]["STLCD"] + dt_Temp.Rows[intI_]["STLSIZ"] + dt_Temp.Rows[intI_]["STLCOSN"] + dt_Temp.Rows[intI_]["STLREVN"] + dt_Temp.Rows[intI_]["PRODPACKAGE"] + "' ";
        //							else
        //								strSQLWhere += " '" + dt_Temp.Rows[intI_]["AONO"] + dt_Temp.Rows[intI_]["FACTORY"] + dt_Temp.Rows[intI_]["STLCD"] + dt_Temp.Rows[intI_]["STLSIZ"] + dt_Temp.Rows[intI_]["STLCOSN"] + dt_Temp.Rows[intI_]["STLREVN"] + dt_Temp.Rows[intI_]["PRODPACKAGE"] + "', ";
        //						}
        //						strSQLWhere += " )";
        //					}
        //					dt_Temp.Clear();
        //					dt_Temp.Dispose();
        //				}
        //			}
        //			//::: Distribute the Material into MTOPS Package of PKMES.T_QC_QCFP
        //			//::: Get PP & T_SD_BOMT
        //			//strSQL = " SELECT ROW_NUMBER() OVER(PARTITION BY T_QC_QCFP.FACTORY ORDER BY T_QC_QCFP.FACTORY, T_QC_QCFP.DELIVERYDATE , T_QC_QCFP.ORDQTY ,  T_QC_QCFP.PLANQTY , " +
        //			//         " T_QC_QCFP.AONO , T_QC_QCFP.STYLECODE , T_QC_QCFP.STYLESIZE , T_QC_QCFP.STYLECOLORSERIAL , T_QC_QCFP.REVNO , T_QC_QCFP.PRDPKG ) AS RowSeqNo , " +
        //			//         " LEAD(T_QC_QCFP.ID, 1, '') OVER (ORDER BY T_QC_QCFP.ID) AS NEXT_ID , " +
        //			//         " T_QC_QCFP.* , V_MRP_PP_WO.WONO , " +
        //			//         " T_SD_BOMT.MAINITEMCODE , T_SD_BOMT.MAINITEMCOLORSERIAL , " +
        //			//         " T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL , " +
        //			//         " T_QC_QCFP.PLANQTY * T_SD_BOMT.UNITCONSUMPTION AS REQUESTQTY , " +
        //			//         " LEAD(T_QC_QCFP.ID, 1, 0) OVER (ORDER BY T_QC_QCFP.ID) AS NEXT_ID " +
        //			//         " FROM PKMES.T_QC_QCFP " +
        //			//         " LEFT JOIN PKERP.V_AO_PPDP ON " +
        //			//         "      T_QC_QCFP.FACTORY = V_AO_PPDP.FACTORY " +
        //			//         "      AND T_QC_QCFP.AONO = V_AO_PPDP.AONO " +
        //			//         "      AND T_QC_QCFP.STYLECODE = V_AO_PPDP.STYLECODE " +
        //			//         "      AND T_QC_QCFP.STYLESIZE = V_AO_PPDP.STYLESIZE " +
        //			//         "      AND T_QC_QCFP.STYLECOLORSERIAL = V_AO_PPDP.STYLECOLORSERIAL " +
        //			//         "      AND T_QC_QCFP.REVNO = V_AO_PPDP.REVNO " +
        //			//         "      AND T_QC_QCFP.PRDPKG = V_AO_PPDP.PRDPKG " +
        //			//         " INNER JOIN PKMES.V_MRP_PP_WO ON " +
        //			//         "      T_QC_QCFP.FACTORY = V_MRP_PP_WO.FACTORY " +
        //			//         "      AND T_QC_QCFP.AONO = V_MRP_PP_WO.AONO " +
        //			//         "      AND T_QC_QCFP.STYLECODE = V_MRP_PP_WO.STLCD " +
        //			//         "      AND T_QC_QCFP.STYLESIZE = V_MRP_PP_WO.STLSIZ " +
        //			//         "      AND T_QC_QCFP.STYLECOLORSERIAL = V_MRP_PP_WO.STLCOSN " +
        //			//         "      AND T_QC_QCFP.REVNO = V_MRP_PP_WO.STLREVN " +
        //			//         "      AND T_QC_QCFP.PRDPKG = V_MRP_PP_WO.PRODPACKAGE " +
        //			//         " INNER JOIN PKERP.T_SD_BOMT ON " +
        //			//         "      T_QC_QCFP.STYLECODE = T_SD_BOMT.STYLECODE " +
        //			//         "      AND T_QC_QCFP.STYLESIZE = T_SD_BOMT.STYLESIZE " +
        //			//         "      AND T_QC_QCFP.STYLECOLORSERIAL = T_SD_BOMT.STYLECOLORSERIAL " +
        //			//         "      AND T_QC_QCFP.REVNO = T_SD_BOMT.REVNO " +
        //			//         " WHERE " +
        //			//         " T_QC_QCFP.QCOFACTORY = '" + Factory + "'  " +
        //			//         " AND T_QC_QCFP.QCOYEAR = " + QCOYear + " " +
        //			//         " AND T_QC_QCFP.QCOWEEKNO = '" + strWeekNum + "'  " +
        //			//         " AND (T_SD_BOMT.ITEMCODE NOT LIKE 'PKG%' OR T_SD_BOMT.ITEMCODE NOT LIKE 'TRE%' )  ";
        //			//2019-12-02 Tai Le(Thomas) add 3 columns {STAR_LEV3, STAR_LEV2, STAR_LEV1}
        //			strSQL = " SELECT ROW_NUMBER() OVER(PARTITION BY T_QC_QCFP.FACTORY ORDER BY T_QC_QCFP.FACTORY, T_QC_QCFP.DELIVERYDATE , T_QC_QCFP.ORDQTY ,  T_QC_QCFP.PLANQTY , " +
        //					 " T_QC_QCFP.AONO , T_QC_QCFP.STYLECODE , T_QC_QCFP.STYLESIZE , T_QC_QCFP.STYLECOLORSERIAL , T_QC_QCFP.REVNO , T_QC_QCFP.PRDPKG ) AS RowSeqNo , " +
        //					 " LEAD(T_QC_QCFP.ID, 1, '') OVER (ORDER BY T_QC_QCFP.ID) AS NEXT_ID , " +
        //					 " T_QC_QCFP.* , V_MRP_PP_WO.WONO , " +
        //					 " T_SD_BOMT.MAINITEMCODE , T_SD_BOMT.MAINITEMCOLORSERIAL , " +
        //					 " T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL , " +
        //					 " T_QC_QCFP.PLANQTY * T_SD_BOMT.UNITCONSUMPTION AS REQUESTQTY , " +
        //					 " LEAD(T_QC_QCFP.ID, 1, 0) OVER (ORDER BY T_QC_QCFP.ID) AS NEXT_ID " +
        //					 " FROM PKMES.T_QC_QCFP " +
        //					 " LEFT JOIN PKERP.V_AO_PPDP ON " +
        //					 "      T_QC_QCFP.FACTORY = V_AO_PPDP.FACTORY " +
        //					 "      AND T_QC_QCFP.AONO = V_AO_PPDP.AONO " +
        //					 "      AND T_QC_QCFP.STYLECODE = V_AO_PPDP.STYLECODE " +
        //					 "      AND T_QC_QCFP.STYLESIZE = V_AO_PPDP.STYLESIZE " +
        //					 "      AND T_QC_QCFP.STYLECOLORSERIAL = V_AO_PPDP.STYLECOLORSERIAL " +
        //					 "      AND T_QC_QCFP.REVNO = V_AO_PPDP.REVNO " +
        //					 "      AND T_QC_QCFP.PRDPKG = V_AO_PPDP.PRDPKG " +
        //					 " LEFT JOIN PKMES.V_MRP_PP_WO ON " +
        //					 "      T_QC_QCFP.STYLECODE = V_MRP_PP_WO.STLCD " +
        //					 "      AND T_QC_QCFP.STYLESIZE = V_MRP_PP_WO.STLSIZ " +
        //					 "      AND T_QC_QCFP.STYLECOLORSERIAL = V_MRP_PP_WO.STLCOSN " +
        //					 "      AND T_QC_QCFP.REVNO = V_MRP_PP_WO.STLREVN " +
        //					 "      AND T_QC_QCFP.PRDPKG = V_MRP_PP_WO.PRODPACKAGE " +
        //					 " INNER JOIN PKERP.T_SD_BOMT ON " +
        //					 "      T_QC_QCFP.STYLECODE = T_SD_BOMT.STYLECODE " +
        //					 "      AND T_QC_QCFP.STYLESIZE = T_SD_BOMT.STYLESIZE " +
        //					 "      AND T_QC_QCFP.STYLECOLORSERIAL = T_SD_BOMT.STYLECOLORSERIAL " +
        //					 "      AND T_QC_QCFP.REVNO = T_SD_BOMT.REVNO " +
        //					 " WHERE " +
        //					 " T_QC_QCFP.QCOFACTORY = '" + Factory + "'  " +
        //					 " AND T_QC_QCFP.QCOYEAR = " + QCOYear + " " +
        //					 " AND T_QC_QCFP.QCOWEEKNO = '" + strWeekNum + "'  " +
        //					 " AND (T_SD_BOMT.ITEMCODE NOT LIKE 'PKG%' AND T_SD_BOMT.ITEMCODE NOT LIKE 'TRE%' )  ";
        //			if (IsSinglePP)
        //				strSQL += " AND T_QC_QCFP.AONO || T_QC_QCFP.FACTORY || T_QC_QCFP.STYLECODE || T_QC_QCFP.STYLESIZE || T_QC_QCFP.STYLECOLORSERIAL || T_QC_QCFP.REVNO || T_QC_QCFP.PRDPKG IN " + strSQLWhere;
        //			strSQL += " ORDER BY T_QC_QCFP.ID , T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL ";
        //			DataTable dt_QCFP = new DataTable();
        //			PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, new List<OracleParameter> { new OracleParameter("FACTORY", Factory) }, out dt_QCFP, out strSQL);
        //			sb.AppendLine($"[PASSED] Binding: PKMES.T_QC_QCFP + T_SD_BOMT ");
        //			//Add column ASSIGNEDQTY for Distribution Purpose.
        //			DataColumn newColumn = new DataColumn("ASSIGNEDQTY", typeof(System.Double)) { DefaultValue = 0.0 };
        //			dt_QCFP.Columns.Add(newColumn);
        //			newColumn.Dispose();
        //			//2019-12-11 Tai Le (Thomas) add 3 more columns for Material Rating
        //			newColumn = new DataColumn("MATPRIORITYLEV3", typeof(System.Double)) { DefaultValue = 0.0 };
        //			dt_QCFP.Columns.Add(newColumn);
        //			newColumn.Dispose();
        //			newColumn = new DataColumn("MATPRIORITYLEV2", typeof(System.Double)) { DefaultValue = 0.0 };
        //			dt_QCFP.Columns.Add(newColumn);
        //			newColumn.Dispose();
        //			newColumn = new DataColumn("MATPRIORITYLEV1", typeof(System.Double)) { DefaultValue = 0.0 };
        //			dt_QCFP.Columns.Add(newColumn);
        //			newColumn.Dispose();
        //			sb.AppendLine($"PASSED: PKMES.T_QC_QCFP + T_SD_BOMT added more columns");
        //			//::: END   2019-12-11
        //			//::: Open T_QC_QCPM to Save the Material Distribution
        //			//::: 2019-04-04: Tai Le (THOMAS) Add  QCOFACTORY, QCOYEAR, QCOWEEKNO To WHERE Syntax
        //			//strSQL = " SELECT * " +
        //			//         " FROM PKMES.T_QC_QCPM " +
        //			//         " WHERE FACTORY = '" + Factory + "' ";
        //			if (IsSinglePP == true)
        //			{
        //				//Delete PKMES.T_QC_QCPM From Related Packages in same WONO
        //				strSQL =
        //					" DELETE PKMES.T_QC_QCPM " +
        //					" WHERE AONO || FACTORY || STYLECODE || STYLESIZE || STYLECOLORSERIAL || REVNO || PRDPKG IN " + strSQLWhere;
        //				OracleCommand oracleCommand = new OracleCommand(strSQL, oracleConnection);
        //				oracleCommand.CommandTimeout = 90;
        //				oracleCommand.ExecuteNonQuery();
        //			}
        //			strSQL = " SELECT * " +
        //					 " FROM PKMES.T_QC_QCPM " +
        //					 " WHERE FACTORY = '" + Factory + "' " +
        //					 " AND QCOFACTORY = '" + Factory + "' " +
        //					 " AND QCOYEAR = " + QCOYear + " " +
        //					 " AND QCOWEEKNO = '" + strWeekNum + "' " +
        //					 "";
        //			if (IsSinglePP == true)
        //				strSQL += " AND AONO || FACTORY || STYLECODE || STYLESIZE || STYLECOLORSERIAL || REVNO || PRDPKG IN " + strSQLWhere;
        //			DataTable dt_T_QC_QCPM = new DataTable();
        //			OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
        //			oracleDataAdapter.Fill(dt_T_QC_QCPM);
        //			if (!String.IsNullOrEmpty(mEnviroment))
        //				if (mEnviroment.ToLower() == "console")
        //					Console.WriteLine("Starting Distribution: PASSED");
        //			retMessage += "Start: Material Distribution";
        //			/** * Code SLOWS DOWN the DB  
        //			////::: DISTRIBUTE MATERIAL
        //			//int intSeqNo = 0;
        //			////::: Distribute WMS Qty (Received Material Qty) 
        //			//strSQL =
        //			//    " SELECT * " +
        //			//    " FROM PKERP.V_WO_RECWMS " +
        //			//    " WHERE ( WO >= " +
        //			//    "   (SELECT MIN (SUBSTR(V_MRP_PP_WO.WONO,1,2))  " +
        //			//    "    FROM PKMES.T_QC_QCFP " +
        //			//    "    INNER JOIN PKMES.V_MRP_PP_WO ON  " +
        //			//    "       T_QC_QCFP.FACTORY = V_MRP_PP_WO.FACTORY " +
        //			//    "       AND T_QC_QCFP.AONO = V_MRP_PP_WO.AONO " +
        //			//    "       AND T_QC_QCFP.STYLECODE = V_MRP_PP_WO.STLCD " +
        //			//    "       AND T_QC_QCFP.STYLESIZE = V_MRP_PP_WO.STLSIZ " +
        //			//    "       AND T_QC_QCFP.STYLECOLORSERIAL = V_MRP_PP_WO.STLCOSN " +
        //			//    "       AND T_QC_QCFP.REVNO = V_MRP_PP_WO.STLREVN " +
        //			//    "       AND T_QC_QCFP.PRDPKG = V_MRP_PP_WO.PRODPACKAGE " +
        //			//    "    WHERE T_QC_QCFP.QCOFACTORY = '" + Factory + "' AND T_QC_QCFP.QCOYEAR = " + QCOYear + " AND T_QC_QCFP.QCOWEEKNO = '" + strWeekNum + "' ";
        //			//if (IsSinglePP) strSQL += " AND T_QC_QCFP.AONO || T_QC_QCFP.FACTORY || T_QC_QCFP.STYLECODE || T_QC_QCFP.STYLESIZE || T_QC_QCFP.STYLECOLORSERIAL || T_QC_QCFP.REVNO || T_QC_QCFP.PRDPKG IN " + strSQLWhere;
        //			//strSQL += "   ) ) OR WO IN (Select AONO From PKMES.T_QC_QCFP Where T_QC_QCFP.QCOFACTORY = '" + Factory + "' AND T_QC_QCFP.QCOYEAR = " + QCOYear + " AND T_QC_QCFP.QCOWEEKNO = '" + strWeekNum + "' Group By AONO ) ";
        //			//if (IsSinglePP)
        //			//    strSQL += " AND WO IN " + strSQLWhereWO;
        //			//strSQL +=
        //			//    " ORDER BY WO , ITEM_CD , COLOR_SERIAL , PLAN_DOQTY ";
        //			//OracleCommand command = new OracleCommand(strSQL, oracleConnection);
        //			//var dr_WMS = command.ExecuteReader();
        //			//DistributeMaterial_T_QC_QCPM(Factory, QCOYear, strWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, dr_WMS, "W", ref intSeqNo);
        //			//dr_WMS.Close();
        //			//dr_WMS.Dispose();
        //			//command.Dispose();
        //			//if (!String.IsNullOrEmpty(mEnviroment))
        //			//    if (mEnviroment.ToLower() == "console")
        //			//        Console.WriteLine("DistributeMaterial_T_QC_QCPM for W: PASSED");
        //			////::: Update the Material Readiness back to dt_QCFP based on dt_T_QC_QCPM  { QUANTITY_A ; REQUESTQTY }
        //			//Update_T_QC_QCFP_MaterialReadiness(ref dt_QCFP, dt_T_QC_QCPM);
        //			////::: Sort dt_QCFP with LcAllFactoryParameters
        //			////Sort_T_QC_QCFP(ref dt_QCFP, LcAllFactoryParameters, "All");
        //			*/
        //			//::: DISTRIBUTE MATERIAL
        //			var DistributeMaterialRes = this.DistributeMaterial(IsSinglePP, OracleConnectionString, Factory, QCOYear, strWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP);
        //			sb.AppendLine($"{DistributeMaterialRes}");
        //			/**
        //			//int intSeqNo = 0;
        //			////::: Distribute WMS Qty (Received Material Qty) 
        //			////::: WMS based on WONO (MRP_OLD data)
        //			//strSQL =
        //			//    " SELECT * " +
        //			//    " FROM PKERP.V_WO_RECWMS " +
        //			//    " WHERE " +
        //			//    " ( WO IN " +
        //			//    "   (Select V_MRP_PP_WO.WONO  " +
        //			//    "    From PKMES.T_QC_QCFP " +
        //			//    "    Inner Join PKMES.V_MRP_PP_WO On  " +
        //			//    "       T_QC_QCFP.FACTORY = V_MRP_PP_WO.FACTORY " +
        //			//    "       AND T_QC_QCFP.AONO = V_MRP_PP_WO.AONO " +
        //			//    "       AND T_QC_QCFP.STYLECODE = V_MRP_PP_WO.STLCD " +
        //			//    "       AND T_QC_QCFP.STYLESIZE = V_MRP_PP_WO.STLSIZ " +
        //			//    "       AND T_QC_QCFP.STYLECOLORSERIAL = V_MRP_PP_WO.STLCOSN " +
        //			//    "       AND T_QC_QCFP.REVNO = V_MRP_PP_WO.STLREVN " +
        //			//    "       AND T_QC_QCFP.PRDPKG = V_MRP_PP_WO.PRODPACKAGE " +
        //			//    "    WHERE T_QC_QCFP.QCOFACTORY = '" + Factory + "' AND T_QC_QCFP.QCOYEAR = " + QCOYear + " AND T_QC_QCFP.QCOWEEKNO = '" + strWeekNum + "' " +
        //			//    "    Group By V_MRP_PP_WO.WONO) " +
        //			//    " ) " +
        //			//    " ORDER BY WO , ITEM_CD , COLOR_SERIAL , PLAN_DOQTY ";
        //			//OracleCommand command = new OracleCommand(strSQL, oracleConnection);
        //			//var dr_WMS = command.ExecuteReader();
        //			//DistributeMaterial_T_QC_QCPM(Factory, QCOYear, strWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, dr_WMS, "W", ref intSeqNo);
        //			//dr_WMS.Close();
        //			//dr_WMS.Dispose();
        //			//command.Dispose();
        //			//if (mEnviroment == "Console")
        //			//    Console.WriteLine("DistributeMaterial_T_QC_QCPM() for W: PASSED");
        //			////::: Distribute WMS Qty (Received Material Qty) 
        //			////::: WMS based on AONO (MRP2 data)
        //			//strSQL =
        //			//    " SELECT * " +
        //			//    " FROM PKERP.V_WO_RECWMS " +
        //			//    " WHERE " +
        //			//    " WO LIKE 'AD%' " +
        //			//    " AND WO IN (Select AONO From PKMES.T_QC_QCFP  Where T_QC_QCFP.QCOFACTORY = '" + Factory + "' AND T_QC_QCFP.QCOYEAR = " + QCOYear + " AND T_QC_QCFP.QCOWEEKNO = '" + strWeekNum + "' Group By AONO) " +
        //			//    " ORDER BY WO , ITEM_CD , COLOR_SERIAL , PLAN_DOQTY ";
        //			//command = new OracleCommand(strSQL, oracleConnection);
        //			//var dr_WMS2 = command.ExecuteReader();
        //			//DistributeMaterial_T_QC_QCPM(Factory, QCOYear, strWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, dr_WMS2, "W2", ref intSeqNo);
        //			//dr_WMS.Close();
        //			//dr_WMS.Dispose();
        //			//command.Dispose();
        //			//if (mEnviroment == "Console")
        //			//    Console.WriteLine("DistributeMaterial_T_QC_QCPM() for W2: PASSED");
        //			////::: Distribute KMS Qty (Incoming Qty)
        //			////Get the Monday based on Year and WeekNo 
        //			////2019-06-15
        //			//if (IsSinglePP)
        //			//    weekNum = cul.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        //			//DateTime dtMonday = PCMGeneralFunctions.GetDateFromWeekNumberAndDayOfWeek(QCOYear, weekNum, 0);
        //			//strSQL = " SELECT WO , ITEM_CD , COLOR_SERIAL , ETA , SUM(SHIP_QTY) PLAN_DOQTY " +
        //			//         " FROM KMS_PSRSHP_TBL@AOMTOPS " +
        //			//         " WHERE DELFLG = 'N' " +
        //			//         " AND ETA IS NOT NULL " +
        //			//         " AND Length(ETA) = 8 " +
        //			//         " AND ETA >= '" + dtMonday.ToString("yyyyMMdd") + "' " +
        //			//         " GROUP BY WO , ITEM_CD , COLOR_SERIAL , ETA  ";
        //			//command = new OracleCommand(strSQL, oracleConnection);
        //			//var dr_KMS = command.ExecuteReader();
        //			//DistributeMaterial_T_QC_QCPM(Factory, QCOYear, strWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, dr_KMS, "K", ref intSeqNo);
        //			//dr_KMS.Close();
        //			//dr_KMS.Dispose();
        //			//command.Dispose();
        //			//if (!String.IsNullOrEmpty(mEnviroment))
        //			//    if (mEnviroment.ToLower() == "console")
        //			//        Console.WriteLine("DistributeMaterial_T_QC_QCPM for K: PASSED");
        //			///*2019-11-01 Tai Le (Thomas) add part of KMS from MRP2* /
        //			//strSQL = " SELECT PRDPKG , ITEM_CD , COLOR_SERIAL , ETA , SUM(ORD_CNF_QTY) PLAN_DOQTY " +
        //			//         " FROM KMS_PSRSHP2_TBL@AOMTOPS " +
        //			//         " WHERE ETA IS NOT NULL " +
        //			//         " AND Length(ETA) = 8 " +
        //			//         " AND ETA >= '" + dtMonday.ToString("yyyyMMdd") + "' " +
        //			//         " GROUP BY PRDPKG , ITEM_CD , COLOR_SERIAL , ETA ";
        //			//command = new OracleCommand(strSQL, oracleConnection);
        //			//var dr_KMS2 = command.ExecuteReader();
        //			//DistributeMaterial_T_QC_QCPM(Factory, QCOYear, strWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, dr_KMS2, "K2", ref intSeqNo);
        //			//dr_KMS2.Close();
        //			//dr_KMS2.Dispose();
        //			//command.Dispose();
        //			//if (!String.IsNullOrEmpty(mEnviroment))
        //			//    if (mEnviroment.ToLower() == "console")
        //			//        Console.WriteLine("DistributeMaterial_T_QC_QCPM for K2: PASSED");
        //			// ::END  2019-11-01 Tai Le (Thomas) add part of KMS from MRP2  
        //			//:::END    DISTRIBUTE MATERIAL
        //			*/
        //			//::: Save T_QC_QCPM
        //			OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
        //			oracleDataAdapter.Update(dt_T_QC_QCPM);
        //			oracleCommandBuilder.Dispose();
        //			if (!String.IsNullOrEmpty(mEnviroment))
        //				if (mEnviroment.ToLower() == "console")
        //					Console.WriteLine("Save & Commit [dt_T_QC_QCPM]: PASSED");
        //			retMessage += "PASSED: Save & Commit [dt_T_QC_QCPM]";
        //			sb.AppendLine($"PASSED: Save & Commit [dt_T_QC_QCPM]");
        //			//::: Update the Material Readiness back to dt_QCFP based on dt_T_QC_QCPM  { QUANTITY_A ; REQUESTQTY }
        //			Update_T_QC_QCFP_MaterialReadiness(ref dt_QCFP, dt_T_QC_QCPM);
        //			dt_T_QC_QCPM.Dispose();
        //			oracleDataAdapter.Dispose();
        //			if (!String.IsNullOrEmpty(mEnviroment))
        //				if (mEnviroment.ToLower() == "console")
        //					Console.WriteLine("Update Material Readiness: PASSED");
        //			retMessage += "PASSED: Update Material Readiness";
        //			sb.AppendLine($"PASSED: Update Material Readiness");
        //			//2019-12-11 Tai Le (Thomas): Remove Repeat row which has  ["MATNORNALRATE"] = -1 
        //			RemoveDuplicateRowQCFP(ref dt_QCFP);
        //			if (!String.IsNullOrEmpty(mEnviroment))
        //				if (mEnviroment.ToLower() == "console")
        //					Console.WriteLine("RemoveDuplicateRowQCFP(): PASSED");
        //			//:::END    2019-12-11 Tai Le
        //			retMessage += "PASSED: RemoveDuplicateRowQCFP()";
        //			sb.AppendLine($"PASSED: RemoveDuplicateRowQCFP()");
        //			//2019-12-11 Tai Le: Add
        //			//Update MATPRIORITYLEV3 ; MATPRIORITYLEV2 ; MATPRIORITYLEV1
        //			UpdateMaterialRateT_QC_QCFP(ref dt_QCFP, Factory, QCOYear, strWeekNum);
        //			if (!String.IsNullOrEmpty(mEnviroment))
        //				if (mEnviroment.ToLower() == "console")
        //					Console.WriteLine("Update UpdateMaterialRate on [dt_QCFP]: PASSED");
        //			retMessage += "PASSED: Update UpdateMaterialRate on [dt_QCFP]";
        //			sb.AppendLine($"PASSED: UpdateMaterialRateT_QC_QCFP()");
        //			//:::END    2019-12-11 Tai Le
        //			//::: Sort dt_QCFP with LcAllFactoryParameters
        //			if (IsSinglePP == false)
        //				Sort_T_QC_QCFP(ref dt_QCFP, LcAllFactoryParameters, "All");
        //			sb.AppendLine($"PASSED: Sort_T_QC_QCFP()");
        //			//::: SAVE dt_T_QC_QUEUE
        //			if (IsSinglePP == false)
        //			{
        //				//PCMGeneralFunctions.GetDateFromWeekNumberAndDayOfWeek(QCOYear, weekNum, 0)
        //				//Save_T_QC_QUEUE(OracleConnectionString, Factory, dtStarDateTime.Date, QCOYear, strWeekNum, dt_QCFP);
        //				Save_T_QC_QUEUE(Factory, WeekMonday.Date, QCOYear, strWeekNum, dt_QCFP);
        //				sb.AppendLine($"PASSED: Save_T_QC_QUEUE()");
        //			}
        //			else
        //			{
        //				Update_T_QC_QUEUE(Factory, QCOYear, strWeekNum, dt_QCFP);
        //				sb.AppendLine($"PASSED: Update_T_QC_QUEUE()");
        //			}
        //			dt_QCFP.Dispose();
        //			if (!String.IsNullOrEmpty(mEnviroment))
        //				if (mEnviroment.ToLower() == "console")
        //					Console.WriteLine("Save/Update [T_QC_QUEUE]: PASSED");
        //			retMessage += "PASSED: Save/Update [T_QC_QUEUE]";
        //			//Update QCORANKINGNEW  
        //			OracleCommand command = new OracleCommand("Update PKMES.T_QC_QUEUE SET QCORANKINGNEW = ROWNUM WHERE QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO ", oracleConnection);
        //			command.Parameters.Add(new OracleParameter("QCOFACTORY", Factory));
        //			command.Parameters.Add(new OracleParameter("QCOYEAR", QCOYear));
        //			command.Parameters.Add(new OracleParameter("QCOWEEKNO", strWeekNum));
        //			if (IsSinglePP == false)
        //				command.ExecuteNonQuery();
        //			command.Dispose();
        //			if (!String.IsNullOrEmpty(mEnviroment))
        //				if (mEnviroment.ToLower() == "console")
        //					Console.WriteLine("Update QCORANKINGNEW on [T_QC_QUEUE]: PASSED");
        //			retMessage += "PASSED: Update QCORANKINGNEW on [T_QC_QUEUE]";
        //			sb.AppendLine($"PASSED: Update QCORANKINGNEW on [T_QC_QUEUE]");
        //		HE_QCOCalculate_Complete:
        //			oracleConnection.Close();
        //			oracleConnection.Dispose();
        //		}
        //	}
        //	catch (Exception ex)
        //	{
        //		blHasError = true;
        //		//strResult = JsonConvert.SerializeObject(new { retResult = false, retData = "", retMsg = ex.Message + "; SQL= " + strSQL });
        //		strErrorMessage = ex.Message;
        //		sb.AppendLine($"System Fatal: {strErrorMessage}");
        //	}
        //	retMessage += strErrorMessage;
        //	if (retMessage.Length > 0)
        //	{
        //		//2019-12-07 Tai Le: send message to Telegram Group "PCMNotify"
        //		var botClient = new TelegramBotClient(TeleTokenID);
        //		botClient.SendTextMessageAsync(-1001407116473, $"Factory[{Factory}], Year[{QCOYear}], WONo [{strWeekNum}] : " + retMessage).Wait();
        //	}
        //HE_Exit_QCOCalculate:
        //	//::: Complete the Flag
        //	if (blUpdateQCOJobStatus)
        //	{
        //		Complete_T_QC_QCFR(Factory, QCOYear, strWeekNum, UserID, strErrorMessage, blHasError);
        //		sb.AppendLine($"PASSED:  Complete_T_QC_QCFR();");
        //	}
        //	if (!blHasError)
        //	{
        //		strErrorMessage = $"QCO Ranking For Factory[{Factory}]; Year[{QCOYear}]; WONo [{strWeekNum}] : Built Success.";
        //		//2019-12-07 Tai Le: send message to Telegram Group "PCMNotify"
        //		var botClient = new TelegramBotClient(TeleTokenID);
        //		botClient.SendTextMessageAsync(-1001407116473, strErrorMessage).Wait();
        //		mFactory = Factory;
        //		mQCOSource = "QCO";
        //	}
        //	//2019-11-14 Tai Le (Thomas): Run the CAPACITY Distribution
        //	//this.CalculateCapaAll(OracleConnectionString);
        //	string _tempCAPAMsg = "";
        //	this.CalculateCAPA(Factory, QCOYear, weekNum, true, out _tempCAPAMsg);
        //	sb.AppendLine($"PASSED:  CalculateCAPA();");
        //	if (pCopyToRunningQCO)
        //	{
        //		CopySysQCOtoSIMQCO(Factory, QCOYear, strWeekNum);
        //		sb.AppendLine($"PASSED:  CopySysQCOtoSIMQCO();");
        //	}
        //	if (!String.IsNullOrEmpty(mEnviroment))
        //		if (mEnviroment.ToLower() == "console")
        //			Console.WriteLine("QCO under " + Factory + " finished:" + strErrorMessage);
        //	return JsonConvert.SerializeObject(new { retResult = !blHasError, retData = "", retMsg = strErrorMessage });
        //}
        //private void UpdateQCORanking_2(Qcops item, int origQCORANKING, string Reason)
        //{
        //    using (OracleConnection OracleConn = new OracleConnection(OPS_Utils.ConstantGeneric.ConnectionStrMes))
        //    {
        //        OracleConn.Open();
        //        var strSQL =
        //      " UPDATE PKMES.T_QC_QUEUE " +
        //      " SET CHANGEQCORANK = :QCORANKNEW , " +
        //      " CHANGEBY = :CHANGEBY , " +
        //      " CHANGEON = :CHANGEON , " +
        //      " REASON = :REASON " +
        //      " WHERE QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO AND QCORANK= :QCORANK ";
        //        OracleCommand oracleCommand = new OracleCommand(strSQL, OracleConn);
        //        oracleCommand.Parameters.Add("QCORANKNEW", origQCORANKING);
        //        oracleCommand.Parameters.Add("CHANGEBY", UserInf.UserName);
        //        oracleCommand.Parameters.Add("CHANGEON", DateTime.Now);
        //        oracleCommand.Parameters.Add("REASON", Reason);
        //        oracleCommand.Parameters.Add("QCOFACTORY", item.QCOFACTORY);
        //        oracleCommand.Parameters.Add("QCOYEAR", item.QCOYEAR);
        //        oracleCommand.Parameters.Add("QCOWEEKNO", item.QCOWEEKNO);
        //        //oracleCommand.Parameters.Add("QCORANK", item.MAXQCORANKING);
        //        oracleCommand.Parameters.Add("QCORANK", item.QCORANK);
        //        oracleCommand.ExecuteNonQuery();
        //        OracleConn.Close();
        //        OracleConn.Dispose();
        //    }
        //}
        //private void UpdateQCORanking_3(Qcops item)
        //{
        //    using (OracleConnection OracleConn = new OracleConnection(OPS_Utils.ConstantGeneric.ConnectionStrMes))
        //    {
        //        OracleConn.Open();
        //        var strSQL =
        //          " SELECT * FROM PKMES.T_QC_QUEUE " +
        //          " WHERE QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO " +
        //          " ORDER BY NVL(CHANGEQCORANK, QCORANK) ";
        //        OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, OracleConn);
        //        oracleDataAdapter.SelectCommand.Parameters.Add("QCOFACTORY", item.QCOFACTORY);
        //        oracleDataAdapter.SelectCommand.Parameters.Add("QCOYEAR", item.QCOYEAR);
        //        oracleDataAdapter.SelectCommand.Parameters.Add("QCOWEEKNO", item.QCOWEEKNO);
        //        DataTable dt = new DataTable();
        //        oracleDataAdapter.Fill(dt);
        //        int intQCORANKINGNEW = 0;
        //        if (dt != null)
        //            foreach (DataRow dr in dt.Rows)
        //            {
        //                intQCORANKINGNEW += 1;
        //                dr["QCORANKINGNEW"] = intQCORANKINGNEW;
        //            }
        //        OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
        //        oracleDataAdapter.Update(dt);
        //        oracleCommandBuilder.Dispose();
        //        if (dt != null)
        //            dt.Dispose();
        //        oracleDataAdapter.Dispose();
        //        OracleConn.Close();
        //        OracleConn.Dispose();
        //    }
        //}
        public void DistributeMaterial_T_QC_QCPMSIM(string vstrFactory, int vintQCOYear, string vstrWeekNo, ref DataTable vdtT_QC_QCPM, ref DataTable vdt_T_QC_QCFP, OracleDataReader vDrMaterialSource, string vType, ref int vintSeqNo)
        {
            try
            {
                /* vType 
					 *  - "W" stand for WMS >> it's Received Qty
					 *  - "K" stand for KMS in Old MRP >> it's ETA Qty
					 *  - "K2" stand for KMS in MRP2 >> it's ETA Qty
					 */
                if (vDrMaterialSource.HasRows)
                {
                    double decAssignQty = 0;
                    bool blInsert = true;
                    while (vDrMaterialSource.Read())
                    {
                        var DOQTY = Convert.ToDouble(vDrMaterialSource["PLAN_DOQTY"].ToString());
                        if (DOQTY <= 0)
                            continue;
                        DateTime dtPRDSDAT = DateTime.Today; //DateTime.ParseExact(PRDSDAT, "yyyyMMdd", new CultureInfo(""));
                        string ETA = "";
                        DateTime dtETA = dtPRDSDAT;
                        if (vType == "K" || vType == "K2")
                        {
                            ETA = vDrMaterialSource["ETA"].ToString();
                            dtETA = DateTime.ParseExact(ETA, "yyyyMMdd", new CultureInfo(""));
                        }
                        string expression = "";
                        if (vType == "K2")
                        {
                            expression = " PRDPKG = '" + vDrMaterialSource["PRDPKG"] + "' " +
                                         " AND ITEMCODE = '" + vDrMaterialSource["ITEM_CD"] + "'  " +
                                         " AND ITEMCOLORSERIAL = '" + vDrMaterialSource["COLOR_SERIAL"] + "' ";
                        }
                        else if (vType == "W2")
                        {
                            expression = " ( AONO = '" + vDrMaterialSource["WO"] + "' ) " +
                                         " AND ( ITEMCODE = '" + vDrMaterialSource["ITEM_CD"] + "' ) " +
                                         " AND ( ITEMCOLORSERIAL = '" + vDrMaterialSource["COLOR_SERIAL"] + "' ) ";
                        }
                        else
                            expression = " ( WONO = '" + vDrMaterialSource["WO"] + "' ) " +
                                         " AND ( ITEMCODE = '" + vDrMaterialSource["ITEM_CD"] + "' ) " +
                                         " AND ( ITEMCOLORSERIAL = '" + vDrMaterialSource["COLOR_SERIAL"] + "' ) ";
                        DataRow[] foundRows = vdt_T_QC_QCFP.Select(expression);
                        foreach (DataRow dr in foundRows)
                        {
                            if (DOQTY > 0)
                            {
                                decAssignQty = Convert.ToDouble(dr["REQUESTQTY"].ToString()) - Convert.ToDouble(dr["ASSIGNEDQTY"].ToString());
                                if (decAssignQty > 0)
                                {
                                    //#Noted: If T_QC_QCPM not Distrbute yet, create new records
                                    DataRow drNew_tmp_T_QC_QCPM = vdtT_QC_QCPM.NewRow();
                                    vintSeqNo = vintSeqNo + 1;
                                    drNew_tmp_T_QC_QCPM["ID"] = dr["FACTORY"] + "-" + PCMGeneralFunctions.GetRight("0000000000000000" + vintSeqNo, 15);
                                    //drNew_tmp_T_QC_QCPM["RANGKING"] = intSeqNo; // dr["RANGKING"];
                                    drNew_tmp_T_QC_QCPM["FACTORY"] = dr["FACTORY"];
                                    //2019-02-28
                                    drNew_tmp_T_QC_QCPM["QCOFACTORY"] = dr["FACTORY"];
                                    drNew_tmp_T_QC_QCPM["QCOYEAR"] = vintQCOYear;
                                    drNew_tmp_T_QC_QCPM["QCOWEEKNO"] = vstrWeekNo;
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
                                    dr["ASSIGNEDQTY"] = Convert.ToDouble(dr["ASSIGNEDQTY"].ToString()) + decAssignQty;
                                    /*2019-11-01 Tai Le (Add) to problem issue at Update_T_QC_QCFP_MaterialReadiness() */
                                    drNew_tmp_T_QC_QCPM["QUANTITY_A"] = 0;
                                    drNew_tmp_T_QC_QCPM["QUANTITY_B"] = 0;
                                    drNew_tmp_T_QC_QCPM["QUANTITY_C"] = 0;
                                    drNew_tmp_T_QC_QCPM["QUANTITY_D"] = 0;
                                    /*2019-11-01 Tai Le (Thomas): specify where data is coming */
                                    drNew_tmp_T_QC_QCPM["SOURCEDATA"] = vType;
                                    if (vType == "W" || vType == "W2")
                                    {
                                        /* When vType = "W" >> it means the RECEIVED QTY
										 *::: ONLY Data for QUANTITY_A   INCLUDED
										 */
                                        //dr["ASSIGNEDQTY"] = Convert.ToDouble(dr["ASSIGNEDQTY"].ToString()) + decAssignQty;
                                        drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty;
                                        drNew_tmp_T_QC_QCPM["QUANTITY_A"] = decAssignQty;
                                        blInsert = true;
                                    }
                                    else if (vType == "K" || vType == "K2")
                                    {
                                        blInsert = false;
                                        /* When vType = "K" >> it means the INCOMING QTY
										*::: ONLY Data for QUANTITY_A   EXCLUDED
										*/
                                        /* 
										* Nếu ETA trong 5 ngày của ngày exec QCO Ranking ; tức  Calc_Date  < ETA < Exec_Date + 5.Days
										*      SHIP_QTY = SHIP_QTY * 50%>> Quantity_B
										* Nếu ETA trong 10 ngày của ngày exec QCO Ranking ; tức  Calc_Date  < ETA < Exec_Date + 10.Days
										*      SHIP_QTY = SHIP_QTY * 30%>> Quantity_C
										* Nếu ETA > 10 ngày của ngày exec QCO Ranking ; tức  Calc_Date + 10.Days < ETA  
										*      SHIP_QTY = SHIP_QTY * 10%>> Quantity_D
										*/
                                        //dr["ASSIGNEDQTY"] = Convert.ToDouble(dr["ASSIGNEDQTY"].ToString()) + decAssignQty;
                                        if (dtETA < dtPRDSDAT)
                                        {
                                            drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = 0;
                                            drNew_tmp_T_QC_QCPM["QUANTITY_A"] = 0;
                                            blInsert = true;
                                        }
                                        else if (dtPRDSDAT < dtETA && dtETA <= dtPRDSDAT.AddDays(5))
                                        {
                                            drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty * 0.5;
                                            //2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column
                                            drNew_tmp_T_QC_QCPM["QUANTITY_B"] = decAssignQty * 0.5;
                                            blInsert = true;
                                        }
                                        else if (dtPRDSDAT.AddDays(5) < dtETA && dtETA <= dtPRDSDAT.AddDays(10))
                                        {
                                            drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty * 0.3;
                                            //2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column
                                            drNew_tmp_T_QC_QCPM["QUANTITY_C"] = decAssignQty * 0.3;
                                            blInsert = true;
                                        }
                                        else
                                        {
                                            drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty * 0.1;
                                            //2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column
                                            drNew_tmp_T_QC_QCPM["QUANTITY_D"] = decAssignQty * 0.1;
                                            blInsert = true;
                                        }
                                    }
                                    else
                                    {
                                        blInsert = false;
                                    }
                                    DOQTY = DOQTY - decAssignQty;
                                    if (blInsert)
                                        vdtT_QC_QCPM.Rows.Add(drNew_tmp_T_QC_QCPM);
                                    if (vintSeqNo % 500 == 0)
                                    {
                                        Debug.Print("Finished " + vintSeqNo + " Material Rows T_QC_QCPM");
                                        if (mEnviroment.ToLower() == "console")
                                            Console.WriteLine("Finished " + vintSeqNo + " Material Rows T_QC_QCPM");
                                    }
                                }
                            }
                        }
                    }
                    Debug.Print("Get out of 'vDrMaterialSource.HasRows' ");
                }
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                if (mEnviroment.ToLower() == "console")
                    Console.WriteLine("ERROR at DistributeMaterial_T_QC_QCPM(): " + Msg);
            }
        }
        #endregion
    }
    //::END class PCMQCOCalculation
    public class FWCP
    {
        /* create Time: 2019-10-01
		 * creator:     Tai Le (Thomas)
		 * Class Name: Factory Weekly Capacity
		 */
        public string FACTORY { get; set; }
        public decimal YEAR { get; set; }
        public decimal WEEKNO { get; set; }
        public decimal TOTALWORKERS { get; set; }
        public decimal CAPACITY { get; set; }
        public decimal TOTALMACHINES { get; set; }
        public DateTime STARTDATE { get; set; }
        public DateTime ENDDATE { get; set; }
        public decimal TOTALSEWER { get; set; }
        public decimal SEWERCAPA { get; set; }
        public string CREATOR { get; set; }
        public DateTime CREATETIME { get; set; }
        public decimal TOTALWORKHOUR { get; set; }
        public FWCP() { }
        protected FWCP(FWCP copy)
        {
            this.FACTORY = copy.FACTORY;
            this.YEAR = copy.YEAR;
            this.WEEKNO = copy.WEEKNO;
            this.TOTALWORKERS = copy.TOTALWORKERS;
            this.CAPACITY = copy.CAPACITY;
            this.TOTALMACHINES = copy.TOTALMACHINES;
            this.STARTDATE = copy.STARTDATE;
            this.ENDDATE = copy.ENDDATE;
            this.TOTALSEWER = copy.TOTALSEWER;
            this.SEWERCAPA = copy.SEWERCAPA;
        }
        public bool Update(string pConnectionString, out string pMessage)
        {
            pMessage = "";
            bool isResult = true;
            string Term = "";
            try
            {
                var strSQL = @" 
                    Select * 
                    From PKMES.T_CM_FWCP 
                    Where FACTORY = :FACTORY
                    AND YEAR = :YEAR 
                    AND  WEEKNO = :WEEKNO 
                ";
                List<OracleParameter> parameters = new List<OracleParameter>();
                parameters.Add(new OracleParameter("FACTORY", FACTORY));
                parameters.Add(new OracleParameter("YEAR", YEAR));
                parameters.Add(new OracleParameter("WEEKNO", WEEKNO));
                using (OracleConnection oracleConn = new OracleConnection(pConnectionString))
                {
                    oracleConn.Open();
                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConn);
                    oracleDataAdapter.SelectCommand.Parameters.AddRange(parameters.ToArray());
                    var _dt = new DataTable();
                    oracleDataAdapter.Fill(_dt);
                    if (_dt != null)
                    {
                        if (_dt.Rows.Count == 0)
                        {
                            Term = "Added";
                            //Add New
                            var _drNew = _dt.NewRow();
                            _drNew["FACTORY"] = FACTORY;
                            _drNew["YEAR"] = YEAR;
                            _drNew["WEEKNO"] = WEEKNO;
                            _drNew["TOTALWORKERS"] = TOTALWORKERS;
                            _drNew["CAPACITY"] = CAPACITY;
                            _drNew["TOTALMACHINES"] = TOTALMACHINES;
                            _drNew["TOTALSEWER"] = TOTALSEWER;
                            _drNew["SEWERCAPA"] = SEWERCAPA;
                            _drNew["STARTDATE"] = STARTDATE;
                            _drNew["ENDDATE"] = ENDDATE;
                            _drNew["CREATOR"] = CREATOR;
                            _drNew["CREATETIME"] = DateTime.Now;
                            _drNew["TOTALWORKHOUR"] = TOTALWORKHOUR; //2019-10-24 Tai Le (Thomas)
                            _dt.Rows.Add(_drNew);
                        }
                        else
                        {
                            Term = "Updated";
                            //Update 
                            var _drNew = _dt.Rows[0];
                            _drNew["TOTALWORKERS"] = TOTALWORKERS;
                            _drNew["CAPACITY"] = CAPACITY;
                            _drNew["TOTALMACHINES"] = TOTALMACHINES;
                            _drNew["TOTALSEWER"] = TOTALSEWER;
                            _drNew["SEWERCAPA"] = SEWERCAPA;
                            _drNew["CREATOR"] = CREATOR;
                            _drNew["CREATETIME"] = DateTime.Now;
                            _drNew["TOTALWORKHOUR"] = TOTALWORKHOUR; //2019-10-24 Tai Le (Thomas)
                            _drNew["CREATOR"] = CREATOR;
                            _drNew["CREATETIME"] = DateTime.Now;
                            ///2020-06-29 Tai Le(Thomas)
                            ///Error happens 2020-W27: wrong the StartDate / End Date
                            _drNew["STARTDATE"] = STARTDATE;
                            _drNew["ENDDATE"] = ENDDATE;
                        }
                        OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                        oracleDataAdapter.Update(_dt);
                        oracleCommandBuilder.Dispose();
                        _dt.Dispose();
                        oracleDataAdapter.Dispose();
                    }
                    oracleConn.Close();
                }
                pMessage = $"Weekly Capacity of Factory[{FACTORY}], Year[{ YEAR}], WeekNo[{ WEEKNO }] {Term}.";
                //return true;
            }
            catch (Exception ex)
            {
                pMessage = "Factory[" + FACTORY + "], Year[" + YEAR + "], WeekNo[" + WEEKNO + "] FAIL: " + ex.Message;
                isResult = false;
            }
            return isResult;
        }
        public static string WeekNoToString(int pWeekNo)
        {
            if (pWeekNo < 10)
                return "0" + pWeekNo.ToString();
            else
                return pWeekNo.ToString();
        }
        public static int StringWeekNoToINT(string pWeekNo)
        {
            /* pWeekNo Format as  W01 , ... , W10 
             * or pWeekNo Format as text like 01 , ... , 10  
             */
            if (pWeekNo.Contains("W"))
                return Convert.ToInt32(pWeekNo.Substring(1, pWeekNo.Length - 1));
            else
                return Convert.ToInt32(pWeekNo);
        }
    }
    public class QCOQueue
    {
        public string QCOFACTORY { get; set; }
        public int QCOYEAR { get; set; }
        public string QCOWEEKNO { get; set; }
        public string QCORANK { get; set; }
        public string CHANGEQCORANK { get; set; }
        public string CHANGEBY { get; set; }
        public DateTime CHANGEON { get; set; }
        public string FACTORY { get; set; }
        public string LINENO { get; set; }
        public string AONO { get; set; }
        public string BUYER { get; set; }
        public string STYLECODE { get; set; }
        public string STYLESIZE { get; set; }
        public string STYLECOLORSERIAL { get; set; }
        public string REVNO { get; set; }
        public string PRDPKG { get; set; }
        public DateTime CREATEDATE { get; set; }
        public string NORMALIZEDPERCENT { get; set; }
        public DateTime? LATESTQCOTIME { get; set; } //2019-06-18 Tai Le(Thomas)
        public QCOQueue() { }
    }
    public class Qcfo
    {
        public decimal ID { get; set; }
        public string FACTORY { get; set; }
        public string PARAMETERNAME { get; set; }
        public string DBFIELDNAME { get; set; }
        public decimal SORTINGSEQ { get; set; }
        public string SORTDIRECTION { get; set; }
        public Qcfo() { }
        public Qcfo(string vFACTORY, string vPARAMETERNAME, string vDBFIELDNAME, string vSORTDIRECTION)
        {
            FACTORY = vFACTORY;
            PARAMETERNAME = vPARAMETERNAME;
            DBFIELDNAME = vDBFIELDNAME;
            SORTDIRECTION = vSORTDIRECTION;
        }
    }
    public class FWES
    {
        public string FACTORY { get; set; }
        public int YEAR { get; set; }
        public string WEEKNO { get; set; }
        public decimal EFFICIENCYPERCEN { get; set; }
        public string CONFIRMYN { get; set; }
        public string CONFIRMBY { get; set; }
        public DateTime? CONFIRMDATE { get; set; }
    }
    public class FAOT
    {
        public string FACTORY { get; set; }
        public string ADTYPE { get; set; }
        public string ISACTIVE { get; set; }
        public DateTime? CREATETIME { get; set; }
        public string CREATOR { get; set; }
    }
    public class Fawk
    {
        public string FACTORY { get; set; }
        public string PLANYEAR { get; set; }
        public string PLANMONTH { get; set; }
        public float WORKER { get; set; }
        public float SEWER { get; set; }
        public float DIRECTWORKER { get; set; }
        public float INDIRECTWORKER { get; set; }
    }
    public class Fwts
    {
        public string FACTORY { get; set; }
        public string LINENO { get; set; }
        public string PLANYEAR { get; set; }
        public string PLANMONTH { get; set; }
        public string WEEKNO { get; set; }
        public string PLANDAY { get; set; }
        public float MORNINGTIME { get; set; }
        public float AFTERNOONTIME { get; set; }
        public float OVERTIME { get; set; }
        public DateTime CREATEDATE { get; set; }
        public string CREATEID { get; set; }
        public DateTime UPDATEDATE { get; set; }
        public string UPDATEID { get; set; }
    }
}
