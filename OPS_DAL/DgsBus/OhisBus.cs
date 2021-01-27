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
    public class OhisBus
    {
        /// <summary>
        /// Get list of machine which sent data to DGS
        /// </summary>
        /// <param name="yyyy"></param>
        /// <param name="mm"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static List<Ohis> GetListMachineDGS(string yyyy, string mm, string dd)
        {
            //string strSql = @" select distinct his.MAC_ADDR, inf.MCHN_ID 
            //                     from t_dg_iot_output_history his 
            //                      left join t_dg_iot_ntwrk_info inf on inf.IOT_MAC_ADDR = his.MAC_ADDR
            //                     where substr(his.pln_date, 1, 4) = ?P_YYYY and substr(his.pln_date, 6, 2) = ?P_MM and substr(his.pln_date, 9, 2) = ?P_DD;  ";

            string strSql = @" select distinct his.MacAddress, inf.MachineId, inf.Factory 
                                 from t_dg_iot_event_counter his 
		                                left join t_dg_mchn_iot_mapping inf on inf.MacAddress = his.MacAddress
                                 where substr(his.ExcDttm, 1, 4) = ?P_YYYY and substr(his.ExcDttm, 6, 2) = ?P_MM and substr(his.ExcDttm, 9, 2) = ?P_DD;  ";
            
            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_YYYY", yyyy),
                new MySqlParameter("P_MM", mm),
                new MySqlParameter("P_DD", dd)
            };

            var listOhis = MySqlDBManager.GetObjectsDgs<Ohis>(strSql, CommandType.Text, param.ToArray());

            return listOhis;
        }
    }
}
