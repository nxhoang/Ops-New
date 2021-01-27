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
    public class OpmtAPIBus
    {
        /// <summary>
        /// Get operation master
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="edition"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<OpmtAPI> GetOpMaster(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string edition)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            List<OracleParameter> oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE",styleCode.ToUpper()),
                new OracleParameter("P_STYLESIZE", styleSize.ToUpper()),
                new OracleParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new OracleParameter("P_REVNO", revNo),
                new OracleParameter("P_OPREVNO", opRevNo),
                new OracleParameter("P_EDITION", edition.ToUpper()),
                cursor
            };
            var lstStyleMaster = OracleDbManager.GetObjects<OpmtAPI>("SP_OPS_GETOPMASTER_API_OPMT", CommandType.StoredProcedure, oracleParams.ToArray());
            
            return lstStyleMaster;
        }
    }
}
