using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using OPS_Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public class DcmtBus
    {
        /// <summary>
        /// Get list of company in corporation
        /// </summary>
        /// <returns></returns>
        public static List<Dcmt> GetCompanyListCoporation()
        {
            string strSql = @" SELECT * FROM T_CM_DCMT WHERE DEPTLEVEL = '01' AND USESTATUS = 'Y' AND DEPTCODE <> '1005' ORDER BY DEPTCODE ";

            //var lstDcmt = OracleDbManager.GetObjects<Dcmt>(strSql, CommandType.Text, null);

            var lstDcmt = MySqlDBManager.GetObjects<Dcmt>(strSql, CommandType.Text, null);

            return lstDcmt;

        }
    }
}
