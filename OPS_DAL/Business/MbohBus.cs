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
    public class MbohBus
    {
        /// <summary>
        /// Get list modules bom header
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Mboh> GetMbomsHeader(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            string strSql = @"SELECT * FROM T_SD_MBOH MBO 
                            WHERE MBO.STYLECODE = :P_STYLECODE AND MBO.STYLESIZE = :P_STYLESIZE AND MBO.STYLECOLORSERIAL = :P_STYLECOLORSERIAL AND MBO.REVNO = :P_REVNO";

            List<OracleParameter> oraParam = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLESIZE", styleSize),
                new OracleParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new OracleParameter("P_REVNO", revNo)
            };
            var listMdlMbom = OracleDbManager.GetObjectsByType<Mboh>(strSql, CommandType.Text, oraParam.ToArray());

            return listMdlMbom;
        }

        /// <summary>
        /// Insert mbom header
        /// </summary>
        /// <param name="mboh"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertMBOMHeader(Mboh mboh, OracleTransaction oraTrans, OracleConnection oraCon)
        {
            var strSql = @"INSERT INTO T_SD_MBOH (STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, MODULEID, OUTSOURCE, LINKED) 
                        VALUES (:P_STYLECODE, :P_STYLESIZE, :P_STYLECOLORSERIAL, :P_REVNO, :P_MODULEID, :P_OUTSOURCE, :P_LINKED)";

            var oraParam = new List<OracleParameter>
                {
                    new OracleParameter("P_STYLECODE", mboh.STYLECODE),
                    new OracleParameter("P_STYLESIZE", mboh.STYLESIZE),
                    new OracleParameter("P_STYLECOLORSERIAL", mboh.STYLECOLORSERIAL),
                    new OracleParameter("P_REVNO", mboh.REVNO),
                    new OracleParameter("P_MODULEID", mboh.MODULEID),
                    new OracleParameter("P_OUTSOURCE", mboh.OUTSOURCE),
                    new OracleParameter("P_LINKED", mboh.LINKED)
                };

            var blIns = OracleDbManager.ExecuteQuery(strSql, oraParam.ToArray(), CommandType.Text, oraTrans, oraCon);

            return blIns != null;
        }

        /// <summary>
        /// Delete MBOM header by style code, size, color and revno
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool DeleteMBOMHeader(string styleCode, string styleSize, string styleColorSerial, string revNo, OracleTransaction oraTran, OracleConnection oraCon)
        {
            var strSql = @"DELETE FROM T_SD_MBOH WHERE STYLECODE = :P_STYLECODE AND STYLESIZE = :P_STYLESIZE AND STYLECOLORSERIAL = :P_STYLECOLORSERIAL AND REVNO = :P_REVNO";

            var oraParam = new List<OracleParameter>
                {
                    new OracleParameter("P_STYLECODE", styleCode),
                    new OracleParameter("P_STYLESIZE", styleSize),
                    new OracleParameter("P_STYLECOLORSERIAL", styleColorSerial),
                    new OracleParameter("P_REVNO", revNo)
                };

            var result = OracleDbManager.ExecuteQuery(strSql, oraParam.ToArray(), CommandType.Text, oraTran, oraCon);

            return result != null;
        }

        /// <summary>
        /// Delete mbom header then insert it again
        /// </summary>
        /// <param name="listMboh"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool DeleteAndInsertMBOMHeader(List<Mboh> listMboh)
        {
            //If there is no mbom header then return false.
            if (listMboh.Count == 0) return false;

            using (var oraCon = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                oraCon.Open();
                var oraTran = oraCon.BeginTransaction();
                try
                {
                    //Delete mbom header before insert it
                    if (DeleteMBOMHeader(listMboh[0].STYLECODE, listMboh[0].STYLESIZE, listMboh[0].STYLECOLORSERIAL, listMboh[0].REVNO, oraTran, oraCon))
                    {
                        foreach (var mboh in listMboh)
                        {
                            InsertMBOMHeader(mboh, oraTran, oraCon);
                        }
                    }
                    else
                    {
                        oraTran.Rollback();
                        return false;
                    }

                    oraTran.Commit();
                    return true;
                }
                catch (Exception)
                {
                    oraTran.Rollback();
                    throw;
                }
            }
        }
    }
}
