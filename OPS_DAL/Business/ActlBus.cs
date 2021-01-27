using System.Collections.Generic;
using System.Data;
using System.Linq;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using MySql.Data.MySqlClient;

namespace OPS_DAL.Business
{
    public class ActlBus
    {
        /// <summary>
        /// Inserts the log.
        /// </summary>
        /// <param name="act">The act.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertLog(Actl act)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) {Direction = ParameterDirection.Output},
                new OracleParameter("P_USERID", act.UserId),
                new OracleParameter("P_ROLEID", act.RoleId),
                new OracleParameter("P_FUNCTIONID", act.FunctionId),
                new OracleParameter("P_OPERATIONID", act.OperationId),
                new OracleParameter("P_SUCCESS", act.Success),
                new OracleParameter("P_REFNO", act.RefNo),
                new OracleParameter("P_REMARK", act.Remark),
                new OracleParameter("P_SYSTEMID", act.SystemId)
            };

            var resInsert = OracleDbManager.ExecuteQuery("SP_PLM_INSERTLOG_ACTL", oracleParams.ToArray(), CommandType.StoredProcedure);

            return resInsert != null && int.Parse(resInsert.ToString()) != 0;

        }

        /// <summary>
        /// Get Actl By Login.
        /// </summary>
        /// <param name="sysId">The sysId.</param>
        /// <param name="funcId">The funcId.</param>
        /// <param name="userId">The userId.</param>
        /// <returns></returns>
        /// Author: VitHV
        public static List<Actl> GetActlByLogin(string sysId, string funcId, string userId)
        {
            if(funcId == ConstantGeneric.ScreenRegistry)
            {
                funcId = "'" + ConstantGeneric.ScreenRegistry + "','"+ ConstantGeneric.ScreenLayout + "'";
            }
            else
            {
                funcId = "'" + funcId + "'";
            }
            string sql = @"SELECT 
              SUBSTR(A.REFNO, 0, 7) STYLECODE
              ,SUBSTR(A.REFNO, 8, 3) STYLESIZE
              ,SUBSTR(A.REFNO, 11, 3) STYLECOLORSERIAL
              ,SUBSTR(A.REFNO, 14, 3) REVNO
              ,SUBSTR(A.REFNO, 17, 3) OPREVNO
              ,SUBSTR(A.REFNO, 20, 1) EDITION
              ,A.TRANSACTIONTIME
              ,A.REMARK,A.REFNO
              ,A.FUNCTIONID
              ,OPERATIONID
              ,B.BUYERSTYLECODE
              ,B.BUYER
              ,B.STYLEGROUP, B.SUBGROUP, B.SUBSUBGROUP
            FROM T_RD_ACTL A
            LEFT JOIN T_00_STMT B ON SUBSTR(A.REFNO, 0, 7) = B.STYLECODE
            WHERE A.SYSTEMID = '" + sysId + @"' AND A.FUNCTIONID IN ("+ funcId + @")
            AND A.USERID = '"+ userId + @"' AND A.SUCCESS = '1'
            AND LENGTH(REFNO) = 20
            AND A.TRANSACTIONTIME > SYSDATE -30
            ORDER BY A.TRANSACTIONTIME DESC ";
            var ret = OracleDbManager.GetObjects<Actl>(sql, null);
            return ret;
        }

        /// <summary>
        /// Get Actl By Login.
        /// </summary>
        /// <param name="sysId">The sysId.</param>
        /// <param name="funcId">The funcId.</param>
        /// <param name="userId">The userId.</param>
        /// <returns></returns>
        /// Author: VitHV
        public static Actl GetActlByLog(string sysId, string funcId, string userId)
        {
            string sql = @"SELECT t1.* FROM( SELECT ROW_NUMBER() OVER (ORDER BY TRANSACTIONTIME DESC) AS rownumber, l.*
                              FROM T_RD_ACTL L 
                              WHERE L.SYSTEMID ='OPS' 
                                    AND L.FUNCTIONID ='LOG'
                                    AND L.OPERATIONID ='I'
                                    AND L.SUCCESS = '1'
                                    AND L.USERID = '" + userId + @"'                                 
                            ) t1 WHERE rownumber =  2";
            var ret = OracleDbManager.GetObjects<Actl>(sql, null).FirstOrDefault();
            return ret;
        }

        #region MySQL

        /// <summary>
        /// Add transaction log
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <param name="functionId"></param>
        /// <param name="operationId"></param>
        /// <param name="successStatus"></param>
        /// <param name="menuId"></param>
        /// <param name="systemId"></param>
        /// <param name="refNo"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool AddTransactionLog(string userId, string roleId, string functionId, string operationId, string successStatus, string menuId, string systemId, string refNo, string remark)
        {
            string strSql = @"  INSERT INTO T_RD_ACTL(USERID, ROLEID, FUNCTIONID, OPERATIONID, SUCCESS, TRANSACTIONTIME, MENUID, SYSTEMID, REFNO, REMARK )
                        VALUES(?P_USERID, ?P_ROLEID, ?P_FUNCTIONID, ?P_OPERATIONID,  ?P_SUCCESS, SYSDATE(), ?P_MENUID, ?P_SYSTEMID, ?P_REFNO, ?P_REMARK); ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_USERID", userId),
                new MySqlParameter("P_ROLEID", roleId),
                new MySqlParameter("P_FUNCTIONID", functionId),
                new MySqlParameter("P_OPERATIONID", operationId),
                new MySqlParameter("P_SUCCESS", successStatus),
                new MySqlParameter("P_MENUID", menuId),
                new MySqlParameter("P_SYSTEMID", systemId),
                new MySqlParameter("P_REFNO", refNo),
                new MySqlParameter("P_REMARK", remark)
            };

            var blIns = MySqlDBManager.ExecuteQuery(strSql, CommandType.Text, param.ToArray());

            return blIns != null;

        }
        #endregion

    }
}
