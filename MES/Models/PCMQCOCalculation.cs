using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

using OPS_DAL.QCOEntities;

using Oracle.ManagedDataAccess.Client;

using Newtonsoft.Json;
 

namespace MES.Models
{
    public class PCMQCOCalculation
    {
        public int intNegativeRank;

        public string mFactory { get; set; }
        public string mEnviroment { get; set; }
        public string mUserID { get; set; }


        public PCMQCOCalculation()
        {
            mEnviroment = "";
            mFactory = ""; 
        }

        public PCMQCOCalculation(string Factory)
        {
            mFactory = Factory;
        }

        public void QCOCalculationAll(string OracleConnectionString, string Factory, string UserID, string UserRole, string ServerPath, out string retMessage)
        {
            retMessage = "";

            using (OracleConnection oracleConnection = new OracleConnection(OracleConnectionString))
            {
                oracleConnection.Open();

                List<PCMQCOCalculation> Factories = new List<PCMQCOCalculation>();

                var strSQL = "Select FACTORY " +
                             " From PKERP.T_CM_FCMT  " +
                             " Where Type = 'P' " +
                             " And Substr(Factory,1,1) = 'P' " +
                             " And Status ='OK' " +
                             " AND FACTORY like '" + Factory + "' " +
                             " Order By Factory ";

                DataTable dt = new DataTable();
                PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, null, out dt, out strSQL);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)

                        foreach (DataRow dr in dt.Rows)
                        {
                            Factories.Add(new PCMQCOCalculation(dr["FACTORY"].ToString()));

                            //QCOCalculation(OracleConnectionString, dr["FACTORY"].ToString(), UserID, UserRole, ServerPath, out retMessage);
                            retMessage = "";
                        }
                    dt.Dispose();
                }

                //QCOCalculation(OracleConnectionString, Factory, Session, Server, out retMessage);

                oracleConnection.Close();
                oracleConnection.Dispose();

                for (int I = 0; I < Factories.Count; I++)
                {
                    if (mEnviroment.ToLower() == "console")
                    {
                        Console.WriteLine("QCO in " + Factories[I].mFactory);
                        Console.WriteLine("================================");
                        Console.WriteLine("");
                    }
                    QCOCalculation(OracleConnectionString, Factories[I].mFactory, UserID, UserRole, ServerPath, false, "", "", "", "", "", "", out retMessage);
                    retMessage = "";
                }

            }
        }

        public string QCOCalculation(string OracleConnectionString, string Factory, string UserID, string UserRole, string ServerPath, bool IsSinglePP, string pAONO, string pStyleCode, string pStyleSize, string pStyleColorSerial, string pRevNo, string pPRDPKD_ID, out string retMessage)
        {
            retMessage = "";

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

            /* Prevent RUN QCO for Factory in case input "pAONO, pStyleCode, pStyleSize, pStyleColorSerial, pRevNo, pPRDPKD_ID" but IsSinglePP = FALSE */
            if (!String.IsNullOrEmpty(pAONO) || !String.IsNullOrEmpty(pStyleCode) || !String.IsNullOrEmpty(pStyleSize) || !String.IsNullOrEmpty(pStyleColorSerial) || !String.IsNullOrEmpty(pRevNo) || !String.IsNullOrEmpty(pPRDPKD_ID))
                IsSinglePP = true;


            //string strResult = "";
            string strSQL = "",
                strErrorMessage = "",
                strSQLWhere = "",
                strSQLWhereWO = ""
                ;

            bool blHasError = false;
            bool blUpdateQCOJobStatus = true;

            //var lcSession = Session; //HttpContext.Current.Session;

            //::: Get WeekNo
            DateTime dtStarDateTime = DateTime.Now.AddHours(36);
            //2019-06-14 Tai Le (Thomas) : Handle Single PP Material Readiness

            //2019-06-14 Tai Le (Thomas): use  intYear to Replace "dtStarDateTime.Year", in this way, able to re-use for SinglePackage
            int QCOYear = dtStarDateTime.Year;

            if (!String.IsNullOrEmpty(mEnviroment))
                if (mEnviroment.ToLower() == "console")
                    Console.WriteLine("dtStarDateTime= " + dtStarDateTime.ToString("s"));

            CultureInfo cul = CultureInfo.CurrentCulture;
            int weekNum = cul.Calendar.GetWeekOfYear(dtStarDateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            string strWeekNum = "W" + PCMGeneralFunctions.GetRight("00" + weekNum, 2);

             
            //var path = Path.Combine(Server.MapPath("~/File/QCOCalculationLog"), Factory + "-" + QCOYear + "-" + strWeekNum + ".txt"); 
            //var path = Path.Combine(ServerPath, Factory + "-" + QCOYear + "-" + strWeekNum + ".txt"); 
            //if (!File.Exists(path)) 
            //    log = new StreamWriter(path); 
            //else 
            //    log = File.AppendText(path); 
             
            if (UserRole == null)
            {
                strErrorMessage = "Can not find User Role to Calculate QCO Ranking.";
                //retMessage = Factory + ": " + strErrorMessage; 

                goto HE_Exit_QCOCalculate;
            }
             
            if (UserRole != "5000" && IsSinglePP == false)
            {
                strErrorMessage = "Wrong Role to Calculate QCO Ranking. Please log in with Role 5000.";
                //retMessage = Factory + ": " + strErrorMessage; 

                goto HE_Exit_QCOCalculate;
            }

            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(OracleConnectionString))
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

                                blHasError = true;
                            }

                            dt.Dispose();
                        }

                    if (blHasError && IsSinglePP == false)
                    {
                        //strResult = JsonConvert.SerializeObject(new { retResult = !blHasError, retData = "", retMsg = strErrorMessage });
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
                        //strResult = JsonConvert.SerializeObject(new { retResult = !blHasError, retData = "", retMsg = strErrorMessage });
                        goto HE_QCOCalculate_Complete;
                    }



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
                            strErrorMessage = "QCO On Factory [" + Factory + "] in Year [" + QCOYear + "] at Week No [" + strWeekNum + "] already EXIST." +
                                              "<BR/>After Saturday 12:00 PM, QCO Calculation is Ready for Next Week No.";
                            blHasError = true;
                            dt.Dispose();
                        }

                    if (blHasError && IsSinglePP == false)
                    {
                        //strResult = JsonConvert.SerializeObject(new { retResult = !blHasError, retData = "", retMsg = strErrorMessage });
                        goto HE_QCOCalculate_Complete;
                    }


                    /*::Processing
                     * 1. From View  "PKERP.VIEW_ERP_PSRSNP_PLAN", look for all the INCOMPLETE Packages
                     * 2. Save the satisfied Package into PKMES.T_QC_QCFP
                     * 3. Sort the PKMES.T_QC_QCFP based on The Factory Sorting Parameters on table PKMES.T_00_QCFO
                     * 4. Based on PKMES.T_QC_QCFP , PKERP.T_SD_BOMT ,  PKERP.V_WMS_PORC , Distribute the Received Qty for Each Package
                     *      4.1. PKMES.T_QC_QCFP  Join  PKERP.T_SD_BOMT together, and order by "Factory Sorting Parameters" , T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL
                     */

                    if (mEnviroment.ToLower() == "console")
                        Console.WriteLine("Start Running QCO For Factory= " + Factory);

                    //Clean the temporary physical Tables { PKMES.T_QC_QCFP ; PKMES.T_QC_QCPM ; PKMES.T_QC_QCPS } 
                    OracleCommand command = new OracleCommand("DELETE PKMES.T_QC_QCFP WHERE FACTORY = :FACTORY AND QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO", oracleConnection);
                    command.Parameters.Add(new OracleParameter("FACTORY", Factory));
                    command.Parameters.Add(new OracleParameter("QCOFACTORY", Factory));
                    command.Parameters.Add(new OracleParameter("QCOYEAR", QCOYear));
                    command.Parameters.Add(new OracleParameter("QCOWEEKNO", strWeekNum));
                    if (IsSinglePP == false)
                        command.ExecuteNonQuery();
                    command.Dispose();


                    command = new OracleCommand("DELETE PKMES.T_QC_QCPM  WHERE FACTORY = :FACTORY AND QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO ", oracleConnection);
                    command.Parameters.Add(new OracleParameter("FACTORY", Factory));
                    command.Parameters.Add(new OracleParameter("QCOFACTORY", Factory));
                    command.Parameters.Add(new OracleParameter("QCOYEAR", QCOYear));
                    command.Parameters.Add(new OracleParameter("QCOWEEKNO", strWeekNum));
                    if (IsSinglePP == false)
                        command.ExecuteNonQuery();
                    command.Dispose();

                    command = new OracleCommand("DELETE PKMES.T_QC_QCPS WHERE FACTORY = :FACTORY AND QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO ", oracleConnection);
                    command.Parameters.Add(new OracleParameter("FACTORY", Factory));
                    command.Parameters.Add(new OracleParameter("QCOFACTORY", Factory));
                    command.Parameters.Add(new OracleParameter("QCOYEAR", QCOYear));
                    command.Parameters.Add(new OracleParameter("QCOWEEKNO", strWeekNum));
                    if (IsSinglePP == false)
                        command.ExecuteNonQuery();
                    command.Dispose();
                     
                    //::: Insert the Flag to mark the Running QCO Factory
                    if (IsSinglePP == false)
                        Insert_T_QC_QCFR(OracleConnectionString, Factory, UserID, "Executing"); 


                    //::: Get Factory QCO Setting:
                    List<Qcfo> LcAllFactoryParameters = null;
                    List<Qcfo> LcNoMateialFactoryParameters = null;
                    FactoryHasMaterialParameter(OracleConnectionString, Factory, out LcAllFactoryParameters, out LcNoMateialFactoryParameters); 


                    //::: Get MTOPS Package From  Chosen Factory 
                    if (IsSinglePP == false)
                        dt = GetMTOPSPackage(OracleConnectionString, Factory); 


                    if (dt == null || dt.Rows.Count == 0)
                    {
                        strErrorMessage = "No AO-MTOPS Package Found Factory [" + Factory + "].";
                        blHasError = true;
                    }

                    if (blHasError && IsSinglePP == false)
                    {
                        //strResult = JsonConvert.SerializeObject(new { retResult = !blHasError, retData = "", retMsg = strErrorMessage });
                        goto HE_QCOCalculate_Complete;
                    }


                    //::: Sort MTOPS Package with Parameter Before Material Readiness
                    if (IsSinglePP == false)
                        Sort_T_QC_QCFP(ref dt, LcNoMateialFactoryParameters, "First"); 


                    //::: Save Sorted PP Into PKMES.T_QC_QCFP
                    if (IsSinglePP == false)
                        Save_T_QC_QCFP(OracleConnectionString, Factory, dtStarDateTime, QCOYear, strWeekNum, dt);

                    if (dt != null)
                    {
                        dt.Clear();
                        dt.Dispose();
                    } 


                    //2019-06-14 Tai Le (Thomas): Handle Update Material Readiness For Single MTOPS Production Package 
                    if (IsSinglePP)
                    {
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

                    //::: Distribute the Material into MTOPS Package of PKMES.T_QC_QCFP
                    //::: Get PP & T_SD_BOMT
                    strSQL = " SELECT ROW_NUMBER() OVER(PARTITION BY T_QC_QCFP.FACTORY ORDER BY T_QC_QCFP.FACTORY, T_QC_QCFP.DELIVERYDATE , T_QC_QCFP.ORDQTY ,  T_QC_QCFP.PLANQTY , " +
                             " T_QC_QCFP.AONO , T_QC_QCFP.STYLECODE , T_QC_QCFP.STYLESIZE , T_QC_QCFP.STYLECOLORSERIAL , T_QC_QCFP.REVNO , T_QC_QCFP.PRDPKG ) AS RowSeqNo , " +
                             " LEAD(T_QC_QCFP.ID, 1, '') OVER (ORDER BY T_QC_QCFP.ID) AS NEXT_ID , " +
                             " T_QC_QCFP.* , V_MRP_PP_WO.WONO , " +
                             " T_SD_BOMT.MAINITEMCODE , T_SD_BOMT.MAINITEMCOLORSERIAL , " +
                             " T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL , " +
                             " T_QC_QCFP.PLANQTY * T_SD_BOMT.UNITCONSUMPTION AS REQUESTQTY , " +
                             " LEAD(T_QC_QCFP.ID, 1, 0) OVER (ORDER BY T_QC_QCFP.ID) AS NEXT_ID " +
                             " FROM PKMES.T_QC_QCFP " +
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
                             " INNER JOIN PKERP.T_SD_BOMT ON " +
                             "      T_QC_QCFP.STYLECODE = T_SD_BOMT.STYLECODE " +
                             "      AND T_QC_QCFP.STYLESIZE = T_SD_BOMT.STYLESIZE " +
                             "      AND T_QC_QCFP.STYLECOLORSERIAL = T_SD_BOMT.STYLECOLORSERIAL " +
                             "      AND T_QC_QCFP.REVNO = T_SD_BOMT.REVNO " +
                             " WHERE " +
                             " T_QC_QCFP.QCOFACTORY = '" + Factory + "'  " +
                             " AND T_QC_QCFP.QCOYEAR = " + QCOYear + " " +
                             " AND T_QC_QCFP.QCOWEEKNO = '" + strWeekNum + "'  " +
                             " AND (T_SD_BOMT.ITEMCODE NOT LIKE 'PKG%' OR T_SD_BOMT.ITEMCODE NOT LIKE 'TRE%' )  ";
                    if (IsSinglePP)
                        strSQL += " AND T_QC_QCFP.AONO || T_QC_QCFP.FACTORY || T_QC_QCFP.STYLECODE || T_QC_QCFP.STYLESIZE || T_QC_QCFP.STYLECOLORSERIAL || T_QC_QCFP.REVNO || T_QC_QCFP.PRDPKG IN " + strSQLWhere;

                    strSQL += " ORDER BY T_QC_QCFP.ID , T_SD_BOMT.ITEMCODE , T_SD_BOMT.ITEMCOLORSERIAL ";

                    DataTable dt_QCFP = new DataTable();
                    PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, new List<OracleParameter> { new OracleParameter("FACTORY", Factory) }, out dt_QCFP, out strSQL);

                    //Add column ASSIGNEDQTY for Distribution Purpose.
                    DataColumn newColumn = new DataColumn("ASSIGNEDQTY", typeof(System.Double)) { DefaultValue = 0.0 };
                    dt_QCFP.Columns.Add(newColumn);

                    //::: Open T_QC_QCPM to Save the Material Distribution
                    //::: 2019-04-04: Tai Le (THOMAS) Add  QCOFACTORY, QCOYEAR, QCOWEEKNO To WHERE Syntax
                    //strSQL = " SELECT * " +
                    //         " FROM PKMES.T_QC_QCPM " +
                    //         " WHERE FACTORY = '" + Factory + "' ";

                    if (IsSinglePP == true)
                    {
                        //Delete PKMES.T_QC_QCPM From Related Packages in same WONO
                        strSQL =
                            " DELETE PKMES.T_QC_QCPM " +
                            " WHERE AONO || FACTORY || STYLECODE || STYLESIZE || STYLECOLORSERIAL || REVNO || PRDPKG IN " + strSQLWhere;
                        OracleCommand oracleCommand = new OracleCommand(strSQL, oracleConnection);
                        oracleCommand.CommandTimeout = 90;
                        oracleCommand.ExecuteNonQuery();
                    }

                    strSQL = " SELECT * " +
                             " FROM PKMES.T_QC_QCPM " +
                             " WHERE FACTORY = '" + Factory + "' " +
                             " AND QCOFACTORY = '" + Factory + "' " +
                             " AND QCOYEAR = " + QCOYear + " " +
                             " AND QCOWEEKNO = '" + strWeekNum + "' " +
                             "";
                    if (IsSinglePP == true)
                        strSQL += " AND AONO || FACTORY || STYLECODE || STYLESIZE || STYLECOLORSERIAL || REVNO || PRDPKG IN " + strSQLWhere;

                    DataTable dt_T_QC_QCPM = new DataTable();
                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
                    oracleDataAdapter.Fill(dt_T_QC_QCPM);

                    //::: DISTRIBUTE MATERIAL
                    int intSeqNo = 0;
                    //::: Distribute WMS Qty (Received Material Qty) 
                    strSQL =
                        " SELECT * " +
                        " FROM PKERP.V_WO_RECWMS " +
                        " WHERE WO >= " +
                        "   (SELECT MIN (SUBSTR(V_MRP_PP_WO.WONO,1,2))  " +
                        "    FROM PKMES.T_QC_QCFP " +
                        "    INNER JOIN PKMES.V_MRP_PP_WO ON  " +
                        "       T_QC_QCFP.FACTORY = V_MRP_PP_WO.FACTORY " +
                        "       AND T_QC_QCFP.AONO = V_MRP_PP_WO.AONO " +
                        "       AND T_QC_QCFP.STYLECODE = V_MRP_PP_WO.STLCD " +
                        "       AND T_QC_QCFP.STYLESIZE = V_MRP_PP_WO.STLSIZ " +
                        "       AND T_QC_QCFP.STYLECOLORSERIAL = V_MRP_PP_WO.STLCOSN " +
                        "       AND T_QC_QCFP.REVNO = V_MRP_PP_WO.STLREVN " +
                        "       AND T_QC_QCFP.PRDPKG = V_MRP_PP_WO.PRODPACKAGE " +
                        "    WHERE T_QC_QCFP.QCOFACTORY = '" + Factory + "' AND T_QC_QCFP.QCOYEAR = " + QCOYear + " AND T_QC_QCFP.QCOWEEKNO = '" + strWeekNum + "' ";
                    if (IsSinglePP)
                        strSQL +=
                            " AND T_QC_QCFP.AONO || T_QC_QCFP.FACTORY || T_QC_QCFP.STYLECODE || T_QC_QCFP.STYLESIZE || T_QC_QCFP.STYLECOLORSERIAL || T_QC_QCFP.REVNO || T_QC_QCFP.PRDPKG IN " + strSQLWhere;
                    strSQL += "   ) ";

                    if (IsSinglePP)
                        strSQL += " AND WO IN " + strSQLWhereWO;

                    strSQL +=
                        " ORDER BY WO , ITEM_CD , COLOR_SERIAL , PLAN_DOQTY ";
                    command = new OracleCommand(strSQL, oracleConnection);
                    var dr_WMS = command.ExecuteReader();
                    DistributeMaterial_T_QC_QCPM(Factory, QCOYear, strWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, dr_WMS, "W", ref intSeqNo);
                    dr_WMS.Close();
                    dr_WMS.Dispose();
                    command.Dispose(); 


                    //::: Update the Material Readiness back to dt_QCFP based on dt_T_QC_QCPM  { QUANTITY_A ; REQUESTQTY }
                    Update_T_QC_QCFP_MaterialReadiness(ref dt_QCFP, dt_T_QC_QCPM);
                    //::: Sort dt_QCFP with LcAllFactoryParameters
                    //Sort_T_QC_QCFP(ref dt_QCFP, LcAllFactoryParameters, "All");


                    //::: Distribute KMS Qty (Incoming Qty)
                    //Get the Monday based on Year and WeekNo 
                    //2019-06-15
                    if (IsSinglePP)
                        weekNum = cul.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                    DateTime dtMonday = PCMGeneralFunctions.GetDateFromWeekNumberAndDayOfWeek(QCOYear, weekNum, 0);

                    strSQL = " SELECT WO , ITEM_CD , COLOR_SERIAL , ETA , SUM(SHIP_QTY) PLAN_DOQTY " +
                             " FROM KMS_PSRSHP_TBL@AOMTOPS " +
                             " WHERE DELFLG = 'N' " +
                             " AND ETA IS NOT NULL " +
                             " AND Length(ETA) = 8 " +
                             " AND ETA >= '" + dtMonday.ToString("yyyyMMdd") + "' " +
                             " GROUP BY WO , ITEM_CD , COLOR_SERIAL , ETA  ";
                    command = new OracleCommand(strSQL, oracleConnection);
                    var dr_KMS = command.ExecuteReader();
                    DistributeMaterial_T_QC_QCPM(Factory, QCOYear, strWeekNum, ref dt_T_QC_QCPM, ref dt_QCFP, dr_KMS, "K", ref intSeqNo);
                    dr_KMS.Close();
                    dr_KMS.Dispose();
                    command.Dispose(); 


                    //::: Save T_QC_QCPM
                    OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                    oracleDataAdapter.Update(dt_T_QC_QCPM);
                    oracleCommandBuilder.Dispose(); 


                    //::: Update the Material Readiness back to dt_QCFP based on dt_T_QC_QCPM  { QUANTITY_A ; REQUESTQTY }
                    Update_T_QC_QCFP_MaterialReadiness(ref dt_QCFP, dt_T_QC_QCPM);
                    dt_T_QC_QCPM.Dispose();
                    oracleDataAdapter.Dispose(); 


                    //::: Sort dt_QCFP with LcAllFactoryParameters
                    if (IsSinglePP == false)
                        Sort_T_QC_QCFP(ref dt_QCFP, LcAllFactoryParameters, "All"); 


                    //::: SAVE dt_T_QC_QUEUE
                    if (IsSinglePP == false)
                        Save_T_QC_QUEUE(OracleConnectionString, Factory, dtStarDateTime, QCOYear, strWeekNum, dt_QCFP);
                    else
                        Update_T_QC_QUEUE(OracleConnectionString, Factory, QCOYear, strWeekNum, dt_QCFP);
                    dt_QCFP.Dispose();

                    //Update QCORANKINGNEW  
                    command = new OracleCommand("Update PKMES.T_QC_QUEUE SET QCORANKINGNEW = ROWNUM WHERE QCOFACTORY = :QCOFACTORY AND QCOYEAR = :QCOYEAR AND QCOWEEKNO = :QCOWEEKNO ", oracleConnection);
                    command.Parameters.Add(new OracleParameter("QCOFACTORY", Factory));
                    command.Parameters.Add(new OracleParameter("QCOYEAR", QCOYear));
                    command.Parameters.Add(new OracleParameter("QCOWEEKNO", strWeekNum));
                    if (IsSinglePP == false)
                        command.ExecuteNonQuery();
                    command.Dispose();

                HE_QCOCalculate_Complete:
                    oracleConnection.Close();
                    oracleConnection.Dispose();
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
                Complete_T_QC_QCFR(OracleConnectionString, Factory, QCOYear, strWeekNum, UserID, strErrorMessage, blHasError); 
            }

            if (!blHasError)
            {
                strErrorMessage = "QCO Ranking For Factory[" + Factory + "]; Year[" + QCOYear + "]; WONo [" + strWeekNum + "] : Built Success."; 
            } 
            return JsonConvert.SerializeObject(new { retResult = !blHasError, retData = "", retMsg = strErrorMessage });
        }

        public void Insert_T_QC_QCFR(string mstrOracleCnnString, string vstrFactory, string vstrCurrentUserID, string executing)
        {
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(mstrOracleCnnString))
                {
                    oracleConnection.Open();

                    //int intI = 0;
                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter("SELECT * FROM PKMES.T_QC_QCFR WHERE FACTORY = '" + vstrFactory + "'  ", oracleConnection);
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

        public bool FactoryHasMaterialParameter(string mstrOracleCnnString, string vstrFactory, out List<Qcfo> lsAllFactoryParameters, out List<Qcfo> lsNoMaterialFactoryParameters)
        {
            lsAllFactoryParameters = null;
            lsNoMaterialFactoryParameters = null;
            bool blHasMaterialQtyPara = false;

            using (OracleConnection oracleConnection = new OracleConnection(mstrOracleCnnString))
            {
                oracleConnection.Open();

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

                        if (dr["PARAMETERNAME"].ToString().ToUpper() == "MATERIAL READINESS")
                        {
                            blHasMaterialQtyPara = true;
                        }

                        if (!blHasMaterialQtyPara)
                            lsTempNoMaterialFactoryParameters.Add(new Qcfo(dr["FACTORY"].ToString(), dr["PARAMETERNAME"].ToString(), dr["DBFIELDNAME"].ToString(), dr["SORTDIRECTION"].ToString()));

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

        public DataTable GetMTOPSPackage(string OracleConnectionString, string vstrFactory)
        {
            DataTable dt = new DataTable();

            using (OracleConnection oracleConnection = new OracleConnection(OracleConnectionString))
            {
                oracleConnection.Open();

                string strSQL =
                    " SELECT ROW_NUMBER() OVER(ORDER BY VIEW_ERP_PSRSNP_PLAN.PRDPKG) as RowSeq, " +
                    " VIEW_ERP_PSRSNP_PLAN.*   " +
                    " FROM PKERP.VIEW_ERP_PSRSNP_PLAN  " +
                    " LEFT JOIN PKERP.V_AO_PPDP ON " +
                    "      VIEW_ERP_PSRSNP_PLAN.FACTORY = V_AO_PPDP.FACTORY " +
                    "      AND VIEW_ERP_PSRSNP_PLAN.AONO = V_AO_PPDP.AONO " +
                    "      AND VIEW_ERP_PSRSNP_PLAN.STYLECODE = V_AO_PPDP.STYLECODE " +
                    "      AND VIEW_ERP_PSRSNP_PLAN.STYLESIZE = V_AO_PPDP.STYLESIZE " +
                    "      AND VIEW_ERP_PSRSNP_PLAN.STYLECOLORSERIAL = V_AO_PPDP.STYLECOLORSERIAL " +
                    "      AND VIEW_ERP_PSRSNP_PLAN.REVNO = V_AO_PPDP.REVNO " +
                    "      AND VIEW_ERP_PSRSNP_PLAN.PRDPKG = V_AO_PPDP.PRDPKG " +
                    " WHERE VIEW_ERP_PSRSNP_PLAN.FACTORY = :FACTORY " +
                    " AND VIEW_ERP_PSRSNP_PLAN.STATUS NOT IN ( '**' , 'AC', 'F-' , 'GD' , 'PS' , 'R-' , 'WC' ) " +
                    " AND VIEW_ERP_PSRSNP_PLAN.FACCLOSE = 'N' " +
                    " AND NVL(VIEW_ERP_PSRSNP_PLAN.PLANQTY,0) - NVL(V_AO_PPDP.PRDQTY,0) > 0 " +
                    " ";

                //strSQL = strSQL + " AND VIEW_ERP_PSRSNP_PLAN.PRDSDAT <= '20190101'  ";

                PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, new List<OracleParameter> { new OracleParameter("FACTORY", vstrFactory) }, out dt, out strSQL);

                oracleConnection.Close();
                oracleConnection.Dispose();
            }

            return dt;
        }

        public void Sort_T_QC_QCFP(ref DataTable vdt_QCFP, List<Qcfo> vQCOFactorySortingParameters, string vType)
        {
            try
            {
                var dv = vdt_QCFP.DefaultView;
                string strSorting = "";

                foreach (var objNonMateialFactoryParameters in vQCOFactorySortingParameters)
                {
                    if (String.IsNullOrEmpty(strSorting))
                        strSorting = objNonMateialFactoryParameters.DBFIELDNAME + " " + objNonMateialFactoryParameters.SORTDIRECTION;
                    else
                        strSorting = strSorting + ", " + objNonMateialFactoryParameters.DBFIELDNAME + " " + objNonMateialFactoryParameters.SORTDIRECTION;
                }

                //::: Extra Sorting Parameter Since has T_SD_BOMT 
                if (vType == "All")
                    strSorting = strSorting + ", ITEMCODE , ITEMCOLORSERIAL ";

                dv.Sort = strSorting;
                vdt_QCFP = dv.ToTable();
            }
            catch (Exception Ex)
            {
                var Msg = Ex.Message;
            }
        }

        public bool Save_T_QC_QCFP(string mstrOracleCnnString, string vstrQCOFactory, DateTime vdtStartTime, int vintQCOYear, string vstrWeekNo, DataTable vdt_T_QC_QCFP)
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
                            if (DateTime.Parse(dr["DELIVERYDATE"].ToString()) < vdtStartTime)
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
                            drNew_T_QC_QCFP["DELIVERYDATE"] = dr["DELIVERYDATE"];
                            drNew_T_QC_QCFP["PRDSDAT"] = dr["PRDSDAT"];
                            drNew_T_QC_QCFP["PRDEDAT"] = dr["PRDEDAT"];

                            /*2019-04-22 Tai Le(Thomas): Add 1 Original Data from AOMTOPS Package {ORDQTY} */
                            drNew_T_QC_QCFP["ORDQTY"] = dr["ORDQTY"];

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

        public void DistributeMaterial_T_QC_QCPM(string vstrFactory, int vintQCOYear, string vstrWeekNo, ref DataTable vdtT_QC_QCPM, ref DataTable vdt_T_QC_QCFP, OracleDataReader vDrMaterialSource, string vType, ref int vintSeqNo)
        {
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

                    while (vDrMaterialSource.Read())
                    {
                        var DOQTY = Convert.ToDouble(vDrMaterialSource["PLAN_DOQTY"].ToString());

                        if (DOQTY <= 0)
                            continue;

                        DateTime dtPRDSDAT = DateTime.Today; //DateTime.ParseExact(PRDSDAT, "yyyyMMdd", new CultureInfo(""));

                        string ETA = "";
                        DateTime dtETA = dtPRDSDAT;
                        if (vType == "K")
                        {
                            ETA = vDrMaterialSource["ETA"].ToString();
                            dtETA = DateTime.ParseExact(ETA, "yyyyMMdd", new CultureInfo(""));
                        }

                        string expression = " WONO = '" + vDrMaterialSource["WO"] + "' " +
                                            " AND ITEMCODE = '" + vDrMaterialSource["ITEM_CD"] + "'  " +
                                            " AND ITEMCOLORSERIAL = '" + vDrMaterialSource["COLOR_SERIAL"] + "' ";

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

                                    if (vType == "W")
                                    {
                                        /* When vType = "W" >> it means the RECEIVED QTY
                                         *::: ONLY Data for QUANTITY_A   INCLUDED
                                         */

                                        dr["ASSIGNEDQTY"] = Convert.ToDouble(dr["ASSIGNEDQTY"].ToString()) + decAssignQty;

                                        drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty;
                                        drNew_tmp_T_QC_QCPM["QUANTITY_A"] = decAssignQty;

                                        blInsert = true;
                                    }
                                    else if (vType == "K")
                                    {
                                        /* When vType = "K" >> it means the INCOMING QTY
                                         *::: ONLY Data for QUANTITY_A   EXCLUDED
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

                                        dr["ASSIGNEDQTY"] = Convert.ToDouble(dr["ASSIGNEDQTY"].ToString()) + decAssignQty;

                                        if (dtETA < dtPRDSDAT)
                                        {
                                            drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = 0;
                                            drNew_tmp_T_QC_QCPM["QUANTITY_A"] = 0;
                                            blInsert = true;

                                        }
                                        else if (dtPRDSDAT < dtETA && dtETA >= dtPRDSDAT.AddDays(5))
                                        {
                                            drNew_tmp_T_QC_QCPM["PLANQUANTITY"] = decAssignQty * 0.5;

                                            //2018-12-17 Tai Le Huu (Thomas) seperate the Qty into 3 column
                                            drNew_tmp_T_QC_QCPM["QUANTITY_B"] = decAssignQty * 0.5;
                                            blInsert = true;
                                        }
                                        else if (dtPRDSDAT.AddDays(5) < dtETA && dtETA >= dtPRDSDAT.AddDays(10))
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
            catch (Exception ex)
            {
                var Msg = ex.Message;

                if (mEnviroment.ToLower() == "console")
                    Console.WriteLine("ERROR at DistributeMaterial_T_QC_QCPM(): " + Msg);
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
                    Debug.Print("Updating Material Readiness For: " +
                                "AONo[" + dr_QCFP["AONO"] + "] ;  " +
                                "StyleCode[" + dr_QCFP["STYLECODE"] + "] ; " +
                                "StyleSize[" + dr_QCFP["STYLESIZE"] + "] ; " +
                                "StyleColorSerial[" + dr_QCFP["STYLECOLORSERIAL"] + "]; " +
                                "RevNo[" + dr_QCFP["REVNO"] + "]; " +
                                "PrdPkg[" + dr_QCFP["PRDPKG"] + "]; " +
                                "ItemCode[" + dr_QCFP["ITEMCODE"] + "]; " +
                                "ItemColorSerial[" + dr_QCFP["ITEMCOLORSERIAL"] + "] ; " +
                                "dbRequestQty= " + dr_QCFP["REQUESTQTY"]);

                    ID = dr_QCFP["ID"].ToString();
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

                    DataRow[] FoundRows_T_QC_QCPM = vdt_T_QC_QCPM.Select(expression);

                    foreach (DataRow dr_FoundRow_T_QC_QCPM in FoundRows_T_QC_QCPM)
                    {
                        if (dr_FoundRow_T_QC_QCPM["QUANTITY_A"] != null)
                            dbReceivedQty = dbReceivedQty + Convert.ToDouble(dr_FoundRow_T_QC_QCPM["QUANTITY_A"].ToString());
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
                        //Debug.Print("Updated Material Readiness For: AONo[" + AONo + "] ;  StyleCode[" + StyleCode + "] ; StyleSize[" + StyleSize + "] ; StyleColorSerial[" + StyleColorSerial + "]; RevNo[" + RevNo + "]; PrdPkg[" + PrdPkg + "] , Material_Readiness= " + Math.Truncate(MaterialReadiness * 100) / 100);
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

                        //Reset the value
                        dbRequestQty = 0;
                        dbReceivedQty = 0;
                        Debug.Print("dbRequestQty= " + dbRequestQty);
                        Debug.Print("dbReceivedQty= " + dbReceivedQty);

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

        public bool Update_T_QC_QUEUE(string mstrOracleCnnString, string vstrQCOFactory, int vintQCOYear, string vstrWeekNo, DataTable vdt_T_QC_QCFP)
        {
            int intI = 0;
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(mstrOracleCnnString))
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
                            if (dr["MATNORNALRATE"].ToString().Length > 0)

                            {
                                OracleCommand oracleCommand = new OracleCommand("" +
                                    " UPDATE PKMES.T_QC_QUEUE " +
                                    " SET NORMALIZEDPERCENT = :NORMALIZEDPERCENT ," +
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

        public bool Save_T_QC_QUEUE(string mstrOracleCnnString, string vstrQCOFactory, DateTime vdtStarDateTime, int vintQCOYear, string vstrWeekNo, DataTable vdt_T_QC_QCFP)
        {
            int intI = 0,
                intQCORANKINGNEW = 0;
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(mstrOracleCnnString))
                {
                    oracleConnection.Open();

                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter("SELECT * FROM PKMES.T_QC_QUEUE WHERE QCOFACTORY = '" + vstrQCOFactory + "' AND QCOYEAR = " + vintQCOYear + " AND QCOWEEKNo = '" + vstrWeekNo + "'  ", oracleConnection);
                    DataTable dt_T_QC_QUEUE = new DataTable();
                    oracleDataAdapter.Fill(dt_T_QC_QUEUE);

                    //int intRowCount = -1 * vdt_T_QC_QCFP.Rows.Count;

                    foreach (DataRow dr in vdt_T_QC_QCFP.Rows)
                    {
                        if (dr["MATNORNALRATE"] != null)
                            if (dr["MATNORNALRATE"].ToString().Length > 0)
                                if (Convert.ToDouble(dr["MATNORNALRATE"].ToString()) > 0)
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
                                    drNew_T_QC_QCFP["DELIVERYDATE"] = dr["DELIVERYDATE"];
                                    drNew_T_QC_QCFP["PRDSDAT"] = dr["PRDSDAT"];
                                    drNew_T_QC_QCFP["PRDEDAT"] = dr["PRDEDAT"];
                                    drNew_T_QC_QCFP["ORDQTY"] = dr["ORDQTY"];

                                    drNew_T_QC_QCFP["NORMALIZEDPERCENT"] = dr["MATNORNALRATE"];

                                    drNew_T_QC_QCFP["CREATEDATE"] = DateTime.Now;
                                    drNew_T_QC_QCFP["LATESTQCOTIME"] = DateTime.Now; //2019-06-18 Tai Le (Thomas)

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
                return false;
            }
        }

        public void Complete_T_QC_QCFR(string mstrOracleCnnString, string vstrQCOFactory, int vintQCOYear, string vstrWeekNo, string vstrCurrentUserID, string vstrResult, bool blHasError)
        {
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(mstrOracleCnnString))
                {
                    oracleConnection.Open();

                    //int intI = 0;
                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter("SELECT * FROM PKMES.T_QC_QCFR WHERE FACTORY = '" + vstrQCOFactory + "' AND STATUS = 'RUNNING' ", oracleConnection);
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

        //public object SaveQCPSRanking(string data, string Reason)
        //{
        //    //    string strMsg = "";
        //    //    bool blResult = false;

        //    //    //if (Session["LoginRole"].ToString() != "5000")
        //    //    if (false)
        //    //    {
        //    //        strMsg = "Not Authorized.<BR/>Please login with Role \"5000 - Production Director\" to proceed this Function.";
        //    //        goto HE_END;
        //    //    }

        //    //    try
        //    //    {
        //    //        //var Qcops = new JavaScriptSerializer().Deserialize<List<Qcops>>(data);

        //    //        var Qcops = JsonConvert.DeserializeObject<Qcops>(data);

        //    //        var Check_ = from item in Qcops where item.QCORANK < 0 select item;

        //    //        if (Check_.Count > 0)
        //    //        {
        //    //            strMsg = "Please DON'T Include Negative QCO_RANK.<BR/>Use \"QCO Display\" to Filter Positive Ranking.";
        //    //            goto HE_END;
        //    //        }

        //    //        if (Qcops.Count > 0)
        //    //        {
        //    //            List<Qcops> HandleList = new List<Qcops>();

        //    //            int intNewRanking = 0;
        //    //            var minQCORANKINGNEW = Qcops.Min(x => x.QCORANKINGNEW);
        //    //            var maxQCORANKINGNEW = Qcops.Max(x => x.QCORANKINGNEW);


        //    //            var minRANKING = Qcops.Min(x => x.RANKING);
        //    //            var maxRANKING = Qcops.Max(x => x.RANKING);

        //    //            int MAXQCORANKING = T_QC_QUEUE_MAXQCORANK(Qcops[0].QCOFACTORY, Qcops[0].QCOYEAR, Qcops[0].QCOWEEKNO);

        //    //            intNewRanking = minRANKING - 1;

        //    //            foreach (Qcops item in Qcops)
        //    //            {
        //    //                intNewRanking += 1;
        //    //                item.intNewRanking = intNewRanking;

        //    //                if (intNewRanking != item.RANKING)
        //    //                {
        //    //                    MAXQCORANKING += 1;

        //    //                    item.MAXQCORANKING = MAXQCORANKING;

        //    //                    HandleList.Add(item);
        //    //                }
        //    //            }

        //    //            foreach (Qcops item in HandleList)
        //    //            {
        //    //                /// Step 1: Update QCORanking = MAXQCORANKING  WHere QCORANKING = QCORANKINGNEW
        //    //                /// Retired
        //    //                UpdateQCORanking_1(item);
        //    //            }

        //    //            foreach (Qcops item in HandleList)
        //    //            {
        //    //                /// Step 2: Update QCORanking = intNewRanking ,
        //    //                ///             ChangeBy , ChangeOn
        //    //                ///             Reason
        //    //                ///             Where QCORANKING = MAXQCORANKING Debug.Print("intNewRanking= " + item.intNewRanking + "; QCORANKINGNEW = " + item.QCORANKINGNEW);
        //    //                var origPP = Qcops.Where(f => f.RANKING == item.intNewRanking).Select(n => n.QCORANK).First();

        //    //                UpdateQCORanking_2(item, origPP, Reason);
        //    //            }

        //    //            /// Step 3: 
        //    //            /// Update QCORANKINGNEW Based On Is_NULL(CHANGEQCORANK, QCORANK), 
        //    //            /// QCORANKINGNEW starts from 1 to n 
        //    //            UpdateQCORanking_3(HandleList[0]);

        //    //            blResult = true;
        //    //            strMsg = "Change-over Ranking Saved!";
        //    //        }
        //    //        else
        //    //        {
        //    //            strMsg = "No Data on 'T_QC_QCPS' to create QCO!";
        //    //        }
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        strMsg = ex.Message;
        //    //    }

        //    //HE_END:
        //    return new { retResult = "", retMsg = "" };
        //}

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
         
        #region Retired Functions
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
        #endregion


        #region Distribute Factory CAPA to QC_QUEUE
        public void CalculateCapaAll(string pConnectionStringMES)
        {
            List<PCMQCOCalculation> Factories = new List<PCMQCOCalculation>();
            using (OracleConnection oracleConnection = new OracleConnection(pConnectionStringMES))
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

                oracleConnection.Close();
            }

            string Msg = "";
            DateTime dtStarDateTime = DateTime.Now.AddHours(36);
            //Determinate the YEAR / WeekNo  
            int QCOYear = dtStarDateTime.Year;

            if (!String.IsNullOrEmpty(mEnviroment))
                if (mEnviroment.ToLower() == "console")
                    Console.WriteLine("dtStarDateTime= " + dtStarDateTime.ToString("s"));

            CultureInfo cul = CultureInfo.CurrentCulture;
            int weekNum = cul.Calendar.GetWeekOfYear(dtStarDateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            //string strWeekNum = "W" + PCMGeneralFunctions.GetRight("00" + weekNum, 2);

            for (int I = 0; I < Factories.Count; I++)
            {
                if (mEnviroment.ToLower() == "console")
                {
                    Console.Write("QCO-Capa in Factory: " + Factories[I].mFactory);
                }

                Msg = "";
                CalculateCAPA(pConnectionStringMES, Factories[I].mFactory, QCOYear, weekNum, true, out Msg);

                if (mEnviroment.ToLower() == "console")
                {
                    Console.WriteLine("QCO-Capa in Factory: " + Factories[I].mFactory + ": DONE");
                    Console.WriteLine("================================");
                    Console.WriteLine("");
                }
            } 
            Factories.Clear();
        }

        public bool CalculateCAPA(string pConnectionStringMES, string pFactory, int pYear, int pWeekNo, bool pIncludeNegativeRank, out string pMessage)
        {
            pMessage = "";
            StringBuilder sb = new StringBuilder();
            DateTime _dtMain = DateTime.Now;

            int I = 0, J = 0, currentJ = 0;


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
                    " ORDER BY QCORANKINGNEW ";

                if (pIncludeNegativeRank)
                    strSQL =
                    " SELECT * " +
                    " FROM PKMES.T_QC_QUEUE " +
                    " WHERE T_QC_QUEUE.QCOFACTORY = :QCOFACTORY " +
                    " AND T_QC_QUEUE.QCOYEAR = :QCOYEAR  " +
                    " AND T_QC_QUEUE.QCOWEEKNO = :QCOWEEKNO  " +
                    " ORDER BY QCORANKINGNEW ";

                List<OracleParameter> parameters = new List<OracleParameter>();
                parameters.Add(new OracleParameter("QCOFACTORY", pFactory.Length == 0 ? mFactory : pFactory));
                parameters.Add(new OracleParameter("QCOYEAR", pYear));
                parameters.Add(new OracleParameter("QCOWEEKNO", "W" + PCMGeneralFunctions.GetRight("00" + pWeekNo.ToString(), 2)));


                OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, pConnectionStringMES);
                oracleDataAdapter.SelectCommand.Parameters.AddRange(parameters.ToArray());

                var _dt_T_QC_QUEUE = new DataTable();
                oracleDataAdapter.Fill(_dt_T_QC_QUEUE);

                if (_dt_T_QC_QUEUE.Rows.Count > 0)
                {

                    var StartDate = new DateTime();
                    var EndDate = new DateTime();
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
                    Dictionary<string, int> StyleDailyRate = new Dictionary<string, int>();
                    Dictionary<string, decimal> StyleTAKETIME = new Dictionary<string, decimal>();
                    Dictionary<string, int> StyleOPTIME = new Dictionary<string, int>();

                    Dictionary<string, double> FactoryLineDailyUsageHour = new Dictionary<string, double>();
                    //Dictionary<string, int> FactoryLineHandleCAPA = new Dictionary<string, int>();


                    bool blUsageFull = false;
                    //bool blUsageFullTT = false;

                    decimal EfficiencyRate = 0;

                    for (I = pWeekNo; I <= 52; I++)
                    {  //Loop for 100 Weeks 
                        blUsageFull = false;
                        //blUsageFullTT = false;

                        var ProcessWeek = I;

                        var objFactoryCAPA = GetFactoryCAPA(pConnectionStringMES, pFactory, pYear, ProcessWeek);
                        if (objFactoryCAPA != null)
                        {
                            StartDate = objFactoryCAPA.STARTDATE.AddHours(7);
                            EndDate = objFactoryCAPA.ENDDATE.AddHours(7);

                            FactoryCAPA = objFactoryCAPA.CAPACITY;
                            //FactoryCAPATT = FactoryCAPA;
                        }
                        else
                        {
                            continue;
                        }

                        var objFactoryWeeklyEfficiency = GetFactoryEfficiency(pConnectionStringMES, pFactory, pYear, "W" + PCMGeneralFunctions.GetRight("00" + ProcessWeek.ToString(), 2));

                        if (objFactoryWeeklyEfficiency != null)
                            EfficiencyRate = objFactoryWeeklyEfficiency.EFFICIENCYPERCEN;
                        else
                            EfficiencyRate = 50;


                        for (J = currentJ; J < _dt_T_QC_QUEUE.Rows.Count; J++)
                        //for (J = currentJ; J < 10; J++)
                        {
                            currentJ = J;
                            Console.WriteLine("currentJ= " + currentJ);
                            sb.AppendLine("");
                            sb.AppendLine("currentJ= " + currentJ);

                            DataRow _dr_T_QC_QUEUE = _dt_T_QC_QUEUE.Rows[J];
                            //Reset:
                            OccupiedCAPA = 0;
                            //OccupiedCAPATT = 0;

                            //Assign 
                            var LINENO = _dr_T_QC_QUEUE["LINENO"].ToString();

                            var STYLECODE = _dr_T_QC_QUEUE["STYLECODE"].ToString();
                            var STYLESIZE = _dr_T_QC_QUEUE["STYLESIZE"].ToString();
                            var STYLECOLORSERIAL = _dr_T_QC_QUEUE["STYLECOLORSERIAL"].ToString();
                            var REVNO = _dr_T_QC_QUEUE["REVNO"].ToString();
                            var PackageQty = (decimal)_dr_T_QC_QUEUE["PLANQTY"];

                            sb.AppendLine("PRDPKG= " + _dr_T_QC_QUEUE["PRDPKG"].ToString() + ", PLANQTY= " + _dr_T_QC_QUEUE["PLANQTY"].ToString());

                            var KEY = STYLECODE.ToString() + STYLESIZE.ToString() + STYLECOLORSERIAL.ToString() + REVNO.ToString() + pFactory;

                            #region GET STYLE OPTIME , STYLE DAILY RATE , STYLE MANCOUNT
                            if (!OPTIMES.ContainsKey(KEY))
                            {
                                //use Dictionary to prevent get the existing OPTIME from DB
                                Msg = "";
                                if (GetStyleOPTIME(pConnectionStringMES, STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, pFactory, out Msg))
                                {
                                    /* [Msg] contain the OPTIME Data of Input Style
                                     * Format:  OPTIME + ";" + DAILYTARGET + ";" + MANCOUNT + ";" + TAKETIME
                                     */
                                    OPTIMES.Add(KEY, Msg);

                                    StyleOPTIME.Add(KEY, Convert.ToInt32(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[0]));
                                    StyleDailyRate.Add(KEY, Convert.ToInt32(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[1]));
                                    StyleManCount.Add(KEY, Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[2]));
                                    StyleTAKETIME.Add(KEY, Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[3]));


                                    StyleOpTime = Convert.ToInt32(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                                    var OPTIME_StyleDailyRate = Convert.ToInt32(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[1]);
                                    //From PKERP.T_OP_OPTIME, DAILY TARGET use 7.5 as standard
                                    StyleOPHourlyRate = Math.Floor((decimal)OPTIME_StyleDailyRate / (decimal)7.5);
                                    StyleOPManCount = Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[2]);
                                    StyleOPTAKETIME = Convert.ToDecimal(Msg.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)[3]);

                                    #region For DEBUGGING
                                    //Console.WriteLine("KEY= " + STYLECODE.ToString() + ";" + STYLESIZE.ToString() + ";" + STYLECOLORSERIAL.ToString() + ";" + REVNO.ToString() + ";" + pFactory);
                                    //Console.WriteLine("OPTIME_StyleDailyRate= " + OPTIME_StyleDailyRate);
                                    //Console.WriteLine("StyleOPHourlyRate= " + StyleOPHourlyRate);
                                    //Console.WriteLine("StyleOPManCount= " + StyleOPManCount);

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
                                //Console.WriteLine("FROM EXISTING DICTIONARY");
                                //Console.WriteLine("KEY= " + STYLECODE.ToString() + ";" + STYLESIZE.ToString() + ";" + STYLECOLORSERIAL.ToString() + ";" + REVNO.ToString() + ";" + pFactory);
                                //Console.WriteLine("OPTIME_StyleDailyRate= " + OPTIME_StyleDailyRate);
                                //Console.WriteLine("StyleOPHourlyRate= " + StyleOPHourlyRate);
                                //Console.WriteLine("StyleOPManCount= " + StyleOPManCount);

                                sb.AppendLine("FROM EXISTING DICTIONARY");
                                sb.AppendLine("KEY= " + STYLECODE.ToString() + ";" + STYLESIZE.ToString() + ";" + STYLECOLORSERIAL.ToString() + ";" + REVNO.ToString() + ";" + pFactory);
                                sb.AppendLine("OPTIME_StyleDailyRate= " + OPTIME_StyleDailyRate);
                                sb.AppendLine("StyleOPHourlyRate= " + StyleOPHourlyRate);
                                sb.AppendLine("StyleOPManCount= " + StyleOPManCount);
                                #endregion
                            }

                            System.Diagnostics.Debug.Print("J=" + J);
                            System.Diagnostics.Debug.Print("");


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

                            _dr_T_QC_QUEUE["OPTIMEHOURLY"] = (decimal)StyleOpTime / 3600; //Unit is Hour 

                            ProductionTime = Math.Round(PackageQty * (decimal)(StyleOpTime / 3600), 2, MidpointRounding.AwayFromZero); //Unit is Hour 
                            OccupiedCAPA = ProductionTime * (2 - EfficiencyRate * (decimal)0.01);

                            #region For DEBUGGER 
                            //Console.WriteLine("ProductionTime= " + ProductionTime);
                            //Console.WriteLine("OccupiedCAPA= " + OccupiedCAPA);
                            //sb.AppendLine("ProductionTime= " + ProductionTime);
                            //sb.AppendLine("OccupiedCAPA= " + OccupiedCAPA);
                            #endregion

                            _dr_T_QC_QUEUE["TAKETIME"] = StyleOPTAKETIME;  
                            //_dr_T_QC_QUEUE["TAKETIMEHOURLY"] = (decimal)(3600.00 / StyleOPTAKETIME); //Number of Bag/Hour

                            #region For DEBUGGER
                            //Console.WriteLine("TAKETIME= " + (decimal)3600.00 / StyleOPTAKETIME);
                            //Console.WriteLine("TAKETIME in Second= " + StyleOPTAKETIME);
                            //sb.AppendLine("TAKETIME= " + (decimal)3600.00 / StyleOPTAKETIME);
                            //sb.AppendLine("TAKETIME in Second= " + StyleOPTAKETIME);
                            #endregion

                            //ProductionTimeTT = Convert.ToDecimal(PackageQty / ((decimal)(3600.00 / StyleOPTAKETIME)));
                            //OccupiedCAPATT = Math.Round(ProductionTimeTT * StyleOPManCount, 2, MidpointRounding.AwayFromZero); 

                            #region For DEBUGGER
                            //Console.WriteLine("ProductionTimeTT= " + ProductionTimeTT);
                            //Console.WriteLine("OccupiedCAPATT= " + OccupiedCAPATT);
                            //sb.AppendLine("ProductionTimeTT= " + ProductionTimeTT);
                            //sb.AppendLine("OccupiedCAPATT= " + OccupiedCAPATT);
                            #endregion

                            #region TBD_Part
                            //_dr_T_QC_QUEUE["PKGOPTIME"] = ProductionTime;
                            //_dr_T_QC_QUEUE["PKGMANCOUNT"] = 0;
                            #endregion

                            if (FactoryCAPA > OccupiedCAPA)
                            {
                                _dr_T_QC_QUEUE["BEGINCAPA"] = FactoryCAPA;
                                _dr_T_QC_QUEUE["USAGECAPA"] = OccupiedCAPA;

                                FactoryCAPA = FactoryCAPA - OccupiedCAPA; //Deduct the Factory Capacity 
                                _dr_T_QC_QUEUE["BALANCECAPA"] = FactoryCAPA;

                                _dr_T_QC_QUEUE["WEEKCAPA"] = pYear.ToString() + " / W" + ProcessWeek.ToString();
                                _dr_T_QC_QUEUE["EFFICIENCY"] = EfficiencyRate;

                                //2019-10-16 Tai Le (Thomas)
                                _dr_T_QC_QUEUE["CAPAALLOCATEBY"] = mUserID;
                                _dr_T_QC_QUEUE["CAPAALLOCATEON"] = _dtMain;

                                //2019-10-25 
                                _dr_T_QC_QUEUE["WEEKWORKHOUR"] = objFactoryCAPA.TOTALWORKHOUR; 
                            }
                            else
                                blUsageFull = true;

                            //if (FactoryCAPATT > OccupiedCAPATT)
                            //{
                            //    _dr_T_QC_QUEUE["BEGINCAPATT"] = FactoryCAPATT;
                            //    _dr_T_QC_QUEUE["USAGECAPATT"] = OccupiedCAPATT; 
                            //    FactoryCAPATT = FactoryCAPATT - OccupiedCAPATT; //Deduct the Factory Capacity 
                            //    _dr_T_QC_QUEUE["BALANCECAPATT"] = FactoryCAPATT; 
                            //}
                            //else blUsageFullTT = true;

                            if (blUsageFull)
                                break;

                        }//Loop to next QCO RANKING. 
                    } //Loop to next WEEK NO.

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
                pMessage = "Error On Factory:" + pFactory + ", Year:" + pYear.ToString() + ", Week:" + pWeekNo.ToString() + " I= ["+I+"] ; J= ["+J+"] ; currentJ= ["+ currentJ + "] ; Description:" + ex.Message;
                Console.WriteLine(pMessage);
                return false;
            }
        }

        public static bool DistributeCAPA(string pConnectionString, string pFactory, int pYear, int pWeekNo, bool pIncludeNegativeRank, out string pMessage)
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
                    var objFactoryWeeklyEfficiency = GetFactoryEfficiency(pConnectionString, pFactory, pYear, "W" + PCMGeneralFunctions.GetRight("00" + ProcessWeek.ToString(), 2));
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

                        var DailyWorkingHours = GetDailyWorkingHours(pConnectionString, pFactory, StartDate.ToString("yyyyMMdd"));

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
                                if (GetStyleOPTIME(pConnectionString, STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, pFactory, out Msg))
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

        private static int GetDailyWorkingHours(string pConnectionString, string pFactory, string pDate)
        {
            /* 
             * pDate has Format "yyyyMMdd"
             */
            int WorkingTime = 0;
            using (OracleConnection oracleConn = new OracleConnection(pConnectionString))
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

        public static bool GetStyleOPTIME(string pConnectionString, string pStyleCode, string pStyleSize, string pStyleColorSerial, string pRevNo, string pFactory, out string pMsg)
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

                using (OracleConnection oracleConn = new OracleConnection(pConnectionString))
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

        public static FWCP GetFactoryCAPA(string pConnectionString, string pFactory, int pYear, int pWeekNo)
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

        public static FWES GetFactoryEfficiency(string pConnectionString, string pFactory, int pYear, string pWeekNo)
        {
            try
            {
                var strSQL =
                    " Select * " +
                    " From PKMES.T_CM_FWES " +
                    " Where FACTORY=:FACTORY " +
                    " And YEAR = :YEAR " +
                    " And WEEKNO = :WEEKNO ";
                var Msg = "";
                List<OracleParameter> parameters = new List<OracleParameter>();
                parameters.Add(new OracleParameter("FACTORY", pFactory));
                parameters.Add(new OracleParameter("YEAR", pYear));
                parameters.Add(new OracleParameter("WEEKNO", pWeekNo));

                return PCMOracleLibrary.QueryToOneObject<FWES>(pConnectionString, strSQL, parameters.ToArray(), out Msg);
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                return null;
            }
        }
        #endregion
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


    //public class Qcfo
    //{
    //    public decimal ID { get; set; }
    //    public string FACTORY { get; set; }
    //    public string PARAMETERNAME { get; set; }
    //    public string DBFIELDNAME { get; set; }
    //    public decimal SORTINGSEQ { get; set; }
    //    public string SORTDIRECTION { get; set; } 
    //    public Qcfo() { }
    //    public Qcfo(string vFACTORY, string vPARAMETERNAME, string vDBFIELDNAME, string vSORTDIRECTION)
    //    {
    //        FACTORY = vFACTORY;
    //        PARAMETERNAME = vPARAMETERNAME;
    //        DBFIELDNAME = vDBFIELDNAME;
    //        SORTDIRECTION = vSORTDIRECTION;
    //    }
    //}
     
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


    //public class FWESBus
    //{
    //    public static bool SaveFWES(string pConnString, FWES objFWES)
    //    {
    //        if (objFWES == null)
    //            return false;

    //        try
    //        {
    //            var strSQL =
    //                " SELECT * " +
    //                " FROM PKMES.T_CM_FWES " +
    //                " WHERE FACTORY = :FACTORY " +
    //                " AND YEAR = :YEAR " +
    //                " AND WEEKNO = :WEEKNO ";

    //            List<OracleParameter> parameters = new List<OracleParameter>();
    //            parameters.Add(new OracleParameter("FACTORY", objFWES.FACTORY));
    //            parameters.Add(new OracleParameter("YEAR", objFWES.YEAR));
    //            parameters.Add(new OracleParameter("WEEKNO", WeekToString(objFWES.WEEKNO)));

    //            OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, pConnString);
    //            oracleDataAdapter.SelectCommand.Parameters.AddRange(parameters.ToArray());

    //            DataTable dt = new DataTable();
    //            oracleDataAdapter.Fill(dt);

    //            if (dt != null)
    //            {
    //                if (dt.Rows.Count > 0)
    //                {
    //                    var dr = dt.Rows[0];

    //                    if (dr["CONFIRMYN"].ToString() == "N")
    //                        dr["EFFICIENCYPERCEN"] = objFWES.EFFICIENCYPERCEN;
    //                }
    //                else
    //                {
    //                    var drNew = dt.NewRow();

    //                    drNew["FACTORY"] = objFWES.FACTORY;
    //                    drNew["YEAR"] = objFWES.YEAR;
    //                    drNew["WEEKNO"] = WeekToString(objFWES.WEEKNO);
    //                    drNew["EFFICIENCYPERCEN"] = objFWES.EFFICIENCYPERCEN;

    //                    drNew["CONFIRMYN"] = "N";

    //                    dt.Rows.Add(drNew);
    //                }
    //            }

    //            //Update back to DB
    //            OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
    //            oracleDataAdapter.Update(dt);
    //            //dt.AcceptChanges();
    //            oracleCommandBuilder.Dispose();


    //            //Clear objects to prevent Memory Leak
    //            parameters.Clear();
    //            dt.Dispose();
    //            oracleDataAdapter.Dispose();

    //            return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            var Msg = ex.Message;
    //            return false;
    //        }
    //    }
    //}


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
    }
     
    public class PCMOracleLibrary
    {
        public static string WeekToString(string pWeekNo)
        {
            if (Convert.ToInt32(pWeekNo) < 10)
                return "W0" + Convert.ToInt32(pWeekNo).ToString();
            else
                return "W" + Convert.ToInt32(pWeekNo).ToString();
        }
        public static OracleConnection GetOracleConnection(string vstrConnectionString)
        {
            OracleConnection connection = new OracleConnection(vstrConnectionString);
            connection.Open();

            return connection;
        }

        public static bool StatementExecution(OracleConnection vOracleConn, string vstrQuery, List<OracleParameter> vParameters, out string vstrErrorMsg, int vCommandTimeout = 60)
        {
            if (vstrQuery.Trim() == "")
            {
                vstrErrorMsg = "Statement is blank";
                return false;
            }

            try
            {
                OracleCommand oracleCommand = new OracleCommand(vstrQuery, vOracleConn)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = Math.Max(vCommandTimeout, 60)
                };

                if (vParameters != null)
                {
                    foreach (var item in vParameters)
                    {
                        oracleCommand.Parameters.Add(item);
                    }
                }

                oracleCommand.ExecuteNonQuery();
                oracleCommand.Dispose();

                vstrErrorMsg = "";
                return true;
            }
            catch (Exception e)
            {
                vstrErrorMsg = e.Message + " " + vstrQuery;
                return false;
            }
        }

        public static bool StatementExecution(string vConnectionString, string vstrQuery, List<OracleParameter> vParameters, out string vstrErrorMsg, int vCommandTimeout = 60)
        {
            if (vstrQuery.Trim() == "")
            {
                vstrErrorMsg = "Statement is blank";
                return false;
            }

            try
            {
                using (OracleConnection oracleConn = new OracleConnection(vConnectionString))
                {
                    oracleConn.Open();
                    OracleCommand oracleCommand = new OracleCommand(vstrQuery, oracleConn)
                    {
                        CommandType = CommandType.Text,
                        CommandTimeout = Math.Max(vCommandTimeout, 60)
                    };

                    if (vParameters != null)
                    {
                        foreach (var item in vParameters)
                        {
                            oracleCommand.Parameters.Add(item);
                        }
                    }

                    oracleCommand.ExecuteNonQuery();
                    oracleCommand.Dispose();
                    oracleConn.Close();
                }

                vstrErrorMsg = "";
                return true;
            }
            catch (Exception e)
            {
                vstrErrorMsg = e.Message + " " + vstrQuery;
                return false;
            }
        }

        public static bool StatementExecution(ref OracleConnection vOracleConn, ref OracleTransaction oracleTransaction, string vstrQuery, List<OracleParameter> vParameters, out string vstrErrorMsg, int vCommandTimeout = 60)
        {
            if (vstrQuery.Trim() == "")
            {
                vstrErrorMsg = "Statement is blank";
                return false;
            }

            try
            {
                OracleCommand oracleCommand = new OracleCommand(vstrQuery, vOracleConn)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = Math.Max(vCommandTimeout, 60),
                    Transaction = oracleTransaction
                };

                if (vParameters != null)
                {
                    foreach (var item in vParameters)
                    {
                        oracleCommand.Parameters.Add(item);
                    }
                }

                oracleCommand.ExecuteNonQuery();
                oracleCommand.Dispose();

                vstrErrorMsg = "";
                return true;
            }
            catch (Exception e)
            {
                vstrErrorMsg = e.Message + " " + vstrQuery;
                return false;
            }
        }

        public static bool StatementToDataTable(OracleConnection vOracleConn, string vstrQuery, List<OracleParameter> vParameters, out DataTable vdtDataTable, out string vstrErrorMsg, int vCommandTimeout = 60)
        {
            if (vstrQuery.Trim() == "")
            {
                vstrErrorMsg = "Statement is blank";
                vdtDataTable = null;
                return false;
            }

            try
            {
                OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(vstrQuery, vOracleConn);
                if (vParameters != null)
                {
                    foreach (var item in vParameters)
                    {
                        oracleDataAdapter.SelectCommand.Parameters.Add(item);
                    }
                }
                oracleDataAdapter.SelectCommand.CommandTimeout = Math.Max(vCommandTimeout, 60);
                DataTable dt = new DataTable();
                dt.CaseSensitive = true;

                oracleDataAdapter.Fill(dt);

                vstrErrorMsg = "";
                vdtDataTable = dt;

                oracleDataAdapter.SelectCommand.Parameters.Clear();
                oracleDataAdapter.Dispose();
                return true;
            }
            catch (Exception e)
            {
                vstrErrorMsg = e.Message;
                vdtDataTable = null;
                return false;
            }
        }

        public static bool StatementToDataSet(OracleConnection vOracleConn, string vstrQuery, List<OracleParameter> vParameters, out DataSet vdsDataSet, out string vstrErrorMsg, int vCommandTimeout = 60)
        {
            if (vstrQuery.Trim() == "")
            {
                vstrErrorMsg = "Statement is blank";
                vdsDataSet = null;
                return false;
            }

            try
            {
                OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(vstrQuery, vOracleConn);
                if (vParameters != null)
                {
                    foreach (var item in vParameters)
                    {
                        oracleDataAdapter.SelectCommand.Parameters.Add(item);
                    }
                }
                oracleDataAdapter.SelectCommand.CommandTimeout = Math.Max(vCommandTimeout, 60);
                DataSet ds = new DataSet();

                oracleDataAdapter.Fill(ds);

                vstrErrorMsg = "";
                vdsDataSet = ds;

                oracleDataAdapter.SelectCommand.Parameters.Clear();
                oracleDataAdapter.Dispose();
                return true;
            }
            catch (Exception e)
            {
                vstrErrorMsg = e.Message;
                vdsDataSet = null;
                return false;
            }
        }

        public static bool StatementDataSet(OracleConnection vOracleConn, string vstrQuery, List<OracleParameter> vParameters, out DataSet vdtDataset, string vstrTableName, out string vstrErrorMsg, int vCommandTimeout = 60)
        {
            if (vstrQuery.Trim() == "")
            {
                vstrErrorMsg = "Statement is blank";
                vdtDataset = null;
                return false;
            }

            try
            {
                OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(vstrQuery, vOracleConn);
                if (vParameters != null)
                {
                    foreach (var item in vParameters)
                    {
                        oracleDataAdapter.SelectCommand.Parameters.Add(item);
                    }
                }
                oracleDataAdapter.SelectCommand.CommandTimeout = Math.Max(vCommandTimeout, 60);
                DataSet ds = new DataSet();
                ds.CaseSensitive = false;

                oracleDataAdapter.Fill(ds, vstrTableName);

                vstrErrorMsg = "";
                vdtDataset = ds;
                return true;
            }
            catch (Exception e)
            {
                vstrErrorMsg = e.Message;
                vdtDataset = null;
                return false;
            }
        }

        public bool UpdateDataTable(OracleConnection vOracleConn, ref OracleDataAdapter vOracleDtAdapter, ref DataTable vdtDataTable)
        {
            try
            {
                OracleCommandBuilder commandBuilder = new OracleCommandBuilder(vOracleDtAdapter);
                vOracleDtAdapter.Update(vdtDataTable);
                commandBuilder.Dispose();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static List<T> QueryToObject<T>(OracleConnection pOracleConn, string pSQL, OracleParameter[] parameters, out string pMessage) where T : class, new()
        {
            pMessage = "";

            try
            {
                var retList = new List<T>();

                using (var command = new OracleCommand(pSQL) { Connection = pOracleConn })
                {
                    command.CommandTimeout = pOracleConn.ConnectionTimeout;
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                    }
                    var dataReader = command.ExecuteReader();
                    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    while (dataReader.Read())
                    {
                        var newObject = new T();
                        for (int i = 0; i < dataReader.FieldCount; i++) //Read All the Columns
                        {
                            string fieldName = dataReader.GetName(i);
                            var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                            if (property == null)
                                continue;
                            if (dataReader[i] != DBNull.Value)
                                property.SetValue(newObject, dataReader[i], null);
                        }
                        retList.Add(newObject);
                    }
                }

                return retList;
            }
            catch (Exception e)
            {
                pMessage = e.Message;
                return null;
            }

        }
        
        public static List<T> QueryToObject<T>(string pConnString, string pSQL, OracleParameter[] parameters, out string pMessage) where T : class, new()
        {
            pMessage = "";

            try
            {
                var retList = new List<T>();

                using (OracleConnection pOracleConn = new OracleConnection(pConnString))
                {
                    pOracleConn.Open();

                    using (var command = new OracleCommand(pSQL) { Connection = pOracleConn })
                    {
                        command.CommandTimeout = pOracleConn.ConnectionTimeout;
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                        }
                        var dataReader = command.ExecuteReader();
                        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                        while (dataReader.Read())
                        {
                            var newObject = new T();
                            for (int i = 0; i < dataReader.FieldCount; i++) //Read All the Columns
                            {
                                string fieldName = dataReader.GetName(i);
                                var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                                if (property == null)
                                    continue;
                                if (dataReader[i] != DBNull.Value)
                                    property.SetValue(newObject, dataReader[i], null);
                            }
                            retList.Add(newObject);
                        }
                    }

                    pOracleConn.Close();
                    pOracleConn.Dispose();
                }

                return retList;
            }
            catch (Exception e)
            {
                pMessage = e.Message;
                return null;
            }

        }

        public static T QueryToOneObject<T>(string pConnString, string pSQL, OracleParameter[] parameters, out string pMessage) where T : class, new()
        {
            pMessage = "";
            string fieldName = "";
            try
            {
                var retList = new List<T>();

                using (OracleConnection pOracleConn = new OracleConnection(pConnString))
                {
                    pOracleConn.Open();

                    using (var command = new OracleCommand(pSQL) { Connection = pOracleConn })
                    {
                        command.CommandTimeout = pOracleConn.ConnectionTimeout;
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                        }
                        var dataReader = command.ExecuteReader();
                        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                        while (dataReader.Read())
                        {
                            var newObject = new T();
                            for (int i = 0; i < dataReader.FieldCount; i++) //Read All the Columns
                            {
                                fieldName = dataReader.GetName(i);
                                var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                                if (property == null)
                                    continue;
                                if (dataReader[i] != DBNull.Value)
                                    property.SetValue(newObject, dataReader[i], null);
                            }
                            retList.Add(newObject);
                        }

                        dataReader.Close();
                        dataReader.Dispose();
                    }

                    pOracleConn.Close();
                    pOracleConn.Dispose();
                }

                return retList.FirstOrDefault();
            }
            catch (Exception e)
            {
                pMessage = "fieldName= " + fieldName + "Error: " + e.Message;
                return null;
            }

        }
         
        public static OracleParameter[] ConvertNullToDbNull(OracleParameter[] parameters)
        {
            if (parameters == null)
                return null;
            foreach (OracleParameter parameter in parameters)
            {
                if (parameter.Value == null)
                    parameter.Value = DBNull.Value;
            }
            return parameters;
        }

        //public static void UpdateDataset(OracleConnection vOracleConn, DataSet vDataset, string vstrTableName = "")
        //{
        //    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter();
        //}
    }


    public class PCMGeneralFunctions
    {
        public static string GetRight(string vstrExpression, int vDigits)
        {
            string mResult = "";

            if (vstrExpression.Length <= vDigits)
            {
                mResult = vstrExpression;
            }
            else
            {
                mResult = vstrExpression.Substring(vstrExpression.Length - vDigits, vDigits);
            }

            return mResult;
        }

        public static string GetLeft(string vstrExpression, int vDigits)
        {
            string mResult = "";

            if (vstrExpression.Length < vDigits)
                vDigits = vstrExpression.Length;

            mResult = vstrExpression.Substring(0, vDigits);

            return mResult;
        }

        public static string GetMid(string vstrExpression, int vStartIndex, int vDigits)
        {
            string mResult = "";

            mResult = vstrExpression.Substring(vStartIndex, vDigits);

            return mResult;
        }
          
        public static DateTime GetDateFromWeekNumberAndDayOfWeek(int year, int weekNumber, int dayOfWeek)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Tuesday - jan1.DayOfWeek;

            DateTime firstMonday = jan1.AddDays(daysOffset);

            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(jan1, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekNumber;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            var result = firstMonday.AddDays(weekNum * 7 + dayOfWeek - 1);
            return result;
        }
         
    }

}