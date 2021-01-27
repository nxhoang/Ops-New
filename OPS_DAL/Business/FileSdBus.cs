using OPS_DAL.DAL;
using OPS_DAL.Entities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Business
{
    /// <summary>
    /// 
    /// </summary>
    public class FileSdBus
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="styleFile"></param>
        /// <param name="styleFileDesc"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<FileSd> GetFiles(FileSd file, string styleFile, string styleFileDesc)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            var oracleParams = new OpsOracleParams(file.StyleCode, file.StyleSize, file.StyleColorSerial, file.RevNo)
            {
                new OracleParameter("P_MASTERCODE", styleFile),
                new OracleParameter("P_CODEDESC", styleFileDesc),
                cursor
            };

            var lstOpDetail = OracleDbManager.GetObjects<FileSd>("SP_OPS_GETFILES_FILE", CommandType.StoredProcedure, oracleParams.ToArray());

            return lstOpDetail;

        }

        /// <summary>
        /// Update used status of uploaded files
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColor"></param>
        /// <param name="rev"></param>
        /// <param name="uploadCode"></param>
        /// <param name="amenDno"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao - Adding on 2019/Jan/11
        public static bool UpdateUsedStatus(string styleCode, string styleSize, string styleColor, string rev, string uploadCode, string amenDno, string status)
        {
            string strSql = @" UPDATE T_SD_FILE SET USED = :P_USED 
                               WHERE STYLECODE = :P_STYLECODE 
                                    AND STYLESIZE = :P_STYLESIZE 
                                    AND STYLECOLORSERIAL = :P_STYLECOLORSERIAL 
                                    AND REVNO = :P_REVNO 
                                    AND AMENDNO = :P_AMENDNO
                                    AND UPLOADCODE = :P_UPLOADCODE AND (USED <> 'N' OR USED IS NULL)  ";

            var oracleParams = new List<OracleParameter>()
            {
                 new OracleParameter("P_USED", status),
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLESIZE", styleSize),
                new OracleParameter("P_STYLECOLORSERIAL", styleColor),
                new OracleParameter("P_REVNO", rev),
                new OracleParameter("P_AMENDNO", amenDno),
                new OracleParameter("P_UPLOADCODE", uploadCode)
            };

            var rs = OracleDbManager.ExecuteQuery(strSql, oracleParams.ToArray(), CommandType.Text);

            return rs.ToString() == "1";
        }

        //START ADD) SON - 17/May/2019
        public static List<FileSd> GetFilesByAo(string buyer, string aoNo, string styleInf, List<string> fileType)
        {
            // var fileTypes = "'CAD File', 'Embroidery Design', 'Printing', 'Marker File', 'Jig', 'Others'";
            //var fileTypes = "CAD File, Embroidery Design, Printing, Marker File, Jig, Others";
            //var fileTypes = new List<String> { "CAD File", "Embroidery Design", "Printing",  "Marker File", "Jig" , "Others"};

            List<OracleParameter> oraParams = new List<OracleParameter>();
            var index = 1;
            var paramName = "";
            var strFileType = "";
            foreach (String item in fileType)
            {
                paramName = "P_FILETYPE" + index;
                oraParams.Add(new OracleParameter(paramName, item));
                paramName = ":" + paramName;
                strFileType = index == fileType.Count ? strFileType + paramName : strFileType + paramName + ",";
               
                index += 1;
            }

            string strSql1 = @" SELECT T1.* FROM (
                                SELECT FIL.STYLECODE, FIL.STYLESIZE, FIL.STYLECOLORSERIAL, FIL.REVNO, FIL.UPLOADCODE, FIL.FILENOTE, FIL.FILEMANE, FIL.REMOTEFILE, FIL.AMENDNO
                                FROM T_SD_FILE FIL
                                WHERE FIL.UPLOADCODE IN ( ";

            var strSql2 = "SELECT S_CODE FROM T_CM_MCMT WHERE M_CODE = 'StyleFile' AND CODE_DESC IN ("+ strFileType +")";

            var strSql3 = @" )
                             )T1 JOIN (
                            SELECT DISTINCT DSM.ADNO, DSM.STYLECODE, DSM.STYLESIZE, DSM.STYLECOLORSERIAL, DSM.REVNO 
                                , STM.BUYER, STM.OLD_STYLECODE, STM.STYLENAME, STM.BUYERSTYLECODE 
                            FROM T_AD_ADSM DSM 
                                JOIN T_00_STMT STM ON STM.STYLECODE = DSM.STYLECODE 
                            WHERE  STM.BUYER = :P_BUYER AND STM.STATUS = 'OK' AND DSM.ADNO = :P_ADNO
                            )T2 ON T1.STYLECODE = T2.STYLECODE AND T1.STYLECOLORSERIAL = T2.STYLECOLORSERIAL AND T1.STYLESIZE = T2.STYLESIZE AND T1.REVNO = T2.REVNO ";

            var styleCon = @"  AND ( UPPER(STYLENAME) LIKE UPPER('%:P_STYLEINF1%') 
                                  OR UPPER(BUYERSTYLECODE) LIKE UPPER('%:P_STYLEINF2%')
                                  OR UPPER(STM.STYLECODE) LIKE UPPER('%:P_STYLEINF3%') )  ";

            var strSql = strSql1 + strSql2 + strSql3;

            oraParams.Add(new OracleParameter("P_BUYER", buyer));
            oraParams.Add(new OracleParameter("P_ADNO", aoNo));

            if (!string.IsNullOrEmpty(styleInf))
            {
                strSql += styleCon;
                oraParams.Add(new OracleParameter("P_STYLEINF1", styleInf));
                oraParams.Add(new OracleParameter("P_STYLEINF2", styleInf));
                oraParams.Add(new OracleParameter("P_STYLEINF3", styleInf));
            }

            var listFiles = OracleDbManager.GetObjects<FileSd>(strSql, CommandType.Text, oraParams.ToArray());

            return listFiles;

        }
        //START ADD) SON - 17/May/2019
    }
}
