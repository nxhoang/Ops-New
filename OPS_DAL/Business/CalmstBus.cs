using OPS_DAL.DAL;
using OPS_DAL.Entities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Business
{
    public class CalmstBus
    {
        public static List<Calmst> GetFacWorkingTimeSheet(string factory, string yyMM)
        {
            string strSql = @" Select * From MT_CALMST_TBL@MTOPSDB where FATOY = :P_FATOY and mothno = :P_MOTHNO ";

            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_FATOY", factory),
                new OracleParameter("P_MOTHNO", yyMM)
            };

            var listCalmst = OracleDbManager.GetObjects<Calmst>(strSql, oraParams.ToArray());
            return listCalmst;
        }

        /// <summary>
        /// Get lines
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="yyMM"></param>
        /// <returns></returns>
        public static List<Lwtmw> GetLines(string factory, string yyMM)
        {
            string strSql = @" select distinct cal.lineno, lin.Fyear as lineName
                               From MT_CALMST_TBL@MTOPSDB cal
                                    join  MT_FATLIN_TBL@MTOPSDB lin on lin.lineno = cal.lineno and lin.fatoy = cal.fatoy 
                               where cal.FATOY = :P_FATOY and cal.mothno = :P_MOTHNO ";

            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_FATOY", factory),
                new OracleParameter("P_MOTHNO", yyMM)
            };

            var listLwtmw = OracleDbManager.GetObjects<Lwtmw>(strSql, oraParams.ToArray());
            return listLwtmw;
        }
    }
}
