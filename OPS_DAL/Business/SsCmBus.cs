using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OPS_DAL.Business
{
    public static class SsCmBus
    {
        /// <summary>
        /// Get Suppiers
        /// </summary>
        /// <returns>LIST SsCm</returns>
        /// Author: VitHV
        public static List<SsCm> GetSuppiers()
        {
            //const string sql = @"SELECT SOS
            //                    ,SHORTNAME
            //                    ,FULLNAME
            //                    ,NATION
            //            FROM T_CM_SSCM 
            //            WHERE  ACCCHK = 'Y' AND STATUS = 'OK'";
            const string sql = @"SELECT SOS
                                ,SHORTNAME
                                ,FULLNAME
                                ,NATION
                        FROM T_CM_SSCM 
                        WHERE  STATUS = 'OK' AND MACHINESUPPLIER = 'Y'";
            return OracleDbManager.GetObjects<SsCm>(sql, null);
        }
        
        /// <summary>
        /// Get sos information by sos code
        /// </summary>
        /// <param name="sos"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static SsCm GetSOS(string sos)
        {
            string strSql = @"SELECT * FROM T_CM_SSCM WHERE SOS = :P_SOS ";
            List<OracleParameter> oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_SOS", sos),

            };
            var sscm = OracleDbManager.GetObjects<SsCm>(strSql, CommandType.Text, oracleParams.ToArray());
            return sscm.FirstOrDefault();
        }

        #region MySQL
        /// <summary>
        /// Get sos information from MySQL
        /// </summary>
        /// <param name="sos"></param>
        /// <returns></returns>
        public static SsCm GetSOSMySQL(string sos)
        {
            string strSql = @"SELECT * FROM T_CM_SSCM WHERE SOS = ?P_SOS ";

            List<MySqlParameter> myParam = new List<MySqlParameter>
            {
                new MySqlParameter("P_SOS", sos)
            };
                        
            var ssCms = MySqlDBManager.GetObjectsConvertType<SsCm>(strSql, CommandType.Text, myParam.ToArray());
            return ssCms.FirstOrDefault();
        }

        /// <summary>
        /// Insert sos information to MySQL with transaction.
        /// </summary>
        /// <param name="sscm"></param>
        /// <param name="myTrans"></param>
        /// <param name="myConnection"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertSOSMySQL(SsCm sscm, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @" INSERT INTO T_CM_SSCM(  
	                                SOS, SHORTNAME, FULLNAME, NATION, REGION, INDUSTRY, ITEMS, ZIPCODE, ADDRESS, CHIEFUSERID
                                  , TEL, FAX, URL, CURRCODE, AVGLEADTIME, OUTCHECK, FOREIGNCHECK, CSUSE, INCOTERMS, BANKCODE
                                  , ACCOUNTNO, OPPTAPECODE, LOSS, CNCODE, CSMAKER, CNCHECK, BUYERCHK, SUPPLYCHK, TREATCHK, OUTCHK
                                  , PURCHASER, AGENT, STATUS, GACHK, ACCCHK, REGISTRYDATE, REGISTERID, MODIFYDATE, MODIFIER, TRANSTYPE
                                  , PAYCONDITION, SAPSUPPLIERID, EPR_USE, MRO_USE, EMAIL, NEWSOS, POSYSTEM, WEBADDRESS, MRP_USE, SOST1
                                  , NOMINATED, MACHINESUPPLIER 
                              )
                              VALUES (
                                    ?P_SOS, ?P_SHORTNAME, ?P_FULLNAME, ?P_NATION, ?P_REGION, ?P_INDUSTRY, ?P_ITEMS, ?P_ZIPCODE, ?P_ADDRESS, ?P_CHIEFUSERID
                                  , ?P_TEL, ?P_FAX, ?P_URL, ?P_CURRCODE, ?P_AVGLEADTIME, ?P_OUTCHECK, ?P_FOREIGNCHECK, ?P_CSUSE, ?P_INCOTERMS, ?P_BANKCODE
                                  , ?P_ACCOUNTNO, ?P_OPPTAPECODE, ?P_LOSS, ?P_CNCODE, ?P_CSMAKER, ?P_CNCHECK, ?P_BUYERCHK, ?P_SUPPLYCHK, ?P_TREATCHK, ?P_OUTCHK
                                  , ?P_PURCHASER, ?P_AGENT, ?P_STATUS, ?P_GACHK, ?P_ACCCHK, ?P_REGISTRYDATE, ?P_REGISTERID, ?P_MODIFYDATE, ?P_MODIFIER, ?P_TRANSTYPE
                                  , ?P_PAYCONDITION, ?P_SAPSUPPLIERID, ?P_EPR_USE, ?P_MRO_USE, ?P_EMAIL, ?P_NEWSOS, ?P_POSYSTEM, ?P_WEBADDRESS, ?P_MRP_USE, ?P_SOST1
                                  , ?P_NOMINATED, ?P_MACHINESUPPLIER
                              );";

            List<MySqlParameter> myParam = new List<MySqlParameter>
            {
                new MySqlParameter("P_SOS", sscm.Sos),
                new MySqlParameter("P_SHORTNAME", sscm.ShortName),
                new MySqlParameter("P_FULLNAME", sscm.FullName),
                new MySqlParameter("P_NATION", sscm.Nation),
                new MySqlParameter("P_REGION", sscm.Region),
                new MySqlParameter("P_INDUSTRY", sscm.Industry),
                new MySqlParameter("P_ITEMS", sscm.Items),
                new MySqlParameter("P_ZIPCODE", sscm.ZipCode),
                new MySqlParameter("P_ADDRESS", sscm.Address),
                new MySqlParameter("P_CHIEFUSERID", sscm.ChiefUserId),
                new MySqlParameter("P_TEL", sscm.Tel),
                new MySqlParameter("P_FAX", sscm.Fax),
                new MySqlParameter("P_URL", sscm.Url),
                new MySqlParameter("P_CURRCODE", sscm.CurrCode),
                new MySqlParameter("P_AVGLEADTIME", sscm.AvgLeadTime),
                new MySqlParameter("P_OUTCHECK", sscm.OutCheck),
                new MySqlParameter("P_FOREIGNCHECK", sscm.FOREIGNCHECK),
                new MySqlParameter("P_CSUSE", sscm.CSUSE),
                new MySqlParameter("P_INCOTERMS", sscm.INCOTERMS),
                new MySqlParameter("P_BANKCODE", sscm.BANKCODE),
                new MySqlParameter("P_ACCOUNTNO", sscm.ACCOUNTNO),
                new MySqlParameter("P_OPPTAPECODE", sscm.OPPTAPECODE),
                new MySqlParameter("P_LOSS", sscm.LOSS),
                new MySqlParameter("P_CNCODE", sscm.CNCODE),
                new MySqlParameter("P_CSMAKER", sscm.CSMAKER),
                new MySqlParameter("P_CNCHECK", sscm.CNCHECK),
                new MySqlParameter("P_BUYERCHK", sscm.BUYERCHK),
                new MySqlParameter("P_SUPPLYCHK", sscm.SUPPLYCHK),
                new MySqlParameter("P_TREATCHK", sscm.TREATCHK),
                new MySqlParameter("P_OUTCHK", sscm.OUTCHK),
                new MySqlParameter("P_PURCHASER", sscm.PURCHASER),
                new MySqlParameter("P_AGENT", sscm.AGENT),
                new MySqlParameter("P_STATUS", sscm.STATUS),
                new MySqlParameter("P_GACHK", sscm.GACHK),
                new MySqlParameter("P_ACCCHK", sscm.ACCCHK),
                new MySqlParameter("P_REGISTRYDATE", sscm.REGISTRYDATE),
                new MySqlParameter("P_REGISTERID", sscm.REGISTERID),
                new MySqlParameter("P_MODIFYDATE", sscm.MODIFYDATE),
                new MySqlParameter("P_MODIFIER", sscm.MODIFIER),
                new MySqlParameter("P_TRANSTYPE", sscm.TRANSTYPE),
                new MySqlParameter("P_PAYCONDITION", sscm.PAYCONDITION),
                new MySqlParameter("P_SAPSUPPLIERID", sscm.SAPSUPPLIERID),
                new MySqlParameter("P_EPR_USE", sscm.ERP_USE),
                new MySqlParameter("P_MRO_USE", sscm.MRO_USE),
                new MySqlParameter("P_EMAIL", sscm.EMAIL),
                new MySqlParameter("P_NEWSOS", sscm.NEWSOS),
                new MySqlParameter("P_POSYSTEM", sscm.POSYSTEM),
                new MySqlParameter("P_WEBADDRESS", sscm.WEBADDRESS),
                new MySqlParameter("P_MRP_USE", sscm.MRP_USE),
                new MySqlParameter("P_SOST1", sscm.SOST1),
                new MySqlParameter("P_NOMINATED", sscm.NOMINATED),
                new MySqlParameter("P_MACHINESUPPLIER", sscm.MACHINESUPPLIER)
            };

            var resIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, myParam.ToArray(), CommandType.Text, myTrans, myConnection);

            return resIns != null;
        }

        /// <summary>
        /// Insert sos to MySQL without transaction
        /// </summary>
        /// <param name="sscm"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertSOSMySQL(SsCm sscm)
        {
            string strSql = @" INSERT INTO T_CM_SSCM(  
	                                SOS, SHORTNAME, FULLNAME, NATION, REGION, INDUSTRY, ITEMS, ZIPCODE, ADDRESS, CHIEFUSERID
                                  , TEL, FAX, URL, CURRCODE, AVGLEADTIME, OUTCHECK, FOREIGNCHECK, CSUSE, INCOTERMS, BANKCODE
                                  , ACCOUNTNO, OPPTAPECODE, LOSS, CNCODE, CSMAKER, CNCHECK, BUYERCHK, SUPPLYCHK, TREATCHK, OUTCHK
                                  , PURCHASER, AGENT, STATUS, GACHK, ACCCHK, REGISTRYDATE, REGISTERID, MODIFYDATE, MODIFIER, TRANSTYPE
                                  , PAYCONDITION, SAPSUPPLIERID, EPR_USE, MRO_USE, EMAIL, NEWSOS, POSYSTEM, WEBADDRESS, MRP_USE, SOST1
                                  , NOMINATED, MACHINESUPPLIER 
                              )
                              VALUES (
                                    ?P_SOS, ?P_SHORTNAME, ?P_FULLNAME, ?P_NATION, ?P_REGION, ?P_INDUSTRY, ?P_ITEMS, ?P_ZIPCODE, ?P_ADDRESS, ?P_CHIEFUSERID
                                  , ?P_TEL, ?P_FAX, ?P_URL, ?P_CURRCODE, ?P_AVGLEADTIME, ?P_OUTCHECK, ?P_FOREIGNCHECK, ?P_CSUSE, ?P_INCOTERMS, ?P_BANKCODE
                                  , ?P_ACCOUNTNO, ?P_OPPTAPECODE, ?P_LOSS, ?P_CNCODE, ?P_CSMAKER, ?P_CNCHECK, ?P_BUYERCHK, ?P_SUPPLYCHK, ?P_TREATCHK, ?P_OUTCHK
                                  , ?P_PURCHASER, ?P_AGENT, ?P_STATUS, ?P_GACHK, ?P_ACCCHK, ?P_REGISTRYDATE, ?P_REGISTERID, ?P_MODIFYDATE, ?P_MODIFIER, ?P_TRANSTYPE
                                  , ?P_PAYCONDITION, ?P_SAPSUPPLIERID, ?P_EPR_USE, ?P_MRO_USE, ?P_EMAIL, ?P_NEWSOS, ?P_POSYSTEM, ?P_WEBADDRESS, ?P_MRP_USE, ?P_SOST1
                                  , ?P_NOMINATED, ?P_MACHINESUPPLIER
                              );";

            List<MySqlParameter> myParam = new List<MySqlParameter>
            {
                new MySqlParameter("P_SOS", sscm.Sos),
                new MySqlParameter("P_SHORTNAME", sscm.ShortName),
                new MySqlParameter("P_FULLNAME", sscm.FullName),
                new MySqlParameter("P_NATION", sscm.Nation),
                new MySqlParameter("P_REGION", sscm.Region),
                new MySqlParameter("P_INDUSTRY", sscm.Industry),
                new MySqlParameter("P_ITEMS", sscm.Items),
                new MySqlParameter("P_ZIPCODE", sscm.ZipCode),
                new MySqlParameter("P_ADDRESS", sscm.Address),
                new MySqlParameter("P_CHIEFUSERID", sscm.ChiefUserId),
                new MySqlParameter("P_TEL", sscm.Tel),
                new MySqlParameter("P_FAX", sscm.Fax),
                new MySqlParameter("P_URL", sscm.Url),
                new MySqlParameter("P_CURRCODE", sscm.CurrCode),
                new MySqlParameter("P_AVGLEADTIME", sscm.AvgLeadTime),
                new MySqlParameter("P_OUTCHECK", sscm.OutCheck),
                new MySqlParameter("P_FOREIGNCHECK", sscm.FOREIGNCHECK),
                new MySqlParameter("P_CSUSE", sscm.CSUSE),
                new MySqlParameter("P_INCOTERMS", sscm.INCOTERMS),
                new MySqlParameter("P_BANKCODE", sscm.BANKCODE),
                new MySqlParameter("P_ACCOUNTNO", sscm.ACCOUNTNO),
                new MySqlParameter("P_OPPTAPECODE", sscm.OPPTAPECODE),
                new MySqlParameter("P_LOSS", sscm.LOSS),
                new MySqlParameter("P_CNCODE", sscm.CNCODE),
                new MySqlParameter("P_CSMAKER", sscm.CSMAKER),
                new MySqlParameter("P_CNCHECK", sscm.CNCHECK),
                new MySqlParameter("P_BUYERCHK", sscm.BUYERCHK),
                new MySqlParameter("P_SUPPLYCHK", sscm.SUPPLYCHK),
                new MySqlParameter("P_TREATCHK", sscm.TREATCHK),
                new MySqlParameter("P_OUTCHK", sscm.OUTCHK),
                new MySqlParameter("P_PURCHASER", sscm.PURCHASER),
                new MySqlParameter("P_AGENT", sscm.AGENT),
                new MySqlParameter("P_STATUS", sscm.STATUS),
                new MySqlParameter("P_GACHK", sscm.GACHK),
                new MySqlParameter("P_ACCCHK", sscm.ACCCHK),
                new MySqlParameter("P_REGISTRYDATE", sscm.REGISTRYDATE),
                new MySqlParameter("P_REGISTERID", sscm.REGISTERID),
                new MySqlParameter("P_MODIFYDATE", sscm.MODIFYDATE),
                new MySqlParameter("P_MODIFIER", sscm.MODIFIER),
                new MySqlParameter("P_TRANSTYPE", sscm.TRANSTYPE),
                new MySqlParameter("P_PAYCONDITION", sscm.PAYCONDITION),
                new MySqlParameter("P_SAPSUPPLIERID", sscm.SAPSUPPLIERID),
                new MySqlParameter("P_EPR_USE", sscm.ERP_USE),
                new MySqlParameter("P_MRO_USE", sscm.MRO_USE),
                new MySqlParameter("P_EMAIL", sscm.EMAIL),
                new MySqlParameter("P_NEWSOS", sscm.NEWSOS),
                new MySqlParameter("P_POSYSTEM", sscm.POSYSTEM),
                new MySqlParameter("P_WEBADDRESS", sscm.WEBADDRESS),
                new MySqlParameter("P_MRP_USE", sscm.MRP_USE),
                new MySqlParameter("P_SOST1", sscm.SOST1),
                new MySqlParameter("P_NOMINATED", sscm.NOMINATED),
                new MySqlParameter("P_MACHINESUPPLIER", sscm.MACHINESUPPLIER)
            };

            var resIns = MySqlDBManager.ExecuteQuery(strSql, CommandType.Text, myParam.ToArray());

            return resIns != null;
        }
        #endregion
    }
}
