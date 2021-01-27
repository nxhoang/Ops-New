using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public class VeppBus
    {
        /// <summary>
        /// Get Aomtops package and calculate remain qty
        /// </summary>
        /// <param name="factoryId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="buyer"></param>
        /// <param name="styleInfo"></param>
        /// <param name="aoNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Vepp> GetAomtopsPackagesModule(string factoryId, string startDate, string endDate, string buyer, string styleInfo, string aoNo)
        {
            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_MESFACTORY", factoryId),
                new OracleParameter("P_FACTORY", factoryId),
                new OracleParameter("P_BUYER", buyer),
                new OracleParameter("P_STARTDATE1", startDate),
                new OracleParameter("P_ENDDATE1", endDate),
                new OracleParameter("P_STARTDATE2", startDate),
                new OracleParameter("P_ENDDATE2", endDate)
            };

            string aoCon = string.Empty;
            string styleInfoCon = string.Empty;
            if (!string.IsNullOrEmpty(aoNo))
            {
                aoCon = @" AND VPP.AONO LIKE UPPER('%'||:P_AONO||'%') ";
                oraParams.Add(new OracleParameter("P_AONO", aoNo));
            }

            if (!string.IsNullOrEmpty(styleInfo))
            {
                styleInfoCon = @" AND (STM.STYLECODE LIKE UPPER('%'||:P_STYLEINF||'%') 
                                        OR UPPER(STM.STYLENAME) LIKE UPPER('%'||:P_STYLEINF||'%') 
                                        OR UPPER(STM.BUYERSTYLECODE) LIKE UPPER('%'||:P_STYLEINF||'%') ) ";
                oraParams.Add(new OracleParameter("P_STYLEINF", styleInfo));
            }

            string strSql = $@"SELECT PKG.PACKAGEGROUP , PKG.SEQNO, T1.REMAINQTY, T1.TARGETQTY,
                                  VPP.FACTORY ,  VPP.LINENO ,  VPP.AONO,
                                  VPP.STYLECODE,  VPP.STYLESIZE ,  VPP.STYLECOLORSERIAL,  VPP.REVNO
                                  ,  VPP.PRDPKG,  VPP.PRDSDAT ,  VPP.PRDEDAT,  VPP.PLANQTY
                                  ,  VPP.STYLECODE ||  VPP.STYLESIZE ||  VPP.STYLECOLORSERIAL ||  VPP.REVNO || VPP.AONO AS STYLEINF
                                  , SCM.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS STYLECOLORWAYS
                                  , STM.STYLECODE || ' - ' || STM.STYLENAME STYLENAME      
                                FROM PKERP.VIEW_ERP_PSRSNP_PLAN VPP
                                JOIN PKERP.T_00_STMT STM ON VPP.STYLECODE =  STM.STYLECODE  
                                JOIN PKERP.T_00_SCMT SCM ON SCM.STYLECODE = STM.STYLECODE AND SCM.STYLECOLORSERIAL = VPP.STYLECOLORSERIAL
                                LEFT JOIN PKMES.T_MX_PPKG PKG ON PKG.PPACKAGE = VPP.PRDPKG
                                LEFT JOIN (
                                    -- calculate remain qty
                                    SELECT MPMT.TARGETQTY - SUM(MPDT.MXTARGET) AS REMAINQTY, MPDT.PACKAGEGROUP, MPMT.TARGETQTY
                                    FROM PKMES.T_MX_MPDT MPDT JOIN PKMES.T_MX_MPMT MPMT ON MPMT.PACKAGEGROUP = MPDT.PACKAGEGROUP    
                                    WHERE MPMT.MESFACTORY = :P_MESFACTORY    
                                    GROUP BY  MPDT.PACKAGEGROUP , MPMT.TARGETQTY
                                ) T1 ON T1.PACKAGEGROUP = PKG.PACKAGEGROUP
                            WHERE 
                                VPP.FACTORY = :P_FACTORY
                                AND STM.BUYER = :P_BUYER 
                                AND( (VPP.PRDSDAT BETWEEN :P_STARTDATE1 AND :P_ENDDATE1) OR (VPP.PRDEDAT BETWEEN :P_STARTDATE2  AND :P_ENDDATE2) )
                                 {aoCon} {styleInfoCon} 
                                ORDER BY VPP.PRDSDAT , VPP.PRDEDAT";

            var lstVepp = OracleDbManager.GetObjectsByType<Vepp>(strSql, CommandType.Text, oraParams.ToArray());

            return lstVepp;
        }

        /// <summary>
        /// Get list of production package from Production Plan.
        /// </summary>
        /// <param name="factoryId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="buyer"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Vepp> GetProductionPackage(string factoryId, string startDate, string endDate, string buyer, string styleInfo, string aoNo)
        {
            string strSql = @"SELECT 
                                  VPP.FACTORY ,  VPP.LINENO ,  VPP.AONO,
                                  VPP.STYLECODE,  VPP.STYLESIZE ,  VPP.STYLECOLORSERIAL,  VPP.REVNO
                                  ,  VPP.PRDPKG,  VPP.PRDSDAT ,  VPP.PRDEDAT,  VPP.PLANQTY
                                  ,  VPP.STYLECODE ||  VPP.STYLESIZE ||  VPP.STYLECOLORSERIAL ||  VPP.REVNO || VPP.AONO AS STYLEINF
                                  , SCM.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS STYLECOLORWAYS
                                  , STM.STYLECODE || ' - ' || STM.STYLENAME STYLENAME
                                FROM PKERP.VIEW_ERP_PSRSNP_PLAN VPP
                                JOIN PKERP.T_00_STMT STM ON VPP.STYLECODE =  STM.STYLECODE  
                                JOIN PKERP.T_00_SCMT SCM ON SCM.STYLECODE = STM.STYLECODE AND SCM.STYLECOLORSERIAL = VPP.STYLECOLORSERIAL
                            WHERE 
                                VPP.FACTORY = :P_FACTORY
                                AND STM.BUYER = :P_BUYER 
                                AND (VPP.PRDSDAT BETWEEN :P_STARTDATE1 AND :P_ENDDATE1)
                                AND (VPP.PRDEDAT BETWEEN :P_STARTDATE2 AND :P_ENDDATE2) ";
            
            string aoCon = @" AND VPP.AONO LIKE UPPER('%'||:P_AONO||'%') ";
            
            string styleInfoCon = @" AND (STM.STYLECODE LIKE UPPER('%'||:P_STYLECODE||'%') 
                                        OR UPPER(STM.STYLENAME) LIKE UPPER('%'||:P_STYLENAME||'%') 
                                        OR UPPER(STM.BUYERSTYLECODE) LIKE UPPER('%'||:P_BUYERSTYLECODE||'%') ) ";

            string strOrderBy = " ORDER BY VPP.PRDSDAT , VPP.PRDEDAT ";

            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_FACTORY", factoryId),
                new OracleParameter("P_BUYER", buyer),
                new OracleParameter("P_STARTDATE1", startDate),
                new OracleParameter("P_ENDDATE1", endDate),
                new OracleParameter("P_STARTDATE2", startDate),
                new OracleParameter("P_ENDDATE2", endDate)
            };

            //Check buyer condition
            if (!string.IsNullOrEmpty(aoNo))
            {
                strSql += aoCon;
                oraParams.Add(new OracleParameter("P_AONO", aoNo));
            }
            
            //Check style information condition
            if (!string.IsNullOrEmpty(styleInfo))
            {
                strSql += styleInfoCon;
                oraParams.Add(new OracleParameter("P_STYLECODE", styleInfo));
                oraParams.Add(new OracleParameter("P_STYLENAME", styleInfo));
                oraParams.Add(new OracleParameter("P_BUYERSTYLECODE", styleInfo));
            }

            strSql += strOrderBy;

            var lstVepp = OracleDbManager.GetObjects<Vepp>(strSql, CommandType.Text, oraParams.ToArray());

            return lstVepp;
        }

        /// <summary>
        /// Get production package from AOMTOP with buyer condition
        /// </summary>
        /// <param name="factoryId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="buyer"></param>
        /// <param name="styleInfo"></param>
        /// <param name="aoNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Vepp> GetProductionPackageMtop(string factoryId, string startDate, string endDate, string buyer, string styleInfo, string aoNo, string searchType)
        {
            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_FACTORY", factoryId),
                new OracleParameter("P_STARTDATE1", startDate),
                new OracleParameter("P_ENDDATE1", endDate),
                new OracleParameter("P_STARTDATE2", startDate),
                new OracleParameter("P_ENDDATE2", endDate)
            };

            string whereBuyer = string.Empty;
            if (!string.IsNullOrEmpty(buyer))
            {
                whereBuyer = " AND STM.BUYER = :P_BUYER ";
                oraParams.Add(new OracleParameter("P_BUYER", buyer));
            }

            string whereStyleInf = string.Empty;
            if (!string.IsNullOrEmpty(styleInfo))
            {
                whereStyleInf = @" AND (STM.STYLECODE LIKE UPPER('%'||:P_STYLEINF||'%') 
                                        OR UPPER(STM.STYLENAME) LIKE UPPER('%'||:P_STYLEINF||'%') 
                                        OR UPPER(STM.BUYERSTYLECODE) LIKE UPPER('%'||:P_STYLEINF||'%') ) ";
                oraParams.Add(new OracleParameter("P_STYLEINF", styleInfo));
            }

            string whereAO = string.Empty;
            if (!string.IsNullOrEmpty(aoNo))
            {
                whereAO = " AND VPP.AONO LIKE UPPER('%'||:P_AONO||'%') ";
                oraParams.Add(new OracleParameter("P_AONO", aoNo));
            }

            string joinTbl = string.Empty;
            string whereSearchType = string.Empty;
            switch (searchType)
            {
                case "1":
                    //listed in qco –if selected this, check in QCO TABLE IF CURRRENT WEEK QCO HAS THIS PACKAGE
                    joinTbl = " LEFT JOIN PKMES.T_QC_QUEUE QC ON QC.PRDPKG = VPP.PRDPKG ";
                    whereSearchType = " AND QC.PRDPKG IS NOT NULL ";
                    break;
                case "2":
                    //Scheduled in MES
                    joinTbl = " LEFT JOIN PKMES.T_MX_PPKG PKG ON PKG.PPACKAGE = VPP.PRDPKG ";
                    whereSearchType = " AND PKG.PPACKAGE IS NOT NULL ";
                    break;
                case "3":
                    //Not scheduled in MES
                    joinTbl = " LEFT JOIN PKMES.T_MX_PPKG PKG ON PKG.PPACKAGE = VPP.PRDPKG ";
                    whereSearchType = " AND PKG.PPACKAGE IS NULL ";
                    break;
                case "4":
                    //Production complete (Shipped already): GD - Completion of Shipment
                    joinTbl = @" LEFT JOIN PKERP.T_AD_ADSM AD ON AD.ADNO = VPP.AONO AND AD.STYLECODE = VPP.STYLECODE  
                                                AND AD.STYLESIZE = VPP.STYLESIZE AND AD.STYLECOLORSERIAL = VPP.STYLECOLORSERIAL AND AD.REVNO = VPP.REVNO ";
                    whereSearchType = " AND AD.STATUS = 'GD' ";
                    break;
                case "5":
                    //Production not complete yet 
                    joinTbl = @" LEFT JOIN PKERP.T_AD_ADSM AD ON AD.ADNO = VPP.AONO AND AD.STYLECODE = VPP.STYLECODE  
                                                AND AD.STYLESIZE = VPP.STYLESIZE AND AD.STYLECOLORSERIAL = VPP.STYLECOLORSERIAL AND AD.REVNO = VPP.REVNO ";
                    whereSearchType = " AND AD.STATUS <> 'GD' ";
                    break;
                default:
                    //No search type condtion
                    break;
            }

            string strSql = $@"SELECT DISTINCT
                                  VPP.FACTORY ,  VPP.LINENO ,  VPP.AONO,
                                  VPP.STYLECODE,  VPP.STYLESIZE ,  VPP.STYLECOLORSERIAL,  VPP.REVNO
                                  ,  VPP.PRDPKG,  VPP.PRDSDAT ,  VPP.PRDEDAT,  VPP.PLANQTY
                                  ,  VPP.STYLECODE ||  VPP.STYLESIZE ||  VPP.STYLECOLORSERIAL ||  VPP.REVNO || VPP.AONO AS STYLEINF
                                  , SCM.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS STYLECOLORWAYS
                                  , STM.STYLECODE || ' - ' || STM.STYLENAME STYLENAME
                                FROM PKERP.VIEW_ERP_PSRSNP_PLAN VPP
                                JOIN PKERP.T_00_STMT STM ON VPP.STYLECODE =  STM.STYLECODE  
                                JOIN PKERP.T_00_SCMT SCM ON SCM.STYLECODE = STM.STYLECODE AND SCM.STYLECOLORSERIAL = VPP.STYLECOLORSERIAL
                                {joinTbl}
                            WHERE 
                                VPP.FACTORY = :P_FACTORY
                                AND (VPP.PRDEDAT BETWEEN :P_STARTDATE1 AND :P_ENDDATE1)
                                AND (VPP.PRDEDAT BETWEEN :P_STARTDATE2 AND :P_ENDDATE2) 
                                {whereBuyer} {whereAO} {whereStyleInf} {whereSearchType}
                            ORDER BY VPP.PRDSDAT , VPP.PRDEDAT";

            var lstVepp = OracleDbManager.GetObjects<Vepp>(strSql, CommandType.Text, oraParams.ToArray());

            return lstVepp;
        }

        /// <summary>
        /// Get line of factory base on the range date
        /// </summary>
        /// <param name="factoryId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static List<Vepp> GetFactoryLines(string factoryId, string startDate, string endDate, string buyer, string styleInfo, string aoNo)
        {
            string strSql = @"SELECT 
                                    DISTINCT  VPP.FACTORY ,  VPP.LINENO
                               FROM PKERP.VIEW_ERP_PSRSNP_PLAN VPP
                                    JOIN PKERP.T_00_STMT STM ON VPP.STYLECODE =  STM.STYLECODE   
                               WHERE 
                                VPP.FACTORY = :P_FACTORY
                                AND STM.BUYER = :P_BUYER 
                                AND (VPP.PRDEDAT BETWEEN :P_STARTDATE1 AND :P_ENDDATE1)
                                AND (VPP.PRDEDAT BETWEEN :P_STARTDATE2 AND :P_ENDDATE2) ";

            //string dateRangeCon = @" AND (VPP.PRDEDAT BETWEEN :P_STARTDATE1 AND :P_ENDDATE1)
            //                    AND (VPP.PRDEDAT BETWEEN :P_STARTDATE2 AND :P_ENDDATE2) ";

            string styleInfoCon = @" AND (STM.STYLECODE LIKE UPPER('%'||:P_STYLECODE||'%') 
                                        OR UPPER(STM.STYLENAME) LIKE UPPER('%'||:P_STYLENAME||'%') 
                                        OR UPPER(STM.BUYERSTYLECODE) LIKE UPPER('%'||:P_BUYERSTYLECODE||'%') ) ";

            string strOrderBy = " ORDER BY VPP.LINENO ";

            var oraParams = new List<OracleParameter>()
            {
               new OracleParameter("P_FACTORY", factoryId),
                new OracleParameter("P_BUYER", buyer),
                new OracleParameter("P_STARTDATE1", startDate),
                new OracleParameter("P_ENDDATE1", endDate),
                new OracleParameter("P_STARTDATE2", startDate),
                new OracleParameter("P_ENDDATE2", endDate)
            };

            ////Check date range condition
            //if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            //{
            //    strSql += dateRangeCon;
            //    oraParams.Add(new OracleParameter("P_STARTDATE1", startDate));
            //    oraParams.Add(new OracleParameter("P_ENDDATE1", endDate));
            //    oraParams.Add(new OracleParameter("P_STARTDATE2", startDate));
            //    oraParams.Add(new OracleParameter("P_ENDDATE2", endDate));
            //}

            //Check style information condition
            if (!string.IsNullOrEmpty(styleInfo))
            {
                strSql += styleInfoCon;
                oraParams.Add(new OracleParameter("P_STYLECODE", styleInfo));
                oraParams.Add(new OracleParameter("P_STYLENAME", styleInfo));
                oraParams.Add(new OracleParameter("P_BUYERSTYLECODE", styleInfo));
            }

            ////Check buyer condition
            //if (!string.IsNullOrEmpty(buyer))
            //{
            //    strSql += " AND STM.BUYER = :P_BUYER ";
            //    oraParams.Add(new OracleParameter("P_BUYER", buyer));
            //}

            strSql += strOrderBy;

            var lstVepp = OracleDbManager.GetObjects<Vepp>(strSql, CommandType.Text, oraParams.ToArray());

            return lstVepp;
        }

        /// <summary>
        /// Get line from AOMTOPS plan
        /// </summary>
        /// <param name="factoryId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="buyer"></param>
        /// <param name="styleInfo"></param>
        /// <param name="aoNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Vepp> GetFactoryLinesMtop(string factoryId, string startDate, string endDate, string buyer, string styleInfo, string aoNo)
        {
            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_FACTORY", factoryId),
                new OracleParameter("P_STARTDATE1", startDate),
                new OracleParameter("P_ENDDATE1", endDate),
                new OracleParameter("P_STARTDATE2", startDate),
                new OracleParameter("P_ENDDATE2", endDate)
            };

            string whereBuyer = string.Empty;
            if (!string.IsNullOrEmpty(buyer))
            {
                whereBuyer = " AND STM.BUYER = :P_BUYER ";
                oraParams.Add(new OracleParameter("P_BUYER", buyer));
            }

            string whereStyleInf = string.Empty;
            if (!string.IsNullOrEmpty(styleInfo))
            {
                whereStyleInf = @" AND (STM.STYLECODE LIKE UPPER('%'||:P_STYLEINF||'%') 
                                        OR UPPER(STM.STYLENAME) LIKE UPPER('%'||:P_STYLEINF||'%') 
                                        OR UPPER(STM.BUYERSTYLECODE) LIKE UPPER('%'||:P_STYLEINF||'%') ) ";
                oraParams.Add(new OracleParameter("P_STYLEINF", styleInfo));
            }

            string strSql = $@"SELECT 
                                    DISTINCT  VPP.FACTORY ,  VPP.LINENO
                               FROM PKERP.VIEW_ERP_PSRSNP_PLAN VPP
                                    JOIN PKERP.T_00_STMT STM ON VPP.STYLECODE =  STM.STYLECODE   
                               WHERE 
                                VPP.FACTORY = :P_FACTORY                                 
                                AND (VPP.PRDEDAT BETWEEN :P_STARTDATE1 AND :P_ENDDATE1)
                                AND (VPP.PRDEDAT BETWEEN :P_STARTDATE2 AND :P_ENDDATE2) 
                                {whereBuyer} {whereStyleInf}
                                ORDER BY VPP.LINENO";

            var lstVepp = OracleDbManager.GetObjects<Vepp>(strSql, CommandType.Text, oraParams.ToArray());

            return lstVepp;
        }
    }
}
