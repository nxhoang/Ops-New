using OPS_DAL.Entities;
using System.Collections.Generic;
using OPS_DAL.DAL;
using System.Data;

namespace OPS_DAL.Business
{
    public class BuyerBus
    {
        public static string OdpConnStrOld = System.Configuration.ConfigurationManager.ConnectionStrings["OdpConnStr"].ConnectionString;
         
        /// <summary>
        /// Get Buyer
        /// </summary>
        /// <returns></returns>
        /// Author: VitHV
        public List<Buyer> GetBuyer()
        {
            const string sql = @"select s_code BuyerCode
                                   ,code_name BuyerName
                           from t_cm_mcmt 
                           where m_code='Buyer' and s_code<>'000' order by s_code  ";
            return OracleDbManager.GetObjects<Buyer>(sql, null);
        }

        public List<Buyer> GetBuyerWithAny()
        {
            // 2019-05-31, Tai Le: (Thomas)

            const string sql =
                " select 'Any' as BuyerCode , 'Any' as BuyerName " +
                " from DUAL " +
                " UNION ALL " +
                " select s_code BuyerCode , code_name BuyerName " +
                " from t_cm_mcmt " +
                " where m_code='Buyer' " +
                " and s_code<>'000' " +
                " ";
            return OracleDbManager.GetObjects<Buyer>(OdpConnStrOld, sql, CommandType.Text, null);
        }
         
    }
}
