using OPS_DAL.DAL;
using OPS_DAL.MtopEntities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MtopBus
{
    public class FATWRKRBus
    {
        /// <summary>
        /// Get factory working time sheet.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="yyMM"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static FATWRKR GetFacWorkers(string factory, string yyMM)
        {
            string strSql = @" Select * From MT_FATWRKR_TBL@MTOPSDB where FATOY = :P_FATOY and mothno = :P_MOTHNO ";

            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_FATOY", factory),
                new OracleParameter("P_MOTHNO", yyMM)
            };

            var fwts = OracleDbManager.GetObjects<FATWRKR>(strSql, oraParams.ToArray()).FirstOrDefault();
            return fwts;
        }
    }
}
