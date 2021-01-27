using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace OPS_DAL.Business
{
    public class IccmBus
    {
        /// <summary>
        /// Get list color by item code
        /// </summary>
        /// <param name="itemCode"></param>
        /// <returns></returns>
        public static List<Iccm> GetListItemCocolor(string itemCode, string itemColorSerial)
        {
            string strSql = @"SELECT * FROM T_00_ICCM WHERE ITEMCODE = :P_ITEMCODE ";

            var colorSerialCon = "AND ITEMCOLORSERIAL = :P_ITEMCOLORSERIAL";

            List<OracleParameter> oraParam = new List<OracleParameter>
            {
                new OracleParameter("P_ITEMCODE", itemCode)
            };

            if (!string.IsNullOrEmpty(itemColorSerial))
            {
                strSql += colorSerialCon;
                oraParam.Add(new OracleParameter("P_ITEMCOLORSERIAL", itemColorSerial));
            }
            
            var listColor = OracleDbManager.GetObjects<Iccm>(strSql, CommandType.Text, oraParam.ToArray());
            return listColor;
        }

        #region MySQL
        /// <summary>
        /// Get list item color
        /// </summary>
        /// <param name="itemCode"></param>
        /// <returns></returns>
        public static List<Iccm> GetListItemColorMySql(string itemCode, string itemColorSerial)
        {
            string strSql = @"SELECT * FROM T_00_ICCM WHERE ITEMCODE = ?P_ITEMCODE ";

            var colorSerialCon = " AND ITEMCOLORSERIAL = :P_ITEMCOLORSERIAL";

            List<MySqlParameter> myParam = new List<MySqlParameter>
            {
                new MySqlParameter("P_ITEMCODE", itemCode)
            };

            if (!string.IsNullOrEmpty(itemColorSerial))
            {
                strSql += colorSerialCon;
                myParam.Add(new MySqlParameter("P_ITEMCOLORSERIAL", itemColorSerial));
            }

            var listColor = MySqlDBManager.GetAll<Iccm>(strSql, CommandType.Text, myParam.ToArray());
            return listColor;
        }

        /// <summary>
        /// Insert item color to MySQL
        /// </summary>
        /// <param name="iccm"></param>
        /// <param name="myTrans"></param>
        /// <param name="myConnection"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertItemColorMySQL(Iccm iccm, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @" INSERT INTO T_00_ICCM(
	                                ITEMCODE, ITEMCOLORWAYS, ITEMCOLORSERIAL, ITEMCOLOR, RGBVALUE, OLD_ITEMCODE, ORIGINAL, TRIAL, DE, REGISTER
	                                , REGISTRYDATE, LASTMODIFIER, LASTMODIFYDATE, OLD_ITEMCOLORSERIAL, SEASONCODE, SEASONCOLORSERIAL)
                                VALUES (
	                                ?P_ITEMCODE, ?P_ITEMCOLORWAYS, ?P_ITEMCOLORSERIAL, ?P_ITEMCOLOR, ?P_RGBVALUE, ?P_OLD_ITEMCODE, ?P_ORIGINAL, ?P_TRIAL, ?P_DE, ?P_REGISTER
	                                , ?P_REGISTRYDATE, ?P_LASTMODIFIER, ?P_LASTMODIFYDATE, ?P_OLD_ITEMCOLORSERIAL, ?P_SEASONCODE, ?P_SEASONCOLORSERIAL
                                );";
            List<MySqlParameter> myParam = new List<MySqlParameter>
            {
                new MySqlParameter("P_ITEMCODE", iccm.ItemCode),
                new MySqlParameter("P_ITEMCOLORWAYS", iccm.ItemColorways),
                new MySqlParameter("P_ITEMCOLORSERIAL", iccm.ItemColorSerial),
                new MySqlParameter("P_ITEMCOLOR", iccm.ItemColor),
                new MySqlParameter("P_RGBVALUE", iccm.RGBValue),
                new MySqlParameter("P_OLD_ITEMCODE", iccm.Old_ItemCode),
                new MySqlParameter("P_ORIGINAL", iccm.Original),
                new MySqlParameter("P_TRIAL", iccm.Trial),
                new MySqlParameter("P_DE", iccm.De),
                new MySqlParameter("P_REGISTER", iccm.Register),
                new MySqlParameter("P_REGISTRYDATE", iccm.RegistryDate),
                new MySqlParameter("P_LASTMODIFIER", iccm.LastModifier),
                new MySqlParameter("P_LASTMODIFYDATE", iccm.LastModifyDate),
                new MySqlParameter("P_OLD_ITEMCOLORSERIAL", iccm.Old_ItemColorSerial),
                new MySqlParameter("P_SEASONCODE", iccm.SeasonCode),
                new MySqlParameter("P_SEASONCOLORSERIAL", iccm.SeasonColorSerial)

            };
            var resIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, myParam.ToArray(), CommandType.Text, myTrans, myConnection);

            return resIns != null;
        }

        public static bool InsertItemColorMySQL(Iccm iccm)
        {
            string strSql = @" INSERT INTO T_00_ICCM(
	                                ITEMCODE, ITEMCOLORWAYS, ITEMCOLORSERIAL, ITEMCOLOR, RGBVALUE, OLD_ITEMCODE, ORIGINAL, TRIAL, DE, REGISTER
	                                , REGISTRYDATE, LASTMODIFIER, LASTMODIFYDATE, OLD_ITEMCOLORSERIAL, SEASONCODE, SEASONCOLORSERIAL)
                                VALUES (
	                                ?P_ITEMCODE, ?P_ITEMCOLORWAYS, ?P_ITEMCOLORSERIAL, ?P_ITEMCOLOR, ?P_RGBVALUE, ?P_OLD_ITEMCODE, ?P_ORIGINAL, ?P_TRIAL, ?P_DE, ?P_REGISTER
	                                , ?P_REGISTRYDATE, ?P_LASTMODIFIER, ?P_LASTMODIFYDATE, ?P_OLD_ITEMCOLORSERIAL, ?P_SEASONCODE, ?P_SEASONCOLORSERIAL
                                );";
            List<MySqlParameter> myParam = new List<MySqlParameter>
            {
                new MySqlParameter("P_ITEMCODE", iccm.ItemCode),
                new MySqlParameter("P_ITEMCOLORWAYS", iccm.ItemColorways),
                new MySqlParameter("P_ITEMCOLORSERIAL", iccm.ItemColorSerial),
                new MySqlParameter("P_ITEMCOLOR", iccm.ItemColor),
                new MySqlParameter("P_RGBVALUE", iccm.RGBValue),
                new MySqlParameter("P_OLD_ITEMCODE", iccm.Old_ItemCode),
                new MySqlParameter("P_ORIGINAL", iccm.Original),
                new MySqlParameter("P_TRIAL", iccm.Trial),
                new MySqlParameter("P_DE", iccm.De),
                new MySqlParameter("P_REGISTER", iccm.Register),
                new MySqlParameter("P_REGISTRYDATE", iccm.RegistryDate),
                new MySqlParameter("P_LASTMODIFIER", iccm.LastModifier),
                new MySqlParameter("P_LASTMODIFYDATE", iccm.LastModifyDate),
                new MySqlParameter("P_OLD_ITEMCOLORSERIAL", iccm.Old_ItemColorSerial),
                new MySqlParameter("P_SEASONCODE", iccm.SeasonCode),
                new MySqlParameter("P_SEASONCOLORSERIAL", iccm.SeasonColorSerial)

            };
            var resIns = MySqlDBManager.ExecuteQuery(strSql, CommandType.Text, myParam.ToArray());

            return resIns != null;
        }
        #endregion
    }
}
