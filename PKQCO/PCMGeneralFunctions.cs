using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.ComponentModel;
using System.Globalization;
namespace PKQCO
{
    public class PCMGeneralFunctions
    {
        public static string GetRight(string vstrExpression, int vDigits)
        {
            //string mResult = "";
            if (vstrExpression.Length <= vDigits)
            {
                return vstrExpression;
            }
            else
            {
                return vstrExpression.Substring(vstrExpression.Length - vDigits, vDigits);
            }
            //return mResult;
        }
        public static string GetLeft(string vstrExpression, int vDigits)
        {
            //string mResult = "";
            if (vstrExpression.Length < vDigits)
                vDigits = vstrExpression.Length;
            return vstrExpression.Substring(0, vDigits);
            //return mResult;
        }
        public static string GetMid(string vstrExpression, int vStartIndex, int vDigits)
        {
            //string mResult = "";
            return  vstrExpression.Substring(vStartIndex, vDigits);
            //return mResult;
        }
        public static DateTime DateTimeMax(DateTime t1, DateTime t2)
        {
            //DateTime Temp = DateTime.MinValue;
            return (t1 > t2) ? t1 : t2;
            //return Temp;
        }
        public static DateTime DateTimeMin(DateTime t1, DateTime t2)
        {
            //DateTime Temp = DateTime.MinValue;
            return (t1 < t2) ? t1 : t2;
            //return Temp;
        }
        public static string GetRoleName(string vstrConnString, string vstrRoleID)
        {
            string Result = "";
            using (OracleConnection voracleConn = new OracleConnection(vstrConnString))
            {
                voracleConn.Open();
                var strSQL = " SELECT ROLEDESC " +
                            " FROM PKERP.T_CM_URLM " +
                            " WHERE ROLEID = :ROLEID ";
                OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, voracleConn);
                oracleDataAdapter.SelectCommand.Parameters.Add(new OracleParameter("ROLEID", vstrRoleID));
                DataTable dataTable = new DataTable();
                oracleDataAdapter.Fill(dataTable);
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        Result = dr["ROLEDESC"].ToString().Trim();
                    }
                }
                dataTable.Dispose();
                oracleDataAdapter.Dispose();
            }
            return Result;
        }
        public static DateTime GetDateFromWeekNumberAndDayOfWeek(int year, int weekNumber, int dayOfWeek)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Tuesday - jan1.DayOfWeek;
            DateTime firstMonday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(jan1, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var weekNum = weekNumber;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }
            var result = firstMonday.AddDays(weekNum * 7 + dayOfWeek - 1);
            return result;
        }
        public static DataTable ToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }
        public static string WeekToString(string pWeekNo)
        {
            if (Convert.ToInt32(pWeekNo) < 10)
                return "W0" + Convert.ToInt32(pWeekNo).ToString();
            else
                return "W" + Convert.ToInt32(pWeekNo).ToString();
        }
        public static int WeekStringToInt(string pWeekNo)
        {
            if (pWeekNo.Contains("W"))
            {
                string _temp = GetRight(pWeekNo, pWeekNo.Length - 1);
                return Convert.ToInt32(_temp);
            }
            else
                return Convert.ToInt32(pWeekNo);
        }
        //public static StyleDORM StyleDORM(string vstrCnnString, string pStyleCode)
        //{
        //    StyleDORM StyleDORM = new StyleDORM();
        //    List<StyleSize> StyleSizes = new List<StyleSize>();
        //    List<StyleColor> StyleColors = new List<StyleColor>();
                //    using (OracleConnection oracleConnection = new OracleConnection(vstrCnnString))
                //    {
                //        oracleConnection.Open();
                //        //Style Size
                //        string strSQL =
                //            " SELECT Distinct StyleSize " +
                //            " FROM PKERP.T_SD_DORM " +
                //            " WHERE StyleCode = :pStyleCode ";
                //        List<OracleParameter> parameters = new List<OracleParameter>();
                //        parameters.Add(new OracleParameter("pStyleCode", pStyleCode));
                //        DataTable dataTable = new DataTable();
                //        PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, parameters, out dataTable, out strSQL);
                //        if (dataTable.Rows.Count > 0)
                //        {
                //            foreach (DataRow dr in dataTable.Rows)
                //            {
                //                StyleSizes.Add(new StyleSize(dr["StyleSize"].ToString()));
                //            }
                //        }
                //        dataTable.Dispose();
                //        //Style Color
                //        strSQL =
                //            " SELECT Distinct T_SD_DORM.StyleColorSerial , T_SD_DORM.StyleColorSerial || ' - ' || NVL(T_00_SCMT.StyleColorWays,'') AS StyleColorWays " +
                //            " FROM PKERP.T_SD_DORM " +
                //            " LEFT JOIN PKERP.T_00_SCMT ON " +
                //            "   T_SD_DORM.StyleCode = T_00_SCMT.StyleCode  AND T_SD_DORM.StyleColorSerial = T_00_SCMT.StyleColorSerial " +
                //            " WHERE T_SD_DORM.StyleCode = :pStyleCode ";
                //        dataTable = new DataTable();
                //        PCMOracleLibrary.StatementToDataTable(oracleConnection, strSQL, parameters, out dataTable, out strSQL);
                //        if (dataTable.Rows.Count > 0)
                //        {
                //            foreach (DataRow dr in dataTable.Rows)
                //            {
                //                StyleColors.Add(new StyleColor(dr["StyleColorSerial"].ToString(), dr["StyleColorWays"].ToString()));
                //            }
                //        }
                //        dataTable.Dispose();
                //        oracleConnection.Close();
                //        oracleConnection.Dispose();
                //    }
                //    StyleDORM.StyleSizes = StyleSizes.ToArray();
                //    StyleDORM.StyleColors = StyleColors.ToArray();
                //    return StyleDORM;
                //}
                //public static List<object> GetMachineList(string vstrCnnString, string vstrSQLWhere = "")
                //{
                //    List<object> Machines = new List<object>();
                //    using (OracleConnection oracleConnection = new OracleConnection(vstrCnnString))
                //    {
                //        oracleConnection.Open();
                //        var strSQL = " SELECT ASSETCODE , '[' || ASSETCODE || '] ' || ASSETNAME AS ASSETNAME " +
                //                     " FROM T_CT_MCCP " +
                //                     " WHERE 1=1 ";
                //        if (vstrSQLWhere.Length > 0)
                //            strSQL = strSQL + " AND (" + vstrSQLWhere + ") ";
                //        strSQL = strSQL + "  ORDER BY ASSETCODE ";
                //        OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
                //        DataTable dataTable = new DataTable();
                //        oracleDataAdapter.Fill(dataTable);
                //        if (dataTable.Rows.Count > 0)
                //        {
                //            foreach (DataRow dr in dataTable.Rows)
                //            {
                //                object tempObject = new { id = dr["ASSETCODE"].ToString(), text = dr["ASSETNAME"].ToString() };
                //                Machines.Add(tempObject);
                //            }
                //        }
                //        dataTable.Dispose();
                //        oracleDataAdapter.Dispose();
                //    }
                //    return Machines;
                //}
                //public static List<object> GetCRCodeList(string vstrCnnString, string vstrSQLWhere = "", string vstrDefaultValue = "")
                //{
                //    List<object> Factories = new List<object>();
                //    using (OracleConnection oracleConnection = new OracleConnection(vstrCnnString))
                //    {
                //        oracleConnection.Open();
                //        var strSQL = " SELECT CRCode , '[' ||  CRCode ||'] ' || CRName as CRName " +
                //                     " FROM PKERP.T_CM_CRMT " +
                //                     " WHERE 1=1 ";
                //        if (vstrSQLWhere.Length > 0)
                //            strSQL = strSQL + " AND " + vstrSQLWhere;
                //        strSQL = strSQL + " ORDER BY CRCode";
                //        OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
                //        DataTable dataTable = new DataTable();
                //        oracleDataAdapter.Fill(dataTable);
                //        if (dataTable.Rows.Count > 0)
                //        {
                //            foreach (DataRow dr in dataTable.Rows)
                //            {
                //                object tempObject;
                //                if (vstrDefaultValue == dr["CRCode"].ToString())
                //                    tempObject = new { id = dr["CRCode"].ToString(), text = dr["CRName"].ToString(), selected = true };
                //                else
                //                    tempObject = new { id = dr["CRCode"].ToString(), text = dr["CRName"].ToString() };
                //                Factories.Add(tempObject);
                //            }
                //        }
                //        dataTable.Dispose();
                //        oracleDataAdapter.Dispose();
                //    }
                //    return Factories;
                //}
                //public static List<object> GetBuyerList(string vstrCnnString, string vstrSQLWhere = "")
                //{
                //    List<object> Buyers = new List<object>();
                //    using (OracleConnection oracleConnection = new OracleConnection(vstrCnnString))
                //    {
                //        oracleConnection.Open();
                //        var strSQL = " SELECT S_CODE, '(' || S_CODE ||') ' || CODE_NAME AS CODE_NAME " +
                //                     " FROM PKERP.T_CM_MCMT " +
                //                     " WHERE M_CODE = 'Buyer' " +
                //                     " AND CODE_STATUS = 'OK' ";
                //        if (vstrSQLWhere.Length > 0)
                //            strSQL = strSQL + " AND " + vstrSQLWhere;
                //        strSQL = strSQL + "  ORDER BY S_CODE  ";
                //        OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
                //        DataTable dataTable = new DataTable();
                //        oracleDataAdapter.Fill(dataTable);
                //        if (dataTable.Rows.Count > 0)
                //        {
                //            foreach (DataRow dr in dataTable.Rows)
                //            {
                //                object tempObject = new { id = dr["S_CODE"].ToString(), text = dr["CODE_NAME"].ToString() };
                //                Buyers.Add(tempObject);
                //            }
                //        }
                //        dataTable.Dispose();
                //        oracleDataAdapter.Dispose();
                //    }
                //    return Buyers;
                //}
                //public static string GetCRName(string vstrConnString, string vstrCRCode)
                //{
                //    string Result = "";
                //    using (OracleConnection voracleConn = new OracleConnection(vstrConnString))
                //    {
                //        voracleConn.Open();
                //        var strSQL = " SELECT CRNAME " +
                //                     " FROM PKERP.T_CM_CRMT " +
                //                     " WHERE T_CM_CRMT.CRCODE = :CRCODE ";
                //        OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, voracleConn);
                //        oracleDataAdapter.SelectCommand.Parameters.Add(new OracleParameter("CRCODE", vstrCRCode));
                //        DataTable dataTable = new DataTable();
                //        oracleDataAdapter.Fill(dataTable);
                //        if (dataTable.Rows.Count > 0)
                //        {
                //            foreach (DataRow dr in dataTable.Rows)
                //            {
                //                Result = dr["CRNAME"].ToString().Trim();
                //            }
                //        }
                //        dataTable.Dispose();
                //        oracleDataAdapter.Dispose();
                //    }
                //    return Result;
                //}
                //public static string GetFactoryName(string vstrCnnString, string vstrFactoryID)
                //{
                //    string strResult = "";
                //    if (vstrFactoryID.Length == 0)
                //    {
                //        strResult = "Factory_ID should not be Empty.";
                //    }
                //    using (OracleConnection oracleConnection = new OracleConnection(vstrCnnString))
                //    {
                //        oracleConnection.Open();
                //        var strSQL = " SELECT * FROM PKERP.T_CM_FCMT WHERE TYPE = 'P' AND FACTORY =  :FACTORY  ";
                //        OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
                //        oracleDataAdapter.SelectCommand.Parameters.Add(new OracleParameter("FACTORY", vstrFactoryID));
                //        DataTable dataTable = new DataTable();
                //        oracleDataAdapter.Fill(dataTable);
                //        if (dataTable.Rows.Count > 0)
                //        {
                //            foreach (DataRow dr in dataTable.Rows)
                //            {
                //                strResult = dr["NAME"].ToString().Trim();
                //            }
                //        }
                //        dataTable.Dispose();
                //        oracleDataAdapter.Dispose();
                //    }
                //    return strResult;
                //}
                //public static List<Select2Option> GetFactoryList(string vstrCnnString, string vstrSQLWhere = "", string vstrDefaultValue = "")
                //{
                //    List<Select2Option> Result = new List<Select2Option>();
                //    using (OracleConnection oracleConnection = new OracleConnection(vstrCnnString))
                //    {
                //        oracleConnection.Open();
                //        var strSQL = " SELECT T_CM_FCMT.FACTORY , '(' || T_CM_FCMT.FACTORY || ') ' || T_CM_FCMT.NAME AS FULLNAME " +
                //                     " FROM PKERP.T_CM_FCMT " +
                //                     " WHERE T_CM_FCMT.TYPE = 'P' " +
                //                     "  AND T_CM_FCMT.STATUS = 'OK' " +
                //                     "  AND SUBSTR(T_CM_FCMT.FACTORY,1,1) = 'P' ";
                //        if (vstrSQLWhere.Length > 0)
                //            strSQL = strSQL + " AND " + vstrSQLWhere;
                //        strSQL = strSQL + "  ORDER BY T_CM_FCMT.FACTORY  ";
                //        OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
                //        DataTable dataTable = new DataTable();
                //        oracleDataAdapter.Fill(dataTable);
                //        if (dataTable.Rows.Count > 0)
                //        {
                //            foreach (DataRow dr in dataTable.Rows)
                //            {
                //                Result.Add(new Select2Option(dr["FACTORY"].ToString(), dr["FULLNAME"].ToString(), vstrDefaultValue == dr["FACTORY"].ToString()));
                //            }
                //        }
                //        dataTable.Dispose();
                //        oracleDataAdapter.Dispose();
                //    }
                //    return Result;
                //}
                //public static List<Select2Option> GetPatternWarehouseList(string vstrCnnString, string vstrDefaultValue = "", string vstrWarehouseType = "")
                //{
                //    List<Select2Option> Result = new List<Select2Option>();
                //    using (OracleConnection oracleConnection = new OracleConnection(vstrCnnString))
                //    {
                //        oracleConnection.Open();
                //        string strSQL =
                //               " Select * " +
                //               " From " +
                //               " (Select Factory AS WHCODE, NAME AS WHNAME, 'PKF' As WHType From PKERP.T_CM_FCMT Where Type = 'P' AND SUBSTR(Factory,1,1) = 'P' AND STATUS = 'OK'  " +
                //               " UNION " +
                //               " Select CRCODE , CRNAME , 'PKCR' From PKERP.T_CM_CRMT " +
                //               " UNION " +
                //               " SELECT SOS , FULLNAME , 'PKTr' FROM PKERP.T_CM_SSCM WHERE STATUS = 'OK' AND TREATCHK = 'Y' " +
                //               " ) " +
                //               " WHERE 1=1 ";
                //        if (!String.IsNullOrEmpty(vstrWarehouseType))
                //            strSQL = strSQL + " AND WHType IN ( '" + vstrWarehouseType + "' ) ";
                //        strSQL = strSQL + " ORDER BY WHCODE ";
                //        OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
                //        DataTable dataTable = new DataTable();
                //        oracleDataAdapter.Fill(dataTable);
                //        if (dataTable.Rows.Count > 0)
                //        {
                //            foreach (DataRow dr in dataTable.Rows)
                //            {
                //                Result.Add(new Select2Option(dr["WHCODE"].ToString(), dr["WHNAME"].ToString(), vstrDefaultValue == dr["WHCODE"].ToString()));
                //            }
                //        }
                //        dataTable.Dispose();
                //        oracleDataAdapter.Dispose();
                //    }
                //    return Result;
                //}
                //public static List<Select2Option> GetUnitList(string vstrCnnString)
                //{
                //    List<Select2Option> Warehouses = new List<Select2Option>();
                //    using (OracleConnection oracleConnection = new OracleConnection(vstrCnnString))
                //    {
                //        oracleConnection.Open();
                //        string strSQL =
                //       " SELECT S_CODE , CODE_NAME " +
                //       " FROM PKERP.T_CM_MCMT " +
                //       " WHERE M_CODE = 'UnitCode' " +
                //       " ORDER BY CODE_DESC , S_CODE ";
                //        OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
                //        DataTable dataTable = new DataTable();
                //        oracleDataAdapter.Fill(dataTable);
                //        if (dataTable.Rows.Count > 0)
                //        {
                //            foreach (DataRow dr in dataTable.Rows)
                //            {
                //                Warehouses.Add(new Select2Option(dr["S_CODE"].ToString(), dr["CODE_NAME"].ToString()));
                //            }
                //        }
                //        dataTable.Dispose();
                //        oracleDataAdapter.Dispose();
                //    }
                //    return Warehouses;
                //}
                //public static List<Select2Option> GetTrType(string vstrCnnString, string vstrDefaultValue = "")
                //{
                //    List<Select2Option> Result = new List<Select2Option>();
                //    using (OracleConnection oracleConnection = new OracleConnection(vstrCnnString))
                //    {
                //        oracleConnection.Open();
                //        string strSQL =
                //       " SELECT S_CODE , CODE_NAME " +
                //       " FROM PKERP.T_CM_MCMT " +
                //       " WHERE M_CODE = 'TrType' " +
                //       " ORDER BY S_CODE ";
                //        OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
                //        DataTable dataTable = new DataTable();
                //        oracleDataAdapter.Fill(dataTable);
                //        if (dataTable.Rows.Count > 0)
                //        {
                //            foreach (DataRow dr in dataTable.Rows)
                //            {
                //                Result.Add(new Select2Option(dr["S_CODE"].ToString(), dr["CODE_NAME"].ToString(), vstrDefaultValue == dr["S_CODE"].ToString()));
                //            }
                //        }
                //        dataTable.Dispose();
                //        oracleDataAdapter.Dispose();
                //    }
                //    return Result;
                //}
                //public static List<Select2Option> GetBuyerList(string vstrCnnString, string vstrSQLWhere = "", string vstrDefaultValue = "")
                //{
                //    List<Select2Option> Result = new List<Select2Option>();
                //    using (OracleConnection oracleConnection = new OracleConnection(vstrCnnString))
                //    {
                //        oracleConnection.Open();
                //        var strSQL = " SELECT S_CODE, '(' || S_CODE ||') ' || CODE_NAME AS CODE_NAME " +
                //                     " FROM PKERP.T_CM_MCMT " +
                //                     " WHERE M_CODE = 'Buyer' " +
                //                     " AND CODE_STATUS = 'OK' ";
                //        if (vstrSQLWhere.Length > 0)
                //            strSQL = strSQL + " AND " + vstrSQLWhere;
                //        strSQL = strSQL + "  ORDER BY S_CODE  ";
                //        OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
                //        DataTable dataTable = new DataTable();
                //        oracleDataAdapter.Fill(dataTable);
                //        if (dataTable.Rows.Count > 0)
                //        {
                //            foreach (DataRow dr in dataTable.Rows)
                //            {
                //                Result.Add(new Select2Option(dr["S_CODE"].ToString(), dr["CODE_NAME"].ToString(), vstrDefaultValue == dr["S_CODE"].ToString()));
                //            }
                //        }
                //        dataTable.Dispose();
                //        oracleDataAdapter.Dispose();
                //    }
                //    return Result;
                //}
                //public static List<Select2Option> GetCRCodeList(string vstrCnnString, string vstrSQLWhere = "", string vstrDefaultValue = "")
                //{
                //    List<Select2Option> Result = new List<Select2Option>();
                //    using (OracleConnection oracleConnection = new OracleConnection(vstrCnnString))
                //    {
                //        oracleConnection.Open();
                //        var strSQL = " SELECT CRCode , '[' ||  CRCode ||'] ' || CRName as CRName " +
                //                      " FROM PKERP.T_CM_CRMT " +
                //                      " WHERE 1=1 ";
                //        if (vstrSQLWhere.Length > 0)
                //            strSQL = strSQL + " AND " + vstrSQLWhere;
                //        strSQL = strSQL + "  ORDER BY CRCode  ";
                //        OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
                //        DataTable dataTable = new DataTable();
                //        oracleDataAdapter.Fill(dataTable);
                //        if (dataTable.Rows.Count > 0)
                //        {
                //            foreach (DataRow dr in dataTable.Rows)
                //            {
                //                Result.Add(new Select2Option(dr["CRCode"].ToString(), dr["CRName"].ToString(), vstrDefaultValue == dr["CRCode"].ToString()));
                //            }
                //        }
                //        dataTable.Dispose();
                //        oracleDataAdapter.Dispose();
                //    }
                //    return Result;
                //}
                //public static List<Select2Option> GetMachineList(string vstrCnnString, string vstrSQLWhere = "", string vstrDefaultValue = "")
                //{
                //    List<Select2Option> Result = new List<Select2Option>();
                //    using (OracleConnection oracleConnection = new OracleConnection(vstrCnnString))
                //    {
                //        oracleConnection.Open();
                //        var strSQL = " SELECT ASSETCODE , '[' || ASSETCODE || '] ' || ASSETNAME AS ASSETNAME " +
                //                    " FROM T_CT_MCCP " +
                //                    " WHERE 1=1 ";
                //        if (vstrSQLWhere.Length > 0)
                //            strSQL = strSQL + " AND (" + vstrSQLWhere + ") ";
                //        strSQL = strSQL + "  ORDER BY ASSETCODE ";
                //        OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConnection);
                //        DataTable dataTable = new DataTable();
                //        oracleDataAdapter.Fill(dataTable);
                //        if (dataTable.Rows.Count > 0)
                //        {
                //            foreach (DataRow dr in dataTable.Rows)
                //            {
                //                Result.Add(new Select2Option(dr["ASSETCODE"].ToString(), dr["ASSETNAME"].ToString(), vstrDefaultValue == dr["ASSETCODE"].ToString()));
                //            }
                //        }
                //        dataTable.Dispose();
                //        oracleDataAdapter.Dispose();
                //    }
                //    return Result;
                //}
                //public static void ImportDataFromExcelFile(string vstrFileDirectory, string vstrTarget, out string vstrResult, string vstrCnnString, string vstrCurrentUserID, string vstrTPMCnnString)
                //{
                //    vstrResult = "";
                //    string strLocalMsg = "";
                //    if (!String.IsNullOrEmpty(vstrFileDirectory) && !String.IsNullOrEmpty(vstrTarget))
                //    {
                //        switch (vstrTarget.ToUpper())
                //        {
                //            case "T_MC_MMCP":
                //                int intSuccessCounter = 0;
                //                string strSuccessRows = "";
                //                int intFailCounter = 0;
                //                string strFailRows = "";
                //                FileInfo fileInfo = new FileInfo(vstrFileDirectory);
                //                var fileExtension = fileInfo.Extension;
                //                if (fileExtension == ".xlsx")
                //                {
                //                    switch (fileExtension.ToLower())
                //                    {
                //                        case ".xls":
                //                            break;
                //                        case ".xlsx":
                //                            var excelConnectionString =
                //                                string.Format(
                //                                    "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}; Extended Properties='Excel 12.0 Xml;HDR=YES;IMEX=1;' ",
                //                                    vstrFileDirectory);
                //                            OleDbConnection connection = new OleDbConnection(excelConnectionString);
                //                            //connection.ConnectionString = excelConnectionString;
                //                            DataTable sheets = GetOleDbSchemaTable(excelConnectionString);
                //                            if (sheets.Rows.Count > 0)
                //                            {
                //                                DataSet ds = new DataSet();
                //                                foreach (DataRow dr in sheets.Rows)
                //                                {
                //                                    intSuccessCounter = 0;
                //                                    intFailCounter = 0;
                //                                    strSuccessRows = "";
                //                                    strFailRows = "";
                //                                    var OrigSheetName = dr["TABLE_NAME"].ToString();
                //                                    if (OrigSheetName.Contains("$"))
                //                                    {
                //                                        var SheetName = OrigSheetName.Replace("$", "");
                //                                        string query = "SELECT * FROM [" + OrigSheetName + "]";
                //                                        ds.Clear();
                //                                        ds.Reset();
                //                                        OleDbDataAdapter data = new OleDbDataAdapter(query, connection);
                //                                        data.Fill(ds);
                //                                        /*Read Data Row in each  EXCEL SHEET */
                //                                        if (ds.Tables[0].Rows.Count > 0)
                //                                        {
                //                                            int intRowSeqNo = 0;
                //                                            if (vstrResult.Length == 0)
                //                                            {
                //                                                vstrResult = "Sheet " + SheetName + " Summary:" +
                //                                                             "<br/>Total: " + ds.Tables[0].Rows.Count;
                //                                            }
                //                                            else
                //                                                vstrResult = vstrResult + "<br/><br/>Sheet " + SheetName + " Summary:" +
                //                                                             "<br/>Total: " + ds.Tables[0].Rows.Count;
                //                                            foreach (DataRow drSheet in ds.Tables[0].Rows)
                //                                            {
                //                                                intRowSeqNo += 1;
                //                                                switch (SheetName)
                //                                                {
                //                                                    case "'Machine-MachineAsset-*'":
                //                                                        /** 
                //                                                         * This sheet will be retired since Machine Asset will be get from TPM Machine Asset Code list 
                //                                                        if (PCMMachineCapacity.InsertMachineCapacityFromExcel(vstrCnnString, drSheet, out strLocalMsg,
                //                                                            vstrCurrentUserID, intRowSeqNo))
                //                                                        {
                //                                                            intSuccessCounter += 1;
                //                                                            strSuccessRows = strSuccessRows.Length == 0
                //                                                                ? "Row: " + intRowSeqNo
                //                                                                : strSuccessRows + "; Row: " + intRowSeqNo;
                //                                                        }
                //                                                        else
                //                                                        {
                //                                                            intFailCounter += 1;
                //                                                            strFailRows = strFailRows.Length == 0
                //                                                                ? "Row: " + intRowSeqNo
                //                                                                : strFailRows + "; Row: " + intRowSeqNo;
                //                                                        }*/
                //                                                        break;
                //                                                    case "'Machine-Material'":
                //                                                        if (PCMMachineCapacity.InsertMachineMaterialCapacityFromExcel(vstrCnnString, vstrTPMCnnString, drSheet, out strLocalMsg, vstrCurrentUserID, intRowSeqNo))
                //                                                        {
                //                                                            intSuccessCounter += 1;
                //                                                            if (strSuccessRows.Length < 100)
                //                                                            {
                //                                                                strSuccessRows = strSuccessRows.Length == 0
                //                                                                    ? "Row: " + intRowSeqNo
                //                                                                    : strSuccessRows + "; Row: " + intRowSeqNo;
                //                                                            }
                //                                                        }
                //                                                        else
                //                                                        {
                //                                                            intFailCounter += 1;
                //                                                            if (strFailRows.Length < 100)
                //                                                            {
                //                                                                strFailRows = strFailRows.Length == 0
                //                                                                    ? "Row: " + intRowSeqNo
                //                                                                    : strFailRows + "; Row: " + intRowSeqNo;
                //                                                            }
                //                                                        }
                //                                                        break;
                //                                                }
                //                                            }
                //                                            vstrResult = vstrResult + "<br/>Success: " + intSuccessCounter + " record(s): " + strSuccessRows
                //                                                         + "<br/>Fail: " + intFailCounter + " record(s): " + strFailRows;
                //                                        }
                //                                        else
                //                                        {
                //                                            if (vstrResult.Length == 0)
                //                                            {
                //                                                vstrResult = "Sheet " + SheetName + " Summary:" +
                //                                                             "<br/>No Data";
                //                                            }
                //                                            else
                //                                                vstrResult = vstrResult + "<br/><br/>Sheet " + SheetName + " Summary:"
                //                                                             + "<br/>No Data";
                //                                        }
                //                                        ds.Tables.Clear();
                //                                        ds.Clear();
                //                                        ds.Reset();
                //                                    }
                //                                }
                //                                ds.Dispose();
                //                            }
                //                            sheets.Dispose();
                //                            break;
                //                    }
                //                }
                //                else
                //                {
                //                    vstrResult = "Please use Excel (2007+)";
                //                }
                //                break;
                //        }
                //    }
                //    else
                //    {
                //        vstrResult = "Invail Input<br/>" +
                //                     " - FileDirectory = " + vstrFileDirectory + "<br/>" +
                //                     " - Target Table= " + vstrTarget;
                //    }
                //}
                //static DataTable GetOleDbSchemaTable(string connectionString)
                //{
                //    using (OleDbConnection connection = new OleDbConnection(connectionString))
                //    {
                //        connection.Open();
                //        DataTable schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                //        return schemaTable;
                //    }
                //}
                //public static bool IsSessionExpired(HttpSessionState vSession)
                //{
                //    if (vSession["CurrentUserID"] == null)
                //    {
                //        return true;
                //    }
                //    return false;
                //}
                //public static string GetUserCRCode(string vstrConnString, string vstrCurrentUserID, string vstrRoleID)
                //{
                //    string Result = "";
                //    using (OracleConnection voracleConn = new OracleConnection(vstrConnString))
                //    {
                //        voracleConn.Open();
                //        var strSQL = " SELECT NVL(T_CM_URMT.CRCODE,'') CRCODE " +
                //                     " FROM PKERP.T_CM_URMT " +
                //                     " WHERE T_CM_URMT.USERID = :USERID  AND  T_CM_URMT.ROLEID = :ROLEID ";
                //        OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, voracleConn);
                //        oracleDataAdapter.SelectCommand.Parameters.Add(new OracleParameter("USERID", vstrCurrentUserID));
                //        oracleDataAdapter.SelectCommand.Parameters.Add(new OracleParameter("ROLEID", vstrRoleID));
                //        DataTable dataTable = new DataTable();
                //        oracleDataAdapter.Fill(dataTable);
                //        if (dataTable.Rows.Count > 0)
                //        {
                //            foreach (DataRow dr in dataTable.Rows)
                //            {
                //                Result = dr["CRCODE"].ToString().Trim();
                //            }
                //        }
                //        dataTable.Dispose();
                //        oracleDataAdapter.Dispose();
                //    }
                //    return Result;
                //}
        }
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
