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
    public class JgrqBus
    {
        /// <summary>
        /// Insert jig request
        /// </summary>
        /// <param name="jigRquest"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertJigRequest(Jgrq jigRquest)
        {
            string strSql = @" INSERT INTO PKMES.T_MX_JGRQ(JIGREQUESTID, PRDPKG, JIGCODE, JIGQTY, REQUESTOR, REQUESTDATE)
                                VALUES (:P_JIGREQUESTID, :P_PRDPKG, :P_JIGCODE, :P_JIGQTY, :P_REQUESTOR, SYSDATE)";

            var param = new List<OracleParameter>()
            {
                new OracleParameter("P_JIGREQUESTID", jigRquest.JIGREQUESTID),
                new OracleParameter("P_PRDPKG", jigRquest.PRDPKG),
                new OracleParameter("P_JIGCODE", jigRquest.JIGCODE),
                new OracleParameter("P_JIGQTY", jigRquest.JIGQTY),
                new OracleParameter("P_REQUESTOR", jigRquest.REQUESTOR)
            };

            var resIns = OracleDbManager.ExecuteQuery(strSql, param.ToArray(), CommandType.Text);

            return resIns != null;
        }

        public static Jgrq GetJigRequest(string prdPkg)
        {
            string strSql = @" SELECT * FROM PKMES.T_MX_PRRD WHERE PRDPKG = :P_PRDPKG ";

            var param = new List<OracleParameter>()
            {
                new OracleParameter("P_PRDPKG", prdPkg)
            };

            var jigRequest = OracleDbManager.GetObjects<Jgrq>(strSql, CommandType.Text, param.ToArray()).FirstOrDefault();

            return jigRequest;
        }
    }
}
