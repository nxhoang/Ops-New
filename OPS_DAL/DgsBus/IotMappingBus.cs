using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.DgsEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.DgsBus
{
    public class IotMappingBus
    {
        /// <summary>
        /// Get list of Iot
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static List<IotMapping> GetListIotDgs(string factory)
        {           
            string strSql = @" select * from t_dg_mchn_iot_mapping where factory = ?p_factory; ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("p_factory", factory)
            };

            var listIot = MySqlDBManager.GetObjectsDgs<IotMapping>(strSql, CommandType.Text, param.ToArray());

            return listIot;
        }
    }
}
