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
    public class OpntAPIBus
    {
        /// <summary>
        /// Get opnt
        /// </summary>
        /// <param name="edition"></param>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="opSerial"></param>
        /// <param name="opNameId"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<OpntAPI> GetSubProcess(string edition, string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, int opSerial, string opNameId, string languageId)
        {
            var oracleParams = new OpsOracleParams(edition.ToUpper(), styleCode.ToUpper(), styleSize.ToUpper(), styleColorSerial, revNo, opRevNo)
            {
                new OracleParameter("P_OPSERIAL", opSerial),
                new OracleParameter("P_OPNAMEID", opNameId),
                new OracleParameter("P_LANGUAGEID", languageId)
            };
            oracleParams.AddCursor();

            var lstOpnts = OracleDbManager.GetObjectsByType<OpntAPI>("SP_OPS_GETOPNAMEDETAILS_OPNT", CommandType.StoredProcedure, oracleParams.ToArray());

            return lstOpnts;
        }
    }
}
