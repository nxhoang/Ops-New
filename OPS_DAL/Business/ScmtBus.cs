using OPS_DAL.DAL;
using OPS_DAL.Entities;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using OPS_Utils;
using System;

namespace OPS_DAL.Business
{
    public class ScmtBus
    {
        #region Oracle database

        /// <summary>
        /// Get style color by style code
        /// </summary>
        /// <param name="styleCode"></param>
        /// <returns></returns>
        public static List<Scmt> GetStyleColorByStyleCode(string styleCode)
        {
            var strSql = @"SELECT * FROM T_00_SCMT WHERE STYLECODE = :P_STYLECODE ORDER BY STYLECOLORSERIAL DESC";

            var oracleParams = new List<OracleParameter> {
                new OracleParameter("P_STYLECODE", styleCode)
            };

            return OracleDbManager.GetObjects<Scmt>(strSql, oracleParams.ToArray());
        }

        /// <summary>
        /// Insert new style color
        /// </summary>
        /// <param name="scmtIns"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static bool InsertStyleColor(Scmt scmtIns, OracleConnection oraConn, OracleTransaction trans)
        {
            var strSql = @"INSERT INTO T_00_SCMT(STYLECODE, STYLECOLORWAYS, STYLECOLORSERIAL, STYLECOLOR) 
                                VALUES(:P_STYLECODE, :P_STYLECOLORWAYS, :P_STYLECOLORSERIAL, :P_STYLECOLOR)";
            var oraParam = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", scmtIns.StyleCode),
                new OracleParameter("P_STYLECOLORWAYS", scmtIns.StyleColorWays),
                new OracleParameter("P_STYLECOLORSERIAL", scmtIns.StyleColorSerial),
                new OracleParameter("P_STYLECOLOR", scmtIns.StyleColor)
            };

            var result = OracleDbManager.ExecuteQuery(strSql, oraParam.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }

        #endregion

        #region MySQL database

        public static bool AddScmt(Scmt scmt, MySqlConnection connection, MySqlTransaction transaction)
        {
            var pList = new List<MySqlParameter>
            {
                new MySqlParameter("P_STYLECODE", scmt.StyleCode),
                new MySqlParameter("P_STYLECOLORWAYS", scmt.StyleColorWays),
                new MySqlParameter("P_STYLECOLORSERIAL", scmt.StyleColorSerial),
                new MySqlParameter("P_STYLECOLOR", scmt.StyleColor)
            };

            var resultScmt = MySqlDBManager.ExecuteQueryWithTrans("SP_MES_INSERT_SCMT", pList.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return resultScmt != null;
        }

        /// <summary>
        /// Insert style color.
        /// </summary>
        /// <param name="scmt"></param>
        /// <param name="myTrans"></param>
        /// <param name="myConnection"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        internal static bool InsertStyleColorToMESMySql(Scmt scmt, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @" INSERT INTO T_00_SCMT(STYLECODE, STYLECOLORWAYS, STYLECOLORSERIAL, STYLECOLOR, RGBVALUE, OLD_STYLECODE, BUYERCOLORCODE, IMAGETYPE)
                                VALUES(?P_STYLECODE, ?P_STYLECOLORWAYS, ?P_STYLECOLORSERIAL, ?P_STYLECOLOR, ?P_RGBVALUE, ?P_OLD_STYLECODE, ?P_BUYERCOLORCODE, ?P_IMAGETYPE); ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_STYLECODE", scmt.StyleCode),
                new MySqlParameter("P_STYLECOLORWAYS", scmt.StyleColorWays),
                new MySqlParameter("P_STYLECOLORSERIAL", scmt.StyleColorSerial),
                new MySqlParameter("P_STYLECOLOR", scmt.StyleColor),
                new MySqlParameter("P_RGBVALUE", scmt.RgbValue),
                new MySqlParameter("P_OLD_STYLECODE", scmt.Old_StyleCode),
                new MySqlParameter("P_BUYERCOLORCODE", scmt.BuyerColorCode),
                new MySqlParameter("P_IMAGETYPE", scmt.ImageType)
            };

            var blIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, param.ToArray(), CommandType.Text, myTrans, myConnection);

            return blIns != null;
        }

        /// <summary>
        /// Insert list of style color to MES MySQL
        /// </summary>
        /// <param name="listSctm"></param>
        /// <returns></returns>
        public static bool InsertListStyleColorToMESMySql(List<Scmt> listSctm)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    foreach (var scmt in listSctm)
                    {
                        InsertStyleColorToMESMySql(scmt, myTrans, myConnection);
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
