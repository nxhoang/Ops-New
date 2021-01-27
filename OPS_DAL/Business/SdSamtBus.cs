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
    public class SdSamtBus
    {
        /// <summary>
        /// Get list modules mbom
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<SdSamt> GetLinkedMbom(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            string strSql = @"SELECT LNK.* , SAM.MODULENAME
                            FROM T_SD_SAMT LNK
                                JOIN T_00_SAMT SAM ON SAM.STYLECODE = LNK.STYLECODE
                            WHERE LNK.STYLECODE = :P_STYLECODE AND LNK.STYLESIZE = :P_STYLESIZE AND LNK.STYLECOLORSERIAL = :P_STYLECOLORSERIAL AND LNK.REVNO = :P_REVNO";

            List<OracleParameter> oraParam = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLESIZE", styleSize),
                new OracleParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new OracleParameter("P_REVNO", revNo)
            };
            var listMdlMbom = OracleDbManager.GetObjectsByType<SdSamt>(strSql, CommandType.Text, oraParam.ToArray());

            return listMdlMbom;
        }

        /// <summary>
        /// Insert linked module bom
        /// </summary>
        /// <param name="mdlMbom"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        private static bool InsertLinkedMbomsTrans(SdSamt linkedMbom, OracleConnection oraConn, OracleTransaction oraTrans)
        {
            string strSql = @" INSERT INTO T_SD_SAMT (STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, MODULEID, LINKED, REGISTRAR, REGISTRYDATE)
                                VALUES(:P_STYLECODE, :P_STYLESIZE, :P_STYLECOLORSERIAL, :P_REVNO, :P_MODULEID, :P_LINKED, :P_REGISTRAR, SYSDATE)";


            var param = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", linkedMbom.StyleCode),
                new OracleParameter("P_STYLESIZE", linkedMbom.StyleSize),
                new OracleParameter("P_STYLECOLORSERIAL", linkedMbom.StyleColorSerial),
                new OracleParameter("P_REVNO", linkedMbom.RevNo),
                new OracleParameter("P_MODULEID", linkedMbom.ModuleId),
                new OracleParameter("P_LINKED", linkedMbom.Linked),
                new OracleParameter("P_REGISTRAR", linkedMbom.Registrar)
            };

            var blIns = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text, oraTrans, oraConn);

            return blIns != null;
        }

        /// <summary>
        /// Delete linked mboms
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="oraConn"></param>
        /// <param name="oraTrans"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        private static bool DeleteLinkedMbomsTrans(string styleCode, string styleSize, string styleColorSerial, string revNo, OracleConnection oraConn, OracleTransaction oraTrans)
        {
            string strSql = @"DELETE FROM T_SD_SAMT WHERE STYLECODE = :P_STYLECODE AND STYLESIZE = :P_STYLESIZE AND STYLECOLORSERIAL = :P_STYLECOLORSERIAL AND REVNO = :P_REVNO";


            var param = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLESIZE", styleSize),
                new OracleParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new OracleParameter("P_REVNO", revNo)
            };

            var blDel = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text, oraTrans, oraConn);

            return blDel != null;
        }

        /// <summary>
        /// Insert list linked mboms
        /// </summary>
        /// <param name="linkedMboms"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertLinkedMboms(List<SdSamt> linkedMboms)
        {
            using (var oraConn = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                oraConn.Open();
                var oraTrans = oraConn.BeginTransaction();
                try
                {
                    //Delete linked modules
                    DeleteLinkedMbomsTrans(linkedMboms[0].StyleCode, linkedMboms[0].StyleSize, linkedMboms[0].StyleColorSerial, linkedMboms[0].RevNo, oraConn, oraTrans);

                    //Insert list linked mbom
                    foreach (var lnkMbom in linkedMboms)
                    {
                        if(lnkMbom.Linked == "1")
                        {
                            if (!InsertLinkedMbomsTrans(lnkMbom, oraConn, oraTrans))
                            {
                                oraTrans.Rollback();
                                return false;
                            }
                        }                        
                    }

                    oraTrans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    oraTrans.Rollback();
                    return false;
                }
            }
        }
    }
}
