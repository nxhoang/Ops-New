using OPS_DAL.APIEntities;
using OPS_DAL.DAL;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.APIBus
{
    public class PatternAPIBus
    {
        /// <summary>
        /// Get pattern
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="colorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="itemcode"></param>
        /// <param name="itemColorSerial"></param>
        /// <param name="mainItemCode"></param>
        /// <param name="mainItemColorSerial"></param>
        /// <returns></returns>
        public static List<PatternAPI> GetPatternByBom(string styleCode, string styleSize, string colorSerial, string revNo,
            string itemcode, string itemColorSerial, string mainItemCode, string mainItemColorSerial)
        {
            var sb = new StringBuilder();
            //var url = FtpInfoBus.GetSubUrl();

            //remember WILL DELETED LATER
            //url = "http://203.113.151.204:8080/PKPDM/";
            sb.Append(@" SELECT P.*, P.PieceQty PieceQtyRest, F.Code_Name AS MainPartName
                        , ICM.ITEMNAME, ICC.ITEMCOLORWAYS, ICM2.ITEMNAME AS MAINITEMNAME, ICC2.ITEMCOLORWAYS AS MAINITEMCOLORWAYS ");
            //sb.Append(@" ,CASE WHEN BOM.CADCOLORSERIAL IS NOT NULL THEN
            //               '" + url + @"'|| SUBSTR(P.STYLECODE,0,3) ||'/' || P.STYLECODE || '/' || P.STYLECODE || P.STYLESIZE 
            //               || BOM.CADCOLORSERIAL || P.REVNO || '/' 
            //               || substr(BOM.CADFILE ,0, instr(BOM.CADFILE ,'.')-1) 
            //               || '/' || P.PIECEUNIQUE || '.PNG' ELSE '' END AS URL
            //            ,'Not yet Linked' Status ");
            sb.AppendLine(" FROM T_SD_PTMT P ");
            sb.AppendLine(" left join(select * from t_cm_mcmt where m_code='MainPart' and s_code <>'000') f on P.MAINPART=f.s_code");
            sb.AppendLine(" LEFT JOIN T_SD_BOMH BOM  ");
            sb.AppendLine(" ON P.STYLECODE = BOM.STYLECODE AND P.STYLESIZE = BOM.STYLESIZE ");
            sb.AppendLine(" AND P.STYLECOLORSERIAL = BOM.STYLECOLORSERIAL AND P.REVNO = BOM.REVNO ");
            sb.AppendLine(@" LEFT JOIN T_00_ICMT ICM ON ICM.ITEMCODE = P.ITEMCODE
                                LEFT JOIN T_00_ICCM ICC ON ICC.ITEMCODE = P.ITEMCODE AND ICC.ITEMCOLORSERIAL = P.ITEMCOLORSERIAL
                                LEFT JOIN T_00_ICMT ICM2 ON ICM2.ITEMCODE = P.MAINITEMCODE
                                LEFT JOIN T_00_ICCM ICC2 ON ICC2.ITEMCODE = P.MAINITEMCODE AND ICC2.ITEMCOLORSERIAL = P.MAINITEMCOLORSERIAL   ");
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
            return OracleDbManager.GetObjects<PatternAPI>(sb.ToString(), prams.ToArray());
        }
    }
}
