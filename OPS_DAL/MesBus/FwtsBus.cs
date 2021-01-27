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
    public class FwtsBus
    {
        #region Oracle

        /// <summary>
        /// Insert working time
        /// </summary>
        /// <param name="fwts"></param>
        /// <param name="oraTrans"></param>
        /// <param name="oraCon"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        private static bool InsertWorkingTimeOracle(Fwts fwts, OracleTransaction oraTrans, OracleConnection oraCon)
        {
            string strSql = @" INSERT INTO PKMES.T_MX_FWTS (FACTORY, PLANYEAR, PLANMONTH, WEEKNO, PLANDAY, MORNINGTIME, AFTERNOONTIME, OVERTIME, CREATEDATE, CREATEID, UPDATEDATE, UPDATEID)
                        VALUES(:P_FACTORY, :P_PLANYEAR, :P_PLANMONTH, :P_WEEKNO, :P_PLANDAY, :P_MORNINGTIME, :P_AFTERNOONTIME, :P_OVERTIME, :P_CREATEDATE, :P_CREATEID, :P_UPDATEDATE, :P_UPDATEID)";

            var param = new List<OracleParameter>()
            {
                new OracleParameter("P_FACTORY", fwts.FACTORY),
                new OracleParameter("P_PLANYEAR", fwts.PLANYEAR),
                new OracleParameter("P_PLANMONTH", fwts.PLANMONTH),
                new OracleParameter("P_WEEKNO", fwts.WEEKNO),
                new OracleParameter("P_PLANDAY", fwts.PLANDAY),
                new OracleParameter("P_MORNINGTIME", fwts.MORNINGTIME),
                new OracleParameter("P_AFTERNOONTIME", fwts.AFTERNOONTIME),
                new OracleParameter("P_OVERTIME", fwts.OVERTIME),
                new OracleParameter("P_CREATEDATE", fwts.CREATEDATE),
                new OracleParameter("P_CREATEID", fwts.CREATEID),
                new OracleParameter("P_UPDATEDATE", fwts.UPDATEDATE),
                new OracleParameter("P_UPDATEID", fwts.UPDATEID)
            };

            var resIns = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text, oraTrans, oraCon);

            return resIns != null;
        }

        /// <summary>
        /// Update working time sheet
        /// </summary>
        /// <param name="fwts"></param>
        /// <param name="oraTrans"></param>
        /// <param name="oraCon"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        private static bool UpdateWorkingTimeOracle(Fwts fwts, OracleTransaction oraTrans, OracleConnection oraCon)
        {
            string strSql = @" UPDATE PKMES.T_MX_FWTS 
                                SET MORNINGTIME = :P_MORNINGTIME, AFTERNOONTIME = :P_AFTERNOONTIME, OVERTIME = :P_OVERTIME
                                WHERE FACTORY = :P_FACTORY AND PLANYEAR = :P_PLANYEAR AND PLANMONTH = :P_PLANMONTH AND PLANDAY = :P_PLANDAY ";

            var param = new List<OracleParameter>()
            {
                new OracleParameter("P_MORNINGTIME", fwts.MORNINGTIME),
                new OracleParameter("P_AFTERNOONTIME", fwts.AFTERNOONTIME),
                new OracleParameter("P_OVERTIME", fwts.OVERTIME),
                new OracleParameter("P_FACTORY", fwts.FACTORY),
                new OracleParameter("P_PLANYEAR", fwts.PLANYEAR),
                new OracleParameter("P_PLANMONTH", fwts.PLANMONTH),
                new OracleParameter("P_PLANDAY", fwts.PLANDAY)
            };

            var resIns = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text, oraTrans, oraCon);

            return resIns != null;
        }

        public static bool UpdateListWorkingTimeOracle(List<Fwts> listFwts)
        {
            using (var oraConn = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                oraConn.Open();
                var oraTrans = oraConn.BeginTransaction();
                try
                {
                    foreach (var fwts in listFwts)
                    {
                        if (!UpdateWorkingTimeOracle(fwts, oraTrans, oraConn))
                        {
                            oraTrans.Rollback();
                            return false;
                        }
                    }

                    oraTrans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    oraTrans.Rollback();
                    return false;
                }
            }
        }

        /// <summary>
        /// Insert list of working time
        /// </summary>
        /// <param name="listFwts"></param>
        /// <returns></returns>
        public static bool InsertListtWorkingTimeOracle(List<Fwts> listFwts)
        {
            using (var oraConn = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                oraConn.Open();
                var oraTrans = oraConn.BeginTransaction();
                try
                {
                    foreach (var fwts in listFwts)
                    {
                        if (!InsertWorkingTimeOracle(fwts, oraTrans, oraConn))
                        {
                            oraTrans.Rollback();
                            return false;
                        }
                    }

                    oraTrans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    oraTrans.Rollback();
                    return false;
                }
            }
        }

        /// <summary>
        /// Get working time from Mtop
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="yyyymm"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        private static List<Fwts> GetLineWorkingTimeFromMtop(string factory, string yyyymm)
        {
            //Get working time of 31 days
            string strSql = @"SELECT * FROM (
                                    SELECT FATOY AS FACTORY, LINENO, SUBSTR(MOTHNO, 1, 4) AS PLANYEAR, SUBSTR(MOTHNO, 5, 2) AS PLANMONTH, WEEKNO,  PLNDAY AS PLANDAY, MORTME AS MORNINGTIME
                                           , ARNTME AS AFTERNOONTIME, REDTME - (MORTME + ARNTME) AS OVERTIME, CRTID AS CREATEID, CRTDAT AS CREATEDATE, UPTDAT AS UPDATEDATE,UPTID AS UPDATEID
                                    FROM MT_CALMST_TBL@MTOPSDB 
                                    WHERE FATOY LIKE UPPER('%'||:P_FATOY||'%')  AND MOTHNO = :P_YYYYMM 
                                    ORDER BY LINENO, MOTHNO, PLNDAY
                                ) WHERE ROWNUM <= 31";

            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_FATOY", factory),
                new OracleParameter("P_YYYYMM", yyyymm.Replace("/",""))
            };

            var listFlws = OracleDbManager.GetObjectsByType<Fwts>(strSql, CommandType.Text, oraParams.ToArray());
            return listFlws;
        }

        /// <summary>
        /// Filter working time of 1 line
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="yyyyMM"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Fwts> GetListWorkingTimeFromMtop(string factory, string yyyyMM)
        {
            //Get list working time sheet from Mtop
            var listFlwsMtop = GetLineWorkingTimeFromMtop(factory, yyyyMM);
            if (listFlwsMtop.Count > 0)
            {
                //Get line no
                var lineNo = listFlwsMtop[0].LINENO;
                //Get working sheet data of line no
                var newFlwsMtop = listFlwsMtop.Where(x => x.LINENO == lineNo);

                return newFlwsMtop.ToList();
            }

            return listFlwsMtop;
        }

        /// <summary>
        /// Get working time of factory by year and month
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="yyyy"></param>
        /// <param name="mm"></param>
        /// <returns>31 working time days</returns>
        /// Author: Son Nguyen Cao
        public static List<Fwts> GetLineWorkingTimeFactoryOraMES(string factory, string yyyy, string mm)
        {
            string strSql = @"SELECT * 
                            FROM PKMES.T_MX_FWTS
                            WHERE FACTORY  = :P_FACTORY AND PLANYEAR = :P_PLANYEAR AND PLANMONTH = :P_PLANMONTH ";

            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_FACTORY", factory),
                new OracleParameter("P_PLANYEAR", yyyy),
                new OracleParameter("P_PLANMONTH", mm)
            };

            var listFlws = OracleDbManager.GetObjectsByType<Fwts>(strSql, CommandType.Text, oraParams.ToArray());
            return listFlws;
        }

        public static List<Fwts> GetWorkingTimeOraMES(string factory, string yyyy, string mm, string dd)
        {
            string strSql = @"SELECT * 
                            FROM PKMES.T_MX_FWTS
                            WHERE FACTORY  = :P_FACTORY AND PLANYEAR = :P_PLANYEAR AND PLANMONTH = :P_PLANMONTH ";

            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_FACTORY", factory),
                new OracleParameter("P_PLANYEAR", yyyy),
                new OracleParameter("P_PLANMONTH", mm)
            };

            if (!string.IsNullOrEmpty(dd))
            {
                strSql += " AND PLANDAY = :P_PLANDAY ";
                oraParams.Add(new OracleParameter("P_PLANDAY", dd));
            }

            var listFlws = OracleDbManager.GetObjectsByType<Fwts>(strSql, CommandType.Text, oraParams.ToArray());
            return listFlws;
        }

        /// <summary>
        /// Get working time by week
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="yyyy"></param>
        /// <param name="weekNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Fwts> GetWorkingTimeByWeekOraMES(string factory, string yyyy, string weekNo)
        {
            string strSql = @"SELECT * 
                            FROM PKMES.T_MX_FWTS
                            WHERE FACTORY  = :P_FACTORY AND PLANYEAR = :P_PLANYEAR AND WEEKNO = :P_WEEKNO ";

            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_FACTORY", factory),
                new OracleParameter("P_PLANYEAR", yyyy),
                new OracleParameter("P_WEEKNO", weekNo)
            };

            var listFlws = OracleDbManager.GetObjectsByType<Fwts>(strSql, CommandType.Text, oraParams.ToArray());
            return listFlws;
        }
        
        #endregion
    }
}
