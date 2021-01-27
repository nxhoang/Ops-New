using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OPS_DAL.DAL;
using OPS_DAL.QCOEntities;
using System.Globalization;
using Oracle.ManagedDataAccess.Client;
using OPS_Utils;

namespace OPS_DAL.QCOBus
{
    public class FLWSBus
    {
        public static List<FLWS> GetFactoryLineWS(string pFactory)
        {
            var strSQL = "Select * From T_CM_FLWS Where 1=1 ";

            if (!String.IsNullOrEmpty(pFactory))
                strSQL = strSQL + " AND FACTORY = '" + pFactory + "' ";

            var lstFcmt = OracleDbManager.GetObjects<FLWS>(strSQL, CommandType.Text, null);

            return lstFcmt;
        }

        public static void ImportWorkingSheetFromMTOPS()
        {
            var strSQL =
                " SELECT A.* , A.MOTHNO || A.PLANDAY AS FULLDATE " +
                " FROM PKAMT.MT_CALMST_TBL@MTOPSDB A " +
                " ORDER BY A.FATOY , A.LINENO , A.MOTHNO , A.PLANDAY ";

            var _dt = OPS_DAL.DAL.OracleDbManager.Query(strSQL, null, OPS_Utils.ConstantGeneric.ConnectionStr);

            string Msg = "", AccumMsg = "";
            if (_dt != null)
            {
                foreach (DataRow dr in _dt.Rows)
                {
                    var Factory = dr["FATOY"].ToString();
                    var LINENO = dr["LINENO"].ToString();
                    var MOTHNO = dr["MOTHNO"].ToString();
                    var Day = dr["PLNDAY"].ToString();

                    var MORTME = dr["MORTME"].ToString();
                    var ARNTME = dr["ARNTME"].ToString();
                    var OVRTME = dr["OVRTME"].ToString();

                    var FULLDATE = dr["FULLDATE"].ToString();

                    if (DateTime.ParseExact(FULLDATE, "yyyyMMdd", new CultureInfo("")) >= DateTime.Today)
                    {
                        Msg = "";
                        SyncFactoryWorksheet(Factory, LINENO, MOTHNO, Day, MORTME, ARNTME, OVRTME, out Msg);

                        if (String.IsNullOrEmpty(AccumMsg))
                            AccumMsg = Msg;
                        else
                            Msg = AccumMsg + "<BR/>" + Msg;
                    } 
                } 
            } 
        }

        private static bool SyncFactoryWorksheet(string pFactory, string pLINENO, string pMOTHNO, string pDay, string pMORTME, string pARNTME, string pOVRTME, out string pMessage)
        {
            pMessage = "";
            try
            {
                var strSQL = "";



                return true;
            }
            catch (Exception ex)
            {
                pMessage = "Error: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Get line working sheet from MTOP by week number
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="yyMM"></param>
        /// <param name="weekNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<FLWS> GetLineWorkingSheetFromMtop(string factory, string yyyy, string weekNo)
        {
            string strSql = @"SELECT FATOY AS FACTORY, LINENO, SUBSTR(MOTHNO, 1, 4) AS YEAR, SUBSTR(MOTHNO, 5, 2) AS MONTH, PLNDAY AS DAY, MORTME AS MORNINGTIME
                                    , ARNTME AS AFTERNOONTIME, OVRTME AS OVERTIME, CRTID AS CREATOR, CRTDAT AS CREATETIME, WEEKNO
                            FROM MT_CALMST_TBL@MTOPSDB 
                            where FATOY = :P_FATOY and MOTHNO = :P_YYYY ";

            //string strSql = @"SELECT FATOY AS FACTORY, LINENO, SUBSTR(MOTHNO, 1, 4) AS YEAR, SUBSTR(MOTHNO, 5, 2) AS MONTH, PLNDAY AS DAY, MORTME AS MORNINGTIME
            //                        , ARNTME AS AFTERNOONTIME, OVRTME AS OVERTIME, CRTID AS CREATOR, CRTDAT AS CREATETIME, WEEKNO
            //                FROM MT_CALMST_TBL@MTOPSDB 
            //                where FATOY = :P_FATOY and MOTHNO = :P_YYYY and weekno = :P_WEEKNO ";

            //string strSql = @"SELECT FATOY AS FACTORY, LINENO , CAST (SUBSTR(MOTHNO, 1, 4) AS INT)  AS YEAR, CAST ( SUBSTR(MOTHNO, 5, 2) AS INT) AS MONTH
            //                      , CAST ( PLNDAY AS INT)  AS DAY, MORTME AS MORNINGTIME
            //                    , ARNTME AS AFTERNOONTIME, OVRTME AS OVERTIME, CRTID AS CREATOR, CRTDAT AS CREATETIME, WEEKNO
            //                FROM MT_CALMST_TBL@MTOPSDB 
            //                where FATOY = :P_FATOY and SUBSTR(MOTHNO, 1, 4) = :P_YYYY and weekno = :P_WEEKNO ";


            //var oraParams = new List<OracleParameter>()
            //{
            //    new OracleParameter("P_FATOY", factory),
            //    new OracleParameter("P_YYYY", yyyy.Replace("/","")),
            //    new OracleParameter("P_WEEKNO", weekNo)
            //}; 
            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_FATOY", factory),
                new OracleParameter("P_YYYY", yyyy.Replace("/","")) 
            };

            var listFlws = OracleDbManager.GetObjectsByType<FLWS>(strSql, CommandType.Text, oraParams.ToArray());
            return listFlws;
        }
                
        /// <summary>
        /// Get line working sheet from MES
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="year"></param>
        /// <param name="weekNo"></param>
        /// <returns></returns>
        public static List<FLWS> GetLineWorkingSheetFromMES(string factory, string year, string weekNo)
        {           
            string strSql = @"SELECT * FROM PKMES.T_CM_FLWS WHERE FACTORY = :P_FACTORY AND YEAR = :P_YEAR AND WEEKNO = :P_WEEKNO ";

            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_FACTORY", factory),
                new OracleParameter("P_YEAR", year),
                new OracleParameter("P_WEEKNO", weekNo)
            };

            var listFlws = OracleDbManager.GetObjectsByType<FLWS>(strSql, CommandType.Text, oraParams.ToArray());
            return listFlws;
        }

        /// <summary>
        /// Insert working sheet to database
        /// </summary>
        /// <param name="flws"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        private static bool InsertWorkingSheet(FLWS flws, OracleTransaction oraTrans, OracleConnection oraCon)
        {
            try
            {
                string strSql = @" INSERT INTO PKMES.T_CM_FLWS(FACTORY, LINESERIAL, YEAR, MONTH, DAY, MORNINGTIME, AFTERNOONTIME, OVERTIME, CREATOR, CREATETIME, WEEKNO, WORKERS, SEWERS, MACHINES)
                            VALUES(:P_FACTORY, :P_LINESERIAL, :P_YEAR, :P_MONTH, :P_DAY, :P_MORNINGTIME, :P_AFTERNOONTIME, :P_OVERTIME, :P_CREATOR, :P_CREATETIME, :P_WEEKNO, :P_WORKERS, :P_SEWERS, :P_MACHINES)";

                var oraParams = new List<OracleParameter>()
                {
                    new OracleParameter("P_FACTORY", flws.FACTORY),
                    new OracleParameter("P_LINESERIAL", flws.LINESERIAL),
                    new OracleParameter("P_YEAR", flws.Year),
                    new OracleParameter("P_MONTH", flws.Month),
                    new OracleParameter("P_DAY", flws.Day),
                    new OracleParameter("P_MORNINGTIME", flws.MORNINGTIME),
                    new OracleParameter("P_AFTERNOONTIME", flws.AFTERNOONTIME),
                    new OracleParameter("P_OVERTIME", flws.OVERTIME),
                    new OracleParameter("P_CREATOR", flws.CREATOR),
                    new OracleParameter("P_CREATETIME", flws.CREATETIME),
                    new OracleParameter("P_WEEKNO", flws.WEEKNO),
                    new OracleParameter("P_WORKERS", flws.WORKERS),
                    new OracleParameter("P_SEWERS", flws.SEWERS),
                    new OracleParameter("P_MACHINES", flws.MACHINES)
                };

               var resIns = OracleDbManager.ExecuteQuery(strSql, oraParams.ToArray(), CommandType.Text, oraTrans, oraCon);

                return resIns != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Import list working sheet from Mtop
        /// </summary>
        /// <param name="listFlws"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool ImportWorkingSheetFromMtop(List<FLWS> listFlws)
        {
            using (var oraCon = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                oraCon.Open();
                var oraTran = oraCon.BeginTransaction();
                try
                {
                    foreach (var flws in listFlws)
                    {
                        InsertWorkingSheet(flws, oraTran, oraCon);
                    }

                    oraTran.Commit();
                    return true;
                }
                catch (Exception)
                {
                    oraTran.Rollback();
                    throw;
                }
            }
        }
    }
}
