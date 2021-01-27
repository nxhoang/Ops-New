using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace OPS_DAL.Business
{
    public class PrthBus
    {
        #region Oracle database

        //Author: HA NGUYEN
        public static List<Prth> GetTempName(string actionCode)
        {
            string sql = @"select prth.tempid, prth.tempname
                            from t_op_actp actp join t_op_prth prth on PRTH.TEMPID = actp.tempid 
                            where actp.actioncode = :P_ACTIONCODE";

            var oracleParams = new List<OracleParameter> { new OracleParameter("P_ACTIONCODE", actionCode) };

            return OracleDbManager.GetObjects<Prth>(sql, oracleParams.ToArray());
        }

        public static List<Prth> GetAllTemplate()
        {
            string sql = @"select prth.tempid, prth.tempname
                            from t_op_prth prth";

            return OracleDbManager.GetObjects<Prth>(sql, null);
        }

        public static bool AddNewTemplate(Prth objTemplate)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    AddObjTemplate(objTemplate, connection, trans);

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

        public static bool AddObjTemplate(Prth objTemplate, OracleConnection oraConn, OracleTransaction trans)
        {
            string sql = @"INSERT INTO T_OP_PRTH (TEMPID, TEMPNAME) VALUES (:P_TEMPID, :P_TEMPNAME)";
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_TEMPID", objTemplate.TempId),
                new OracleParameter("P_TEMPNAME", objTemplate.TempName),
            };
            var result = OracleDbManager.ExecuteQuery(sql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }

        public static bool DeleteTemplate(List<string> lstTemplate)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    foreach (var template in lstTemplate)
                    {
                        if (DeleteProcess(template, trans, connection))
                        {
                            if (DeleteActionCode(template, trans, connection))
                            {
                                if (DeleteTemplate1(template, trans, connection))
                                {
                                    trans.Commit();
                                    return true;
                                };
                            }
                            else
                            {
                                trans.Rollback();
                                return false;
                            }
                        }
                        else
                        {
                            trans.Rollback();
                            return false;
                        }
                    }
                    return false;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public static bool DeleteTemplate1(string template, OracleTransaction trans, OracleConnection oraConn)
        {
            string sql = @"DELETE FROM T_OP_PRTH where TEMPID = :P_TEMPID";
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_TEMPID", template),
            };
            var result = OracleDbManager.ExecuteQuery(sql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }

        public static bool DeleteProcess(string template, OracleTransaction trans, OracleConnection oraConn)
        {
            string sql = @"DELETE FROM T_OP_OPTP where TEMPID = :P_TEMPID";
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_TEMPID", template),
            };
            var result = OracleDbManager.ExecuteQuery(sql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }

        public static bool DeleteActionCode(string template, OracleTransaction trans, OracleConnection oraConn)
        {
            string sql = @"DELETE FROM T_OP_ACTP where TEMPID = :P_TEMPID";
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_TEMPID", template),
            };
            var result = OracleDbManager.ExecuteQuery(sql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }

        #endregion

        #region MySql database

        public static List<Prth> GetByActionCode(string actionCode)
        {
            var prs = new List<MySqlParameter> { new MySqlParameter("P_ACTIONCODE", actionCode) };

            var result = MySqlDBManager.GetAll<Prth>("SP_MES_GETBYACTIONCODE_PRTH", CommandType.StoredProcedure, prs.ToArray());
            return result;
        }

        #endregion
    }
}
