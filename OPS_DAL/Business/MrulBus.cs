using OPS_DAL.DAL;
using OPS_DAL.Entities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Business
{
    public class MrulBus
    {
        /// <summary>
        /// Get list of modules level
        /// </summary>
        /// <param name="mrul"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Mrul> GetModulesLevel(Mrul mrul)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            var oracleParams = new List<OracleParameter>
            {
                cursor,
                new OracleParameter("P_STYLEGROUP", mrul.StyleGroup),
                new OracleParameter("P_STYLESUBGROUP", mrul.StyleSubGroup),
                new OracleParameter("P_STYLESUBSUBGROUP", mrul.StyleSubSubGroup),
                new OracleParameter("P_MACHINERANGECODE", mrul.MachineRangeCode)
            };
            var lstModules = OracleDbManager.GetObjects<Mrul>("SP_OPS_GETMODULESLEVEL_MRUL", CommandType.StoredProcedure, oracleParams.ToArray());

            return lstModules;
        }

        /// <summary>
        /// Get list of modules by style group and sub group
        /// </summary>
        /// <param name="stlGroup"></param>
        /// <param name="stlSubGroup"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Mrul> GetModulesStlGroupAndSubGroup(string stlGroup, string stlSubGroup)
        {
            string strSql = @"SELECT * FROM T_CM_MRUL WHERE STYLEGROUP = :P_STYLEGROUP AND STYLESUBGROUP = :P_STYLESUBGROUP ";

            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLEGROUP", stlGroup),
                new OracleParameter("P_STYLESUBGROUP", stlSubGroup)
            };
            var lstModules = OracleDbManager.GetObjects<Mrul>(strSql, CommandType.Text, oracleParams.ToArray());

            return lstModules;
        }

        /// <summary>
        /// Get list of item level master by main level and level no
        /// </summary>
        /// <param name="mainLevel"></param>
        /// <param name="levelNo"></param>
        /// <returns>List item level</returns>
        /// Author: Son Nguyen Cao
        public static List<Mrul> GetModules(string mainLevel, string levelNo)
        {
            string strSql = @"SELECT ICL.LEVELCODE || '-S' AS MODULELEVELCODE, ICL.LEVELDESC 
                                FROM T_00_ICLM ICL WHERE MAINLEVEL = :P_MAINLEVEL AND ICL.LEVELNO = :P_LEVELNO and ICL.LEVELUSE = 'Y' ";

            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_MAINLEVEL", mainLevel),
                new OracleParameter("P_LEVELNO", levelNo)
            };
            var listIclm = OracleDbManager.GetObjects<Mrul>(strSql, CommandType.Text, oracleParams.ToArray());

            return listIclm;
        }

        /// <summary>
        /// Get all modules level
        /// </summary>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        //public static List<Mrul> GetModulesLevel()
        //{
        //    var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
        //    var oracleParams = new List<OracleParameter>
        //    {
        //        cursor
        //    };
        //    var lstModules = OracleDbManager.GetObjects<Mrul>("SP_OPS_GETMODULESLEVEL_ICLM", CommandType.StoredProcedure, oracleParams.ToArray());

        //    return lstModules;
        //}
    }
}
