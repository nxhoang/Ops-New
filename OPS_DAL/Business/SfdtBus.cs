using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OPS_DAL.Business
{
    public class SfdtBus
    {
        /// <summary>
        /// Get list of style file by style code
        /// </summary>
        /// <param name="styleCode"></param>
        /// <returns></returns>
        public static List<Sfdt> GetStyleFileByStyleCode(string styleCode)
        {
            var strSql = @"SELECT STYLECODE, SERIAL, FILENAME, DESCRIPTION, IS_MAIN ISMAIN 
                            FROM T_00_SFDT WHERE STYLECODE = :P_STYLECODE ";
            var oracleParams = new List<OracleParameter>()
            {
                new OracleParameter("P_STYLECODE", styleCode)
            };

            var listSfdt = OracleDbManager.GetObjects<Sfdt>(strSql, CommandType.Text, oracleParams.ToArray());

            return listSfdt;
        }

        /// <summary>
        /// Get style file detail
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="serail"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Sfdt GetStyleFile(string styleCode, string serail)
        {
            var strSql = @"SELECT STYLECODE, SERIAL, FILENAME, DESCRIPTION, IS_MAIN ISMAIN 
                            FROM T_00_SFDT WHERE STYLECODE = :P_STYLECODE AND SERIAL = :P_SERIAL ";
            var oracleParams = new List<OracleParameter>()
            {
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_SERIAL", serail)
            };

            var sfdt = OracleDbManager.GetObjects<Sfdt>(strSql, CommandType.Text, oracleParams.ToArray()).FirstOrDefault();

            return sfdt;
        }

        /// <summary>
        /// Get max style serial 
        /// </summary>
        /// <param name="styleCode"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static int GetMaxSerialStyleCode(string styleCode)
        {
            var strSql = @"SELECT STYLECODE, SERIAL, FILENAME, DESCRIPTION, IS_MAIN ISMAIN 
                            FROM T_00_SFDT WHERE STYLECODE = :P_STYLECODE
                            and serial =(SELECT MAX(SERIAL) FROM T_00_SFDT WHERE STYLECODE = :P_STYLECODE )";

            var oracleParams = new List<OracleParameter>()
            {
                new OracleParameter("P_STYLECODE", styleCode)
            };

            var sfdt = OracleDbManager.GetObjects<Sfdt>(strSql, CommandType.Text, oracleParams.ToArray()).FirstOrDefault();

            return sfdt == null ? 1 : sfdt.Serial + 1;
        }

        /// <summary>
        /// Update style file detail
        /// </summary>
        /// <param name="sfdt"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateStyleFileDetail(Sfdt sfdt, OracleConnection oraConn, OracleTransaction trans)
        {
            var strSql = "UPDATE T_00_SFDT SET FILENAME = :P_FILENAME, DESCRIPTION = :P_DESCRIPTION, IS_MAIN = :P_ISMAIN WHERE  STYLECODE = :P_STYLECODE AND SERIAL = :P_SERIAL ";
            var oracleParams = new List<OracleParameter>()
            {
                new OracleParameter("P_FILENAME", sfdt.FileName),
                new OracleParameter("P_DESCRIPTION", sfdt.Description),
                new OracleParameter("P_ISMAIN", sfdt.IsMain),
                new OracleParameter("P_STYLECODE", sfdt.StyleCode),
                new OracleParameter("P_SERIAL", sfdt.Serial)
            };

            var res = OracleDbManager.ExecuteQuery(strSql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return res != null;
        }

        /// <summary>
        /// Insert style file detail
        /// </summary>
        /// <param name="sfdt"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertStyleFileDetail(Sfdt sfdt, OracleConnection oraConn, OracleTransaction trans)
        {
            var strSql = @"INSERT INTO T_00_SFDT (STYLECODE, SERIAL, FILENAME, DESCRIPTION, IS_MAIN) 
                            VALUES (:P_STYLECODE, :P_SERIAL, :P_FILENAME, :P_DESCRIPTION, :P_ISMAIN) ";
            var oracleParams = new List<OracleParameter>()
            {
                new OracleParameter("P_STYLECODE", sfdt.StyleCode),
                new OracleParameter("P_SERIAL", sfdt.Serial),
                new OracleParameter("P_FILENAME", sfdt.FileName),
                new OracleParameter("P_DESCRIPTION", sfdt.Description),
                new OracleParameter("P_ISMAIN", sfdt.IsMain)
            };

            var res = OracleDbManager.ExecuteQuery(strSql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return res != null;
        }

        /// <summary>
        /// Insert, update style file detail and update picture name of style
        /// </summary>
        /// <param name="sfdtIns"></param>
        /// <param name="sfdtUpd"></param>
        /// <returns></returns>
        public static bool InsertUpdateStyleFileDetail(Sfdt sfdtIns, Sfdt sfdtUpd)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    //Update is main for previous serial
                    if (sfdtUpd.StyleCode != null && sfdtUpd.Serial != 0)
                    {
                        if (!UpdateStyleFileDetail(sfdtUpd, connection, trans))
                        {
                            trans.Rollback();
                            return false;
                        }
                    }

                    //Update is_main
                    if (!StmtBus.UpdateStylePictureName(sfdtIns.StyleCode, sfdtIns.FileName, connection, trans) || !InsertStyleFileDetail(sfdtIns, connection, trans))
                    {
                        trans.Rollback();
                        return false;
                    }

                    trans.Commit();
                    return true;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        #region MySQL
        /// <summary>
        /// Get list of style file  detail
        /// </summary>
        /// <param name="styleCode"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Sfdt> GetListStyleFileMySql(string styleCode)
        {
            var strSql = @"SELECT STYLECODE, SERIAL, FILENAME, DESCRIPTION, IS_MAIN ISMAIN 
                            FROM T_00_SFDT WHERE STYLECODE = ?P_STYLECODE; ";

            var oracleParams = new List<MySqlParameter> { new MySqlParameter("P_STYLECODE", styleCode) };
            var listSfdt = MySqlDBManager.GetAll<Sfdt>(strSql, CommandType.Text, oracleParams.ToArray());

            return listSfdt;
        }

        /// <summary>
        /// Insert style file detail
        /// </summary>
        /// <param name="sfdt"></param>
        /// <param name="myTrans"></param>
        /// <param name="myConnection"></param>
        /// <returns></returns>
        public static bool InsertStyleFileMySql(Sfdt sfdt, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @" INSERT INTO T_00_SFDT(STYLECODE, SERIAL, FILENAME, DESCRIPTION, IS_MAIN)
                                VALUES(?P_STYLECODE, ?P_SERIAL, ?P_FILENAME, ?P_DESCRIPTION, ?P_IS_MAIN); ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_STYLECODE", sfdt.StyleCode),
                new MySqlParameter("P_SERIAL", sfdt.Serial),
                new MySqlParameter("P_FILENAME", sfdt.FileName),
                new MySqlParameter("P_DESCRIPTION", sfdt.Description),
                new MySqlParameter("P_IS_MAIN", sfdt.IsMain)
            };

            var blIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, param.ToArray(), CommandType.Text, myTrans, myConnection);

            return blIns != null;
        }

        /// <summary>
        /// Insert list of style file (MySQL)
        /// </summary>
        /// <param name="listSfdt"></param>
        /// <returns></returns>
        /// Author: Nguyen Cao Son
        public static bool InsertListStyleFileMySql(List<Sfdt> listSfdt)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    foreach (var sfdt in listSfdt)
                    {
                        InsertStyleFileMySql(sfdt, myTrans, myConnection);
                    }

                    myTrans.Commit();
                    return true;
                }
                catch
                {
                    myTrans.Rollback();
                    throw;
                }
            }
        }
        #endregion

    }
}
