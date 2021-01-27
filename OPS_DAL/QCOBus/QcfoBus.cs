using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using OPS_DAL.DAL;
using OPS_DAL.QCOEntities;
using OPS_Utils;

using Oracle.ManagedDataAccess.Client;

namespace OPS_DAL.QCOBus
{
    public class QcfoBus
    {
       //public static string OdpConnStrOld = System.Configuration.ConfigurationManager.ConnectionStrings["OdpConnStrOld"].ConnectionString;
         
        /* private string FACTORY { get; set; }
         private int SORTINGSEQ { get; set; }
         private string PARAMETERNAME { get; set; }
         private decimal ORIGSEQNO { get; set; }
         private string GROUPNAME { get; set; }
         private string SORTDIRECTION { get; set; }
         */

        public static bool ClearAll(string vstrFactory)
        {
            try
            {
                string strSQL = " DELETE PKMES.T_00_QCFO WHERE FACTORY = :FACTORY ";

                List<OracleParameter> parameters = new List<OracleParameter>()
                    {
                        new OracleParameter("FACTORY", vstrFactory)
                    };

                OracleDbManager.ExecuteQuery(strSQL, parameters.ToArray(), CommandType.Text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool AddNew(string vstrFactory, string vstrPARAMETERNAME, string vstrCurrentUser)
        {
            try
            {
                var arrPARAMETERNAME = vstrPARAMETERNAME.Split(';');
                //2020-04-01 Tai Le (Thomas): Handle Sorting Direction: Ascending / Descending

                int intNextSortingSeq = NextSortingSeq(vstrFactory);

                if (intNextSortingSeq > 0)
                {
                    string strSQL = " INSERT INTO PKMES.T_00_QCFO (FACTORY , SORTINGSEQ , PARAMETERNAME, SORTDIRECTION, LASTMODIFIER, LASTUPDATETIME) " +
                                    " VALUES ( :FACTORY , :SORTINGSEQ , :PARAMETERNAME, :SORTDIRECTION, :LASTMODIFIER , SYSDATE ) ";

                    List<OracleParameter> parameters = new List<OracleParameter>()
                    {
                        new OracleParameter("FACTORY", vstrFactory),
                        new OracleParameter("SORTINGSEQ", intNextSortingSeq),
                        new OracleParameter("PARAMETERNAME", arrPARAMETERNAME[0]),
                        new OracleParameter("SORTDIRECTION", String.IsNullOrEmpty(arrPARAMETERNAME[1])? "ASC" : arrPARAMETERNAME[1]),
                        new OracleParameter("LASTMODIFIER", vstrCurrentUser)
                    };

                    OracleDbManager.ExecuteQuery(strSQL, parameters.ToArray(), CommandType.Text);
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch
            {
                return false;
            }
        }

        public static int NextSortingSeq(string vstrFactory)
        {
            try
            {
                int intSeqNo = 0;

                string strSQL = " Select MAX(SORTINGSEQ)" +
                                " From PKMES.T_00_QCFO " +
                                " Where FACTORY = :FACTORY ";

                List<OracleParameter> parameters = new List<OracleParameter>()
                {
                    new OracleParameter("FACTORY",vstrFactory)
                };

                DataTable dt = OracleDbManager.Query(strSQL, parameters.ToArray());

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        if (dt.Rows[0][0] != null)
                        {
                            if (dt.Rows[0][0].ToString().Length > 0)
                                intSeqNo = Convert.ToInt32(dt.Rows[0][0]);
                            else
                                intSeqNo = 0;
                        }
                        else
                            intSeqNo = 0;
                    }
                    else
                        intSeqNo = 0;

                    dt.Dispose();
                    return intSeqNo + 1;
                }
                return 1;
            }
            catch
            {
                return -1;
            }
        }

        public static List<Qcfo> GetMasterSettings(string vstrFactory)
        {
            var sb = new StringBuilder();
            sb.AppendLine(
                " SELECT ROW_NUMBER() OVER(ORDER BY A.SORTINGSEQ ) AS ID , A.FACTORY, A.PARAMETERNAME, A.SORTINGSEQ , A.SORTDIRECTION " +
                " FROM PKMES.T_00_QCFO A " +
                " WHERE FACTORY = :FACTORY " +
                " ORDER BY SORTINGSEQ ");
            List<OracleParameter> oracleParameters = new List<OracleParameter>
            {
                new OracleParameter("FACTORY",vstrFactory)
            };

            return OracleDbManager.GetObjects<Qcfo>(sb.ToString(), CommandType.Text, oracleParameters.ToArray(), EnumDataSource.OdpConnStr);
        }
    }

    public class FcmtBus
    {
        public static string OdpConnStrOld = System.Configuration.ConfigurationManager.ConnectionStrings["OdpConnStr"].ConnectionString;
         
        /// <summary>
        /// Get factory by factory id or all
        /// </summary>
        /// <param name="factoryId"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Fcmt> GetFactories(string factoryId)
        {
            string strSql = @"SELECT ROW_NUMBER() OVER(ORDER BY FACTORY) AS ID ,
                                FACTORY , NAME 
                                FROM T_CM_FCMT 
                                WHERE TYPE = 'P' AND STATUS = 'OK' AND SUBSTR(FACTORY,1,1) = 'P'  ";
            var oraParams = new List<OracleParameter>();

            if (!string.IsNullOrEmpty(factoryId))
            {
                strSql += " AND FACTORY = :P_FACTORY ";
                oraParams.Add(new OracleParameter("P_FACTORY", factoryId));
            }

            strSql += " ORDER BY FACTORY ";

            var lstFcmt = OracleDbManager.GetObjects<Fcmt>(strSql, CommandType.Text, oraParams.ToArray());

            return lstFcmt;
        }

        public static List<Fcmt> GetFactoriesWithAny(string DBType)
        {
            List<Fcmt> FactoriesWithAny = new List<Fcmt>();

            string sql =
                " SELECT ROW_NUMBER() OVER(ORDER BY FACTORY) AS ID , FACTORY , NAME " +
                " FROM " +
                " ( " +
                " select 'Any' FACTORY, 'Any' NAME " +
                " from Dual " +
                " UNION " +
                " Select FACTORY , NAME " +
                " From T_CM_FCMT " +
                " WHERE TYPE = 'P' " +
                " AND STATUS = 'OK' " +
                " AND SUBSTR(FACTORY,1,1) = 'P'  " +
                " ) Main ";


            switch (DBType.ToLower())
            {
                case "oracle":
                    {
                        FactoriesWithAny = OracleDbManager.GetObjects<Fcmt>(sql, CommandType.Text, null);
                    }
                    break;

                case "mysql":
                    {
                        FactoriesWithAny = MySqlDBManager.GetObjects<Fcmt>(sql, CommandType.Text, null);
                    }
                    break;
            }

            return FactoriesWithAny;

            //var lstFcmt = OracleDbManager.GetObjects<Fcmt>(OdpConnStrOld, strSql, CommandType.Text, null); 
            //return lstFcmt;
        }


    }


    public class WHMTBus {
        public static string OdpConnStrOld = System.Configuration.ConfigurationManager.ConnectionStrings["OdpConnStr"].ConnectionString;

        public static List<WHMT> GetWarehouseMasterList(string pType)
        {
            string sb = "";
            switch (pType) {
                case "PatternWarehouse":
                    sb = 
                        " SELECT * " +
                        " FROM (" +
                        "   Select CRCODE AS WHCODE , CRNAME AS WHNAME " +
                        "   From T_CM_CRMT " +
                        "   UNION " +
                        "   Select Factory AS WHCODE , NAME AS WHNAME " +
                        "   From T_CM_FCMT " +
                        "   Where Type = 'P' And Status = 'OK' AND SUBSTR(Factory,1,1) = 'P' " +
                        " ) MAIN ";
                    break;

                default:
                    sb = " SELECT * FROM PKERP.T_CM_WMMT WHERE STATUS = 'OK' ";
                    break;
            }
             
            return OracleDbManager.GetObjects<WHMT>(sb, CommandType.Text, null, EnumDataSource.OdpConnStr);
        }

    }
}
