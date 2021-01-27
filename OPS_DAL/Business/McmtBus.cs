using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using Oracle.ManagedDataAccess.Client;

namespace OPS_DAL.Business
{
    public class McmtBus
    {
        #region Oracle database
        /// <summary>
        /// Get list buyer base on AO qty
        /// </summary>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Mcmt> GetBuyersByAOQty()
        {
            string strSql = @"SELECT T1.*, T2.QTY FROM (
                                SELECT MC.S_CODE SUBCODE, MC.S_CODE ||' - '|| MC.CODE_NAME CODENAME 
                                FROM T_CM_MCMT MC WHERE M_CODE = 'Buyer' AND CODE_STATUS = 'OK'
                            )T1 LEFT JOIN (
                                SELECT SUBSTR(STYLECODE,1,3) AS BUYER, SUM(ADQTY) AS QTY 
                                FROM T_AD_ADSM  
                                WHERE EXTRACT (YEAR FROM FA_CONFIRM) = EXTRACT (YEAR FROM SYSDATE)
                                GROUP BY SUBSTR(STYLECODE,1,3) 
                            )T2 ON T2.BUYER = T1.SUBCODE
                            ORDER BY T2.QTY DESC NULLS LAST";

            var listBuyer = OracleDbManager.GetObjects<Mcmt>(strSql, null);

            return listBuyer;
        }

        /// <summary>
        /// SON ADD
        /// </summary>
        /// <param name="mCode"></param>
        /// <returns>A list master code</returns>
        public static List<Mcmt> GetMasterCode(string mCode)
        {
            var sb = new StringBuilder();
            sb.AppendLine(" SELECT");
            sb.AppendLine("     M_CODE MASTERCODE, S_CODE SUBCODE, CODE_NAME CODENAME, CODE_DESC CODEDESC ");
            sb.AppendLine("     , CODE_DETAIL CODEDETAIL, CODE_DETAIL2 CODEDETAIL2, CODE_STATUS CODESTATUS ");
            sb.AppendLine(" FROM T_CM_MCMT WHERE M_CODE = '" + mCode + "' and s_code <> '000'  ");
            sb.AppendLine(" ORDER BY CODE_DESC");

            var lstMasterCode = OracleDbManager.GetObjects<Mcmt>(sb.ToString(), null);
            return lstMasterCode;
        }

        /// <summary>
        /// Get master code by master code and status
        /// </summary>
        /// <param name="mCode"></param>
        /// <param name="codeStatus"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Mcmt> GetMasterCodeByStauts(string mCode, string codeStatus)
        {
            string strSql = @"SELECT  M_CODE MASTERCODE, S_CODE SUBCODE, CODE_NAME CODENAME, CODE_DESC CODEDESC 
                                , CODE_DETAIL CODEDETAIL, CODE_DETAIL2 CODEDETAIL2, CODE_STATUS CODESTATUS  
                                FROM PKERP.T_CM_MCMT 
                                WHERE M_CODE = :P_MCODE AND s_code <> '000' ";

            var whereConStatus = " AND CODE_STATUS = :P_CODESTATUS ";
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_MCODE", mCode)
            };

            //Check code description condition
            if (!string.IsNullOrEmpty(codeStatus))
            {
                oracleParams.Add(new OracleParameter("P_CODESTATUS", codeStatus));
                strSql += whereConStatus;
            }

            strSql += "ORDER BY CODE_NAME";

            var lstMcmt = OracleDbManager.GetObjects<Mcmt>(strSql, CommandType.Text, oracleParams.ToArray());

            return lstMcmt;
        }

        /// <summary>
        /// list SMS type
        /// </summary>
        /// <param name="mCode"></param>
        /// <returns>A list master code</returns>
        /// Author: VitHV
        public static List<Mcmt> GetSmSgTyPe()
        {
            string sql = "SELECT DISTINCT CODE_DESC SUBCODE FROM T_CM_MCMT WHERE M_CODE = 'SystemMessage'";
            var lstMasterCode = OracleDbManager.GetObjects<Mcmt>(sql, null);
            return lstMasterCode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mCode"></param>
        /// <returns>A list master code</returns>
        /// Author: VitHV
        public static List<Mcmt> GetSmSgContext()
        {
            string sql = "SELECT S_CODE SUBCODE, CODE_NAME CODENAME, CODE_DESC CodeDesc FROM T_CM_MCMT WHERE M_CODE = 'SystemMessage'";
            var lstMasterCode = OracleDbManager.GetObjects<Mcmt>(sql, null);
            return lstMasterCode;
        }

        /// <summary>
        /// Get master code with master code, subcode and code description
        /// </summary>
        /// <param name="masterCode"></param>
        /// <param name="subCode"></param>
        /// <param name="codeDesc"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Mcmt> GetMasterCode2(string masterCode, string subCode, string codeDesc)
        {
            string strWhere = "1 = 1 ";
            if (!string.IsNullOrWhiteSpace(masterCode))
            {
                strWhere += " AND M_CODE = '" + masterCode + "'";
            }

            if (!string.IsNullOrWhiteSpace(subCode))
            {
                strWhere += " AND S_CODE = '" + subCode + "'";
            }

            if (!string.IsNullOrWhiteSpace(codeDesc))
            {
                strWhere += " AND CODE_DESC = '" + codeDesc + "'";
            }

            var sb = new StringBuilder();
            sb.AppendLine(" SELECT");
            sb.AppendLine("     M_CODE MASTERCODE, S_CODE SUBCODE, CODE_NAME CODENAME, CODE_DESC CODEDESC ");
            sb.AppendLine("     , CODE_DETAIL CODEDETAIL, CODE_DETAIL2 CODEDETAIL2, CODE_STATUS CODESTATUS ");
            sb.AppendLine(" FROM T_CM_MCMT WHERE s_code <> '000' AND " + strWhere);
            sb.AppendLine(" ORDER BY CODE_NAME ");

            var lstMasterCode = OracleDbManager.GetObjects<Mcmt>(sb.ToString(), null);
            return lstMasterCode;
        }

        /// <summary>
        /// Get machine category
        /// </summary>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static List<Mcmt> GetCategorysMachineTool(string isMachine)
        {
            var strSQL = string.Empty;
            if (isMachine == "1")
            {
                strSQL = @"SELECT S_CODE SUBCODE,CODE_NAME CODENAME
                            FROM T_CM_MCMT 
                            WHERE ( M_CODE IN( 'SewingMc','NonSewingMc')) 
                            ORDER BY CODE_NAME";
            }
            else
            {
                strSQL = @"SELECT S_CODE SUBCODE,CODE_NAME CODENAME 
                            FROM T_CM_MCMT 
                            WHERE M_CODE = 'OPTool'
                            ORDER BY CODE_NAME";
            }
            var lstCat = OracleDbManager.GetObjects<Mcmt>(strSQL, null);

            return lstCat;
        }

        /// <summary>
        /// Get master code with code description and code detail
        /// </summary>
        /// <param name="masterCode"></param>
        /// <param name="subCode"></param>
        /// <param name="codeDesc"></param>
        /// <param name="codeDetail"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static List<Mcmt> GetMasterCode3(string masterCode, string subCode, string codeDesc, string codeDetail)
        {
            string strWhere = "1 = 1 ";
            if (!string.IsNullOrWhiteSpace(masterCode))
            {
                strWhere += " AND M_CODE = '" + masterCode + "'";
            }

            if (!string.IsNullOrWhiteSpace(subCode))
            {
                strWhere += " AND S_CODE = '" + subCode + "'";
            }

            if (!string.IsNullOrWhiteSpace(codeDesc))
            {
                strWhere += " AND CODE_DESC = '" + codeDesc + "'";
            }

            if (!string.IsNullOrWhiteSpace(codeDetail))
            {
                strWhere += " AND CODE_DETAIL = '" + codeDetail + "'";
            }

            var sb = new StringBuilder();
            sb.AppendLine(" SELECT");
            sb.AppendLine("     M_CODE MASTERCODE, S_CODE SUBCODE, CODE_NAME CODENAME, CODE_DESC CODEDESC ");
            sb.AppendLine("     , CODE_DETAIL CODEDETAIL, CODE_DETAIL2 CODEDETAIL2, CODE_STATUS CODESTATUS ");
            sb.AppendLine(" FROM T_CM_MCMT WHERE s_code <> '000' AND " + strWhere);
            sb.AppendLine(" ORDER BY CODE_NAME ");

            var lstMasterCode = OracleDbManager.GetObjects<Mcmt>(sb.ToString(), null);
            return lstMasterCode;
        }

        /// <summary>
        /// Gets the master code.
        /// </summary>
        /// <param name="mCode">The m code.</param>
        /// <param name="mDes">The m DES.</param>
        /// <returns></returns>
        /// Author: VitHV
        public static List<Mcmt> GetMasterCode(string mCode, string mDes)
        {
            var sb = new StringBuilder();
            sb.AppendLine(" SELECT");
            sb.AppendLine("     M_CODE MASTERCODE, S_CODE SUBCODE, CODE_NAME CODENAME, CODE_DESC CODEDESC ");
            sb.AppendLine("     , CODE_DETAIL CODEDETAIL, CODE_DETAIL2 CODEDETAIL2, CODE_STATUS CODESTATUS ");
            sb.AppendLine(" FROM T_CM_MCMT WHERE M_CODE LIKE '%" + mCode + "%' and CODE_DESC LIKE '%" + mDes + "%' and s_code <> '000'  ");
            sb.AppendLine(" ORDER BY CODE_NAME ");

            var lstMasterCode = OracleDbManager.GetObjects<Mcmt>(sb.ToString(), null);
            return lstMasterCode;
        }

        /// <summary>
        /// Gets the master code by code.
        /// </summary>
        /// <param name="mCode">The m code.</param>
        /// <param name="mDes">The m DES.</param>
        /// <returns></returns>
        /// Author: VitHV
        public static List<Mcmt> GetMasterCodeByCode()
        {
            var sb = new StringBuilder();
            sb.AppendLine(" SELECT");
            sb.AppendLine("     M_CODE MASTERCODE, S_CODE SUBCODE, CODE_NAME CODENAME, CODE_DESC CODEDESC ");
            sb.AppendLine("     , CODE_DETAIL CODEDETAIL, CODE_DETAIL2 CODEDETAIL2, CODE_STATUS CODESTATUS ");
            sb.AppendLine(" FROM T_CM_MCMT WHERE M_CODE IN( 'OPTool','MachineType') AND s_code <> '000'  ");
            sb.AppendLine(" ORDER BY CODE_NAME ");
            var lstMasterCode = OracleDbManager.GetObjects<Mcmt>(sb.ToString(), null);
            return lstMasterCode;
        }

        /// <summary>
        /// Gets the name of the sub code by code.
        /// </summary>
        /// <param name="codeName">Name of the code.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Mcmt GetSubCodeByCodeName(string codeName)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            List<OracleParameter> oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_CODENAME", codeName),
                cursor
            };
            var masterCode = OracleDbManager.GetObjects<Mcmt>("SP_OPS_GETSUBCODEBYNAME_MCMT", CommandType.StoredProcedure, oracleParams.ToArray());
            return masterCode.FirstOrDefault();
        }

        /// <summary>
        /// Get list style file
        /// </summary>
        /// <returns></returns>
        public static List<Mcmt> GetStyleFiles()
        {
            string strSql = @"SELECT DISTINCT SUBSTR(CODE_DETAIL2, 2) CODEDETAIL2, LOWER(CODE_DETAIL) CODEDETAIL
                                FROM T_CM_MCMT WHERE M_CODE = 'StyleFile' 
                                AND CODE_DESC IN ('Embroidery Design', 'Jig', 'Machine File', 'Marker File', 'Others', 'Printing', 'CAD File')
                                AND CODE_DETAIL IS NOT NULL
                                ORDER BY CODEDETAIL";
            var lstMasterCode = OracleDbManager.GetObjects<Mcmt>(strSql, null);
            return lstMasterCode;
        }

        #endregion

        #region MySQL database

        public static Mcmt GetByCodeName(string codeName)
        {
            var ps = new List<MySqlParameter> { new MySqlParameter("P_CODENAME", codeName) };
            var mcmt = MySqlDBManager.GetAll<Mcmt>("SP_MES_GETBYCODENAME_MCMT", CommandType.StoredProcedure,
                ps.ToArray());

            return mcmt.FirstOrDefault();
        }

        /// <summary>
        /// Gets the by master code.
        /// </summary>
        /// <param name="mCode">The master code.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static List<Mcmt> GetByMasterCode(string mCode)
        {
            var ps = new List<MySqlParameter> { new MySqlParameter("p_mcode", mCode) };
            var mcmts = MySqlDBManager.GetAll<Mcmt>("SP_MES_GETBYMCODE_MCMT", CommandType.StoredProcedure,
                ps.ToArray());

            return mcmts;
        }

        /// <summary>
        /// Gets the categories machine tool.
        /// </summary>
        /// <param name="isMachine">The is machine.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static List<Mcmt> GetCategoriesMachineTool(string isMachine)
        {
            var ps = new List<MySqlParameter> { new MySqlParameter("p_ismachine", isMachine) };
            var mcmts = MySqlDBManager.GetAll<Mcmt>("SP_MES_GETCATEGORIES_MCMT", CommandType.StoredProcedure,
                ps.ToArray());

            return mcmts;
        }

        #endregion
    }
}
