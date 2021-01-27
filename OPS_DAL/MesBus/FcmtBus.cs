using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public class FcmtBus
    {
        private readonly MySqlDBManager _MySqlDBManager = new MySqlDBManager();

        /// <summary>
        /// Get factory by factory id or all
        /// </summary>
        /// <param name="factoryId"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Fcmt> GetFactories(string factoryId)
        {
            //string strSql = @"SELECT * FROM T_CM_FCMT WHERE TYPE = 'P' AND STATUS = 'OK' AND SUBSTR(FACTORY,1,1) = 'P' ";
            string strSql = @"SELECT fcm.factory, concat('[',fcm.factory, '] ',  fcm.name) as name, fcm.address, fcm.status, fcm.corporation, fcm.team, fcm.capa, fcm.department, fcm.nation 
                              FROM T_CM_FCMT FCM WHERE TYPE = 'P' AND STATUS = 'OK' AND SUBSTR(FACTORY,1,1) = 'P' ";

            var oraParams = new List<MySqlParameter>();

            if (!string.IsNullOrEmpty(factoryId))
            {
                strSql += " AND FACTORY = :P_FACTORY ";
                oraParams.Add(new MySqlParameter("P_FACTORY", factoryId));
            }

            var lstFcmt = MySqlDBManager.GetObjects<Fcmt>(strSql, CommandType.Text, oraParams.ToArray());

            return lstFcmt;
        }

        /// <summary>
        /// Get list of factory by corporation code
        /// </summary>
        /// <param name="corporationCode"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Fcmt> GetFactoriesByCorporation(string corporationCode)
        {
            string strSql = @" SELECT FCM.FACTORY, FCM.NAME, FCM.STATUS, FCM.CORPORATION, FCM.NATION
                                FROM T_CM_FCMT FCM WHERE TYPE = 'P' AND STATUS = 'OK'  AND CORPORATION = ?P_CORPORATION ";

            var oraParams = new List<MySqlParameter> {
                new MySqlParameter("P_CORPORATION", corporationCode)
            };

            var lstFcmt = MySqlDBManager.GetObjects<Fcmt>(strSql, CommandType.Text, oraParams.ToArray());

            return lstFcmt;
        }

        public async Task<List<Fcmt>> GetByCorporation(string corporationCode)
        {
            string sqlQuery = @"SELECT 
                                    FCM.FACTORY,
                                    FCM.NAME,
                                    FCM.STATUS,
                                    FCM.CORPORATION,
                                    FCM.NATION
                                FROM
                                    T_CM_FCMT FCM
                                WHERE
                                    TYPE = 'P' AND STATUS = 'OK'
                                        AND CORPORATION = ?P_CORPORATION;";

            var oraParams = new List<MySqlParameter> {
                new MySqlParameter("P_CORPORATION", corporationCode)
            };

            var factories = await _MySqlDBManager.GetAllAsync<Fcmt>(ConstantGeneric.ConnectionStrMesMySql,
                sqlQuery, CommandType.Text, oraParams.ToArray());

            return factories;
        }

        /// <summary>
        /// Get list of factories by corporation code
        /// </summary>
        /// <param name="corporationCode">Corporation code</param>
        /// <returns>List of factories</returns>
        public async Task<List<Fcmt>> GetHrmCorpsByPkCorp(string corporationCode)
        {
            var strSql = @"SELECT 
                                HrmCorpCode
                            FROM
                                mes.T_CM_FCMT FCMT
                            WHERE
                                TYPE = 'P' AND STATUS = 'OK'
                                    AND CORPORATION = ?P_CORPORATION
                            GROUP BY HrmCorpCode";

            var oraParams = new List<MySqlParameter> { new MySqlParameter("P_CORPORATION", corporationCode)};
            var fcmts = await _MySqlDBManager.GetAllAsync<Fcmt>(ConstantGeneric.ConnectionStrMesMySql, 
                strSql, CommandType.Text, oraParams.ToArray());

            return fcmts;
        }

        public static List<Flsm> GetSummaryByCorporation(string corporation)
        {
            var queryStr = @"SELECT
                            fcm.factory as FactoryId,
                            fcm.name as FactoryName,                           
                            fls.Width,
                            fls.Length,
                            fls.LastUpdated
                        FROM
                            t_cm_fcmt fcm
                            LEFT JOIN t_cm_flsm fls ON fcm.factory = fls.factory
                        WHERE
                            type = 'P'
                            AND   status = 'OK'
                            AND   corporation = :P_CORPORATION";

            var oracleParams = new List<OracleParameter> { new OracleParameter("P_CORPORATION", corporation) };
            var flsms = OracleDbManager.GetObjects<Flsm>(queryStr, CommandType.Text, oracleParams.ToArray(),
                EnumDataSource.PkMes);

            return flsms;
        }

        public static List<Flsm> SummarizeByCorp(string corType, string status, string corporation, int tenantId)
        {
            switch (tenantId)
            {
                case 1:
                    return MySqlSummarizeByCorp(corType, status, corporation);
                case 2:
                    return OracleSummarizeByCorp(corType, status, corporation);
                default:
                    return null;
            }
        }

        public static List<Flsm> OracleSummarizeByCorp(string corType, string status, string corporation)
        {
            var oracleParams = new List<OracleParameter> {
                new OracleParameter("p_type", corType),
                new OracleParameter("p_status", status),
                new OracleParameter("p_corporation", corporation),
                new OracleParameter("out_cursor", OracleDbType.RefCursor){Direction=ParameterDirection.Output}
            };

            var flsms = OracleDbManager.GetObjects<Flsm>("sp_mes_sum_flsm", CommandType.StoredProcedure,
                oracleParams.ToArray(), EnumDataSource.PkMes);

            return flsms;
        }

        public static List<Flsm> MySqlSummarizeByCorp(string corType, string status, string corp)
        {
            var oracleParams = new List<MySqlParameter> {
                new MySqlParameter("p_type", corType),
                new MySqlParameter("p_status", status),
                new MySqlParameter("p_corporation", corp)
            };

            var flsms = MySqlDBManager.GetAll<Flsm>("sp_mes_sum_flsm", CommandType.StoredProcedure,
                oracleParams.ToArray());

            return flsms;
        }
        
        public static List<Fcmt> GetCentralFactories(string factoryId)
        {
            string strSql = @"SELECT * FROM T_CM_FCMT WHERE TYPE = 'P' AND STATUS = 'OK' AND SUBSTR(FACTORY,1,1) = 'P' ";

            var oraParams = new List<OracleParameter>();

            if (!string.IsNullOrEmpty(factoryId))
            {
                strSql += " AND FACTORY = :P_FACTORY ";
                oraParams.Add(new OracleParameter("P_FACTORY", factoryId));
            }

            strSql += " Order By FACTORY ";

            var lstFcmt = OracleDbManager.GetObjects<Fcmt>(strSql, CommandType.Text, oraParams.ToArray());

            return lstFcmt;
        }

        public static bool UpdateLastUpdated(string factory)
        {
            var q = $"update `mes`.`t_cm_fcmt` set LastUpdated = now() where factory = '{factory}';";
            var rs = MySqlDBManager.ExecuteQuery(q, CommandType.Text, null);
            return rs != null;
        }
    }
}
