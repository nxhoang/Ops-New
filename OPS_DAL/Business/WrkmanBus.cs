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
    public class WrkmanBus
    {
        /// <summary>
        /// Get working machines
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="yyMM"></param>
        /// <returns></returns>
        public static List<Wrkman> GetWorkingMachines(string factory, string yyMM)
        {
            string strSql = @" Select * From MT_WRKMAN_TBL@MTOPSDB where FATOY = :P_FATOY and mothno = :P_MOTHNO ";

            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_FATOY", factory),
                new OracleParameter("P_MOTHNO", yyMM)
            };

            var listWrkman = OracleDbManager.GetObjects<Wrkman>(strSql, oraParams.ToArray());
            return listWrkman;
        }
        
    }
}
