using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using OPS_DAL.DAL;
using OPS_DAL.CuttingPlanEntity;

using Newtonsoft.Json;


namespace MES.Models
{
    public class GridData
    {
        #region Grid by Oracle
        public static string GetGridData(string pConnectionString, string pSQL, string pSQLWhere, GridSettings gridRequest, string pDateTimeFormat = "yyyy/MM/dd")
        {
            try
            {
                //Variable Declare
                decimal totalPages = 0, totalRecords = 0;
                decimal intRowPerPage = gridRequest.pageSize;

                var strSortColumn = gridRequest.sortColumn != String.Empty ? gridRequest.sortColumn : "";
                var strSortOrder = gridRequest.sortColumn != String.Empty ? gridRequest.sortOrder.Trim() : "";

                if (!(strSortOrder == "ASC" || strSortOrder == "DESC"))
                    strSortOrder = "";

                string strOrderSQL = "";
                if (!String.IsNullOrEmpty(strSortColumn))
                    strOrderSQL = strSortColumn + " " + strSortOrder;
                else
                    strOrderSQL = " RANKING ASC ";

                string strSQL = "";
                strSQL = pSQL + " WHERE 1=1 ";
                if (!String.IsNullOrEmpty(pSQLWhere.Trim()))
                    strSQL = strSQL + " AND " + pSQLWhere;


                var strMainSQL =
                    " SELECT SubmainData.* , ROW_NUMBER() OVER(ORDER BY " + strOrderSQL + " ) AS MyRowSeq  " +
                    " FROM (" + strSQL + ") SubmainData " +
                    " WHERE 1=1 ";

                if (!String.IsNullOrEmpty(gridRequest.extraWhere))
                    strMainSQL = strMainSQL + " AND " + gridRequest.extraWhere;

                DataTable dt = new DataTable();
                dt = OracleDbManager.Query(strMainSQL, null, pConnectionString);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        totalRecords = dt.Rows.Count;

                        if (intRowPerPage <= 0)
                            intRowPerPage = totalRecords;

                        totalPages = Math.Ceiling(totalRecords / intRowPerPage);
                    }
                    dt.Dispose();
                }

                decimal StartingIndex = 1 + intRowPerPage * (gridRequest.pageIndex - 1);
                if (StartingIndex <= 0)
                    StartingIndex = 1;

                decimal EndIndex = intRowPerPage * gridRequest.pageIndex;
                if (EndIndex <= 0)
                    EndIndex = totalRecords;

                var _strMainSQL = " SELECT MainData.* " +
                                 " FROM (" + strMainSQL + ") MainData " +
                                 " WHERE MyRowSeq >= " + StartingIndex + " " +
                                 " AND   MyRowSeq <= " + EndIndex +
                                 " ORDER BY MyRowSeq ";

                dt = OracleDbManager.Query(_strMainSQL, null, pConnectionString);

                var tmpResult = JsonConvert.SerializeObject(new
                {
                    total = totalPages,
                    page = gridRequest.pageIndex,
                    records = totalRecords,
                    rows = dt
                }, new JsonSerializerSettings { DateFormatString = pDateTimeFormat });
                dt.Dispose();

                return tmpResult; 
            }
            catch (Exception ex)
            {
                var msg = ex.Message;

                return JsonConvert.SerializeObject(new
                {
                    total = 0,
                    page = 0,
                    records = 0,
                    rows = ""
                });
            }
        }
        public static GridDataResult GetGridResultOracle(string pConnectionString, string pSQL, string pSQLWhere, GridSettings gridRequest, string pDateTimeFormat = "yyyy/MM/dd")
        {
            try
            {
                //Variable Declare
                decimal totalPages = 0, totalRecords = 0;
                decimal intRowPerPage = gridRequest.pageSize;

                var strSortColumn = gridRequest.sortColumn != String.Empty ? gridRequest.sortColumn : "";
                var strSortOrder = gridRequest.sortColumn != String.Empty ? gridRequest.sortOrder.Trim() : "";

                if (!(strSortOrder == "ASC" || strSortOrder == "DESC"))
                    strSortOrder = "";

                string strOrderSQL = "";
                if (!String.IsNullOrEmpty(strSortColumn))
                    strOrderSQL = strSortColumn + " " + strSortOrder;
                else
                    strOrderSQL = " RANKING ASC ";

                string strSQL = "";
                strSQL = pSQL + " WHERE 1=1 ";
                if (!String.IsNullOrEmpty(pSQLWhere.Trim()))
                    strSQL = strSQL + " AND " + pSQLWhere;


                var strMainSQL =
                    " SELECT SubmainData.* , ROW_NUMBER() OVER(ORDER BY " + strOrderSQL + " ) AS MyRowSeq  " +
                    " FROM (" + strSQL + ") SubmainData " +
                    " WHERE 1=1 ";

                if (!String.IsNullOrEmpty(gridRequest.extraWhere))
                    strMainSQL = strMainSQL + " AND " + gridRequest.extraWhere;

                DataTable dt = new DataTable();
                dt = OracleDbManager.Query(strMainSQL, null, pConnectionString);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        totalRecords = dt.Rows.Count;

                        if (intRowPerPage <= 0)
                            intRowPerPage = totalRecords;

                        totalPages = Math.Ceiling(totalRecords / intRowPerPage);
                    }
                    dt.Dispose();
                }

                decimal StartingIndex = 1 + intRowPerPage * (gridRequest.pageIndex - 1);
                if (StartingIndex <= 0)
                    StartingIndex = 1;

                decimal EndIndex = intRowPerPage * gridRequest.pageIndex;
                if (EndIndex <= 0)
                    EndIndex = totalRecords;

                var _strMainSQL = " SELECT MainData.* " +
                                 " FROM (" + strMainSQL + ") MainData " +
                                 " WHERE MyRowSeq >= " + StartingIndex + " " +
                                 " AND   MyRowSeq <= " + EndIndex +
                                 " ORDER BY MyRowSeq ";

                dt = OracleDbManager.Query(_strMainSQL, null, pConnectionString);

                var tmpResult = new GridDataResult
                {
                    total = totalPages,
                    page = gridRequest.pageIndex,
                    records = totalRecords,
                    rows = dt
                };

                dt.Dispose();

                return tmpResult;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;

                return  new GridDataResult
                {
                    total = 0,
                    page = 0,
                    records = 0,
                    rows = null
                } ;
            }
        }
        
        public static DataTable GetGridDataTable(string pConnectionString, string pSQL, string pSQLWhere, GridSettings gridRequest) {
            try
            {
                //Variable Declare
                decimal totalPages = 0, totalRecords = 0;
                decimal intRowPerPage = gridRequest.pageSize;

                var strSortColumn = gridRequest.sortColumn != String.Empty ? gridRequest.sortColumn : "";
                var strSortOrder = gridRequest.sortColumn != String.Empty ? gridRequest.sortOrder.Trim() : "";

                if (!(strSortOrder == "ASC" || strSortOrder == "DESC"))
                    strSortOrder = "";

                string strOrderSQL = "";
                if (!String.IsNullOrEmpty(strSortColumn))
                    strOrderSQL = strSortColumn + " " + strSortOrder;

                string strSQL = "";
                strSQL = pSQL + " WHERE 1=1 ";
                if (!String.IsNullOrEmpty(pSQLWhere))
                    strSQL = strSQL + " AND " + pSQLWhere;


                var strMainSQL =
                    " SELECT SubmainData.* , ROW_NUMBER() OVER(ORDER BY " + strOrderSQL + " ) AS MyRowSeq  " +
                    " FROM (" + strSQL + ") SubmainData " +
                    " WHERE 1=1 ";

                if (!String.IsNullOrEmpty(gridRequest.extraWhere))
                    strMainSQL = strMainSQL + " AND " + gridRequest.extraWhere;

                DataTable dt = new DataTable();
                dt = OracleDbManager.Query(strMainSQL, null, pConnectionString);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        totalRecords = dt.Rows.Count;

                        if (intRowPerPage <= 0)
                            intRowPerPage = totalRecords;

                        totalPages = Math.Ceiling(totalRecords / intRowPerPage);
                    }
                    dt.Dispose();
                }

                decimal StartingIndex = 1 + intRowPerPage * (gridRequest.pageIndex - 1);
                if (StartingIndex <= 0)
                    StartingIndex = 1;

                decimal EndIndex = intRowPerPage * gridRequest.pageIndex;
                if (EndIndex <= 0)
                    EndIndex = totalRecords;

                var _strMainSQL = " SELECT MainData.* " +
                                 " FROM (" + strMainSQL + ") MainData " +
                                 " WHERE MyRowSeq >= " + StartingIndex + " " +
                                 " AND   MyRowSeq <= " + EndIndex +
                                 " ORDER BY MyRowSeq ";

                dt = OracleDbManager.Query(_strMainSQL, null, pConnectionString); 
                return dt;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return new DataTable();
            } 
        }
        #endregion Grid by Oracle


        #region Grid by MySQL 
        public static string GetGridDataMySQL(string pConnectionString, string pSQL, string pSQLWhere, GridSettings gridRequest, string pDateTimeFormat = "yyyy/MM/dd")
        {
            try
            {
                //Variable Declare
                decimal totalPages = 0, totalRecords = 0;
                decimal intRowPerPage = gridRequest.pageSize;

                var strSortColumn = gridRequest.sortColumn != String.Empty ? gridRequest.sortColumn : "";
                var strSortOrder = gridRequest.sortColumn != String.Empty ? gridRequest.sortOrder.Trim() : "";

                if (!(strSortOrder == "ASC" || strSortOrder == "DESC"))
                    strSortOrder = "";

                string strOrderSQL = "";
                if (!String.IsNullOrEmpty(strSortColumn))
                    strOrderSQL = strSortColumn + " " + strSortOrder;
                else
                    strOrderSQL = " RANKING ASC ";

                string strSQL = "";
                strSQL = pSQL + " WHERE 1=1 ";
                if (!String.IsNullOrEmpty(pSQLWhere.Trim()))
                    strSQL = strSQL + " AND " + pSQLWhere;


                var strMainSQL =
                    " SELECT SubmainData.* , RANKING AS MyRowSeq  " +
                    " FROM (" + strSQL + ") SubmainData " +
                    " WHERE 1=1 ";

                if (!String.IsNullOrEmpty(gridRequest.extraWhere))
                    strMainSQL = strMainSQL + " AND " + gridRequest.extraWhere;

                DataTable dt = new DataTable();
                //dt = OracleDbManager.Query(strMainSQL, null, pConnectionString);
                dt = MySqlDBManager.QueryToDatable(pConnectionString, strMainSQL, CommandType.Text, null);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        totalRecords = dt.Rows.Count;

                        if (intRowPerPage <= 0)
                            intRowPerPage = totalRecords;

                        totalPages = Math.Ceiling(totalRecords / intRowPerPage);
                    }
                    dt.Dispose();
                }

                decimal StartingIndex = 1 + intRowPerPage * (gridRequest.pageIndex - 1);
                if (StartingIndex <= 0)
                    StartingIndex = 1;

                decimal EndIndex = intRowPerPage * gridRequest.pageIndex;
                if (EndIndex <= 0)
                    EndIndex = totalRecords;

                var _strMainSQL = " SELECT MainData.* " +
                                 " FROM (" + strMainSQL + ") MainData " +
                                 " WHERE MyRowSeq >= " + StartingIndex + " " +
                                 " AND   MyRowSeq <= " + EndIndex +
                                 " ORDER BY MyRowSeq ";

                //dt = OracleDbManager.Query(_strMainSQL, null, pConnectionString);
                dt = MySqlDBManager.QueryToDatable (pConnectionString , _strMainSQL, CommandType.Text , null);

                var tmpResult = JsonConvert.SerializeObject(new GridDataResult
                {
                    total = totalPages,
                    page = gridRequest.pageIndex,
                    records = totalRecords,
                    rows = dt
                }, new JsonSerializerSettings { DateFormatString = pDateTimeFormat });
                dt.Dispose();

                return tmpResult;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;

                return JsonConvert.SerializeObject(new GridDataResult
                {
                    total = 0,
                    page = 0,
                    records = 0,
                    rows = null
                });
            }
        }

        public static GridDataResult GetGridResultMySQL(string pConnectionString, string pSQL, string pSQLWhere, string pOrderByClause, GridSettings gridRequest, string pDateTimeFormat = "yyyy/MM/dd")
        {
            try
            {
                //Variable Declare
                decimal totalPages = 0, totalRecords = 0;
                decimal intRowPerPage = gridRequest.pageSize;

                var strSortColumn = gridRequest.sortColumn != String.Empty ? gridRequest.sortColumn : "";
                var strSortOrder = gridRequest.sortColumn != String.Empty ? gridRequest.sortOrder.Trim() : "";

                if (!(strSortOrder == "ASC" || strSortOrder == "DESC"))
                    strSortOrder = "";

                string strOrderSQL = "";
                if (!String.IsNullOrEmpty(strSortColumn))
                    strOrderSQL = strSortColumn + " " + strSortOrder;
                else
                    strOrderSQL = " RANKING ASC ";

                string strSQL = "";
                strSQL = pSQL + " WHERE 1=1 ";
                if (!String.IsNullOrEmpty(pSQLWhere.Trim()))
                    strSQL = strSQL + " AND " + pSQLWhere;

                if (!String.IsNullOrEmpty(pOrderByClause))
                    strSQL = strSQL + " " + pOrderByClause;

                var strMainSQL =
                    " SELECT SubmainData.* , RANKING AS MyRowSeq  " +
                    " FROM (" + strSQL + ") SubmainData " +
                    " WHERE 1=1 ";

                if (!String.IsNullOrEmpty(gridRequest.extraWhere))
                    strMainSQL = strMainSQL + " AND " + gridRequest.extraWhere;

                DataTable dt = new DataTable();
                //dt = OracleDbManager.Query(strMainSQL, null, pConnectionString);
                dt = MySqlDBManager.QueryToDatable(pConnectionString, strMainSQL, CommandType.Text, null);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        totalRecords = dt.Rows.Count;

                        if (intRowPerPage <= 0)
                            intRowPerPage = totalRecords;

                        totalPages = Math.Ceiling(totalRecords / intRowPerPage);
                    }
                    dt.Dispose();
                }

                decimal StartingIndex = 1 + intRowPerPage * (gridRequest.pageIndex - 1);
                if (StartingIndex <= 0)
                    StartingIndex = 1;

                decimal EndIndex = intRowPerPage * gridRequest.pageIndex;
                if (EndIndex <= 0)
                    EndIndex = totalRecords;

                var _strMainSQL = " SELECT MainData.* " +
                                 " FROM (" + strMainSQL + ") MainData " +
                                 " WHERE MyRowSeq >= " + StartingIndex + " " +
                                 " AND   MyRowSeq <= " + EndIndex +
                                 " ORDER BY MyRowSeq ";

                //dt = OracleDbManager.Query(_strMainSQL, null, pConnectionString);
                dt = MySqlDBManager.QueryToDatable(pConnectionString, _strMainSQL, CommandType.Text, null);

                var _GridDataResult = new GridDataResult()
                {
                    total = totalPages,
                    page = gridRequest.pageIndex,
                    records = totalRecords,
                    rows = dt
                };
                dt.Dispose();

                return _GridDataResult;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;

                return new GridDataResult
                {
                    total = 0,
                    page = 0,
                    records = 0,
                    rows = null
                };
            }
        }
        #endregion Grid by MySQL
         
    }


}