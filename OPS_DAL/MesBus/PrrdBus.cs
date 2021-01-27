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
    public class PrrdBus
    {
        /// <summary>
        /// Insert package readiness
        /// </summary>
        /// <param name="prrd"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertPackageReadiness(Prrd prrd)
        {
            string strSql = @" INSERT INTO PKMES.T_MX_PRRD (PRDPKG, FACTORY, JIG, MOULD, SOP) 
                                VALUES (:P_PRDPKG, :P_FACTORY, :P_JIG, :P_MOULD, :P_SOP) ";

            var param = new List<OracleParameter>()
            {
                new OracleParameter("P_PRDPKG", prrd.PRDPKG),
                new OracleParameter("P_FACTORY", prrd.FACTORY),
                new OracleParameter("P_JIG", prrd.JIG),
                new OracleParameter("P_MOULD", prrd.MOULD),
                new OracleParameter("P_SOP", prrd.SOP)
            };

            var resIns = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text);

            return resIns != null;
        }

        /// <summary>
        /// Get package readiness
        /// </summary>
        /// <param name="prdPkg"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Prrd GetPackageReadiness(string prdPkg)
        {
            string strSql = @" SELECT * FROM PKMES.T_MX_PRRD WHERE PRDPKG = :P_PRDPKG ";

            var param = new List<OracleParameter>()
            {
                new OracleParameter("P_PRDPKG", prdPkg)
            };

            var pkgReadiness = OracleDbManager.GetObjects<Prrd>(strSql, CommandType.Text, param.ToArray()).FirstOrDefault();

            return pkgReadiness;
        }
    }
}
