using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public class LndtBus
    {
        #region Oracle
        /// <summary>
        /// Insert line detail with oracle
        /// </summary>
        /// <param name="lineDt"></param>
        /// <param name="oraTrans"></param>
        /// <param name="oraCon"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertLineDetailOracle(Lndt lineDt, OracleTransaction oraTrans, OracleConnection oraCon)
        {
            string strSql = @"insert into PKMES.t_mx_lndt(mxpackage, lineserial, moduleId, processgroup, prodate, registerid, registrydate)
                                values(:P_MXPACKAGE, :P_LINESERIAL, :P_MODULEID, :P_PROCESSGROUP, :P_PRODATE, :P_REGISTERID, SYSDATE)";

            var param = new List<OracleParameter>()
            {
                new OracleParameter("P_MXPACKAGE", lineDt.MxPackage),
                new OracleParameter("P_LINESERIAL", lineDt.LineSerial),
                new OracleParameter("P_MODULEID", lineDt.ModuleId),
                new OracleParameter("P_PROCESSGROUP", lineDt.ProcessGroup),
                new OracleParameter("P_PRODATE", lineDt.ProDate),
                new OracleParameter("P_REGISTERID", lineDt.RegisterId)
            };

            var resIns = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text, oraTrans, oraCon);

            return resIns != null;
        }

        /// <summary>
        /// Insert line detail with transaction
        /// </summary>
        /// <param name="lineDt"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertLineDetailOracle(Lndt lineDt)
        {
            using (var oraConn = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                oraConn.Open();
                var oraTrans = oraConn.BeginTransaction();
                try
                {
                    if(!InsertLineDetailOracle(lineDt, oraTrans, oraConn))
                    {
                        oraTrans.Rollback();
                        return false;
                    }

                    oraTrans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    oraTrans.Rollback();
                    return false;
                }
            }
        }

        /// <summary>
        /// Delete line detail with transaction
        /// </summary>
        /// <param name="mxPacakge"></param>
        /// <param name="lineSerial"></param>
        /// <param name="oraTrans"></param>
        /// <param name="oraCon"></param>
        /// <returns></returns>
        public static bool DeleteLineDetailOracle(string mxPacakge, string lineSerial, OracleTransaction oraTrans, OracleConnection oraCon)
        {
            string strSql = @" DELETE FROM PKMES.T_MX_LNDT WHERE MXPACKAGE = :P_MXPACKAGE ";

            var lineSerialCon = @" AND LINESERIAL = :P_LINESERIAL ";

            var param = new List<OracleParameter>()
            {
                new OracleParameter("P_MXPACKAGE", mxPacakge)
            };

            //Check line serial condition, If it is not empty then combine line serial condition to the query
            if (!string.IsNullOrEmpty(lineSerial))
            {
                strSql += lineSerialCon;
                param.Add(new OracleParameter("P_LINESERIAL", lineSerial));
            }

            var resIns = OracleDbManager.ExecuteQuery(strSql,  param.ToArray(), CommandType.Text, oraTrans, oraCon);

            return resIns != null;
        }

        /// <summary>
        /// Delete line detail
        /// </summary>
        /// <param name="mxPacakge"></param>
        /// <param name="lineSerial"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool DeleteLineDetailOracle(string mxPacakge, string lineSerial)
        {
            using (var oraConn = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                oraConn.Open();
                var oraTrans = oraConn.BeginTransaction();
                try
                {
                    if (!DeleteLineDetailOracle(mxPacakge, lineSerial, oraTrans, oraConn))
                    {
                        oraTrans.Rollback();
                        return false;
                    }

                    oraTrans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    oraTrans.Rollback();
                    return false;
                }
            }
        }

        #endregion

        #region MySQL      

        /// <summary>
        /// Get list of line detail by MES package
        /// </summary>
        /// <param name="mesPackage"></param>
        /// <returns></returns>
        public static List<Lndt> GetLinesDetailByMesPkgMySql(string mesPackage)
        {
            //Get style code in mes package 
            var styleCode = mesPackage.Split('_')[3].Substring(0, 7);

            string strSql = @"SELECT LNDT.* , LIN.LINENAME, MCM.CODE_NAME AS PROCESSGROUPNAME, SAM.MODULENAME
                            FROM T_MX_LNDT LNDT 
	                            JOIN T_CM_LINE LIN ON LIN.LINESERIAL = LNDT.LINESERIAL
                                LEFT JOIN T_CM_MCMT MCM ON MCM.S_CODE = LNDT.PROCESSGROUP AND MCM.M_CODE = 'OPGroup'
                                LEFT JOIN T_00_SAMT SAM ON SAM.MODULEID = LNDT.MODULEID AND SAM.STYLECODE = @P_STYLECODE
                            WHERE MXPACKAGE  = @P_MXPACKAGE ;";

            var param = new List<MySqlParameter>()
            {
                new MySqlParameter("P_MXPACKAGE", mesPackage),
                new MySqlParameter("P_STYLECODE", styleCode)
            };

            var listLndt = MySqlDBManager.GetAll<Lndt>(strSql, CommandType.Text, param.ToArray());

            return listLndt;
        }

        /// <summary>
        /// Insert line detail
        /// </summary>
        /// <param name="lineDt"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertLineDetailMySql(Lndt lineDt, MySqlTransaction myTrans, MySqlConnection myConn)
        {
            string strSql = @"insert into t_mx_lndt(mxpackage, lineserial, moduleId, processgroup, prodate, registerid, registrydate)
                                values(?P_MXPACKAGE, ?P_LINESERIAL, ?P_MODULEID, ?P_PROCESSGROUP, ?P_PRODATE, ?P_REGISTERID, SYSDATE());";

            var param = new List<MySqlParameter>()
            {
                new MySqlParameter("P_MXPACKAGE", lineDt.MxPackage),
                new MySqlParameter("P_LINESERIAL", lineDt.LineSerial),
                new MySqlParameter("P_MODULEID", lineDt.ModuleId),
                new MySqlParameter("P_PROCESSGROUP", lineDt.ProcessGroup),
                new MySqlParameter("P_PRODATE", lineDt.ProDate),
                new MySqlParameter("P_REGISTERID", lineDt.RegisterId)
            };

            var resIns = MySqlDBManager.ExecuteQueryWithTrans(strSql,  param.ToArray(), CommandType.Text, myTrans, myConn);

            return resIns != null;
        }

        public static bool InsertLineDetailMySql(Lndt lineDt)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    if (!InsertLineDetailMySql(lineDt, myTrans, myConnection))
                    {
                        myTrans.Rollback();
                        return false;
                    }

                    if (!InsertLineDetailOracle(lineDt))
                    {
                        myTrans.Rollback();
                        return false;
                    }

                    myTrans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    myTrans.Rollback();
                    return false;
                }
            }
        }

        /// <summary>
        /// Delete line detail with transaction
        /// </summary>
        /// <param name="mxPacakge"></param>
        /// <param name="lineSerial"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool DeleteLineDetailMySql(string mxPacakge, string lineSerial, MySqlTransaction myTrans, MySqlConnection myConn)
        {
            string strSql = @" DELETE FROM T_MX_LNDT WHERE MXPACKAGE = ?P_MXPACKAGE ";

            var lineSerialCon = @" AND LINESERIAL = ?P_LINESERIAL ";

            var param = new List<MySqlParameter>()
            {
                new MySqlParameter("P_MXPACKAGE", mxPacakge)
            };

            //Check line serial condition, If it is not empty then combine line serial condition to the query
            if (!string.IsNullOrEmpty(lineSerial))
            {
                strSql += lineSerialCon;
                param.Add(new MySqlParameter("P_LINESERIAL", lineSerial));
            }

            var resIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, param.ToArray(), CommandType.Text, myTrans, myConn);

            return resIns != null;
        }

        /// <summary>
        /// Delete line detail
        /// </summary>
        /// <param name="mxPacakge"></param>
        /// <param name="lineSerial"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool DeleteLineDetailMySql(string mxPacakge, string lineSerial)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    if (!DeleteLineDetailMySql(mxPacakge, lineSerial, myTrans, myConnection))
                    {
                        myTrans.Rollback();
                        return false;
                    }

                    if(!DeleteLineDetailOracle(mxPacakge, lineSerial))
                    {
                        myTrans.Rollback();
                        return false;
                    }

                    myTrans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    myTrans.Rollback();
                    return false;
                }
            }
        }

        #endregion
    }
}
