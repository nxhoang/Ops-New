using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Business
{
    public class SgmcBus
    {
        /// <summary>
        /// Get list of machine by style group
        /// </summary>
        /// <param name="styleGroup"></param>
        /// <param name="subGroup"></param>
        /// <param name="subSubGroup"></param>
        /// <returns></returns>
        public static List<Sgmc> GetStyleGroupMachines(string styleGroup, string subGroup, string subSubGroup)
        {
            string strSql = @"SELECT SGM.STYLEGROUP, SGM.SUBGROUP, SGM.SUBSUBGROUP, SGM.MACHINEID, SGM.REGISTERID, SGM.REGISTRYDATE
                                    , CASE WHEN SGM.MAINMACHINE = '0' THEN '' ELSE 'Y' END MAINMACHINE
                                    , MC1.CODE_NAME AS STYLEGROUPNAME, MC2.CODE_NAME AS SUBGROUPNAME, MC2.CODE_NAME SUBSUBGROUPNAME, USM.NAME AS REGISTERNAME, OTM.ITEMNAME MACHINENAME
                            FROM T_00_SGMC SGM
                                JOIN T_CM_MCMT MC1 ON MC1.S_CODE = SGM.STYLEGROUP AND MC1.M_CODE = 'StyleGroup'
                                LEFT JOIN T_CM_MCMT MC2 ON MC2.S_CODE = SGM.SUBGROUP AND MC2.M_CODE = 'StyleSubGroup'
                                LEFT JOIN T_CM_MCMT MC3 ON MC3.S_CODE = SGM.SUBSUBGROUP AND MC3.M_CODE = 'StyleSubSubGroup'
                                LEFT JOIN T_CM_USMT USM ON USM.USERID = SGM.REGISTERID 
                                LEFT JOIN T_OP_OTMT OTM ON OTM.ITEMCODE = SGM.MACHINEID
                            WHERE STYLEGROUP = :P_STYLEGROUP ";

            string subGroupCon = "AND SUBGROUP = :P_SUBGROUP";
            string subSubGroupCon = "AND SUBSUBGROUP = :P_SUBSUBGROUP";

            var oraParams = new List<OracleParameter>()
            {
               new OracleParameter("P_STYLEGROUP", styleGroup)
            };

            if (!string.IsNullOrWhiteSpace(subGroup) && subGroup != "000")
            {
                strSql += subGroupCon;
                oraParams.Add(new OracleParameter("P_SUBGROUP", subGroup));
            }

            if (!string.IsNullOrWhiteSpace(subSubGroup) && subSubGroup != "000")
            {
                strSql += subSubGroupCon;
                oraParams.Add(new OracleParameter("P_SUBSUBGROUP", subSubGroupCon));
            }
                        
            var listSgmc = OracleDbManager.GetObjects<Sgmc>(strSql, CommandType.Text, oraParams.ToArray());

            return listSgmc;
        }

        /// <summary>
        /// Insert style group machine to database
        /// </summary>
        /// <param name="sgmc"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertStyleGroupMachine(Sgmc sgmc, OracleConnection oraConn, OracleTransaction oraTrans)
        {

            string sql = @"  INSERT INTO T_00_SGMC (STYLEGROUP, SUBGROUP, SUBSUBGROUP, MACHINEID, MAINMACHINE, REGISTERID, REGISTRYDATE)
                            VALUES(:P_STYLEGROUP, :P_SUBGROUP, :P_SUBSUBGROUP, :P_MACHINEID, :P_MAINMACHINE, :P_REGISTERID, SYSDATE)";
            var prams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLEGROUP",sgmc.STYLEGROUP),
                new OracleParameter("P_SUBGROUP", sgmc.SUBGROUP),
                new OracleParameter("P_SUBSUBGROUP", sgmc.SUBSUBGROUP),
                new OracleParameter("P_MACHINEID", sgmc.MACHINEID),
                new OracleParameter("P_MAINMACHINE", sgmc.MAINMACHINE),
                new OracleParameter("P_REGISTERID", sgmc.REGISTERID)
            };
            var resIns = OracleDbManager.ExecuteQuery(sql, prams.ToArray(), CommandType.Text, oraTrans, oraConn);
            return resIns != null;
        }

        /// <summary>
        /// Insert list of style group machine
        /// </summary>
        /// <param name="listSgmc"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertListStlGroupMachine(List<Sgmc> listSgmc)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    foreach (var sgmc in listSgmc)
                    {
                        if (!InsertStyleGroupMachine(sgmc, connection, trans))
                        {
                            trans.Rollback();
                            return false;
                        }
                    }
                    
                    trans.Commit();
                    return true;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
    }
}
