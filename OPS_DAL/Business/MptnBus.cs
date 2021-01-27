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
    public class MptnBus
    {
        /// <summary>
        /// Get list pattern MBOM
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        /// Date: 27/Jan/2021
        public static List<Mpnt> GetPatternsMbom(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            string strSql = @"SELECT BOMH.CADFILE, MPT.* 
                            FROM T_SD_MPTN MPT
                                JOIN T_SD_BOMH BOMH ON BOMH.STYLECODE = MPT.STYLECODE AND BOMH.STYLESIZE = MPT.STYLESIZE AND BOMH.STYLECOLORSERIAL = MPT.STYLECOLORSERIAL AND BOMH.REVNO = MPT.REVNO
                            WHERE MPT.STYLECODE = :P_STYLECODE AND MPT.STYLESIZE = :P_STYLESIZE AND MPT.STYLECOLORSERIAL = :P_STYLECOLORSERIAL AND MPT.REVNO = :P_REVNO";

            List<OracleParameter> oraParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLESIZE", styleSize),
                new OracleParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new OracleParameter("P_REVNO", revNo)
            };

            var listPatternsMbom = OracleDbManager.GetObjectsByType<Mpnt>(strSql, CommandType.Text, oraParams.ToArray());

            return listPatternsMbom;
        }
    }
}
