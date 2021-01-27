using System.Collections.Generic;
using System.Data;
using System.Text;
using OPS_DAL.DAL;
using OPS_DAL.SystemEntities;
using OPS_Utils;

namespace OPS_DAL.SystemBus
{
    public class SystemBus
    {
        public static string OdpConnStrOld = System.Configuration.ConfigurationManager.ConnectionStrings["OdpConnStr"].ConnectionString;

        public static List<SystemEntities.System> GetSystemList(string DBType)
        {
            List<SystemEntities.System> SystemList = new List<SystemEntities.System>();

            const string sql =
                " Select S_CODE AS System_ID , CODE_NAME AS System_Name " +
                " From T_CM_MCMT " +
                " Where M_CODE = 'System'  "; 

            switch (DBType.ToLower())
            {
                case "oracle":
                    {
                        SystemList= OracleDbManager.GetObjects<SystemEntities.System>(sql, CommandType.Text, null);
                    }
                    break;

                case "mysql":
                    {
                        SystemList= MySqlDBManager.GetObjects<SystemEntities.System>(sql, CommandType.Text, null);
                    }
                    break;  
            }

            return SystemList;
        }
    }
}
