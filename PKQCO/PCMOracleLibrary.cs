using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace PKQCO
{
    public class PCMOracleLibrary
    {
        public static OracleConnection GetOracleConnection(string vstrConnectionString)
        {
            OracleConnection connection = new OracleConnection(vstrConnectionString);
            connection.Open();
            return connection;
        }
        public static bool StatementExecution(OracleConnection vOracleConn, string vstrQuery, List<OracleParameter> vParameters, out string vstrErrorMsg, int vCommandTimeout = 60)
        {
            if (vstrQuery.Trim() == "")
            {
                vstrErrorMsg = "Statement is blank";
                return false;
            }
            try
            {
                OracleCommand oracleCommand = new OracleCommand(vstrQuery, vOracleConn)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = Math.Max(vCommandTimeout, 60)
                };
                if (vParameters != null)
                {
                    foreach (var item in vParameters)
                    {
                        oracleCommand.Parameters.Add(item);
                    }
                }
                oracleCommand.ExecuteNonQuery();
                oracleCommand.Dispose();
                vstrErrorMsg = "";
                return true;
            }
            catch (Exception e)
            {
                vstrErrorMsg = e.Message + " " + vstrQuery;
                return false;
            }
        }
        public static bool StatementExecution(string vConnectionString, string vstrQuery, List<OracleParameter> vParameters, out string vstrErrorMsg, int vCommandTimeout = 60)
        {
            if (vstrQuery.Trim() == "")
            {
                vstrErrorMsg = "Statement is blank";
                return false;
            }
            try
            {
                using (OracleConnection oracleConn = new OracleConnection(vConnectionString))
                {
                    oracleConn.Open();
                    OracleCommand oracleCommand = new OracleCommand(vstrQuery, oracleConn)
                    {
                        CommandType = CommandType.Text,
                        CommandTimeout = Math.Max(vCommandTimeout, 60)
                    };
                    if (vParameters != null)
                    {
                        foreach (var item in vParameters)
                        {
                            oracleCommand.Parameters.Add(item);
                        }
                    }
                    oracleCommand.ExecuteNonQuery();
                    oracleCommand.Parameters.Clear(); //2019-11-1 Tai Le (Thomas) add
                    oracleCommand.Dispose();
                    oracleConn.Close();
                }
                vstrErrorMsg = "";
                return true;
            }
            catch (Exception e)
            {
                vstrErrorMsg = e.Message + " " + vstrQuery;
                return false;
            }
        }
        public static bool StatementExecution(ref OracleConnection vOracleConn, ref OracleTransaction oracleTransaction, string vstrQuery, List<OracleParameter> vParameters, out string vstrErrorMsg, int vCommandTimeout = 60)
        {
            if (vstrQuery.Trim() == "")
            {
                vstrErrorMsg = "Statement is blank";
                return false;
            }
            try
            {
                OracleCommand oracleCommand = new OracleCommand(vstrQuery, vOracleConn)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = Math.Max(vCommandTimeout, 60),
                    Transaction = oracleTransaction
                };
                if (vParameters != null)
                {
                    foreach (var item in vParameters)
                    {
                        oracleCommand.Parameters.Add(item);
                    }
                }
                oracleCommand.ExecuteNonQuery();
                oracleCommand.Dispose();
                vstrErrorMsg = "";
                return true;
            }
            catch (Exception e)
            {
                vstrErrorMsg = e.Message + " " + vstrQuery;
                return false;
            }
        }
        public static bool StatementToDataTable(OracleConnection vOracleConn, string vstrQuery, List<OracleParameter> vParameters, out DataTable vdtDataTable, out string vstrErrorMsg, int vCommandTimeout = 60)
        {
            if (vstrQuery.Trim() == "")
            {
                vstrErrorMsg = "Statement is blank";
                vdtDataTable = null;
                return false;
            }
            try
            {
                OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(vstrQuery, vOracleConn);
                if (vParameters != null)
                {
                    foreach (var item in vParameters)
                    {
                        oracleDataAdapter.SelectCommand.Parameters.Add(item);
                    }
                }
                oracleDataAdapter.SelectCommand.CommandTimeout = Math.Max(vCommandTimeout, 60);
                DataTable dt = new DataTable();
                dt.CaseSensitive = true;
                oracleDataAdapter.Fill(dt);
                vstrErrorMsg = "";
                vdtDataTable = dt;
                oracleDataAdapter.SelectCommand.Parameters.Clear();
                oracleDataAdapter.Dispose();
                return true;
            }
            catch (Exception e)
            {
                vstrErrorMsg = e.Message;
                vdtDataTable = null;
                return false;
            }
        }
        public static bool StatementToDataTable(string vOracleConnString, string vstrQuery, List<OracleParameter> vParameters, out DataTable vdtDataTable, out string vstrErrorMsg, int vCommandTimeout = 60)
        {
            if (vstrQuery.Trim() == "")
            {
                vstrErrorMsg = "Statement is blank";
                vdtDataTable = null;
                return false;
            }
            try
            {
                using (OracleConnection vOracleConn = new OracleConnection(vOracleConnString))
                {
                    vOracleConn.Open();
                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(vstrQuery, vOracleConn);
                    if (vParameters != null)
                    {
                        foreach (var item in vParameters)
                        {
                            oracleDataAdapter.SelectCommand.Parameters.Add(item);
                        }
                    }
                    oracleDataAdapter.SelectCommand.CommandTimeout = Math.Max(vCommandTimeout, 60);
                    DataTable dt = new DataTable();
                    dt.CaseSensitive = true;
                    oracleDataAdapter.Fill(dt);
                    vstrErrorMsg = "";
                    vdtDataTable = dt;
                    oracleDataAdapter.SelectCommand.Parameters.Clear();
                    oracleDataAdapter.Dispose();
                    vOracleConn.Close();
                }
                return true;
            }
            catch (Exception e)
            {
                vstrErrorMsg = e.Message;
                vdtDataTable = null;
                return false;
            }
        }
        public static bool StatementToDataTable(string vOracleConnString, string vstrQuery, string vstrWhereQuery , List<OracleParameter> vParameters, out DataTable vdtDataTable, out string vstrErrorMsg, int vCommandTimeout = 60)
        {
            if (vstrQuery.Trim() == "")
            {
                vstrErrorMsg = "Statement is blank";
                vdtDataTable = null;
                return false;
            }
            try
            {
                using (OracleConnection vOracleConn = new OracleConnection(vOracleConnString))
                {
                    vOracleConn.Open();
                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(vstrQuery, vOracleConn);
                    if (vParameters != null)
                    {
                        foreach (var item in vParameters)
                        {
                            oracleDataAdapter.SelectCommand.Parameters.Add(item);
                        }
                    }
                    oracleDataAdapter.SelectCommand.CommandTimeout = Math.Max(vCommandTimeout, 60);
                    DataTable dt = new DataTable();
                    dt.CaseSensitive = true;
                    oracleDataAdapter.Fill(dt);
                    vstrErrorMsg = "";
                    vdtDataTable = dt;
                    oracleDataAdapter.SelectCommand.Parameters.Clear();
                    oracleDataAdapter.Dispose();
                    vOracleConn.Close();
                }
                return true;
            }
            catch (Exception e)
            {
                vstrErrorMsg = e.Message;
                vdtDataTable = null;
                return false;
            }
        }
        public static bool StatementToDataSet(OracleConnection vOracleConn, string vstrQuery, List<OracleParameter> vParameters, out DataSet vdsDataSet, out string vstrErrorMsg, int vCommandTimeout = 60)
        {
            if (vstrQuery.Trim() == "")
            {
                vstrErrorMsg = "Statement is blank";
                vdsDataSet = null;
                return false;
            }
            try
            {
                OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(vstrQuery, vOracleConn);
                if (vParameters != null)
                {
                    foreach (var item in vParameters)
                    {
                        oracleDataAdapter.SelectCommand.Parameters.Add(item);
                    }
                }
                oracleDataAdapter.SelectCommand.CommandTimeout = Math.Max(vCommandTimeout, 60);
                DataSet ds = new DataSet();
                oracleDataAdapter.Fill(ds);
                vstrErrorMsg = "";
                vdsDataSet = ds;
                oracleDataAdapter.SelectCommand.Parameters.Clear();
                oracleDataAdapter.Dispose();
                return true;
            }
            catch (Exception e)
            {
                vstrErrorMsg = e.Message;
                vdsDataSet = null;
                return false;
            }
        }
        public static bool StatementDataSet(OracleConnection vOracleConn, string vstrQuery, List<OracleParameter> vParameters, out DataSet vdtDataset, string vstrTableName, out string vstrErrorMsg, int vCommandTimeout = 60)
        {
            if (vstrQuery.Trim() == "")
            {
                vstrErrorMsg = "Statement is blank";
                vdtDataset = null;
                return false;
            }
            try
            {
                OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(vstrQuery, vOracleConn);
                if (vParameters != null)
                {
                    foreach (var item in vParameters)
                    {
                        oracleDataAdapter.SelectCommand.Parameters.Add(item);
                    }
                }
                oracleDataAdapter.SelectCommand.CommandTimeout = Math.Max(vCommandTimeout, 60);
                DataSet ds = new DataSet();
                ds.CaseSensitive = false;
                oracleDataAdapter.Fill(ds, vstrTableName);
                vstrErrorMsg = "";
                vdtDataset = ds;
                return true;
            }
            catch (Exception e)
            {
                vstrErrorMsg = e.Message;
                vdtDataset = null;
                return false;
            }
        }
        public bool UpdateDataTable(OracleConnection vOracleConn, ref OracleDataAdapter vOracleDtAdapter, ref DataTable vdtDataTable)
        {
            try
            {
                OracleCommandBuilder commandBuilder = new OracleCommandBuilder(vOracleDtAdapter);
                vOracleDtAdapter.Update(vdtDataTable);
                commandBuilder.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static List<T> QueryToObject<T>(OracleConnection pOracleConn, string pSQL, OracleParameter[] parameters, out string pMessage) where T : class, new()
        {
            pMessage = "";
            try
            {
                var retList = new List<T>();
                using (var command = new OracleCommand(pSQL) { Connection = pOracleConn })
                {
                    command.CommandTimeout = pOracleConn.ConnectionTimeout;
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                    }
                    var dataReader = command.ExecuteReader();
                    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    while (dataReader.Read())
                    {
                        var newObject = new T();
                        for (int i = 0; i < dataReader.FieldCount; i++) //Read All the Columns
                        {
                            string fieldName = dataReader.GetName(i);
                            var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                            if (property == null)
                                continue;
                            if (dataReader[i] != DBNull.Value)
                                property.SetValue(newObject, dataReader[i], null);
                        }
                        retList.Add(newObject);
                    }
                }
                return retList;
            }
            catch (Exception e)
            {
                pMessage = e.Message;
                return null;
            }
        }
        
        public static List<T> QueryToObject<T>(string pConnString, string pSQL, OracleParameter[] parameters, out string pMessage) where T : class, new()
        {
            pMessage = "";
            try
            {
                var retList = new List<T>();
                using (OracleConnection pOracleConn = new OracleConnection(pConnString))
                {
                    pOracleConn.Open();
                    using (var command = new OracleCommand(pSQL) { Connection = pOracleConn })
                    {
                        command.CommandTimeout = pOracleConn.ConnectionTimeout;
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                        }
                        var dataReader = command.ExecuteReader();
                        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        while (dataReader.Read())
                        {
                            var newObject = new T();
                            for (int i = 0; i < dataReader.FieldCount; i++) //Read All the Columns
                            {
                                string fieldName = dataReader.GetName(i);
                                var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                                if (property == null)
                                    continue;
                                if (dataReader[i] != DBNull.Value)
                                    property.SetValue(newObject, dataReader[i], null);
                            }
                            retList.Add(newObject);
                        }
                    }
                    pOracleConn.Close();
                    pOracleConn.Dispose();
                }
                return retList;
            }
            catch (Exception e)
            {
                pMessage = e.Message;
                return null;
            }
        }
        public static T QueryToOneObject<T>(string pConnString, string pSQL, OracleParameter[] parameters, out string pMessage) where T : class, new()
        {
            pMessage = "";
            string fieldName = "";
            try
            {
                var retList = new List<T>();
                using (OracleConnection pOracleConn = new OracleConnection(pConnString))
                {
                    pOracleConn.Open();
                    using (var command = new OracleCommand(pSQL) { Connection = pOracleConn })
                    {
                        command.CommandTimeout = pOracleConn.ConnectionTimeout;
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(ConvertNullToDbNull(parameters));
                        }
                        var dataReader = command.ExecuteReader();
                        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        while (dataReader.Read())
                        {
                            var newObject = new T();
                            for (int i = 0; i < dataReader.FieldCount; i++) //Read All the Columns
                            {
                                fieldName = dataReader.GetName(i);
                                var property = properties.FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                                if (property == null)
                                    continue;
                                if (dataReader[i] != DBNull.Value)
                                    property.SetValue(newObject, dataReader[i], null);
                            }
                            retList.Add(newObject);
                        }
                        dataReader.Close();
                        dataReader.Dispose();
                    }
                    pOracleConn.Close();
                    pOracleConn.Dispose();
                }
                return retList.FirstOrDefault();
            }
            catch (Exception e)
            {
                pMessage = "fieldName= " + fieldName + "Error: " + e.Message;
                return null;
            }
        }
         
        public static OracleParameter[] ConvertNullToDbNull(OracleParameter[] parameters)
        {
            if (parameters == null)
                return null;
            foreach (OracleParameter parameter in parameters)
            {
                if (parameter.Value == null)
                    parameter.Value = DBNull.Value;
            }
            return parameters;
        }
        //public static void UpdateDataset(OracleConnection vOracleConn, DataSet vDataset, string vstrTableName = "")
        //{
        //    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter();
        //}
    }
}
