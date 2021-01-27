using OPS_Utils;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Business
{
    public class ImportExcel
    {
        public string BulkExcel(string path, string sheet, string tableName)
        {
            try
            {
                string excelConnectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=Excel 8.0", path);
                using (OleDbConnection connection = new OleDbConnection(excelConnectionString))
                {
                    OleDbCommand command = new OleDbCommand("Select * FROM ["+sheet+"$]", connection);
                    connection.Open();

                    using (DbDataReader dr = command.ExecuteReader())
                    {
                        // SQL Server Connection String 
                        string sqlConnectionString = ConstantGeneric.ConnectionStr;
                        // Bulk Copy to SQL Server 
                        using (SqlBulkCopy bulkCopy =new SqlBulkCopy(sqlConnectionString))
                        {
                            bulkCopy.DestinationTableName = tableName;
                            bulkCopy.WriteToServer(dr);
                        }
                    }
                }
                return "The data has been exported succefuly from Excel to SQL";
            }

            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string BulkInset(string path, string sheet, string tableName)
        {
            try
            {
                string excelConnectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=Excel 8.0", path);
                using (OleDbConnection connection = new OleDbConnection(excelConnectionString))
                {
                    OleDbCommand command = new OleDbCommand("Select * FROM [" + sheet + "$]", connection);
                    connection.Open();

                    using (DbDataReader dr = command.ExecuteReader())
                    {
                        // SQL Server Connection String 
                        string sqlConnectionString = ConstantGeneric.ConnectionStr;
                        // Bulk Copy to SQL Server 
                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConnectionString))
                        {
                            bulkCopy.DestinationTableName = tableName;
                            bulkCopy.WriteToServer(dr);
                        }
                    }
                }
                return "The data has been exported succefuly from Excel to SQL";
            }

            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
