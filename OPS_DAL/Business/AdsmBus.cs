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
    public class AdsmBus
    {
        /// <summary>
        /// Get list style by buyer, ao number or style information
        /// </summary>
        /// <param name="buyer"></param>
        /// <param name="aoNo"></param>
        /// <param name="styleInf"></param>
        /// <returns></returns>
        public static List<Adsm> GetListStyleCodeByBuyer(string buyer, string aoNo, string styleInf)
        {

            string strSql = @" SELECT DISTINCT DSM.ADNO, DSM.STYLECODE, DSM.STYLESIZE, DSM.STYLECOLORSERIAL, DSM.REVNO
                                , STM.BUYER, STM.OLD_STYLECODE, STM.STYLENAME, STM.BUYERSTYLECODE
                            FROM T_AD_ADSM DSM 
                                JOIN T_00_STMT STM ON STM.STYLECODE = DSM.STYLECODE
                            WHERE   STM.STATUS = 'OK' AND STM.BUYER = :P_BUYER AND DSM.ADNO = :P_ADNO";

            var styleCon = @"  AND ( UPPER(STYLENAME) LIKE UPPER('%:P_STYLEINF1%') 
                                  OR UPPER(BUYERSTYLECODE) LIKE UPPER('%:P_STYLEINF2%')
                                  OR UPPER(STM.STYLECODE) LIKE UPPER('%:P_STYLEINF3%') )  ";

            List<OracleParameter> oraParams = new List<OracleParameter>
            {
                new OracleParameter("P_BUYER", buyer),
                new OracleParameter("P_ADNO", aoNo)

            };

            if (!string.IsNullOrEmpty(styleInf))
            {
                strSql += styleCon;
                oraParams.Add(new OracleParameter("P_STYLEINF1", styleInf));
                oraParams.Add(new OracleParameter("P_STYLEINF2", styleInf));
                oraParams.Add(new OracleParameter("P_STYLEINF3", styleInf));
            }

            var listAdsm = OracleDbManager.GetObjects<Adsm>(strSql, CommandType.Text, oraParams.ToArray());

            return listAdsm;

        }
    }
}
