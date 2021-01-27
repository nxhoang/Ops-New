using OPS_DAL.Entities;
using System.Collections.Generic;
using System.Text;
using OPS_DAL.DAL;
using Oracle.ManagedDataAccess.Client;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Data;
using System.Globalization;
using System;

namespace OPS_DAL.Business
{
    public class DormBus
    {
        /// <summary>
        /// Searches the styles.
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="buyerCode">The buyer code.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="searchText">The search text.</param>
        /// <returns></returns>
        public List<Dorm> SearchStyles(int pageIndex, int pageSize, string buyerCode, string start, string end, string searchText, string aoNumber, string searchType)
        {

            searchText = searchText.Trim().ToLower();
            aoNumber = aoNumber.Trim().ToLower();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"WITH OPMT AS(
                SELECT DISTINCT * FROM(
                SELECT DISTINCT T.STYLECODE, T.STYLESIZE, T.STYLECOLORSERIAL, T.REVNO FROM T_OP_OPMT T
                UNION ALL
                SELECT DISTINCT T.STYLECODE, T.STYLESIZE, T.STYLECOLORSERIAL, T.REVNO FROM T_SD_OPMT T
                UNION ALL
                SELECT DISTINCT T.STYLECODE, T.STYLESIZE, T.STYLECOLORSERIAL, T.REVNO FROM T_MT_OPMT T
                UNION ALL
                SELECT DISTINCT T.STYLECODE, T.STYLESIZE, T.STYLECOLORSERIAL, T.REVNO FROM PKMES.T_MX_OPMT T
                )
            ),
             OPMT2 AS(
                   SELECT * FROM OPMT WHERE STYLECOLORSERIAL = '000' 
             )");
            sb.AppendLine("SELECT X.* FROM (");
            sb.AppendLine("SELECT");
            sb.AppendLine(" COUNT(*) OVER() Total,");
            sb.AppendLine(" ROW_NUMBER() OVER(ORDER BY TO_DATE(A.REGISTRYDATE,'YYYY/MM/DD HH24:MI:SS') DESC) R,");
            sb.AppendLine(" A.STYLECODE,");
            sb.AppendLine(" B.STYLENAME,");
            sb.AppendLine(" B.STYLEGROUP, B.SUBGROUP, B.SUBSUBGROUP, B.BUYER,"); //SON ADD
            sb.AppendLine(" A.STYLECOLORSERIAL,");
            sb.AppendLine(" A.STYLESIZE,");
            sb.AppendLine(" D.STYLECOLORSERIAL || ' - ' || D.STYLECOLORWAYS STYLECOLORWAYS,");
            sb.AppendLine(" A.REGISTRYDATE,");
            sb.AppendLine(" A.REVNO,");
            sb.AppendLine(" TO_CHAR(A.AD_CONFIRM,'DD/MM/YYYY') AdConfirm,");
            sb.AppendLine(" TO_CHAR(A.AD_DEV_SALES,'DD/MM/YYYY') AdDevSale ,");
            sb.AppendLine(" CASE WHEN A.AD_CONFIRM IS NOT NULL THEN 'Final Confirmed' ");
            sb.AppendLine(" WHEN A.AD_DEV_SALES IS NOT NULL THEN 'Confirmed' ELSE 'Open' END STATUS,");
            sb.AppendLine(" B.BUYERSTYLECODE,");
            sb.AppendLine(" B.BUYERSTYLENAME,");
            sb.AppendLine(" F.S_CODE BuyerCode,");
            sb.AppendLine(" F.CODE_NAME BuyerName,");
            sb.AppendLine(" E.Name as REGISTER,");
            sb.AppendLine(" CASE WHEN  NVL(OPMT.STYLECODE,OPMT2.STYLECODE ) IS NULL THEN 'NO' ELSE 'YES' END AS HAVE");
            sb.AppendLine(" FROM t_Sd_dorm A");
            sb.AppendLine(" LEFT JOIN T_00_STMT B ON A.STYLECODE = B.STYLECODE");
            sb.AppendLine(" LEFT JOIN T_00_SCMT D ON (A.STYLECODE = D.STYLECODE AND A.STYLECOLORSERIAL = D.STYLECOLORSERIAL)");
            sb.AppendLine(" LEFT JOIN t_cm_usmt E ON A.REGISTER=E.USERID");
            sb.AppendLine(" LEFT JOIN(SELECT * FROM t_cm_mcmt WHERE m_code= 'Buyer' and s_code<> '000') F ON B.BUYER = F.S_CODE");
            sb.AppendLine(@" LEFT JOIN OPMT ON A.STYLECODE = OPMT.STYLECODE AND A.STYLESIZE = OPMT.STYLESIZE 
                                AND A.STYLECOLORSERIAL = OPMT.STYLECOLORSERIAL AND A.REVNO = OPMT.REVNO 
                            LEFT JOIN OPMT2 ON A.STYLECODE = OPMT2.STYLECODE AND A.STYLESIZE = OPMT2.STYLESIZE 
                                AND A.REVNO = OPMT2.REVNO ");
            if (aoNumber != "")
            {
                sb.AppendLine(" LEFT JOIN T_AD_ADSM G ON G.STYLECODE = A.STYLECODE AND G.STYLECOLORSERIAL = A.STYLECOLORSERIAL");
            }
            sb.AppendLine(" WHERE 1=1");

            //START ADD - SON) 7/Oct/2020 - check search type condition
            string searchTypeCon = string.Empty;
            switch (searchType)
            {
                case "1":
                    //OUTSTANDING – Meaning from T_Ad_Adsm if any style package of ao (OPEN) not yet closed can be matched to operation plan
                    searchTypeCon = @" and a.stylecode||a.stylesize||a.stylecolorserial||a.revno in (
                                        select ad.stylecode||ad.stylesize||ad.stylecolorserial||ad. revno from t_ad_adsm ad where ad.status <> 'GD'
                                    )";
                    break;
                case "2":
                    //listed in qco –if selected this, check in QCO TABLE IF CURRRENT WEEK QCO HAS THIS PACKAGE
                    // Gets the Calendar instance associated with a CultureInfo.
                    CultureInfo myCI = new CultureInfo("en-US");
                    Calendar myCal = myCI.Calendar;
                    // Gets the DTFI properties required by GetWeekOfYear.
                    CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
                    DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;

                    var weekNo = "W" + myCal.GetWeekOfYear(DateTime.Now, myCWR, myFirstDOW).ToString("D2");
                    var curYear = DateTime.Now.Year;

                    searchTypeCon = $@" and a.stylecode||a.stylesize||a.stylecolorserial||a.revno in (
                                        select distinct  qco.stylecode || qco.stylesize || qco.stylecolorserial || qco.revno  from pkmes.T_QC_QUEUE qco where qcoyear = '{curYear}' and qcoweekno = '{weekNo}' and NVL(CHANGEQCORANK, QCORANK)  > 0
                                    )";
                    break;
                case "3":
                    //RAISED AO BUT AO STATUS AND HAS PLAN MPS AND NOT CLOSED NOT CANCELLED
                    searchTypeCon = @" and a.stylecode||a.stylesize||a.stylecolorserial||a.revno in (
                                        select ad.stylecode||ad.stylesize||ad.stylecolorserial||ad. revno from t_ad_adsm ad where AD.PD_STR is not null and ad.pd_end is not null and (ad.status <> 'GD' or ad.status <> 'AC')
                                    )";
                    break;
                case "4":
                    //RAISED AO BUT AO STATUS IS NOT YET MPS AND NOT CLOSED NOT CANCELLED
                    searchTypeCon = @" and a.stylecode||a.stylesize||a.stylecolorserial||a.revno in (
                                        select ad.stylecode||ad.stylesize||ad.stylecolorserial||ad. revno from t_ad_adsm ad where AD.PD_STR is null and ad.pd_end is null and (ad.status <> 'GD' or ad.status <> 'AC')
                                    )";
                    break;
                default:
                    //No search type condtion
                    break;
            }

            //Append search type condition to where clause
            sb.AppendLine(searchTypeCon);
            //END ADD - SON) 7/Oct/2020

            if (aoNumber != "")
            {
                sb.Append(" AND LOWER(G.ADNO) = '" + aoNumber + "' ");
            }
            if (buyerCode != "")
                sb.AppendLine(" AND B.BUYER='" + buyerCode + "'");
            if (start != "")
                sb.AppendLine(@" AND TO_DATE(SUBSTR(A.REGISTRYDATE,0,10),'YYYY/MM/DD') >=TO_DATE('" + start + "', 'YYYY/MM/DD') " +
                                "AND TO_DATE(SUBSTR(A.REGISTRYDATE, 0, 10), 'YYYY/MM/DD') <= TO_DATE('" + end + "', 'YYYY/MM/DD') ");
            if (searchText != "")
            {
                sb.AppendLine(" AND (LOWER(A.STYLECODE) LIKE '%" + searchText + "%'");
                sb.AppendLine(" OR LOWER(B.STYLENAME) LIKE '%" + searchText + "%'");
                sb.AppendLine(" OR LOWER(B.BUYERSTYLECODE) LIKE '%" + searchText + "%'");
                sb.AppendLine(" OR LOWER(B.BUYERSTYLENAME) LIKE '%" + searchText + "%')");
            }
            sb.Append(" ) X");
            sb.AppendLine(" WHERE R BETWEEN "+ ((pageIndex - 1) * pageSize + 1)+ " AND " + pageIndex * pageSize);
            sb.AppendLine(" ORDER BY R ");
            return OracleDbManager.GetObjects<Dorm>(sb.ToString(), null);
        }

        /// <summary>
        /// GetNewsStyles
        /// </summary>
        /// <returns></returns>
        /// Author: VitHV
        public List<Dorm> GetNewsStyles()
        {

            string sql = @"SELECT * FROM(
                            SELECT
                                A.STYLECODE
                               ,A.STYLESIZE
                               ,A.STYLECOLORSERIAL
                               ,A.REVNO
                               ,B.STYLENAME
                               ,B.BUYER
                               ,B.STYLEGROUP, B.SUBGROUP, B.SUBSUBGROUP
                               ,D.STYLECOLORSERIAL || ' - ' || D.STYLECOLORWAYS STYLECOLORWAYS
                               ,A.REGISTRYDATE,
                               TO_CHAR(A.AD_CONFIRM,'DD/MM/YYYY') AdConfirm,
                               TO_CHAR(A.AD_DEV_SALES,'DD/MM/YYYY') AdDevSale 
                             FROM t_Sd_dorm A
                             LEFT JOIN T_00_STMT B ON A.STYLECODE = B.STYLECODE
                             LEFT JOIN T_00_SCMT D ON A.STYLECODE = D.STYLECODE AND A.STYLECOLORSERIAL = D.STYLECOLORSERIAL
                             WHERE LENGTH(A.REGISTRYDATE) > 10 
                             ORDER BY A.REGISTRYDATE DESC
                            ) WHERE ROWNUM < 16";
            
            return OracleDbManager.GetObjects<Dorm>(sql, null);
        }

        /// <summary>
        /// Get Dorm information
        /// </summary>
        /// <param name="stlCode"></param>
        /// <param name="stlSize"></param>
        /// <param name="stlColorSerial"></param>
        /// <param name="stlRevNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Dorm GetDormByCode(string stlCode, string stlSize, string stlColorSerial, string stlRevNo)
        {
            var strSql = @"  SELECT * FROM T_SD_DORM WHERE STYLECODE = :P_STYLECODE AND STYLESIZE = :P_STYLESIZE AND STYLECOLORSERIAL = :P_STYLECOLORSERIAL AND REVNO = :P_REVNO ";

            var oracleParams = new List<OracleParameter> {
                new OracleParameter("P_STYLECODE", stlCode),
                new OracleParameter("P_STYLESIZE", stlSize),
                new OracleParameter("P_STYLECOLORSERIAL", stlColorSerial),
                new OracleParameter("P_REVNO", stlRevNo),
            };

            return OracleDbManager.GetObjects<Dorm>(strSql, oracleParams.ToArray()).FirstOrDefault();
        }

        #region MySQL

        /// <summary>
        /// Get DORM from my SQL
        /// </summary>
        /// <param name="stlCode"></param>
        /// <param name="stlSize"></param>
        /// <param name="stlColorSerial"></param>
        /// <param name="stlRevNo"></param>
        /// <returns></returns>
        public static Dorm GetDormMySQL(string stlCode, string stlSize, string stlColorSerial, string stlRevNo)
        {
            var strSql = @"  SELECT * FROM T_SD_DORM WHERE STYLECODE = ?P_STYLECODE AND STYLESIZE = ?P_STYLESIZE AND STYLECOLORSERIAL = ?P_STYLECOLORSERIAL AND REVNO = ?P_REVNO ";

            var parMySql = new List<MySqlParameter> {
                new MySqlParameter("P_STYLECODE", stlCode),
                new MySqlParameter("P_STYLESIZE", stlSize),
                new MySqlParameter("P_STYLECOLORSERIAL", stlColorSerial),
                new MySqlParameter("P_REVNO", stlRevNo),
            };

            return MySqlDBManager.GetAll<Dorm>(strSql, CommandType.Text, parMySql.ToArray()).FirstOrDefault();
        }

        /// <summary>
        /// Insert DROM to MES MySQL
        /// </summary>
        /// <param name="dorm"></param>
        /// <returns></returns>
        public static bool InsertDORMToMESMySql(Dorm dorm)
        {
            string strSql = @"INSERT INTO T_SD_DORM(
                                STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, REGISTER, REGISTRYDATE, PD_BOM_SALES, PD_BOM_DESIGN, PD_BOM_IM, PD_DESIGN
                                , PD_QTYASS, PD_OPLAN_LEAN, PD_OPLAN_PROD, PD_TORDER, PD_PRECOST, AD_BOM_SALES, AD_BOM_DESIGN, AD_BOM_IM, AD_DESIGN, AD_QTYASS
                                , AD_OPLAN_LEAN, AD_OPLAN_PROD, AD_TORDER, AD_PRECOST, FACTORY, REMARKS, PD_DEV_MD, PD_DEV_SALES, PD_BOM_QTYASS, PD_PT_QTYASS
                                , PD_CONFIRM, AD_DEV_MD, AD_DEV_SALES, AD_BOM_QTYASS, AD_PT_QTYASS, AD_CONFIRM, LABORPROC, LABORCOST, FINALREVCHK, `USE`
                                , FRONTVIEW, SIDEVIEW, DESCRIPTION1, DESCRIPTION2, DESCRIPTION3, BOMFILE, FILEDESCRIPTION1, FILEDESCRIPTION2, FILEDESCRIPTION3, SEASONCODE
                                , COLLECNAME, SAMPLESTAGE, MODEL, PATTERN, FUNCTIONS, FUN_ATCH, PICTURE, PDMFILE, DWG_FILE, MDL_FILE
                                , PLX_FILE, PDF_LAYOUT, IGX_FILE, PDF_SOP, CDR_FILE, DXF_FILE, GBR_FILE, MINI_MARKER, CUTTING_FILE, SPEC_FILE
                                , FILE_PLAN, TRIM_FILE, DIFFICULTY, UNITPRODUCTIVITY, LEARNINGCURVE, OFFEROPPRICE, TARGETOPPRICE)
                              VALUES
                                (?P_STYLECODE, ?P_STYLESIZE, ?P_STYLECOLORSERIAL, ?P_REVNO, ?P_REGISTER, ?P_REGISTRYDATE, ?P_PD_BOM_SALES, ?P_PD_BOM_DESIGN, ?P_PD_BOM_IM, ?P_PD_DESIGN
                                , ?P_PD_QTYASS, ?P_PD_OPLAN_LEAN, ?P_PD_OPLAN_PROD, ?P_PD_TORDER, ?P_PD_PRECOST, ?P_AD_BOM_SALES, ?P_AD_BOM_DESIGN, ?P_AD_BOM_IM, ?P_AD_DESIGN, ?P_AD_QTYASS
                                , ?P_AD_OPLAN_LEAN, ?P_AD_OPLAN_PROD, ?P_AD_TORDER, ?P_AD_PRECOST, ?P_FACTORY, ?P_REMARKS, ?P_PD_DEV_MD, ?P_PD_DEV_SALES, ?P_PD_BOM_QTYASS, ?P_PD_PT_QTYASS
                                , ?P_PD_CONFIRM, ?P_AD_DEV_MD, ?P_AD_DEV_SALES, ?P_AD_BOM_QTYASS, ?P_AD_PT_QTYASS, ?P_AD_CONFIRM, ?P_LABORPROC, ?P_LABORCOST, ?P_FINALREVCHK, ?P_USE
                                , ?P_FRONTVIEW, ?P_SIDEVIEW, ?P_DESCRIPTION1, ?P_DESCRIPTION2, ?P_DESCRIPTION3, ?P_BOMFILE, ?P_FILEDESCRIPTION1, ?P_FILEDESCRIPTION2, ?P_FILEDESCRIPTION3, ?P_SEASONCODE
                                , ?P_COLLECNAME, ?P_SAMPLESTAGE, ?P_MODEL, ?P_PATTERN, ?P_FUNCTIONS, ?P_FUN_ATCH, ?P_PICTURE, ?P_PDMFILE, ?P_DWG_FILE, ?P_MDL_FILE
                                , ?P_PLX_FILE, ?P_PDF_LAYOUT, ?P_IGX_FILE, ?P_PDF_SOP, ?P_CDR_FILE, ?P_DXF_FILE, ?P_GBR_FILE, ?P_MINI_MARKER, ?P_CUTTING_FILE, ?P_SPEC_FILE
                                , ?P_FILE_PLAN, ?P_TRIM_FILE, ?P_DIFFICULTY, ?P_UNITPRODUCTIVITY, ?P_LEARNINGCURVE, ?P_OFFEROPPRICE, ?P_TARGETOPPRICE); ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_STYLECODE", dorm.StyleCode),
                new MySqlParameter("P_STYLESIZE", dorm.StyleSize),
                new MySqlParameter("P_STYLECOLORSERIAL", dorm.StyleColorSerial),
                new MySqlParameter("P_REVNO", dorm.RevNo),
                new MySqlParameter("P_REGISTER", dorm.Register),
                new MySqlParameter("P_REGISTRYDATE", dorm.RegistryDate),
                new MySqlParameter("P_PD_BOM_SALES", dorm.PD_BOM_SALES),
                new MySqlParameter("P_PD_BOM_DESIGN", dorm.PD_BOM_DESIGN),
                new MySqlParameter("P_PD_BOM_IM", dorm.PD_BOM_IM),
                new MySqlParameter("P_PD_DESIGN", dorm.PD_DESIGN),
                new MySqlParameter("P_PD_QTYASS", dorm.PD_QTYASS),
                new MySqlParameter("P_PD_OPLAN_LEAN", dorm.PD_OPLAN_LEAN),
                new MySqlParameter("P_PD_OPLAN_PROD", dorm.PD_OPLAN_PROD),
                new MySqlParameter("P_PD_TORDER", dorm.PD_TORDER),
                new MySqlParameter("P_PD_PRECOST", dorm.PD_PRECOST),
                new MySqlParameter("P_AD_BOM_SALES", dorm.AD_BOM_SALES),
                new MySqlParameter("P_AD_BOM_DESIGN", dorm.AD_BOM_DESIGN),
                new MySqlParameter("P_AD_BOM_IM", dorm.AD_BOM_IM),
                new MySqlParameter("P_AD_DESIGN", dorm.AD_DESIGN),
                new MySqlParameter("P_AD_QTYASS", dorm.AD_QTYASS),
                new MySqlParameter("P_AD_OPLAN_LEAN", dorm.AD_OPLAN_LEAN),
                new MySqlParameter("P_AD_OPLAN_PROD", dorm.AD_OPLAN_PROD),
                new MySqlParameter("P_AD_TORDER", dorm.AD_TORDER),
                new MySqlParameter("P_AD_PRECOST", dorm.AD_PRECOST),
                new MySqlParameter("P_FACTORY", dorm.FACTORY),
                new MySqlParameter("P_REMARKS", dorm.REMARKS),
                new MySqlParameter("P_PD_DEV_MD", dorm.PD_DEV_MD),
                new MySqlParameter("P_PD_DEV_SALES", dorm.PD_DEV_SALES),
                new MySqlParameter("P_PD_BOM_QTYASS", dorm.PD_BOM_QTYASS),
                new MySqlParameter("P_PD_PT_QTYASS", dorm.PD_PT_QTYASS),
                new MySqlParameter("P_PD_CONFIRM", dorm.PD_CONFIRM),
                new MySqlParameter("P_AD_DEV_MD", dorm.AD_DEV_MD),
                new MySqlParameter("P_AD_DEV_SALES", dorm.AD_DEV_SALES),
                new MySqlParameter("P_AD_BOM_QTYASS", dorm.AD_BOM_QTYASS),
                new MySqlParameter("P_AD_PT_QTYASS", dorm.AD_PT_QTYASS),
                new MySqlParameter("P_AD_CONFIRM", dorm.AD_CONFIRM),
                new MySqlParameter("P_LABORPROC", dorm.LABORPROC),
                new MySqlParameter("P_LABORCOST", dorm.LABORCOST),
                new MySqlParameter("P_FINALREVCHK", dorm.FINALREVCHK),
                new MySqlParameter("P_USE", dorm.USE),
                new MySqlParameter("P_FRONTVIEW", dorm.FRONTVIEW),
                new MySqlParameter("P_SIDEVIEW", dorm.SIDEVIEW),
                new MySqlParameter("P_DESCRIPTION1", dorm.DESCRIPTION1),
                new MySqlParameter("P_DESCRIPTION2", dorm.DESCRIPTION2),
                new MySqlParameter("P_DESCRIPTION3", dorm.DESCRIPTION3),
                new MySqlParameter("P_BOMFILE", dorm.BOMFILE),
                new MySqlParameter("P_FILEDESCRIPTION1", dorm.FILEDESCRIPTION1),
                new MySqlParameter("P_FILEDESCRIPTION2", dorm.FILEDESCRIPTION2),
                new MySqlParameter("P_FILEDESCRIPTION3", dorm.FILEDESCRIPTION3),
                new MySqlParameter("P_SEASONCODE", dorm.SEASONCODE),
                new MySqlParameter("P_COLLECNAME", dorm.COLLECNAME),
                new MySqlParameter("P_SAMPLESTAGE", dorm.SAMPLESTAGE),
                new MySqlParameter("P_MODEL", dorm.MODEL),
                new MySqlParameter("P_PATTERN", dorm.PATTERN),
                new MySqlParameter("P_FUNCTIONS", dorm.FUNCTIONS),
                new MySqlParameter("P_FUN_ATCH", dorm.FUN_ATCH),
                new MySqlParameter("P_PICTURE", dorm.PICTURE),
                new MySqlParameter("P_PDMFILE", dorm.PDMFILE),
                new MySqlParameter("P_DWG_FILE", dorm.DWG_FILE),
                new MySqlParameter("P_MDL_FILE", dorm.MDL_FILE),
                new MySqlParameter("P_PLX_FILE", dorm.PLX_FILE),
                new MySqlParameter("P_PDF_LAYOUT", dorm.PDF_LAYOUT),
                new MySqlParameter("P_IGX_FILE", dorm.IGX_FILE),
                new MySqlParameter("P_PDF_SOP", dorm.PDF_SOP),
                new MySqlParameter("P_CDR_FILE", dorm.CDR_FILE),
                new MySqlParameter("P_DXF_FILE", dorm.DXF_FILE),
                new MySqlParameter("P_GBR_FILE", dorm.GBR_FILE),
                new MySqlParameter("P_MINI_MARKER", dorm.MINI_MARKER),
                new MySqlParameter("P_CUTTING_FILE", dorm.CUTTING_FILE),
                new MySqlParameter("P_SPEC_FILE", dorm.SPEC_FILE),
                new MySqlParameter("P_FILE_PLAN", dorm.FILE_PLAN),
                new MySqlParameter("P_TRIM_FILE", dorm.TRIM_FILE),
                new MySqlParameter("P_DIFFICULTY", dorm.DIFFICULTY),
                new MySqlParameter("P_UNITPRODUCTIVITY", dorm.UNITPRODUCTIVITY),
                new MySqlParameter("P_LEARNINGCURVE", dorm.LEARNINGCURVE),
                new MySqlParameter("P_OFFEROPPRICE", dorm.OFFEROPPRICE),
                new MySqlParameter("P_TARGETOPPRICE", dorm.TARGETOPPRICE)
            };

            var blIns = MySqlDBManager.ExecuteQuery(strSql, CommandType.Text, param.ToArray());

            return blIns != null;
        }
        #endregion

    }
}
