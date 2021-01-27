using OPS_DAL.DAL;
using OPS_DAL.Entities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Business
{
    public class OnamBus
    {
        /// <summary>
        /// Get process name template
        /// </summary>
        /// <param name="actionCode"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Onam> GetProcessTempalte(string actionCode, string lanId)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_ACTIONCODE", actionCode),
                new OracleParameter("P_LANGUAGEID", lanId?.ToLower()),
                new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }

            };

            var lstOnam = OracleDbManager.GetObjects<Onam>("SP_OPS_GETPRONAMETEMP_ONAM", CommandType.StoredProcedure, oracleParams.ToArray());

            return lstOnam;
            
        }
    }
}
