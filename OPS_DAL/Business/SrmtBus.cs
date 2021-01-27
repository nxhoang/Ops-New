using System.Collections.Generic;
using System.Data;
using System.Linq;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using Oracle.ManagedDataAccess.Client;
using MySql.Data.MySqlClient;

namespace OPS_DAL.Business
{
    public class SrmtBus
    {
        /// <summary>
        /// Gets the user right.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <param name="systemId">The system identifier.</param>
        /// <param name="menuId">The menu identifier.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Srmt GetUserRoleInfo(string roleId, string systemId, string menuId)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            List<OracleParameter> oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_ROLEID", roleId),
                new OracleParameter("P_SYSTEMID", systemId),
                new OracleParameter("P_MENU", menuId),
                cursor
            };
            var userRight = OracleDbManager.GetObjects<Srmt>("SP_OPS_GETUSERROLE_SRMT", CommandType.StoredProcedure, oracleParams.ToArray());
            return userRight.FirstOrDefault();
        }

        #region MySQL MES
        /// <summary>
        /// Gets the user right.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <param name="systemId">The system identifier.</param>
        /// <param name="menuId">The menu identifier.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Srmt GetUserRoleInfoMySql(string roleId, string systemId, string menuId)
        {
            string strSql = @" SELECT OWNER_ID OWNERID, IS_VIEW ISVIEW, IS_ADD ISADD, IS_UPDATE ISUPDATE,
                                       IS_DELETE ISDELETE, IS_PRINT ISPRINT, IS_EXPORT ISEXPORT, IS_CONFIRM ISCONFIRM
                               FROM   T_CM_SRMT
                               WHERE   SYSTEM_ID = ?P_SYSTEMID
                                       AND OWNER_ID = ?P_ROLEID
                                       AND MENU_ID = ?P_MENU; ";

            var oraParams = new List<MySqlParameter>() {
                new MySqlParameter("P_SYSTEMID", systemId),
                new MySqlParameter("P_ROLEID", roleId),
                new MySqlParameter("P_MENU", menuId)
            };

            var lstSrmt = MySqlDBManager.GetObjects<Srmt>(strSql, CommandType.Text, oraParams.ToArray()).FirstOrDefault();

            return lstSrmt;
        }

        #endregion
    }
}
