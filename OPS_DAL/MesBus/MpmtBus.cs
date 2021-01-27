using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    /// <summary>
    /// T_MX_MPMT
    /// </summary>
    public class MpmtBus
    {
        /// <summary>
        /// Get Style Information
        /// </summary>
        /// <param name="pkgGroup"></param>
        /// <returns></returns>
        public static Mpmt GetStyleInfoByPkgGroup(string pkgGroup)
        {
            var strSql = @" select stm.BUYERSTYLECODE, stm.BUYERSTYLENAME, scm.STYLECOLORWAYS, mpm.PACKAGEGROUP, mpm.STYLECODE, mpm.stylesize, mpm.STYLECOLORSERIAL, mpm.REVNO, mpm.TARGETQTY
                                , concat('http://203.113.151.204:8080/PKPDM/style/', Substr(mpm.StyleCode,1,3) , '/' , mpm.StyleCode , '/Images/' , sfd.FileName ) ImgLink
                            from t_mx_mpmt mpm
                                left join t_00_stmt	stm on stm.STYLECODE = mpm.STYLECODE
                                left join t_00_scmt scm on scm.stylecode = mpm.STYLECODE and scm.STYLECOLORSERIAL = mpm.STYLECOLORSERIAL
                                left join t_00_sfdt sfd on sfd.stylecode = mpm.stylecode and IS_MAIN = 'Y'
                            where mpm.PACKAGEGROUP = ?P_PACKAGEGROUP ; ";
            var oraParams = new List<MySqlParameter>
            {
                new MySqlParameter("P_PACKAGEGROUP", pkgGroup)
            };

            var mpmt = MySqlDBManager.GetObjects<Mpmt>(strSql, CommandType.Text, oraParams.ToArray()).FirstOrDefault();

            return mpmt;
        }

        /// <summary>
        /// Get list of group packages
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="plnStartDate"></param>
        /// <param name="plnEndDate"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static List<Mpmt> GetGroupPackages(string factory, string plnStartDate, string plnEndDate, string buyer, string styleInf, string aoNo)
        {
            /// 2020-08-07 Tai Le(Thomas)
            /// Add new field "MATReadiness"
            /// Original code:
            /// string strSqlSelT1 = @"SELECT T1.*, SUM(MPD.MX_IOT_COMPLETED) TOTALMADEQTY  FROM ( ";

            string strSqlSelT1 = @"SELECT T1.*, SUM(MPD.MX_IOT_COMPLETED) TOTALMADEQTY , cast(0 as decimal(6,2)) as MATReadiness FROM ( ";
            string strSqlJoinT1 = @" )T1 JOIN t_mx_mpdt MPD ON MPD.PACKAGEGROUP = T1.PACKAGEGROUP
                                  GROUP BY T1.MESFACTORY, T1.PACKAGEGROUP,  T1.STYLECODE, T1.STYLESIZE, T1.STYLECOLORSERIAL, T1.REVNO, T1.BUYER, T1.TARGETQTY,T1.STATUS, T1.MADEQTY, T1.MESPLNSTARTDATE, T1.MESPLNENDDATE, T1.AONO;";

            string strSql = @" SELECT DISTINCT PMT.MESFACTORY, PMT.PACKAGEGROUP,  PMT.STYLECODE, PMT.STYLESIZE, PMT.STYLECOLORSERIAL, PMT.REVNO, PMT.BUYER, PMT.TARGETQTY 
                                , PMT.STATUS, PMT.MADEQTY  
                                , date_format(str_to_date(MESPLNSTARTDATE, '%Y%m%d'),'%Y-%m-%d') AS MESPLNSTARTDATE
                                , date_format(str_to_date(PMT.MESPLNENDDATE , '%Y%m%d'),'%Y-%m-%d') AS MESPLNENDDATE
                                , PKG.AONO
                                , mcm.CODE_NAME AS STATUSNAME, STM.STYLENAME, STM.BUYERSTYLECODE, concat(PMT.STYLECOLORSERIAL, ' - ', SCM.STYLECOLORWAYS ) AS STYLECOLORWAYS
                                , concat(FCM.FACTORY, ' - ', FCM.NAME)  AS FACTORYNAME                                
                                FROM MES.T_MX_MPMT PMT JOIN T_MX_PPKG PKG ON PKG.PACKAGEGROUP = PMT.PACKAGEGROUP
                                     LEFT JOIN T_00_STMT STM ON STM.STYLECODE = PMT.STYLECODE
                                     LEFT JOIN T_CM_MCMT MCM ON MCM.S_CODE = PMT.STATUS AND M_CODE = 'PackageGroupStatus'
                                     LEFT JOIN T_00_SCMT SCM ON SCM.STYLECODE = PMT.STYLECODE AND SCM.STYLECOLORSERIAL = PMT.STYLECOLORSERIAL
                                     LEFT JOIN T_CM_FCMT FCM ON FCM.FACTORY = PMT.MESFACTORY
                                WHERE PMT.MESFACTORY = ?P_FACTORY
                                    AND ((PMT.MESPLNSTARTDATE BETWEEN ?P_MESPLNSTARTDATE1 AND ?P_MESPLNENDDATE1) 
                                    OR (PMT.MESPLNENDDATE BETWEEN ?P_MESPLNSTARTDATE2 AND ?P_MESPLNENDDATE2)) ";

            var buyerCon = " AND PMT.BUYER = ?P_BUYER ";
            //var aoNoCon = " AND PKG.AONO = UPPER(?P_AONO) ";
            var aoNoCon = " AND PKG.AONO LIKE UPPER(CONCAT('%', ?P_AONO, '%')) ";
            string styleInfCon = @" AND ( STM.STYLECODE LIKE UPPER(?P_STYLECODE)
                                        OR UPPER(STM.STYLENAME) LIKE UPPER(?P_STYLENAME)
                                        OR UPPER(STM.BUYERSTYLECODE) LIKE UPPER(?P_BUYERSTYLECODE) 
                                        OR UPPER(STM.BUYERSTYLENAME) LIKE UPPER(?P_BUYERSTYLENAME)) ";

            var oraParams = new List<MySqlParameter>
            {
                new MySqlParameter("P_FACTORY", factory),
                new MySqlParameter("P_MESPLNSTARTDATE1", plnStartDate),
                new MySqlParameter("P_MESPLNENDDATE1", plnEndDate),
                new MySqlParameter("P_MESPLNSTARTDATE2", plnStartDate),
                new MySqlParameter("P_MESPLNENDDATE2", plnEndDate)
            };

            if (!string.IsNullOrEmpty(buyer))
            {
                strSql += buyerCon;
                oraParams.Add(new MySqlParameter("P_BUYER", buyer));
            }

            if (!string.IsNullOrEmpty(aoNo))
            {
                strSql += aoNoCon;
                oraParams.Add(new MySqlParameter("P_AONO", aoNo));
            }

            if (!string.IsNullOrEmpty(styleInf))
            {
                strSql += styleInfCon;
                oraParams.Add(new MySqlParameter("P_STYLECODE", "%" + styleInf + "%"));
                oraParams.Add(new MySqlParameter("P_STYLENAME", "%" + styleInf + "%"));
                oraParams.Add(new MySqlParameter("P_BUYERSTYLECODE", "%" + styleInf + "%"));
                oraParams.Add(new MySqlParameter("P_BUYERSTYLENAME", "%" + styleInf + "%"));
            }

            string strSqlFull = strSqlSelT1 + strSql + strSqlJoinT1;

            var lstMpmt = MySqlDBManager.GetObjectsConvertType<Mpmt>(strSqlFull, CommandType.Text, oraParams.ToArray());
            oraParams.Clear();
            strSqlFull = "";

            ///* 2020-08-06 Tai Le(Thomas)
            ///* Based on the MES Package Group => Source MTOPS Package => QCO Material Detail => SUM(Plan_Qyt) / SUM(Request_Qty) => MES Package Group Material Readiness
            ///* Solution: use Linq
            try
            {


                // 1. MES Package Group 
                var MESPGroupList = lstMpmt.Select(x => x.PackageGroup).Distinct().ToArray();//string.Join("','", lstMpmt.Select(x => x.PackageGroup).Distinct().ToArray());

                // 2. Source MTOPS Package 
                foreach (string MESPG in MESPGroupList)
                {

                    strSqlFull = $"Select PACKAGEGROUP, PPACKAGE From t_mx_ppkg Where PACKAGEGROUP in ('{MESPG}') ";

                    // 3. QCO Material Detail => SUM(Plan_Qyt) / SUM(Request_Qty) => MES Package Group Material Readiness
                    // This part is using Oracle PKMES DB
                    string MTOPSPP = "";
                    var _data = MySqlDBManager.QueryToDatable(strSqlFull, CommandType.Text, null);
                    if (_data != null)
                    {
                        if (_data.Rows.Count > 0)
                        {
                            foreach (DataRow dr in _data.Rows)
                            {
                                if (String.IsNullOrEmpty(MTOPSPP))
                                    MTOPSPP = dr["PPACKAGE"].ToString();
                                else
                                    MTOPSPP = $"{MTOPSPP}','{dr["PPACKAGE"].ToString()}";
                            }
                        }
                        _data.Dispose();
                    }

                    //MES Package Group Material Readiness
                    decimal MATReadiness = 0;
                    /// 2020-08-10 Tai Le(Thomas)
                    /// Add:    AND QCOFactory = '{factory}'
                    strSqlFull = $@"
Select cast (AVG(NORMALIZEDPERCENT) AS decimal(5,2)) as MATReadiness
From PKMES.V_QCO_PP_LATESTQCO 
Where PRDPKG IN ( '{MTOPSPP}' ) 
AND QCOFactory = '{factory}'
";
                    var _T_QC_QCPM = OracleDbManager.Query(strSqlFull, null);
                    if (_T_QC_QCPM != null)
                    {
                        if (_T_QC_QCPM.Rows.Count > 0)
                        {
                            foreach (DataRow dr in _T_QC_QCPM.Rows)
                            {
                                MATReadiness = decimal.Parse(dr["MATReadiness"].ToString());
                            }
                        }
                        _data.Dispose();
                    }

                    //Update Material Readiness to the returned list
                    lstMpmt.Where(w => w.PackageGroup == MESPG).ToList().ForEach(f => f.MATReadiness = MATReadiness);
                }
            }
            catch (Exception eX)
            {
                var msg = eX.Message;
            }
            ///::END 2020-08-06 Tai Le(Thomas)

            return lstMpmt;
        }

        /// <summary>
        /// Get max package group.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="yymm"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static string GetMaxPackageGroup(string factory, string yymm)
        {
            string strSql = @" SELECT MAX(PACKAGEGROUP) PACKAGEGROUP  FROM T_MX_MPMT WHERE MESFACTORY = ?P_FACTORY AND SUBSTRING(PACKAGEGROUP, 6, 4) = ?P_YYMM ";

            var oraParams = new List<MySqlParameter>
            {
                new MySqlParameter("P_FACTORY", factory),
                new MySqlParameter("P_YYMM", yymm)
            };

            var mpmt = MySqlDBManager.GetObjects<Mpmt>(strSql, CommandType.Text, oraParams.ToArray()).FirstOrDefault();

            var strPkgGroup = factory + "-" + yymm + "-";
            if (mpmt.PackageGroup == null)
            {
                strPkgGroup += "000000001";
            }
            else
            {
                //Get the number of package group and inscrease 1
                var pkgNum = int.Parse(mpmt.PackageGroup.Substring(10, 9)) + 1;
                strPkgGroup += pkgNum.ToString("D9");
            }
            return strPkgGroup;
        }

        /// <summary>
        /// Update status of package group
        /// </summary>
        /// <param name="packageGroup"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static bool UpdatePackageGroupStatus(string packageGroup, string status)
        {
            string strSql = @"UPDATE T_MX_MPMT SET STATUS = ?P_STATUS WHERE PACKAGEGROUP = ?P_PACKAGEGROUP ";

            var myParam = new List<MySqlParameter>
            {
                new MySqlParameter("P_STATUS", status),
                new MySqlParameter("P_PACKAGEGROUP", packageGroup)
            };

            var resUpdate = MySqlDBManager.ExecuteQuery(strSql, CommandType.Text, myParam.ToArray());

            return resUpdate != null;
        }

        /// <summary>
        /// Insert package group with transaction
        /// </summary>
        /// <param name="mpmt"></param>
        /// <param name="myTrans"></param>
        /// <param name="myConnection"></param>
        /// <returns></returns>
        public static bool InsertPackageGroups(Mpmt mpmt, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @" INSERT INTO T_MX_MPMT (PACKAGEGROUP, MESFACTORY, STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, BUYER, TARGETQTY, STATUS, MESPLNSTARTDATE, MESPLNENDDATE
                                                        , MESACTSTARTDATE, MESACTENDDATE, PRIORITY, MADEQTY, REGISTRAR, CREATEDATE, LASTUPDATEDATE)
                                                    VALUES(?P_PACKAGEGROUP, ?P_MESFACTORY, ?P_STYLECODE, ?P_STYLESIZE, ?P_STYLECOLORSERIAL, ?P_REVNO, ?P_BUYER, ?P_TARGETQTY, ?P_STATUS, ?P_MESPLNSTARTDATE, ?P_MESPLNENDDATE
                                                    , ?P_MESACTSTARTDATE, ?P_MESACTENDDATE, ?P_PRIORITY, ?P_MADEQTY, ?P_REGISTRAR, SYSDATE(), SYSDATE()); ";


            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_PACKAGEGROUP", mpmt.PackageGroup),
                new MySqlParameter("P_MESFACTORY", mpmt.MesFactory),
                new MySqlParameter("P_STYLECODE", mpmt.StyleCode),
                new MySqlParameter("P_STYLESIZE", mpmt.StyleSize),
                new MySqlParameter("P_STYLECOLORSERIAL", mpmt.StyleColorSerial),
                new MySqlParameter("P_REVNO", mpmt.RevNo),
                new MySqlParameter("P_BUYER", mpmt.Buyer),
                new MySqlParameter("P_TARGETQTY", mpmt.TargetQty),
                new MySqlParameter("P_STATUS", mpmt.Status),
                new MySqlParameter("P_MESPLNSTARTDATE", mpmt.MesPlnStartDate),
                new MySqlParameter("P_MESPLNENDDATE", mpmt.MesPlnEndDate),
                new MySqlParameter("P_MESACTSTARTDATE", mpmt.MesActStartDate),
                new MySqlParameter("P_MESACTENDDATE", mpmt.MesActEndDate),
                new MySqlParameter("P_PRIORITY", mpmt.Priority),
                new MySqlParameter("P_MADEQTY", mpmt.MadeQty),
                new MySqlParameter("P_REGISTRAR", mpmt.Registrar)
            };

            var blIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, param.ToArray(), CommandType.Text, myTrans, myConnection);

            return blIns != null;
        }

        /// <summary>
        /// Insert list of package group.
        /// </summary>
        /// <param name="lstMpmt"></param>
        /// <returns></returns>
        public static bool InsertListPackageGroups(List<Mpmt> lstMpmt)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    foreach (var mpdt in lstMpmt)
                    {
                        InsertPackageGroups(mpdt, myTrans, myConnection);

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
        /// Get package group by factory and AO number
        /// </summary>
        /// <param name="mesFac"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="ppFactory"></param>
        /// <param name="aoNo"></param>
        /// <returns></returns>
        public static List<Mpmt> GetPackageGroupByFactory(string mesFac, string startDate, string endDate, string ppFactory, string aoNo, string buyer, string styleInf)
        {
            string strSql1 = @"select distinct t1.*  , case when t1.targetqty = t1.mxtarget then 'D' else 'N' end distributeStatus, cast(t1.targetqty - t1.mxtarget as SIGNED) as RemainQty
                               from t_mx_ppkg pkg 
			                        join ( ";

            string strSql2 = @" select  pmt.* , sum(pdt.mxtarget) mxtarget, max(SEQNO) maxSeq
				                            from t_mx_mpmt pmt 
						                        join t_mx_mpdt pdt on pmt.packagegroup = pdt.packagegroup
				                            where pdt.factory = ?P_MESFACTORY  
						                            and pdt.plnstartdate between ?P_PLNSTARTDATE1 and ?P_PLNENDDATE1  
						                            and pdt.plnenddate between ?P_PLNSTARTDATE2 and ?P_PLNENDDATE2 ";

            string strSql3 = @"   group by pmt.packagegroup
                                        )t1 on t1.packagegroup = pkg.packagegroup 
	                           where pkg.factory = ?P_PPFACTORY  ";

            //string aoCon = "and pkg.aono = ?P_AONO ";
            string aoCon = "and pkg.aono LIKE concat('%', ?P_AONO, '%') ";
            string conBuyer = " and pmt.buyer = ?P_BUYER  ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_MESFACTORY", mesFac),
                new MySqlParameter("P_PLNSTARTDATE1", startDate),
                new MySqlParameter("P_PLNENDDATE1", endDate),
                new MySqlParameter("P_PLNSTARTDATE2", startDate),
                new MySqlParameter("P_PLNENDDATE2", endDate)
            };

            if (!string.IsNullOrEmpty(buyer))
            {
                strSql2 += conBuyer;
                param.Add(new MySqlParameter("P_BUYER", buyer));
            }

            param.Add(new MySqlParameter("P_PPFACTORY", ppFactory));

            if (!string.IsNullOrEmpty(aoNo))
            {
                strSql3 += aoCon;
                param.Add(new MySqlParameter("P_AONO", aoNo));
            }

            var strSql = strSql1 + strSql2 + strSql3;
            var lstMpmt = MySqlDBManager.GetObjects<Mpmt>(strSql, CommandType.Text, param.ToArray());

            return lstMpmt;
        }

        /// <summary>
        /// Get package group with style information
        /// </summary>
        /// <param name="mesFac"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="ppFactory"></param>
        /// <param name="aoNo"></param>
        /// <param name="buyer"></param>
        /// <param name="styleInf"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Mpmt> GetPackageGroup(string mesFac, string startDate, string endDate, string ppFactory, string aoNo, string buyer, string styleInf)
        {
            string styleInfCon = string.Empty;
            string joinStyleTable = string.Empty;
            if (!string.IsNullOrEmpty(styleInf))
            {
                joinStyleTable = " LEFT JOIN t_00_stmt STM ON STM.STYLECODE = PMT.STYLECODE ";
                styleInfCon = $" and (pmt.stylecode like '%{styleInf}%' or stm.stylename like '%{styleInf}%' or stm.buyerstylecode like '%{styleInf}%' or stm.buyerstylename like '%{styleInf}%') ";
            }

            string strSql1 = @"select distinct t1.*  , case when t1.targetqty = t1.mxtarget then 'D' else 'N' end distributeStatus, cast(t1.targetqty - t1.mxtarget as SIGNED) as RemainQty
                               from t_mx_ppkg pkg 
			                        join ( ";

            string strSql2 = $@" select  pmt.* , sum(pdt.mxtarget) mxtarget, max(SEQNO) maxSeq
				                            from t_mx_mpmt pmt 
                                                {joinStyleTable}
						                        join t_mx_mpdt pdt on pmt.packagegroup = pdt.packagegroup
				                            where pdt.factory = ?P_MESFACTORY  
						                            and pdt.plnstartdate between ?P_PLNSTARTDATE1 and ?P_PLNENDDATE1  
						                            and pdt.plnenddate between ?P_PLNSTARTDATE2 and ?P_PLNENDDATE2 
                                                {styleInfCon}";

            string strSql3 = @"   group by pmt.packagegroup
                                        )t1 on t1.packagegroup = pkg.packagegroup 
	                           where pkg.factory = ?P_PPFACTORY  ";

            //string aoCon = "and pkg.aono = ?P_AONO ";
            string aoCon = "and pkg.aono LIKE concat('%', ?P_AONO, '%') ";
            string conBuyer = " and pmt.buyer = ?P_BUYER  ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_MESFACTORY", mesFac),
                new MySqlParameter("P_PLNSTARTDATE1", startDate),
                new MySqlParameter("P_PLNENDDATE1", endDate),
                new MySqlParameter("P_PLNSTARTDATE2", startDate),
                new MySqlParameter("P_PLNENDDATE2", endDate)
            };

            if (!string.IsNullOrEmpty(buyer))
            {
                strSql2 += conBuyer;
                param.Add(new MySqlParameter("P_BUYER", buyer));
            }

            param.Add(new MySqlParameter("P_PPFACTORY", ppFactory));

            if (!string.IsNullOrEmpty(aoNo))
            {
                strSql3 += aoCon;
                param.Add(new MySqlParameter("P_AONO", aoNo));
            }

            var strSql = strSql1 + strSql2 + strSql3;
            var lstMpmt = MySqlDBManager.GetObjects<Mpmt>(strSql, CommandType.Text, param.ToArray());

            return lstMpmt;
        }

        public static List<Mpmt> GetProductionPackageGroupByFactoryMES(string mesFac, string startDate, string endDate, string ppFactory, string aoNo, string buyer, string styleInf)
        {
            string strSql1 = @"select t1.*, pkg.ppackage  from t_mx_ppkg pkg join ( ";
            string strSql2 = @"select distinct t1.*  , case when t1.targetqty = t1.mxtarget then 'D' else 'N' end distributeStatus, cast(t1.targetqty - t1.mxtarget as SIGNED) as RemainQty
                               from t_mx_ppkg pkg 
			                        join ( ";

            string strSql3 = @" select  pmt.* , sum(pdt.mxtarget) mxtarget, max(SEQNO) maxSeq
				                            from t_mx_mpmt pmt 
						                        join t_mx_mpdt pdt on pmt.packagegroup = pdt.packagegroup
				                            where pdt.factory = ?P_MESFACTORY  
						                            and pdt.plnstartdate between ?P_PLNSTARTDATE1 and ?P_PLNENDDATE1  
						                            and pdt.plnenddate between ?P_PLNSTARTDATE2 and ?P_PLNENDDATE2 ";

            string strSql4 = @"   group by pmt.packagegroup
                                        )t1 on t1.packagegroup = pkg.packagegroup 
	                           where pkg.factory = ?P_PPFACTORY  ";

            string strSql5 = @")t1 on t1.packagegroup = pkg.packagegroup;";

            string aoCon = "and pkg.aono = ?P_AONO ";
            string conBuyer = " and pmt.buyer = ?P_BUYER  ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_MESFACTORY", mesFac),
                new MySqlParameter("P_PLNSTARTDATE1", startDate),
                new MySqlParameter("P_PLNENDDATE1", endDate),
                new MySqlParameter("P_PLNSTARTDATE2", startDate),
                new MySqlParameter("P_PLNENDDATE2", endDate)
            };

            if (!string.IsNullOrEmpty(buyer))
            {
                strSql3 += conBuyer;
                param.Add(new MySqlParameter("P_BUYER", buyer));
            }

            param.Add(new MySqlParameter("P_PPFACTORY", ppFactory));

            if (!string.IsNullOrEmpty(aoNo))
            {
                strSql4 += aoCon;
                param.Add(new MySqlParameter("P_AONO", aoNo));
            }

            var strSql = strSql1 + strSql2 + strSql3 + strSql4 + strSql5;
            var lstMpmt = MySqlDBManager.GetObjects<Mpmt>(strSql, CommandType.Text, param.ToArray());

            return lstMpmt;
        }

        /// <summary>
        /// Get list package group which has no MES package
        /// </summary>
        /// <param name="myTrans"></param>
        /// <param name="myConnection"></param>
        /// <returns></returns>
        public static List<Mpmt> GetListNoUsedPackageGroup()
        {
            string strSql = @"select * from t_mx_mpmt where PACKAGEGROUP not in (select pt.PACKAGEGROUP from t_mx_mpmt pt join t_mx_mpdt dt on dt.packagegroup = pt.packagegroup); ";

            var listMpmt = MySqlDBManager.GetObjects<Mpmt>(strSql, CommandType.Text, null);

            return listMpmt;

        }

        public static bool DeletePackageGroupById(string packageGroup, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @"delete from t_mx_mpmt where packagegroup  = ?P_PACKAGEGROUP; ";

            var myParam = new List<MySqlParameter>
            {
                new MySqlParameter("P_PACKAGEGROUP", packageGroup)
            };

            var blDel = MySqlDBManager.ExecuteQueryWithTrans(strSql, myParam.ToArray(), CommandType.Text, myTrans, myConnection);

            return blDel != null;
        }

        public static bool DeleteListPackageGroupById(List<Mpmt> listPackageGroup)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    foreach (var mpmt in listPackageGroup)
                    {
                        // Delete package group
                        DeletePackageGroupById(mpmt.PackageGroup, myTrans, myConnection);
                        //Delete production pacakge group
                        PpkgBus.DeleteProductionPackageByPkgGroup(mpmt.PackageGroup, myTrans, myConnection);
                    }

                    //If delete list package group in Oracle success then rollback MySql transaction.
                    if (!DeleteListPackageGroupByIdOracle(listPackageGroup))
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
        /// Complete package group
        /// </summary>
        /// <param name="packageGroup"></param>
        /// <param name="pkgGrouptatus"></param>
        /// <param name="completedId"></param>
        /// <returns></returns>
        public static bool CompletePackageGroup(string packageGroup, string pkgGrouptatus, string completedId)
        {
            string strSql = @"update t_mx_mpmt set status = ?P_STATUS, completedid = ?P_COMPLETEDID, completeddate = sysdate() where packagegroup = ?P_PACKAGEGROUP;";


            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_PACKAGEGROUP", packageGroup),
                new MySqlParameter("P_STATUS", pkgGrouptatus),
                new MySqlParameter("P_COMPLETEDID", completedId)
            };

            var blIns = MySqlDBManager.ExecuteQuery(strSql, CommandType.Text, param.ToArray());

            return blIns != null;
        }

        /// <summary>
        /// Update plan date of package group
        /// </summary>
        /// <param name="mpmt"></param>
        /// <param name="myTrans"></param>
        /// <param name="myConnection"></param>
        /// <returns></returns>
        public static bool UpdatePlanDatePackageGroup(Mpmt mpmt, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @" update t_mx_mpmt set MESPLNSTARTDATE = ?P_MESPLNSTARTDATE, MESPLNENDDATE = ?P_MESPLNENDDATE 
                                , UPDATEDID = ?P_UPDATEDID , LASTUPDATEDATE = SYSDATE() where packagegroup = ?P_PACKAGEGROUP; ";


            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_PACKAGEGROUP", mpmt.PackageGroup),
                new MySqlParameter("P_MESPLNSTARTDATE", mpmt.MesPlnStartDate),
                new MySqlParameter("P_MESPLNENDDATE", mpmt.MesPlnEndDate),
                new MySqlParameter("P_UPDATEDID", mpmt.UpdatedId),
            };

            var blIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, param.ToArray(), CommandType.Text, myTrans, myConnection);

            return blIns != null;
        }

        /// <summary>
        /// Get package group by production package
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="proPackage"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Mpmt GetPackageGroupByProductionPackage(string factory, string proPackage)
        {
            string strSql = @" SELECT PMT.*, PMT.TARGETQTY , CAST(pmt.TARGETQTY - SUM(PDT.MXTARGET)  AS SIGNED)  REMAINQTY 
                                FROM T_MX_PPKG PKG 
	                                JOIN T_MX_MPMT PMT ON PMT.PACKAGEGROUP = PKG.PACKAGEGROUP 
                                    JOIN T_MX_MPDT PDT ON PDT.PACKAGEGROUP = PMT.PACKAGEGROUP 
                                WHERE PKG.FACTORY = ?P_FACTORY AND PKG.PPACKAGE = ?P_PPACKAGE
                                GROUP BY  PDT.PACKAGEGROUP, PDT.FACTORY;  ";

            var oraParams = new List<MySqlParameter>
            {
                new MySqlParameter("P_FACTORY", factory),
                new MySqlParameter("P_PPACKAGE", proPackage)
            };

            return MySqlDBManager.GetObjects<Mpmt>(strSql, CommandType.Text, oraParams.ToArray()).FirstOrDefault();

        }

        #region Oracle
        /// <summary>
        /// Insert package group into Oracle
        /// </summary>
        /// <param name="mpmt"></param>
        /// <param name="oraTrans"></param>
        /// <param name="oraConn"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertPackageGroupsOracle(Mpmt mpmt, OracleTransaction oraTrans, OracleConnection oraConn)
        {
            string strSql = @" INSERT INTO T_MX_MPMT (PACKAGEGROUP, MESFACTORY, STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, BUYER, TARGETQTY, STATUS, MESPLNSTARTDATE, MESPLNENDDATE
                                                        , MESACTSTARTDATE, MESACTENDDATE, PRIORITY, MADEQTY, REGISTRAR, CREATEDATE, LASTUPDATEDATE)
                                                    VALUES(:P_PACKAGEGROUP, :P_MESFACTORY, :P_STYLECODE, :P_STYLESIZE, :P_STYLECOLORSERIAL, :P_REVNO, :P_BUYER, :P_TARGETQTY, :P_STATUS, :P_MESPLNSTARTDATE, :P_MESPLNENDDATE
                                                    , :P_MESACTSTARTDATE, :P_MESACTENDDATE, :P_PRIORITY, :P_MADEQTY, :P_REGISTRAR, SYSDATE, SYSDATE)";


            var param = new List<OracleParameter>
            {
                new OracleParameter("P_PACKAGEGROUP", mpmt.PackageGroup),
                new OracleParameter("P_MESFACTORY", mpmt.MesFactory),
                new OracleParameter("P_STYLECODE", mpmt.StyleCode),
                new OracleParameter("P_STYLESIZE", mpmt.StyleSize),
                new OracleParameter("P_STYLECOLORSERIAL", mpmt.StyleColorSerial),
                new OracleParameter("P_REVNO", mpmt.RevNo),
                new OracleParameter("P_BUYER", mpmt.Buyer),
                new OracleParameter("P_TARGETQTY", mpmt.TargetQty),
                new OracleParameter("P_STATUS", mpmt.Status),
                new OracleParameter("P_MESPLNSTARTDATE", mpmt.MesPlnStartDate),
                new OracleParameter("P_MESPLNENDDATE", mpmt.MesPlnEndDate),
                new OracleParameter("P_MESACTSTARTDATE", mpmt.MesActStartDate),
                new OracleParameter("P_MESACTENDDATE", mpmt.MesActEndDate),
                new OracleParameter("P_PRIORITY", mpmt.Priority),
                new OracleParameter("P_MADEQTY", mpmt.MadeQty),
                new OracleParameter("P_REGISTRAR", mpmt.Registrar)
            };

            var blIns = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text, oraTrans, oraConn);

            return blIns != null;
        }

        /// <summary>
        /// Update plan date of Package Group
        /// </summary>
        /// <param name="mpmt"></param>
        /// <param name="oraTrans"></param>
        /// <param name="oraConn"></param>
        /// <returns></returns>
        public static bool UpdatePlanDatePackageGroupOracle(Mpmt mpmt, OracleTransaction oraTrans, OracleConnection oraConn)
        {
            string strSql = @" update t_mx_mpmt set MESPLNSTARTDATE = :P_MESPLNSTARTDATE, MESPLNENDDATE = :P_MESPLNENDDATE 
                                , UPDATEDID = :P_UPDATEDID , LASTUPDATEDATE = SYSDATE where packagegroup = :P_PACKAGEGROUP ";


            var param = new List<OracleParameter>
            {
                new OracleParameter("P_PACKAGEGROUP", mpmt.PackageGroup),
                new OracleParameter("P_MESPLNSTARTDATE", mpmt.MesPlnStartDate),
                new OracleParameter("P_MESPLNENDDATE", mpmt.MesPlnEndDate),
                new OracleParameter("P_UPDATEDID", mpmt.UpdatedId),
            };

            var blIns = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text, oraTrans, oraConn);

            return blIns != null;
        }

        /// <summary>
        /// Get list no used package group
        /// </summary>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Mpmt> GetListNoUsedPackageGroupOracle()
        {
            string strSql = @"select * from t_mx_mpmt where PACKAGEGROUP not in (select pt.PACKAGEGROUP from t_mx_mpmt pt join t_mx_mpdt dt on dt.packagegroup = pt.packagegroup) ";

            var listMpmt = OracleDbManager.GetObjects<Mpmt>(strSql, CommandType.Text, null);

            return listMpmt;

        }

        /// <summary>
        /// Delete package group in MES Oracle
        /// </summary>
        /// <param name="packageGroup"></param>
        /// <param name="oraTrans"></param>
        /// <param name="oraConn"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool DeletePackageGroupByIdOracle(string packageGroup, OracleTransaction oraTrans, OracleConnection oraConn)
        {
            string strSql = @"delete from t_mx_mpmt where packagegroup  = :P_PACKAGEGROUP ";

            var myParam = new List<OracleParameter>
            {
                new OracleParameter("P_PACKAGEGROUP", packageGroup)
            };

            var blDel = OracleDbManager.ExecuteQuery(strSql, myParam.ToArray(), CommandType.Text, oraTrans, oraConn);

            return blDel != null;
        }

        /// <summary>
        /// Delete package group in Oracle
        /// </summary>
        /// <param name="listPackageGroup"></param>
        /// <returns></returns>
        public static bool DeleteListPackageGroupByIdOracle(List<Mpmt> listPackageGroup)
        {
            using (var oraConn = new OracleConnection(ConstantGeneric.ConnectionStrMes))
            {
                oraConn.Open();
                var oraTrans = oraConn.BeginTransaction();
                try
                {
                    foreach (var mpmt in listPackageGroup)
                    {
                        // Delete package group
                        DeletePackageGroupByIdOracle(mpmt.PackageGroup, oraTrans, oraConn);
                        //Delete production pacakge group
                        PpkgBus.DeleteProPackageByPkgGroupOracle(mpmt.PackageGroup, oraTrans, oraConn);
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
        #endregion

    }
}
