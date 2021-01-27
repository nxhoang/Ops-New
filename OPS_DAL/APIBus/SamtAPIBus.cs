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
    public class SamtAPIBus
    {
        /// <summary>
        /// Get new modules
        /// </summary>
        /// <param name="styleCode"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<SamtAPI> GetNewModules(string styleCode)
        {
            string strSql = @"SELECT SAM.STYLECODE, SAM.MODULEID, SAM.MODULENAME
                            FROM T_00_SAMT SAM
                                LEFT JOIN T_00_ICMT ICM ON ICM.ITEMCODE = SAM.MODULEID
                                LEFT JOIN T_00_ICLM ICL ON ICL.LEVELCODE = ICM.LEVELNO_01
                            WHERE SAM.STYLECODE = :P_STYLECODE 
                                    AND ICM.BUYER = :P_BUYER AND ICM.MAINLEVEL = 'SUB' 
                                    AND ICL.MAINLEVEL = 'SUB' AND ICL.LEVELNO = '01' AND ICL.LEVELUSE = 'Y'";

            List<OracleParameter> oraParam = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_BUYER", styleCode.Substring(0,3))

            };
            var listModule = OracleDbManager.GetObjectsByType<SamtAPI>(strSql, CommandType.Text, oraParam.ToArray());

            return listModule;
        }
    }
}
