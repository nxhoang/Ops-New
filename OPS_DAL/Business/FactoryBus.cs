using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_DAL.MesEntities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OPS_DAL.Business
{
    /// <summary>
    /// Factory business to handle events interact DB
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    public class FactoryBus
    {
        #region MySql database

        /// <summary>
        /// Gets the by factory type and status.
        /// </summary>
        /// <param name="factoryType">Type of the factory.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static List<FactoryEntity> GetByFacTypeAndStatus(string factoryType, string status)
        {
            var prs = new List<MySqlParameter> {
                new MySqlParameter("P_TYPE", factoryType),
                new MySqlParameter("P_STATUS", status)
            };

            var factories = MySqlDBManager.GetObjects<FactoryEntity>("SP_MES_GETBYTYPEANDSTATUS_FCMT",
                CommandType.StoredProcedure, prs.ToArray());

            return factories;
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="tbsps">The list of space tables.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool MySqlSaveChanges(List<TableSpaceEntity> tbsps)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    foreach (var tbsp in tbsps)
                    {
                        MySqlSaveTbsp(tbsp, connection, transaction);
                    }

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// MySQL saving TBSP.
        /// </summary>
        /// <param name="tbsp">The space table.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool MySqlSaveTbsp(TableSpaceEntity tbsp, MySqlConnection connection, MySqlTransaction transaction)
        {
            var prs = new List<MySqlParameter>
                    {
                        new MySqlParameter("p_tableid", tbsp.TableId),
                        new MySqlParameter("p_tablename", tbsp.TableName),
                        new MySqlParameter("p_angle", tbsp.Angle),
                        new MySqlParameter("p_tblocation", tbsp.TbLocation)
                    };
            var result = MySqlDBManager.ExecuteQueryWithTrans("SP_MES_SAVE_TPSP", prs.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return result != null;
        }
        #endregion

        #region Oracle database

        /// <summary>
        /// Get development team with has factory code start character is not P
        /// </summary>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        /// Date: 26/Jan/2021
        public static List<FactoryEntity> GetDevelopmentTeam()
        {
            string strSql = @"SELECT factory as factoryId, name as factoryname
                                FROM T_CM_FCMT 
                                WHERE TYPE = 'P' AND STATUS  = 'OK' AND SUBSTR(FACTORY, 1, 1) <> 'P' ORDER BY FACTORY";

            var listDevTeam = OracleDbManager.GetObjects<FactoryEntity>(strSql, CommandType.Text, null);

            return listDevTeam;
        }

        /// <summary>
        /// Gets the by type and status.
        /// </summary>
        /// <param name="factoryType">Type of the factory.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static List<FactoryEntity> GetByTypeAndStatus(string factoryType, string status)
        {
            var oracleParams = new List<OracleParameter> {
                new OracleParameter("P_TYPE", factoryType),
                new OracleParameter("P_STATUS", status),
                new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor){Direction=ParameterDirection.Output}
            };

            var factories = OracleDbManager.GetObjects<FactoryEntity>("SP_OPS_GETBYTYPEANDSTATUS_FCMT",
                CommandType.StoredProcedure, oracleParams.ToArray());

            return factories;
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="tbsps">The list of space tables.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool OracleSaveChanges(List<TableSpaceEntity> tbsps)
        {
            using (var oracleConnection = new OracleConnection(ConstantGeneric.ConnectionStrMes))
            {
                oracleConnection.Open();
                var oracleTransaction = oracleConnection.BeginTransaction();

                try
                {
                    var arParam = new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16)
                    {
                        Direction = ParameterDirection.Output
                    };
                    var oracleParams = new List<OracleParameter>
                    {
                        arParam,
                        new OracleParameter("p_tableid", tbsps.Select(x=>x.TableId).ToArray()),
                        new OracleParameter("p_tablename", tbsps.Select(x=>x.TableName).ToArray()),
                        new OracleParameter("p_tbangle", tbsps.Select(x=>x.Angle).ToArray()),
                        new OracleParameter("p_tblocation", tbsps.Select(x=>x.TbLocation).ToArray())
                    };
                    var result = OracleDbManager.ExecuteQuery("sp_mes_savechanges_tpsp", oracleParams.ToArray(),
                        CommandType.StoredProcedure, tbsps.Count, oracleTransaction, oracleConnection);

                    oracleTransaction.Commit();

                    return result is Array affectedRows && affectedRows.Length > 0;
                }
                catch
                {
                    oracleTransaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Saves the seat detail.
        /// </summary>
        /// <param name="opdts">The list of operation details.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool OracleSaveSeatDetail(List<Opdt> opdts)
        {
            using (var oracleConnection = new OracleConnection(ConstantGeneric.ConnectionStrMes))
            {
                oracleConnection.Open();
                var oracleTransaction = oracleConnection.BeginTransaction();

                try
                {
                    var affectedRowPara = new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16)
                    {
                        Direction = ParameterDirection.Output
                    };
                    var oracleParams = new OpsOracleParams(opdts.Select(x => x.Edition).ToArray(),
                        opdts.Select(x => x.StyleCode).ToArray(),
                        opdts.Select(x => x.StyleSize).ToArray(), opdts.Select(x => x.StyleColorSerial).ToArray(),
                        opdts.Select(x => x.RevNo).ToArray(), opdts.Select(x => x.OpRevNo).ToArray());

                    var oracleParas = new List<OracleParameter>
                    {
                        new OracleParameter("P_OPSERIAL", opdts.Select(x=>x.OpSerial).ToArray()),
                        new OracleParameter("P_LineSerial", opdts.Select(x=>x.LineSerial).ToArray()),
                        new OracleParameter("P_TableId", opdts.Select(x=>x.TableId).ToArray()),
                        new OracleParameter("P_SeatNo", opdts.Select(x=>x.SeatNo).ToArray())
                    };

                    oracleParams.AddRange(oracleParas);
                    oracleParams.Insert(0, affectedRowPara);

                    var result = OracleDbManager.ExecuteQuery("SP_MES_SAVESEAT_OPDT", oracleParams.ToArray(),
                        CommandType.StoredProcedure, opdts.Count, oracleTransaction, oracleConnection);

                    oracleTransaction.Commit();

                    return result is Array affectedRows && affectedRows.Length > 0;
                }
                catch
                {
                    oracleTransaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the FLSM by factory.
        /// </summary>
        /// <param name="factoryId">The factory identifier.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static Flsm GetFlsmByFactory(string factoryId, int tenantId)
        {
            switch (tenantId)
            {
                case 1:
                    return MySqlGetFlsmByFactory(factoryId);
                case 2:
                    return OrableGetFlsmByFactory(factoryId);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Orables the get FLSM by factory.
        /// </summary>
        /// <param name="factoryId">The factory identifier.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static Flsm OrableGetFlsmByFactory(string factoryId)
        {
            var oracleParams = new List<OracleParameter> {
                new OracleParameter("P_factoryId", factoryId),
                new OracleParameter("out_cursor", OracleDbType.RefCursor){Direction=ParameterDirection.Output}
            };

            var flsms = OracleDbManager.GetObjects<Flsm>("sp_mes_getbyfactory_flsm",
                CommandType.StoredProcedure, oracleParams.ToArray(), EnumDataSource.PkMes);

            return flsms.FirstOrDefault();
        }

        /// <summary>
        /// Inserts the FLSM.
        /// </summary>
        /// <param name="flsm">The FLSM.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static Flsm InsertFlsm(Flsm flsm, int tenantId)
        {
            switch (tenantId)
            {
                case 1:
                    return MySqlInsertFlsm(flsm);
                case 2:
                    return OrableInsertFlsm(flsm);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Mies the SQL insert FLSM.
        /// </summary>
        /// <param name="flsm">The FLSM.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static Flsm MySqlInsertFlsm(Flsm flsm)
        {
            var oracleParams = new List<MySqlParameter>
            {
                new MySqlParameter("p_factory", flsm.FactoryId),
                new MySqlParameter("p_width", flsm.Width),
                new MySqlParameter("p_length", flsm.Length)
            };

            var result = MySqlDBManager.ExecuteNonQuery("sp_mes_insert_flsm", CommandType.StoredProcedure,
                oracleParams.ToArray());

            return result != null ? flsm : null;
        }

        /// <summary>
        /// Orables the insert FLSM.
        /// </summary>
        /// <param name="flsm">The FLSM.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static Flsm OrableInsertFlsm(Flsm flsm)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("p_factory", flsm.FactoryId),
                new OracleParameter("p_width", flsm.Width),
                new OracleParameter("p_length", flsm.Length)
            };

            var result = OracleDbManager.ExecuteQuery("sp_mes_insert_flsm", oracleParams.ToArray(),
                CommandType.StoredProcedure, ConstantGeneric.ConnectionStrMes);

            return result != null ? flsm : null;
        }

        /// <summary>
        /// Mies the SQL get FLSM by factory.
        /// </summary>
        /// <param name="factoryId">The factory identifier.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static Flsm MySqlGetFlsmByFactory(string factoryId)
        {
            var mySqlParams = new List<MySqlParameter> {
                new MySqlParameter("P_factoryId", factoryId)
            };

            var flsms = MySqlDBManager.GetObjects<Flsm>("sp_mes_getbyfactory_flsm", CommandType.StoredProcedure,
                mySqlParams.ToArray());

            return flsms.FirstOrDefault();
        }

        /// <summary>
        /// Updates the FLSM.
        /// </summary>
        /// <param name="flsm">The FLSM.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool UpdateFlsm(Flsm flsm, int tenantId)
        {
            switch (tenantId)
            {
                case 1:
                    return MySqlUpdateFlsm(flsm);
                case 2:
                    return OrableUpdateFlsm(flsm);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Orables the update FLSM.
        /// </summary>
        /// <param name="flsm">The FLSM.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool OrableUpdateFlsm(Flsm flsm)
        {
            var arParam = new OracleParameter("p_affectedrows", OracleDbType.Int16)
            {
                Direction = ParameterDirection.Output
            };

            var oracleParams = new List<OracleParameter>
            {
                arParam,
                new OracleParameter("p_factoryid", flsm.FactoryId),
                new OracleParameter("p_width", flsm.Width),
                new OracleParameter("p_length", flsm.Length)
            };

            var result = OracleDbManager.ExecuteQuery("sp_mes_update_flsm", oracleParams.ToArray(),
                CommandType.StoredProcedure, ConstantGeneric.ConnectionStrMes);
            var affectedRow = int.Parse(result.ToString());

            return affectedRow > 0;
        }

        /// <summary>
        /// Mies the SQL update FLSM.
        /// </summary>
        /// <param name="flsm">The FLSM.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool MySqlUpdateFlsm(Flsm flsm)
        {
            //var arParam = new MySqlParameter("p_affectedrows", MySqlDbType.Int16)
            //{
            //    Direction = ParameterDirection.Output
            //};

            var mysqlParams = new List<MySqlParameter>
            {
                new MySqlParameter("p_factoryid", flsm.FactoryId),
                new MySqlParameter("p_width", flsm.Width),
                new MySqlParameter("p_length", flsm.Length)
            };

            var q = @"UPDATE t_cm_flsm
                    SET
                        width = ?p_width,
                        length = ?p_length,
                        lastupdated = SYSDATE()
                    WHERE
                        factory = ?p_factoryid;";

            var result = MySqlDBManager.ExecuteQuery(q, CommandType.Text, mysqlParams.ToArray());
            var affectedRow = int.Parse(result.ToString());

            if (affectedRow > 0)
            {
                // Updating LastUpdated date column to t_cm_fcmt
                MesBus.FcmtBus.UpdateLastUpdated(flsm.FactoryId);
            }

            return affectedRow > 0;
        }

        /// <summary>
        /// MySQL save seat detail.
        /// </summary>
        /// <param name="opdts">The opdts.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool MySqlSaveSeatDetail(List<Opdt> opdts)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    SaveSeats(opdts, connection, transaction);

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Bulk saving the seats.
        /// </summary>
        /// <param name="opdts">The operation detail.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 27-Aug-19
        public static bool SaveSeats(List<Opdt> opdts, MySqlConnection connection, MySqlTransaction transaction)
        {
            string lineSerialData = "", tableData = "", seatNoData = "", opSerials = "";

            for (int i = 0; i < opdts.Count; i++)
            {
                lineSerialData += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].LineSerial}' ";
                tableData += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].TableId}' ";
                seatNoData += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].SeatNo}' ";
                if (i != opdts.Count - 1)
                {
                    opSerials += $"'{opdts[i].OpSerial}',";
                }
                else
                {
                    opSerials += $"'{opdts[i].OpSerial}'";
                }
            }
            string q = $"UPDATE mes.t_mx_opdt SET lineserial = (CASE opserial {lineSerialData} END), " +
                       $"tableid = (CASE opserial {tableData} END), seatno = (CASE opserial {seatNoData} END) " +
                       $"WHERE OpSerial IN({opSerials}) AND stylecode = '{opdts[0].StyleCode}' AND stylesize = '{opdts[0].StyleSize}' " +
                       $"AND stylecolorserial = '{opdts[0].StyleColorSerial}' AND revno = '{opdts[0].RevNo}' " +
                       $"AND oprevno = '{opdts[0].OpRevNo}';";
            using (MySqlCommand myCmd = new MySqlCommand(q, connection, transaction))
            {
                myCmd.CommandType = CommandType.Text;
                myCmd.ExecuteNonQuery();
            }

            return true;
        }

        #endregion
    }
}
