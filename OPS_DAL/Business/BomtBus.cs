using OPS_DAL.Entities;
using System.Collections.Generic;
using System.Text;
using OPS_DAL.DAL;
using MySql.Data.MySqlClient;
using System.Data;
using OPS_Utils;
using System;

namespace OPS_DAL.Business
{
    public class BomtBus
    {
        /// <summary>
        /// T_SD_BOMT
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="colorSerial"></param>
        /// <param name="revNo"></param>
        /// <returns></returns>
        public List<Bomt> GetBom(string styleCode, string styleSize, string colorSerial, string revNo)
        {
            var sb = new StringBuilder();
            sb.Append(@" SELECT DISTINCT B.*, P.MAINITEMCODE PATTERNCODE
                        ,D.STYLECOLORWAYS ItemColorWays, C.ITEMNAME
                        ,'Not yet Linked' Status ");
            sb.AppendLine(" FROM T_SD_BOMT B ");
            sb.AppendLine(" LEFT JOIN T_00_SCMT D ON B.STYLECODE = D.STYLECODE AND B.STYLECOLORSERIAL = D.STYLECOLORSERIAL ");
            sb.AppendLine(" LEFT JOIN t_00_icmt C ON B.MAINITEMCODE = C.ITEMCODE ");
            sb.Append(@" LEFT JOIN (SELECT DISTINCT STYLECODE
                  , STYLESIZE
                  , STYLECOLORSERIAL
                  , REVNO
                  , ITEMCODE
                  , ITEMCOLORSERIAL
                  , MAINITEMCODE
                  , Mainitemcolorserial
            FROM T_SD_PTMT WHERE STYLECODE = :STYLECODE
                AND STYLESIZE = :STYLESIZE AND STYLECOLORSERIAL = :STYLECOLORSERIAL ANd REVNO = :REVNO) P ");
            sb.AppendLine(@"ON B.STYLECODE = P.STYLECODE
                            AND B.STYLESIZE = P.STYLESIZE
                            AND B.STYLECOLORSERIAL = P.STYLECOLORSERIAL
                            AND B.REVNO = P.REVNO
                            AND B.ITEMCODE = P.ITEMCODE
                            AND B.ITEMCOLORSERIAL = P.ITEMCOLORSERIAL
                            AND B.MAINITEMCODE = P.MAINITEMCODE
                            AND B.Mainitemcolorserial = P.Mainitemcolorserial");
            sb.AppendLine(" WHERE B.STYLECODE=:STYLECODE");
            sb.AppendLine(" AND B.STYLESIZE=:STYLESIZE");
            sb.AppendLine(" AND B.STYLECOLORSERIAL =:STYLECOLORSERIAL");
            sb.AppendLine(" AND B.REVNO=:REVNO");
            sb.AppendLine(" ORDER BY B.ITEMCODE, B.ITEMCOLORSERIAL");
            var prams = new OpsParams(styleCode, styleSize, colorSerial, revNo);
            prams.ReplacePbyEmpty();
            var ret = OracleDbManager.GetObjects<Bomt>(sb.ToString(), prams.ToArray());
            return ret;
        }

        /// <summary>
        /// Get BOM detail by style Code, Size, Color and Revision
        /// </summary>
        /// <param name="stlCode"></param>
        /// <param name="stlSize"></param>
        /// <param name="stlColorSerial"></param>
        /// <param name="stlRevNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Bomt> GetBOMDetail(string stlCode, string stlSize, string stlColorSerial, string stlRevNo)
        {
            //var strSql = @"  SELECT * FROM T_SD_BOMT WHERE STYLECODE = :P_STYLECODE AND STYLESIZE = :P_STYLESIZE AND STYLECOLORSERIAL = :P_STYLECOLORSERIAL AND REVNO = :P_REVNO ";
            var strSql = @"  select bom.*, icm.itemname , bom.itemcolorserial  || ' - ' ||ICC.ITEMCOLORWAYS as ITEMCOLORWAYS
                             from t_sd_bomt bom join t_00_icmt icm on icm.itemcode = bom.itemcode 
                                join t_00_iccm icc on icc.itemcode = bom.itemcode and icc.itemcolorserial = bom.itemcolorserial  
                             WHERE STYLECODE = :P_STYLECODE AND STYLESIZE = :P_STYLESIZE AND STYLECOLORSERIAL = :P_STYLECOLORSERIAL AND REVNO = :P_REVNO ";

            var oraPrams = new OpsParams(stlCode, stlSize, stlColorSerial, stlRevNo);

            return OracleDbManager.GetObjects<Bomt>(strSql, oraPrams.ToArray());
        }

        #region MySQL

        /// <summary>
        /// Get bom detail from mySQL
        /// </summary>
        /// <param name="stlCode"></param>
        /// <param name="stlSize"></param>
        /// <param name="stlColorSerial"></param>
        /// <param name="stlRevNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Bomt> GetBOMDetailMySQL(string stlCode, string stlSize, string stlColorSerial, string stlRevNo)
        {
            var strSql = @" SELECT icm.itemname, concat(ccm.ITEMCOLORSERIAL, ' - ', ccm.ITEMCOLORWAYS )  ItemColor, scm.FullName , bom.* 
                            FROM T_SD_BOMT bom
	                            left join t_00_icmt icm on icm.itemcode = bom.itemcode
                                left join t_00_iccm ccm on ccm.itemcode = bom.itemcode and ccm.ITEMCOLORSERIAL = bom.itemcolorserial
                                left join t_cm_sscm scm on scm.SOS =  bom.SOS
                            WHERE STYLECODE = ?P_STYLECODE AND STYLESIZE = ?P_STYLESIZE AND STYLECOLORSERIAL = ?P_STYLECOLORSERIAL AND REVNO = ?P_REVNO ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_STYLECODE", stlCode),
                new MySqlParameter("P_STYLESIZE", stlSize),
                new MySqlParameter("P_STYLECOLORSERIAL", stlColorSerial),
                new MySqlParameter("P_REVNO", stlRevNo),
            };

            return MySqlDBManager.GetObjects<Bomt>(strSql, CommandType.Text, param.ToArray());
        }

        /// <summary>
        /// Insert bomt to MES mySQL
        /// </summary>
        /// <param name="bomt"></param>
        /// <param name="myTrans"></param>
        /// <param name="myConnection"></param>
        /// <returns></returns>
        internal static bool InsertBOMTToMESMySql(Bomt bomt, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @" INSERT INTO T_SD_BOMT( STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, ITEMCODE, ITEMCOLORSERIAL, MAINITEMCODE, MAINITEMCOLORSERIAL, CONSUMPUNIT, QTY
                                , UNITCONSUMPTION, ITEMREGISTER, REGISTRYDATE, LASTMODIFIER, LASTMODIDATE, SOS, NATION, EXCRATIO, CURRCODE, STDPRICE
                                , OFFERCONSUMPTION, OFFERPRICE, REMARKS, TDCHECK, FACTORY, SOSITEMCODE, BOMFILE, LEVA, LEVB, LEVC
                                , LOTSIZE, SPREADING, LAYER, MARKER, USECONSUMPTION, IMAGENAME, CONFSTATUS, PATTERNCONS, MARKERCONS, CONFDATE
                                , POLYCONS, CAD_MATERIAL, SETCHECK, GENNAME, MARKERCONSUNIT, AREACONS, AREACONSUNIT, MARKER_LOSSRATE, MANUALCONS, CONSUMPTION_TYPE, MANUALCONSUNIT)
                               VALUES( ?P_STYLECODE, ?P_STYLESIZE, ?P_STYLECOLORSERIAL, ?P_REVNO, ?P_ITEMCODE, ?P_ITEMCOLORSERIAL, ?P_MAINITEMCODE, ?P_MAINITEMCOLORSERIAL, ?P_CONSUMPUNIT, ?P_QTY
                                , ?P_UNITCONSUMPTION, ?P_ITEMREGISTER, ?P_REGISTRYDATE, ?P_LASTMODIFIER, ?P_LASTMODIDATE, ?P_SOS, ?P_NATION, ?P_EXCRATIO, ?P_CURRCODE, ?P_STDPRICE
                                , ?P_OFFERCONSUMPTION, ?P_OFFERPRICE, ?P_REMARKS, ?P_TDCHECK, ?P_FACTORY, ?P_SOSITEMCODE, ?P_BOMFILE, ?P_LEVA, ?P_LEVB, ?P_LEVC
                                , ?P_LOTSIZE, ?P_SPREADING, ?P_LAYER, ?P_MARKER, ?P_USECONSUMPTION, ?P_IMAGENAME, ?P_CONFSTATUS, ?P_PATTERNCONS, ?P_MARKERCONS, ?P_CONFDATE
                                , ?P_POLYCONS, ?P_CAD_MATERIAL, ?P_SETCHECK, ?P_GENNAME, ?P_MARKERCONSUNIT, ?P_AREACONS, ?P_AREACONSUNIT, ?P_MARKER_LOSSRATE, ?P_MANUALCONS, ?P_CONSUMPTION_TYPE, ?P_MANUALCONSUNIT);";

            var param = new List<MySqlParameter>
            {
               new MySqlParameter("P_STYLECODE", bomt.StyleCode),
                new MySqlParameter("P_STYLESIZE", bomt.StyleSize),
                new MySqlParameter("P_STYLECOLORSERIAL", bomt.StyleColorSerial),
                new MySqlParameter("P_REVNO", bomt.RevNo),
                new MySqlParameter("P_ITEMCODE", bomt.ItemCode),
                new MySqlParameter("P_ITEMCOLORSERIAL", bomt.ItemColorSerial),
                new MySqlParameter("P_MAINITEMCODE", bomt.MainItemCode),
                new MySqlParameter("P_MAINITEMCOLORSERIAL", bomt.MainItemColorSerial),
                new MySqlParameter("P_CONSUMPUNIT", bomt.ConsumpUnit),
                new MySqlParameter("P_QTY", bomt.Qty),
                new MySqlParameter("P_UNITCONSUMPTION", bomt.UnitConsumption),
                new MySqlParameter("P_ITEMREGISTER", bomt.ItemRegister),
                new MySqlParameter("P_REGISTRYDATE", bomt.RegistryDate),
                new MySqlParameter("P_LASTMODIFIER", bomt.LastModifier),
                new MySqlParameter("P_LASTMODIDATE", bomt.LastModiDate),
                new MySqlParameter("P_SOS", bomt.Sos),
                new MySqlParameter("P_NATION", bomt.Nation),
                new MySqlParameter("P_EXCRATIO", bomt.EXCRATIO),
                new MySqlParameter("P_CURRCODE", bomt.CurrCode),
                new MySqlParameter("P_STDPRICE", bomt.StdPrice),
                new MySqlParameter("P_OFFERCONSUMPTION", bomt.OFFERCONSUMPTION),
                new MySqlParameter("P_OFFERPRICE", bomt.OFFERPRICE),
                new MySqlParameter("P_REMARKS", bomt.REMARKS),
                new MySqlParameter("P_TDCHECK", bomt.TDCHECK),
                new MySqlParameter("P_FACTORY", bomt.FACTORY),
                new MySqlParameter("P_SOSITEMCODE", bomt.SOSITEMCODE),
                new MySqlParameter("P_BOMFILE", bomt.BOMFILE),
                new MySqlParameter("P_LEVA", bomt.LEVA),
                new MySqlParameter("P_LEVB", bomt.LEVB),
                new MySqlParameter("P_LEVC", bomt.LEVC),
                new MySqlParameter("P_LOTSIZE", bomt.LOTSIZE),
                new MySqlParameter("P_SPREADING", bomt.SPREADING),
                new MySqlParameter("P_LAYER", bomt.LAYER),
                new MySqlParameter("P_MARKER", bomt.MARKER),
                new MySqlParameter("P_USECONSUMPTION", bomt.USECONSUMPTION),
                new MySqlParameter("P_IMAGENAME", bomt.IMAGENAME),
                new MySqlParameter("P_CONFSTATUS", bomt.CONFSTATUS),
                new MySqlParameter("P_PATTERNCONS", bomt.PATTERNCONS),
                new MySqlParameter("P_MARKERCONS", bomt.MARKERCONS),
                new MySqlParameter("P_CONFDATE", bomt.CONFDATE),
                new MySqlParameter("P_POLYCONS", bomt.POLYCONS),
                new MySqlParameter("P_CAD_MATERIAL", bomt.CAD_MATERIAL),
                new MySqlParameter("P_SETCHECK", bomt.SETCHECK),
                new MySqlParameter("P_GENNAME", bomt.GENNAME),
                new MySqlParameter("P_MARKERCONSUNIT", bomt.MARKERCONSUNIT),
                new MySqlParameter("P_AREACONS", bomt.AREACONS),
                new MySqlParameter("P_AREACONSUNIT", bomt.AREACONSUNIT),
                new MySqlParameter("P_MARKER_LOSSRATE", bomt.MARKER_LOSSRATE),
                new MySqlParameter("P_MANUALCONS", bomt.MANUALCONS),
                new MySqlParameter("P_CONSUMPTION_TYPE", bomt.CONSUMPTION_TYPE),
                new MySqlParameter("P_MANUALCONSUNIT", bomt.MANUALCONSUNIT)
            };

            var blIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, param.ToArray(), CommandType.Text, myTrans, myConnection);

            return blIns != null;
        }

        /// <summary>
        /// Insert list of BOMT to MES mySQL
        /// </summary>
        /// <param name="listBomt"></param>
        /// <returns></returns>
        public static bool InsertListBOMTToMESMySql(List<Bomt> listBomt)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    foreach (var bomt in listBomt)
                    {
                        InsertBOMTToMESMySql(bomt, myTrans, myConnection);
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
