using OPS_DAL.APIEntities;
using OPS_DAL.DAL;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.APIBus
{
    public class OpdtAPIBus
    {
        /// <summary>
        /// Get opeartion plan detail
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="edition"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<OpdtAPI> GetOperationPlanDetail(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string edition)
        {
            //Get table name base on edition
            var tbNameOpdt = CommonMethod.GetTableNameDetailByEdition(edition.ToUpper());
            var tbNameOpmt = CommonMethod.GetTableNameMasterByEdition(edition.ToUpper());

            //Return empty list if table is empty
            if (string.IsNullOrEmpty(tbNameOpdt) || string.IsNullOrEmpty(tbNameOpdt)) return new List<OpdtAPI>();

            //Check opeartion revision
            string opRevNoCon = string.IsNullOrWhiteSpace(opRevNo) ? " and 1 = 1 " : " and opm.oprevno = :P_OPREVNO";

            string strSql = $@"select  opd.stylecode, opd.stylesize, opd.stylecolorserial, opd.revno, opd.oprevno, opd.opserial, opd.opnum, opd.opgroup, opd.opname 
                            , OPD.FACTORY, OPD.MACHINETYPE, opd.optime, opd.opprice, OPD.OFFEROPPRICE, opd.machinecount, opd.remarks, opd.maxtime, opd.mancount, OPD.NEXTOP
                            , opd.outsourced, opd.jobtype, opd.benchmarktime, opd.labortype, opd.moduleid, SAM.MODULENAME, opd.hotspot, opd.toolid, opsstate, opd.optimebalancing
                            , opd.actioncode, opd.stitchcount
                            , case when imagename is not null then 'http://118.69.170.24:8005/ops/ProcessImages/' || imagename else '' end as ProcessImage
                   from {tbNameOpmt} opm 
                        left join {tbNameOpdt} opd on opd.stylecode = opm.stylecode and opm.stylesize = opd.stylesize 
                            and OPM.STYLECOLORSERIAL = opd.stylecolorserial and OPM.REVNO = opd.revno and opm.oprevno = opd.oprevno     
                        LEFT JOIN T_00_SAMT SAM ON SAM.MODULEID = OPD.MODULEID AND SAM.STYLECODE = upper(:P_STYLECODE2)
                    where opm.stylecode = upper(:P_STYLECODE) and opm.stylesize = upper(:P_STYLESIZE) and opm.stylecolorserial = :P_STYLECOLORSERIAL and OPM.REVNO = :P_REVNO {opRevNoCon} ";

            string lastConfirmed = $@"and opm.last_updated_time = (    
                                select max(om.last_updated_time)
                                from {tbNameOpmt} om 
                                where om.stylecode = opm.stylecode and om.stylesize = opm.stylesize and om.stylecolorserial = opm.stylecolorserial 
                                        and OM.REVNO = OPM.REVNO  and OM.CONFIRMCHK = 'Y' 
                                group by om.stylecode, om.stylesize, om.stylecolorserial, OM.REVNO
                            )";

            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE2", styleCode),
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLESIZE", styleSize),
                new OracleParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new OracleParameter("P_REVNO", revNo)
            };

            if (!string.IsNullOrWhiteSpace(opRevNo))
            {
                oracleParams.Add(new OracleParameter("P_OPREVNO", opRevNo));                
            }
            else
            {
                strSql += lastConfirmed;
            }

            var listOpdt = OracleDbManager.GetObjectsByType<OpdtAPI>(strSql, CommandType.Text, oracleParams.ToArray());

            return listOpdt;
        }
    }
}
