using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OPS_DAL.SystemEntities;
using OPS_DAL.DAL;

namespace OPS_DAL.SystemBus
{
    public class SaleTeamBus
    {
        public static string OdpConnStrOld = System.Configuration.ConfigurationManager.ConnectionStrings["OdpConnStr"].ConnectionString;
        public static List<SaleTeam> GetSaleTeamList(string DBType)
        {
            List<SaleTeam> SaleTeamList = new List<SaleTeam>();

            const string sql =
                  " SELECT DISTINCT T_CM_MCMT.CODE_DESC AS TeamID ,  REPLACE( T_CM_URLM.ROLEDESC, 'Manager of ')  AS TeamDesc  " +
                  " FROM T_CM_MCMT " +
                  " INNER JOIN T_CM_URLM ON " +
                  "   T_CM_MCMT.CODE_DESC = T_CM_URLM.RoleID" +
                  " WHERE M_CODE = 'Buyer'  " +
                  "";
            //return OracleDbManager.GetObjects<SaleTeam>(OdpConnStrOld, sql, CommandType.Text, null);

            switch (DBType.ToLower())
            {
                case "oracle":
                    {
                        SaleTeamList = OracleDbManager.GetObjects<SaleTeam>(sql, CommandType.Text, null);
                    }
                    break;

                case "mysql":
                    {
                        SaleTeamList = MySqlDBManager.GetObjects<SaleTeam>(sql, CommandType.Text, null);
                    }
                    break;
            }

            return SaleTeamList;
        }
    }
}
