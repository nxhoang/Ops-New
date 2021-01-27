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
    public class FawkBus
    {
        #region Oracle

        #region MTOP
        /// <summary>
        /// Get the number of woker by factory and month and year
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="yyyymm"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Fawk GetFactoryWorkerFromMtop(string factory, string yyyymm)
        {
            //Get working time of 31 days
            string strSql = @"SELECT FATOY AS FACTORY, SUBSTR(MOTHNO, 1,4) AS PLANYEAR, SUBSTR(MOTHNO, 5, 2) AS PLANMONTH, WORKER, SEWER 
                              FROM MT_FATWRKR_TBL@MTOPSDB WHERE FATOY = :P_FATOY AND MOTHNO = :P_MOTHNO ";

            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_FATOY", factory),
                new OracleParameter("P_MOTHNO", yyyymm.Replace("/",""))
            };

            var listFawk = OracleDbManager.GetObjectsByType<Fawk>(strSql, CommandType.Text, oraParams.ToArray()).FirstOrDefault();
            return listFawk;
        }
        #endregion

        #region MES
        /// <summary>
        /// Insert factory worker to MES
        /// </summary>
        /// <param name="fawk"></param>
        /// <param name="oraTrans"></param>
        /// <param name="oraCon"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        private static bool InsertFactoryWorkerToMES(Fawk fawk, OracleTransaction oraTrans, OracleConnection oraCon)
        {
            string strSql = @" INSERT INTO PKMES.T_MX_FAWK (FACTORY, PLANYEAR, PLANMONTH, WORKER, SEWER, DIRECTWORKER, INDIRECTWORKER)
                                VALUES (:P_FACTORY, :P_PLANYEAR, :P_PLANMONTH, :P_WORKER, :P_SEWER, :P_DIRECTWORKER, :P_INDIRECTWORKER) ";

            var param = new List<OracleParameter>()
            {
                new OracleParameter("P_FACTORY", fawk.FACTORY),
                new OracleParameter("P_PLANYEAR", fawk.PLANYEAR),
                new OracleParameter("P_PLANMONTH", fawk.PLANMONTH),
                new OracleParameter("P_WORKER", fawk.WORKER),
                new OracleParameter("P_SEWER", fawk.SEWER),
                new OracleParameter("P_DIRECTWORKER", fawk.DIRECTWORKER),
                new OracleParameter("P_INDIRECTWORKER", fawk.INDIRECTWORKER)
            };

            var resIns = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text, oraTrans, oraCon);

            return resIns != null;
        }

        /// <summary>
        /// Update factory worker
        /// </summary>
        /// <param name="fawk"></param>
        /// <param name="oraTrans"></param>
        /// <param name="oraCon"></param>
        /// <returns></returns>
        public static bool UpdateFactoryWorkerToMES(Fawk fawk)
        {
            string strSql = @" UPDATE PKMES.T_MX_FAWK SET WORKER = :P_WORKER, SEWER = :P_SEWER, DIRECTWORKER = :P_DIRECTWORKER, INDIRECTWORKER = :P_INDIRECTWORKER WHERE FACTORY = :P_FACTORY AND PLANYEAR = :P_PLANYEAR AND PLANMONTH = :P_PLANMONTH";

            var param = new List<OracleParameter>()
            {
                new OracleParameter("P_WORKER", fawk.WORKER),
                new OracleParameter("P_SEWER", fawk.SEWER),
                new OracleParameter("P_DIRECTWORKER", fawk.DIRECTWORKER),
                new OracleParameter("P_INDIRECTWORKER", fawk.INDIRECTWORKER),
                new OracleParameter("P_FACTORY", fawk.FACTORY),
                new OracleParameter("P_PLANYEAR", fawk.PLANYEAR),
                new OracleParameter("P_PLANMONTH", fawk.PLANMONTH)
            };

            var resUpd = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text);

            return resUpd != null;
        }

        /// <summary>
        /// Insert Factory worker to MES database
        /// </summary>
        /// <param name="fawk"></param>
        /// <returns></returns>
        /// Author: Nguyen Cao Son
        public static bool InsertFactoryWorkerToMES(Fawk fawk)
        {
            using (var oraConn = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                oraConn.Open();
                var oraTrans = oraConn.BeginTransaction();
                try
                {
                    if (!InsertFactoryWorkerToMES(fawk, oraTrans, oraConn))
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
        /// Get factory worker from MES
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="yyyy"></param>
        /// <param name="mm"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Fawk GetFactoryWorkerFromMES(string factory, string yyyy, string mm)
        {
            string strSql = @"SELECT * FROM PKMES.T_MX_FAWK
                                WHERE FACTORY = :P_FACTORY AND PLANYEAR = :P_PLANYEAR AND PLANMONTH = :P_PLANMONTH ";

            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_FACTORY", factory),
                new OracleParameter("PLANYEAR", yyyy),
                new OracleParameter("P_PLANMONTH", mm),
            };

            var listFawk = OracleDbManager.GetObjectsByType<Fawk>(strSql, CommandType.Text, oraParams.ToArray()).FirstOrDefault();
            return listFawk;
        }
        #endregion

        #endregion

    }
}
