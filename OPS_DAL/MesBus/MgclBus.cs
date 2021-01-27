using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public  class MgclBus
    {
        public static bool InsertReadinessCheckList(Mgcl mgcl)
        {
            string strSql = @" INSERT INTO t_mx_mgcl 
                                (packagegroup, checklistid, confirmer, confirmtime)
                                VALUES (?p_packagegroup, ?p_checklistid, ?p_confirmer, SYSDATE()) ";

            var par = new List<MySqlParameter>() {
                new MySqlParameter("p_packagegroup", mgcl.PackageGroup),
                new MySqlParameter("p_checklistid", mgcl.CheckListId),
                new MySqlParameter("p_confirmer", mgcl.Confirmer)
            };

            //var result = OracleDbManager.ExecuteQuery(strSql, oraParams.ToArray(), CommandType.Text);
            var result = MySqlDBManager.ExecuteQuery(strSql, CommandType.Text, par.ToArray());

            return result != null;
        }

        public static async Task<IEnumerable<Mgcl>> GetMaterialReadiness(string packageGroup)
        {
            string strSql = @" SELECT *
                                FROM T_MX_Mgcl where packagegroup = ?p_packagegroup; ";

            var par = new List<MySqlParameter>() {
                new MySqlParameter("p_packagegroup", packageGroup)
            };

            var listMgcl = MySqlDBManager.GetAll<Mgcl>(strSql, CommandType.Text, par.ToArray());

            return await Task.FromResult<IEnumerable<Mgcl>>(listMgcl);
        }

    }
}
