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
    public class OpTimeBus
    {
        public static int CheckOpTimeExistence(OpTimeEntity opt)
        {
            var oracleParams = new OpsOracleParams(opt.StyleCode, opt.StyleSize, opt.StyleColorSerial, opt.RevNo)
            {
                new OracleParameter("p_FACTORY", opt.Factoy),
                new OracleParameter("RESULT", OracleDbType.Int16) { Direction = ParameterDirection.Output }
            };

            var resIns = OracleDbManager.ExecuteQuery("SP_OPS_UpdateOPSummary_New", oracleParams.ToArray(), CommandType.StoredProcedure);

            return int.Parse(resIns.ToString());
        }

        /// <summary>
        /// Get optime
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static OpTimeEntity GetStyleOpTime(string factory, string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            string strSql = @" SELECT * FROM T_OP_OPTIME  
                                WHERE STYLECODE = :P_STYLECODE AND STYLESIZE = :P_STYLESIZE AND STYLECOLORSERIAL = :P_STYLECOLORSERIAL 
                                        AND REVNO = :P_REVNO AND FACTORY = :P_FACTORY ";
           
            var oracleParams = new OpsOracleParams(styleCode, styleSize, styleColorSerial, revNo)
            {
                new OracleParameter("P_FACTORY", factory)
            };

            var opTime = OracleDbManager.GetObjects<OpTimeEntity>(strSql, oracleParams.ToArray()).FirstOrDefault();
            return opTime;
        }
    }
}
