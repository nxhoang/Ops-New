using OPS_DAL.APIEntities;
using OPS_DAL.DAL;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.APIBus
{
    public class IcmtAPIBus
    {
        /// <summary>
        /// Get list module
        /// </summary>
        /// <param name="buyerCode"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<IcmtAPI> GetItemCodeModules(string buyerCode)
        {
            string strSql = @"SELECT ICM.ITEMCODE, ICM.ITEMNAME  
                            FROM T_00_ICMT ICM
                                JOIN T_00_ICLM ICL ON ICL.LEVELCODE = ICM.LEVELNO_01
                            WHERE ICM.BUYER = :P_BUYER AND ICM.MAINLEVEL = 'SUB' 
                                AND ICL.MAINLEVEL = 'SUB' AND ICL.LEVELNO = '01' AND ICL.LEVELUSE = 'Y'
                            ORDER BY ICM.LEVELNO_01";

            List<OracleParameter> oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_BUYER", buyerCode),

            };
            var listModule = OracleDbManager.GetObjectsByType<IcmtAPI>(strSql, CommandType.Text, oracleParams.ToArray());

            return listModule;
        }
    }
}
