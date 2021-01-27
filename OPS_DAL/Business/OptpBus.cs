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
    public class OptpBus
    {
        //Author: HA NGUYEN
        public static List<Optp> GetProcessNameTable(string opActionCode, string opTempId, string languageId)
        {
            var strSql = @" SELECT ACTIONCODE, TEMPID, OPN.OPNAMEID, 
                                CASE
                               WHEN (:P_LANGUAGEID = 'vi' OR :P_LANGUAGEID = 'v') THEN OPN.VIETNAM
                               WHEN (:P_LANGUAGEID = 'id' OR :P_LANGUAGEID = 'i') THEN OPN.INDONESIA
                               WHEN (:P_LANGUAGEID = 'mm' OR :P_LANGUAGEID = 'm') THEN OPN.MYANMAR
                               WHEN (:P_LANGUAGEID = 'et' OR :P_LANGUAGEID = 't') THEN OPN.ETHIOPIA
                                    ELSE OPN.ENGLISH
                                END
                                   AS OPNAMELAN 
                                FROM T_OP_OPTP OPT JOIN T_OP_OPNM OPN ON OPT.OPNAMEID = OPN.OPNAMEID
                                WHERE ACTIONCODE = :P_ACTIONCODE AND TEMPID = :P_TEMPID ";

           var oracleParams = new List<OracleParameter> {

                new OracleParameter("P_LANGUAGEID", languageId?.ToLower()),
                new OracleParameter("P_LANGUAGEID", languageId?.ToLower()),
                new OracleParameter("P_LANGUAGEID", languageId?.ToLower()),
                new OracleParameter("P_LANGUAGEID", languageId?.ToLower()),
                new OracleParameter("P_LANGUAGEID", languageId?.ToLower()),
                new OracleParameter("P_LANGUAGEID", languageId?.ToLower()),
                new OracleParameter("P_LANGUAGEID", languageId?.ToLower()),
                new OracleParameter("P_LANGUAGEID", languageId?.ToLower()),
                new OracleParameter("P_ACTIONCODE", opActionCode),
                new OracleParameter("P_TEMPID", opTempId)
            };

            return OracleDbManager.GetObjects<Optp>(strSql, oracleParams.ToArray());
        }
        
        public static bool AddProcessToTemplate(List<Optp> lstProcess)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    foreach (var optp in lstProcess)
                    {
                        AddProcessTemplate(optp, connection, trans);
                    }
                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public static bool AddProcessTemplate(Optp lstProcess, OracleConnection oraConn, OracleTransaction trans)
        {
            string sql = @"INSERT INTO T_OP_OPTP (ACTIONCODE, TEMPID, OPNAMEID) VALUES (:P_ACTIONCODE, :P_TEMPID, :P_OPNAMEID)";
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_ACTIONCODE", lstProcess.ActionCode),
                new OracleParameter("P_TEMPID", lstProcess.TempId),
                new OracleParameter("P_OPNAMEID", lstProcess.OpNameId),
            };
            var result = OracleDbManager.ExecuteQuery(sql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }

        public static bool DeleteProcessTemplate(List<Optp> lstProcess)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    foreach (var process in lstProcess)
                    {
                        if (!DeleteTemplate1(process, trans, connection))
                        {
                            trans.Rollback();
                            return false;
                        };
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public static bool DeleteTemplate1(Optp process, OracleTransaction trans, OracleConnection oraConn)
        {
            string sql = @"DELETE FROM T_OP_OPTP WHERE ACTIONCODE = :P_ACTIONCODE AND TEMPID = :P_TEMPID AND OPNAMEID = :P_OPNAMEID";
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_ACTIONCODE", process.ActionCode),
                new OracleParameter("P_TEMPID", process.TempId),
                new OracleParameter("P_OPNAMEID", process.OpNameId),
            };
            var result = OracleDbManager.ExecuteQuery(sql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }

        public static bool DeleteActionCodeTemplate(Optp template, OracleTransaction trans, OracleConnection oraConn)
        {
            string sql = @"DELETE FROM T_OP_OPTP where TEMPID = :P_TEMPID and ACTIONCODE =:P_ACTIONCODE";
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_TEMPID", template.TempId),
                new OracleParameter("P_ACTIONCODE", template.ActionCode),
            };
            var result = OracleDbManager.ExecuteQuery(sql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }
    }
}
