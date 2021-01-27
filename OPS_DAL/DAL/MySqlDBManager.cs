using MySql.Data.MySqlClient;
using OPS_Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OPS_DAL.DAL
{
    public class MySqlDBManager
    {
        #region MES My Sql

        public static List<T> GetObjects<T>(string commandText, CommandType commandType, MySqlParameter[] parameters) where T : new()
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                using (var command = new MySqlCommand(commandText) { CommandType = commandType, Connection = connection })
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
        /// Get object with type is converted
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<T> GetObjectsConvertType<T>(string commandText, CommandType commandType, MySqlParameter[] parameters, int ConnTimeout = 30) where T : new()
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                using (var command = new MySqlCommand(commandText) { CommandType = commandType, Connection = connection })
                {
                    command.CommandTimeout = Math.Max(connection.ConnectionTimeout, ConnTimeout);
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
                            if (property == null) continue;

                            if (dataReader[i] != DBNull.Value)
                            {
                                var objType = property.PropertyType;

                                //Get type of object if is Nullable
                                MethodInfo mi = property.GetMethod;
                                Type retval = mi.ReturnType;
                                Type objTypeNullable = Nullable.GetUnderlyingType(retval);
                                if (objTypeNullable != null)
                                {
                                    objType = objTypeNullable;
                                }

                                var newVal = Convert.ChangeType(dataReader[i], objType);
                                property.SetValue(newObject, newVal, null);
                            }
                        }
                        result.Add(newObject);
                    }
                    return result;
                }
            }
        }

        public static List<T> GetObjects<T>(string commandText, CommandType commandType, MySqlParameter[] parameters, string connectionString) where T : new()
        {
            /* 2019-06-07 
             * Tai Le (Thomas): Add New Function with 1 more Argument "connectionString"
             */

            string L_connectionString = ConstantGeneric.ConnectionStrMesMySql;
            if (!String.IsNullOrEmpty(connectionString))
                L_connectionString = connectionString;

            using (var connection = new MySqlConnection(L_connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand(commandText) { CommandType = commandType, Connection = connection })
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

        /// <summary>
        /// Get lists of objects with type of column data type converted to c# data type
        /// Author: Nguyen Xuan Hoang
        /// </summary>
        /// <typeparam name="T">object as entity</typeparam>
        /// <param name="commandText">command text</param>
        /// <param name="commandType">command type</param>
        /// <param name="parameters">parameters</param>
        /// <returns>List of objects</returns>
        public static List<T> GetAll<T>(string commandText, CommandType commandType, MySqlParameter[] parameters) where T : new()
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                using (var command = new MySqlCommand(commandText) { CommandType = commandType, Connection = connection })
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
                        for (var i = 0; i < dataReader.FieldCount; i++)
                        {
                            var fieldName = dataReader.GetName(i);
                            var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName,
                                StringComparison.OrdinalIgnoreCase));
                            if (property == null) continue;
                            var dr = dataReader[i];

                            if (dataReader[i] == DBNull.Value) continue;
                            if (dr is long || dr is double || dr is int || dr is ulong || dr is decimal)
                            {
                                var pt = Nullable.GetUnderlyingType(property.PropertyType);
                                if (pt == null) pt = property.PropertyType;
                                var v = Convert.ChangeType(dr, pt);
                                property.SetValue(newObject, v, null);
                            }
                            else
                            {
                                property.SetValue(newObject, dataReader[i], null);
                            }
                        }
                        result.Add(newObject);
                    }
                    return result;
                }
            }
        }

        /// <summary>
        /// Asynchronously getting lists of objects with type of column data type converted to c# data type
        /// Author: Nguyen Xuan Hoang
        /// </summary>
        /// <typeparam name="T">object as entity</typeparam>
        /// <param name="commandText">command text</param>
        /// <param name="commandType">command type</param>
        /// <param name="parameters">parameters</param>
        /// <param name="conn">connection string</param>
        /// <returns>List of objects</returns>
        public async Task<List<T>> GetAllAsync<T>(string conn, string commandText, CommandType commandType, MySqlParameter[] parameters) where T : new()
        {
            using (var connection = new MySqlConnection(conn))
            {
                connection.Open();
                using (var command = new MySqlCommand(commandText) { CommandType = commandType, Connection = connection })
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
                        for (var i = 0; i < dataReader.FieldCount; i++)
                        {
                            var fieldName = dataReader.GetName(i);
                            var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName,
                                StringComparison.OrdinalIgnoreCase));
                            if (property == null) continue;
                            var dr = dataReader[i];

                            if (dataReader[i] == DBNull.Value) continue;
                            if (dr is long || dr is double || dr is int || dr is ulong || dr is decimal)
                            {
                                var pt = Nullable.GetUnderlyingType(property.PropertyType);
                                if (pt == null) pt = property.PropertyType;
                                var v = Convert.ChangeType(dr, pt);
                                property.SetValue(newObject, v, null);
                            }
                            else
                            {
                                property.SetValue(newObject, dataReader[i], null);
                            }
                        }
                        result.Add(newObject);
                    }
                    return result;
                }
            }
        }

        public static object ExecuteQuery(string commandText, CommandType commandType, MySqlParameter[] parameters, int ConnectionTimeOut = 30)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                using (var command = new MySqlCommand(commandText) { CommandType = commandType, Connection = connection })
                {
                    command.CommandTimeout = connection.ConnectionTimeout;
                    if (ConnectionTimeOut > connection.ConnectionTimeout)
                        command.CommandTimeout = ConnectionTimeOut;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                    }
                    int roweffect = command.ExecuteNonQuery();

                    command.Parameters.Clear(); //2020-08-05 Tai Le(Thomas)

                    if (commandType == CommandType.Text)
                    {
                        return roweffect.ToString();
                    }
                    return command.Parameters[0].Value;
                }
            }
        }

        public static object ExecuteNonQuery(string commandText, CommandType commandType, MySqlParameter[] parameters)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                using (var command = new MySqlCommand(commandText) { CommandType = commandType, Connection = connection })
                {
                    command.CommandTimeout = connection.ConnectionTimeout;

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
        /// The query for inserting an object to database.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="ConnectionTimeOut">The connection time out.</param>
        /// <returns>Last inserted id</returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 22-Nov-19
        public static long InsertQuery(string commandText, CommandType commandType, MySqlParameter[] parameters,
            int ConnectionTimeOut = 30)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                using (var command = new MySqlCommand(commandText) { CommandType = commandType, Connection = connection })
                {
                    command.CommandTimeout = connection.ConnectionTimeout;
                    if (ConnectionTimeOut > connection.ConnectionTimeout)
                        command.CommandTimeout = ConnectionTimeOut;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                    }
                    command.ExecuteNonQuery();

                    return command.LastInsertedId;
                }
            }
        }

        /// <summary>
        /// Excute query with transaction
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="commandType"></param>
        /// <param name="myTrans"></param>
        /// <param name="myConnection"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static object ExecuteQueryWithTrans(string commandText, MySqlParameter[] parameters, CommandType commandType,
            MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            using (var myCommand = new MySqlCommand(commandText) { CommandType = commandType, Connection = myConnection })
            {
                myCommand.CommandTimeout = myConnection.ConnectionTimeout;
                myCommand.Transaction = myTrans;

                if (parameters != null)
                {
                    myCommand.Parameters.AddRange(ConvertNullToDbNull(parameters));
                }
                int effectedRow = myCommand.ExecuteNonQuery();
                if (commandType == CommandType.Text)
                {
                    return effectedRow.ToString();
                }
                var id = myCommand.Parameters[0].Value;

                return id;
            }
        }

        /// <summary>
        /// This function is based on ExecuteQueryWithTrans
        /// Target: Asynchronous function to resolve issue connection.
        /// </summary>
        /// <param name="commandText">command text (query string)</param>
        /// <param name="parameters">mysql parameters</param>
        /// <param name="commandType">command type</param>
        /// <param name="myTrans">transaction</param>
        /// <param name="myConnection">connectin</param>
        /// <param name="connectionTimeout">default connection timeout</param>
        /// <returns>id of the first row</returns>
        /// Author: Nguyen Xuan Hoang
        public async Task<object> ExecuteWithTransAsync(string commandText, MySqlParameter[] parameters, CommandType commandType,
            MySqlTransaction myTrans, MySqlConnection myConnection, int connectionTimeout = 120)
        {
            using (var myCommand = new MySqlCommand(commandText) {CommandType = commandType, Connection = myConnection,
                CommandTimeout = connectionTimeout, Transaction = myTrans})
            {
                //myConnection.Open();
                if (parameters != null)
                {
                    myCommand.Parameters.AddRange(ConvertNullToDbNull(parameters));
                }
                var effectedRow = await myCommand.ExecuteNonQueryAsync();
                if (commandType == CommandType.Text)
                {
                    return effectedRow.ToString();
                }
                var id = myCommand.Parameters[0].Value;

                return id;
            }

            //var myCommand = new MySqlCommand(commandText)
            //{
            //    CommandType = commandType,
            //    Connection = myConnection,
            //    CommandTimeout = myConnection.ConnectionTimeout,
            //    Transaction = myTrans
            //};
        }

        /// <summary>
        /// Asynchronously executing query
        /// </summary>
        /// <param name="conn">connection string</param>
        /// <param name="commandText">query string</param>
        /// <param name="commandType">Text or store procedure</param>
        /// <returns></returns>
        public async Task<bool> ExecuteNonQueryAsync(string conn, string commandText, CommandType commandType)
        {
            using (var connection = new MySqlConnection(conn))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    using (var myCmd = new MySqlCommand(commandText, connection, transaction))
                    {
                        myCmd.CommandType = commandType;
                        var r = await myCmd.ExecuteNonQueryAsync();

                        transaction.Commit();

                        return r > 0;
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<bool> ExecuteNonQueryAsync(string conn, string commandText, CommandType commandType, MySqlParameter[] parameters)
        {
            using (var connection = new MySqlConnection(conn))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    using (var myCmd = new MySqlCommand(commandText, connection, transaction))
                    {
                        if (parameters != null) myCmd.Parameters.AddRange(ConvertNullToDbNull(parameters));

                        myCmd.CommandType = commandType;
                        var r = await myCmd.ExecuteNonQueryAsync();
                        transaction.Commit();

                        return r > 0;
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static MySqlParameter[] ConvertNullToDbNull(MySqlParameter[] parameters)
        {
            if (parameters == null) return null;
            foreach (MySqlParameter parameter in parameters)
            {
                if (parameter.Value == null) parameter.Value = DBNull.Value;
            }
            return parameters;
        }


        public static DataTable QueryToDatable(string mySQLConnString, string commandText, CommandType commandType, MySqlParameter[] parameters, int ConnectionTimeOut = 30)
        {
            try
            {
                DataTable retDt = new DataTable();
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(commandText, mySQLConnString))
                {
                    if (parameters != null)
                        adapter.SelectCommand.Parameters.AddRange(parameters);

                    if (ConnectionTimeOut > 30)
                        adapter.SelectCommand.CommandTimeout = ConnectionTimeOut;

                    adapter.Fill(retDt);
                    adapter.SelectCommand.Parameters.Clear();

                    //adapter.Dispose();
                }

                return retDt;
            }
            catch (Exception EX)
            {
                var msg = EX.Message;

                return null;
            }

        }


        //2020-08-07 Tai Le(Thomas)
        public static DataTable QueryToDatable(string commandText, CommandType commandType, MySqlParameter[] parameters, int ConnectionTimeOut = 30)
        {
            try
            {
                DataTable retDt = new DataTable();
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(commandText, ConstantGeneric.ConnectionStrMesMySql))
                {
                    if (parameters != null)
                        adapter.SelectCommand.Parameters.AddRange(parameters);

                    if (ConnectionTimeOut > 30)
                        adapter.SelectCommand.CommandTimeout = ConnectionTimeOut;

                    adapter.Fill(retDt);

                    if (parameters != null)
                        adapter.SelectCommand.Parameters.Clear();
                }
                return retDt;
            }
            catch (Exception EX)
            {
                var msg = EX.Message;
                return null;
            }
        }
        //::END     2020-08-07 Tai Le(Thomas)

        #endregion

        #region DGS My Sql
        public static List<T> GetObjectsDgs<T>(string commandText, CommandType commandType, MySqlParameter[] parameters) where T : new()
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrDgsMySql))
            {
                connection.Open();
                using (var command = new MySqlCommand(commandText) { CommandType = commandType, Connection = connection })
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
                            if (property == null) continue;

                            //if (dataReader[i] != DBNull.Value) property.SetValue(newObject, dataReader[i], null);
                            if (dataReader[i] != DBNull.Value)
                            {
                                //Change data type of object
                                var newVal = Convert.ChangeType(dataReader[i], property.PropertyType);
                                property.SetValue(newObject, newVal, null);
                            }
                        }
                        result.Add(newObject);
                    }
                    return result;
                }
            }
        }

        public static object ExecuteQueryDgs(string commandText, CommandType commandType, MySqlParameter[] parameters, int ConnectionTimeOut = 30)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrDgsMySql))
            {
                connection.Open();
                using (var command = new MySqlCommand(commandText) { CommandType = commandType, Connection = connection })
                {
                    command.CommandTimeout = connection.ConnectionTimeout;
                    if (ConnectionTimeOut > connection.ConnectionTimeout)
                        command.CommandTimeout = ConnectionTimeOut;

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

        #endregion
    }
}
