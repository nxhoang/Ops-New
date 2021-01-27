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
    public class MccaBus
    {
        /// <summary>
        /// Get machine categories
        /// </summary>
        /// <param name="opNameId"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Mcca> GetMachineCategories(string opNameId)
        {
            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("p_opnameid", opNameId)
            };

            string strSql = $@"select * from t_op_mcca  where opnameid = :p_opnameid ";

            var listMchCat = OracleDbManager.GetObjects<Mcca>(strSql, CommandType.Text, oraParams.ToArray());

            return listMchCat;
        }
    }
}
