using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.DgsEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.DgsBus
{
    public class SchlBus
    {
        /// <summary>
        /// Get production target
        /// </summary>
        /// <param name="plnDate"></param>
        /// <param name="factory"></param>
        /// <param name="stylInf"></param>
        /// <param name="processName"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Schl> ProductionTarget(string plnDate, string factory, string linecd, string stylInf, string processName)
        {           
            
            string strSql = @" select sum(data) data, sum(trgt_qty) trgtQty, ( sum(trgt_qty) - sum(data)) AS pending
	                                , styl_nm stylnm, prcs_nm prcsnm, fctr_cd fctrcd, LINE_CD LINECD, pln_date plndate
                                from (
		                                SELECT T2.* 
		                                FROM (
			                                SELECT HIS.mac_addr, his.data, his.crt_dttm 
					                                , T1.PLN_DATE, T1.mchn_id, T1.fctr_cd , T1.line_cd, T1.styl_nm, T1.prcs_nm, T1.trgt_qty
							                                FROM t_dg_iot_output_history HIS 
							                                JOIN t_dg_iot_ntwrk_info INF ON INF.IOT_MAC_ADDR = HIS.MAC_ADDR
							                                JOIN (
										                                SELECT  MST.PLN_DATE, mst.mchn_id, mst.fctr_cd , mst.line_cd, mst.styl_nm, mst.prcs_nm, sum(mst.trgt_qty)  trgt_qty
										                                FROM t_dg_mcnh_schdl_mst MST 
										                                WHERE  MST.PLN_DATE = ?P_PLNDATE1 
												                                AND MST.FCTR_CD = ?P_FCTRCD 
												                                AND MST.STYL_NM = ?P_STYLNM  
												                                and MST.PRCS_NM = ?P_PRCSNM
												                                AND MST.LINE_CD = ?P_LINECD
										                                group by MST.PLN_DATE, mst.mchn_id, mst.fctr_cd , mst.line_cd, mst.styl_nm, mst.prcs_nm
								                                ) T1 ON T1.MCHN_ID = INF.MCHN_ID  				
			                                WHERE his.PLN_DATE = ?P_PLNDATE2
		                                )T2 JOIN  ( 
					                                select max(crt_dttm) crtDate, MAC_ADDR, PROCESS_ID from t_dg_iot_output_history  
					                                WHERE PLN_DATE = ?P_PLNDATE3
					                                GROUP BY MAC_ADDR,  PROCESS_ID 
				                                ) T3 on (t3.mac_addr = t2.mac_addr and t2.crt_dttm = t3.crtdate) 
                                ) T4
                                group by styl_nm, prcs_nm, fctr_cd, LINE_CD, pln_date  ; ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_PLNDATE1", plnDate),
                new MySqlParameter("P_FCTRCD", factory),
                new MySqlParameter("P_STYLNM", stylInf),
                new MySqlParameter("P_PRCSNM", processName),
                new MySqlParameter("P_LINECD", linecd),
                new MySqlParameter("P_PLNDATE2", plnDate),
                new MySqlParameter("P_PLNDATE3", plnDate),
            };

            var lstSchl = MySqlDBManager.GetObjectsDgs<Schl>(strSql, CommandType.Text, param.ToArray());

            return lstSchl;

        }

        /// <summary>
        /// Get line by factory and process name in DGS database
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="plnDate"></param>
        /// <param name="processName"></param>
        /// <returns></returns>
        public static List<Schl> GetLinesByFactoryDgs(string factory, string plnDate, string processName)
        {
            string strSql = @" SELECT DISTINCT LINE LINECD 
                                FROM t_dg_mcnh_schdl_mst 
                                WHERE factory = ?P_FCTRCD and  PLN_DATE = ?P_PLNDATE AND processid = ?P_PRCSNM ;  ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_FCTRCD", factory),
                new MySqlParameter("P_PLNDATE", plnDate),
                new MySqlParameter("P_PRCSNM", processName),
            };

            var lstSchl = MySqlDBManager.GetObjectsDgs<Schl>(strSql, CommandType.Text, param.ToArray());

            return lstSchl;

        }
    }
}
