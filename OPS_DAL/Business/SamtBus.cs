using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OPS_DAL.Business
{
    public class SamtBus
    {
        #region Oracle database

        /// <summary>
        /// Get list of module include new and old
        /// </summary>
        /// <param name="styleCode"></param>
        /// <returns></returns>
        public static List<Samt> GetModules(string styleCode)
        {
            var buyerCode = styleCode.Substring(0, 3);
            string strSql = @"SELECT SAM.STYLECODE, SAM.MODULEID, SAM.MODULENAME, ICL.LEVELUSE ISNEW
                                FROM T_00_SAMT SAM 
                                LEFT JOIN T_00_ICMT ICM ON ICM.ITEMCODE = SAM.MODULEID
                                LEFT JOIN T_00_ICLM ICL ON ICL.LEVELCODE = ICM.LEVELNO_01 AND ICL.LEVELNO = '01' AND ICL.MAINLEVEL = 'SUB'
                                WHERE SAM.STYLECODE = :P_STYLECODE AND ICM.BUYER = :P_BUYER AND ICM.MAINLEVEL = 'SUB' ";

            List<OracleParameter> oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_BUYER", buyerCode)
            };
            var listModule = OracleDbManager.GetObjectsByType<Samt>(strSql, CommandType.Text, oracleParams.ToArray());

            return listModule;
        }

        /// <summary>
        /// Get new modules list
        /// </summary>
        /// <param name="styleCode"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        /// Date: 15/Jul/2020
        public static List<Samt> GetNewModules(string styleCode)
        {
            var buyerCode = styleCode.Substring(0, 3);
            string strSql = @"SELECT T3.STYLECODE, T3.ITEMCODE MODULEID, T3.ITEMNAME MODULENAME   FROM (
                            SELECT T1.*, T2.MODULEID, T2.STYLECODE FROM (
                                SELECT ICM.ITEMCODE, ICM.ITEMNAME, ICM.MAINLEVEL, ICM.LEVELNO_01
                                FROM T_00_ICMT ICM 
                                JOIN T_00_ICLM ICL ON ICL.LEVELCODE = ICM.LEVELNO_01
                                WHERE ICM.BUYER = :P_BUYER AND ICM.MAINLEVEL = 'SUB' AND ICL.MAINLEVEL = 'SUB' AND ICL.LEVELNO = '01' AND ICL.LEVELUSE = 'Y'
                            )T1 LEFT JOIN (
                                SELECT SAM.STYLECODE, SAM.MODULEID, SAM.MODULENAME, ICM.LEVELNO_01 , ICL.LEVELUSE
                                FROM T_00_SAMT SAM
                                    LEFT JOIN T_00_ICMT ICM ON ICM.ITEMCODE = SAM.MODULEID
                                    LEFT JOIN T_00_ICLM ICL ON ICL.LEVELCODE = ICM.LEVELNO_01
                                WHERE STYLECODE = :P_STYLECODE  AND ICL.MAINLEVEL = 'SUB' AND ICL.LEVELNO = '01' AND ICL.LEVELUSE = 'Y'
                            )T2 ON T2.MODULEID = T1.ITEMCODE
                        )T3 WHERE T3.MODULEID IS NULL";

            List<OracleParameter> oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_BUYER", buyerCode),
                new OracleParameter("P_STYLECODE", styleCode)

            };
            var listModule = OracleDbManager.GetObjectsByType<Samt>(strSql, CommandType.Text, oracleParams.ToArray());

            return listModule;
        }

        /// <summary>
        /// Insert module
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertModule(Samt module, OracleConnection oraConn, OracleTransaction trans)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) {Direction = ParameterDirection.Output},
                new OracleParameter("P_STYLECODE", module.StyleCode),
                new OracleParameter("P_MODULEID", module.ModuleId),
                new OracleParameter("P_MODULENAME", module.ModuleName),
                new OracleParameter("P_REGISTRAR", module.Registrar),
                new OracleParameter("P_REGISTRYDATE", module.RegistryDate),
                new OracleParameter("P_FINALASSEMBLY", module.FinalAssembly),
                new OracleParameter("P_CONFIRMED", module.Confirmed),
                new OracleParameter("P_PARTID", module.PartId)
            };

            var resInsert = OracleDbManager.ExecuteQuery("SP_OPS_INSERTMODULE_SAMT", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resInsert != null && int.Parse(resInsert.ToString()) != 0;

        }

        /// <summary>
        /// Insert list of modules
        /// </summary>
        /// <param name="lstModule"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static bool InsertModulesList(List<Samt> lstModule)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    foreach (var samt in lstModule)
                    {
                        //Insert module
                        if (InsertModule(samt, connection, trans)) continue;

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
        /// Get list of modules
        /// </summary>
        /// <param name="styleCode"></param>
        /// <returns></returns>
        public static List<Samt> GetModules(string styleCode, int pageIndex, int pageSize)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_PAGEINDEX", pageIndex),
                new OracleParameter("P_PAGESIZE", pageSize),
                cursor
            };
            var lstModules = OracleDbManager.GetObjects<Samt>("SP_OPS_GETMODULES_SAMT", CommandType.StoredProcedure, oracleParams.ToArray());

            return lstModules;
        }

        /// <summary>
        /// Get module by key code
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static List<Samt> GetModulesByCode(string styleCode, string moduleId)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_MODULEID", moduleId),
                cursor
            };
            var lstModules = OracleDbManager.GetObjects<Samt>("SP_OPS_GETMODULESBYCODE_SAMT", CommandType.StoredProcedure, oracleParams.ToArray());

            return lstModules;
        }

        /// <summary>
        /// Get samt by key styleCode
        /// </summary>
        /// <param name="styleCode"></param>
        /// <returns></returns>
        /// Author: VitHV.
        public static List<Samt> GetModulesByCode(string styleCode)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", styleCode),
                cursor
            };
            var lstModules = OracleDbManager.GetObjects<Samt>("SP_OPS_GETSAMTBYCODE_SAMT", CommandType.StoredProcedure, oracleParams.ToArray());

            return lstModules;
        }

        /// <summary>
        /// Get samt by key styleCode
        /// </summary>
        /// <returns></returns>
        /// Author: VitHV.
        public static List<Samt> GetAllModules()
        {
            var lstModules = OracleDbManager.GetObjects<Samt>("SELECT MODULEID, MODULENAME FROM T_00_SAMT", null);
            return lstModules;
        }

        /// <summary>
        /// Delete module
        /// </summary>
        /// <param name="samt"></param>
        /// <param name="trans"></param>
        /// <param name="oraConn"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool DeleteModule(Samt samt, OracleTransaction trans, OracleConnection oraConn)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) {Direction = ParameterDirection.Output},
                new OracleParameter("P_STYLECODE", samt.StyleCode),
                new OracleParameter("P_MODULEID", samt.ModuleId)
            };

            var resDel = OracleDbManager.ExecuteQuery("SP_OPS_DELETEMODULE_SAMT", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resDel != null && int.Parse(resDel.ToString()) != 0;

        }

        /// <summary>
        /// Delete a list module.
        /// </summary>
        /// <param name="lstSamt"></param>
        /// <returns></returns>
        public static bool DeleteModulesList(List<Samt> lstSamt)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    foreach (var samt in lstSamt)
                    {
                        if (DeleteModule(samt, trans, connection)) continue;

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
        /// Get max part id of module
        /// </summary>
        /// <param name="styleCode"></param>
        /// <returns></returns>
        public static int GetMaxPartIdBuyer(string styleCode)
        {
            var strSql = @" SELECT NVL(MAX(PARTID), 0) MAXPARTID FROM T_00_SAMT WHERE STYLECODE = :P_STYLECODE AND PARTID != 99 ";

            List<OracleParameter> oraParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", styleCode)
            };

            var lstIcmt = OracleDbManager.GetObjects<Samt>(strSql, CommandType.Text, oraParams.ToArray()).FirstOrDefault();

            return lstIcmt.MaxPartId == "0" ? 0 : int.Parse(lstIcmt.MaxPartId);
        }

        /// <summary>
        /// Update module comment
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="moduleId"></param>
        /// <param name="partComment"></param>
        /// <param name="trans"></param>
        /// <param name="oraConn"></param>
        /// <returns></returns>
        private static bool UpdateModuleComment(string styleCode, string moduleId, string partComment, OracleTransaction trans, OracleConnection oraConn)
        {
            //START MOD - SON) 8/Sep/2020            
            var strSql = @" UPDATE T_00_SAMT SET PARTCOMMENT = :P_PARTCOMMENT WHERE STYLECODE = :P_STYLECODE AND MODULEID = :P_MODULEID ";
            
            //var strSql = @" UPDATE T_00_SAMT SET PARTCOMMENT = :P_PARTCOMMENT, SUBGROUP = :P_SUBGROUP WHERE STYLECODE = :P_STYLECODE AND MODULEID = :P_MODULEID ";
            //END MOD - SON) 8/Sep/2020

            List<OracleParameter> oraParams = new List<OracleParameter>
            {
                new OracleParameter("P_PARTCOMMENT", partComment),
                //new OracleParameter("P_SUBGROUP", subGroup), //ADD - SON) 8/Sep/2020
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_MODULEID", moduleId)
            };

            var resUpd = OracleDbManager.ExecuteQuery(strSql, oraParams.ToArray(), CommandType.Text, trans, oraConn);

            return resUpd != null && int.Parse(resUpd.ToString()) != 0;
        }

        /// <summary>
        /// Update list of module comment
        /// </summary>
        /// <param name="listModule"></param>
        /// <returns></returns>
        public static bool UpdateModulesCommentList(List<Samt> listModule)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    foreach (var mdl in listModule)
                    {
                        if (UpdateModuleComment(mdl.StyleCode, mdl.ModuleId, mdl.PartComment, trans, connection)) continue; //MOD - SON) 8/Sep/2020 - Add mdl.SubGroup

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

        #endregion

        #region MySql database               

        /// <summary>
        /// Gets the by code.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="moduleId">The module identifier.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 05-Jul-19
        public static List<Samt> GetByCode(string styleCode, string moduleId)
        {
            var prs = new List<MySqlParameter>
            {
                new MySqlParameter("P_STYLECODE", styleCode),
                new MySqlParameter("P_MODULEID", moduleId)
            };
            var modules = MySqlDBManager.GetAll<Samt>("SP_MES_GETBYCODE_SAMT", CommandType.StoredProcedure, prs.ToArray());

            return modules;
        }

        /// <summary>
        /// Gets the by style code.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 05-Jul-19
        public static List<Samt> GetByStyleCode(string styleCode)
        {
            var prs = new List<MySqlParameter> { new MySqlParameter("P_STYLECODE", styleCode) };
            var modules = MySqlDBManager.GetAll<Samt>("SP_MES_GETBYSTYLECODE_SAMT", CommandType.StoredProcedure,
                prs.ToArray());

            return modules;
        }

        /// <summary>
        /// Insert SAMT to MES MySQL
        /// </summary>
        /// <param name="samt"></param>
        /// <param name="myTrans"></param>
        /// <param name="myConnection"></param>
        /// <returns></returns>
        internal static bool InserSAMTToMESMySql(Samt samt, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @" INSERT INTO T_00_SAMT(STYLECODE, MODULEID, MODULENAME, REGISTRAR, REGISTRYDATE, FINALASSEMBLY, CONFIRMED, PARTID, PARTCOMMENT)
                                VALUES(?P_STYLECODE, ?P_MODULEID, ?P_MODULENAME, ?P_REGISTRAR, ?P_REGISTRYDATE, ?P_FINALASSEMBLY, ?P_CONFIRMED, ?P_PARTID, ?P_PARTCOMMENT); ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_STYLECODE", samt.StyleCode),
                new MySqlParameter("P_MODULEID", samt.ModuleId),
                new MySqlParameter("P_MODULENAME", samt.ModuleName),
                new MySqlParameter("P_REGISTRAR", samt.Registrar),
                new MySqlParameter("P_REGISTRYDATE", samt.RegistryDate),
                new MySqlParameter("P_FINALASSEMBLY", samt.FinalAssembly),
                new MySqlParameter("P_CONFIRMED", samt.Confirmed),
                new MySqlParameter("P_PARTID", samt.PartId),
                new MySqlParameter("P_PARTCOMMENT", samt.PartComment)
            };

            var blIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, param.ToArray(), CommandType.Text, myTrans, myConnection);

            return blIns != null;
        }

        /// <summary>
        /// Insert list of SAMT
        /// </summary>
        /// <param name="listSamt"></param>
        /// <returns></returns>
        public static bool InsertListSAMTToMESMySql(List<Samt> listSamt)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    foreach (var samt in listSamt)
                    {
                        InserSAMTToMESMySql(samt, myTrans, myConnection);
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
        #endregion
    }
}
