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
    public class MccnBus
    {
        #region Oracle

        /// <summary>
        /// Get list machine count in MES oracle database
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="yyyy"></param>
        /// <param name="mm"></param>
        /// <param name="dd"></param>
        /// <param name="serverNo"></param>
        /// <returns></returns>
        public static List<Mccn> GetListMachineCountOracle(string factory, string yyyy, string mm, string dd, string serverNo)
        {
            //Get list machine count with server No
            string strSql1 = @"  SELECT MCC.*
                                FROM PKMES.T_CM_CSDT CSD 
                                     JOIN PKMES.T_MX_MCCN MCC ON MCC.FACTORY = CSD.FACTORY
                                WHERE MCC.FACTORY = :P_FACTORY AND MCC.YEAR_COUNT = :P_YEAR_COUNT 
                                    AND MCC.MONTH_COUNT = :P_MONTH_COUNT AND MCC.DAY_COUNT = :P_DAY_COUNT 
                                    AND CSD.SERVERNO = :P_SERVERNO   ";

            //Get list machine count without server No
            string strSql2 = @"SELECT * FROM PKMES.T_MX_MCCN MCC
                               WHERE MCC.FACTORY  = :P_FACTORY AND MCC.YEAR_COUNT = :P_YEAR_COUNT 
                                    AND MCC.MONTH_COUNT = :P_MONTH_COUNT AND MCC.DAY_COUNT = :P_DAY_COUNT";

            var strSql = strSql2;

            var param = new List<OracleParameter>
            {
                new OracleParameter("P_FACTORY", factory),
                new OracleParameter("P_YEAR_COUNT", yyyy),
                new OracleParameter("P_MONTH_COUNT", mm),
                new OracleParameter("P_DAY_COUNT", dd)
            };

            if (!string.IsNullOrEmpty(serverNo))
            {
                strSql = strSql1;
                param.Add(new OracleParameter("P_SERVERNO", serverNo));
            }

            var listMccn = OracleDbManager.GetObjectsByType<Mccn>(strSql, CommandType.Text, param.ToArray());

            return listMccn;
        }

        /// <summary>
        /// Insert machine count to Oracle MES database
        /// </summary>
        /// <param name="mccn"></param>
        /// <param name="oraTrans"></param>
        /// <param name="oraCon"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertMachineCountOracle(Mccn mccn, OracleTransaction oraTrans, OracleConnection oraCon)
        {
            string strSql = @" INSERT INTO PKMES.T_MX_MCCN(FACTORY, YEAR_COUNT, MONTH_COUNT, DAY_COUNT, WEEKNO, MACHINE_COUNT_DGS, MACHINE_COUNT_PKG, MACHINE_COUNT_MES, UPDATE_DATE)
                                 VALUES (:P_FACTORY, :P_YEAR_COUNT, :P_MONTH_COUNT, :P_DAY_COUNT, :P_WEEKNO, :P_MACHINE_COUNT_DGS, :P_MACHINE_COUNT_PKG, :P_MACHINE_COUNT_MES, SYSDATE)  ";

            var param = new List<OracleParameter>
            {
                new OracleParameter("P_FACTORY", mccn.FACTORY),
                new OracleParameter("P_YEAR_COUNT", mccn.YEAR_COUNT),
                new OracleParameter("P_MONTH_COUNT", mccn.MONTH_COUNT),
                new OracleParameter("P_DAY_COUNT", mccn.DAY_COUNT),
                new OracleParameter("P_WEEKNO", mccn.WEEKNO),
                new OracleParameter("P_MACHINE_COUNT_DGS", mccn.MACHINE_COUNT_DGS),
                new OracleParameter("P_MACHINE_COUNT_PKG", mccn.MACHINE_COUNT_PKG),
                new OracleParameter("P_MACHINE_COUNT_MES", mccn.MACHINE_COUNT_MES)
            };

            var resIns = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text, oraTrans, oraCon);

            return resIns != null;
        }

        /// <summary>
        /// Update machine count in Oracle MES database
        /// </summary>
        /// <param name="mccn"></param>
        /// <param name="oraTrans"></param>
        /// <param name="oraCon"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpateMachineCountOracle(Mccn mccn, OracleTransaction oraTrans, OracleConnection oraCon)
        {
            string strSql = @"UPDATE PKMES.T_MX_MCCN SET MACHINE_COUNT_DGS = :P_MACHINE_COUNT_DGS, MACHINE_COUNT_PKG = :P_MACHINE_COUNT_PKG
		                        , MACHINE_COUNT_MES = :P_MACHINE_COUNT_MES, UPDATE_DATE = SYSDATE WHERE FACTORY = :P_FACTORY AND YEAR_COUNT = :P_YEAR_COUNT AND MONTH_COUNT = :P_MONTH_COUNT AND DAY_COUNT = :P_DAY_COUNT  ";

            var param = new List<OracleParameter>
            {

                new OracleParameter("P_MACHINE_COUNT_DGS", mccn.MACHINE_COUNT_DGS),
                new OracleParameter("P_MACHINE_COUNT_PKG", mccn.MACHINE_COUNT_PKG),
                new OracleParameter("P_MACHINE_COUNT_MES", mccn.MACHINE_COUNT_MES),
                new OracleParameter("P_FACTORY", mccn.FACTORY),
                new OracleParameter("P_YEAR_COUNT", mccn.YEAR_COUNT),
                new OracleParameter("P_MONTH_COUNT", mccn.MONTH_COUNT),
                new OracleParameter("P_DAY_COUNT", mccn.DAY_COUNT)
            };

            var resIns = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text, oraTrans, oraCon);

            return resIns != null;
        }

        /// <summary>
        /// Insert list of machine count 
        /// </summary>
        /// <param name="listMccn"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertListMachineCountOracle(List<Mccn> listMccn)
        {
            using (var oraCon = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                oraCon.Open();
                var oraTran = oraCon.BeginTransaction();
                try
                {
                    foreach (var mccn in listMccn)
                    {
                        if (GetListMachineCountOracle(mccn.FACTORY, mccn.YEAR_COUNT, mccn.MONTH_COUNT, mccn.DAY_COUNT, null).Count() > 0)
                        {
                            //Update machine count
                            if (!UpateMachineCountOracle(mccn, oraTran, oraCon))
                            {
                                oraTran.Rollback();
                                return false;
                            }
                        }
                        else
                        {
                            if (!InsertMachineCountOracle(mccn, oraTran, oraCon))
                            {
                                oraTran.Rollback();
                                return false;
                            }
                        }
                    }

                    oraTran.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    oraTran.Rollback();
                    return false;
                }
            }
        }
        #endregion

        /// <summary>
        /// Get list machine count
        /// </summary>
        /// <param name="serverNo"></param>
        /// <param name="factory"></param>
        /// <param name="yyyy"></param>
        /// <param name="mm"></param>
        /// <param name="dd"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Mccn> GetListMachineCount(string factory, string yyyy, string mm, string dd, string serverNo)
        {
            //Get list machine count with server No
            string strSql1 = @"  SELECT MCC.*
                                FROM T_CM_CSDT CSD 
                                     JOIN T_MX_MCCN MCC ON MCC.FACTORY = CSD.FACTORY
                                WHERE MCC.YEAR_COUNT = ?P_YEAR_COUNT 
                                    AND MCC.MONTH_COUNT = ?P_MONTH_COUNT AND MCC.DAY_COUNT = ?P_DAY_COUNT 
                                    AND CSD.SERVERNO = ?P_SERVERNO ;  ";

            //Get list machine count without server No
            string strSql2 = @"SELECT * FROM T_MX_MCCN MCC
                               WHERE MCC.FACTORY  = ?P_FACTORY AND MCC.YEAR_COUNT = ?P_YEAR_COUNT 
                                    AND MCC.MONTH_COUNT = ?P_MONTH_COUNT AND MCC.DAY_COUNT = ?P_DAY_COUNT;";

            var strSql = strSql2;

            var param = new List<MySqlParameter>
            {
                //new MySqlParameter("P_FACTORY", factory),
                new MySqlParameter("P_YEAR_COUNT", yyyy),
                new MySqlParameter("P_MONTH_COUNT", mm),
                new MySqlParameter("P_DAY_COUNT", dd)
            };

            if (!string.IsNullOrEmpty(serverNo))
            {
                strSql = strSql1;
                param.Add(new MySqlParameter("P_SERVERNO", serverNo));
            }
            else
            {
                param.Insert(0, new MySqlParameter("P_FACTORY", factory));
            }

            var listMccn = MySqlDBManager.GetAll<Mccn>(strSql, CommandType.Text, param.ToArray());

            return listMccn;
        }

        /// <summary>
        /// Get max machine count by month
        /// </summary>
        /// <param name="serverNo"></param>
        /// <param name="yyyy"></param>
        /// <param name="mm"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Mccn> GetMaxMachineCountByMonth(string serverNo, string yyyy, string mm)
        {
            //Get list machine count without server No
            string strSql = @"SELECT MCC.FACTORY, MAX(MCC.MACHINE_COUNT_DGS) MACHINE_COUNT_DGS, MAX(MCC.MACHINE_COUNT_PKG) MACHINE_COUNT_PKG
		                                , MAX(MCC.MACHINE_COUNT_MES) MACHINE_COUNT_MES
                                 FROM T_MX_MCCN MCC
	                                JOIN T_CM_CSDT CSD ON CSD.FACTORY = MCC.FACTORY
                                WHERE CSD.SERVERNO = ?P_SERVERNO AND MCC.YEAR_COUNT = ?P_YEAR_COUNT AND MCC.MONTH_COUNT = ?P_MONTH_COUNT
                                 GROUP BY MCC.FACTORY, MCC.YEAR_COUNT, MCC.MONTH_COUNT; ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_SERVERNO", serverNo),
                new MySqlParameter("P_YEAR_COUNT", yyyy),
                new MySqlParameter("P_MONTH_COUNT", mm)
            };
                      
            var listMccn = MySqlDBManager.GetAll<Mccn>(strSql, CommandType.Text, param.ToArray());

            return listMccn;
        }

        /// <summary>
        /// Get list machine count by week.
        /// </summary>
        /// <param name="serverNo"></param>
        /// <param name="yyyy"></param>
        /// <param name="weekNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Mccn> GetMaxMachineCountByWeek(string serverNo, string yyyy, string weekNo)
        {
            //Get list machine count without server No
            string strSql = @"SELECT MCC.FACTORY, MAX(MCC.MACHINE_COUNT_DGS) MACHINE_COUNT_DGS, MAX(MCC.MACHINE_COUNT_PKG) MACHINE_COUNT_PKG
		                                , MAX(MCC.MACHINE_COUNT_MES) MACHINE_COUNT_MES
                                 FROM T_MX_MCCN MCC
	                                JOIN T_CM_CSDT CSD ON CSD.FACTORY = MCC.FACTORY
                                WHERE CSD.SERVERNO = ?P_SERVERNO AND MCC.YEAR_COUNT = ?P_YEAR_COUNT AND MCC.WEEKNO = ?P_WEEKNO
                                 GROUP BY MCC.FACTORY, MCC.YEAR_COUNT, MCC.WEEKNO; ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_SERVERNO", serverNo),
                new MySqlParameter("P_YEAR_COUNT", yyyy),
                new MySqlParameter("P_WEEKNO", weekNo)
            };

            var listMccn = MySqlDBManager.GetAll<Mccn>(strSql, CommandType.Text, param.ToArray());

            return listMccn;
        }

        /// <summary>
        /// Insert machine count
        /// </summary>
        /// <param name="mccn"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertMachineCount(Mccn mccn, MySqlTransaction myTrans, MySqlConnection myCon)
        {
            string strSql = @" INSERT INTO T_MX_MCCN(FACTORY, YEAR_COUNT, MONTH_COUNT, DAY_COUNT, WEEKNO, MACHINE_COUNT_DGS, MACHINE_COUNT_PKG, MACHINE_COUNT_MES, UPDATE_DATE)
                                 VALUES (?P_FACTORY, ?P_YEAR_COUNT, ?P_MONTH_COUNT, ?P_DAY_COUNT, ?P_WEEKNO, ?P_MACHINE_COUNT_DGS, ?P_MACHINE_COUNT_PKG, ?P_MACHINE_COUNT_MES, SYSDATE());  ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_FACTORY", mccn.FACTORY),
                new MySqlParameter("P_YEAR_COUNT", mccn.YEAR_COUNT),
                new MySqlParameter("P_MONTH_COUNT", mccn.MONTH_COUNT),
                new MySqlParameter("P_DAY_COUNT", mccn.DAY_COUNT),
                new MySqlParameter("P_WEEKNO", mccn.WEEKNO),
                new MySqlParameter("P_MACHINE_COUNT_DGS", mccn.MACHINE_COUNT_DGS),
                new MySqlParameter("P_MACHINE_COUNT_PKG", mccn.MACHINE_COUNT_PKG),
                new MySqlParameter("P_MACHINE_COUNT_MES", mccn.MACHINE_COUNT_MES)
            };

            var resIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, param.ToArray(), CommandType.Text, myTrans, myCon);

            return resIns != null;
        }

        public static bool UpateMachineCount(Mccn mccn, MySqlTransaction myTrans, MySqlConnection myCon)
        {
            string strSql = @"UPDATE T_MX_MCCN SET MACHINE_COUNT_DGS = ?P_MACHINE_COUNT_DGS, MACHINE_COUNT_PKG = ?P_MACHINE_COUNT_PKG
		                        , MACHINE_COUNT_MES = ?P_MACHINE_COUNT_MES, UPDATE_DATE = SYSDATE() WHERE FACTORY = ?P_FACTORY AND YEAR_COUNT = ?P_YEAR_COUNT AND MONTH_COUNT = ?P_MONTH_COUNT AND DAY_COUNT = ?P_DAY_COUNT  ;";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_FACTORY", mccn.FACTORY),
                new MySqlParameter("P_YEAR_COUNT", mccn.YEAR_COUNT),
                new MySqlParameter("P_MONTH_COUNT", mccn.MONTH_COUNT),
                new MySqlParameter("P_DAY_COUNT", mccn.DAY_COUNT),
                new MySqlParameter("P_MACHINE_COUNT_DGS", mccn.MACHINE_COUNT_DGS),
                new MySqlParameter("P_MACHINE_COUNT_PKG", mccn.MACHINE_COUNT_PKG),
                new MySqlParameter("P_MACHINE_COUNT_MES", mccn.MACHINE_COUNT_MES)
            };

            var resIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, param.ToArray(), CommandType.Text, myTrans, myCon);

            return resIns != null;
        }

        /// <summary>
        /// Insert list of machine count
        /// </summary>
        /// <param name="listMccn"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertListMachineCount(List<Mccn> listMccn)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    foreach (var mccn in listMccn)
                    {
                        if(GetListMachineCount(mccn.FACTORY, mccn.YEAR_COUNT, mccn.MONTH_COUNT, mccn.DAY_COUNT, null).Count() > 0)
                        {
                            //Update machine count
                            if(!UpateMachineCount(mccn, myTrans, myConnection))
                            {
                                myTrans.Rollback();
                                return false;
                            }
                        }
                        else
                        {
                            if (!InsertMachineCount(mccn, myTrans, myConnection))
                            {
                                myTrans.Rollback();
                                return false;
                            }
                        }                        
                    }

                    //Insert list machine count to MES oracle
                    if (!InsertListMachineCountOracle(listMccn))
                    {
                        myTrans.Rollback();
                        return false;
                    }

                    myTrans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    myTrans.Rollback();
                    return false;
                }
            }
        }
    }
}
