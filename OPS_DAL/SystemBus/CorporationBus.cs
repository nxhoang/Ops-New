using System.Collections.Generic;
using System.Data;
using System.Text;
using OPS_DAL.DAL;
using OPS_DAL.SystemEntities;
using OPS_Utils;

namespace OPS_DAL.SystemBus
{
    public class CorporationBus
    {
        //public static string OdpConnStrOld = System.Configuration.ConfigurationManager.ConnectionStrings["OdpConnStr"].ConnectionString;

        public static List<Corporation> GetCorporationList()
        {
            const string sql = " SELECT S_CODE , CODE_NAME FROM T_CM_MCMT WHERE M_CODE = 'Corporation'  ORDER BY S_CODE ";
            return OracleDbManager.GetObjects<Corporation>(System.Configuration.ConfigurationManager.ConnectionStrings["OdpConnStr"].ConnectionString, sql, CommandType.Text, null);
        }
    }
}
