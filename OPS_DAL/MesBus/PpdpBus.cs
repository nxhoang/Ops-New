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
    public class PpdpBus
    {
        /// <summary>
        /// Get production remain qty
        /// </summary>
        /// <param name="factoryId"></param>
        /// <param name="prdPkg"></param>
        /// <returns></returns>
        /// Author: Nguyen Cao Son
        public static Ppdp GetPPRemainQuantity(string factoryId, string prdPkg)
        {
            string strSql = @"SELECT * 
                                FROM V_AO_PPDP 
                                WHERE FACTORY = :P_FACTORY AND PRDPKG = :P_PRDPKG  ";
            
            var oraParams = new List<OracleParameter>()
            {
                new OracleParameter("P_FACTORY", factoryId),
                new OracleParameter("P_PRDPKG", prdPkg)
            };
           
            var ppdp = OracleDbManager.GetObjects<Ppdp>(strSql, CommandType.Text, oraParams.ToArray()).FirstOrDefault();

            return ppdp;
        }
    }
}
