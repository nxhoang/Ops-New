using OPS_Utils;
//using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OPS_DAL.DAL
{
    public class OracleDI
    {
        public OracleParameter[] ConvertNullToDbNull(OracleParameter[] parameters)
        {
            if (parameters == null) return null;
            foreach (OracleParameter parameter in parameters)
            {
                if (parameter.Value == null) parameter.Value = DBNull.Value;
            }
            return parameters;
        }

        public async Task<List<T>> GetAllAsync<T>(string commandText, CommandType commandType, OracleParameter[] parameters, string connectionStr) where T : new()
        {
            using (var connection = new OracleConnection(connectionStr))
            {
                connection.Open();
                using (var command = new OracleCommand(commandText) { CommandType = commandType, Connection = connection })
                {
                    command.CommandTimeout = connection.ConnectionTimeout;
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                    }
                    var dataReader = await command.ExecuteReaderAsync();
                    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    var result = new List<T>();

                    while (dataReader.Read())
                    {
                        var newObject = new T();
                        for (int i = 0; i < dataReader.FieldCount; i++)
                        {
                            var fieldName = dataReader.GetName(i);
                            var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                            if (property == null)
                                continue;
                            if (dataReader[i] != DBNull.Value)
                                property.SetValue(newObject, dataReader[i], null);
                        }
                        result.Add(newObject);
                    }
                    return result;
                }
            }
        }
    }

    /// <summary>
    /// Oracle database manager
    /// Handle interaction database
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    public static class OracleDbManager
    {

        public static OracleCommand BuildSqlCommand(string strSql, OracleParameter[] parameters, string connstr)
        {
            OracleConnection conn = new OracleConnection(connstr);
            var cmd = conn.CreateCommand();
            cmd.CommandTimeout = conn.ConnectionTimeout;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = strSql;

            if (parameters != null)
            {
                foreach (OracleParameter param in parameters)
                {
                    if (param.Value == null)
                        param.Value = DBNull.Value;
                }
                cmd.Parameters.AddRange(parameters);
            }
            return cmd;
        }

        public static DataTable Query(string strSql, OracleParameter[] parameters)
        {
            OracleConnection conn = new OracleConnection(ConstantGeneric.ConnectionStr);
            OracleCommand cmd = null;
            OracleDataAdapter dtaAdap;
            DataTable dttResult = new DataTable { CaseSensitive = true };
            try
            {
                cmd = BuildSqlCommand(strSql, parameters, ConstantGeneric.ConnectionStr);
                //cmd.BindByName = true;
                conn.Open();
                dtaAdap = new OracleDataAdapter(cmd);
                dtaAdap.Fill(dttResult);
                conn.Close();
                return dttResult;
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
            }
        }

        public static DataTable Query(string strSql, OracleParameter[] parameters, string connstr, int Timeout = 30)
        {
            /* 2019-06-13 Tai Le (Thomas)
             */

            var LcConnstr = ConstantGeneric.ConnectionStr;
            if (!String.IsNullOrEmpty(connstr))
                LcConnstr = connstr;

            OracleConnection conn = new OracleConnection(LcConnstr);
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandTimeout = conn.ConnectionTimeout;

            if (Timeout > 30)
                cmd.CommandTimeout = Timeout;

            cmd.CommandType = CommandType.Text;
            cmd.CommandText = strSql;

            if (parameters != null)
            {
                foreach (OracleParameter param in parameters)
                {
                    if (param.Value == null)
                        param.Value = DBNull.Value;
                }
                cmd.Parameters.AddRange(parameters);
            }

            OracleDataAdapter dtaAdap;
            DataTable dttResult = new DataTable { CaseSensitive = true };
            try
            {
                conn.Open();
                dtaAdap = new OracleDataAdapter(cmd);
                dtaAdap.Fill(dttResult);
                conn.Close();
                return dttResult;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return null;
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
            }
        }


        /// <summary>
        /// Converts the null to database null.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Oracle parameters</returns>
        /// Author: Nguyen Xuan Hoang
        public static OracleParameter[] ConvertNullToDbNull(OracleParameter[] parameters)
        {
            if (parameters == null) return null;
            foreach (OracleParameter parameter in parameters)
            {
                if (parameter.Value == null) parameter.Value = DBNull.Value;
            }
            return parameters;
        }

        /// <summary>
        /// Gets the objects.
        /// </summary>
        /// <typeparam name="T">Object</typeparam>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>List of objects</returns>
        /// Author: Nguyen Xuan Hoang
        public static List<T> GetObjects<T>(string commandText, CommandType commandType, OracleParameter[] parameters) where T : new()
        {
            return GetObjects<T>(commandText, commandType, parameters, ConstantGeneric.ConnectionStr);
        }

        /// <summary>
        /// Gets the objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="connectionStr">The connection string.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 31-May-19
        public static List<T> GetObjects<T>(string commandText, CommandType commandType, OracleParameter[] parameters, string connectionStr) where T : new()
        {
            string fieldName = "";
            try
            {
                using (var connection = new OracleConnection(connectionStr))
                {
                    connection.Open();
                    using (var command = new OracleCommand(commandText) { CommandType = commandType, Connection = connection })
                    {
                        command.CommandTimeout = connection.ConnectionTimeout;
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                        }
                        var dataReader = command.ExecuteReader();
                        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        var result = new List<T>();

                        while (dataReader.Read())
                        {
                            var newObject = new T();
                            for (int i = 0; i < dataReader.FieldCount; i++)
                            {
                                fieldName = dataReader.GetName(i);
                                var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                                if (property == null)
                                    continue;
                                if (dataReader[i] != DBNull.Value)
                                    property.SetValue(newObject, dataReader[i], null);
                            }
                            result.Add(newObject);
                        }
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                var Msg = "Error at fieldName= " + fieldName + ": " + ex.Message;
                return new List<T>();
            }
        }

        /// <summary>
        /// Gets the objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="connectionStr">The connection string.</param>
        /// <param name="connectionTimeout">The connection timeout (second).</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 28-Aug-19
        public static List<T> GetObjects<T>(string commandText, CommandType commandType, OracleParameter[] parameters, string connectionStr, int connectionTimeout) where T : new()
        {
            using (var connection = new OracleConnection(connectionStr))
            {
                connection.Open();
                using (var command = new OracleCommand(commandText) { CommandType = commandType, Connection = connection })
                {
                    command.CommandTimeout = connectionTimeout;
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                    }
                    var dataReader = command.ExecuteReader();
                    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    var result = new List<T>();

                    while (dataReader.Read())
                    {
                        var newObject = new T();
                        for (int i = 0; i < dataReader.FieldCount; i++)
                        {
                            string fieldName = dataReader.GetName(i);
                            var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                            if (property == null)
                                continue;
                            if (dataReader[i] != DBNull.Value)
                                property.SetValue(newObject, dataReader[i], null);
                        }
                        result.Add(newObject);
                    }
                    return result;
                }
            }
        }

        public static async Task<List<T>> GetAllAsync<T>(string connectionString, string commandText, CommandType commandType, OracleParameter[] parameters) where T : new()
        {
            string connString = ConstantGeneric.ConnectionStr;

            if (!string.IsNullOrEmpty(connectionString)) connString = connectionString;

            using (var connection = new OracleConnection(connString))
            {
                connection.Open();
                using (var command = new OracleCommand(commandText) { CommandType = commandType, Connection = connection })
                {
                    command.CommandTimeout = connection.ConnectionTimeout;
                    if (parameters != null)
                    {
                        if (parameters.Length > 0) command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                    }
                    var dataReader = await command.ExecuteReaderAsync();
                    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    var result = new List<T>();

                    while (dataReader.Read())
                    {
                        var newObject = new T();
                        for (var i = 0; i < dataReader.FieldCount; i++)
                        {
                            var fieldName = dataReader.GetName(i);
                            var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                            if (property == null) continue;
                            if (dataReader[i] != DBNull.Value) property.SetValue(newObject, dataReader[i], null);
                        }
                        result.Add(newObject);
                    }
                    return result;
                }
            }
        }

        /// <summary>
        /// Same as    public static List<T> GetObjects<T>(string commandText, CommandType commandType, OracleParameter[] parameters)
        /// But has 1 more Argument : Connection_String ; if Connection_String is NULL (or empty) , use the default    ConstantGeneric.ConnectionStr
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pConnectionString"></param>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static List<T> GetObjects<T>(string pConnectionString, string commandText, CommandType commandType, OracleParameter[] parameters) where T : new()
        {
            string ConnectionString = ConstantGeneric.ConnectionStr;

            if (!String.IsNullOrEmpty(pConnectionString))
                ConnectionString = pConnectionString;

            using (var connection = new OracleConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new OracleCommand(commandText) { CommandType = commandType, Connection = connection })
                {
                    command.CommandTimeout = connection.ConnectionTimeout;
                    if (parameters != null)
                    {
                        if (parameters.Length > 0)
                            command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                    }
                    var dataReader = command.ExecuteReader();
                    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    var result = new List<T>();

                    while (dataReader.Read())
                    {
                        var newObject = new T();
                        for (int i = 0; i < dataReader.FieldCount; i++)
                        {
                            string fieldName = dataReader.GetName(i);
                            var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                            if (property == null)
                                continue;
                            if (dataReader[i] != DBNull.Value)
                                property.SetValue(newObject, dataReader[i], null);
                        }
                        result.Add(newObject);
                    }
                    return result;
                }
            }
        }

        ///// <summary>
        ///// Connect to live server.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="commandText"></param>
        ///// <param name="commandType"></param>
        ///// <param name="parameters"></param>
        ///// <returns></returns>
        ///// Author: Son Nguyen Cao.
        //public static List<T> GetObjectsLive<T>(string commandText, CommandType commandType, OracleParameter[] parameters) where T : new()
        //{
        //    using (var connection = new OracleConnection(ConstantGeneric.ConnectionStrLive))
        //    {
        //        connection.Open();
        //        using (var command = new OracleCommand(commandText) { CommandType = commandType, Connection = connection })
        //        {
        //            command.CommandTimeout = connection.ConnectionTimeout;
        //            if (parameters != null)
        //            {
        //                command.Parameters.AddRange(ConvertNullToDbNull(parameters));
        //            }
        //            var dataReader = command.ExecuteReader();
        //            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //            var result = new List<T>();

        //            while (dataReader.Read())
        //            {
        //                var newObject = new T();
        //                for (int i = 0; i < dataReader.FieldCount; i++)
        //                {
        //                    string fieldName = dataReader.GetName(i);
        //                    var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
        //                    if (property == null) continue;
        //                    if (dataReader[i] != DBNull.Value) property.SetValue(newObject, dataReader[i], null);
        //                }
        //                result.Add(newObject);
        //            }
        //            return result;
        //        }
        //    }
        //}

        /// <summary>
        /// Gets the objects.
        /// </summary>
        /// <typeparam name="T">Object</typeparam>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>List of objects</returns>
        /// Author: Nguyen Xuan Hoang
        public static List<T> GetObjects<T>(string commandText, OracleParameter[] parameters) where T : new()
        {
            return GetObjects<T>(commandText, CommandType.Text, parameters);
        }

        /// <summary>
        /// Gets the objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="connectionTimeout">The connection timeout.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 28-Aug-19
        public static List<T> GetObjects<T>(string commandText, OracleParameter[] parameters, int connectionTimeout) where T : new()
        {
            return GetObjects<T>(commandText, CommandType.Text, parameters, ConstantGeneric.ConnectionStr, connectionTimeout);
        }

        /// <summary>
        /// Gets the objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="connectionStr">The connection string.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 31-May-19
        public static List<T> GetObjects<T>(string commandText, OracleParameter[] parameters, string connectionStr) where T : new()
        {
            return GetObjects<T>(commandText, CommandType.Text, parameters, connectionStr);
        }


        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="connectionTimeOut">Command Timeout.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static object ExecuteQuery(string commandText, OracleParameter[] parameters, CommandType commandType, int connectionTimeOut = 30)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                using (var command = new OracleCommand(commandText) { CommandType = commandType, Connection = connection })
                {
                    command.CommandTimeout = connection.ConnectionTimeout;
                    if (connectionTimeOut > connection.ConnectionTimeout)
                        command.CommandTimeout = connectionTimeOut;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                    }
                    int roweffect = command.ExecuteNonQuery();
                    if (commandType == CommandType.Text)
                    {
                        return roweffect.ToString();
                    }
                    return command.Parameters[0].Value;
                }
            }
        }

        /// <summary>
        /// Executes the query.
        /// Insert or update list of objects
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="arrayBindCount">The array bind count.</param>
        /// <param name="oracleTransaction">The oracle transaction</param>
        /// <param name="oracleConnection">The oracle connection</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static object ExecuteQuery(string commandText, OracleParameter[] parameters, CommandType commandType,
            int arrayBindCount, OracleTransaction oracleTransaction, OracleConnection oracleConnection)
        {
            using (var command = new OracleCommand(commandText) { CommandType = commandType, Connection = oracleConnection })
            {
                command.ArrayBindCount = arrayBindCount;
                command.CommandTimeout = oracleConnection.ConnectionTimeout;
                command.Transaction = oracleTransaction;
                if (parameters != null)
                {
                    command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                }
                command.ExecuteNonQuery();
                var id = command.Parameters[0].Value;
                return id;
            }
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="oracleTransaction">The oracle transaction.</param>
        /// <param name="oracleConnection">The oracle connection.</param>
        /// <returns>First column of row</returns>
        /// Author: Nguyen Xuan Hoang
        public static object ExecuteQuery(string commandText, OracleParameter[] parameters, CommandType commandType,
            OracleTransaction oracleTransaction, OracleConnection oracleConnection)
        {
            using (var command = new OracleCommand(commandText) { CommandType = commandType, Connection = oracleConnection })
            {
                command.CommandTimeout = oracleConnection.ConnectionTimeout;
                command.Transaction = oracleTransaction;

                if (parameters != null)
                {
                    command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                }
                int roweffect = command.ExecuteNonQuery();
                if (commandType == CommandType.Text)
                {
                    return roweffect.ToString();
                }
                var id = command.Parameters[0].Value;

                return id;
            }
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The first column of the row</returns>
        /// Author: Nguyen Xuan Hoang
        public static object ExecuteQuery(string commandText, OracleParameter[] parameters, CommandType commandType, string connectionString)
        {
            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();
                var oracleTransaction = connection.BeginTransaction();

                try
                {
                    var result = ExecuteQuery(commandText, parameters, commandType, oracleTransaction, connection);
                    oracleTransaction.Commit();

                    return result;
                }
                catch (Exception ex)
                {
                    try
                    {
                        oracleTransaction.Rollback();
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        throw;
                    }
                    Console.WriteLine(ex);
                    throw;
                }
            }
        }

        public static async Task<bool> ExecQueryAsync(string commandText, OracleParameter[] parameters,
            CommandType commandType, string connectionStr)
        {
            using (var con = new OracleConnection(connectionStr))
            {
                con.Open();
                using (OracleCommand command = new OracleCommand(commandText) { CommandType = commandType, Connection = con })
                {
                    command.CommandTimeout = con.ConnectionTimeout;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                    }
                    var rs = await command.ExecuteNonQueryAsync();
                    con.Close();

                    return rs > 0;
                }
            }
        }

        #region: Custom Code by Tai Le (Thomas) 
        /// <summary>
        /// Creator: Tai Le (Thomas)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pDt"></param>
        /// <returns></returns>
        public static List<T> GetObjectsFromDataTable<T>(DataTable pDt) where T : new()
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var result = new List<T>();

            if (pDt != null)
                if (pDt.Rows.Count > 0)
                {
                    foreach (DataRow dt in pDt.Rows)
                    {
                        var newObject = new T();
                        foreach (DataColumn dc in pDt.Columns)
                        {
                            string fieldName = dc.ColumnName;
                            var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                            if (property == null)
                                continue;
                            if (dt[fieldName] != DBNull.Value)
                                property.SetValue(newObject, dt[fieldName], null);
                        }
                        result.Add(newObject);
                    }
                }

            return result;
        }

        /// <summary>
        /// Creator: Tai Le (Thomas)
        /// Reference: https://stackoverflow.com/questions/4415519/best-way-to-remove-duplicate-entries-from-a-data-table 
        /// </summary>
        /// <param name="dTable"></param>
        /// <param name="colName"></param>
        /// <returns></returns>
        public static DataTable RemoveDuplicateRows(DataTable dTable, string colName)
        {
            Hashtable hTable = new Hashtable();
            ArrayList duplicateList = new ArrayList();

            //Add list of all the unique item value to hashtable, which stores combination of key, value pair.
            //And add duplicate item value in arraylist.
            foreach (DataRow drow in dTable.Rows)
            {
                if (hTable.Contains(drow[colName]))
                    duplicateList.Add(drow);
                else
                    hTable.Add(drow[colName], string.Empty);
            }

            //Removing a list of duplicate items from datatable.
            foreach (DataRow dRow in duplicateList)
                dTable.Rows.Remove(dRow);

            //Datatable which contains unique records will be return as output.
            return dTable;
        }

        /// <summary>
        /// Creator: Tai Le (Thomas)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <param name="vEnumDataSource"></param>
        /// <returns></returns>
        public static List<T> GetObjects<T>(string commandText, CommandType commandType, OracleParameter[] parameters, EnumDataSource vEnumDataSource) where T : new()
        {
            /*  Creator: Tai Le Huu (Thomas)
             *  Create Time: 2018-11-20
             *  Purpose: Improvement of current public static List<T> GetObjects<T> but have 1 more Argument to specify the Connection
             */
            try
            {

                string strCnnString = "";

                switch (vEnumDataSource)
                {
                    case EnumDataSource.PkMes:
                        strCnnString = ConstantGeneric.ConnectionStrMes;
                        break;
                    default:
                        strCnnString = ConstantGeneric.ConnectionStr;
                        break;
                }

                using (var connection = new OracleConnection(strCnnString))
                {
                    connection.Open();
                    using (var command = new OracleCommand(commandText) { CommandType = commandType, Connection = connection })
                    {
                        command.CommandTimeout = connection.ConnectionTimeout;
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                        }
                        var dataReader = command.ExecuteReader();
                        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        var result = new List<T>();

                        while (dataReader.Read())
                        {
                            var newObject = new T();
                            for (int i = 0; i < dataReader.FieldCount; i++)
                            {
                                string fieldName = dataReader.GetName(i);
                                var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                                if (property == null)
                                    continue;
                                if (dataReader[i] != DBNull.Value)
                                    property.SetValue(newObject, dataReader[i], null);
                            }
                            result.Add(newObject);
                        }
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                string strA = e.Message;
                return null;
            }

        }
        public static async Task<List<T>> GetObjectsAsync<T>(string commandText, CommandType commandType, OracleParameter[] parameters, string connectionStr) where T : new()
        {
            string fieldName = "";
            try
            {
                using (var connection = new OracleConnection(connectionStr))
                {
                    connection.Open();
                    using (var command = new OracleCommand(commandText) { CommandType = commandType, Connection = connection })
                    {
                        command.CommandTimeout = connection.ConnectionTimeout;
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                        }
                        var dataReader = await command.ExecuteReaderAsync();

                        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        var result = new List<T>();

                        while (dataReader.Read())
                        {
                            var newObject = new T();
                            for (int i = 0; i < dataReader.FieldCount; i++)
                            {
                                fieldName = dataReader.GetName(i);
                                var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                                if (property == null)
                                    continue;
                                if (dataReader[i] != DBNull.Value)
                                    property.SetValue(newObject, dataReader[i], null);
                            }
                            result.Add(newObject);
                        }
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                var Msg = "Error at fieldName= " + fieldName + ": " + ex.Message;
                return new List<T>();
            }
        }
        public static async Task<List<T>> GetObjectsAsync<T>(string commandText, CommandType commandType, OracleParameter[] parameters, OracleConnection OracleCnn) where T : new()
        {
            string fieldName = "";
            try
            {

                using (var command = new OracleCommand(commandText) { CommandType = commandType, Connection = OracleCnn })
                {
                    command.CommandTimeout = OracleCnn.ConnectionTimeout;
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                    }
                    var dataReader = await command.ExecuteReaderAsync();

                    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    var result = new List<T>();

                    while (dataReader.Read())
                    {
                        var newObject = new T();
                        for (int i = 0; i < dataReader.FieldCount; i++)
                        {
                            fieldName = dataReader.GetName(i);
                            var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                            if (property == null)
                                continue;
                            if (dataReader[i] != DBNull.Value)
                                property.SetValue(newObject, dataReader[i], null);
                        }
                        result.Add(newObject);
                    }
                    return result;
                }

            }
            catch (Exception ex)
            {
                var Msg = "Error at fieldName= " + fieldName + ": " + ex.Message;
                return new List<T>();
            }
        }
        public static async Task<bool> ExecuteQueryAsync(string commandText, OracleParameter[] parameters, CommandType commandType,
            string oracleConnString)
        {
            try
            {
                using (OracleConnection oracleConnection = new OracleConnection(oracleConnString))
                {
                    oracleConnection.Open();
                    using (OracleCommand command = new OracleCommand(commandText) { CommandType = commandType, Connection = oracleConnection })
                    {
                        command.CommandTimeout = oracleConnection.ConnectionTimeout;

                        if (parameters != null)
                        {
                            command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                        }
                        await command.ExecuteNonQueryAsync();
                    }

                    oracleConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                return false;
            }
        }
        public static async Task<bool> ExecuteQueryAsync(string commandText, OracleParameter[] parameters, CommandType commandType,
           OracleConnection OracleCnn)
        {
            try
            {
                using (OracleCommand command = new OracleCommand(commandText) { CommandType = commandType, Connection = OracleCnn })
                {
                    command.CommandTimeout = OracleCnn.ConnectionTimeout;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                    }
                    await command.ExecuteNonQueryAsync();
                }


                return true;
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                return false;
            }
        }
        #endregion

        #region Custom Get function Mr. Son
        public static List<T> GetObjectsByType<T>(string commandText, CommandType commandType, OracleParameter[] parameters) where T : new()
        {
            return GetObjectsByType<T>(commandText, commandType, parameters, ConstantGeneric.ConnectionStr);
        }

        /// <summary>
        /// Get object by type (convert type of object from DB to type of object on class)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <param name="connectionStr"></param>
        /// <param name="connectionTimeout"></param>
        /// <returns></returns>
        public static List<T> GetObjectsByType<T>(string commandText, CommandType commandType, OracleParameter[] parameters, string connectionStr) where T : new()
        {
            using (var connection = new OracleConnection(connectionStr))
            {
                connection.Open();
                using (var command = new OracleCommand(commandText) { CommandType = commandType, Connection = connection })
                {
                    command.CommandTimeout = connection.ConnectionTimeout;
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                    }
                    var dataReader = command.ExecuteReader();
                    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    var result = new List<T>();

                    while (dataReader.Read())
                    {
                        var newObject = new T();
                        for (int i = 0; i < dataReader.FieldCount; i++)
                        {
                            string fieldName = dataReader.GetName(i);
                            var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                            if (property == null)
                                continue;
                            if (dataReader[i] != DBNull.Value)
                            {
                                //Change data type of object
                                //var newVal = Convert.ChangeType(dataReader[i], property.PropertyType);
                                //property.SetValue(newObject, newVal, null);

                                Type propType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                                object safeValue = Convert.ChangeType(dataReader[i], propType);
                                property.SetValue(newObject, safeValue, null);
                            }
                        }
                        result.Add(newObject);
                    }
                    return result;
                }
            }
        }
        #endregion
    }
}