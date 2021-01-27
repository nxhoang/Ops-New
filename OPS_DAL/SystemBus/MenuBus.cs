using System.Collections.Generic;
using System.Data;
using System.Text;
using OPS_DAL.DAL;
using OPS_DAL.SystemEntities;
using OPS_Utils;

namespace OPS_DAL.SystemBus
{
    public class MenuBus
    {
        public static string OdpConnStrOld = System.Configuration.ConfigurationManager.ConnectionStrings["OdpConnStr"].ConnectionString;

        public static List<Menu> GetSystemMenuList(string DBType)
        {
            List<Menu> SystemMenuList = new List<Menu>();

            const string sql =
                " Select System_ID , Menu_ID , Menu_Name " +
                " From T_CM_MENU " +
                " Order by System_ID , Menu_ID ";

            switch (DBType.ToLower())
            {
                case "oracle":
                    {
                        SystemMenuList = OracleDbManager.GetObjects<Menu>(sql, CommandType.Text, null);
                    }
                    break;

                case "mysql":
                    {
                        SystemMenuList = MySqlDBManager.GetObjects<Menu>(sql, CommandType.Text, null);
                    }
                    break;
            }

            return SystemMenuList;
        }
    }
}

