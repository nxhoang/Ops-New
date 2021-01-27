using OPS_DAL.APIEntities;
using OPS_DAL.DAL;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.APIBus
{
    public class DormAPIBus
    {        
        /// <summary>
        /// Get style
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="buyerCode"></param>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<DormAPI> GetStyles(int pageIndex, int pageSize, string buyerCode, string styleCode, string styleSize, string styleColorSerial)
        {

            var fromIdx = (pageIndex - 1) * pageSize + 1;
            var toIdx = pageIndex * pageSize;

            string strSql1 = @"SELECT X.* FROM (";

            var strSql2 = @"         SELECT
                                     COUNT(*) OVER() Total,
                                     ROW_NUMBER() OVER(ORDER BY TO_DATE(A.REGISTRYDATE,'YYYY/MM/DD HH24:MI:SS') DESC) R,
                                     A.STYLECODE,
                                     B.STYLENAME,
                                     B.STYLEGROUP, B.SUBGROUP, B.SUBSUBGROUP, B.BUYER,
                                     A.STYLECOLORSERIAL,
                                     A.STYLESIZE,
                                     D.STYLECOLORSERIAL || ' - ' || D.STYLECOLORWAYS STYLECOLORWAYS,
                                     A.REGISTRYDATE,
                                     A.REVNO,
                                     TO_CHAR(A.AD_CONFIRM,'DD/MM/YYYY') ADCONFIRM,
                                     TO_CHAR(A.AD_DEV_SALES,'DD/MM/YYYY') ADDEVSALE ,
                                     CASE WHEN A.AD_CONFIRM IS NOT NULL THEN 'Final Confirmed' 
                                     WHEN A.AD_DEV_SALES IS NOT NULL THEN 'Confirmed' ELSE 'Open' END STATUS,
                                     B.BUYERSTYLECODE,
                                     B.BUYERSTYLENAME,
                                     F.S_CODE BuyerCode,
                                     F.CODE_NAME BuyerName,
                                     E.Name as REGISTER 
                                     FROM t_Sd_dorm A
                                     LEFT JOIN T_00_STMT B ON A.STYLECODE = B.STYLECODE
                                     LEFT JOIN T_00_SCMT D ON (A.STYLECODE = D.STYLECODE AND A.STYLECOLORSERIAL = D.STYLECOLORSERIAL)
                                     LEFT JOIN t_cm_usmt E ON A.REGISTER=E.USERID
                                     LEFT JOIN(SELECT * FROM t_cm_mcmt WHERE m_code= 'Buyer' and s_code<> '000') F ON B.BUYER = F.S_CODE
                                     WHERE B.BUYER= :P_BUYER";

            var strSql3 = @" ) X WHERE R BETWEEN :P_FROMIDX AND :P_TOIDX
                    ORDER BY R ";

            var oracleParams = new List<OracleParameter> {
                new OracleParameter("P_BUYER", buyerCode.ToUpper())
            };

            if (!string.IsNullOrEmpty(styleCode))
            {
                strSql2 += " AND (LOWER(A.STYLECODE) LIKE  UPPER('%'||:P_STYLECODE||'%')";
                oracleParams.Add(new OracleParameter("P_STYLECODE", styleCode));
            }

            if (!string.IsNullOrEmpty(styleSize))
            {
                strSql2 += " AND (LOWER(A.STYLESIZE) LIKE  UPPER('%'||:P_STYLESIZE||'%') ";
                oracleParams.Add(new OracleParameter("P_STYLESIZE", styleSize));
            }

            if (!string.IsNullOrEmpty(styleColorSerial))
            {
                strSql2 += " AND (LOWER(A.STYLECOLORSERIAL) LIKE  UPPER('%'||:P_STYLECOLORSERIAL||'%')";
                oracleParams.Add(new OracleParameter("P_STYLECOLORSERIAL", styleColorSerial));
            }

            oracleParams.Add(new OracleParameter("P_FROMIDX", fromIdx));
            oracleParams.Add(new OracleParameter("P_TOIDX", toIdx));

            var listStyle = OracleDbManager.GetObjects<DormAPI>(strSql1 + strSql2 + strSql3, CommandType.Text, oracleParams.ToArray());

            return listStyle;
        }
    }
}
