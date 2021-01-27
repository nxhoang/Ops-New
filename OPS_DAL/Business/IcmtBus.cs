using OPS_DAL.Entities;
using OPS_DAL.DAL;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using OPS_Utils;
using System;
using MySql.Data.MySqlClient;

namespace OPS_DAL.Business
{
    public class IcmtBus
    {      
        /// <summary>
        /// Get item code information by item code
        /// </summary>
        /// <param name="itemCode"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static Icmt GetItemCode(string itemCode)
        {
            string strSql = @"SELECT ITEMCODE, OLD_ITEMCODE OldItemCode, ITEMNAME, IMPORTFULLNAME, MAINLEVEL, LEVELNO_01 LevelNo01, LEVELNO_02 LevelNo02, LEVELNO_03 LevelNo03, LEVELNO_04 LevelNo04, LEVELNO_05 LevelNo05
                                , LEVELNO_06 LevelNo06, LEVELNO_07 LevelNo07, LEVELNO_08 LevelNo08, LEVELNO_09 LevelNo09, HSCODE, PRINTGRADE, NDC, PLANTYPE, MINORDERSIZE, ITEMREGISTER
                                , REGISTRYDATE, LASTMODIDATE, SOS, CONSUMPUNIT, SIZEUNIT, WIDTH, ENDWISE, THICKNESS, HEIGHT, WEIGHTUNIT
                                , GRAVITY, WEIGHT, PACKINGUNIT, PACKINGQTY, PACKINGSIZEUNIT, PACKINGWIDTH, PACKINGENDWISE, PACKINGHEIGHT, ITEMUSE, CURRCODE
                                , STDPRICE, REMARKS, STANDARD, PURCHASER, CBM, LEADTIME, ESTPRICE, BUYER, TEMPFILE, USABLEWIDTH
                                , POQTYUNIT, LATESTPOSOS, LATESTBUYER, NEWITEM, CUSTOMSCODE, USERITEMNAME, BUYERITEMCODE, SUPPLIERITEMCODE, LEVELNO_10 LevelNo10, PLACINGTYPE
                                , IMPORTEDDATE, PACKINGGWEIGHT 
                            FROM T_00_ICMT WHERE ITEMCODE = :P_ITEMCODE";
            List<OracleParameter> oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_ITEMCODE", itemCode),

            };
            var lstItemLevel = OracleDbManager.GetObjectsByType<Icmt>(strSql, CommandType.Text, oracleParams.ToArray());
            return lstItemLevel.FirstOrDefault();
        }

        /// <summary>
        /// Get item code master.
        /// </summary>
        /// <param name="mainLevel"></param>
        /// <param name="levelNo"></param>
        /// <param name="buyer"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Icmt GetItemCode(string mainLevel, string levelNo, string buyer)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            List<OracleParameter> oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_MAINLEVEL", mainLevel),
                new OracleParameter("P_LEVELNO", levelNo),
                new OracleParameter("P_BUYER", buyer),
                cursor
            };
            var lstItemLevel = OracleDbManager.GetObjects<Icmt>("SP_OPS_GETITEMCODE_ICMT", CommandType.StoredProcedure, oracleParams.ToArray());
            return lstItemLevel.FirstOrDefault();
        }

        public static Icmt GetItemCodeByLevel(string mainLevel, string levelNoCode, string levelNo, string buyer)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            List<OracleParameter> oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_MAINLEVEL", mainLevel),
                new OracleParameter("P_LEVELNOCODE", levelNoCode),
                new OracleParameter("P_BUYER", buyer),
                new OracleParameter("P_LEVELNO", levelNo),
                cursor
            };
            var lstItemLevel = OracleDbManager.GetObjects<Icmt>("SP_OPS_GETITEMCODEBYLEVEL_ICMT", CommandType.StoredProcedure, oracleParams.ToArray());
            return lstItemLevel.FirstOrDefault();
        }

        /// <summary>
        /// Get list of icmt
        /// </summary>
        /// <param name="mainLevel"></param>
        /// <param name="buyer"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static List<Icmt> GetListIcmt(string mainLevel, string buyer)
        {
            var strSql = @"SELECT ITEMCODE, ITEMNAME, MAINLEVEL, LEVELNO_01 AS LEVELNO01, LEVELNO_02 AS LEVELNO02
                            FROM T_00_ICMT 
                            WHERE MAINLEVEL = :P_MAINLEVEL AND BUYER = :P_BUYER";

            List<OracleParameter> oraParams = new List<OracleParameter>
            {
                new OracleParameter("P_MAINLEVEL", mainLevel),
                new OracleParameter("P_BUYER", buyer),
            };

            var lstIcmt = OracleDbManager.GetObjects<Icmt>(strSql, CommandType.Text, oraParams.ToArray());

            return lstIcmt;
        }

        /// <summary>
        /// Get max item code.
        /// </summary>
        /// <param name="mainLevel"></param>
        /// <param name="buyer"></param>
        /// <returns></returns>
        public static string GetMaxItemCode(string mainLevel, string buyer)
        {
            var strSql = @"SELECT MAX(ITEMCODE) MAXITEMCODE
                            FROM T_00_ICMT 
                            WHERE MAINLEVEL = :P_MAINLEVEL AND BUYER = :P_BUYER";

            List<OracleParameter> oraParams = new List<OracleParameter>
            {
                new OracleParameter("P_MAINLEVEL", mainLevel),
                new OracleParameter("P_BUYER", buyer),
            };

            var icmt = OracleDbManager.GetObjects<Icmt>(strSql, CommandType.Text, oraParams.ToArray()).FirstOrDefault();
            if (icmt.MaxItemCode == null) return mainLevel + buyer + "0000000";

            return icmt.MaxItemCode;
        }

        public static string GetMaxItemCodeModule(string mainLevel, string buyer)
        {
            var strSql = @"SELECT MAX(ITEMCODE) MAXITEMCODE
                            FROM T_00_ICMT 
                            WHERE MAINLEVEL = :P_MAINLEVEL AND BUYER = :P_BUYER AND TO_NUMBER(SUBSTR(ITEMCODE, 7,7)) < 99"; //99 is Everything else module

            List<OracleParameter> oraParams = new List<OracleParameter>
            {
                new OracleParameter("P_MAINLEVEL", mainLevel),
                new OracleParameter("P_BUYER", buyer),
            };

            var icmt = OracleDbManager.GetObjects<Icmt>(strSql, CommandType.Text, oraParams.ToArray()).FirstOrDefault();
            if (icmt.MaxItemCode == null) return mainLevel + buyer + "0000000";

            return icmt.MaxItemCode;
        }

        /// <summary>
        /// Insert item master.
        /// </summary>
        /// <param name="icmt"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static bool InsertItemCode(Icmt icmt, OracleConnection oraConn, OracleTransaction trans)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) {Direction = ParameterDirection.Output},
                new OracleParameter("P_ITEMCODE", icmt.ItemCode),
                new OracleParameter("P_ITEMNAME", icmt.ItemName),
                new OracleParameter("P_MAINLEVEL", icmt.MainLevel),
                new OracleParameter("P_LEVELNO_01", icmt.LevelNo01),
                new OracleParameter("P_LEVELNO_02", icmt.LevelNo02), //ADD) SON - 13/Jun/2019
                new OracleParameter("P_ITEMREGISTER", icmt.ItemRegister),
                new OracleParameter("P_REGISTRYDATE", icmt.RegistryDate),
                new OracleParameter("P_BUYER", icmt.Buyer)
            };

            var resInsert = OracleDbManager.ExecuteQuery("SP_OPS_INSERTITEMCODE_ICMT", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resInsert != null && int.Parse(resInsert.ToString()) != 0;
        }

        public static bool InsertItemCodeList(List<Icmt> lstIcmt)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    foreach (var icmt in lstIcmt)
                    {
                        //Insert module
                        if (InsertItemCode(icmt, connection, trans)) continue;

                        trans.Rollback();
                        return false;
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        #region MySql
        /// <summary>
        /// Get Item Code in MySQL
        /// </summary>
        /// <param name="itemCode"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Icmt GetItemCodeMySql(string itemCode)
        {
            string strSql = @"SELECT * FROM T_00_ICMT WHERE ITEMCODE = ?P_ITEMCODE";
            List<MySqlParameter> oracleParams = new List<MySqlParameter>
            {
                new MySqlParameter("P_ITEMCODE", itemCode),

            };
            var lstItemLevel = MySqlDBManager.GetAll<Icmt>(strSql, CommandType.Text, oracleParams.ToArray());
            return lstItemLevel.FirstOrDefault();
        }

        public static bool InsertItemCodeMySQL(Icmt icmt, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @" INSERT INTO T_00_ICMT (
                                      ITEMCODE, OLD_ITEMCODE, ITEMNAME, IMPORTFULLNAME, MAINLEVEL, LEVELNO_01, LEVELNO_02, LEVELNO_03, LEVELNO_04, LEVELNO_05
                                    , LEVELNO_06, LEVELNO_07, LEVELNO_08, LEVELNO_09, HSCODE, PRINTGRADE, NDC, PLANTYPE, MINORDERSIZE, ITEMREGISTER
                                    , REGISTRYDATE, LASTMODIDATE, SOS, CONSUMPUNIT, SIZEUNIT, WIDTH, ENDWISE, THICKNESS, HEIGHT, WEIGHTUNIT
                                    , GRAVITY, WEIGHT, PACKINGUNIT, PACKINGQTY, PACKINGSIZEUNIT, PACKINGWIDTH, PACKINGENDWISE, PACKINGHEIGHT, ITEMUSE, CURRCODE
                                    , STDPRICE, REMARKS, STANDARD, PURCHASER, CBM, LEADTIME, ESTPRICE, BUYER, TEMPFILE, USABLEWIDTH
                                    , POQTYUNIT, LATESTPOSOS, LATESTBUYER, NEWITEM, CUSTOMSCODE, USERITEMNAME, BUYERITEMCODE, SUPPLIERITEMCODE, LEVELNO_10, PLACINGTYPE
                                    , IMPORTEDDATE, PACKINGGWEIGHT 
                                    )
                            VALUES (
                                  ?P_ITEMCODE, ?P_OLD_ITEMCODE, ?P_ITEMNAME, ?P_IMPORTFULLNAME, ?P_MAINLEVEL, ?P_LEVELNO_01, ?P_LEVELNO_02, ?P_LEVELNO_03, ?P_LEVELNO_04, ?P_LEVELNO_05
                                , ?P_LEVELNO_06, ?P_LEVELNO_07, ?P_LEVELNO_08, ?P_LEVELNO_09, ?P_HSCODE, ?P_PRINTGRADE, ?P_NDC, ?P_PLANTYPE, ?P_MINORDERSIZE, ?P_ITEMREGISTER
                                , ?P_REGISTRYDATE, ?P_LASTMODIDATE, ?P_SOS, ?P_CONSUMPUNIT, ?P_SIZEUNIT, ?P_WIDTH, ?P_ENDWISE, ?P_THICKNESS, ?P_HEIGHT, ?P_WEIGHTUNIT
                                , ?P_GRAVITY, ?P_WEIGHT, ?P_PACKINGUNIT, ?P_PACKINGQTY, ?P_PACKINGSIZEUNIT, ?P_PACKINGWIDTH, ?P_PACKINGENDWISE, ?P_PACKINGHEIGHT, ?P_ITEMUSE, ?P_CURRCODE
                                , ?P_STDPRICE, ?P_REMARKS, ?P_STANDARD, ?P_PURCHASER, ?P_CBM, ?P_LEADTIME, ?P_ESTPRICE, ?P_BUYER, ?P_TEMPFILE, ?P_USABLEWIDTH
                                , ?P_POQTYUNIT, ?P_LATESTPOSOS, ?P_LATESTBUYER, ?P_NEWITEM, ?P_CUSTOMSCODE, ?P_USERITEMNAME, ?P_BUYERITEMCODE, ?P_SUPPLIERITEMCODE, ?P_LEVELNO_10, ?P_PLACINGTYPE
                                , ?P_IMPORTEDDATE, ?P_PACKINGGWEIGHT
                            )";
            List<MySqlParameter> myParam = new List<MySqlParameter>
            {
                new MySqlParameter("P_ITEMCODE", icmt.ItemCode),
                new MySqlParameter("P_OLD_ITEMCODE", icmt.OldtemCode),
                new MySqlParameter("P_ITEMNAME", icmt.ItemName),
                new MySqlParameter("P_IMPORTFULLNAME", icmt.ImportFullName),
                new MySqlParameter("P_MAINLEVEL", icmt.MainLevel),
                new MySqlParameter("P_LEVELNO_01", icmt.LevelNo01),
                new MySqlParameter("P_LEVELNO_02", icmt.LevelNo02),
                new MySqlParameter("P_LEVELNO_03", icmt.LevelNo03),
                new MySqlParameter("P_LEVELNO_04", icmt.LevelNo04),
                new MySqlParameter("P_LEVELNO_05", icmt.LevelNo05),
                new MySqlParameter("P_LEVELNO_06", icmt.LevelNo06),
                new MySqlParameter("P_LEVELNO_07", icmt.LevelNo07),
                new MySqlParameter("P_LEVELNO_08", icmt.LevelNo08),
                new MySqlParameter("P_LEVELNO_09", icmt.LevelNo09),
                new MySqlParameter("P_HSCODE", icmt.HsCode),
                new MySqlParameter("P_PRINTGRADE", icmt.PrintGrade),
                new MySqlParameter("P_NDC", icmt.Ndc),
                new MySqlParameter("P_PLANTYPE", icmt.PlanType),
                new MySqlParameter("P_MINORDERSIZE", icmt.MinOrderSize),
                new MySqlParameter("P_ITEMREGISTER", icmt.ItemRegister),
                new MySqlParameter("P_REGISTRYDATE", icmt.RegistryDate),
                new MySqlParameter("P_LASTMODIDATE", icmt.LastModiDate),
                new MySqlParameter("P_SOS", icmt.Sos),
                new MySqlParameter("P_CONSUMPUNIT", icmt.ConsumpUnit),
                new MySqlParameter("P_SIZEUNIT", icmt.SizeUnit),
                new MySqlParameter("P_WIDTH", icmt.Width),
                new MySqlParameter("P_ENDWISE", icmt.EndWise),
                new MySqlParameter("P_THICKNESS", icmt.Thickness),
                new MySqlParameter("P_HEIGHT", icmt.Height),
                new MySqlParameter("P_WEIGHTUNIT", icmt.WeightUnit),
                new MySqlParameter("P_GRAVITY", icmt.Gravity),
                new MySqlParameter("P_WEIGHT", icmt.Weight),
                new MySqlParameter("P_PACKINGUNIT", icmt.PackingUnit),
                new MySqlParameter("P_PACKINGQTY", icmt.PackingQty),
                new MySqlParameter("P_PACKINGSIZEUNIT", icmt.PackingSizeUnit),
                new MySqlParameter("P_PACKINGWIDTH", icmt.PackingWidth),
                new MySqlParameter("P_PACKINGENDWISE", icmt.PackingEndWise),
                new MySqlParameter("P_PACKINGHEIGHT", icmt.PackingHeight),
                new MySqlParameter("P_ITEMUSE", icmt.ItemUse),
                new MySqlParameter("P_CURRCODE", icmt.CurrCode),
                new MySqlParameter("P_STDPRICE", icmt.StdPrice),
                new MySqlParameter("P_REMARKS", icmt.Remarks),
                new MySqlParameter("P_STANDARD", icmt.Standard),
                new MySqlParameter("P_PURCHASER", icmt.Purchaser),
                new MySqlParameter("P_CBM", icmt.Cbm),
                new MySqlParameter("P_LEADTIME", icmt.LeadTime),
                new MySqlParameter("P_ESTPRICE", icmt.EstPrice),
                new MySqlParameter("P_BUYER", icmt.Buyer),
                new MySqlParameter("P_TEMPFILE", icmt.TempFile),
                new MySqlParameter("P_USABLEWIDTH", icmt.UsableWidth),
                new MySqlParameter("P_POQTYUNIT", icmt.PoQtyUnit),
                new MySqlParameter("P_LATESTPOSOS", icmt.LatestPoSos),
                new MySqlParameter("P_LATESTBUYER", icmt.LatestBuyer),
                new MySqlParameter("P_NEWITEM", icmt.NewItem),
                new MySqlParameter("P_CUSTOMSCODE", icmt.CustomsCode),
                new MySqlParameter("P_USERITEMNAME", icmt.UserItemName),
                new MySqlParameter("P_BUYERITEMCODE", icmt.BuyerItemCode),
                new MySqlParameter("P_SUPPLIERITEMCODE", icmt.SupplierItemCode),
                new MySqlParameter("P_LEVELNO_10", icmt.LevelNo10),
                new MySqlParameter("P_PLACINGTYPE", icmt.PlacingType),
                new MySqlParameter("P_IMPORTEDDATE", icmt.ImportedDate),
                new MySqlParameter("P_PACKINGGWEIGHT", icmt.PackingGWeight)
            };
            var resIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, myParam.ToArray(), CommandType.Text, myTrans, myConnection);

            return resIns != null;
        }

        public static bool InsertItemCodeMySQL(Icmt icmt)
        {
            string strSql = @" INSERT INTO T_00_ICMT (
                                      ITEMCODE, OLD_ITEMCODE, ITEMNAME, IMPORTFULLNAME, MAINLEVEL, LEVELNO_01, LEVELNO_02, LEVELNO_03, LEVELNO_04, LEVELNO_05
                                    , LEVELNO_06, LEVELNO_07, LEVELNO_08, LEVELNO_09, HSCODE, PRINTGRADE, NDC, PLANTYPE, MINORDERSIZE, ITEMREGISTER
                                    , REGISTRYDATE, LASTMODIDATE, SOS, CONSUMPUNIT, SIZEUNIT, WIDTH, ENDWISE, THICKNESS, HEIGHT, WEIGHTUNIT
                                    , GRAVITY, WEIGHT, PACKINGUNIT, PACKINGQTY, PACKINGSIZEUNIT, PACKINGWIDTH, PACKINGENDWISE, PACKINGHEIGHT, ITEMUSE, CURRCODE
                                    , STDPRICE, REMARKS, STANDARD, PURCHASER, CBM, LEADTIME, ESTPRICE, BUYER, TEMPFILE, USABLEWIDTH
                                    , POQTYUNIT, LATESTPOSOS, LATESTBUYER, NEWITEM, CUSTOMSCODE, USERITEMNAME, BUYERITEMCODE, SUPPLIERITEMCODE, LEVELNO_10, PLACINGTYPE
                                    , IMPORTEDDATE, PACKINGGWEIGHT 
                                    )
                            VALUES (
                                  ?P_ITEMCODE, ?P_OLD_ITEMCODE, ?P_ITEMNAME, ?P_IMPORTFULLNAME, ?P_MAINLEVEL, ?P_LEVELNO_01, ?P_LEVELNO_02, ?P_LEVELNO_03, ?P_LEVELNO_04, ?P_LEVELNO_05
                                , ?P_LEVELNO_06, ?P_LEVELNO_07, ?P_LEVELNO_08, ?P_LEVELNO_09, ?P_HSCODE, ?P_PRINTGRADE, ?P_NDC, ?P_PLANTYPE, ?P_MINORDERSIZE, ?P_ITEMREGISTER
                                , ?P_REGISTRYDATE, ?P_LASTMODIDATE, ?P_SOS, ?P_CONSUMPUNIT, ?P_SIZEUNIT, ?P_WIDTH, ?P_ENDWISE, ?P_THICKNESS, ?P_HEIGHT, ?P_WEIGHTUNIT
                                , ?P_GRAVITY, ?P_WEIGHT, ?P_PACKINGUNIT, ?P_PACKINGQTY, ?P_PACKINGSIZEUNIT, ?P_PACKINGWIDTH, ?P_PACKINGENDWISE, ?P_PACKINGHEIGHT, ?P_ITEMUSE, ?P_CURRCODE
                                , ?P_STDPRICE, ?P_REMARKS, ?P_STANDARD, ?P_PURCHASER, ?P_CBM, ?P_LEADTIME, ?P_ESTPRICE, ?P_BUYER, ?P_TEMPFILE, ?P_USABLEWIDTH
                                , ?P_POQTYUNIT, ?P_LATESTPOSOS, ?P_LATESTBUYER, ?P_NEWITEM, ?P_CUSTOMSCODE, ?P_USERITEMNAME, ?P_BUYERITEMCODE, ?P_SUPPLIERITEMCODE, ?P_LEVELNO_10, ?P_PLACINGTYPE
                                , ?P_IMPORTEDDATE, ?P_PACKINGGWEIGHT
                            )";
            List<MySqlParameter> myParam = new List<MySqlParameter>
            {
                new MySqlParameter("P_ITEMCODE", icmt.ItemCode),
                new MySqlParameter("P_OLD_ITEMCODE", icmt.OldtemCode),
                new MySqlParameter("P_ITEMNAME", icmt.ItemName),
                new MySqlParameter("P_IMPORTFULLNAME", icmt.ImportFullName),
                new MySqlParameter("P_MAINLEVEL", icmt.MainLevel),
                new MySqlParameter("P_LEVELNO_01", icmt.LevelNo01),
                new MySqlParameter("P_LEVELNO_02", icmt.LevelNo02),
                new MySqlParameter("P_LEVELNO_03", icmt.LevelNo03),
                new MySqlParameter("P_LEVELNO_04", icmt.LevelNo04),
                new MySqlParameter("P_LEVELNO_05", icmt.LevelNo05),
                new MySqlParameter("P_LEVELNO_06", icmt.LevelNo06),
                new MySqlParameter("P_LEVELNO_07", icmt.LevelNo07),
                new MySqlParameter("P_LEVELNO_08", icmt.LevelNo08),
                new MySqlParameter("P_LEVELNO_09", icmt.LevelNo09),
                new MySqlParameter("P_HSCODE", icmt.HsCode),
                new MySqlParameter("P_PRINTGRADE", icmt.PrintGrade),
                new MySqlParameter("P_NDC", icmt.Ndc),
                new MySqlParameter("P_PLANTYPE", icmt.PlanType),
                new MySqlParameter("P_MINORDERSIZE", icmt.MinOrderSize),
                new MySqlParameter("P_ITEMREGISTER", icmt.ItemRegister),
                new MySqlParameter("P_REGISTRYDATE", icmt.RegistryDate),
                new MySqlParameter("P_LASTMODIDATE", icmt.LastModiDate),
                new MySqlParameter("P_SOS", icmt.Sos),
                new MySqlParameter("P_CONSUMPUNIT", icmt.ConsumpUnit),
                new MySqlParameter("P_SIZEUNIT", icmt.SizeUnit),
                new MySqlParameter("P_WIDTH", icmt.Width),
                new MySqlParameter("P_ENDWISE", icmt.EndWise),
                new MySqlParameter("P_THICKNESS", icmt.Thickness),
                new MySqlParameter("P_HEIGHT", icmt.Height),
                new MySqlParameter("P_WEIGHTUNIT", icmt.WeightUnit),
                new MySqlParameter("P_GRAVITY", icmt.Gravity),
                new MySqlParameter("P_WEIGHT", icmt.Weight),
                new MySqlParameter("P_PACKINGUNIT", icmt.PackingUnit),
                new MySqlParameter("P_PACKINGQTY", icmt.PackingQty),
                new MySqlParameter("P_PACKINGSIZEUNIT", icmt.PackingSizeUnit),
                new MySqlParameter("P_PACKINGWIDTH", icmt.PackingWidth),
                new MySqlParameter("P_PACKINGENDWISE", icmt.PackingEndWise),
                new MySqlParameter("P_PACKINGHEIGHT", icmt.PackingHeight),
                new MySqlParameter("P_ITEMUSE", icmt.ItemUse),
                new MySqlParameter("P_CURRCODE", icmt.CurrCode),
                new MySqlParameter("P_STDPRICE", icmt.StdPrice),
                new MySqlParameter("P_REMARKS", icmt.Remarks),
                new MySqlParameter("P_STANDARD", icmt.Standard),
                new MySqlParameter("P_PURCHASER", icmt.Purchaser),
                new MySqlParameter("P_CBM", icmt.Cbm),
                new MySqlParameter("P_LEADTIME", icmt.LeadTime),
                new MySqlParameter("P_ESTPRICE", icmt.EstPrice),
                new MySqlParameter("P_BUYER", icmt.Buyer),
                new MySqlParameter("P_TEMPFILE", icmt.TempFile),
                new MySqlParameter("P_USABLEWIDTH", icmt.UsableWidth),
                new MySqlParameter("P_POQTYUNIT", icmt.PoQtyUnit),
                new MySqlParameter("P_LATESTPOSOS", icmt.LatestPoSos),
                new MySqlParameter("P_LATESTBUYER", icmt.LatestBuyer),
                new MySqlParameter("P_NEWITEM", icmt.NewItem),
                new MySqlParameter("P_CUSTOMSCODE", icmt.CustomsCode),
                new MySqlParameter("P_USERITEMNAME", icmt.UserItemName),
                new MySqlParameter("P_BUYERITEMCODE", icmt.BuyerItemCode),
                new MySqlParameter("P_SUPPLIERITEMCODE", icmt.SupplierItemCode),
                new MySqlParameter("P_LEVELNO_10", icmt.LevelNo10),
                new MySqlParameter("P_PLACINGTYPE", icmt.PlacingType),
                new MySqlParameter("P_IMPORTEDDATE", icmt.ImportedDate),
                new MySqlParameter("P_PACKINGGWEIGHT", icmt.PackingGWeight)
            };
            var resIns = MySqlDBManager.ExecuteQuery(strSql, CommandType.Text, myParam.ToArray());

            return resIns != null;
        }
        #endregion
    }
}
