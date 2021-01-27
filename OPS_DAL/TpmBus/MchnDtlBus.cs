using OPS_DAL.DAL;
using OPS_DAL.TpmEntities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.TpmBus
{
    public class MchnDtlBus
    {
        /// <summary>
        /// Get list of machines by factory
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<MchnDtl> GetListMachineTPM(string factory, bool getIot)
        {
            string strSql = @" SELECT MST.MCHN_MST_CD, MST.MCHN_MST_NM  
                                    , DTL.MCHN_DTL_CD, MCHN_DTL_NM, MAC 
                                FROM PKTPM.T_TP_MCHN_MST MST 
                                    JOIN PKTPM.T_TP_MCHN_DTL DTL ON DTL.MCHN_MST_CD = MST.MCHN_MST_CD 
                                WHERE MST.DEL_YN = 'N' AND DTL.FCTR_CD = :P_FCTR_CD ";

            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_FCTR_CD", factory)
            };

            if (getIot)
            {
                strSql += " AND  MAC IS NOT NULL ";
            }

            var listMchnDtl = OracleDbManager.GetObjectsByType<MchnDtl>(strSql, CommandType.Text, oraParams.ToArray());

            return listMchnDtl;
        }
    }
}
