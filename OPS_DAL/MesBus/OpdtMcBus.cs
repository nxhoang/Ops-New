using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public class OpdtMcBus
    {
        /// <summary>
        /// Get list of machine scan MES package
        /// </summary>
        /// <param name="yyyy"></param>
        /// <param name="mm"></param>
        /// <returns></returns>
        public static List<OpdtMc> GetMachineScanMesPkg(string factory, string yyyy, string mm, string dd)
        {
            string strSql = @" select distinct opd.mcid
                                from t_mx_opdt_mc opd
	                                join t_mx_opmt opm on opm.STYLECODE = opd.stylecode and opm.stylesize = opd.STYLESIZE and opm.STYLECOLORSERIAL = opd.STYLECOLORSERIAL and opd.REVNO = opm.REVNO
	                                join t_mx_mpdt mp on mp.MXPACKAGE = opm.MXPACKAGE
                                where mp.FACTORY = ?P_FACTORY and  YEAR(opd.LAST_IOT_DATA_RECEIVE_TIME) = ?P_YYYY and MONTH(opd.LAST_IOT_DATA_RECEIVE_TIME) = ?P_MM
                                        and DAY(opd.LAST_IOT_DATA_RECEIVE_TIME) = ?P_DD ;  ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_FACTORY", factory),
                new MySqlParameter("P_YYYY", yyyy),
                new MySqlParameter("P_MM", mm),
                new MySqlParameter("P_DD", dd)
            };

            var listOpdtMc = MySqlDBManager.GetObjects<OpdtMc>(strSql, CommandType.Text, param.ToArray());

            return listOpdtMc;

        }

        /// <summary>
        /// Get scaned process
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="startDate"></param>
        /// <param name="mesPkg"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<OpdtMc> GetScannedProcess(string factory, string startDate, string mesPkg)
        {
            string strSql = @" select opd.STYLECODE, OPD.STYLESIZE, OPD.STYLECOLORSERIAL, OPD.REVNO, OPD.OPREVNO, OPD.OPSERIAL, OPD.MCID, OPD.IOT_MODULE_MAC, OPD.MC_PAIR_TIME
                                from  t_mx_mpdt mpd
                                    join t_mx_opmt opm on opm.MXPACKAGE = mpd.MXPACKAGE
                                    join t_mx_opdt_mc opd on opd.stylecode = opm.STYLECODE and opd.STYLECOLORSERIAL = opm.STYLECOLORSERIAL and opd.STYLESIZE = opm.STYLESIZE and opd.REVNO = opm.REVNO and opd.OPREVNO = opm.OPREVNO
                                where  mpd.factory = ?P_FACTORY and mpd.PLNSTARTDATE = ?P_PLNSTARTDATE and mpd.MXPACKAGE = ?P_MXPACKAGE; ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_FACTORY", factory),
                new MySqlParameter("P_PLNSTARTDATE", startDate),
                new MySqlParameter("P_MXPACKAGE", mesPkg)
            };

            var listOpdtMc = MySqlDBManager.GetObjects<OpdtMc>(strSql, CommandType.Text, param.ToArray());

            return listOpdtMc;

        }
    }
}
