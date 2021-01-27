using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Business
{
    public class SsmtBus
    {
        /// <summary>
        /// Get style size master by style code
        /// </summary>
        /// <param name="styleCode"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Ssmt> GetStyleSizeMasterByCode(string styleCode)
        {
            var strSql = " SELECT * FROM T_00_SSMT WHERE STYLECODE = :P_STYLECODE ";

            List<OracleParameter> oraParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", styleCode),
            };

            var listSsmt = OracleDbManager.GetObjects<Ssmt>(strSql, CommandType.Text, oraParams.ToArray());

            return listSsmt;
        }

        #region MySQL
        /// <summary>
        /// Insert style size master to MySql
        /// </summary>
        /// <param name="sstm"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        internal static bool InsertStyleSizeToMESMySql(Ssmt ssmt, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @" INSERT INTO T_00_SSMT(STYLECODE, STYLESIZE, OLD_STYLECODE) VALUES(?P_STYLECODE, ?P_STYLESIZE, ?P_OLD_STYLECODE); ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_STYLECODE", ssmt.StyleCode),
                new MySqlParameter("P_STYLESIZE", ssmt.StyleSize),
                new MySqlParameter("P_OLD_STYLECODE", ssmt.Old_StyleCode)
            };

            var blIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, param.ToArray(), CommandType.Text, myTrans, myConnection);

            return blIns != null;           
        }

        public static bool InsertListStyleSizeToMESMySql(List<Ssmt> listSstm)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    foreach (var mpdt in listSstm)
                    {
                        InsertStyleSizeToMESMySql(mpdt, myTrans, myConnection);

                    }

                    myTrans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    myTrans.Rollback();
                    throw;
                }
            }
        }

        #endregion

    }
}
