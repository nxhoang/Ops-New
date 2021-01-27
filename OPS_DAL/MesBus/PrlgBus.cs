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
    public class PrlgBus
    {
        /// <summary>
        /// Get MES production log by mes package
        /// </summary>
        /// <param name="mxPackage"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Prlg GetMesProductionLog(string mxPackage)
        {
            //Get working time of 31 days
            string strSql = @"SELECT * FROM PKMES.T_MX_PRLG WHERE MXPACKAGE = :P_MXPACKAGE ";

            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_MXPACKAGE", mxPackage)
            };

            var proLog = OracleDbManager.GetObjectsByType<Prlg>(strSql, CommandType.Text, oraParams.ToArray()).FirstOrDefault();

            return proLog;
        }

        /// <summary>
        /// Insert mes production log
        /// </summary>
        /// <param name="prlg"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        private static bool InsertMesProductionLogWithTrans(Prlg prlg, OracleTransaction oraTran, OracleConnection oraCon)
        {
            string strSql = @" INSERT INTO PKMES.T_MX_PRLG(MXPACKAGE, MXTARGET, ACHIEVEDQTY, MACHINES, WORKERS, WORKERSOT, WORKINGHOURS, OVERTIME, REGISTERID, REGISTRYDATE) 
                                VALUES(:P_MXPACKAGE, :P_MXTARGET, :P_ACHIEVEDQTY, :P_MACHINES, :P_WORKERS, :P_WORKERSOT, :P_WORKINGHOURS, :P_OVERTIME, :P_REGISTERID, SYSDATE) ";

            var param = new List<OracleParameter>()
            {
                new OracleParameter("P_MXPACKAGE", prlg.MXPACKAGE),
                new OracleParameter("P_MXTARGET", prlg.MXTARGET),
                new OracleParameter("P_ACHIEVEDQTY", prlg.ACHIEVEDQTY),
                new OracleParameter("P_MACHINES", prlg.MACHINES),
                new OracleParameter("P_WORKERS", prlg.WORKERS),
                new OracleParameter("P_WORKERSOT", prlg.WORKERSOT),
                new OracleParameter("P_WORKINGHOURS", prlg.WORKINGHOURS),
                new OracleParameter("P_OVERTIME", prlg.OVERTIME),
                new OracleParameter("P_REGISTERID", prlg.REGISTERID)
            };

            var resIns = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text, oraTran, oraCon);

            return resIns != null;
        }

        /// <summary>
        /// Insert mes pacakge production log
        /// </summary>
        /// <param name="prlg"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertMesProductionLog(Prlg prlg)
        {
            using (var oraConn = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                oraConn.Open();
                var oraTrans = oraConn.BeginTransaction();
                try
                {
                    //Update mes production log in oracle then update mes package quantity
                    if (InsertMesProductionLogWithTrans(prlg, oraTrans, oraConn) && MpdtBus.UpdateManualMxPkgQuantityOracle(prlg.PACKAGEGROUP, prlg.SEQNO, prlg.MXPACKAGE, (int)prlg.ACHIEVEDQTY))
                    {
                        //Update package quantity 
                        if (!MpdtBus.UpdateManualMxPkgQuantity(prlg.PACKAGEGROUP, prlg.SEQNO, prlg.MXPACKAGE, (int)prlg.ACHIEVEDQTY))
                        {
                            oraTrans.Rollback();
                            return false;
                        }
                    }
                    else
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
        /// Update Mes production log
        /// </summary>
        /// <param name="prlg"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        private static bool UpdateMesProductionLogWithTrans(Prlg prlg, OracleTransaction oraTran, OracleConnection oraCon)
        {
            string strSql = @" UPDATE PKMES.T_MX_PRLG SET ACHIEVEDQTY = :P_ACHIEVEDQTY, MACHINES = :P_MACHINES, WORKERS = :P_WORKERS
                                                        , WORKERSOT = :P_WORKERSOT, WORKINGHOURS = :P_WORKINGHOURS, OVERTIME = :P_OVERTIME
                                                        , REGISTERID = :P_REGISTERID, REGISTRYDATE = SYSDATE WHERE MXPACKAGE = :P_MXPACKAGE ";

            var param = new List<OracleParameter>()
            {
                new OracleParameter("P_ACHIEVEDQTY", prlg.ACHIEVEDQTY),
                new OracleParameter("P_MACHINES", prlg.MACHINES),
                new OracleParameter("P_WORKERS", prlg.WORKERS),
                new OracleParameter("P_WORKERSOT", prlg.WORKERSOT),
                new OracleParameter("P_WORKINGHOURS", prlg.WORKINGHOURS),
                new OracleParameter("P_OVERTIME", prlg.OVERTIME),
                new OracleParameter("P_REGISTERID", prlg.REGISTERID),
                new OracleParameter("P_MXPACKAGE", prlg.MXPACKAGE)
            };

            var resIns = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text, oraTran, oraCon);

            return resIns != null;
        }

        /// <summary>
        /// Update MES production log
        /// </summary>
        /// <param name="prlg"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateMesProductionLog(Prlg prlg)
        {
            using (var oraConn = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                oraConn.Open();
                var oraTrans = oraConn.BeginTransaction();
                try
                {
                    //Update mes production log and mes package quantity in oracle
                    if (UpdateMesProductionLogWithTrans(prlg, oraTrans, oraConn)
                        && MpdtBus.UpdateManualMxPkgQuantityOracle(prlg.PACKAGEGROUP, prlg.SEQNO, prlg.MXPACKAGE, (int)prlg.ACHIEVEDQTY))

                    {
                        //Update mes package quantity in MySQL
                        if (MpdtBus.UpdateManualMxPkgQuantity(prlg.PACKAGEGROUP, prlg.SEQNO, prlg.MXPACKAGE, (int)prlg.ACHIEVEDQTY))
                        {
                            oraTrans.Rollback();
                            return false;
                        }
                    }
                    else
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
    }
}
