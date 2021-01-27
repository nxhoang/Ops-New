using OPS_DAL.DAL;
using OPS_DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Business
{
    public static class UrlmBus
    {
        #region MySql
        public static List<Urlm> GetMasterRoleList()
        {
            var sb = new StringBuilder();
            sb.Append(@" 
                    SELECT RoleId , RoleDesc
                    FROM t_cm_urlm
                    ");
            return MySqlDBManager.GetObjects<Urlm>(sb.ToString(), CommandType.Text, null);
        }
        #endregion

        #region Oracle
        /// <summary>
        /// Get list sale team
        /// </summary>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Urlm> GetSaleTeams()
        {
            string strSql = @"SELECT T1.*, URL.* FROM (
                                SELECT DISTINCT CODE_DESC 
                                FROM T_CM_MCMT WHERE M_CODE LIKE '%Buyer%'
                            )T1 JOIN T_CM_URLM URL ON URL.ROLEID = T1.CODE_DESC";

            var listUrlm = OracleDbManager.GetObjects<Urlm>(strSql, CommandType.Text, null);

            return listUrlm;
        }
        #endregion
    }
}
