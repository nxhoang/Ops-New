using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public class AdsmBus
    {
        /// <summary>
        /// Get Ao Style master
        /// </summary>
        /// <param name="aoNo"></param>
        /// <param name="buyer"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Adsm> GetAoStyleList(string aoNo, string buyer, string factory, string buyerInfo)
        {
            string strSql = @" SELECT ADS.ADNO, ADS.STYLECODE, ADS.STYLESIZE, ADS.STYLECOLORSERIAL, ADS.REVNO, ADS.STYLESERIAL, ADS.DESTINATION 
                                    , ADS.DELIVERYDATE, ADS.ADQTY, ADS.QTYMODIDATE, ADS.CURRCODE, ADS.STDPRICE, ADS.ADPRICE, ADS.FACTORY, ADS.STATUS, ADS.PD_STR PDSTR 
                                    , ADS.PD_END PDEND, ADS.AD_STR ADSTR, ADS.AD_END ADEND, ADS.ASSCHK, ADS.BOM_CONFIRM BOMCONFIRM, ADS.FA_CONFIRM FACONFIRM 
                                    , ADS.MTS, ADS.WORKFACTORY 
                                    , CMT.CODE_NAME STATUSNAME 
                                    , TMT.BUYERSTYLECODE || ' - ' || TMT.BUYERSTYLENAME AS BUYERSTYLENAME
                                    , ADS.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS AS STYLECOLORWAYS                                   
                                FROM T_AD_ADSM ADS
                                    LEFT JOIN T_CM_MCMT CMT ON CMT.M_CODE = 'AOStatus' AND CMT.S_CODE = ADS.STATUS AND S_CODE NOT IN ('**', '000', 'AC')
                                    LEFT JOIN T_00_STMT TMT ON TMT.STYLECODE = ADS.STYLECODE
                                    LEFT JOIN T_00_SCMT SCM ON SCM.STYLECODE = ADS.STYLECODE AND SCM.STYLECOLORSERIAL = ADS.STYLECOLORSERIAL 
                                WHERE 1=1 ";

            string aoNoCon = " AND ADS.ADNO = :P_AONO ";
            string buyerCon = " AND SUBSTR(ADS.STYLECODE, 1, 3) = :P_BUYER ";
            string facCon = " AND ADS.FACTORY = :P_FACTORY ";
            string buyerInfoCon = " AND ( UPPER(TMT.STYLECODE) LIKE UPPER('%'||:P_STYLECODE||'%') OR UPPER(TMT.STYLENAME) LIKE UPPER('%'||:P_STYLENAME||'%') OR UPPER(TMT.BUYERSTYLECODE) LIKE UPPER('%'||:P_BUYERSTYLECODE||'%') ) ";

            var oraParams = new List<OracleParameter>();
            if(!string.IsNullOrEmpty(aoNo))
            {
                strSql += aoNoCon;
                oraParams.Add(new OracleParameter("P_AONO", aoNo));
            }

            if (!string.IsNullOrEmpty(buyer))
            {
                strSql += buyerCon;
                oraParams.Add(new OracleParameter("P_BUYER", buyer));
            }

            if (!string.IsNullOrEmpty(factory))
            {
                strSql += facCon;
                oraParams.Add(new OracleParameter("P_FACTORY", factory));
            }

            if (!string.IsNullOrEmpty(buyerInfo))
            {
                strSql += buyerInfoCon;
                oraParams.Add(new OracleParameter("P_STYLECODE", buyerInfo));
                oraParams.Add(new OracleParameter("P_STYLENAME", buyerInfo));
                oraParams.Add(new OracleParameter("P_BUYERSTYLECODE", buyerInfo));
            }

            var lstAdsm = OracleDbManager.GetObjects<Adsm>(strSql, CommandType.Text, oraParams.ToArray());

            return lstAdsm;
        }

    }
}
