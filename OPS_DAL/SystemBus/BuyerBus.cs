using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OPS_DAL.DAL;

namespace OPS_DAL.SystemBus
{
    public class BuyerBus
    {
        //public static string OdpConnStrOld = System.Configuration.ConfigurationManager.ConnectionStrings["OdpConnStr"].ConnectionString;

        public List<SystemEntities.Buyer> GetBuyerWithAny(string DBType)
        {
            // 2019-05-31, Tai Le: (Thomas)
            List<SystemEntities.Buyer> Buyers = new List<SystemEntities.Buyer>();

            const string sql =
                    " Select * " +
                    " From (Select 'Any' as BuyerCode , 'Any' as BuyerName " +
                    " from DUAL " +
                    " UNION ALL " +
                    " select UPPER(s_code) BuyerCode , UPPER(code_name) BuyerName " +
                    " from t_cm_mcmt " +
                    " where m_code='Buyer' " +
                    " and s_code<>'000' " +
                    " ) Main" +
                    " Order By Main.BuyerCode " +
                    " ";

            switch (DBType.ToLower())
            {
                case "oracle":
                    { 
                        Buyers = OracleDbManager.GetObjects<SystemEntities.Buyer>(sql, CommandType.Text, null);
                    }
                    break;

                case "mysql":
                    { 
                        Buyers = MySqlDBManager.GetObjects<SystemEntities.Buyer>( sql, CommandType.Text, null);
                    }
                    break; 
            }

            return Buyers;
        }

    }
}
