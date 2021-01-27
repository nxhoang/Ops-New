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
    public class UfmtBus
    {
        /// <summary>
        /// Get list of videos from server.
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="serverLink"></param>
        /// <returns></returns>
        public static List<Ufmt> GetVideoLinks(string styleCode, string styleSize, string styleColorSerial, string revNo, string videoServer)
        {
            var oracleParams = new OpsOracleParams(styleCode, styleSize, styleColorSerial, revNo)
            {
                new OracleParameter("P_VIDEOSERVERLINK", videoServer),               
                new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };

            var lstVideoLinks = OracleDbManager.GetObjects<Ufmt>("SP_OPS_GETVIDEOLINKS_UFMT", CommandType.StoredProcedure, oracleParams.ToArray());

            return lstVideoLinks;
        }

        //START ADD: HA
        public static List<Ufmt> GetVideoByStylecode (string stylecode)
        {
            var strSql = @"select * from T_PB_UFMT where SUBSTR(CORPORATION,1,7) = :P_STYLECODE";

            var oracleParams = new List<OracleParameter> {
                new OracleParameter("P_STYLECODE", stylecode)
            };

            return OracleDbManager.GetObjects<Ufmt>(strSql, oracleParams.ToArray());
        }
        //END ADD: HA
    }
}
