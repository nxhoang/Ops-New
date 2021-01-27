using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public class QueueBus
    {
        /// <summary>
        /// Get lines by factory id
        /// </summary>
        /// <param name="qcoFactory"></param>
        /// <param name="qcoYear"></param>
        /// <param name="qcoWeekNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<QueueEntity> GetProductionPackageByQco(string qcoFactory, string qcoYear, string qcoWeekNo, string buyer, string aoNo,  string styleInf)
        {
            var strSql = @" SELECT  QUE.QCOFACTORY, QUE.QCOYEAR, QUE.QCOWEEKNO, QUE.FACTORY, QUE.LINENO, QUE.AONO, QUE.BUYER, QUE.STYLECODE, QUE.STYLESIZE, QUE.STYLECOLORSERIAL
                                , QUE.REVNO, QUE.PRDPKG, QUE.NORMALIZEDPERCENT, QUE.PLANQTY, QUE.PRDSDAT, QUE.PRDEDAT, QUE.ORDQTY , NVL(QUE.CHANGEQCORANK , QUE.QCORANK) AS QCORANK
                            , QUE.STYLECODE || QUE.STYLESIZE || QUE.STYLECOLORSERIAL || QUE.REVNO ||QUE.AONO AS STYLEINF, QUE.PLANQTY REMAINQTY, QUE.DELIVERYDATE
                            FROM T_QC_QUEUE QUE 
                                    JOIN PKERP.T_00_STMT STM ON STM.STYLECODE = QUE.STYLECODE
                            WHERE QUE.QCOFACTORY = :P_QCOFACTORY  AND QCOYEAR = :P_QCOYEAR AND QUE.QCOWEEKNO = :P_QCOWEEKNO  ";

            string aoCon = " AND QUE.AONO = UPPER(:P_AONO) ";
            string buyerCon = " AND STM.BUYER = UPPER(:P_BUYER) ";
            string styleInfCon = @" AND ( STM.STYLECODE LIKE UPPER('%'||:P_STYLECODE||'%')
                                        OR UPPER(STM.STYLENAME) LIKE UPPER('%'||:P_STYLENAME||'%')
                                        OR UPPER(STM.BUYERSTYLECODE) LIKE UPPER('%'||:P_BUYERSTYLECODE||'%') 
                                        OR UPPER(STM.BUYERSTYLENAME) LIKE UPPER('%'||:P_BUYERSTYLENAME||'%') ) ";
            
            string orderBy = " ORDER BY QCORANK ";

            var oracleParams = new List<OracleParameter> {
                new OracleParameter("P_QCOFACTORY", qcoFactory),
                new OracleParameter("P_QCOYEAR", qcoYear),
                new OracleParameter("P_QCOWEEKNO", qcoWeekNo)
            };

            if (!string.IsNullOrEmpty(aoNo))
            {
                strSql += aoCon;
                oracleParams.Add(new OracleParameter("P_AONO", aoNo));
            }

            if (!string.IsNullOrEmpty(buyer))
            {
                strSql += buyerCon;
                oracleParams.Add(new OracleParameter("P_BUYER", buyer));
            }

            if (!string.IsNullOrEmpty(styleInf))
            {
                strSql += styleInfCon;
                oracleParams.Add(new OracleParameter("P_STYLECODE", styleInf));
                oracleParams.Add(new OracleParameter("P_STYLENAME", styleInf));
                oracleParams.Add(new OracleParameter("P_BUYERSTYLECODE", styleInf));
                oracleParams.Add(new OracleParameter("P_BUYERSTYLENAME", styleInf));
            }

            strSql += orderBy;

            var lines = OracleDbManager.GetObjects<QueueEntity>(strSql, CommandType.Text, oracleParams.ToArray(), ConstantGeneric.ConnectionStrMes);
            
            return lines;
        }
    }
}
