using OPS_DAL.APIEntities;
using OPS_DAL.DAL;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.APIBus
{
    public class UsmtAPIBus
    {
        /// <summary>
        /// Get all user information by userID
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static UsmtAPI GetUserInfo(string userId)
        {
            var strSql = "SELECT * FROM T_CM_USMT WHERE USERID = :P_USERID ";
            var oracleParams = new List<OracleParameter>()
            {
                new OracleParameter("P_USERID", userId)
            };

            var usmt = OracleDbManager.GetObjects<UsmtAPI>(strSql, CommandType.Text, oracleParams.ToArray()).FirstOrDefault();

            return usmt;
        }
    }
}
