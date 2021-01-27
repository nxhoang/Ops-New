using OPS_DAL.Entities;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Text;
using OPS_DAL.DAL;
using MySql.Data.MySqlClient;
using System.Data;
using OPS_Utils;
using System;

namespace OPS_DAL.Business
{
    /// <summary>
    /// T_SD_PTMT
    /// </summary>
    public class PatternBus
    {
        /// <summary>
        /// T_SD_PTMT
        /// </summary>
        /// <param name="styleCode">styleCode</param>
        /// <param name="styleSize">styleSize</param>
        /// <param name="colorSerial">colorSerial</param>
        /// <param name="revNo">revNo</param>
        /// <param name="itemcode">itemcode</param>
        /// <param name="itemColorSerial">itemColorSerial</param>
        /// <param name="mainItemCode">mainItemCode</param>
        /// <param name="mainItemColorSerial">MainItemColorSerial</param>
        /// <returns></returns>
        /// Author: VitHV
        public List<Pattern> GetPatternByBom(string styleCode, string styleSize, string colorSerial, string revNo,
            string itemcode, string itemColorSerial, string mainItemCode, string mainItemColorSerial)
        {
            var sb = new StringBuilder();
            var url = FtpInfoBus.GetSubUrl();

            //remember WILL DELETED LATER
            url = "http://203.113.151.204:8080/PKPDM/";
            sb.Append(" SELECT P.*, P.PieceQty PieceQtyRest, F.Code_Name AS MainPartName");
            sb.Append(@" ,CASE WHEN BOM.CADCOLORSERIAL IS NOT NULL THEN
                           '" + url + @"'|| SUBSTR(P.STYLECODE,0,3) ||'/' || P.STYLECODE || '/' || P.STYLECODE || P.STYLESIZE 
                           || BOM.CADCOLORSERIAL || P.REVNO || '/' 
                           || substr(BOM.CADFILE ,0, instr(BOM.CADFILE ,'.')-1) 
                           || '/' || P.PIECEUNIQUE || '.PNG' ELSE '' END AS URL
                        ,'Not yet Linked' Status ");
            sb.AppendLine(" FROM T_SD_PTMT P ");
            sb.AppendLine(" left join(select * from t_cm_mcmt where m_code='MainPart' and s_code <>'000') f on P.MAINPART=f.s_code");
            sb.AppendLine(" LEFT JOIN T_SD_BOMH BOM  ");
            sb.AppendLine(" ON P.STYLECODE = BOM.STYLECODE AND P.STYLESIZE = BOM.STYLESIZE ");
            sb.AppendLine(" AND P.STYLECOLORSERIAL = BOM.STYLECOLORSERIAL AND P.REVNO = BOM.REVNO ");
            sb.AppendLine(" WHERE P.STYLECODE=:STYLECODE  AND P.STYLESIZE=:STYLESIZE");
            sb.AppendLine(" AND P.STYLECOLORSERIAL =:STYLECOLORSERIAL AND P.REVNO=:REVNO");
            sb.AppendLine(" AND P.ITEMCODE =: ITEMCODE");
            sb.AppendLine(" AND P.ITEMCOLORSERIAL =: ITEMCOLORSERIAL");
            sb.AppendLine(" AND P.MAINITEMCODE =: MAINITEMCODE");
            sb.AppendLine(" AND P.MAINITEMCOLORSERIAL =: MAINITEMCOLORSERIAL");
            var prams = new OpsParams(styleCode, styleSize, colorSerial, revNo);
            prams.ReplacePbyEmpty();
            prams.Add(new OracleParameter("ITEMCODE", itemcode));
            prams.Add(new OracleParameter("ITEMCOLORSERIAL", itemColorSerial));
            prams.Add(new OracleParameter("MAINITEMCODE", mainItemCode));
            prams.Add(new OracleParameter("MAINITEMCOLORSERIAL", mainItemColorSerial));
            return OracleDbManager.GetObjects<Pattern>(sb.ToString(), prams.ToArray());
        }
        /// <summary>
        /// T_SD_PTMT
        /// </summary>
        /// <param name="styleCode">styleCode</param>
        /// <param name="styleSize">styleSize</param>
        /// <param name="colorSerial">colorSerial</param>
        /// <param name="revNo">revNo</param>
        /// <param name="itemcode">itemcode</param>
        /// <param name="itemColorSerial">itemColorSerial</param>
        /// <param name="mainItemCode">mainItemCode</param>
        /// <param name="mainItemColorSerial">MainItemColorSerial</param>
        /// <param name="edition"></param>
        /// <returns></returns>
        /// Author: VitHV
        public List<Pattern> GetPattern(string styleCode, string styleSize, string colorSerial, string revNo,
            string itemcode, string itemColorSerial, string patternSerial, string mainItemCode, string mainItemColorSerial)
        {
            var sb = new StringBuilder();
            //var url = FtpInfoBus.GetSubUrl();
            sb.Append(" SELECT P.* ");
            sb.AppendLine(" FROM T_SD_PTMT P ");
            sb.AppendLine(" WHERE P.STYLECODE=:STYLECODE  AND P.STYLESIZE=:STYLESIZE");
            sb.AppendLine(" AND P.STYLECOLORSERIAL =:STYLECOLORSERIAL AND P.REVNO=:REVNO");
            sb.AppendLine(" AND P.ITEMCODE =: ITEMCODE");
            sb.AppendLine(" AND P.ITEMCOLORSERIAL =: ITEMCOLORSERIAL");
            sb.AppendLine(" AND P.PATTERNSERIAL =: PATTERNSERIAL");
            sb.AppendLine(" AND P.MAINITEMCODE =: MAINITEMCODE");
            sb.AppendLine(" AND P.MAINITEMCOLORSERIAL =: MAINITEMCOLORSERIAL");
            var prams = new OpsParams(styleCode, styleSize, colorSerial, revNo);
            prams.ReplacePbyEmpty();
            prams.Add(new OracleParameter("ITEMCODE", itemcode));
            prams.Add(new OracleParameter("ITEMCOLORSERIAL", itemColorSerial));
            prams.Add(new OracleParameter("PATTERNSERIAL", patternSerial));
            prams.Add(new OracleParameter("MAINITEMCODE", mainItemCode));
            prams.Add(new OracleParameter("MAINITEMCOLORSERIAL", mainItemColorSerial));
            return OracleDbManager.GetObjects<Pattern>(sb.ToString(), prams.ToArray());
        }

        /// <summary>
        /// Get linked pattern by style code
        /// </summary>
        /// <param name="stlCode"></param>
        /// <param name="stlSize"></param>
        /// <param name="stlColorSerial"></param>
        /// <param name="stlRevNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Pattern> GetPatternByStyleCode(string stlCode, string stlSize, string stlColorSerial, string stlRevNo)
        {
            var strSql = @"  SELECT * FROM T_SD_PTMT WHERE STYLECODE = :P_STYLECODE AND STYLESIZE = :P_STYLESIZE AND STYLECOLORSERIAL = :P_STYLECOLORSERIAL AND REVNO = :P_REVNO ";

            var oraPrams = new OpsParams(stlCode, stlSize, stlColorSerial, stlRevNo);

            return OracleDbManager.GetObjects<Pattern>(strSql, oraPrams.ToArray());
        }

        #region MySQL

        /// <summary>
        /// Get list of patterns by item code and item colorserial
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="itemCode"></param>
        /// <param name="itemColorSerial"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Pattern> GetPaternsMySql(string styleCode, string styleSize, string styleColorSerial, string revNo, string itemCode, string itemColorSerial)
        {
            string strSql = @" SELECT MCM.CODE_NAME MAINPARTNAME, PTM.* 
                                FROM T_SD_PTMT PTM 
                                    LEFT JOIN T_CM_MCMT MCM ON MCM.S_CODE = PTM.MAINPART AND MCM.M_CODE = 'MainPart'
                                WHERE STYLECODE = ?P_STYLECODE  AND STYLESIZE = ?P_STYLESIZE AND STYLECOLORSERIAL = ?P_STYLECOLORSERIAL AND REVNO = ?P_REVNO AND ITEMCODE = ?P_ITEMCODE AND ITEMCOLORSERIAL = ?P_ITEMCOLORSERIAL
                                ORDER BY PATTERNSERIAL;";
            List<MySqlParameter> myParam = new List<MySqlParameter>
            {
                new MySqlParameter("P_STYLECODE", styleCode),
                new MySqlParameter("P_STYLESIZE", styleSize),
                new MySqlParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new MySqlParameter("P_REVNO", revNo),
                new MySqlParameter("P_ITEMCODE", itemCode),
                new MySqlParameter("P_ITEMCOLORSERIAL", itemColorSerial),

            };
            var listPtnt = MySqlDBManager.GetObjectsConvertType<Pattern>(strSql, CommandType.Text, myParam.ToArray());

            return listPtnt;
        }

        /// <summary>
        /// Get all patterns by style
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static List<Pattern> GetPaternsMySql(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            string strSql = @" SELECT MCM.CODE_NAME MAINPARTNAME, PTM.* 
                                FROM T_SD_PTMT PTM 
                                    LEFT JOIN T_CM_MCMT MCM ON MCM.S_CODE = PTM.MAINPART AND MCM.M_CODE = 'MainPart'
                                WHERE STYLECODE = ?P_STYLECODE  AND STYLESIZE = ?P_STYLESIZE AND STYLECOLORSERIAL = ?P_STYLECOLORSERIAL AND REVNO = ?P_REVNO
                                ORDER BY PATTERNSERIAL;";
            List<MySqlParameter> myParam = new List<MySqlParameter>
            {
                new MySqlParameter("P_STYLECODE", styleCode),
                new MySqlParameter("P_STYLESIZE", styleSize),
                new MySqlParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new MySqlParameter("P_REVNO", revNo)

            };
            var listPtnt = MySqlDBManager.GetObjectsConvertType<Pattern>(strSql, CommandType.Text, myParam.ToArray());

            return listPtnt;
        }

        /// <summary>
        /// Insert patterns to MES MySQL
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="myTrans"></param>
        /// <param name="myConnection"></param>
        /// <returns></returns>
        internal static bool InsertPatternToMESMySql(Pattern ptmt, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @" INSERT INTO T_SD_PTMT(STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, ITEMCODE, ITEMCOLORSERIAL, PATTERNSERIAL, MAINITEMCODE, MAINITEMCOLORSERIAL, MAINPART
                                , PIECE, SIZEUNIT, WIDTH, ENDWISE, HEIGHT, PIECEQTY, UNITCONSUMPTION, CURRCODE, TRPRICE, PATTERNPRICE
                                , REMARKS, PDCODE, REGISTER, QTYASSUMER, PATTERNFILE1, PATTERNFILE2, MARKGRAP, MARKWIDTH, MARKQTY, MARKMETHOD
                                , MARKEFFICIENCY, MARKFILE, POLYGON, IMAGENAME, FILL, STROKE, STROKEWIDTH, OPPOSITE, LAST_UPDATED_TIME, PURCHASESET
                                , CONSUMPUNIT, PIECEUNIQUE, AREA, MIRROR, CUTTINGMACHINE)
                               VALUES(?P_STYLECODE,?P_STYLESIZE,?P_STYLECOLORSERIAL,?P_REVNO,?P_ITEMCODE,?P_ITEMCOLORSERIAL,?P_PATTERNSERIAL,?P_MAINITEMCODE,?P_MAINITEMCOLORSERIAL,?P_MAINPART
                                ,?P_PIECE,?P_SIZEUNIT,?P_WIDTH,?P_ENDWISE,?P_HEIGHT,?P_PIECEQTY,?P_UNITCONSUMPTION,?P_CURRCODE,?P_TRPRICE,?P_PATTERNPRICE
                                ,?P_REMARKS,?P_PDCODE,?P_REGISTER,?P_QTYASSUMER,?P_PATTERNFILE1,?P_PATTERNFILE2,?P_MARKGRAP,?P_MARKWIDTH,?P_MARKQTY,?P_MARKMETHOD
                                ,?P_MARKEFFICIENCY,?P_MARKFILE,?P_POLYGON,?P_IMAGENAME,?P_FILL,?P_STROKE,?P_STROKEWIDTH,?P_OPPOSITE,?P_LAST_UPDATED_TIME,?P_PURCHASESET
                                ,?P_CONSUMPUNIT,?P_PIECEUNIQUE,?P_AREA,?P_MIRROR,?P_CUTTINGMACHINE);";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_STYLECODE", ptmt.StyleCode),
                new MySqlParameter("P_STYLESIZE", ptmt.StyleSize),
                new MySqlParameter("P_STYLECOLORSERIAL", ptmt.StyleColorSerial),
                new MySqlParameter("P_REVNO", ptmt.RevNo),
                new MySqlParameter("P_ITEMCODE", ptmt.ItemCode),
                new MySqlParameter("P_ITEMCOLORSERIAL", ptmt.ItemColorSerial),
                new MySqlParameter("P_PATTERNSERIAL", ptmt.PatternSerial),
                new MySqlParameter("P_MAINITEMCODE", ptmt.MainItemCode),
                new MySqlParameter("P_MAINITEMCOLORSERIAL", ptmt.MainItemColorSerial),
                new MySqlParameter("P_MAINPART", ptmt.MainPart),
                new MySqlParameter("P_PIECE", ptmt.Piece),
                new MySqlParameter("P_SIZEUNIT", ptmt.SizeUnit),
                new MySqlParameter("P_WIDTH", ptmt.Width),
                new MySqlParameter("P_ENDWISE", ptmt.EndWise),
                new MySqlParameter("P_HEIGHT", ptmt.Height),
                new MySqlParameter("P_PIECEQTY", ptmt.PieceQty),
                new MySqlParameter("P_UNITCONSUMPTION", ptmt.UnitConsumption),
                new MySqlParameter("P_CURRCODE", ptmt.CurrCode),
                new MySqlParameter("P_TRPRICE", ptmt.TrPrice),
                new MySqlParameter("P_PATTERNPRICE", ptmt.PatternPrice),
                new MySqlParameter("P_REMARKS", ptmt.Remarks),
                new MySqlParameter("P_PDCODE", ptmt.PdCode),
                new MySqlParameter("P_REGISTER", ptmt.Register),
                new MySqlParameter("P_QTYASSUMER", ptmt.QtyAssumer),
                new MySqlParameter("P_PATTERNFILE1", ptmt.PatternFile1),
                new MySqlParameter("P_PATTERNFILE2", ptmt.PatternFile2),
                new MySqlParameter("P_MARKGRAP", ptmt.MarkGrap),
                new MySqlParameter("P_MARKWIDTH", ptmt.MarkWidth),
                new MySqlParameter("P_MARKQTY", ptmt.MarkQty),
                new MySqlParameter("P_MARKMETHOD", ptmt.MarkMethod),
                new MySqlParameter("P_MARKEFFICIENCY", ptmt.MarkEfficiency),
                new MySqlParameter("P_MARKFILE", ptmt.MarkFile),
                new MySqlParameter("P_POLYGON", ptmt.Polygon),
                new MySqlParameter("P_IMAGENAME", ptmt.ImageName),
                new MySqlParameter("P_FILL", ptmt.Fill),
                new MySqlParameter("P_STROKE", ptmt.Stroke),
                new MySqlParameter("P_STROKEWIDTH", ptmt.StrokeWidth),
                new MySqlParameter("P_OPPOSITE", ptmt.Opposite),
                new MySqlParameter("P_LAST_UPDATED_TIME", ptmt.LAST_UPDATED_TIME),
                new MySqlParameter("P_PURCHASESET", ptmt.PURCHASESET),
                new MySqlParameter("P_CONSUMPUNIT", ptmt.ConsumpUnit),
                new MySqlParameter("P_PIECEUNIQUE", ptmt.PieceUnique),
                new MySqlParameter("P_AREA", ptmt.Area),
                new MySqlParameter("P_MIRROR", ptmt.Mirror),
                new MySqlParameter("P_CUTTINGMACHINE", ptmt.CuttingMachine)
            };

            var blIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, param.ToArray(), CommandType.Text, myTrans, myConnection);

            return blIns != null;
        }

        /// <summary>
        /// Insert list of pattern to MES MySQL
        /// </summary>
        /// <param name="listPattern"></param>
        /// <returns></returns>
        public static bool InsertListPatternToMESMySql(List<Pattern> listPattern)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    foreach (var pattern in listPattern)
                    {
                        InsertPatternToMESMySql(pattern, myTrans, myConnection);
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
