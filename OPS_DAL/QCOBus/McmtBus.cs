using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OPS_DAL.QCOEntities; 
using OPS_DAL.DAL;


namespace OPS_DAL.QCOBus
{
    public class McmtBus
    {
        public static List<Mcmt> GetAOTypeList()
        {
            var strSQL = "SELECT * FROM T_CM_MCMT WHERE M_CODE = 'ADType' AND S_Code <> '000' ORDER BY S_CODE ";
            var lstMcmt = OracleDbManager.GetObjects<Mcmt>(strSQL, CommandType.Text, null);

            return lstMcmt;
        } 
    }
}
