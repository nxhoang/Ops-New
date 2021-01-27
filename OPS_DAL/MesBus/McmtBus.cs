using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public class McmtBus
    {
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
                                FROM T_CM_MCMT 
                                WHERE M_CODE = ?P_MCODE AND s_code <> '000' ";

            var whereConStatus = " AND CODE_STATUS = ?P_CODESTATUS ";
            var oracleParams = new List<MySqlParameter>
            {
                new MySqlParameter("P_MCODE", mCode)
            };

            //Check code description condition
            if (!string.IsNullOrEmpty(codeStatus))
            {
                oracleParams.Add(new MySqlParameter("P_CODESTATUS", codeStatus));
                strSql += whereConStatus;
            }

            strSql += "ORDER BY CODE_NAME";

            var lstMcmt = MySqlDBManager.GetObjects<Mcmt>(strSql, CommandType.Text, oracleParams.ToArray());

            return lstMcmt;
        }
    }
}
