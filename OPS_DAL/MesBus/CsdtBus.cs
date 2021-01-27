using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public class CsdtBus
    {
        /// <summary>
        /// Get list of factory in coporate setup detail table
        /// </summary>
        /// <param name="serverNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Csdt> GetListFactories(string serverNo)
        {
            string strSql = @" select * from T_CM_csdt where SERVERNO = ?P_SERVERNO ;  ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_SERVERNO", serverNo)
            };

            var listCsdt = MySqlDBManager.GetObjects<Csdt>(strSql, CommandType.Text, param.ToArray());

            return listCsdt;

        }
    }
}
