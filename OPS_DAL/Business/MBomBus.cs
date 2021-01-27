using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Business
{
    public class MBomBus
    {
        /// <summary>
        /// Get list modules which linked material
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<MBom> GetModulesMbom(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            var strSql = @" SELECT DISTINCT MBO.STYLECODE, MBO.STYLESIZE, MBO.STYLECOLORSERIAL, MBO.REVNO, MBO.MODULEITEMCODE, SAM.MODULENAME, MBOH.LINKED
                            FROM T_SD_MBOM MBO
                                JOIN T_00_SAMT SAM ON SAM.MODULEID = MBO.MODULEITEMCODE AND SAM.STYLECODE = MBO.STYLECODE
                                LEFT JOIN T_SD_MBOH MBOH ON MBOH.STYLECODE = MBO.STYLECODE AND MBOH.STYLESIZE = MBO.STYLESIZE AND MBOH.STYLECOLORSERIAL = MBO.STYLECOLORSERIAL AND MBOH.REVNO = MBO.REVNO AND MBOH.MODULEID = MBO.MODULEITEMCODE                                
                                WHERE MBO.STYLECODE = :P_STYLECODE AND MBO.STYLESIZE = :P_STYLESIZE AND MBO.STYLECOLORSERIAL = :P_STYLECOLORSERIAL AND MBO.REVNO = :P_REVNO
                            ORDER BY MBO.MODULEITEMCODE ";

            List<OracleParameter> oraParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLESIZE", styleSize),
                new OracleParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new OracleParameter("P_REVNO", revNo)
            };

            var listMBom = OracleDbManager.GetObjects<MBom>(strSql, CommandType.Text, oraParams.ToArray());

            return listMBom;
        }

        public static List<MBom> GetModulesByModuleId(string styleCode, string moduelId)
        {
            var strSql = @" SELECT STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, MODULEITEMCODE
                            FROM T_SD_MBOM  
                            WHERE STYLECODE = :P_STYLECODE AND MODULEITEMCODE = :P_MODULEITEMCODE ";

            List<OracleParameter> oraParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_MODULEITEMCODE", moduelId)                
            };

            var listMBom = OracleDbManager.GetObjects<MBom>(strSql, CommandType.Text, oraParams.ToArray());

            return listMBom;
        }

        /// <summary>
        /// Get Bom module by style code
        /// </summary>
        /// <param name="stlCode"></param>
        /// <param name="stlSize"></param>
        /// <param name="stlColorSerial"></param>
        /// <param name="stlRevNo"></param>
        /// <returns></returns>
        public static List<MBom> GetMBOMByStyleCode(string stlCode, string stlSize, string stlColorSerial, string stlRevNo)
        {
            var strSql = @"  SELECT * FROM T_SD_MBOM WHERE STYLECODE = :P_STYLECODE AND STYLESIZE = :P_STYLESIZE AND STYLECOLORSERIAL = :P_STYLECOLORSERIAL AND REVNO = :P_REVNO ";

            var oraPrams = new OpsParams(stlCode, stlSize, stlColorSerial, stlRevNo);

            return OracleDbManager.GetObjects<MBom>(strSql, oraPrams.ToArray());
        }

        #region MySQL

        /// <summary>
        /// Get list MBOM
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static List<MBom> GetListMBOMMySQL(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            string strSql = @"SELECT icm.itemname, concat(ccm.ITEMCOLORSERIAL, ' - ', ccm.ITEMCOLORWAYS )  ItemColor, scm.FullName , bom.* 
                                FROM T_SD_mBOM bom
	                                left join t_00_icmt icm on icm.itemcode = bom.itemcode
	                                left join t_00_iccm ccm on ccm.itemcode = bom.itemcode and ccm.ITEMCOLORSERIAL = bom.itemcolorserial
	                                left join t_cm_sscm scm on scm.SOS =  bom.SOS
                                WHERE bom.STYLECODE = ?P_STYLECODE AND bom.STYLESIZE = ?P_STYLESIZE 
                                      AND bom.STYLECOLORSERIAL = ?P_STYLECOLORSERIAL AND bom.REVNO = ?P_REVNO; ";

            List<MySqlParameter> myParam = new List<MySqlParameter>
            {
                new MySqlParameter("P_STYLECODE", styleCode),
                new MySqlParameter("P_STYLESIZE", styleSize),
                new MySqlParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new MySqlParameter("P_REVNO", revNo)
            };

            var listMbom = MySqlDBManager.GetObjectsConvertType<MBom>(strSql, CommandType.Text, myParam.ToArray());
            
            return listMbom;
        }

        /// <summary>
        /// Insert MBOM into MES MySQL
        /// </summary>
        /// <param name="mbom"></param>
        /// <param name="myTrans"></param>
        /// <param name="myConnection"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        internal static bool InsertMBOMToMESMySql(MBom mbom, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @" INSERT INTO T_SD_MBOM(
                                STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, MODULEITEMCODE, ITEMCODE, ITEMCOLORSERIAL, MAINITEMCODE, MAINITEMCOLORSERIAL, CONSUMPUNIT
                                , QTY, UNITCONSUMPTION, ITEMREGISTER, REGISTRYDATE, SOS, NATION, CURRCODE, STDPRICE, TDCHECK, PATTERNCONS
                                , MARKERCONS, CONFDATE, POLYCONS, CAD_MATERIAL, SETCHECK, GENNAME, MARKERCONSUNIT, AREACONS, AREACONSUNIT, MARKER_LOSSRATE
                                )
                                VALUES(
                                ?P_STYLECODE, ?P_STYLESIZE, ?P_STYLECOLORSERIAL, ?P_REVNO, ?P_MODULEITEMCODE, ?P_ITEMCODE, ?P_ITEMCOLORSERIAL, ?P_MAINITEMCODE, ?P_MAINITEMCOLORSERIAL, ?P_CONSUMPUNIT
                                , ?P_QTY, ?P_UNITCONSUMPTION, ?P_ITEMREGISTER, ?P_REGISTRYDATE, ?P_SOS, ?P_NATION, ?P_CURRCODE, ?P_STDPRICE, ?P_TDCHECK, ?P_PATTERNCONS
                                , ?P_MARKERCONS, ?P_CONFDATE, ?P_POLYCONS, ?P_CAD_MATERIAL, ?P_SETCHECK, ?P_GENNAME, ?P_MARKERCONSUNIT, ?P_AREACONS, ?P_AREACONSUNIT, ?P_MARKER_LOSSRATE
                                ); ";

            var param = new List<MySqlParameter>
            {
               new MySqlParameter("P_STYLECODE", mbom.StyleCode),
                new MySqlParameter("P_STYLESIZE", mbom.StyleSize),
                new MySqlParameter("P_STYLECOLORSERIAL", mbom.StyleColorSerial),
                new MySqlParameter("P_REVNO", mbom.RevNo),
                new MySqlParameter("P_MODULEITEMCODE", mbom.ModuleItemCode),
                new MySqlParameter("P_ITEMCODE", mbom.ItemCode),
                new MySqlParameter("P_ITEMCOLORSERIAL", mbom.ItemColorSerial),
                new MySqlParameter("P_MAINITEMCODE", mbom.MainItemCode),
                new MySqlParameter("P_MAINITEMCOLORSERIAL", mbom.MainItemColorSerial),
                new MySqlParameter("P_CONSUMPUNIT", mbom.ConsumpUnit),
                new MySqlParameter("P_QTY", mbom.Qty),
                new MySqlParameter("P_UNITCONSUMPTION", mbom.UnitConsumption),
                new MySqlParameter("P_ITEMREGISTER", mbom.ItemRegister),
                new MySqlParameter("P_REGISTRYDATE", mbom.RegistryDate),
                new MySqlParameter("P_SOS", mbom.Sos),
                new MySqlParameter("P_NATION", mbom.Nation),
                new MySqlParameter("P_CURRCODE", mbom.CurrCode),
                new MySqlParameter("P_STDPRICE", mbom.StdPrice),
                new MySqlParameter("P_TDCHECK", mbom.TdCheck),
                new MySqlParameter("P_PATTERNCONS", mbom.PatternCons),
                new MySqlParameter("P_MARKERCONS", mbom.MarkerCons),
                new MySqlParameter("P_CONFDATE", mbom.ConfDate),
                new MySqlParameter("P_POLYCONS", mbom.PolyCons),
                new MySqlParameter("P_CAD_MATERIAL", mbom.Cad_Material),
                new MySqlParameter("P_SETCHECK", mbom.SetCheck),
                new MySqlParameter("P_GENNAME", mbom.GenName),
                new MySqlParameter("P_MARKERCONSUNIT", mbom.MarkerConsUnit),
                new MySqlParameter("P_AREACONS", mbom.AreaCons),
                new MySqlParameter("P_AREACONSUNIT", mbom.AreaConsUnit),
                new MySqlParameter("P_MARKER_LOSSRATE", mbom.Marker_LossRate)
            };

            var blIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, param.ToArray(), CommandType.Text, myTrans, myConnection);

            return blIns != null;
        }

        /// <summary>
        /// Insert list of MBOM to MES MySQL
        /// </summary>
        /// <param name="listMbom"></param>
        /// <returns></returns>
        public static bool InsertListMBOMToMESMySql(List<MBom> listMbom)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    foreach (var mbom in listMbom)
                    {
                        InsertMBOMToMESMySql(mbom, myTrans, myConnection);
                    }

                    myTrans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    myTrans.Rollback();
                    throw;
                }
            }
        }
        #endregion

    }
}
