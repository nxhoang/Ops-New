using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OPS_DAL.MesBus
{
    public class TableSpaceBus
    {
        #region Oracle db

        /// <summary>
        /// Adds the table space.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static TableSpaceEntity OracleInsert(TableSpaceEntity item)
        {
            var paramList = new List<OracleParameter>
            {
                new OracleParameter("p_TableId", item.TableId),
                new OracleParameter("p_factory", item.Factory),
                new OracleParameter("p_lineserial", item.LineSerial),
                new OracleParameter("p_tablename", item.TableName),
                new OracleParameter("p_tbcategory", item.TbCategory),
                new OracleParameter("p_tbangle", item.Angle),
                new OracleParameter("p_tblocation", item.TbLocation),
                new OracleParameter("p_seattotal", item.SeatTotal),
                new OracleParameter("p_seatdistance", item.SeatDistance),
                new OracleParameter("p_seattype", item.SeatType),
                new OracleParameter("p_virtualwidth", item.VirtualWidth),
                new OracleParameter("p_virtuallength", item.VirtualLength),
                new OracleParameter("p_actualwidth", item.ActualWidth),
                new OracleParameter("p_actuallength", item.ActualLength),
                new OracleParameter("p_rate", item.Rate)
            };
            var tableId = OracleDbManager.ExecuteQuery("sp_mes_insert_tbsp", paramList.ToArray(),
                CommandType.StoredProcedure, ConstantGeneric.ConnectionStrMes);

            return tableId == null ? null : item;
        }

        /// <summary>
        /// Oracles the get by factory.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static List<TableSpaceEntity> OracleGetByFactory(string factory)
        {
            var oracleParams = new List<OracleParameter> {
                new OracleParameter("p_factory", factory),
                new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor)
                {
                    Direction=ParameterDirection.Output
                }
            };

            var tables = OracleDbManager.GetObjects<TableSpaceEntity>("sp_mes_getbyfactory_tbsp",
                CommandType.StoredProcedure, oracleParams.ToArray(), EnumDataSource.PkMes);

            return tables;
        }

        /// <summary>
        /// Deletes the table.
        /// </summary>
        /// <param name="tableId">The table identifier.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool OracleDeleteTable(int tableId)
        {
            var arParam = new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16)
            {
                Direction = ParameterDirection.Output
            };

            var oracleParams = new List<OracleParameter>
            {
                arParam,
                new OracleParameter("p_tableid", tableId)
            };

            var result = OracleDbManager.ExecuteQuery("sp_mes_delete_tbsp", oracleParams.ToArray(),
                CommandType.StoredProcedure, ConstantGeneric.ConnectionStrMes);
            var affectedRow = int.Parse(result.ToString());

            return affectedRow > 0;
        }

        /// <summary>
        /// Updates the seat.
        /// </summary>
        /// <param name="tableId">The table identifier.</param>
        /// <param name="seatTotal">The seat total.</param>
        /// <param name="tbWidth">Width of the tb.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool OracleUpdateSeat(decimal tableId, decimal seatTotal, decimal tbWidth)
        {
            var arParam = new OracleParameter("p_affectedrows", OracleDbType.Int16)
            {
                Direction = ParameterDirection.Output
            };

            var oracleParams = new List<OracleParameter>
            {
                arParam,
                new OracleParameter("p_tableid", tableId),
                new OracleParameter("p_seattotal", seatTotal),
                new OracleParameter("p_virtualwidth", tbWidth)
            };

            var result = OracleDbManager.ExecuteQuery("sp_mes_updateseat_tpsp", oracleParams.ToArray(),
                CommandType.StoredProcedure, ConstantGeneric.ConnectionStrMes);
            var affectedRow = int.Parse(result.ToString());

            return affectedRow > 0;
        }

        /// <summary>
        /// Gets the table by identifier.
        /// </summary>
        /// <param name="tableId">The table identifier.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static TableSpaceEntity GetTableById(int tableId)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("p_tableid", tableId),
                new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor){Direction=ParameterDirection.Output}
            };

            var result = OracleDbManager.GetObjects<TableSpaceEntity>("sp_mes_getbyid_tbsp", CommandType.StoredProcedure,
                oracleParams.ToArray(), EnumDataSource.PkMes);

            return result.FirstOrDefault();
        }

        /// <summary>
        /// Updates the table.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool OracleUpdate(TableSpaceEntity table)
        {
            var arParam = new OracleParameter("p_affectedrows", OracleDbType.Int16)
            {
                Direction = ParameterDirection.Output
            };

            var oracleParams = new List<OracleParameter>
            {
                arParam,
                new OracleParameter("p_tableid", table.TableId),
                new OracleParameter("p_lineserial", table.LineSerial),
                new OracleParameter("p_seattype", table.SeatType),
                new OracleParameter("p_angle", table.Angle),
                new OracleParameter("p_tblocation", table.TbLocation),
                new OracleParameter("p_seattotal", table.SeatTotal),
                new OracleParameter("p_seatdistance", table.SeatDistance),
                new OracleParameter("p_actualwidth", table.ActualWidth),
                new OracleParameter("p_actuallength", table.ActualLength),
                new OracleParameter("p_virtualwidth", table.VirtualWidth),
                new OracleParameter("p_virtuallength", table.VirtualLength)
            };

            var result = OracleDbManager.ExecuteQuery("SP_MES_UPDATE_TPSP", oracleParams.ToArray(),
                CommandType.StoredProcedure, ConstantGeneric.ConnectionStrMes);
            var affectedRow = int.Parse(result.ToString());

            return affectedRow > 0;
        }

        #endregion

        #region MySQL db

        /// <summary>
        /// MySQL get by factory.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static List<TableSpaceEntity> MySqlGetByFactory(string factory)
        {
            var prs = new List<MySqlParameter> { new MySqlParameter("P_FACTORY", factory) };
            var tables = MySqlDBManager.GetAll<TableSpaceEntity>("SP_MES_GETBYFACTORY_TBSP", CommandType.StoredProcedure,
                prs.ToArray());

            return tables;
        }

        /// <summary>
        /// Adds the table space.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static TableSpaceEntity MySqlInsert(TableSpaceEntity item)
        {
            var tableIdParam = new MySqlParameter("p_tableid", OracleDbType.Decimal)
            {
                Direction = ParameterDirection.Output
            };
            var prs = new List<MySqlParameter>
            {
                tableIdParam,
                new MySqlParameter("p_factory", item.Factory),
                new MySqlParameter("p_lineserial", item.LineSerial),
                new MySqlParameter("p_tablename", item.TableName),
                new MySqlParameter("p_tbcategory", item.TbCategory),
                new MySqlParameter("p_angle", item.Angle),
                new MySqlParameter("p_tblocation", item.TbLocation),
                new MySqlParameter("p_seattotal", item.SeatTotal),
                new MySqlParameter("p_seatdistance", item.SeatDistance),
                new MySqlParameter("p_seattype", item.SeatType),
                new MySqlParameter("p_virtualwidth", item.VirtualWidth),
                new MySqlParameter("p_virtuallength", item.VirtualLength),
                new MySqlParameter("p_actualwidth", item.ActualWidth),
                new MySqlParameter("p_actuallength", item.ActualLength),
                new MySqlParameter("p_rate", item.Rate)
            };
            var tableId = MySqlDBManager.ExecuteNonQuery("SP_MES_INSERT_TBSP", CommandType.StoredProcedure, prs.ToArray());

            if (tableId == null) return null;
            // Updating LastUpdated date column to t_cm_fcmt
            FcmtBus.UpdateLastUpdated(item.Factory);

            var id = int.Parse(tableId.ToString());
            item.TableId = id;

            return item;
        }

        /// <summary>
        /// Updates the table.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool MySqlUpdate(TableSpaceEntity table)
        {
            var prs = new List<MySqlParameter>
            {
                new MySqlParameter("p_tableid", table.TableId),
                new MySqlParameter("p_factory", table.Factory),
                new MySqlParameter("p_lineserial", table.LineSerial),
                new MySqlParameter("p_tablename", table.TableName),
                new MySqlParameter("p_tbcategory", table.TbCategory),
                new MySqlParameter("p_angle", table.Angle),
                new MySqlParameter("p_tblocation", table.TbLocation),
                new MySqlParameter("p_seattotal", table.SeatTotal),
                new MySqlParameter("p_seatdistance", table.SeatDistance),
                new MySqlParameter("p_seattype", table.SeatType),
                new MySqlParameter("p_virtualwidth", table.VirtualWidth),
                new MySqlParameter("p_virtuallength", table.VirtualLength),
                new MySqlParameter("p_actualwidth", table.ActualWidth),
                new MySqlParameter("p_actuallength", table.ActualLength),
                new MySqlParameter("p_rate", table.Rate)
            };
            var result = MySqlDBManager.ExecuteNonQuery("SP_MES_UPDATE_TPSP", CommandType.StoredProcedure, prs.ToArray());
            if (result != null)
            {
                // Updating LastUpdated date column to t_cm_fcmt
                FcmtBus.UpdateLastUpdated(table.Factory);
            }

            return result != null;
        }

        /// <summary>
        /// Deletes the table.
        /// </summary>
        /// <param name="tableId">The table identifier.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool MySqlDeleteTable(int tableId)
        {
            var prs = new List<MySqlParameter> { new MySqlParameter("p_tableid", tableId) };
            var result = MySqlDBManager.ExecuteNonQuery("SP_MES_DELETE_TBSP", CommandType.StoredProcedure, prs.ToArray());

            if (result != null)
            {
                var tbsp = TableSpaceBus.GetTableById(tableId);
                // Updating LastUpdated date column to t_cm_fcmt
                FcmtBus.UpdateLastUpdated(tbsp.Factory);
            }

            return result != null;
        }

        /// <summary>
        /// Updates the seat.
        /// </summary>
        /// <param name="tableId">The table identifier.</param>
        /// <param name="seatTotal">The seat total.</param>
        /// <param name="tbWidth">Width of the tb.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool MySqlUpdateSeat(decimal tableId, decimal seatTotal, decimal tbWidth)
        {
            var prs = new List<MySqlParameter>
            {
                new MySqlParameter("p_tableid", tableId),
                new MySqlParameter("p_seattotal", seatTotal),
                new MySqlParameter("p_virtualwidth", tbWidth)
            };
            var result = MySqlDBManager.ExecuteNonQuery("SP_MES_UPDATESEAT_TPSP", CommandType.StoredProcedure, prs.ToArray());

            return result != null;
        }
        #endregion
    }
}
