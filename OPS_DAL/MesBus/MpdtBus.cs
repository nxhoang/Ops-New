using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_DAL.MesEntities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace OPS_DAL.MesBus
{
    public class MpdtBus
    {
        /// <summary>
        /// Get Mes packages from oracle database
        /// </summary>
        /// <param name="prdFactory"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="buyer"></param>
        /// <param name="styleInf"></param>
        /// <param name="aoNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Mpdt> GetMesPackagesModule(string prdFactory, string startDate, string endDate, string buyer, string styleInf, string aoNo)
        {
            var oraParams = new List<OracleParameter>
            {
                new OracleParameter("P_FACTORY", prdFactory),
                new OracleParameter("P_BUYER", buyer),
                new OracleParameter("P_PRDSTARTDATE", startDate),
                new OracleParameter("P_PRDENDDATE", endDate)
            };

            string aoNoWhere = string.Empty;
            string styleInfWhere = string.Empty;
            if (!string.IsNullOrEmpty(aoNo))
            {
                aoNoWhere = @" AND VPP.AONO LIKE UPPER('%'||:P_AONO||'%') ";
                oraParams.Add(new OracleParameter("P_AONO", aoNo));
            }

            if (!string.IsNullOrEmpty(styleInf))
            {
                styleInfWhere = @" AND (STM.STYLECODE LIKE UPPER('%'||:P_STYLEINF||'%') 
                                        OR UPPER(STM.STYLENAME) LIKE UPPER('%'||:P_STYLEINF||'%') 
                                        OR UPPER(STM.BUYERSTYLECODE) LIKE UPPER('%'||:P_STYLEINF||'%') ) ";
                oraParams.Add(new OracleParameter("P_STYLEINF", styleInf));
            }
          
            string strSql = $@"SELECT MPDT.LINESERIAL || '#' || NVL( MPDT.LINENO,'') AS LINECOMBINATION, LI.LINENAME 
                                    ,T1.*, MPMT.STYLECODE || MPMT.STYLESIZE || MPMT.STYLECOLORSERIAL || MPMT.REVNO || T1.AONO AS STYLEINF
                                        ,MPMT.STYLECODE , MPMT.STYLESIZE , MPMT.STYLECOLORSERIAL , MPMT.REVNO
                                   , MPDT.* 
                           FROM (
                                SELECT DISTINCT PKG.PACKAGEGROUP, PKG.AONO        
                                     FROM PKERP.VIEW_ERP_PSRSNP_PLAN VPP
                                     JOIN PKMES.T_MX_PPKG PKG ON PKG.PPACKAGE = VPP.PRDPKG                   
                                WHERE 
                                VPP.FACTORY = :P_FACTORY
                                AND VPP.BUYER = :P_BUYER
                                AND( (VPP.PRDSDAT BETWEEN :P_PRDSTARTDATE AND :P_PRDENDDATE) OR (VPP.PRDEDAT BETWEEN :P_PRDSTARTDATE  AND :P_PRDENDDATE) )
                                {aoNoWhere} {styleInfWhere}
                            )T1 JOIN PKMES.T_MX_MPMT MPMT ON MPMT.PACKAGEGROUP = T1.PACKAGEGROUP
                            JOIN PKMES.T_MX_MPDT MPDT ON MPDT.PACKAGEGROUP = MPMT.PACKAGEGROUP
                            LEFT JOIN PKMES.T_CM_LINE LI ON LI.LINESERIAL = MPDT.LINESERIAL ";

            

            var lstMpdt = OracleDbManager.GetObjectsByType<Mpdt>(strSql, CommandType.Text, oraParams.ToArray());

            return lstMpdt;
        }

        /// <summary>
        /// Get mes packages which have Iot send data in last 30 minute.
        /// </summary>
        /// <param name="factoryId"></param>
        /// <param name="yyyyMMdd"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Mpdt> GetMesPackageInLast30Mins(string factoryId, string yyyyMMdd)
        {
            var strSql = @"select t1.*
                                , case when t1.LAST_IOT_DATA_RECEIVE_TIME is null then 'N' 
		                                else 
			                                case when sysdate() > DATE_ADD(t1.LAST_IOT_DATA_RECEIVE_TIME, INTERVAL 30 MINUTE) then 'N' else 'Y' end 
		                                end IsActive 
                                from (
	                                select pdt.MXPACKAGE, pdt.LINESERIAL
                                    , max(opt.LAST_IOT_DATA_RECEIVE_TIME) as LAST_IOT_DATA_RECEIVE_TIME		
		                                from t_mx_mpdt pdt 
				                                join t_mx_opmt opm on opm.MXPACKAGE = pdt.MXPACKAGE
				                                join t_mx_opdt opt on opt.STYLECODE = opm.STYLECODE and opt.STYLESIZE = opm.STYLESIZE and opt.STYLECOLORSERIAL = opm.STYLECOLORSERIAL
										                                and opt.REVNO = opm.REVNO and opt.OPREVNO = opm.OPREVNO				
	                                where pdt.PLNSTARTDATE = ?P_PLNSTARTDATE and pdt.FACTORY = ?P_FACTORY and opt.IOTTYPE = 'FA'
	                                group by pdt.MXPACKAGE, pdt.LINESERIAL
                                )t1;";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_PLNSTARTDATE", yyyyMMdd),
                new MySqlParameter("P_FACTORY", factoryId)
            };

            var lstMpdt = MySqlDBManager.GetObjects<Mpdt>(strSql, CommandType.Text, param.ToArray());

            return lstMpdt;
        }

        /// <summary>
        /// Get list of Mes pakage by factory id and date
        /// </summary>
        /// <param name="factoryId"></param>
        /// <param name="yyMMDD"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Mpdt> GetListMesPkg(string factoryId, string yyyyMMdd)
        {
            var strSql = @"select li.linename, t1.* from t_cm_line li left join (
	                            select pdt.PACKAGEGROUP, pdt.MXPACKAGE, pdt.MXTARGET, pdt.FACTORY, pdt.LINESERIAL, pdt.FINISHEDQTY, pdt.MX_IOT_COMPLETED
			                            , pdt.MX_IOT_COMPLETED_DGS, pdt.MX_MANUAL_COMPLETED
			                            , case when MX_IOT_Completed is null then 'N' else 'Y' end IsActive
	                            from t_mx_mpdt pdt 
		                            -- join t_mx_mpmt pmt on pmt. packagegroup = pdt.PACKAGEGROUP     
	                            where pdt.PLNSTARTDATE = ?P_PLNSTARTDATE  and pdt.factory = ?P_FACTORY
                            )t1 ON li.LINESERIAL = t1.LINESERIAL
                            where li.factory = ?P_LINEFACTORY
                            order by t1.factory desc
                                , case when MX_IOT_Completed is null then 'N' else 'Y' end desc
		                        , CAST((case when regexp_substr(li.linename, '(\\d)(\\d)') is null 
		                                then regexp_substr(li.linename, '(\\d)') 
                                        else regexp_substr(li.linename, '(\\d)(\\d)') end) AS UNSIGNED)
                                , li.LINENAME;";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_PLNSTARTDATE", yyyyMMdd),
                new MySqlParameter("P_FACTORY", factoryId),
                new MySqlParameter("P_LINEFACTORY", factoryId)
            };

            var lstMpdt = MySqlDBManager.GetObjects<Mpdt>(strSql, CommandType.Text, param.ToArray());

            return lstMpdt;
        }

        /// <summary>
        /// Get mes package by style information
        /// </summary>
        /// <param name="plnStartDate"></param>
        /// <param name="factoryId"></param>
        /// <param name="lineSerial"></param>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Mpdt GetMesPackage(string plnStartDate, string factoryId, string lineSerial, string styleCode, string styleSize, string styleColorSerial, string revNo)
        {            

            var strSql = @"select pdt.* 
                            from t_mx_mpdt pdt 
	                            join t_mx_mpmt pmt on pmt. packagegroup = pdt.PACKAGEGROUP 
                            where pdt.FACTORY = ?P_FACTORY and pdt.LINESERIAL = ?P_LINESERIAL and pdt.PLNSTARTDATE = ?P_PLNSTARTDATE 
	                            and pmt.STYLECODE = ?P_STYLECODE and pmt.STYLESIZE = ?P_STYLESIZE and pmt.STYLECOLORSERIAL = ?P_STYLECOLORSERIAL and pmt.REVNO = ?P_REVNO;";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_FACTORY", factoryId),
                new MySqlParameter("P_LINESERIAL", lineSerial),
                new MySqlParameter("P_PLNSTARTDATE", plnStartDate),
                new MySqlParameter("P_STYLECODE", styleCode),
                new MySqlParameter("P_STYLESIZE", styleSize),
                new MySqlParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new MySqlParameter("P_REVNO", revNo)
            };

            var mpdt = MySqlDBManager.GetObjects<Mpdt>(strSql, CommandType.Text, param.ToArray()).FirstOrDefault();

            return mpdt;
        }

        /// <summary>
        /// Update manual mx package quantiy
        /// </summary>
        /// <param name="mxPackage"></param>
        /// <param name="manualQty"></param>
        /// <returns></returns>
        private static bool UpdateManualMxPkgQuantityMySql(string pkgGroup, int seqNo, string mxPackage, int manualQty, MySqlTransaction myTran, MySqlConnection myCon)
        {
            var strSql = @"update t_mx_mpdt set mx_manual_completed = ?P_MX_MANUAL_COMPLETED where PACKAGEGROUP = ?P_PACKAGEGROUP AND SEQNO = ?P_SEQNO AND MXPACKAGE = ?P_MXPACKAGE;";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_PACKAGEGROUP", pkgGroup),
                new MySqlParameter("P_SEQNO", seqNo),
                new MySqlParameter("P_MXPACKAGE", mxPackage),
                new MySqlParameter("P_MX_MANUAL_COMPLETED", manualQty)
            };

            var blRes = MySqlDBManager.ExecuteQueryWithTrans(strSql, param.ToArray(), CommandType.Text, myTran, myCon );

            return blRes != null;
        }

        /// <summary>
        /// Update mes package quantity by manual
        /// </summary>
        /// <param name="pkgGroup"></param>
        /// <param name="seqNo"></param>
        /// <param name="mxPackage"></param>
        /// <param name="manualQty"></param>
        /// <param name="myTran"></param>
        /// <param name="myCon"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateManualMxPkgQuantity(string pkgGroup, int seqNo, string mxPackage, int manualQty)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    if (!UpdateManualMxPkgQuantityMySql(pkgGroup, seqNo, mxPackage, manualQty, myTrans, myConnection))
                    {
                        myTrans.Rollback();
                        return false;
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

        /// <summary>
        /// Get list of mes package 
        /// </summary>
        /// <param name="factoryId"></param>
        /// <param name="strDate"></param>
        /// <returns></returns>
        public static List<Mpdt> GetMesPackagesScheduled(string factoryId, string scheDate)
        {
            //string strSql = $@"select li.LINENAME, li.WORKERS, case when mxpackage is null then 'N' else 'Y' end IsActive, t1.* from t_cm_line li 
            //                left join (
            //                    SELECT  mpt.*   
            //                    FROM T_MX_MPdT mpt 
            //                    WHERE mpt.factory = ?P_MESFACTORY and ?P_DATE between mpt.plnstartdate and mpt.plnenddate 
            //                )t1 on t1.LINESERIAL = li.lineserial
            //                WHERE  li.factory = ?P_LINEFACTORY;";

            var strSql = @"select li.LINENAME, li.WORKERS, case when mxpackage is null then 'N' else 'Y' end IsActive, t1.* 
                           from t_cm_line li left join (
                                select  max(opt.LAST_IOT_DATA) MX_IOT_Completed, OPT.LINESERIAL OP_LINE, mp.LINESERIAL, mp.mxpackage, mp.PLNSTARTDATE, mp.MXTARGET
                                from t_mx_mpdt mp 
		                                join t_mx_opmt opm on  mp.MXPACKAGE = opm.MXPACKAGE
                                        join t_mx_opdt opt on opt.stylecode = opm.STYLECODE 
				                                and opt.stylecolorserial = opm.STYLECOLORSERIAL 
				                                and opt.stylesize = opm.STYLESIZE 
                                                and opt.REVNO = opm.REVNO 
                                                and opt.OPREVNO = opm.OPREVNO
		                                join t_mx_opdt_mc mc on mc.stylecode = opt.STYLECODE 
				                                and mc.stylecolorserial = opt.STYLECOLORSERIAL 
				                                and mc.stylesize = opt.STYLESIZE 
                                                and mc.REVNO = opt.REVNO 
                                                and mc.OPREVNO = opt.OPREVNO
                                                and mc.OPSERIAL = opt.OPSERIAL
                                where mp.FACTORY = ?P_MESFACTORY and mp.PLNSTARTDATE = ?P_DATE 
                                group by OPT.lineserial, mp.LINESERIAL, mp.MXPACKAGE
                            )t1 on t1.op_line = li.lineserial 
                            where li.factory = ?P_LINEFACTORY
                            order by t1.plnstartdate desc
                                    , CAST((case when regexp_substr(li.linename, '(\\d)(\\d)') is null 
		                                    then regexp_substr(li.linename, '(\\d)') 
                                            else regexp_substr(li.linename, '(\\d)(\\d)') end) AS UNSIGNED)
                                    , li.LINENAME;";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_MESFACTORY", factoryId),
                new MySqlParameter("P_DATE", scheDate),
                new MySqlParameter("P_LINEFACTORY", factoryId)
            };

            var lstMpdt = MySqlDBManager.GetObjects<Mpdt>(strSql, CommandType.Text, param.ToArray());

            return lstMpdt;
        }

        /// <summary>
        /// Get MES package working process
        /// </summary>
        /// <param name="mesPackage"></param>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Mpdt> GetMesPackageWorkingProcess(string mesPackage, string styleCode, string styleSize, string styleColorSerial, string revNo, string getType)
        {
            string strSqlOpGroup = @"select sum(opd.LAST_IOT_DATA) AS TotalMade, opd.IOTTYPE, opd.OPGROUP, mcm.code_name AS OpGroupName
		                                     , mpm.PACKAGEGROUP
		                                    , mpd.MXPACKAGE, mpd.LINENO, mpd.SEQNO, mpd.MXTARGET, mpd.FACTORY, mpd.LINESERIAL 
		                                    , lin.LINENAME
		                                    , abs(mpd.MXTARGET - opd.LAST_IOT_DATA) As Balance
		                                    , case when opd.IOTTYPE = 'FA' then opd.last_iot_data else 0 end as Shipped      
                                    from t_mx_mpdt mpd  
	                                    join t_mx_mpmt mpm  on mpd.PACKAGEGROUP = mpm.PACKAGEGROUP
                                        left join t_cm_line lin on lin.LINESERIAL = mpd.LINESERIAL
                                        left join t_mx_opmt opm on opm.MXPACKAGE = mpd.MXPACKAGE
                                        left join t_mx_opdt opd on opd.STYLECODE = opm.STYLECODE and opd.STYLESIZE = opm.STYLESIZE and opd.STYLECOLORSERIAL = opm.STYLECOLORSERIAL 
								                                    and opd.REVNO = opm.REVNO and opd.OPREVNO = opm.OPREVNO 
                                        left join t_cm_mcmt mcm on mcm.S_CODE = opd.OPGROUP and mcm.M_CODE = 'OPGroup'
	                                    left join t_00_samt sam on sam.STYLECODE = opd.STYLECODE and sam.MODULEID = opd.MODULEID
                                    where opd.last_iot_data is not null and mpd.MXPACKAGE = ?P_MXPACKAGE
		                                    and opm.STYLECODE = ?P_STYLECODE and opm.STYLESIZE = ?P_STYLESIZE and opm.STYLECOLORSERIAL = ?P_STYLECOLORSERIAL and opm.REVNO = ?P_REVNO
                                    group by opd.IOTTYPE, opd.OPGROUP; ";

            string strSqlModule = @"select sum(opd.LAST_IOT_DATA) AS TotalMade, opd.IOTTYPE, opd.moduleid
		                             , mpm.PACKAGEGROUP
		                            , mpd.MXPACKAGE, mpd.LINENO, mpd.SEQNO, mpd.MXTARGET, mpd.FACTORY, mpd.LINESERIAL 
                                    , lin.LINENAME, sam.MODULENAME
                                    , abs(mpd.MXTARGET - opd.LAST_IOT_DATA) As Balance
                                    , case when opd.IOTTYPE = 'FA' then opd.last_iot_data else 0 end as Shipped         
                            from t_mx_mpmt mpm 
	                            join t_mx_mpdt mpd on mpd.PACKAGEGROUP = mpm.PACKAGEGROUP
                                left join t_cm_line lin on lin.LINESERIAL = mpd.LINESERIAL
                                left join t_mx_opmt opm on opm.MXPACKAGE = mpd.MXPACKAGE
                                left join t_mx_opdt opd on opd.STYLECODE = opm.STYLECODE and opd.STYLESIZE = opm.STYLESIZE and opd.STYLECOLORSERIAL = opm.STYLECOLORSERIAL 
                                                        and opd.REVNO = opm.REVNO and opd.OPREVNO = opm.OPREVNO
	                            left join t_00_samt sam on sam.STYLECODE = opd.STYLECODE and sam.MODULEID = opd.MODULEID
                            where opd.last_iot_data is not null and mpd.MXPACKAGE = ?P_MXPACKAGE
                                    and opm.STYLECODE = ?P_STYLECODE and opm.STYLESIZE = ?P_STYLESIZE and opm.STYLECOLORSERIAL = ?P_STYLECOLORSERIAL and opm.REVNO = ?P_REVNO
                            group by opd.IOTTYPE, opd.moduleid; ";

            var strSql = getType == "2" ? strSqlOpGroup : strSqlModule;

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_MXPACKAGE", mesPackage),
                new MySqlParameter("P_STYLECODE", styleCode),
                new MySqlParameter("P_STYLESIZE", styleSize),
                new MySqlParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new MySqlParameter("P_REVNO", revNo)
            };

            var lstMpdt = MySqlDBManager.GetAll<Mpdt>(strSql, CommandType.Text, param.ToArray());

            return lstMpdt;
        }

        /// <summary>
        /// Get mes package by package group
        /// </summary>
        /// <param name="packageGroup"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static List<Mpdt> GetMesPackages(string packageGroup, string seqNo)
        {
            string strSql = @"SELECT
                                m.stylecode,
                                m.stylesize,
                                m.stylecolorserial,
                                m.revno,
                                date_format(str_to_date(d.PLNSTARTDATE, '%Y%m%d'),'%Y-%m-%d') AS PLNSTARTDATE, 
                                date_format(str_to_date(d.PLNENDDATE , '%Y%m%d'),'%Y-%m-%d') AS PLNENDDATE,   	
                                d.PACKAGEGROUP, d.SEQNO, d.LINENO, d.MXPACKAGE, d.MXTARGET, d.FACTORY, d.FINISHEDQTY, d.LASTPRODUCTIONSYNC, d.TAKTTIME, d.WORKINGHOURS, d.PLNACTSTARTDATE, d.PLNACTENDDATE
                                , d.MXSTATUS, d.CONFIRMEDID, d.CONFIRMEDDATE, d.PRIORITY, d.MX_IOT_COMPLETED, d.MX_MANUAL_COMPLETED, d.REGISTRAR, d.CREATEDATE, d.LASTUPDATEDATE, d.UPDATEDID
                                , mc.code_name as StatusName, lin.LINENAME, d.LINESERIAL
                            FROM
                                t_mx_mpdt d
                                JOIN t_mx_mpmt m ON d.packagegroup = m.packagegroup
                                left join t_cm_mcmt mc on d.mxstatus = mc.s_code
                                left join t_cm_line lin on lin.lineserial = d.lineserial
                            WHERE
                                d.packagegroup = ?p_packagegroup
                                and mc.m_code = 'MESPackageStatus'";

            string seqCon = " AND SEQNO = ?P_SEQNO ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_PACKAGEGROUP", packageGroup)
            };

            //Check seqNo condition
            if (!string.IsNullOrEmpty(seqNo))
            {
                param.Add(new MySqlParameter("P_SEQNO", seqNo));
                strSql += seqCon;
            }

            var lstMpdt = MySqlDBManager.GetObjects<Mpdt>(strSql, CommandType.Text, param.ToArray());

            return lstMpdt;
        }

        /// <summary>
        /// Get mes packages by factory and date distribution.
        /// </summary>
        /// <param name="mesFac"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="ppFactory"></param>
        /// <param name="aoNo"></param>
        /// <returns></returns>
        public static List<Mpdt> GetMesPackagesByFactory(string mesFac, string startDate, string endDate, string ppFactory, string aoNo, string buyer, string styleInf)
        {
            //string strSql = @" select t1.*, pdt.* , concat(t1.stylecode, t1.stylesize, t1.stylecolorserial, t1.revno, t1.aono) StyleInf 
            //                     from t_mx_mpdt pdt   
            //                      join (
            //                        select distinct pmt.PACKAGEGROUP, pmt.MESFACTORY, pmt.stylecode, pmt.stylesize, pmt.stylecolorserial, pmt.revno, pmt.buyer, pkg.aoNo   
            //                        from t_mx_mpmt pmt join t_mx_ppkg pkg on pkg.packagegroup = pmt.packagegroup  
            //                        where pkg.factory = ?P_PPFACTORY and pkg.aono = ?P_AONO  
            //                         )t1 on t1.packagegroup = pdt.packagegroup   
            //                     where pdt.factory = ?P_MESFACTORY    
            //                      and pdt.plnstartdate between ?P_PLNSTARTDATE1 and ?P_PLNENDDATE1  
            //                       and pdt.plnenddate between ?P_PLNSTARTDATE2 and ?P_PLNENDDATE2;      ";

            string strSql1 = @" select t1.*, pdt.* , concat(t1.stylecode, t1.stylesize, t1.stylecolorserial, t1.revno, t1.aono) StyleInf 
                                from t_mx_mpdt pdt   
	                                join (  ";

            string strSql2 = @" select distinct pmt.PACKAGEGROUP, pmt.MESFACTORY, pmt.stylecode, pmt.stylesize, pmt.stylecolorserial, pmt.revno, pmt.buyer, pkg.aoNo   
			                                from t_mx_mpmt pmt join t_mx_ppkg pkg on pkg.packagegroup = pmt.packagegroup  
			                                where pkg.factory = ?P_PPFACTORY ";

            string strSql3 = @"  )t1 on t1.packagegroup = pdt.packagegroup   
                                where pdt.factory = ?P_MESFACTORY    
	                                and pdt.plnstartdate between ?P_PLNSTARTDATE1 and ?P_PLNENDDATE1  
	                                 and pdt.plnenddate between ?P_PLNSTARTDATE2 and ?P_PLNENDDATE2;   ";

            string conAo = " and pkg.aono = ?P_AONO ";
            string conBuyer = " and pmt.buyer = ?P_BUYER ";



            var param = new List<MySqlParameter>() {
                new MySqlParameter("P_PPFACTORY", ppFactory)
            };

            if (!string.IsNullOrEmpty(aoNo))
            {
                strSql2 += conAo;
                param.Add(new MySqlParameter("P_AONO", aoNo));
            }

            if (!string.IsNullOrEmpty(buyer))
            {
                strSql2 += conBuyer;
                param.Add(new MySqlParameter("P_BUYER", buyer));
            }


            var param2 = new List<MySqlParameter>
            {

                new MySqlParameter("P_MESFACTORY", mesFac),
                new MySqlParameter("P_PLNSTARTDATE1", startDate),
                new MySqlParameter("P_PLNENDDATE1", endDate),
                new MySqlParameter("P_PLNSTARTDATE2", startDate),
                new MySqlParameter("P_PLNENDDATE2", endDate)
            };

            param.AddRange(param2);

            string strSql = strSql1 + strSql2 + strSql3;

            var lstMpdt = MySqlDBManager.GetObjects<Mpdt>(strSql, CommandType.Text, param.ToArray());

            return lstMpdt;
        }

        /// <summary>
        /// Get max MES package id
        /// </summary>
        /// <param name="packageGroup"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static int GetMaxMesSeqPackageGroup(string packageGroup)
        {
            string strSql = @"select max(SEQNO) seqno, PACKAGEGROUP from t_mx_mpdt where packagegroup = ?P_PACKAGEGROUP; ";


            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_PACKAGEGROUP", packageGroup)
            };

            var mpdt = MySqlDBManager.GetObjects<Mpdt>(strSql, CommandType.Text, param.ToArray()).FirstOrDefault();

            if (mpdt == null) return 1;

            return mpdt.SeqNo + 1;

        }

        /// <summary>
        /// Update mes start plan
        /// </summary>
        /// <param name="packageGroup"></param>
        /// <param name="seqNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateMesStartPlan(string packageGroup, string seqNo)
        {
            string strSql = @" UPDATE T_MX_MPDT SET PLNACTSTARTDATE = SYSDATE() WHERE PACKAGEGROUP = ?P_PACKAGEGROUP AND SEQNO = ?P_SEQNO; ";

            var par = new List<MySqlParameter>
            {
                new MySqlParameter("P_PACKAGEGROUP", packageGroup),
                new MySqlParameter("P_SEQNO", seqNo)
            };

            var resUpdate = MySqlDBManager.ExecuteQuery(strSql, CommandType.Text, par.ToArray());

            return resUpdate != null;
        }

        /// <summary>
        /// Insert list of mes package
        /// </summary>
        /// <param name="mpdt"></param>
        /// <param name="myTrans"></param>
        /// <param name="myConnection"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertMesPackage(Mpdt mpdt, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @" INSERT INTO T_MX_MPDT(PACKAGEGROUP, SEQNO, LINENO, MXPACKAGE, MXTARGET, PLNSTARTDATE, PLNENDDATE, FACTORY, FINISHEDQTY, LASTPRODUCTIONSYNC, TAKTTIME, WORKINGHOURS, MXSTATUS, REGISTRAR, CREATEDATE, LASTUPDATEDATE, UPDATEDID, LINESERIAL)
                                VALUES(?P_PACKAGEGROUP, ?P_SEQNO, ?P_LINENO, ?P_MXPACKAGE, ?P_MXTARGET, ?P_PLNSTARTDATE, ?P_PLNENDDATE, ?P_FACTORY, ?P_FINISHEDQTY, ?P_LASTPRODUCTIONSYNC, ?P_TAKTTIME, ?P_WORKINGHOURS, ?MXSTATUS, ?P_REGISTRAR, SYSDATE(), SYSDATE(), ?UPDATEDID, ?LINESERIAL) ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_PACKAGEGROUP", mpdt.PackageGroup),
                new MySqlParameter("P_SEQNO", mpdt.SeqNo),
                new MySqlParameter("P_LINENO", mpdt.LineNo),
                new MySqlParameter("P_MXPACKAGE", mpdt.MxPackage),
                new MySqlParameter("P_MXTARGET", mpdt.MxTarget),
                new MySqlParameter("P_PLNSTARTDATE", mpdt.PlnStartDate),
                new MySqlParameter("P_PLNENDDATE", mpdt.PlnEndDate),
                new MySqlParameter("P_FACTORY", mpdt.Factory),
                new MySqlParameter("P_FINISHEDQTY", mpdt.FinishedQty),
                new MySqlParameter("P_LASTPRODUCTIONSYNC", mpdt.LastProductionSync),
                new MySqlParameter("P_TAKTTIME", mpdt.TaktTime),
                new MySqlParameter("P_WORKINGHOURS", mpdt.WorkingHours),
                new MySqlParameter("MXSTATUS", mpdt.MxStatus),
                new MySqlParameter("P_REGISTRAR", mpdt.Registrar),
                new MySqlParameter("UPDATEDID", mpdt.UpdatedId),
                new MySqlParameter("LINESERIAL", mpdt.LineSerial)
            };

            var blIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, param.ToArray(), CommandType.Text, myTrans, myConnection);

            return blIns != null;
        }

        /// <summary>
        /// Update plan start date and end date
        /// </summary>
        /// <param name="mpdt"></param>
        /// <param name="myTrans"></param>
        /// <param name="myConnection"></param>
        /// <returns></returns>
        internal static bool UpdatePlanDateMesPackage(Mpdt mpdt, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @" update t_mx_mpdt set plnstartdate = ?P_PLNSTARTDATE, plnenddate = ?P_PLNENDDATE, lineno = ?P_LINENO 
                                , lastupdatedate = SYSDATE(), updatedId = ?P_UPDATEDID, lineserial = ?P_LINESERIAL where packagegroup = ?P_PACKAGEGROUP and seqno = ?P_SEQNO;";


            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_PACKAGEGROUP", mpdt.PackageGroup),
                new MySqlParameter("P_SEQNO", mpdt.SeqNo),
                new MySqlParameter("P_PLNSTARTDATE", mpdt.PlnStartDate),
                new MySqlParameter("P_PLNENDDATE", mpdt.PlnEndDate),
                new MySqlParameter("P_LINENO", mpdt.LineNo),
                new MySqlParameter("P_UPDATEDID", mpdt.UpdatedId),
                new MySqlParameter("P_LINESERIAL", mpdt.LineSerial)
            };

            var blIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, param.ToArray(), CommandType.Text, myTrans, myConnection);

            return blIns != null;
        }

        internal static bool DeleteMesPackage(string pkgGroup, int seqNo, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @" delete from t_mx_mpdt where packagegroup = ?P_PACKAGEGROUP and seqno = ?P_SEQNO ";


            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_PACKAGEGROUP",pkgGroup),
                new MySqlParameter("P_SEQNO",seqNo)
            };

            var blDel = MySqlDBManager.ExecuteQueryWithTrans(strSql, param.ToArray(), CommandType.Text, myTrans, myConnection);

            return blDel != null;
        }

        public static bool InsertListMesPackages(List<Mpdt> lstMpdt)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    foreach (var mpdt in lstMpdt)
                    {
                        InsertMesPackage(mpdt, myTrans, myConnection);

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

        public static bool UpdateListPlanMesPackages(List<Mpdt> lstMpdt)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    foreach (var mpdt in lstMpdt)
                    {
                        UpdatePlanDateMesPackage(mpdt, myTrans, myConnection);

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

        /// <summary>
        /// Insert list of packages group and mes packages
        /// </summary>
        /// <param name="lstMpmt"></param>
        /// <param name="lstMpdt"></param>
        /// <returns></returns>
        public static bool InsertPackageGroupAndMesPackages(List<Mpmt> lstMpmt, List<Ppkg> lstPpkg, List<Mpdt> lstMpdt)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    if (lstMpmt != null)
                    {
                        //Insert package group master
                        foreach (var mpmt in lstMpmt)
                        {
                            MpmtBus.InsertPackageGroups(mpmt, myTrans, myConnection);
                        }
                    }

                    if (lstPpkg != null)
                    {
                        //Insert production package group
                        foreach (var ppkg in lstPpkg)
                        {
                            PpkgBus.InsertPPPackageGroups(ppkg, myTrans, myConnection);
                        }
                    }

                    if (lstMpdt != null)
                    {
                        //Insert list of mes packages
                        foreach (var mpdt in lstMpdt)
                        {
                            InsertMesPackage(mpdt, myTrans, myConnection);

                            //START ADD) SON (2019.10.10) - 11 October 2019 - Delete line detai
                            var proDate = DateTime.ParseExact(mpdt.PlnStartDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                            var lineDt = new Lndt { MxPackage = mpdt.MxPackage, LineSerial = mpdt.LineSerial, ProDate = proDate, RegisterId = mpdt.Registrar };
                            LndtBus.InsertLineDetailMySql(lineDt, myTrans, myConnection);
                            //END ADD) SON (2019.10.10) - 11 October 2019
                        }
                    }

                    //Insert MES package into Oracle
                    if (!InsertMesPackagesInfoOracle(lstMpmt, lstPpkg, lstMpdt))
                    {
                        myTrans.Rollback();
                        return false;
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

        public static bool DeleteMesPackages(List<Mpdt> lstMpdt)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {

                    if (lstMpdt != null)
                    {
                        //Insert list of mes packages
                        foreach (var mpdt in lstMpdt)
                        {
                            DeleteMesPackage(mpdt.PackageGroup, mpdt.SeqNo, myTrans, myConnection);
                        }
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

        public static List<Mpdt> GetMesPackagesByPackageGroup(string packageGroup)
        {

            string strSql = @" select concat(pdt.lineserial, '#', COALESCE( pdt.lineno,'')) as linecombination, pdt.* , t1.*, concat(t1.stylecode, t1.stylesize, t1.stylecolorserial, t1.revno, t1.aono) StyleInf, lin.LINENAME
                                from t_mx_mpdt pdt 
			                                join (
				                                 select  pmt.PACKAGEGROUP , pmt.stylecode, pmt.stylesize, pmt.stylecolorserial, pmt.revno, pmt.buyer, pkg.aoNo  
				                                 from t_mx_mpmt pmt 
						                                join t_mx_ppkg pkg on pmt.packagegroup = pkg.packagegroup 
				                                 where  pmt.packagegroup = ?P_PACKAGEGROUP1		 
				                                 group by pmt.packagegroup  
                                             )t1 on t1.packagegroup = pdt.packagegroup   
                                            join t_cm_line lin on lin.LINESERIAL = pdt.lineserial
                                where pdt.packagegroup = ?P_PACKAGEGROUP2;       ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_PACKAGEGROUP1", packageGroup),
                new MySqlParameter("P_PACKAGEGROUP2", packageGroup)
            };

            var lstMpdt = MySqlDBManager.GetObjects<Mpdt>(strSql, CommandType.Text, param.ToArray());

            return lstMpdt;
        }

        public static bool UpdateMesPackage(List<Mpmt> lstMpmt, List<Ppkg> lstPpkg, List<Mpdt> lstMpdt, List<Mpdt> lstMesPkgUpd, List<Mpdt> listMesPkdDel, List<Mpmt> listMpmt)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    if (listMpmt != null)
                    {
                        //Update plan date of package group
                        foreach (var mpmt in listMpmt)
                        {
                            MpmtBus.UpdatePlanDatePackageGroup(mpmt, myTrans, myConnection);
                        }
                    }

                    if (lstMpmt != null)
                    {
                        //Insert package group master
                        foreach (var mpmt in lstMpmt)
                        {
                            MpmtBus.InsertPackageGroups(mpmt, myTrans, myConnection);
                        }
                    }

                    if (lstPpkg != null)
                    {
                        //Insert production package group
                        foreach (var ppkg in lstPpkg)
                        {
                            PpkgBus.InsertPPPackageGroups(ppkg, myTrans, myConnection);
                        }
                    }

                    if (lstMpdt != null)
                    {
                        //Insert list of mes packages
                        foreach (var mpdt in lstMpdt)
                        {
                            InsertMesPackage(mpdt, myTrans, myConnection);

                            //START ADD) SON (2019.10.10) - 10 October 2019 - Delete line detai
                            var proDate = DateTime.ParseExact(mpdt.PlnStartDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                            var lineDt = new Lndt { MxPackage = mpdt.MxPackage, LineSerial = mpdt.LineSerial, ProDate = proDate, RegisterId = mpdt.Registrar };
                            LndtBus.InsertLineDetailMySql(lineDt, myTrans, myConnection);
                            //END ADD) SON (2019.10.10) - 10 October 2019
                        }
                    }

                    //Update plan date of mes package
                    if (lstMesPkgUpd != null)
                    {
                        //Insert list of mes packages
                        foreach (var updMpdt in lstMesPkgUpd)
                        {
                            UpdatePlanDateMesPackage(updMpdt, myTrans, myConnection);

                            //START ADD) SON (2019.10.10) - 11 October 2019 - Delete line detail and insert new line detail
                            LndtBus.DeleteLineDetailMySql(updMpdt.MxPackage, null, myTrans, myConnection);

                            //Crete production date.
                            var proDate = DateTime.ParseExact(updMpdt.PlnStartDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                            //Create object line detail to insert to database
                            var lineDt = new Lndt { MxPackage = updMpdt.MxPackage, LineSerial = updMpdt.LineSerial, ProDate = proDate, RegisterId = updMpdt.Registrar };
                            LndtBus.InsertLineDetailMySql(lineDt, myTrans, myConnection);
                            //END ADD) SON (2019.10.10) - 11 October 2019
                        }
                    }

                    //Delete mes package
                    if (listMesPkdDel != null)
                    {
                        //delete list of mes packages
                        foreach (var delMpdt in listMesPkdDel)
                        {
                            DeleteMesPackage(delMpdt.PackageGroup, delMpdt.SeqNo, myTrans, myConnection);

                            //START ADD) SON (2019.10.10) - 10 October 2019 - Delete line detail
                            LndtBus.DeleteLineDetailMySql(delMpdt.MxPackage, null, myTrans, myConnection);
                            //END ADD) SON (2019.10.10) - 10 October 2019
                        }
                    }

                    //Update MES package on Oracle.
                    if (!UpdateMesPackageOracle(lstMpmt, lstPpkg, lstMpdt, lstMesPkgUpd, listMesPkdDel, listMpmt))
                    {
                        myTrans.Rollback();
                        return false;
                    }

                    myTrans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    myTrans.Rollback();
                    return false;
                }
            }
        }

        /// <summary>
        /// Update MES package status
        /// </summary>
        /// <param name="packageGroup"></param>
        /// <param name="seqNo"></param>
        /// <param name="mesStatus"></param>
        /// <param name="confirmedId"></param>
        /// <returns></returns>
        public static bool UpdateMESPackageStatus(string packageGroup, string seqNo, string mesStatus, string confirmedId)
        {
            string strSql = @" update t_mx_mpdt set mxstatus = ?P_MXSTATUS, confirmedid = ?P_CONFIRMEDID, confirmeddate = sysdate() where packagegroup = ?P_PACKAGEGROUP AND SEQNO = ?P_SEQNO; ";


            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_PACKAGEGROUP", packageGroup),
                new MySqlParameter("P_SEQNO", seqNo),
                new MySqlParameter("P_MXSTATUS", mesStatus),
                new MySqlParameter("P_CONFIRMEDID", confirmedId)
            };

            var blIns = MySqlDBManager.ExecuteQuery(strSql, CommandType.Text, param.ToArray());

            return blIns != null;
        }

        /// <summary>
        /// Gets the by mxPackage.
        /// </summary>
        /// <param name="mxPackage">The mxPackage.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 30-Jul-19
        public static Mpdt GetByMxPackage(string mxPackage)
        {
            var prs = new List<MySqlParameter> { new MySqlParameter("P_MXPACKAGE", mxPackage) };
            var mpdt = MySqlDBManager.GetAll<Mpdt>("SP_MES_GETBYMXPACKAGE_MPDT", CommandType.StoredProcedure, prs.ToArray());

            return mpdt.FirstOrDefault();
        }

        #region Oracle
        /// <summary>
        /// Insert MES package to Oracle.
        /// </summary>
        /// <param name="mpdt"></param>
        /// <param name="oraTrans"></param>
        /// <param name="oraConn"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertMesPackageOracle(Mpdt mpdt, OracleTransaction oraTrans, OracleConnection oraConn)
        {
            string strSql = @" INSERT INTO T_MX_MPDT(PACKAGEGROUP, SEQNO, LINENO, MXPACKAGE, MXTARGET, PLNSTARTDATE, PLNENDDATE, FACTORY, FINISHEDQTY, LASTPRODUCTIONSYNC, TAKTTIME, WORKINGHOURS, MXSTATUS, REGISTRAR, CREATEDATE, LASTUPDATEDATE, UPDATEDID, LINESERIAL)
                                VALUES(:P_PACKAGEGROUP, :P_SEQNO, :P_LINENO, :P_MXPACKAGE, :P_MXTARGET, :P_PLNSTARTDATE, :P_PLNENDDATE, :P_FACTORY, :P_FINISHEDQTY, :P_LASTPRODUCTIONSYNC, :P_TAKTTIME, :P_WORKINGHOURS, :P_MXSTATUS, :P_REGISTRAR, SYSDATE, SYSDATE, :P_UPDATEDID, :P_LINESERIAL) ";

            var param = new List<OracleParameter>
            {
                new OracleParameter("P_PACKAGEGROUP", mpdt.PackageGroup),
                new OracleParameter("P_SEQNO", mpdt.SeqNo),
                new OracleParameter("P_LINENO", mpdt.LineNo),
                new OracleParameter("P_MXPACKAGE", mpdt.MxPackage),
                new OracleParameter("P_MXTARGET", mpdt.MxTarget),
                new OracleParameter("P_PLNSTARTDATE", mpdt.PlnStartDate),
                new OracleParameter("P_PLNENDDATE", mpdt.PlnEndDate),
                new OracleParameter("P_FACTORY", mpdt.Factory),
                new OracleParameter("P_FINISHEDQTY", mpdt.FinishedQty),
                new OracleParameter("P_LASTPRODUCTIONSYNC", mpdt.LastProductionSync),
                new OracleParameter("P_TAKTTIME", mpdt.TaktTime),
                new OracleParameter("P_WORKINGHOURS", mpdt.WorkingHours),
                new OracleParameter("P_MXSTATUS", mpdt.MxStatus),
                new OracleParameter("P_REGISTRAR", mpdt.Registrar),
                new OracleParameter("P_UPDATEDID", mpdt.UpdatedId),
                new OracleParameter("P_LINESERIAL", mpdt.LineSerial)
            };

            var blIns = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text, oraTrans, oraConn);

            return blIns != null;
        }

        /// <summary>
        /// Insert MES package information into Oracle
        /// </summary>
        /// <param name="lstMpmt"></param>
        /// <param name="lstPpkg"></param>
        /// <param name="lstMpdt"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertMesPackagesInfoOracle(List<Mpmt> lstMpmt, List<Ppkg> lstPpkg, List<Mpdt> lstMpdt)
        {
            using (var oraConn = new OracleConnection(ConstantGeneric.ConnectionStrMes))
            {
                oraConn.Open();
                var oraTrans = oraConn.BeginTransaction();
                try
                {
                    if (lstMpmt != null)
                    {
                        //Insert package group master
                        foreach (var mpmt in lstMpmt)
                        {
                            MpmtBus.InsertPackageGroupsOracle(mpmt, oraTrans, oraConn);
                        }
                    }

                    if (lstPpkg != null)
                    {
                        //Insert production package group
                        foreach (var ppkg in lstPpkg)
                        {
                            PpkgBus.InsertPPPackageGroupsOracle(ppkg, oraTrans, oraConn);
                        }
                    }

                    if (lstMpdt != null)
                    {
                        //Insert list of mes packages
                        foreach (var mpdt in lstMpdt)
                        {
                            InsertMesPackageOracle(mpdt, oraTrans, oraConn);

                            //START ADD) SON (2019.10.10) - 11 October 2019 - Insert line detail
                            var proDate = DateTime.ParseExact(mpdt.PlnStartDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                            var lineDt = new Lndt { MxPackage = mpdt.MxPackage, LineSerial = mpdt.LineSerial, ProDate = proDate, RegisterId = mpdt.Registrar };
                            LndtBus.InsertLineDetailOracle(lineDt, oraTrans, oraConn);
                            //END ADD) SON (2019.10.10) - 11 October 2019
                        }
                    }

                    oraTrans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    oraTrans.Rollback();
                    return false;
                }
            }
        }

        /// <summary>
        /// Update plan date of MES package
        /// </summary>
        /// <param name="mpdt"></param>
        /// <param name="oraTrans"></param>
        /// <param name="oraConn"></param>
        /// <returns></returns>
        internal static bool UpdatePlanDateMesPackageOracle(Mpdt mpdt, OracleTransaction oraTrans, OracleConnection oraConn)
        {
            string strSql = @" update t_mx_mpdt set plnstartdate = :P_PLNSTARTDATE, plnenddate = :P_PLNENDDATE, lineno = :P_LINENO 
                                , lastupdatedate = SYSDATE, updatedId = :P_UPDATEDID, lineserial = :P_LINESERIAL where packagegroup = :P_PACKAGEGROUP and seqno = :P_SEQNO ";


            var param = new List<OracleParameter>
            {
                new OracleParameter("P_PACKAGEGROUP", mpdt.PackageGroup),
                new OracleParameter("P_SEQNO", mpdt.SeqNo),
                new OracleParameter("P_PLNSTARTDATE", mpdt.PlnStartDate),
                new OracleParameter("P_PLNENDDATE", mpdt.PlnEndDate),
                new OracleParameter("P_LINENO", mpdt.LineNo),
                new OracleParameter("P_UPDATEDID", mpdt.UpdatedId),
                new OracleParameter("P_LINESERIAL", mpdt.LineSerial)
            };

            var blIns = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text, oraTrans, oraConn);

            return blIns != null;
        }

        /// <summary>
        /// Delete MES package in oracle
        /// </summary>
        /// <param name="pkgGroup"></param>
        /// <param name="seqNo"></param>
        /// <param name="oraTrans"></param>
        /// <param name="oraConn"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        internal static bool DeleteMesPackageOracle(string pkgGroup, int seqNo, OracleTransaction oraTrans, OracleConnection oraConn)
        {
            string strSql = @" delete from t_mx_mpdt where packagegroup = :P_PACKAGEGROUP and seqno = :P_SEQNO ";


            var param = new List<OracleParameter>
            {
                new OracleParameter("P_PACKAGEGROUP",pkgGroup),
                new OracleParameter("P_SEQNO",seqNo)
            };

            var blDel = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text, oraTrans, oraConn);

            return blDel != null;
        }

        public static bool UpdateMesPackageOracle(List<Mpmt> lstMpmt, List<Ppkg> lstPpkg, List<Mpdt> lstMpdt, List<Mpdt> lstMesPkgUpd, List<Mpdt> listMesPkdDel, List<Mpmt> listMpmt)
        {
            using (var oraConn = new OracleConnection(ConstantGeneric.ConnectionStrMes))
            {
                oraConn.Open();
                var oraTrans = oraConn.BeginTransaction();
                try
                {
                    if (listMpmt != null)
                    {
                        //Update plan date of package group
                        foreach (var mpmt in listMpmt)
                        {
                            MpmtBus.UpdatePlanDatePackageGroupOracle(mpmt, oraTrans, oraConn);
                        }
                    }

                    if (lstMpmt != null)
                    {
                        //Insert new package group
                        foreach (var mpmt in lstMpmt)
                        {
                            MpmtBus.InsertPackageGroupsOracle(mpmt, oraTrans, oraConn);
                        }
                    }

                    if (lstPpkg != null)
                    {
                        //Insert new production package group
                        foreach (var ppkg in lstPpkg)
                        {
                            PpkgBus.InsertPPPackageGroupsOracle(ppkg, oraTrans, oraConn);
                        }
                    }

                    if (lstMpdt != null)
                    {
                        //Insert list of new mes packages
                        foreach (var mpdt in lstMpdt)
                        {
                            InsertMesPackageOracle(mpdt, oraTrans, oraConn);

                            //START ADD) SON (2019.10.10) - 11 October 2019 - Delete line detai
                            var proDate = DateTime.ParseExact(mpdt.PlnStartDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                            var lineDt = new Lndt { MxPackage = mpdt.MxPackage, LineSerial = mpdt.LineSerial, ProDate = proDate, RegisterId = mpdt.Registrar };
                            LndtBus.InsertLineDetailOracle(lineDt, oraTrans, oraConn);
                            //END ADD) SON (2019.10.10) - 11 October 2019
                        }
                    }

                    //Update plan date of mes package
                    if (lstMesPkgUpd != null)
                    {
                        //Insert list of mes packages
                        foreach (var updMpdt in lstMesPkgUpd)
                        {
                            UpdatePlanDateMesPackageOracle(updMpdt, oraTrans, oraConn);

                            //START ADD) SON (2019.10.10) - 11 October 2019 - Delete line detail and insert new line detail
                            LndtBus.DeleteLineDetailOracle(updMpdt.MxPackage, null, oraTrans, oraConn);

                            //Get production date
                            var proDate = DateTime.ParseExact(updMpdt.PlnStartDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                            //Create object line detail to insert
                            var lineDt = new Lndt { MxPackage = updMpdt.MxPackage, LineSerial = updMpdt.LineSerial, ProDate = proDate, RegisterId = updMpdt.Registrar };
                            LndtBus.InsertLineDetailOracle(lineDt, oraTrans, oraConn);
                            //END ADD) SON (2019.10.10) - 11 October 2019
                        }
                    }

                    //Delete mes package
                    if (listMesPkdDel != null)
                    {
                        //delete list of mes packages
                        foreach (var delMpdt in listMesPkdDel)
                        {
                            DeleteMesPackageOracle(delMpdt.PackageGroup, delMpdt.SeqNo, oraTrans, oraConn);

                            //START ADD) SON (2019.10.10) - 11 October 2019 - Delete line detail
                            LndtBus.DeleteLineDetailOracle(delMpdt.MxPackage, null, oraTrans, oraConn);
                            //END ADD) SON (2019.10.10) - 11 October 2019
                        }
                    }

                    oraTrans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    oraTrans.Rollback();
                    return false;
                }
            }
        }

        /// <summary>
        /// Update mx package quantity in Oracle.
        /// </summary>
        /// <param name="pkgGroup"></param>
        /// <param name="seqNo"></param>
        /// <param name="mxPackage"></param>
        /// <param name="manualQty"></param>
        /// <param name="oraTran"></param>
        /// <param name="oraCon"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        private static bool UpdateManualMxPkgQuantityOracle(string pkgGroup, int seqNo, string mxPackage, int manualQty, OracleTransaction oraTran, OracleConnection oraCon)
        {
            var strSql = @"update pkmes.t_mx_mpdt set mx_manual_completed = :P_MX_MANUAL_COMPLETED where PACKAGEGROUP = :P_PACKAGEGROUP AND SEQNO = :P_SEQNO AND MXPACKAGE = :P_MXPACKAGE";

            var param = new List<OracleParameter>
            {
                new OracleParameter("P_MX_MANUAL_COMPLETED", manualQty),
                new OracleParameter("P_PACKAGEGROUP", pkgGroup),
                new OracleParameter("P_SEQNO", seqNo),
                new OracleParameter("P_MXPACKAGE", mxPackage)
            };

            var blIns = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text, oraTran, oraCon);

            return blIns != null;

        }

        /// <summary>
        /// Update manual mes package quantity
        /// </summary>
        /// <param name="pkgGroup"></param>
        /// <param name="seqNo"></param>
        /// <param name="mxPackage"></param>
        /// <param name="manualQty"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateManualMxPkgQuantityOracle(string pkgGroup, int seqNo, string mxPackage, int manualQty)
        {

            using (var oraConn = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                oraConn.Open();
                var oraTrans = oraConn.BeginTransaction();
                try
                {
                    if (!UpdateManualMxPkgQuantityOracle(pkgGroup, seqNo, mxPackage, manualQty, oraTrans, oraConn))
                    {
                        oraTrans.Rollback();
                        return false;
                    }

                    oraTrans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    oraTrans.Rollback();
                    return false;
                }
            }
        }

        #endregion

        /*----- Code by Dinh Van, 2020-09-21 -----*/
        public static List<Mpdt> GetListMesPkgHaveNameFactory(string factoryId, string yyyyMMdd)
        {
            var strSql = @"select li.linename, t1.* from t_cm_line li left join (
	                            select pdt.PACKAGEGROUP, pdt.MXPACKAGE, pdt.MXTARGET, pdt.FACTORY, pdt.LINESERIAL, pdt.FINISHEDQTY, pdt.MX_IOT_COMPLETED
			                            , pdt.MX_IOT_COMPLETED_DGS, pdt.MX_MANUAL_COMPLETED
			                            , case when MX_IOT_Completed is null then 'N' else 'Y' end IsActive
                                        , concat('[',fcm.factory, '] ',  fcm.name) as factoryName
	                            from t_mx_mpdt pdt 
		                            join T_CM_FCMT fcm on fcm. FACTORY = pdt.FACTORY     
	                            where pdt.PLNSTARTDATE = ?P_PLNSTARTDATE  and pdt.factory = ?P_FACTORY
                            )t1 ON li.LINESERIAL = t1.LINESERIAL
                            where li.factory = ?P_LINEFACTORY
                            order by t1.factory desc
                                , case when MX_IOT_Completed is null then 'N' else 'Y' end desc
		                        , CAST((case when regexp_substr(li.linename, '(\\d)(\\d)') is null 
		                                then regexp_substr(li.linename, '(\\d)') 
                                        else regexp_substr(li.linename, '(\\d)(\\d)') end) AS UNSIGNED)
                                , li.LINENAME;";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_PLNSTARTDATE", yyyyMMdd),
                new MySqlParameter("P_FACTORY", factoryId),
                new MySqlParameter("P_LINEFACTORY", factoryId)
            };

            var lstMpdt = MySqlDBManager.GetObjects<Mpdt>(strSql, CommandType.Text, param.ToArray());

            return lstMpdt;
        }
    }
}
