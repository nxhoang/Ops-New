using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OPS_DAL.MesBus
{
    public class PpkgBus
    {
        /// <summary>
        /// Get production package
        /// </summary>
        /// <param name="productionPkgId"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Ppkg GetProPackageById(string productionPkgId)
        {
            string strSql = @" SELECT PPK.* , 0 as NORMALIZEDPERCENT , '' as LATESTQCOTIME
                                FROM T_MX_PPKG PPK
                                WHERE PPK.PPACKAGE = ?P_PPACKAGE";

            var oraParams = new List<MySqlParameter>
            {
                new MySqlParameter("P_PPACKAGE", productionPkgId)
            };

            var ppkg = MySqlDBManager.GetObjectsConvertType<Ppkg>(strSql, CommandType.Text, oraParams.ToArray()).FirstOrDefault();

            return ppkg;
        }

        /// <summary>
        /// Get production packages
        /// </summary>
        /// <param name="packageGroup"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Ppkg> GetProPackages(string packageGroup)
        {
            string strSql = @" SELECT PPK.* , 0 as NORMALIZEDPERCENT , '' as LATESTQCOTIME
                                FROM T_MX_PPKG PPK
                                WHERE PPK.PACKAGEGROUP = ?P_PACKAGEGROUP  ";

            // 2019-06-18 Tai Le (Thomas) 
             strSql = @" SELECT PPK.* , '' as NORMALIZEDPERCENT , '' as LATESTQCOTIME , T_MX_MPMT.*
                         FROM T_MX_PPKG PPK 
                         INNER JOIN T_MX_MPMT ON PPK.PACKAGEGROUP = T_MX_MPMT.PACKAGEGROUP
                         WHERE PPK.PACKAGEGROUP = ?P_PACKAGEGROUP  ";

            var oraParams = new List<MySqlParameter>
            {
                new MySqlParameter("P_PACKAGEGROUP", packageGroup)
            };

            var lstPpkg = MySqlDBManager.GetObjects<Ppkg>(strSql, CommandType.Text, oraParams.ToArray());

            return lstPpkg;
        }

        /// <summary>
        /// Get list of distributed production package by AO number and factory id
        /// </summary>
        /// <param name="aoNo"></param>
        /// <param name="factoryId"></param>
        /// <returns></returns>
        public static List<Ppkg> GetListPackageGroupByAoNoFactory(string aoNo, string factoryId)
        {
            string strSql = @" SELECT * FROM T_MX_PPKG 
                               WHERE AONO = ?P_AONO AND FACTORY = ?P_FACTORY  ";

            var oraParams = new List<MySqlParameter>
            {
                new MySqlParameter("P_AONO", aoNo),
                new MySqlParameter("P_FACTORY", factoryId)
            };

            var lstPpkg = MySqlDBManager.GetObjects<Ppkg>(strSql, CommandType.Text, oraParams.ToArray());

            return lstPpkg;
        }

        /// <summary>
        /// Insert production package group
        /// </summary>
        /// <param name="ppkg"></param>
        /// <param name="myTrans"></param>
        /// <param name="myConnection"></param>
        /// <returns></returns>
        public static bool InsertPPPackageGroups(Ppkg ppkg, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @" INSERT INTO T_MX_PPKG (PACKAGEGROUP, SEQNO, PPACKAGE, FACTORY, AONO, ORDQTY, PLANQTY)
                                VALUES(?P_PACKAGEGROUP, ?P_SEQNO, ?P_PPACKAGE, ?P_FACTORY, ?P_AONO, ?P_ORDQTY, ?P_PLANQTY) ; ";


            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_PACKAGEGROUP", ppkg.PackageGroup),
                new MySqlParameter("P_SEQNO", ppkg.SeqNo),
                new MySqlParameter("P_PPACKAGE", ppkg.PPackage),
                new MySqlParameter("P_FACTORY", ppkg.Factory),
                new MySqlParameter("P_AONO", ppkg.AoNo),
                new MySqlParameter("P_ORDQTY", ppkg.OrdQty),
                new MySqlParameter("P_PLANQTY", ppkg.PlanQty)
            };

            var blIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, param.ToArray(), CommandType.Text, myTrans, myConnection);

            return blIns != null;
        }

        /// <summary>
        /// Insert list of production package group.
        /// </summary>
        /// <param name="lstPpkg"></param>
        /// <returns></returns>
        public static bool InsertListPPPackageGroups(List<Ppkg> lstPpkg)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    foreach (var ppkg in lstPpkg)
                    {
                        InsertPPPackageGroups(ppkg, myTrans, myConnection);
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
        /// Get production package group by factory and AO number
        /// </summary>
        /// <param name="mesFac"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="ppFactory"></param>
        /// <param name="aoNo"></param>
        /// <returns></returns>
        public static List<Ppkg> GetProductionPackageByFactory(string mesFac, string startDate, string endDate, string ppFactory, string aoNo, string buyer, string styleInf)
        {           
            string strSql1 = @" select pkg.*
                               from t_mx_ppkg pkg 
			                        join ( ";

            string strSql2 = @" select  pmt.PACKAGEGROUP
				                            from t_mx_mpmt pmt 
						                        join t_mx_mpdt pdt on pmt.packagegroup = pdt.packagegroup
				                            where pdt.factory = ?P_MESFACTORY  
						                        and pdt.plnstartdate between ?P_PLNSTARTDATE1 and ?P_PLNENDDATE1  
						                            and pdt.plnenddate between ?P_PLNSTARTDATE2 and ?P_PLNENDDATE2	";

            string strSql3 = @"  group by pmt.packagegroup
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

            var lstPpkg = MySqlDBManager.GetObjects<Ppkg>(strSql, CommandType.Text, param.ToArray());

            return lstPpkg;
        }

        /// <summary>
        /// Get list production package which has no relationship with MES package
        /// </summary>
        /// <param name="myTrans"></param>
        /// <param name="myConnection"></param>
        /// <returns></returns>
        public static List<Ppkg> GetListNoUsedProductionPackage()
        {
            string strSql = @"select * from t_mx_ppkg where PACKAGEGROUP not in (select pt.PACKAGEGROUP from t_mx_mpmt pt join t_mx_mpdt dt on dt.packagegroup = pt.packagegroup); ";

            var lstPpkg = MySqlDBManager.GetObjects<Ppkg>(strSql, CommandType.Text, null);

            return lstPpkg;
            
        }

        /// <summary>
        /// Delete production package by package group
        /// </summary>
        /// <param name="packageGroup"></param>
        /// <param name="myTrans"></param>
        /// <param name="myConnection"></param>
        /// <returns></returns>
        public static bool DeleteProductionPackageByPkgGroup(string packageGroup, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @"delete from t_mx_ppkg where packagegroup = ?P_PACKAGEGROUP; ";

            var myParam = new List<MySqlParameter>
            {
                new MySqlParameter("P_PACKAGEGROUP", packageGroup)
            };

            var blDel = MySqlDBManager.ExecuteQueryWithTrans(strSql, myParam.ToArray(), CommandType.Text, myTrans, myConnection);

            return blDel != null;
        }

        #region Oracle
        /// <summary>
        /// Insert production packgage and package group mapping
        /// </summary>
        /// <param name="ppkg"></param>
        /// <param name="oraTrans"></param>
        /// <param name="oraConn"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertPPPackageGroupsOracle(Ppkg ppkg, OracleTransaction oraTrans, OracleConnection oraConn)
        {
            string strSql = @" INSERT INTO T_MX_PPKG (PACKAGEGROUP, SEQNO, PPACKAGE, FACTORY, AONO, ORDQTY, PLANQTY)
                                VALUES(:P_PACKAGEGROUP, :P_SEQNO, :P_PPACKAGE, :P_FACTORY, :P_AONO, :P_ORDQTY, :P_PLANQTY) ";
            
            var param = new List<OracleParameter>
            {
                new OracleParameter("P_PACKAGEGROUP", ppkg.PackageGroup),
                new OracleParameter("P_SEQNO", ppkg.SeqNo),
                new OracleParameter("P_PPACKAGE", ppkg.PPackage),
                new OracleParameter("P_FACTORY", ppkg.Factory),
                new OracleParameter("P_AONO", ppkg.AoNo),
                new OracleParameter("P_ORDQTY", ppkg.OrdQty),
                new OracleParameter("P_PLANQTY", ppkg.PlanQty)
            };

            var blIns = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text, oraTrans, oraConn);

            return blIns != null;
        }

        /// <summary>
        /// Delete production package by package group in Oracle
        /// </summary>
        /// <param name="packageGroup"></param>
        /// <param name="oraTrans"></param>
        /// <param name="oraConn"></param>
        /// <returns></returns>
        public static bool DeleteProPackageByPkgGroupOracle(string packageGroup, OracleTransaction oraTrans, OracleConnection oraConn)
        {
            string strSql = @"delete from t_mx_ppkg where packagegroup = :P_PACKAGEGROUP ";

            var myParam = new List<OracleParameter>
            {
                new OracleParameter("P_PACKAGEGROUP", packageGroup)
            };

            var blDel = OracleDbManager.ExecuteQuery(strSql, myParam.ToArray(), CommandType.Text, oraTrans, oraConn);

            return blDel != null;
        }
        #endregion

    }
}
