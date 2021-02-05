using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OPS_DAL.Business
{
    public class OpmtBus
    {
        #region General

        private readonly OpdtBus _OpdtBus = new OpdtBus();
        private readonly OpntBus _OpntBus = new OpntBus();
        private readonly MySqlDBManager _MySqlDBManager = new MySqlDBManager();

        public static Opmt GetBySourceDb(Opmt opmt, int sourceDb)
        {
            switch (sourceDb)
            {
                case 1:
                    return GetOpmt(opmt);
                case 2:
                    return GetOpsMasterByCode(opmt).FirstOrDefault();
                default:
                    return null;
            }
        }

        #endregion

        #region Oracle Database

        public static List<Opmt> GetRecentOperationPlan(int pageIndex, int pageSize, string buyer, string styleInf, decimal recentDay)
        {
            string recentDayCon = string.Empty;
            string styleInfCon = string.Empty;
            string buyerCon = string.Empty;

            var oraParam = new List<OracleParameter>();

            if (!string.IsNullOrEmpty(buyer))
            {
                buyerCon = @" and stm.buyer = :p_buyer ";
                oraParam.Add(new OracleParameter("p_buyer", buyer));
            }

            if (!string.IsNullOrEmpty(styleInf))
            {
                styleInfCon = @" and (
                                    stm.stylecode like upper('%'||:p_styleInf||'%')
                                    or upper(stm.stylename) like upper('%'||:p_styleInf||'%')
                                    or upper(stm.buyerstylecode) like upper('%'||:p_styleInf||'%')
                                    or upper(stm.buyerstylename) like upper('%'||:p_styleInf||'%')
                                ) ";

                oraParam.Add(new OracleParameter("p_styleInf", styleInf));
            }

            if (recentDay > 0)
            {
                recentDayCon = $" and opm.last_updated_time > (sysdate - {recentDay}) ";
            }

            string strSql = $@" select * from (
                                    select rownum r__, t1.* from (
                                            select t3.*,  COUNT(*) OVER() TotalRecords  from (
                                                    select 'P' edition, 'PDM' edition2, opm.last_updated_time as LastUpdateTime, opm.stylecode, opm.stylesize, opm.stylecolorserial, OPM.REVNO, opm.oprevno, opm.optime, opm.machinecount
                                                                , OPM.CONFIRMCHK, opm.opcount, OPM.MANCOUNT, opm.registerid, opm.registrydate
                                                                , stm.buyer, stm.stylename, stm.buyerstylecode, stm.buyerstylename
                                                                , scm.stylecolorserial || ' - ' || SCM.STYLECOLORWAYS as stylecolorways
                                                                , USM.USERID || ' - ' || usm.name as RegisterName
                                                                , '' as factoryname
                                                        from t_sd_opmt opm
                                                            join t_00_stmt stm on stm.stylecode = opm.stylecode
                                                            join t_00_scmt scm on scm.stylecode = opm.stylecode and scm.stylecolorserial = opm.stylecolorserial
                                                            left join t_cm_usmt usm on USM.USERID = opm.registerid
                                                        where opm.last_updated_time is not null 
                                                            {buyerCon}
                                                            {styleInfCon}
                                                            {recentDayCon}
                                                       union all
                                                        select 'A' edition, 'AOMTOPS' edition2, opm.last_updated_time as LastUpdateTime, opm.stylecode, opm.stylesize, opm.stylecolorserial, OPM.REVNO, opm.oprevno, opm.optime, opm.machinecount
                                                                , OPM.CONFIRMCHK, opm.opcount, OPM.MANCOUNT, opm.registerid, opm.registrydate
                                                                , stm.buyer, stm.stylename, stm.buyerstylecode, stm.buyerstylename
                                                                , scm.stylecolorserial || ' - ' || SCM.STYLECOLORWAYS as stylecolorways
                                                                , USM.USERID || ' - ' || usm.name as RegisterName
                                                                , fac.name as factoryname
                                                        from t_mt_opmt opm
                                                            join t_00_stmt stm on stm.stylecode = opm.stylecode
                                                            join t_00_scmt scm on scm.stylecode = opm.stylecode and scm.stylecolorserial = opm.stylecolorserial
                                                            left join t_cm_usmt usm on USM.USERID = opm.registerid
                                                            LEFT JOIN t_cm_fcmt fac ON opm.factory = fac.factory
                                                        where opm.last_updated_time is not null 
                                                            {buyerCon}
                                                            {styleInfCon}
                                                            {recentDayCon}
                                                        union all
                                                        select 'M' edition, 'MES' edition2, opm.last_updated_time as LastUpdateTime, opm.stylecode, opm.stylesize, opm.stylecolorserial, OPM.REVNO, opm.oprevno, opm.optime, opm.machinecount
                                                                , OPM.CONFIRMCHK, opm.opcount, OPM.MANCOUNT, opm.registerid, opm.registrydate
                                                                , stm.buyer, stm.stylename, stm.buyerstylecode, stm.buyerstylename
                                                                , scm.stylecolorserial || ' - ' || SCM.STYLECOLORWAYS as stylecolorways
                                                                , USM.USERID || ' - ' || usm.name as RegisterName
                                                                , fac.name as factoryname
                                                        from pkmes.t_mx_opmt opm
                                                            join t_00_stmt stm on stm.stylecode = opm.stylecode
                                                            join t_00_scmt scm on scm.stylecode = opm.stylecode and scm.stylecolorserial = opm.stylecolorserial
                                                            left join t_cm_usmt usm on USM.USERID = opm.registerid
                                                            LEFT JOIN t_cm_fcmt fac ON opm.factory = fac.factory
                                                        where opm.last_updated_time is not null 
                                                            {buyerCon}
                                                            {styleInfCon}
                                                            {recentDayCon}
                                                    )t3
                                                    order by t3.lastupdatetime desc
                                             )t1
                                             WHERE rownum < (({pageIndex} * {pageSize}) + 1 )                                        
                                   )t2
                                  WHERE r__ >= ((({pageIndex} - 1) * {pageSize}) + 1)";

            var lstOpm = OracleDbManager.GetObjects<Opmt>(strSql, oraParam.ToArray());

            return lstOpm;

        }

        /// <summary>
        /// Author: Son Nguyen Cao
        /// Date: 29 July 2017
        /// Get list operation master
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="styleRevision"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>List object Operation Master</returns>
        public static List<Opmt> GetOpMaster(string styleCode, string styleSize, string styleColorSerial, string styleRevision, int pageIndex, int pageSize)
        {

            var sb = new StringBuilder();

            //sb.AppendLine(" SELECT OPM.*, TO_CHAR(OPM.LAST_UPDATED_TIME,'YYYY/MM/DD HH24:MI:SS') LastUpdateTime FROM ");
            sb.AppendLine(" SELECT OPM.*, OPM.LAST_UPDATED_TIME LastUpdateTime FROM ");
            sb.AppendLine(" (  ");
            sb.AppendLine("     SELECT COUNT(*) OVER(ORDER BY LAST_UPDATED_TIME DESC) Records, COUNT(*) OVER() TotalRecords, ROW_NUMBER() over(order by OOP.STYLECODE) R, OOP.* ");
            sb.AppendLine("     FROM ");
            sb.AppendLine("     ( ");
            sb.AppendLine("        SELECT ");
            sb.AppendLine("              'P' AS EDITION, 'PDM' AS EDITION2, SOP.STYLECODE, SOP.STYLESIZE, SOP.STYLECOLORSERIAL, SOP.REVNO, SOP.OPREVNO, SOP.OPTIME, SOP.OPPRICE ");
            sb.AppendLine("              , SOP.MACHINECOUNT, SOP. OPCOUNT, SOP.MANCOUNT, SOP.LAST_UPDATED_TIME, SOP.REMARKS, SOP.LANGUAGE, SOP.CONFIRMCHK ");
            sb.AppendLine("              , SOP.PROCESSWIDTH, SOP.PROCESSHEIGHT, SOP.GROUPMODE, SOP.CANVASHEIGHT, SOP.LAYOUTFONTSIZE ");
            sb.AppendLine("              , SOP.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS AS STYLECOLORWAYS");
            sb.AppendLine("              , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP, '' FACTORY ");
            sb.AppendLine("         FROM ");
            sb.AppendLine("              T_SD_OPMT SOP ");
            sb.AppendLine("              LEFT JOIN T_00_SCMT SCM ON (SOP.STYLECODE = SCM.STYLECODE AND SOP.STYLECOLORSERIAL = SCM.STYLECOLORSERIAL) ");
            sb.AppendLine("              LEFT JOIN T_00_STMT STM ON SOP.STYLECODE = STM.STYLECODE ");
            sb.AppendLine("         WHERE ");
            sb.AppendLine("              SOP.STYLECODE = :STYLECODE AND SOP.STYLESIZE = :STYLESIZE ");
            sb.AppendLine("              AND (SOP.STYLECOLORSERIAL = :STYLECOLORSERIAL OR SOP.STYLECOLORSERIAL = '000') AND SOP.REVNO = :REVNO ");
            sb.AppendLine("         UNION ");
            sb.AppendLine("         SELECT ");
            sb.AppendLine("              'O' AS EDITION, 'OPS' AS EDITION2, OOP.STYLECODE, OOP.STYLESIZE, OOP.STYLECOLORSERIAL, OOP.REVNO, OOP.OPREVNO, OOP.OPTIME, OOP.OPPRICE ");
            sb.AppendLine("              , OOP.MACHINECOUNT, OOP.OPCOUNT, OOP.MANCOUNT, OOP.LAST_UPDATED_TIME, OOP.REMARKS, OOP.LANGUAGE, OOP.CONFIRMCHK ");
            sb.AppendLine("              , OOP.PROCESSWIDTH, OOP.PROCESSHEIGHT, OOP.GROUPMODE, OOP.CANVASHEIGHT, OOP.LAYOUTFONTSIZE ");
            sb.AppendLine("              , OOP.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS AS STYLECOLORWAYS ");
            sb.AppendLine("              , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP, '' FACTORY ");
            sb.AppendLine("         FROM ");
            sb.AppendLine("              T_OP_OPMT OOP ");
            sb.AppendLine("              LEFT JOIN T_00_SCMT SCM ON (OOP.STYLECODE = SCM.STYLECODE AND OOP.STYLECOLORSERIAL = SCM.STYLECOLORSERIAL) ");
            sb.AppendLine("              LEFT JOIN T_00_STMT STM ON OOP.STYLECODE = STM.STYLECODE ");
            sb.AppendLine("         WHERE ");
            sb.AppendLine("             OOP.STYLECODE = :STYLECODE AND OOP.STYLESIZE = :STYLESIZE ");
            sb.AppendLine("             AND (OOP.STYLECOLORSERIAL = :STYLECOLORSERIAL OR OOP.STYLECOLORSERIAL = '000') AND OOP.REVNO = :REVNO ");
            sb.AppendLine("         UNION ");
            sb.AppendLine("         SELECT ");
            sb.AppendLine("              'A' AS EDITION, 'AOM' AS EDITION2, AOM.STYLECODE, AOM.STYLESIZE, AOM.STYLECOLORSERIAL, AOM.REVNO, AOM.OPREVNO, AOM.OPTIME, AOM.OPPRICE ");
            sb.AppendLine("              , AOM.MACHINECOUNT, AOM.OPCOUNT, AOM.MANCOUNT, AOM.LAST_UPDATED_TIME, AOM.REMARKS, AOM.LANGUAGE, AOM.CONFIRMCHK ");
            sb.AppendLine("              , AOM.PROCESSWIDTH, AOM.PROCESSHEIGHT, AOM.GROUPMODE, AOM.CANVASHEIGHT, AOM.LAYOUTFONTSIZE ");
            sb.AppendLine("              , AOM.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS AS STYLECOLORWAYS ");
            sb.AppendLine("              , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP, AOM.FACTORY");
            sb.AppendLine("         FROM ");
            sb.AppendLine("              T_MT_OPMT AOM ");
            sb.AppendLine("              LEFT JOIN T_00_SCMT SCM ON (AOM.STYLECODE = SCM.STYLECODE AND AOM.STYLECOLORSERIAL = SCM.STYLECOLORSERIAL) ");
            sb.AppendLine("              LEFT JOIN T_00_STMT STM ON AOM.STYLECODE = STM.STYLECODE ");
            sb.AppendLine("         WHERE ");
            sb.AppendLine("             AOM.STYLECODE = :STYLECODE AND AOM.STYLESIZE = :STYLESIZE ");
            sb.AppendLine("             AND (AOM.STYLECOLORSERIAL = :STYLECOLORSERIAL OR AOM.STYLECOLORSERIAL = '000') AND AOM.REVNO = :REVNO ");
            sb.AppendLine("         UNION ");
            sb.AppendLine("         SELECT ");
            sb.AppendLine("              'M' AS EDITION, 'MES' AS EDITION2, MES.STYLECODE, MES.STYLESIZE, MES.STYLECOLORSERIAL, MES.REVNO, MES.OPREVNO, MES.OPTIME, MES.OPPRICE ");
            sb.AppendLine("              , MES.MACHINECOUNT, MES.OPCOUNT, MES.MANCOUNT, MES.LAST_UPDATED_TIME, MES.REMARKS, MES.LANGUAGE, MES.CONFIRMCHK ");
            sb.AppendLine("              , MES.PROCESSWIDTH, MES.PROCESSHEIGHT, '' AS GROUPMODE, 0 AS CANVASHEIGHT, 0 AS LAYOUTFONTSIZE ");
            sb.AppendLine("              , MES.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS AS STYLECOLORWAYS ");
            sb.AppendLine("              , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP, '' FACTORY ");
            sb.AppendLine("         FROM ");
            sb.AppendLine("              T_MX_OPMT MES ");
            sb.AppendLine("              LEFT JOIN T_00_SCMT SCM ON (MES.STYLECODE = SCM.STYLECODE AND MES.STYLECOLORSERIAL = SCM.STYLECOLORSERIAL) ");
            sb.AppendLine("              LEFT JOIN T_00_STMT STM ON MES.STYLECODE = STM.STYLECODE ");
            sb.AppendLine("         WHERE ");
            sb.AppendLine("             MES.STYLECODE = :STYLECODE AND MES.STYLESIZE = :STYLESIZE ");
            sb.AppendLine("             AND (MES.STYLECOLORSERIAL = :STYLECOLORSERIAL OR MES.STYLECOLORSERIAL = '000') AND MES.REVNO = :REVNO ");
            sb.AppendLine("     ) OOP ");
            sb.AppendLine(" ) OPM ");
            //sb.AppendLine(" WHERE R  BETWEEN ((:PAGEINDEX * :PAGESIZE) + 1) AND ((:PAGEINDEX - 1) * :PAGESIZE + 1)   ");
            sb.AppendLine(" WHERE R  BETWEEN " + ((pageIndex - 1) * pageSize + 1) + " AND " + (pageIndex * pageSize + 1));
            sb.AppendLine(" ORDER BY R ");

            OracleParameter[] prams = new OracleParameter[4];
            prams[0] = new OracleParameter("STYLECODE", styleCode);
            prams[1] = new OracleParameter("STYLESIZE", styleSize);
            prams[2] = new OracleParameter("STYLECOLORSERIAL", styleColorSerial);
            prams[3] = new OracleParameter("REVNO", styleRevision);
            //prams[4] = new OracleParameter("PAGEINDEX", pageIndex);
            //prams[5] = new OracleParameter("PAGESIZE", pageSize);

            var lstOpm = OracleDbManager.GetObjects<Opmt>(sb.ToString(), prams);

            return lstOpm;

        }

        public static List<Opmt> GetOpMaster2(string styleCode, string styleSize, string styleColorSerial, string styleRevision)
        {
            var sb = new StringBuilder();

            sb.AppendLine("         SELECT ");
            sb.AppendLine("              3 AS SORTING, 'P' AS EDITION, 'PDM' AS EDITION2, SOP.STYLECODE, SOP.STYLESIZE, SOP.STYLECOLORSERIAL, SOP.REVNO, SOP.OPREVNO, SOP.OPTIME, SOP.OPPRICE ");
            sb.AppendLine("              , SOP.MACHINECOUNT, SOP. OPCOUNT, SOP.MANCOUNT, SOP.LAST_UPDATED_TIME AS LastUpdateTime, SOP.REMARKS, SOP.LANGUAGE, SOP.CONFIRMCHK ");
            sb.AppendLine("              , SOP.PROCESSWIDTH, SOP.PROCESSHEIGHT, SOP.GROUPMODE, SOP.CANVASHEIGHT, SOP.LAYOUTFONTSIZE ");
            sb.AppendLine("              , SOP.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS AS STYLECOLORWAYS");
            sb.AppendLine("              , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP, '' FACTORY ");
            sb.AppendLine("              , SOP.REGISTERID, SOP.CONFIRMEDID, '' AS MXPACKAGE ");
            sb.AppendLine("              , sum(opd.optime) As TotalOpTime ");
            sb.AppendLine("              , mcm.code_name as reasonname, sop.opsource "); //ADD - SON) 30/Jan/2021
            sb.AppendLine("         FROM ");
            sb.AppendLine("              T_SD_OPMT SOP ");
            sb.AppendLine("              LEFT JOIN T_00_SCMT SCM ON (SOP.STYLECODE = SCM.STYLECODE AND SOP.STYLECOLORSERIAL = SCM.STYLECOLORSERIAL) ");
            sb.AppendLine("              LEFT JOIN T_00_STMT STM ON SOP.STYLECODE = STM.STYLECODE ");
            sb.AppendLine(@"             left join t_sd_opdt opd on opd.stylecode = SOP.STYLECODE and OPD.STYLESIZE = sop.stylesize 
                                                and opd.stylecolorserial = sop.stylecolorserial
                                                and opd.revno = sop.revno and OPD.OPREVNO = SOP.OPREVNO ");
            sb.AppendLine("             left join t_cm_mcmt mcm on mcm.s_code = sop.reason and m_code = 'OPReason' "); //ADD - SON) 30/Jan/2021
            sb.AppendLine("         WHERE ");
            sb.AppendLine("              SOP.STYLECODE = :STYLECODE AND SOP.STYLESIZE = :STYLESIZE ");
            sb.AppendLine("              AND (SOP.STYLECOLORSERIAL = :STYLECOLORSERIAL OR SOP.STYLECOLORSERIAL = '000') AND SOP.REVNO = :REVNO ");
            sb.AppendLine(@"        group by SOP.STYLECODE, SOP.STYLESIZE, SOP.STYLECOLORSERIAL, SOP.REVNO, SOP.OPREVNO, SOP.OPTIME, SOP.OPPRICE 
                                          , SOP.MACHINECOUNT, SOP. OPCOUNT, SOP.MANCOUNT, SOP.LAST_UPDATED_TIME, SOP.REMARKS, SOP.LANGUAGE, SOP.CONFIRMCHK 
                                          , SOP.PROCESSWIDTH, SOP.PROCESSHEIGHT, SOP.GROUPMODE, SOP.CANVASHEIGHT, SOP.LAYOUTFONTSIZE 
                                          , SOP.STYLECOLORSERIAL , SCM.STYLECOLORWAYS
                                          , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP
                                          , SOP.REGISTERID, SOP.CONFIRMEDID, mcm.code_name, sop.opsource ");
            sb.AppendLine("         UNION ");
            sb.AppendLine("         SELECT ");
            sb.AppendLine("              4 AS SORTING, 'O' AS EDITION, 'OPS' AS EDITION2, OOP.STYLECODE, OOP.STYLESIZE, OOP.STYLECOLORSERIAL, OOP.REVNO, OOP.OPREVNO, OOP.OPTIME, OOP.OPPRICE ");
            sb.AppendLine("              , OOP.MACHINECOUNT, OOP.OPCOUNT, OOP.MANCOUNT, OOP.LAST_UPDATED_TIME AS LastUpdateTime, OOP.REMARKS, OOP.LANGUAGE, OOP.CONFIRMCHK ");
            sb.AppendLine("              , OOP.PROCESSWIDTH, OOP.PROCESSHEIGHT, OOP.GROUPMODE, OOP.CANVASHEIGHT, OOP.LAYOUTFONTSIZE ");
            sb.AppendLine("              , OOP.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS AS STYLECOLORWAYS ");
            sb.AppendLine("              , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP, '' FACTORY ");
            sb.AppendLine("              , OOP.REGISTERID, OOP.CONFIRMEDID, '' AS MXPACKAGE ");
            sb.AppendLine("              , sum(opd.optime) As TotalOpTime ");
            sb.AppendLine("              , mcm.code_name as reasonname, OOP.opsource "); //ADD - SON) 30/Jan/2021
            sb.AppendLine("         FROM ");
            sb.AppendLine("              T_OP_OPMT OOP ");
            sb.AppendLine("              LEFT JOIN T_00_SCMT SCM ON (OOP.STYLECODE = SCM.STYLECODE AND OOP.STYLECOLORSERIAL = SCM.STYLECOLORSERIAL) ");
            sb.AppendLine("              LEFT JOIN T_00_STMT STM ON OOP.STYLECODE = STM.STYLECODE ");
            sb.AppendLine(@"             left join t_op_opdt opd on opd.stylecode = OOP.STYLECODE and OPD.STYLESIZE = OOP.stylesize 
                                                and opd.stylecolorserial = OOP.stylecolorserial  
                                                and opd.revno = OOP.revno and OPD.OPREVNO = OOP.OPREVNO ");
            sb.AppendLine("             left join t_cm_mcmt mcm on mcm.s_code = oop.reason and m_code = 'OPReason' "); //ADD - SON) 30/Jan/2021
            sb.AppendLine("         WHERE ");
            sb.AppendLine("             OOP.STYLECODE = :STYLECODE AND OOP.STYLESIZE = :STYLESIZE ");
            sb.AppendLine("             AND (OOP.STYLECOLORSERIAL = :STYLECOLORSERIAL OR OOP.STYLECOLORSERIAL = '000') AND OOP.REVNO = :REVNO ");
            sb.AppendLine(@"        group by OOP.STYLECODE, OOP.STYLESIZE, OOP.STYLECOLORSERIAL, OOP.REVNO, OOP.OPREVNO, OOP.OPTIME, OOP.OPPRICE 
                                              , OOP.MACHINECOUNT, OOP. OPCOUNT, OOP.MANCOUNT, OOP.LAST_UPDATED_TIME, OOP.REMARKS, OOP.LANGUAGE, OOP.CONFIRMCHK 
                                              , OOP.PROCESSWIDTH, OOP.PROCESSHEIGHT, OOP.GROUPMODE, OOP.CANVASHEIGHT, OOP.LAYOUTFONTSIZE 
                                              , OOP.STYLECOLORSERIAL , SCM.STYLECOLORWAYS
                                              , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP
                                              , OOP.REGISTERID, OOP.CONFIRMEDID, mcm.code_name, OOP.opsource  ");
            sb.AppendLine("         UNION ");
            sb.AppendLine("         SELECT ");
            sb.AppendLine("              2 AS SORTING, 'A' AS EDITION, 'AOMTOPS' AS EDITION2, AOM.STYLECODE, AOM.STYLESIZE, AOM.STYLECOLORSERIAL, AOM.REVNO, AOM.OPREVNO, AOM.OPTIME, AOM.OPPRICE ");
            sb.AppendLine("              , AOM.MACHINECOUNT, AOM.OPCOUNT, AOM.MANCOUNT, AOM.LAST_UPDATED_TIME AS LastUpdateTime, AOM.REMARKS, AOM.LANGUAGE, AOM.CONFIRMCHK ");
            sb.AppendLine("              , AOM.PROCESSWIDTH, AOM.PROCESSHEIGHT, AOM.GROUPMODE, AOM.CANVASHEIGHT, AOM.LAYOUTFONTSIZE ");
            sb.AppendLine("              , AOM.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS AS STYLECOLORWAYS ");
            sb.AppendLine("              , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP, f.name as factory");
            sb.AppendLine("              , AOM.REGISTERID, AOM.CONFIRMEDID, '' AS MXPACKAGE");
            sb.AppendLine("              , sum(opd.optime) As TotalOpTime ");
            sb.AppendLine("              , mcm.code_name as reasonname, aom.opsource"); //ADD - SON) 30/Jan/2021
            sb.AppendLine("         FROM ");
            sb.AppendLine("              T_MT_OPMT AOM ");
            sb.AppendLine("              LEFT JOIN T_00_SCMT SCM ON (AOM.STYLECODE = SCM.STYLECODE AND AOM.STYLECOLORSERIAL = SCM.STYLECOLORSERIAL) ");
            sb.AppendLine("              LEFT JOIN T_00_STMT STM ON AOM.STYLECODE = STM.STYLECODE LEFT JOIN t_cm_fcmt f ON aom.factory = f.factory");
            sb.AppendLine(@"             left join t_mt_opdt opd on opd.stylecode = AOM.STYLECODE and OPD.STYLESIZE = AOM.stylesize 
                                                and opd.stylecolorserial = AOM.stylecolorserial  
                                                and opd.revno = AOM.revno and OPD.OPREVNO = AOM.OPREVNO  ");
            sb.AppendLine("             left join t_cm_mcmt mcm on mcm.s_code = aom.reason and m_code = 'OPReason' "); //ADD - SON) 30/Jan/2021
            sb.AppendLine("         WHERE ");
            sb.AppendLine("             AOM.STYLECODE = :STYLECODE AND AOM.STYLESIZE = :STYLESIZE ");
            sb.AppendLine("             AND (AOM.STYLECOLORSERIAL = :STYLECOLORSERIAL OR AOM.STYLECOLORSERIAL = '000') AND AOM.REVNO = :REVNO ");
            sb.AppendLine(@"         group by AOM.STYLECODE, AOM.STYLESIZE, AOM.STYLECOLORSERIAL, AOM.REVNO, AOM.OPREVNO, AOM.OPTIME, AOM.OPPRICE 
                                          , AOM.MACHINECOUNT, AOM. OPCOUNT, AOM.MANCOUNT, AOM.LAST_UPDATED_TIME, AOM.REMARKS, AOM.LANGUAGE, AOM.CONFIRMCHK 
                                          , AOM.PROCESSWIDTH, AOM.PROCESSHEIGHT, AOM.GROUPMODE, AOM.CANVASHEIGHT, AOM.LAYOUTFONTSIZE 
                                          , AOM.STYLECOLORSERIAL , SCM.STYLECOLORWAYS
                                          , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP,  f.name
                                          , AOM.REGISTERID, AOM.CONFIRMEDID, mcm.code_name, AOM.opsource ");
            sb.AppendLine("         UNION ");
            sb.AppendLine("         SELECT ");
            sb.AppendLine("              1 AS SORTING, 'M' AS EDITION, 'MES' AS EDITION2, MES.STYLECODE, MES.STYLESIZE, MES.STYLECOLORSERIAL, MES.REVNO, MES.OPREVNO, MES.OPTIME, MES.OPPRICE ");
            sb.AppendLine("              , MES.MACHINECOUNT, MES.OPCOUNT, MES.MANCOUNT, MES.LAST_UPDATED_TIME AS LastUpdateTime, MES.REMARKS, MES.LANGUAGE, MES.CONFIRMCHK ");
            sb.AppendLine("              , MES.PROCESSWIDTH, MES.PROCESSHEIGHT, '' AS GROUPMODE, 0 AS CANVASHEIGHT, 0 AS LAYOUTFONTSIZE ");
            sb.AppendLine("              , MES.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS AS STYLECOLORWAYS ");
            sb.AppendLine("              , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP, f.name as factory");
            sb.AppendLine("              , '' AS REGISTERID, '' AS CONFIRMEDID, MES.MXPACKAGE ");
            sb.AppendLine("              , sum(opd.optime) As TotalOpTime ");
            sb.AppendLine("              , mcm.code_name as reasonname, mes.opsource "); //ADD - SON) 30/Jan/2021
            sb.AppendLine("         FROM ");
            sb.AppendLine("              PKMES.T_MX_OPMT MES "); //MOD) SON - 04/Jun/2019 - Get MES edition from MES schema
            sb.AppendLine("              LEFT JOIN T_00_SCMT SCM ON (MES.STYLECODE = SCM.STYLECODE AND MES.STYLECOLORSERIAL = SCM.STYLECOLORSERIAL) ");
            sb.AppendLine("              LEFT JOIN T_00_STMT STM ON MES.STYLECODE = STM.STYLECODE LEFT JOIN t_cm_fcmt f ON mes.factory = f.factory");
            sb.AppendLine(@"             left join PKMES.t_MX_opdt opd on opd.stylecode = MES.STYLECODE and OPD.STYLESIZE = MES.stylesize 
                                                and opd.stylecolorserial = MES.stylecolorserial  
                                                and opd.revno = MES.revno and OPD.OPREVNO = MES.OPREVNO  ");
            sb.AppendLine("             left join t_cm_mcmt mcm on mcm.s_code = MES.reason and m_code = 'OPReason' "); //ADD - SON) 30/Jan/2021
            sb.AppendLine("         WHERE ");
            sb.AppendLine("             MES.STYLECODE = :STYLECODE AND MES.STYLESIZE = :STYLESIZE ");
            sb.AppendLine("             AND (MES.STYLECOLORSERIAL = :STYLECOLORSERIAL OR MES.STYLECOLORSERIAL = '000') AND MES.REVNO = :REVNO ");
            sb.AppendLine(@"        group by MES.STYLECODE, MES.STYLESIZE, MES.STYLECOLORSERIAL, MES.REVNO, MES.OPREVNO, MES.OPTIME, MES.OPPRICE 
                                          , MES.MACHINECOUNT, MES. OPCOUNT, MES.MANCOUNT, MES.LAST_UPDATED_TIME, MES.REMARKS, MES.LANGUAGE, MES.CONFIRMCHK 
                                          , MES.PROCESSWIDTH, MES.PROCESSHEIGHT, MES.GROUPMODE, MES.CANVASHEIGHT, MES.LAYOUTFONTSIZE 
                                          , MES.STYLECOLORSERIAL , SCM.STYLECOLORWAYS
                                          , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP, f.name
                                          , MES.MXPACKAGE, mcm.code_name, mes.opsource ");
            sb.AppendLine("         ORDER BY LastUpdateTime DESC");

            OracleParameter[] prams = new OracleParameter[4];
            prams[0] = new OracleParameter("STYLECODE", styleCode);
            prams[1] = new OracleParameter("STYLESIZE", styleSize);
            prams[2] = new OracleParameter("STYLECOLORSERIAL", styleColorSerial);
            prams[3] = new OracleParameter("REVNO", styleRevision);

            var lstOpm = OracleDbManager.GetObjects<Opmt>(sb.ToString(), prams);

            return lstOpm;
        }

        //Author: Son Nguyen Cao
        public static List<Opmt> GetOpMasterByEdition(string styleCode, string styleSize, string styleColorSerial, string styleRevision, int pageIndex, int pageSize, string edition)
        {

            var sb = new StringBuilder();

            string strSqlPdm =
                        @"SELECT  
                               'P' AS EDITION, 'PDM' AS EDITION2, SOP.STYLECODE, SOP.STYLESIZE, SOP.STYLECOLORSERIAL, SOP.REVNO, SOP.OPREVNO, SOP.OPTIME, SOP.OPPRICE  
                               , SOP.MACHINECOUNT, SOP.OPCOUNT, SOP.MANCOUNT, SOP.LAST_UPDATED_TIME, SOP.REMARKS, SOP.LANGUAGE, SOP.CONFIRMCHK  
                               , SOP.PROCESSWIDTH, SOP.PROCESSHEIGHT, SOP.GROUPMODE, SOP.CANVASHEIGHT, SOP.LAYOUTFONTSIZE
                               , SOP.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS AS STYLECOLORWAYS
                               , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP, '' FACTORY
                               , SOP.REGISTERID, SOP.CONFIRMEDID
                          FROM  
                               T_SD_OPMT SOP  
                               LEFT JOIN T_00_SCMT SCM ON (SOP.STYLECODE = SCM.STYLECODE AND SOP.STYLECOLORSERIAL = SCM.STYLECOLORSERIAL)  
                               LEFT JOIN T_00_STMT STM ON SOP.STYLECODE = STM.STYLECODE
                          WHERE  
                               SOP.STYLECODE = :STYLECODE AND SOP.STYLESIZE = :STYLESIZE  
                               AND (SOP.STYLECOLORSERIAL = :STYLECOLORSERIAL OR SOP.STYLECOLORSERIAL = '000') AND SOP.REVNO = :REVNO ";

            string strSqlOps = @"SELECT  
                               'O' AS EDITION, 'OPS' AS EDITION2, OOP.STYLECODE, OOP.STYLESIZE, OOP.STYLECOLORSERIAL, OOP.REVNO, OOP.OPREVNO, OOP.OPTIME, OOP.OPPRICE  
                               , OOP.MACHINECOUNT, OOP.OPCOUNT, OOP.MANCOUNT, OOP.LAST_UPDATED_TIME, OOP.REMARKS, OOP.LANGUAGE, OOP.CONFIRMCHK  
                               , OOP.PROCESSWIDTH, OOP.PROCESSHEIGHT, OOP.GROUPMODE, OOP.CANVASHEIGHT, OOP.LAYOUTFONTSIZE
                               , OOP.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS AS STYLECOLORWAYS 
                               , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP, '' FACTORY
                               , OOP.REGISTERID, OOP.CONFIRMEDID 
                          FROM  
                               T_OP_OPMT OOP  
                               LEFT JOIN T_00_SCMT SCM ON (OOP.STYLECODE = SCM.STYLECODE AND OOP.STYLECOLORSERIAL = SCM.STYLECOLORSERIAL)  
                               LEFT JOIN T_00_STMT STM ON OOP.STYLECODE = STM.STYLECODE
                          WHERE  
                              OOP.STYLECODE = :STYLECODE AND OOP.STYLESIZE = :STYLESIZE  
                              AND (OOP.STYLECOLORSERIAL = :STYLECOLORSERIAL OR OOP.STYLECOLORSERIAL = '000') AND OOP.REVNO = :REVNO  ";

            string strSqlAom =
                 @"SELECT 
                      'A' AS EDITION, 'AOM' AS EDITION2, AOM.STYLECODE, AOM.STYLESIZE, AOM.STYLECOLORSERIAL, AOM.REVNO, AOM.OPREVNO, AOM.OPTIME, AOM.OPPRICE 
                      , AOM.MACHINECOUNT, AOM.OPCOUNT, AOM.MANCOUNT, AOM.LAST_UPDATED_TIME, AOM.REMARKS, AOM.LANGUAGE, AOM.CONFIRMCHK 
                      , AOM.PROCESSWIDTH, AOM.PROCESSHEIGHT, AOM.GROUPMODE, AOM.CANVASHEIGHT, AOM.LAYOUTFONTSIZE
                      , AOM.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS AS STYLECOLORWAYS 
                      , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP, AOM.FACTORY
                      , AOM.REGISTERID, AOM.CONFIRMEDID
                 FROM 
                      T_MT_OPMT AOM 
                      LEFT JOIN T_00_SCMT SCM ON (AOM.STYLECODE = SCM.STYLECODE AND AOM.STYLECOLORSERIAL = SCM.STYLECOLORSERIAL) 
                      LEFT JOIN T_00_STMT STM ON AOM.STYLECODE = STM.STYLECODE
                 WHERE 
                     AOM.STYLECODE = :STYLECODE AND AOM.STYLESIZE = :STYLESIZE 
                     AND (AOM.STYLECOLORSERIAL = :STYLECOLORSERIAL OR AOM.STYLECOLORSERIAL = '000') AND AOM.REVNO = :REVNO  ";

            string strSqlMes =
                @"SELECT 
                      'M' AS EDITION, 'MES' AS EDITION2, MES.STYLECODE, MES.STYLESIZE, MES.STYLECOLORSERIAL, MES.REVNO, MES.OPREVNO, MES.OPTIME, MES.OPPRICE 
                      , MES.MACHINECOUNT, MES.OPCOUNT, MES.MANCOUNT, MES.LAST_UPDATED_TIME, MES.REMARKS, MES.LANGUAGE, MES.CONFIRMCHK 
                      , MES.PROCESSWIDTH, MES.PROCESSHEIGHT, '' AS GROUPMODE, 0 AS CANVASHEIGHT, 0 AS LAYOUTFONTSIZE 
                      , MES.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS AS STYLECOLORWAYS 
                      , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP, '' FACTORY
                      , '' AS REGISTERID, '' AS CONFIRMEDID
                 FROM 
                      T_MX_OPMT MES 
                      LEFT JOIN T_00_SCMT SCM ON (MES.STYLECODE = SCM.STYLECODE AND MES.STYLECOLORSERIAL = SCM.STYLECOLORSERIAL) 
                      LEFT JOIN T_00_STMT STM ON MES.STYLECODE = STM.STYLECODE
                 WHERE 
                     MES.STYLECODE = :STYLECODE AND MES.STYLESIZE = :STYLESIZE 
                     AND (MES.STYLECOLORSERIAL = :STYLECOLORSERIAL OR MES.STYLECOLORSERIAL = '000') AND MES.REVNO = :REVNO ";

            //sb.AppendLine(" SELECT OPM.*, TO_CHAR(OPM.LAST_UPDATED_TIME,'YYYY/MM/DD HH24:MI:SS') LastUpdateTime FROM ");
            sb.AppendLine(" SELECT OPM.*, OPM.LAST_UPDATED_TIME LastUpdateTime FROM ");
            sb.AppendLine(" (  ");
            sb.AppendLine("     SELECT COUNT(*) OVER(ORDER BY LAST_UPDATED_TIME DESC) Records, COUNT(*) OVER() TotalRecords, ROW_NUMBER() over(order by OOP.STYLECODE) R, OOP.* ");
            sb.AppendLine("     FROM ");
            sb.AppendLine("     ( ");

            if (edition == ConstantGeneric.EditionPdm)
            {
                sb.AppendLine(strSqlPdm);
            }
            else if (edition == ConstantGeneric.EditionOps)
            {
                sb.AppendLine(strSqlOps);
            }
            else if (edition == ConstantGeneric.EditionAom)
            {
                sb.AppendLine(strSqlAom);
            }
            else if (edition == ConstantGeneric.EditionMes)
            {
                sb.AppendLine(strSqlMes);
            }
            else
            {
                sb.AppendLine(strSqlPdm);
                sb.AppendLine(" UNION ");
                sb.AppendLine(strSqlOps);
                sb.AppendLine(" UNION ");
                sb.AppendLine(strSqlAom);
                sb.AppendLine(" UNION ");
                sb.AppendLine(strSqlMes);
            }

            sb.AppendLine("     ) OOP ");
            sb.AppendLine(" ) OPM ");
            sb.AppendLine(" WHERE R  BETWEEN " + ((pageIndex - 1) * pageSize + 1) + " AND " + (pageIndex * pageSize + 1));
            sb.AppendLine(" ORDER BY R ");

            OracleParameter[] prams = new OracleParameter[4];
            prams[0] = new OracleParameter("STYLECODE", styleCode);
            prams[1] = new OracleParameter("STYLESIZE", styleSize);
            prams[2] = new OracleParameter("STYLECOLORSERIAL", styleColorSerial);
            prams[3] = new OracleParameter("REVNO", styleRevision);

            var lstOpm = OracleDbManager.GetObjects<Opmt>(sb.ToString(), prams);

            return lstOpm;

        }

        public static List<Opmt> GetOpMasterByEdition2(string styleCode, string styleSize, string styleColorSerial, string styleRevision, string edition)
        {
            var sb = new StringBuilder();

            string strSqlPdm =
                        @"SELECT  
                               'P' AS EDITION, 'PDM' AS EDITION2, SOP.STYLECODE, SOP.STYLESIZE, SOP.STYLECOLORSERIAL, SOP.REVNO, SOP.OPREVNO, SOP.OPTIME, SOP.OPPRICE  
                               , SOP.MACHINECOUNT, SOP.OPCOUNT, SOP.MANCOUNT, SOP.LAST_UPDATED_TIME LastUpdateTime, SOP.REMARKS, SOP.LANGUAGE, SOP.CONFIRMCHK  
                               , SOP.PROCESSWIDTH, SOP.PROCESSHEIGHT, SOP.GROUPMODE, SOP.CANVASHEIGHT, SOP.LAYOUTFONTSIZE
                               , SOP.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS AS STYLECOLORWAYS
                               , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP, '' FACTORY
                               , SOP.REGISTERID, SOP.CONFIRMEDID, '' AS MXPACKAGE
                          FROM  
                               T_SD_OPMT SOP  
                               LEFT JOIN T_00_SCMT SCM ON (SOP.STYLECODE = SCM.STYLECODE AND SOP.STYLECOLORSERIAL = SCM.STYLECOLORSERIAL)  
                               LEFT JOIN T_00_STMT STM ON SOP.STYLECODE = STM.STYLECODE
                          WHERE  
                               SOP.STYLECODE = :STYLECODE AND SOP.STYLESIZE = :STYLESIZE  
                               AND (SOP.STYLECOLORSERIAL = :STYLECOLORSERIAL OR SOP.STYLECOLORSERIAL = '000') AND SOP.REVNO = :REVNO 
                           ORDER BY LastUpdateTime DESC";

            string strSqlOps = @"SELECT  
                               'O' AS EDITION, 'OPS' AS EDITION2, OOP.STYLECODE, OOP.STYLESIZE, OOP.STYLECOLORSERIAL, OOP.REVNO, OOP.OPREVNO, OOP.OPTIME, OOP.OPPRICE  
                               , OOP.MACHINECOUNT, OOP.OPCOUNT, OOP.MANCOUNT, OOP.LAST_UPDATED_TIME LastUpdateTime, OOP.REMARKS, OOP.LANGUAGE, OOP.CONFIRMCHK  
                               , OOP.PROCESSWIDTH, OOP.PROCESSHEIGHT, OOP.GROUPMODE, OOP.CANVASHEIGHT, OOP.LAYOUTFONTSIZE
                               , OOP.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS AS STYLECOLORWAYS 
                               , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP, '' FACTORY
                               , OOP.REGISTERID, OOP.CONFIRMEDID, '' AS MXPACKAGE
                          FROM  
                               T_OP_OPMT OOP  
                               LEFT JOIN T_00_SCMT SCM ON (OOP.STYLECODE = SCM.STYLECODE AND OOP.STYLECOLORSERIAL = SCM.STYLECOLORSERIAL)  
                               LEFT JOIN T_00_STMT STM ON OOP.STYLECODE = STM.STYLECODE
                          WHERE  
                              OOP.STYLECODE = :STYLECODE AND OOP.STYLESIZE = :STYLESIZE  
                              AND (OOP.STYLECOLORSERIAL = :STYLECOLORSERIAL OR OOP.STYLECOLORSERIAL = '000') AND OOP.REVNO = :REVNO  
                          ORDER BY LastUpdateTime DESC";

            string strSqlAom =
                 @"SELECT 
                      'A' AS EDITION, 'AOMTOPS' AS EDITION2, AOM.STYLECODE, AOM.STYLESIZE, AOM.STYLECOLORSERIAL, AOM.REVNO, AOM.OPREVNO, AOM.OPTIME, AOM.OPPRICE 
                      , AOM.MACHINECOUNT, AOM.OPCOUNT, AOM.MANCOUNT, AOM.LAST_UPDATED_TIME LastUpdateTime, AOM.REMARKS, AOM.LANGUAGE, AOM.CONFIRMCHK 
                      , AOM.PROCESSWIDTH, AOM.PROCESSHEIGHT, AOM.GROUPMODE, AOM.CANVASHEIGHT, AOM.LAYOUTFONTSIZE
                      , AOM.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS AS STYLECOLORWAYS 
                      , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP, AOM.FACTORY
                      , AOM.REGISTERID, AOM.CONFIRMEDID, '' AS MXPACKAGE
                 FROM 
                      T_MT_OPMT AOM 
                      LEFT JOIN T_00_SCMT SCM ON (AOM.STYLECODE = SCM.STYLECODE AND AOM.STYLECOLORSERIAL = SCM.STYLECOLORSERIAL) 
                      LEFT JOIN T_00_STMT STM ON AOM.STYLECODE = STM.STYLECODE
                 WHERE 
                     AOM.STYLECODE = :STYLECODE AND AOM.STYLESIZE = :STYLESIZE 
                     AND (AOM.STYLECOLORSERIAL = :STYLECOLORSERIAL OR AOM.STYLECOLORSERIAL = '000') AND AOM.REVNO = :REVNO  
                  ORDER BY LastUpdateTime DESC";

            //MOD) SON - 04/Jun/2019 - Get MES edition from MES schema (change T_MX_OPMT into PKMES.T_MX_OPMT)
            string strSqlMes =
                @"SELECT 
                      'M' AS EDITION, 'MES' AS EDITION2, MES.STYLECODE, MES.STYLESIZE, MES.STYLECOLORSERIAL, MES.REVNO, MES.OPREVNO, MES.OPTIME, MES.OPPRICE 
                      , MES.MACHINECOUNT, MES.OPCOUNT, MES.MANCOUNT, MES.LAST_UPDATED_TIME LastUpdateTime, MES.REMARKS, MES.LANGUAGE, MES.CONFIRMCHK 
                      , MES.PROCESSWIDTH, MES.PROCESSHEIGHT, '' AS GROUPMODE, 0 AS CANVASHEIGHT, 0 AS LAYOUTFONTSIZE 
                      , MES.STYLECOLORSERIAL || ' - ' || SCM.STYLECOLORWAYS AS STYLECOLORWAYS 
                      , STM.BUYERSTYLECODE, STM.BUYERSTYLENAME, STM.BUYER, STM.STYLEGROUP, STM.SUBGROUP, STM.SUBSUBGROUP, MES.FACTORY
                      , '' AS REGISTERID, '' AS CONFIRMEDID, MES.MXPACKAGE
                FROM 
                      PKMES.T_MX_OPMT MES 
                      LEFT JOIN T_00_SCMT SCM ON (MES.STYLECODE = SCM.STYLECODE AND MES.STYLECOLORSERIAL = SCM.STYLECOLORSERIAL) 
                      LEFT JOIN T_00_STMT STM ON MES.STYLECODE = STM.STYLECODE
                 WHERE 
                     MES.STYLECODE = :STYLECODE AND MES.STYLESIZE = :STYLESIZE 
                     AND (MES.STYLECOLORSERIAL = :STYLECOLORSERIAL OR MES.STYLECOLORSERIAL = '000') AND MES.REVNO = :REVNO 
                  ORDER BY LastUpdateTime DESC";

            if (edition == ConstantGeneric.EditionPdm)
            {
                sb.AppendLine(strSqlPdm);
            }
            else if (edition == ConstantGeneric.EditionOps)
            {
                sb.AppendLine(strSqlOps);
            }
            else if (edition == ConstantGeneric.EditionAom)
            {
                sb.AppendLine(strSqlAom);
            }
            else if (edition == ConstantGeneric.EditionMes)
            {
                sb.AppendLine(strSqlMes);
            }
            else
            {
                sb.AppendLine(strSqlPdm);
                sb.AppendLine(" UNION ");
                sb.AppendLine(strSqlOps);
                sb.AppendLine(" UNION ");
                sb.AppendLine(strSqlAom);
                sb.AppendLine(" UNION ");
                sb.AppendLine(strSqlMes);
            }

            OracleParameter[] prams = new OracleParameter[4];
            prams[0] = new OracleParameter("STYLECODE", styleCode);
            prams[1] = new OracleParameter("STYLESIZE", styleSize);
            prams[2] = new OracleParameter("STYLECOLORSERIAL", styleColorSerial);
            prams[3] = new OracleParameter("REVNO", styleRevision);

            var lstOpm = OracleDbManager.GetObjects<Opmt>(sb.ToString(), prams, ConstantGeneric.ConnectionStr);

            return lstOpm;
        }

        public static List<Opmt> MesGetOpmt(string styleCode, string styleSize, string styleColorSerial, string opRevNo,
            string edition)
        {
            string query = @"SELECT 'M' AS edition, 'MES' AS edition2, mes.stylecode, mes.stylesize, mes.stylecolorserial," +
                           " mes.revno, mes.oprevno, mes.optime, mes.opprice, mes.machinecount, mes.opcount, mes.mancount," +
                           " mes.last_updated_time lastupdatetime, mes.remarks, mes.language, mes.confirmchk, " +
                           "mes.processwidth, mes.processheight, '' AS groupmode, 0 AS canvasheight, 0 AS layoutfontsize," +
                           " mes.stylecolorserial || ' - ' || scm.stylecolorways AS stylecolorways, stm.buyerstylecode, " +
                           "stm.buyerstylename, stm.buyer, stm.stylegroup, stm.subgroup, stm.subsubgroup, mes.factory," +
                           " '' AS registerid, '' AS confirmedid, mes.mxpackage " +
                           "FROM pkmes.t_mx_opmt mes LEFT JOIN t_00_scmt scm " +
                           "ON(mes.stylecode = scm.stylecode AND mes.stylecolorserial = scm.stylecolorserial) " +
                           "LEFT JOIN t_00_stmt stm ON mes.stylecode = stm.stylecode " +
                           "WHERE mes.stylecode = :p_stylecode AND mes.stylesize = :p_stylesize " +
                           "AND( mes.stylecolorserial = :p_stylecolorserial OR mes.stylecolorserial = '000') " +
                           "AND mes.revno = :p_revno ORDER BY lastupdatetime DESC";
            OracleParameter[] prams = new OracleParameter[4];
            prams[0] = new OracleParameter("p_stylecode", styleCode);
            prams[1] = new OracleParameter("p_stylesize", styleSize);
            prams[2] = new OracleParameter("p_stylecolorserial", styleColorSerial);
            prams[3] = new OracleParameter("p_revno", opRevNo);
            var opmts = OracleDbManager.GetObjects<Opmt>(query, CommandType.Text, prams, ConstantGeneric.ConnectionStr,
                ConstantGeneric.OraTimeout);

            return opmts;
        }

        /// <summary>
        /// Gets the maximum op revision.
        /// </summary>
        /// <param name="edition">The edition.</param>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColorSerial">The style color serial.</param>
        /// <param name="styleRevision">The style revision.</param>
        /// <param name="isPrivate"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static int GetMaxOpRevision(string edition, string styleCode, string styleSize, string styleColorSerial,
            string styleRevision, bool isPrivate)
        {
            var tableName = isPrivate ? CommonMethod.GetOpmtNameByEdition(edition) : CommonMethod.GetTableNameMasterByEdition(edition);
            if (string.IsNullOrEmpty(tableName)) return 0;

            var sb = new StringBuilder();
            sb.AppendLine(" SELECT MAX(OPM.OPREVNO) MaxOpRevNo ");
            sb.AppendLine(" FROM " + tableName + "  OPM ");
            sb.AppendLine(" WHERE ");
            sb.AppendLine("      OPM.STYLECODE = :STYLECODE AND OPM.STYLESIZE = :STYLESIZE ");
            sb.AppendLine("      AND (OPM.STYLECOLORSERIAL = :STYLECOLORSERIAL OR OPM.STYLECOLORSERIAL = '000') AND OPM.REVNO = :REVNO ");

            OracleParameter[] prams = new OracleParameter[4];
            prams[0] = new OracleParameter("STYLECODE", styleCode);
            prams[1] = new OracleParameter("STYLESIZE", styleSize);
            prams[2] = new OracleParameter("STYLECOLORSERIAL", styleColorSerial);
            prams[3] = new OracleParameter("REVNO", styleRevision);

            //START MOD) SON - 04/Jun/2019
            var objOpmt = OracleDbManager.GetObjects<Opmt>(sb.ToString(), CommandType.Text, prams).FirstOrDefault();

            //var objOpmt = new Opmt();
            //if(edition != ConstantGeneric.EditionMes)
            //{
            //    objOpmt = OracleDbManager.GetObjects<Opmt>(sb.ToString(), CommandType.Text, prams).FirstOrDefault();
            //}
            //else
            //{
            //    objOpmt = OracleDbManager.GetObjects<Opmt>(sb.ToString(), CommandType.Text, prams, ConstantGeneric.ConnectionStrMes).FirstOrDefault();
            //}

            //START MOD) SON - 04/Jun/2019

            if (objOpmt != null) return int.Parse(objOpmt.MaxOpRevNo ?? "0") + 1;

            return 0;
        }

        /// <summary>
        /// Adds the op master.
        /// </summary>
        /// <param name="edition">The edition add.</param>
        /// <param name="opMaster">The op master.</param>
        /// <param name="oraConn">The ora connection.</param>
        /// <param name="trans">The trans.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool AddOpMaster(string edition, Opmt opMaster, OracleConnection oraConn, OracleTransaction trans, bool isPrivate)
        {
            string tableName = "";
            if (isPrivate)
            {
                tableName = CommonMethod.GetOpmtNameByEdition(edition);
            }
            else
            {
                tableName = CommonMethod.GetTableNameMasterByEdition(edition);
            }
            var sb = new StringBuilder();
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("STYLECODE", opMaster.StyleCode),
                new OracleParameter("STYLESIZE", opMaster.StyleSize),
                new OracleParameter("STYLECOLORSERIAL", opMaster.StyleColorSerial),
                new OracleParameter("REVNO", opMaster.RevNo),
                new OracleParameter("OPREVNO", opMaster.OpRevNo),
                new OracleParameter("OPTIME", opMaster.OpTime),
                new OracleParameter("MACHINECOUNT", opMaster.MachineCount),
                new OracleParameter("OPCOUNT", opMaster.OpCount),
                new OracleParameter("MANCOUNT", opMaster.ManCount),
                new OracleParameter("LANGUAGE", opMaster.Language),
                new OracleParameter("REMARKS", opMaster.Remarks),
                new OracleParameter("GROUPMODE", opMaster.GroupMode),
                new OracleParameter("PROCESSWIDTH", opMaster.ProcessWidth),
                new OracleParameter("PROCESSHEIGHT", opMaster.ProcessHeight),
                new OracleParameter("LAYOUTFONTSIZE", opMaster.LayoutFontSize),
                new OracleParameter("CANVASHEIGHT", opMaster.CanvasHeight)
            };
            string strColumn = "STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, OPREVNO, OPTIME, MACHINECOUNT, OPCOUNT, MANCOUNT, LAST_UPDATED_TIME, LANGUAGE, REMARKS, GROUPMODE, PROCESSWIDTH, PROCESSHEIGHT, LAYOUTFONTSIZE, CANVASHEIGHT ";
            string strValue = ":STYLECODE, :STYLESIZE, :STYLECOLORSERIAL, :REVNO, :OPREVNO, :OPTIME, :MACHINECOUNT, :OPCOUNT, :MANCOUNT, SYSDATE, :LANGUAGE, :REMARKS,:GROUPMODE,:PROCESSWIDTH,:PROCESSHEIGHT,:LAYOUTFONTSIZE,:CANVASHEIGHT";
            if (edition != ConstantGeneric.EditionAom)
            {
                // If Save as factory edition, factory will not be null or empty
                if (!String.IsNullOrEmpty(opMaster.Factory))
                {
                    tableName = ConstantGeneric.TableMtOpmt;
                    strColumn = strColumn + ", FACTORY ";
                    strValue = strValue + " , :FACTORY ";
                    oracleParams.Add(new OracleParameter("FACTORY", opMaster.Factory));
                }
                else
                {
                    if (isPrivate && (edition == ConstantGeneric.EditionPdm))
                    {
                        // do no thing
                    }
                    else
                    {
                        strColumn = strColumn + ", REGISTERID, REGISTRYDATE ";
                        strValue = strValue + " , :REGISTERID, SYSDATE ";
                        oracleParams.Add(new OracleParameter("REGISTERID", opMaster.RegisterId));
                    }

                }
            }
            sb.AppendLine(" INSERT INTO ");
            sb.AppendLine("      " + tableName + "");
            sb.AppendLine(" (" + strColumn + " ) ");
            sb.AppendLine(" VALUES ( " + strValue + " ) ");

            var result = OracleDbManager.ExecuteQuery(sb.ToString(), oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opmt"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool AddOpMaster(Opmt opmt, OracleConnection oraConn, OracleTransaction trans)
        {
            var oracleParams = new OpsOracleParams(opmt?.Edition.Substring(0, 1), opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo)
            {
                new OracleParameter("P_OPTIME", opmt.OpTime),
                new OracleParameter("P_OPPRICE", opmt.OpPrice),
                new OracleParameter("P_MACHINECOUNT", opmt.MachineCount),
                new OracleParameter("P_OPCOUNT", opmt.OpCount),
                new OracleParameter("P_MANCOUNT", opmt.ManCount),
                new OracleParameter("P_FILENAME", opmt.FileName),
                new OracleParameter("P_FILENAME2", opmt.FileName2),
                new OracleParameter("P_FILEPDF", opmt.FilePdf),
                new OracleParameter("P_FILEPDF2", opmt.FilePdf2),
                new OracleParameter("P_PDMFILE", opmt.PdmFile),//10
                new OracleParameter("P_PROCESSWIDTH", opmt.ProcessWidth),
                new OracleParameter("P_PROCESSHEIGHT", opmt.ProcessHeight),
                new OracleParameter("P_LAYOUTFONTSIZE", opmt.LayoutFontSize),
                new OracleParameter("P_LANGUAGE", opmt.Language),
                new OracleParameter("P_BENCHMARKTIME", opmt.BenchMarkTime),
                new OracleParameter("P_REMARKS", opmt.Remarks),
                new OracleParameter("P_TARGETOFFERPRICE", opmt.TargetOfferPrice),
                new OracleParameter("P_OFFEROPPRICE", opmt.OfferOpPrice),
                new OracleParameter("P_REGISTERID", opmt.RegisterId),
                new OracleParameter("P_GROUPMODE", opmt.GroupMode), //20
                new OracleParameter("P_CANVASHEIGHT", opmt.CanvasHeight),
                new OracleParameter("P_PLANPDF", opmt.PlanPdf),
                new OracleParameter("P_COLORTHEME", opmt.ColorTheme),
                new OracleParameter("P_FACTORY", opmt.Factory), //24
                new OracleParameter("P_MXPACKAGE", opmt.MxPackage),
                new OracleParameter("P_SYNCED", opmt.Synced),
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) {Direction = ParameterDirection.Output}
            };

            var resAdd = OracleDbManager.ExecuteQuery("SP_OPS_INSERTOPERATIONMT_OPMT", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resAdd != null;
        }

        /// <summary>
        /// Add operation plan master for new OPS
        /// </summary>
        /// <param name="opmt"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        /// Date: 4/Feb/2021
        public static bool AddOpMaster_New(Opmt opmt, OracleConnection oraConn, OracleTransaction trans)
        {            
            var oracleParams = new OpsOracleParams(opmt?.Edition.Substring(0, 1), opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo)
            {
                new OracleParameter("P_OPTIME", opmt.OpTime),
                new OracleParameter("P_OPPRICE", opmt.OpPrice),
                new OracleParameter("P_MACHINECOUNT", opmt.MachineCount),
                new OracleParameter("P_OPCOUNT", opmt.OpCount),
                new OracleParameter("P_MANCOUNT", opmt.ManCount),
                new OracleParameter("P_FILENAME", opmt.FileName),
                new OracleParameter("P_FILENAME2", opmt.FileName2),
                new OracleParameter("P_FILEPDF", opmt.FilePdf),
                new OracleParameter("P_FILEPDF2", opmt.FilePdf2),
                new OracleParameter("P_PDMFILE", opmt.PdmFile),//10
                new OracleParameter("P_PROCESSWIDTH", opmt.ProcessWidth),
                new OracleParameter("P_PROCESSHEIGHT", opmt.ProcessHeight),
                new OracleParameter("P_LAYOUTFONTSIZE", opmt.LayoutFontSize),
                new OracleParameter("P_LANGUAGE", opmt.Language),
                new OracleParameter("P_BENCHMARKTIME", opmt.BenchMarkTime),
                new OracleParameter("P_REMARKS", opmt.Remarks),
                new OracleParameter("P_TARGETOFFERPRICE", opmt.TargetOfferPrice),
                new OracleParameter("P_OFFEROPPRICE", opmt.OfferOpPrice),
                new OracleParameter("P_REGISTERID", opmt.RegisterId),
                new OracleParameter("P_GROUPMODE", opmt.GroupMode), //20
                new OracleParameter("P_CANVASHEIGHT", opmt.CanvasHeight),
                new OracleParameter("P_PLANPDF", opmt.PlanPdf),
                new OracleParameter("P_COLORTHEME", opmt.ColorTheme),
                new OracleParameter("P_FACTORY", opmt.Factory), //24
                new OracleParameter("P_MXPACKAGE", opmt.MxPackage),
                new OracleParameter("P_SYNCED", opmt.Synced),
                new OracleParameter("P_REASON", opmt.Reason),
                new OracleParameter("P_OPSOURCE", opmt.OpSource),
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) {Direction = ParameterDirection.Output}
            };

            var resAdd = OracleDbManager.ExecuteQuery("sp_ops_insertopmt_opmt_new", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resAdd != null;
        }

        /// <summary>
        /// Adds the new ops master detail.
        /// </summary>
        /// <param name="edition">The edition.</param>
        /// <param name="opMaster">The op master.</param>
        /// <param name="lstOpdt">The LST opdt.</param>
        /// <param name="lstOptl">The LST optl.</param>
        /// <param name="copyToolLinking">The copy tool linking.</param>
        /// <param name="copySelectPlan">The copy select plan.</param>
        /// <param name="registerEmptyPlan">The register empty plan.</param>
        /// <param name="importFile">The import file.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool AddNewOpsMasterDetail(string edition, Opmt opMaster, List<Opdt> lstOpdt, List<Opnt> lstOpnt,
            List<Optl> lstOptl, string copyToolLinking, string copySelectPlan, string registerEmptyPlan, string importFile, List<Prot> listProt)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    //Check NEUTRAL color, if color is neutral then must insert to t_00_scmt table.
                    if (opMaster.StyleColorSerial == ConstantGeneric.NeutralColorSerial)
                    {
                        //Check exist neutral color.
                        var lstScmt = ScmtBus.GetStyleColorByStyleCode(opMaster.StyleCode);
                        var neuScmt = lstScmt.Where(s => s.StyleColorSerial == ConstantGeneric.NeutralColorSerial).FirstOrDefault();
                        //If style neutral color is null then insert new color.
                        if (neuScmt == null)
                        {
                            //Get style color of the latest color serial
                            var styleColor = lstScmt.FirstOrDefault().StyleColor;

                            var scmt = new Scmt
                            {
                                StyleCode = opMaster.StyleCode,
                                StyleColorWays = ConstantGeneric.NeutralColorWays,
                                StyleColorSerial = ConstantGeneric.NeutralColorSerial,
                                StyleColor = styleColor
                            };
                            ScmtBus.InsertStyleColor(scmt, connection, trans);
                        }
                    }

                    //Add operation master
                    AddOpMaster(opMaster, connection, trans);

                    if (copySelectPlan == ConstantGeneric.True)
                    {
                        //Copy operation plan detail
                        foreach (var opdt in lstOpdt)
                        {
                            if (opdt.Edition == ConstantGeneric.EditionMes)
                            {
                                OpdtBus.CreateOpdt(opdt, connection, trans);
                            }
                            else
                            {
                                OpdtBus.AddOpDetail(opdt, connection, trans);
                            }
                        }

                        foreach (var opnt in lstOpnt)
                        {
                            OpntBus.InsertOpNameDetail(opnt, connection, trans);
                        }

                        //Add list of patterns linking
                        foreach (var prot in listProt)
                        {
                            ProtBus.AddPatternBom(prot, connection, trans);
                        }

                        if (copyToolLinking == ConstantGeneric.True)
                        {
                            //Copy tool linking
                            foreach (var tool in lstOptl)
                            {
                                OptlBus.AddToolLinking(tool, connection, trans);
                            }
                        }
                    }
                    else if (importFile == ConstantGeneric.True)
                    {
                        foreach (var opnt in lstOpnt)
                        {
                            OpntBus.InsertOpNameDetail(opnt, connection, trans);
                        }

                        //Copy operation plan from csv file
                        foreach (var opdt in lstOpdt)
                        {
                            if (opdt.Edition == ConstantGeneric.EditionMes)
                            {
                                OpdtBus.CreateOpdt(opdt, connection, trans);
                            }
                            else
                            {
                                OpdtBus.AddOpDetail(opdt, connection, trans);
                            }
                        }
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Add new operation plan for ops v3
        /// </summary>
        /// <param name="edition"></param>
        /// <param name="opMaster"></param>
        /// <param name="lstOpdt"></param>
        /// <param name="lstOpnt"></param>
        /// <param name="lstOptl"></param>
        /// <param name="copyToolLinking"></param>
        /// <param name="copySelectPlan"></param>
        /// <param name="registerEmptyPlan"></param>
        /// <param name="importFile"></param>
        /// <param name="listProt"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        /// Date: 4/Feb/2021
        public static bool AddNewOpsMasterDetail_New(string edition, Opmt opMaster, List<Opdt> lstOpdt, List<Opnt> lstOpnt,
            List<Optl> lstOptl, string copyToolLinking, string copySelectPlan, string registerEmptyPlan, string importFile, List<Prot> listProt)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    //Check NEUTRAL color, if color is neutral then must insert to t_00_scmt table.
                    if (opMaster.StyleColorSerial == ConstantGeneric.NeutralColorSerial)
                    {
                        //Check exist neutral color.
                        var lstScmt = ScmtBus.GetStyleColorByStyleCode(opMaster.StyleCode);
                        var neuScmt = lstScmt.Where(s => s.StyleColorSerial == ConstantGeneric.NeutralColorSerial).FirstOrDefault();
                        //If style neutral color is null then insert new color.
                        if (neuScmt == null)
                        {
                            //Get style color of the latest color serial
                            var styleColor = lstScmt.FirstOrDefault().StyleColor;

                            var scmt = new Scmt
                            {
                                StyleCode = opMaster.StyleCode,
                                StyleColorWays = ConstantGeneric.NeutralColorWays,
                                StyleColorSerial = ConstantGeneric.NeutralColorSerial,
                                StyleColor = styleColor
                            };
                            ScmtBus.InsertStyleColor(scmt, connection, trans);
                        }
                    }

                    //Add operation master
                    AddOpMaster_New(opMaster, connection, trans);

                    if (copySelectPlan == ConstantGeneric.True)
                    {
                        //Copy operation plan detail
                        foreach (var opdt in lstOpdt)
                        {
                            if (opdt.Edition == ConstantGeneric.EditionMes)
                            {
                                OpdtBus.CreateOpdt(opdt, connection, trans);
                            }
                            else
                            {
                                OpdtBus.AddOpDetail_New(opdt, connection, trans);
                            }
                        }

                        foreach (var opnt in lstOpnt)
                        {
                            OpntBus.InsertOpNameDetail_New(opnt, connection, trans);
                        }

                        //Add list of patterns linking
                        foreach (var prot in listProt)
                        {
                            ProtBus.AddPatternBom(prot, connection, trans);
                        }

                        if (copyToolLinking == ConstantGeneric.True)
                        {
                            //Copy tool linking
                            foreach (var tool in lstOptl)
                            {
                                OptlBus.AddToolLinking(tool, connection, trans);
                            }
                        }
                    }
                    else if (importFile == ConstantGeneric.True)
                    {
                        foreach (var opnt in lstOpnt)
                        {
                            OpntBus.InsertOpNameDetail_New(opnt, connection, trans);
                        }

                        //Copy operation plan from csv file
                        foreach (var opdt in lstOpdt)
                        {
                            if (opdt.Edition == ConstantGeneric.EditionMes)
                            {
                                OpdtBus.CreateOpdt(opdt, connection, trans);
                            }
                            else
                            {
                                OpdtBus.AddOpDetail_New(opdt, connection, trans);
                            }
                        }
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public static bool AddNewMESOpMasterDetail(string edition, Opmt opMaster, List<Opdt> lstOpdt, List<Opnt> lstOpnt,
            List<Optl> lstOptl, List<Prot> listProt, string copyToolLinking, string copySelectPlan)
        {
            //using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr)) ConnectionStrMes
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStrMes))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    //Check NEUTRAL color, if color is neutral then must insert to t_00_scmt table.
                    if (opMaster.StyleColorSerial == ConstantGeneric.NeutralColorSerial)
                    {
                        //Check exist neutral color.
                        var lstScmt = ScmtBus.GetStyleColorByStyleCode(opMaster.StyleCode);
                        var neuScmt = lstScmt.Where(s => s.StyleColorSerial == ConstantGeneric.NeutralColorSerial).FirstOrDefault();
                        //If style neutral color is null then insert new color.
                        if (neuScmt == null)
                        {
                            //Get style color of the latest color serial
                            var styleColor = lstScmt.FirstOrDefault().StyleColor;

                            var scmt = new Scmt
                            {
                                StyleCode = opMaster.StyleCode,
                                StyleColorWays = ConstantGeneric.NeutralColorWays,
                                StyleColorSerial = ConstantGeneric.NeutralColorSerial,
                                StyleColor = styleColor
                            };
                            ScmtBus.InsertStyleColor(scmt, connection, trans);
                        }
                    }

                    //Add operation master
                    AddOpMaster(opMaster, connection, trans);

                    if (copySelectPlan == ConstantGeneric.True)
                    {
                        //Copy operation plan detail
                        foreach (var opdt in lstOpdt)
                        {
                            OpdtBus.CreateOpdt(opdt, connection, trans);
                        }

                        foreach (var opnt in lstOpnt)
                        {
                            OpntBus.InsertOpNameDetail(opnt, connection, trans);
                        }

                        if (copyToolLinking == ConstantGeneric.True)
                        {
                            //Copy tool linking
                            foreach (var tool in lstOptl)
                            {
                                OptlBus.AddToolLinking(tool, connection, trans);
                            }

                            foreach (var prot in listProt)
                            {
                                ProtBus.AddPatternBom(prot, connection, trans);
                            }
                        }
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Update op time master.
        /// </summary>
        /// <param name="opdt"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static bool UpdateOpTimeMaster(Opdt opdt, OracleConnection oraConn, OracleTransaction trans)
        {
            var oracleParams = new OpsOracleParams(opdt.Edition, opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo, opdt.OpRevNo)
            {
                new OracleParameter("P_OPTIME", opdt.TackTime)
            };

            var resUpdate = OracleDbManager.ExecuteQuery("SP_OPS_UPDATEOPTIME_OPMT", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resUpdate != null;
        }

        /// <summary>
        /// Update object operation master
        /// </summary>
        /// <param name="opmt"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateOpMaster(Opmt opmt, OracleConnection oraConn, OracleTransaction trans)
        {
            var oracleParams = new OpsOracleParams(opmt?.Edition.Substring(0, 1), opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo)
            {
                new OracleParameter("P_OPTIME", opmt.OpTime),
                new OracleParameter("P_OPPRICE", opmt.OpPrice),
                new OracleParameter("P_MACHINECOUNT", opmt.MachineCount),
                new OracleParameter("P_CONFIRMCHK", opmt.ConfirmChk),
                new OracleParameter("P_OPCOUNT", opmt.OpCount),
                new OracleParameter("P_MANCOUNT", opmt.ManCount),
                new OracleParameter("P_FILENAME", opmt.FileName),
                new OracleParameter("P_FILENAME2", opmt.FileName2),
                new OracleParameter("P_FILEPDF", opmt.FilePdf),
                new OracleParameter("P_FILEPDF2", opmt.FilePdf2), //10
                new OracleParameter("P_PDMFILE", opmt.PdmFile),
                new OracleParameter("P_PROCESSWIDTH", opmt.ProcessWidth),
                new OracleParameter("P_PROCESSHEIGHT", opmt.ProcessHeight),
                new OracleParameter("P_LAYOUTFONTSIZE", opmt.LayoutFontSize),
                new OracleParameter("P_LANGUAGE", opmt.Language),
                new OracleParameter("P_LASTUPDATEDTIME", opmt.LastUpdateTime),
                new OracleParameter("P_BENCHMARKTIME", opmt.BenchMarkTime),
                new OracleParameter("P_REMARKS", opmt.Remarks),
                new OracleParameter("P_TARGETOFFERPRICE", opmt.TargetOfferPrice),
                new OracleParameter("P_OFFEROPPRICE", opmt.OfferOpPrice), //20
                new OracleParameter("P_CONFIRMEDDATE", opmt.ConfirmedDate),
                new OracleParameter("P_REGISTERID", opmt.RegisterId),
                new OracleParameter("P_REGISTRYDATE", opmt.RegistryDate),
                new OracleParameter("P_CONFIRMEDID", opmt.ConfirmedId),
                new OracleParameter("P_GROUPMODE", opmt.GroupMode),
                new OracleParameter("P_CANVASHEIGHT", opmt.CanvasHeight),
                new OracleParameter("P_PLANPDF", opmt.PlanPdf),
                new OracleParameter("P_COLORTHEME", opmt.ColorTheme),
                new OracleParameter("P_FACTORY", opmt.Factory),
                new OracleParameter("P_SYNCED", opmt.Synced)
            };

            var resUpdate = OracleDbManager.ExecuteQuery("SP_OPS_UPDATEOPMASTER_OPMT", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resUpdate != null;
        }

        /// <summary>
        /// Update operation master
        /// </summary>
        /// <param name="objOpmt"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateOpMaster(Opmt objOpmt)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    if (!UpdateOpMaster(objOpmt, connection, trans))
                    {
                        trans.Rollback();
                        return false;
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Copying opmt and opdt.
        /// </summary>
        /// <param name="opmt">Operation master.</param>
        /// <param name="opdts">The list of operation details.</param>
        /// Author: Nguyen Xuan Hoang
        public static bool CopyOpsMaster(Opmt opmt, List<Opdt> opdts)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    var edition = string.IsNullOrWhiteSpace(opmt.Edition2) ? opmt.Edition : opmt.Edition2;
                    var maxRevNoInt = GetMaxOpRevision(edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, true);
                    var maxRevNoStr = maxRevNoInt == 0 ? "001" : maxRevNoInt.ToString("D3");
                    var oldOpmt = GetOpsMasterByCode(opmt).FirstOrDefault();

                    //START MOD) SON - 05/Mar/2019: Get list process with standard name
                    var oldOpdts = OpdtBus.GetOpDetailByLanguage(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial,
                    opmt.RevNo, opmt.OpRevNo, opmt.Edition, opmt.Language);

                    ////Get list of process detail with standard name
                    //var oldOpdts = OpdtBus.GetOpDetailWithStandardName(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial,
                    //opmt.RevNo, opmt.OpRevNo, opmt.Edition, opmt.Language);

                    ////Recalculate takt time and recount the number of processes, machines and men in operation plan
                    //if (oldOpmt.OpCount > oldOpdts.Count())
                    //{
                    //    //Count the number of processes, machines and men.
                    //    var opmtCount = OpmtBus.CountProcessesWithStandardName(oldOpmt.Edition, oldOpmt.StyleCode, oldOpmt.StyleSize, oldOpmt.StyleColorSerial, oldOpmt.RevNo, oldOpmt.OpRevNo);
                    //    //Recalculate takt time
                    //    var taktTime = OpdtBus.GetTackTimeWithStandardName(oldOpmt);

                    //    if (opmtCount != null)
                    //    {
                    //        oldOpmt.OpCount = int.Parse(opmtCount.SumOpCount.ToString());
                    //        oldOpmt.MachineCount = int.Parse(opmtCount.SumMachineCount.ToString());
                    //        //START MOD) SON - 19/Apr/2019 - Make mancount up round
                    //        //oldOpmt.ManCount = int.Parse(opmtCount.SumManCount.ToString());
                    //        oldOpmt.ManCount = Convert.ToInt32(Math.Ceiling(opmtCount.SumManCount.Value));
                    //        //END MOD) SON - 19/Apr/2019

                    //    }
                    //    else //There is no process with standard name.
                    //    {
                    //        oldOpmt.OpCount = 0;
                    //        oldOpmt.MachineCount = 0;
                    //        oldOpmt.ManCount = 0;
                    //    }

                    //    oldOpmt.OpTime = int.Parse(taktTime.ToString());

                    //}
                    //END MOD) SON - 05/Mar/2019

                    // Update field opmt
                    if (oldOpmt != null)
                    {
                        oldOpmt.RegisterId = opmt.RegisterId;
                        oldOpmt.OpRevNo = maxRevNoStr;
                        oldOpmt.ConfirmChk = "";
                        oldOpmt.Language = opmt.Language;
                        oldOpmt.GroupMode = opmt.GroupMode;
                        oldOpmt.ProcessWidth = opmt.ProcessWidth;
                        oldOpmt.ProcessHeight = opmt.ProcessHeight;
                        oldOpmt.LayoutFontSize = opmt.LayoutFontSize;
                        oldOpmt.CanvasHeight = opmt.CanvasHeight;
                        if (!string.IsNullOrWhiteSpace(opmt.Factory)) oldOpmt.Factory = opmt.Factory;
                        oldOpmt.Remarks = opmt.Remarks;
                        oldOpmt.Reason = opmt.Reason;
                        oldOpmt.Edition = edition;

                        AddOpMaster(oldOpmt, connection, trans);
                    }

                    foreach (var opdt in oldOpdts)
                    {
                        var newOpdt = opdts.FirstOrDefault(d => d.StyleCode == opdt.StyleCode && d.StyleSize == opdt.StyleSize
                                   && d.StyleColorSerial == opdt.StyleColorSerial && d.RevNo == opdt.RevNo &&
                                   d.OpRevNo == opdt.OpRevNo && d.OpSerial == opdt.OpSerial);

                        // Update fields of opdt
                        opdt.OpRevNo = maxRevNoStr;
                        if (newOpdt != null)
                        {
                            opdt.OpName = newOpdt.OpName;
                            //opdt.OpSerial = newOpdt.OpSerial;
                            opdt.OpGroup = newOpdt.OpGroup;
                            opdt.MachineType = newOpdt.MachineType;
                            opdt.ModuleId = newOpdt.ModuleId;
                            opdt.NextOp = newOpdt.NextOp;
                            opdt.X = newOpdt.X;
                            opdt.Y = newOpdt.Y;
                            opdt.Page = newOpdt.Page;
                            opdt.DisplayColor = newOpdt.DisplayColor;
                        }

                        opdt.Edition = edition;
                        bool kq = OpdtBus.AddOpDetail(opdt, connection, trans);
                        if (kq)
                        {
                            //Get list opeartion name detai for coping
                            var lstOpnt = OpntBus.GetOpNameDetails(opmt.Edition, opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo, opmt.OpRevNo, opdt.OpSerial.ToString(), "", "");
                            foreach (var opnt in lstOpnt)
                            {
                                opnt.Edition = edition;
                                opnt.OpRevNo = maxRevNoStr;
                                OpntBus.InsertOpNameDetail(opnt, connection, trans);
                            }
                        }
                    }

                    //START MOD) SON - 05/Mar/2019: Get list of tool and machine of process with standard name                   
                    //var optls = OptlBus.GetListToolLinkingWithStandardName(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo, opmt.Edition);

                    //START MOD - SON - Get list of too linking by edtion
                    //var optls = OptlBus.GetListToolLinking(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo);
                    var optls = OptlBus.GetListToolLinking(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo, opmt.Edition);
                    //END MOD - SON

                    //END MOD) SON - 05/Mar/2019

                    // Change Ops master key in list copy tool linking
                    foreach (var tool in optls)
                    {
                        //tool.StyleCode = opmt.StyleCode;
                        //tool.StyleSize = opmt.StyleSize;
                        //tool.StyleColorSerial = opmt.StyleColorSerial;
                        //tool.RevNo = opmt.RevNo;
                        tool.OpRevNo = maxRevNoStr;
                        tool.Edition = edition;
                        OptlBus.AddToolLinking(tool, connection, trans);
                    }

                    //START ADD) SON - 10/Jul/2019 - Get linked pattern 
                    var listProt = ProtBus.GetListPatternLinked(opmt.StyleCode, opmt.StyleSize,
                        opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo, opmt.Edition);

                    //Change operation plan key 
                    foreach (var prot in listProt)
                    {
                        prot.OpRevNo = maxRevNoStr;
                        prot.Edition = edition;

                        ProtBus.AddPatternBom(prot, connection, trans);
                    }
                    //END ADD) SON - 10/Jul/2019
                    trans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return false;
                }
            }
        }

        /// <summary>
        /// Deletes the ops master.
        /// </summary>
        /// <param name="opMaster">The op detail.</param>
        /// <param name="trans">The trans.</param>
        /// <param name="oraConn">The ora connection.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool DeleteOpsMaster(Opmt opMaster, OracleTransaction trans, OracleConnection oraConn)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) {Direction = ParameterDirection.Output},
                new OracleParameter("P_STYLECODE", opMaster.StyleCode),
                new OracleParameter("P_STYLESIZE", opMaster.StyleSize),
                new OracleParameter("P_STYLECOLORSERIAL", opMaster.StyleColorSerial),
                new OracleParameter("P_REVNO", opMaster.RevNo),
                new OracleParameter("P_OPREVNO", opMaster.OpRevNo),
                new OracleParameter("P_EDITION", opMaster.Edition)
            };

            var resDel = OracleDbManager.ExecuteQuery("SP_OPS_DELETEOPSMASTER_OPMT", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resDel != null && int.Parse(resDel.ToString()) != 0;

        }

        /// <summary>
        /// Deletes the opmt in PKMES.
        /// </summary>
        /// <param name="opmt">The operation master.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 19-Jul-19
        public static bool MesDeleteOpmt(Opmt opmt, OracleTransaction transaction, OracleConnection connection)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) {Direction = ParameterDirection.Output},
                new OracleParameter("P_STYLECODE", opmt.StyleCode),
                new OracleParameter("P_STYLESIZE", opmt.StyleSize),
                new OracleParameter("P_STYLECOLORSERIAL", opmt.StyleColorSerial),
                new OracleParameter("P_REVNO", opmt.RevNo),
                new OracleParameter("P_OPREVNO", opmt.OpRevNo),
                new OracleParameter("P_MXPACKAGE", opmt.MxPackage)
            };

            var result = OracleDbManager.ExecuteQuery("SP_MES_DELETE_OPMT", oracleParams.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return result != null && int.Parse(result.ToString()) != 0;
        }

        /// <summary>
        /// Deletes the ops master detail tool linking.
        /// </summary>
        /// <param name="opMaster">The op master.</param>
        /// <param name="lstOpdt">The LST opdt.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        //public static bool DeleteOpsMasterDetailToolLinking(Opmt opMaster, List<Opdt> lstOpdt)
        public static bool DeleteOpsMasterDetailToolLinking(Opmt opMaster)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    if (OptlBus.DeleteOpsToolByOpmt(opMaster, trans, connection)
                             && OpntBus.DeleteOpNameDetailByOpmt(opMaster, connection, trans)
                             && ProtBus.DeletePatternBomByOpmt(opMaster, trans, connection)
                             && OpdtBus.DeleteOpDetailByOpmt(opMaster, trans, connection))
                    {
                        //Delete ops master
                        if (!DeleteOpsMaster(opMaster, trans, connection))
                        {
                            trans.Rollback();
                            return false;
                        }
                    }
                    else
                    {
                        trans.Rollback();
                        return false;
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Delete operation plan including Tools, OP name details, Pattern and OP details.
        /// </summary>
        /// <param name="opmt">The opmt.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 19-Jul-19
        //public static bool MesDeleteOperationPlan(Opmt opmt)
        //{
        //    using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
        //    {
        //        connection.Open();
        //        var trans = connection.BeginTransaction();
        //        try
        //        {
        //            if (OptlBus.DeleteOpsToolByOpmt(opmt, trans, connection)
        //                && OpntBus.DeleteOpNameDetailByOpmt(opmt, connection, trans)
        //                && ProtBus.DeletePatternBomByOpmt(opmt, trans, connection)
        //                && OpdtBus.DeleteOpDetailByOpmt(opmt, trans, connection))
        //            {
        //                //Delete ops master
        //                if (!MesDeleteOpmt(opmt, trans, connection))
        //                {
        //                    trans.Rollback();
        //                    return false;
        //                }
        //            }
        //            else
        //            {
        //                trans.Rollback();
        //                return false;
        //            }
        //            trans.Commit();
        //            return true;
        //        }
        //        catch (Exception)
        //        {
        //            trans.Rollback();
        //            throw;
        //        }
        //    }
        //}

        public static bool DeleteMESOpMaster(Opmt opMaster)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStrMes))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    if (OptlBus.DeleteOpsToolByOpmt(opMaster, trans, connection)
                             && OpntBus.DeleteOpNameDetailByOpmt(opMaster, connection, trans)
                             && ProtBus.DeletePatternBomByOpmt(opMaster, trans, connection)
                             && OpdtBus.DeleteOpDetailByOpmt(opMaster, trans, connection))
                    {
                        //Delete ops master
                        if (!DeleteOpsMaster(opMaster, trans, connection))
                        {
                            trans.Rollback();
                            return false;
                        }
                    }
                    else
                    {
                        trans.Rollback();
                        return false;
                    }

                    trans.Commit();
                    return true;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Confirms the ops master and detail.
        /// </summary>
        /// <param name="opMaster">The op master.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool ConfirmOpsMasterAndDetail(Opmt opMaster)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    if (!ConfirmOpsMaster(opMaster, trans, connection))
                    {
                        //No need to confirm OpsDetail ?
                        trans.Rollback();
                        return false;
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        ///// <summary>
        ///// Confirm MES operaiton plan
        ///// </summary>
        ///// <param name="opMaster"></param>
        ///// <returns></returns>
        //public static bool ConfirmMESOperationPlan(Opmt opMaster)
        //{
        //    using (var connection = new OracleConnection(ConstantGeneric.ConnectionStrMes))
        //    {
        //        connection.Open();
        //        var trans = connection.BeginTransaction();
        //        try
        //        {
        //            if (!ConfirmOpsMaster(opMaster, trans, connection))
        //            {
        //                //No need to confirm OpsDetail ?
        //                trans.Rollback();
        //                return false;
        //            }

        //            trans.Commit();
        //            return true;
        //        }
        //        catch (Exception)
        //        {
        //            trans.Rollback();
        //            throw;
        //        }
        //    }
        //}

        /// <summary>
        /// Confirms the ops master.
        /// </summary>
        /// <param name="opMaster">The op master.</param>
        /// <param name="trans">The trans.</param>
        /// <param name="oraConn">The ora connection.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool ConfirmOpsMaster(Opmt opMaster, OracleTransaction trans, OracleConnection oraConn)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) {Direction = ParameterDirection.Output},
                new OracleParameter("P_EDITION", opMaster.Edition.Substring(0,1)),
                new OracleParameter("P_STYLECODE", opMaster.StyleCode),
                new OracleParameter("P_STYLESIZE", opMaster.StyleSize),
                new OracleParameter("P_STYLECOLORSERIAL", opMaster.StyleColorSerial),
                new OracleParameter("P_REVNO", opMaster.RevNo),
                new OracleParameter("P_OPREVNO", opMaster.OpRevNo),
                new OracleParameter("P_CONFIRMID", opMaster.ConfirmedId)
            };

            var resDel = OracleDbManager.ExecuteQuery("SP_OPS_CONFIRMOPMT_OPMT", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resDel != null && int.Parse(resDel.ToString()) != 0;

        }

        /// <summary>
        /// Gets the ops master by code.
        /// </summary>
        /// <param name="opMaster">The op master.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Opmt> GetOpsMasterByCode(Opmt opMaster)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            List<OracleParameter> oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", opMaster.StyleCode),
                new OracleParameter("P_STYLESIZE", opMaster.StyleSize),
                new OracleParameter("P_STYLECOLORSERIAL", opMaster.StyleColorSerial),
                new OracleParameter("P_REVNO", opMaster.RevNo),
                new OracleParameter("P_OPREVNO", opMaster.OpRevNo),
                new OracleParameter("P_EDITION", opMaster.Edition),
                cursor
            };
            var lstStyleMaster = OracleDbManager.GetObjects<Opmt>("SP_OPS_GETOPSMASTERBYCODE_OPMT", CommandType.StoredProcedure, oracleParams.ToArray());
            return lstStyleMaster;
        }

        /// <summary>
        /// Gets the by mes package.
        /// </summary>
        /// <param name="opmt">The operation master.</param>
        /// <returns>List of operation masters</returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 26-Feb-19
        public static List<Opmt> GetByMxPackage(Opmt opmt)
        {
            var cursor = new OracleParameter("out_cursor", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            var oracleParams = new List<OracleParameter>
            {

                new OracleParameter("p_stylecode", opmt.StyleCode),
                new OracleParameter("p_stylesize", opmt.StyleSize),
                new OracleParameter("p_stylecolorserial", opmt.StyleColorSerial),
                new OracleParameter("p_revno", opmt.RevNo),
                new OracleParameter("p_mxpackage", opmt.MxPackage),
                cursor
            };
            var opmts = OracleDbManager.GetObjects<Opmt>("SP_MES_GETBYMXPACKAGE_OPMT", CommandType.StoredProcedure,
                oracleParams.ToArray(), EnumDataSource.PkMes);

            return opmts;
        }

        /// <summary>
        /// Count process by operation master
        /// </summary>
        /// <param name="opmt"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static Opmt CountOpDetail(string edition, string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            var oracleParams = new OpsOracleParams(edition?.Substring(0, 1), styleCode, styleSize, styleColorSerial, revNo, opRevNo)
            {
                cursor
            };
            //oracleParams.AddCursor();

            var resOpmt = OracleDbManager.GetObjects<Opmt>("SP_OPS_COUNTOPDETAIL_OPDT", CommandType.StoredProcedure, oracleParams.ToArray());

            return resOpmt.FirstOrDefault();
        }

        /// <summary>
        /// Count the number of processes which use standard name
        /// </summary>
        /// <param name="edition"></param>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Opmt CountProcessesWithStandardName(string edition, string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            var oracleParams = new OpsOracleParams(edition?.Substring(0, 1), styleCode, styleSize, styleColorSerial, revNo, opRevNo)
            {
                cursor
            };

            var resOpmt = OracleDbManager.GetObjects<Opmt>("SP_OPS_CNTOPDETAILSTDNAME_OPDT", CommandType.StoredProcedure, oracleParams.ToArray());

            return resOpmt.FirstOrDefault();
        }

        /// <summary>
        /// Updates the opmt mxpackage.
        /// </summary>
        /// <param name="opmt">The operation master.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 16-Jul-19
        public static bool UpdateOpmtMxPackage(Opmt opmt)
        {
            var prs = new OpsOracleParams(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo)
            {
                new OracleParameter("P_MXPACKAGE", opmt.MxPackage)
            };
            prs.Insert(0, new OracleParameter("p_affectedrows", OracleDbType.Int16)
            {
                Direction = ParameterDirection.Output
            });

            var result = OracleDbManager.ExecuteQuery("SP_MES_UPDATEMXPACKAGE_OPMT", prs.ToArray(),
                CommandType.StoredProcedure, ConstantGeneric.ConnectionStrMes);

            return result != null && int.Parse(result.ToString()) != 0;
        }

        /// <summary>
        /// Get operation plan target quantity
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Opmt GetOpTargetQuantity(string styleCode, string styleSize, string styleColorSerial, string revNo, string tableNameOpmt, string tableNameOpdt)
        {
            string strSql = $@"select opm.stylecode, opm.stylesize, opm.stylecolorserial, opm.revno, opm.oprevno 
                                , sum(opd.machinecount) SumMachineCount, sum(opd.mancount) SumManCount, max(opd.optime) optime
                                , round( 7.5 * ( 3600 / max(
                                                                 CASE
                                                                     WHEN opd.mancount < 1
                                                                             OR mod(opd.mancount,1) <> 0 THEN 0 
                                                                        ELSE round(opd.optime / opd.mancount)
                                                                 END   
                                                            ))) as targetperday
                        from {tableNameOpmt} opm 
                              join {tableNameOpdt} opd 
                                    on opd.stylecode = opm.stylecode and opd.stylesize = opm.stylesize and opd.stylecolorserial = opm.stylecolorserial 
                                    and opd.revno = opm.revno and opd.oprevno = opm.oprevno
                        where opm.stylecode = :P_STYLECODE and opm.stylesize = :P_STYLESIZE and opm.stylecolorserial = :P_STYLECOLORSERIAL and opm.revno = :P_REVNO
                                                    and last_updated_time = (select max(last_updated_time)
                                                                             from {tableNameOpmt} 
                                                                              where stylecode = :P_STYLECODE and stylesize = :P_STYLESIZE and stylecolorserial = :P_STYLECOLORSERIAL and revno = :P_REVNO )
                        group by opm.stylecode, opm.stylesize, opm.stylecolorserial, opm.revno, opm.oprevno";

            var oraParam = new List<OracleParameter>()
            {
                //new OracleParameter("P_STYLECODE2", styleCode),
                //new OracleParameter("P_STYLESIZE2", styleSize),
                //new OracleParameter("P_STYLECOLORSERIAL2", styleColorSerial),
                //new OracleParameter("P_REVNO2", revNo),
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLESIZE", styleSize),
                new OracleParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new OracleParameter("P_REVNO", revNo)
            };

            var opmt = OracleDbManager.GetObjectsByType<Opmt>(strSql, CommandType.Text, oraParam.ToArray()).FirstOrDefault();

            return opmt;

        }

        /// <summary>
        /// Update is used status for operation plan master.
        /// </summary>
        /// <param name="opdt"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        /// Date: 15/Jun/2020
        public static bool UpdateIsUsedStatus(string edition, string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo)
        {
            var oracleParams = new OpsOracleParams(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo);


            var resUpdate = OracleDbManager.ExecuteQuery("SP_OPS_UPDATEISUSED_OPMT", oracleParams.ToArray(), CommandType.StoredProcedure);

            return resUpdate != null;
        }
        #endregion

        #region MySQL Database

        /// <summary>
        /// Gets the opmt.
        /// </summary>
        /// <param name="opmt">The opmt.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static Opmt GetOpmt(Opmt opmt)
        {
            var ps = new OpsMySqlParams(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo);
            var opms = MySqlDBManager.GetAll<Opmt>("sp_mes_get_opmt", CommandType.StoredProcedure, ps.ToArray());

            return opms.FirstOrDefault();
        }

        /// <summary>
        /// MySQL get by mx package.
        /// </summary>
        /// <param name="opmt">The opmt.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static List<Opmt> MySqlGetByMxPackage(Opmt opmt)
        {
            var ps = new OpsMySqlParams(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo)
            {
                new MySqlParameter("P_MXPACKAGE", opmt.MxPackage)
            };
            var opms = MySqlDBManager.GetAll<Opmt>("SP_MES_GETBYMXPACKAGE_OPMT", CommandType.StoredProcedure, ps.ToArray());

            return opms;
        }

        /// <summary>
        /// Get MxPackage by Operation Plan key code
        /// </summary>
        /// <param name="opmt"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static async Task<Opmt> GetByMxPackageAsyncMySql(Opmt opmt)
        {
            var ps = new OpsMySqlParams(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo)
            {
                new MySqlParameter("P_MXPACKAGE", opmt.MxPackage)
            };
            var opms = MySqlDBManager.GetAll<Opmt>("SP_MES_GETBYMXPACKAGE_OPMT", CommandType.StoredProcedure, ps.ToArray()).FirstOrDefault();

            return await Task.FromResult(opms);
        }

        /// <summary>
        /// Adds the operation plan.
        /// </summary>
        /// <param name="edition">The edition.</param>
        /// <param name="opmt">The opmt.</param>
        /// <param name="opdts">The opdts.</param>
        /// <param name="opnts">The opnts.</param>
        /// <param name="optls">The optls.</param>
        /// <param name="isCopyTool">The is copy tool.</param>
        /// <param name="isCopySelectPlan">The is copy select plan.</param>
        /// <param name="isImportFile">The is import file.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool AddOperationPlan(string edition, Opmt opmt, List<Opdt> opdts, List<Opnt> opnts, List<Optl> optls,
            bool isCopyTool, bool isCopySelectPlan, bool isImportFile)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                var trans = connection.BeginTransaction();

                try
                {
                    //Check NEUTRAL color, if color is neutral then must insert to t_00_scmt table.
                    if (opmt.StyleColorSerial == ConstantGeneric.NeutralColorSerial)
                    {
                        //Check exist neutral color.
                        var scmts = ScmtBus.GetStyleColorByStyleCode(opmt.StyleCode);
                        var neuScmt = scmts.FirstOrDefault(s => s.StyleColorSerial == ConstantGeneric.NeutralColorSerial);

                        //If style neutral color is null then insert new color.
                        if (neuScmt == null)
                        {
                            //Get style color of the latest color serial
                            var styleColor = scmts.FirstOrDefault()?.StyleColor;

                            var scmt = new Scmt
                            {
                                StyleCode = opmt.StyleCode,
                                StyleColorWays = ConstantGeneric.NeutralColorWays,
                                StyleColorSerial = ConstantGeneric.NeutralColorSerial,
                                StyleColor = styleColor
                            };
                            ScmtBus.AddScmt(scmt, connection, trans);
                        }
                    }

                    //Add operation master
                    AddOpmt(opmt, connection, trans);

                    if (isCopySelectPlan)
                    {
                        //Copy operation plan detail
                        foreach (var opdt in opdts)
                        {
                            if (opdt.Edition == ConstantGeneric.EditionMes)
                            {
                                OpdtBus.InsertOpdt(opdt, connection, trans);
                            }
                        }

                        foreach (var opnt in opnts)
                        {
                            OpntBus.InsertOpnt(opnt, connection, trans);
                        }

                        if (isCopyTool)
                        {
                            //Copy tool linking
                            foreach (var tool in optls)
                            {
                                OptlBus.InsertTool(tool, connection, trans);
                            }
                        }
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public async Task<bool> RegisterOp(Opmt opmt, List<Opdt> opdts, List<Opnt> opnts, List<Optl> optls,
            bool isCopyTool, bool isCopySelectPlan)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                using (var trans = connection.BeginTransaction())
                {
                    try
                    {
                        //Check NEUTRAL color, if color is neutral then must insert to t_00_scmt table.
                        if (opmt.StyleColorSerial == ConstantGeneric.NeutralColorSerial)
                        {
                            //Check exist neutral color.
                            var scmts = ScmtBus.GetStyleColorByStyleCode(opmt.StyleCode);
                            var neuScmt = scmts.FirstOrDefault(s => s.StyleColorSerial == ConstantGeneric.NeutralColorSerial);

                            //If style neutral color is null then insert new color.
                            if (neuScmt == null)
                            {
                                //Get style color of the latest color serial
                                var styleColor = scmts.FirstOrDefault()?.StyleColor;

                                var scmt = new Scmt
                                {
                                    StyleCode = opmt.StyleCode,
                                    StyleColorWays = ConstantGeneric.NeutralColorWays,
                                    StyleColorSerial = ConstantGeneric.NeutralColorSerial,
                                    StyleColor = styleColor
                                };
                                ScmtBus.AddScmt(scmt, connection, trans);
                            }
                        }

                        //Add operation master
                        await AddOpmt1(opmt, connection, trans);

                        if (isCopySelectPlan)
                        {
                            //Copying operation plan detail (opdt)
                            await _OpdtBus.BulkInsertOpdtAsync(opdts, connection, trans);

                            //trans.Commit();

                            //Copying process name (opnt)
                            
                            //foreach (var opnt in opnts)
                            //{
                            //    OpntBus.InsertOpnt(opnt, connection, trans);
                            //}

                            if (isCopyTool)
                            {
                                //Copy tool linking
                                foreach (var tool in optls)
                                {
                                    OptlBus.InsertTool(tool, connection, trans);
                                }
                            }
                        }

                        trans.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        await connection.CloseAsync();
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public string AddOpmtQuery(Opmt opmt)
        {
            if (opmt == null) return null;
            var q = $@"INSERT INTO `mes`.`t_mx_opmt`
                            (`STYLECODE`,
                            `STYLESIZE`,
                            `STYLECOLORSERIAL`,
                            `REVNO`,
                            `OPREVNO`,
                            `OPTIME`,
                            `OPPRICE`,
                            `MACHINECOUNT`,
                            `CONFIRMCHK`,
                            `OPCOUNT`,
                            `MANCOUNT`,
                            `FILENAME`,
                            `FILENAME2`,
                            `FILEPDF`,
                            `FILEPDF2`,
                            `PDMFILE`,
                            `PROCESSWIDTH`,
                            `PROCESSHEIGHT`,
                            `PLANPDF`,
                            `COLORTHEME`,
                            `REMARKS`,
                            `LAYOUTFONTSIZE`,
                            `LANGUAGE`,
                            `LAST_UPDATED_TIME`,
                            `BENCHMARKTIME`,
                            `FACTORY`,                            
                            `GROUPMODE`,
                            `CANVASHEIGHT`,
                            `MXPACKAGE`,
                            `REGISTERID`,
                            `REGISTRYDATE`)
                            VALUES ('{opmt.StyleCode}',
                            '{opmt.StyleSize}',
                            '{opmt.StyleColorSerial}',
                            '{opmt.RevNo}',
                            '{opmt.OpRevNo}',
                            '{opmt.OpTime}>,
                            '{opmt.OpPrice}>,
                            '{opmt.MachineCount}>,
                            '{opmt.ConfirmChk}',
                            '{opmt.OpCount}>,
                            '{opmt.ManCount}>,
                            '{opmt.FileName}',
                            '{opmt.FileName2}',
                            '{opmt.FilePdf}',
                            '{opmt.FilePdf2}',
                            '{opmt.PdmFile}',
                            '{opmt.ProcessWidth}',
                            '{opmt.ProcessHeight}',
                            '{opmt.PlanPdf}',
                            '{opmt.ColorTheme}',
                            '{opmt.Remarks}',
                            '{opmt.LayoutFontSize}',
                            '{opmt.Language}',
                            '{opmt.LastUpdateTime}',
                            '{opmt.BenchMarkTime}',
                            '{opmt.Factory}',
                            '{opmt.GroupMode}',
                            '{opmt.CanvasHeight}',
                            '{opmt.MxPackage}',
                            '{opmt.RegisterId}',
                            '{opmt.RegistryDate}')";

            return q;
        }

        public string BulkInsertOpntQuery(List<Opnt> opnts)
        {
            if (opnts == null || opnts.Count == 0 ) return null;
            var qValue = "";

            for (var i = 0; i < opnts.Count; i++)
            {
                qValue += $"('{opnts[i].Edition}','{opnts[i].StyleCode}','{opnts[i].StyleSize}','{opnts[i].StyleColorSerial}'," +
                          $"'{opnts[i].RevNo}','{opnts[i].OpRevNo}','{opnts[i].OpSerial}','{opnts[i].OpNameId}','{opnts[i].OpTime}'," +
                          $"'{opnts[i].OpnSerial}','{opnts[i].MachineType}','{opnts[i].MachineCount}','{opnts[i].Remarks}','{opnts[i].MaxTime}'," +
                          $"'{opnts[i].ManCount}','{opnts[i].JobType}','{opnts[i].ToolId}','{opnts[i].ActionCode}')";

                if (i != opnts.Count - 1) qValue += ",";
            }

            var q = $@"INSERT INTO `mes`.`t_mx_opdt`
                            (`STYLECODE`,
                            `STYLESIZE`,
                            `STYLECOLORSERIAL`,
                            `REVNO`,
                            `OPREVNO`,
                            `OPSERIAL`,
                            `OpNameId`,
                            `OpTime`,
                            `OpnSerial`,
                            `MachineType`,
                            `MachineCount`,
                            `Remarks`,
                            `MaxTime`,
                            `ManCount`,
                            `JobType`,
                            `ToolId`,
                            `ActionCode`)
                        VALUES {qValue};";

            return q;
        }

        public string BulkInsertOpdtQuery(List<Opdt> opdts)
        {
            if (opdts == null || opdts.Count == 0) return null;
            var qValue = "";

            for (var i = 0; i < opdts.Count; i++)
            {
                var opName = opdts[i].OpName?.Length > 200 ? opdts[i].OpName.Substring(0, 199) : opdts[i].OpName;
                var mcPairDate = $"{opdts[i].McPairDate.Year}-{opdts[i].McPairDate.Month}-{opdts[i].McPairDate.Day}";

                qValue += $"('{opdts[i].StyleCode}','{opdts[i].StyleSize}','{opdts[i].StyleColorSerial}','{opdts[i].RevNo}'," +
                          $"'{opdts[i].OpRevNo}','{opdts[i].OpSerial}','{opdts[i].OpNum}','{opdts[i].OpGroup}','{opName}'," +
                          $"'{opdts[i].Factory}','{opdts[i].MachineType}','{opdts[i].ThreadColor}','{opdts[i].OpDesc}'," +
                          $"'{opdts[i].OpTime}','{opdts[i].OpPrice}','{opdts[i].OfferOpPrice}','{opdts[i].MachineCount}'," +
                          $"'{opdts[i].Remarks}','{opdts[i].MaxTime}','{opdts[i].ManCount}','{opdts[i].FileName}'," +
                          $"'{opdts[i].NextOp}','{opdts[i].OutSourced}','{opdts[i].X}','{opdts[i].Y}','{opdts[i].ImageName}'," +
                          $"'{opdts[i].DisplayColor}','{opdts[i].Page}','{opdts[i].GroupColor}','{opdts[i].VideoFile}'," +
                          $"'{opdts[i].JobType}','{opdts[i].SeatNo}','{opdts[i].SewingFile}','{opdts[i].BenchmarkTime}'," +
                          $"'{opdts[i].LaborType}','{opdts[i].ComponentId}','{opdts[i].ActionCode}','{opdts[i].ModuleId}'," +
                          $"'{opdts[i].HotSpot}','{opdts[i].ToolId}','{opdts[i].StitchCount}','{opdts[i].OpTimeBalancing}'," +
                          $"'{opdts[i].OpsState}','{opdts[i].TableId}','{opdts[i].LineSerial}','{opdts[i].McId}','{mcPairDate}'," +
                          $"{opdts[i].AssemblyMdl},{opdts[i].FinalAssembly},'{opdts[i].IotType}','{opdts[i].PaintingType}'," +
                          $"'{opdts[i].MaterialType}','{opdts[i].DryingTime}','{opdts[i].Temperature}','{opdts[i].CoolingTime}')";

                if (i != opdts.Count - 1) qValue += ",";
            }

            var q = $@"INSERT INTO `mes`.`t_mx_opdt`
                            (`STYLECODE`,
                            `STYLESIZE`,
                            `STYLECOLORSERIAL`,
                            `REVNO`,
                            `OPREVNO`,
                            `OPSERIAL`,
                            `OPNUM`,
                            `OPGROUP`,
                            `OPNAME`,
                            `FACTORY`,
                            `MACHINETYPE`,
                            `THREADCOLOR`,
                            `OPDESC`,
                            `OPTIME`,
                            `OPPRICE`,
                            `OFFEROPPRICE`,
                            `MACHINECOUNT`,
                            `REMARKS`,
                            `MAXTIME`,
                            `MANCOUNT`,
                            `FILENAME`,
                            `NEXTOP`,
                            `OUTSOURCED`,
                            `X`,
                            `Y`,
                            `IMAGENAME`,
                            `DISPLAYCOLOR`,
                            `PAGE`,
                            `GROUPCOLOR`,
                            `VIDEOFILE`,
                            `JOBTYPE`,
                            `SEATNO`,
                            `SEWINGFILE`,
                            `BENCHMARKTIME`,
                            `LABORTYPE`,
                            `COMPONENTID`,
                            `ACTIONCODE`,
                            `MODULEID`,
                            `HOTSPOT`,
                            `TOOLID`,
                            `STITCHCOUNT`,
                            `OPTIMEBALANCING`,
                            `OPSSTATE`,
                            `TABLEID`,
                            `LINESERIAL`,
                            `MCID`,
                            `MC_PAIR_DATE`,
                            `ASSEMBLYMDL`,
                            `FINALASSEMBLY`,
                            `IOTTYPE`,
                            `PAINTINGTYPE`,
                            `MATERIALTYPE`,
                            `DRYINGTIME`,
                            `TEMPERATURE`,
                            `COOLINGTIME`)
                        VALUES {qValue};";
            return q;
        }

        public async Task<bool> RegisterPlan(Opmt opmt, List<Opdt> opdts, List<Opnt> opnts, List<Optl> optls,
           bool isCopyTool, bool isCopySelectPlan)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                using (var trans = connection.BeginTransaction())
                {
                    try
                    { 
                        if (opmt == null) return false;
                        var addOpmtQuery = AddOpmtQuery(opmt);
                        using (var mySqlCommand = connection.CreateCommand())
                        {
                            mySqlCommand.Transaction = trans;
                            mySqlCommand.CommandText = addOpmtQuery;
                            mySqlCommand.CommandType = CommandType.Text;
                            var addOpmtResult = await mySqlCommand.ExecuteNonQueryAsync();
                            if (addOpmtResult > 0)
                            {
                                if (isCopySelectPlan)
                                {
                                    if (opdts != null && opdts.Count > 0)
                                    {
                                        var bulkInsertOpdtQuery = BulkInsertOpdtQuery(opdts);
                                        mySqlCommand.CommandText = bulkInsertOpdtQuery;
                                        var bulkInsertOpdtResult = await mySqlCommand.ExecuteNonQueryAsync();
                                    }

                                    if (opnts != null && opnts.Count > 0)
                                    {
                                        var bulkInsertOpntQuery = BulkInsertOpntQuery(opnts);
                                        mySqlCommand.CommandText = bulkInsertOpntQuery;
                                        var bulkInsertOpntResult = await mySqlCommand.ExecuteNonQueryAsync();
                                    }

                                    //if (isCopyTool)
                                    //{
                                    //    var bulkInsertToolQuery = "";
                                    //    mySqlCommand.CommandText = bulkInsertToolQuery;
                                    //    var bulkInsertToolResult = await mySqlCommand.ExecuteNonQueryAsync();
                                    //}
                                }
                            }

                            trans.Commit();
                            return true;
                        }
                    }
                    catch (Exception)
                    {
                        await connection.CloseAsync();
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Adds the opmt.
        /// </summary>
        /// <param name="opmt">The opmt.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool AddOpmt(Opmt opmt, MySqlConnection connection, MySqlTransaction transaction)
        {
            var ps = new OpsMySqlParams(opmt?.Edition.Substring(0, 1), opmt?.StyleCode, opmt?.StyleSize,
                opmt?.StyleColorSerial, opmt?.RevNo, opmt?.OpRevNo)
            {
                new MySqlParameter("P_OPTIME", opmt?.OpTime),
                new MySqlParameter("P_OPPRICE", opmt?.OpPrice),
                new MySqlParameter("P_MACHINECOUNT", opmt?.MachineCount),
                new MySqlParameter("P_OPCOUNT", opmt?.OpCount),
                new MySqlParameter("P_MANCOUNT", opmt?.ManCount),
                new MySqlParameter("P_FILENAME", opmt?.FileName),
                new MySqlParameter("P_FILENAME2", opmt?.FileName2),
                new MySqlParameter("P_FILEPDF", opmt?.FilePdf),
                new MySqlParameter("P_FILEPDF2", opmt?.FilePdf2),
                new MySqlParameter("P_PDMFILE", opmt?.PdmFile),
                new MySqlParameter("P_PROCESSWIDTH", opmt?.ProcessWidth),
                new MySqlParameter("P_PROCESSHEIGHT", opmt?.ProcessHeight),
                new MySqlParameter("P_LAYOUTFONTSIZE", opmt?.LayoutFontSize),
                new MySqlParameter("P_LANGUAGE", opmt?.Language),
                new MySqlParameter("P_BENCHMARKTIME", opmt?.BenchMarkTime),
                new MySqlParameter("P_REMARKS", opmt?.Remarks),
                new MySqlParameter("P_TARGETOFFERPRICE", opmt?.TargetOfferPrice),
                new MySqlParameter("P_OFFEROPPRICE", opmt?.OfferOpPrice),
                new MySqlParameter("P_REGISTERID", opmt?.RegisterId),
                new MySqlParameter("P_GROUPMODE", opmt?.GroupMode),
                new MySqlParameter("P_CANVASHEIGHT", opmt?.CanvasHeight),
                new MySqlParameter("P_PLANPDF", opmt?.PlanPdf),
                new MySqlParameter("P_COLORTHEME", opmt?.ColorTheme),
                new MySqlParameter("P_FACTORY", opmt?.Factory),
                new MySqlParameter("P_MXPACKAGE", opmt?.MxPackage.Trim()),
                new MySqlParameter("P_AFFECTEDROWS", MySqlDbType.Int16) {Direction = ParameterDirection.Output}
            };

            var result = MySqlDBManager.ExecuteQueryWithTrans("SP_MES_INSERT_OPMT", ps.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return result != null;
        }

        public async Task<bool> AddOpmt1(Opmt opmt, MySqlConnection connection, MySqlTransaction transaction)
        {
            var ps = new OpsMySqlParams(opmt?.Edition.Substring(0, 1), opmt?.StyleCode, opmt?.StyleSize,
                opmt?.StyleColorSerial, opmt?.RevNo, opmt?.OpRevNo)
            {
                new MySqlParameter("P_OPTIME", opmt?.OpTime),
                new MySqlParameter("P_OPPRICE", opmt?.OpPrice),
                new MySqlParameter("P_MACHINECOUNT", opmt?.MachineCount),
                new MySqlParameter("P_OPCOUNT", opmt?.OpCount),
                new MySqlParameter("P_MANCOUNT", opmt?.ManCount),
                new MySqlParameter("P_FILENAME", opmt?.FileName),
                new MySqlParameter("P_FILENAME2", opmt?.FileName2),
                new MySqlParameter("P_FILEPDF", opmt?.FilePdf),
                new MySqlParameter("P_FILEPDF2", opmt?.FilePdf2),
                new MySqlParameter("P_PDMFILE", opmt?.PdmFile),
                new MySqlParameter("P_PROCESSWIDTH", opmt?.ProcessWidth),
                new MySqlParameter("P_PROCESSHEIGHT", opmt?.ProcessHeight),
                new MySqlParameter("P_LAYOUTFONTSIZE", opmt?.LayoutFontSize),
                new MySqlParameter("P_LANGUAGE", opmt?.Language),
                new MySqlParameter("P_BENCHMARKTIME", opmt?.BenchMarkTime),
                new MySqlParameter("P_REMARKS", opmt?.Remarks),
                new MySqlParameter("P_TARGETOFFERPRICE", opmt?.TargetOfferPrice),
                new MySqlParameter("P_OFFEROPPRICE", opmt?.OfferOpPrice),
                new MySqlParameter("P_REGISTERID", opmt?.RegisterId),
                new MySqlParameter("P_GROUPMODE", opmt?.GroupMode),
                new MySqlParameter("P_CANVASHEIGHT", opmt?.CanvasHeight),
                new MySqlParameter("P_PLANPDF", opmt?.PlanPdf),
                new MySqlParameter("P_COLORTHEME", opmt?.ColorTheme),
                new MySqlParameter("P_FACTORY", opmt?.Factory),
                new MySqlParameter("P_MXPACKAGE", opmt?.MxPackage.Trim()),
                new MySqlParameter("P_AFFECTEDROWS", MySqlDbType.Int16) {Direction = ParameterDirection.Output}
            };

            var result = await _MySqlDBManager.ExecuteWithTransAsync("SP_MES_INSERT_OPMT", ps.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return result != null;
        }

        public async Task<bool> AddOpmtAsync(Opmt opmt, MySqlConnection connection, MySqlTransaction transaction)
        {
            if (opmt == null) return false;
            var q = $@"INSERT INTO `mes`.`t_mx_opmt`
                            (`STYLECODE`,
                            `STYLESIZE`,
                            `STYLECOLORSERIAL`,
                            `REVNO`,
                            `OPREVNO`,
                            `OPTIME`,
                            `OPPRICE`,
                            `MACHINECOUNT`,
                            `CONFIRMCHK`,
                            `OPCOUNT`,
                            `MANCOUNT`,
                            `FILENAME`,
                            `FILENAME2`,
                            `FILEPDF`,
                            `FILEPDF2`,
                            `PDMFILE`,
                            `PROCESSWIDTH`,
                            `PROCESSHEIGHT`,
                            `PLANPDF`,
                            `COLORTHEME`,
                            `REMARKS`,
                            `LAYOUTFONTSIZE`,
                            `LANGUAGE`,
                            `LAST_UPDATED_TIME`,
                            `BENCHMARKTIME`,
                            `FACTORY`,                            
                            `GROUPMODE`,
                            `CANVASHEIGHT`,
                            `MXPACKAGE`,
                            `REGISTERID`,
                            `REGISTRYDATE`)
                            VALUES ('{opmt.StyleCode}',
                            '{opmt.StyleSize}',
                            '{opmt.StyleColorSerial}',
                            '{opmt.RevNo}',
                            '{opmt.OpRevNo}',
                            '{opmt.OpTime}>,
                            '{opmt.OpPrice}>,
                            '{opmt.MachineCount}>,
                            '{opmt.ConfirmChk}',
                            '{opmt.OpCount}>,
                            '{opmt.ManCount}>,
                            '{opmt.FileName}',
                            '{opmt.FileName2}',
                            '{opmt.FilePdf}',
                            '{opmt.FilePdf2}',
                            '{opmt.PdmFile}',
                            '{opmt.ProcessWidth}',
                            '{opmt.ProcessHeight}',
                            '{opmt.PlanPdf}',
                            '{opmt.ColorTheme}',
                            '{opmt.Remarks}',
                            '{opmt.LayoutFontSize}',
                            '{opmt.Language}',
                            '{opmt.LastUpdateTime}',
                            '{opmt.BenchMarkTime}',
                            '{opmt.Factory}',
                            '{opmt.GroupMode}',
                            '{opmt.CanvasHeight}',
                            '{opmt.MxPackage}',
                            '{opmt.RegisterId}',
                            '{opmt.RegistryDate}')";

            var result = await _MySqlDBManager.ExecuteWithTransAsync(q, null, CommandType.Text, transaction, connection);

            return result != null;
        }

        /// <summary>
        /// Counts the opdt.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColorSerial">The style color serial.</param>
        /// <param name="revNo">The rev no.</param>
        /// <param name="opRevNo">The op rev no.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static Opmt CountOpdt(string styleCode, string styleSize, string styleColorSerial, string revNo,
            string opRevNo)
        {
            var mySqlParams = new OpsMySqlParams(styleCode, styleSize, styleColorSerial, revNo, opRevNo);
            var opmts = MySqlDBManager.GetAll<Opmt>("SP_MES_COUNT_OPDT", CommandType.StoredProcedure, mySqlParams.ToArray());

            return opmts.FirstOrDefault();
        }

        /// <summary>
        /// Counts the operation plan.
        /// </summary>
        /// <param name="edition">The edition.</param>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColorSerial">The style color serial.</param>
        /// <param name="revNo">The rev no.</param>
        /// <param name="opRevNo">The op rev no.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static Opmt CountOperationPlan(string edition, string styleCode, string styleSize, string styleColorSerial,
            string revNo, string opRevNo)
        {
            //Get operation master by id
            var opmtId = new Opmt
            {
                Edition = edition,
                StyleCode = styleCode,
                StyleSize = styleSize,
                StyleColorSerial = styleColorSerial,
                RevNo = revNo,
                OpRevNo = opRevNo
            };
            var opmt = GetOpmt(opmtId);

            //Count operation plan.
            var opmtCount = CountOpdt(styleCode, styleSize, styleColorSerial, revNo, opRevNo);
            if (opmt != null)
            {
                if (opmtCount == null)
                {
                    opmt.MachineCount = 0;
                    opmt.ManCount = 0;
                    opmt.OpCount = 0;
                    opmt.OpTime = 0;
                }
                else
                {
                    //Calculate tack time.                             
                    var tackTime = (int)OpdtBus.GetTackTime(opmt);
                    if (opmt.OpTime > tackTime) { opmt.OpTime = tackTime; }
                    opmt.MachineCount = (int)opmtCount.SumMachineCount;
                    if (opmtCount.SumManCount != null) opmt.ManCount = (int)opmtCount.SumManCount;
                    opmt.OpCount = (int)opmtCount.SumOpCount;
                    opmt.OpTime = tackTime;
                }
            }

            return opmt;
        }

        /// <summary>
        /// Updates the operation master.
        /// </summary>
        /// <param name="opmt">The operation master.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool UpdateOpmt(Opmt opmt)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    var arParam = new MySqlParameter("p_affectedrows", MySqlDbType.Int16)
                    {
                        Direction = ParameterDirection.Output
                    };
                    var ps = new OpsMySqlParams(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo,
                        opmt.OpRevNo)
                    {
                        arParam,
                        new MySqlParameter("P_OPTIME", opmt.OpTime),
                        new MySqlParameter("P_OPPRICE", opmt.OpPrice),
                        new MySqlParameter("P_MACHINECOUNT", opmt.MachineCount),
                        new MySqlParameter("P_CONFIRMCHK", opmt.ConfirmChk),
                        new MySqlParameter("P_OPCOUNT", opmt.OpCount),
                        new MySqlParameter("P_MANCOUNT", opmt.ManCount),
                        new MySqlParameter("P_FILENAME", opmt.FileName),
                        new MySqlParameter("P_FILENAME2", opmt.FileName2),
                        new MySqlParameter("P_FILEPDF", opmt.FilePdf),
                        new MySqlParameter("P_FILEPDF2", opmt.FilePdf2),
                        new MySqlParameter("P_PDMFILE", opmt.PdmFile),
                        new MySqlParameter("P_PROCESSWIDTH", opmt.ProcessWidth),
                        new MySqlParameter("P_PROCESSHEIGHT", opmt.ProcessHeight),
                        new MySqlParameter("P_LAYOUTFONTSIZE", opmt.LayoutFontSize),
                        new MySqlParameter("P_LANGUAGE", opmt.Language),
                        new MySqlParameter("P_LASTUPDATEDTIME", DateTime.Now),
                        new MySqlParameter("P_BENCHMARKTIME", opmt.BenchMarkTime),
                        new MySqlParameter("P_REMARKS", opmt.Remarks),
                        new MySqlParameter("P_GROUPMODE", opmt.GroupMode),
                        new MySqlParameter("P_CANVASHEIGHT", opmt.CanvasHeight),
                        new MySqlParameter("P_PLANPDF", opmt.PlanPdf),
                        new MySqlParameter("P_COLORTHEME", opmt.ColorTheme),
                        new MySqlParameter("P_FACTORY", opmt.Factory),
                        new MySqlParameter("P_MXPACKAGE", opmt.MxPackage)
                    };
                    var result = MySqlDBManager.ExecuteQueryWithTrans("SP_MES_UPDATE_OPMT", ps.ToArray(),
                        CommandType.StoredProcedure, trans, connection);

                    trans.Commit();
                    return result != null;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Updates the layout.
        /// </summary>
        /// <param name="opmt">The opmt.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool UpdateLayout(Opmt opmt, MySqlConnection connection, MySqlTransaction transaction)
        {
            var ps = new OpsMySqlParams(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo)
            {
                new MySqlParameter("P_LANGUAGE", opmt.Language),
                new MySqlParameter("P_GROUPMODE", opmt.GroupMode),
                new MySqlParameter("P_FACTORY", opmt.Factory),
                new MySqlParameter("P_PROCESSWIDTH", opmt.ProcessWidth),
                new MySqlParameter("P_PROCESSHEIGHT", opmt.ProcessHeight),
                new MySqlParameter("P_LAYOUTFONTSIZE", opmt.LayoutFontSize),
                new MySqlParameter("P_CANVASHEIGHT", opmt.CanvasHeight),
                new MySqlParameter("P_REMARKS", opmt.Remarks)
            };
            var result = MySqlDBManager.ExecuteQueryWithTrans("SP_MES_UPDATELAYOUT_OPMT", ps.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return result != null;
        }

        /// <summary>
        /// Gets the maximum oprev number.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColorSerial">The style color serial.</param>
        /// <param name="revNo">The rev no.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 20-Jul-19
        public static int GetMaxOpRevNo(string styleCode, string styleSize, string styleColorSerial,
            string revNo)
        {
            var oracleParams = new OpsMySqlParams(styleCode, styleSize, styleColorSerial, revNo);
            var opmts = MySqlDBManager.GetAll<Opmt>("SP_MES_GETMAXOPREVNO_OPDT", CommandType.StoredProcedure,
                oracleParams.ToArray()).FirstOrDefault();

            if (opmts != null) return int.Parse(opmts.MaxOpRevNo ?? "0") + 1;

            return 0;
        }

        /// <summary>
        /// Updates the op time.
        /// </summary>
        /// <param name="opdt">The opdt.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool UpdateOpTime(Opdt opdt, MySqlConnection connection, MySqlTransaction transaction)
        {
            var oracleParams = new OpsMySqlParams(opdt.Edition, opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial,
                opdt.RevNo, opdt.OpRevNo)
            {
                new MySqlParameter("P_OPTIME", opdt.TackTime)
            };

            var resUpdate = MySqlDBManager.ExecuteQueryWithTrans("SP_MES_UPDATEOPTIME_OPMT", oracleParams.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return resUpdate != null;
        }

        /// <summary>
        /// Confirms the operation plan.
        /// </summary>
        /// <param name="opmt">The opmt.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool ConfirmOperationPlan(Opmt opmt)
        {
            var oracleParams = new List<MySqlParameter>
            {
                new MySqlParameter("P_STYLECODE", opmt.StyleCode),
                new MySqlParameter("P_STYLESIZE", opmt.StyleSize),
                new MySqlParameter("P_STYLECOLORSERIAL", opmt.StyleColorSerial),
                new MySqlParameter("P_REVNO", opmt.RevNo),
                new MySqlParameter("P_OPREVNO", opmt.OpRevNo),
                new MySqlParameter("P_CONFIRMCHK", opmt.ConfirmChk)
            };

            var confirmed = MySqlDBManager.ExecuteNonQuery("SP_MES_CONFIRM_OPMT", CommandType.StoredProcedure,
                oracleParams.ToArray());

            return confirmed != null;
        }

        /// <summary>
        /// Deletes the specified opmt.
        /// </summary>
        /// <param name="opmt">The opmt.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool Delete(Opmt opmt, MySqlTransaction transaction, MySqlConnection connection)
        {
            var oracleParams = new List<MySqlParameter>
            {
                new MySqlParameter("P_STYLECODE", opmt.StyleCode),
                new MySqlParameter("P_STYLESIZE", opmt.StyleSize),
                new MySqlParameter("P_STYLECOLORSERIAL", opmt.StyleColorSerial),
                new MySqlParameter("P_REVNO", opmt.RevNo),
                new MySqlParameter("P_OPREVNO", opmt.OpRevNo)
            };

            var result = MySqlDBManager.ExecuteQueryWithTrans("SP_MES_DELETE_OPMT", oracleParams.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return result != null;
        }

        /// <summary>
        /// Deletes the operation plan.
        /// </summary>
        /// <param name="opmt">The opmt.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 20-Jun-19
        public static bool DeleteOperationPlan(Opmt opmt)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    if (OptlBus.DeleteByOpmt(opmt, transaction, connection) &&
                        OpntBus.DeleteByOpmt(opmt, connection, transaction) &&
                        //ProtBus.DeletePatternBomByOpmt(opmt, trans, connection) &&
                        OpdtBus.DeleteByOpmt(opmt, transaction, connection))
                    {
                        //Delete op master
                        if (!Delete(opmt, transaction, connection))
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                    else
                    {
                        transaction.Rollback();
                        return false;
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        #endregion
    }
}
