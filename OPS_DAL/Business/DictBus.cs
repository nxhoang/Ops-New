using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OPS_DAL.Business
{
    /// <summary>
    /// This is operation name business interact with T_SD_OPNM table
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    public class OperationNameBus
    {
        #region MySQL database

        /// <summary>
        /// Gets the by language.
        /// </summary>
        /// <param name="languageId">The language identifier.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 21-Jun-19
        public static List<OperationName> GetByLanguage(string languageId)
        {
            var prs = new List<MySqlParameter>
            {
                new MySqlParameter("P_LANGUAGEID", languageId.ToLower())
            };
            var opNames = MySqlDBManager.GetAll<OperationName>("SP_MES_GETBYLANGUAGE_OPNM", CommandType.StoredProcedure,
                prs.ToArray());

            return opNames;
        }

        /// <summary>
        /// Searches the specified language identifier.
        /// </summary>
        /// <param name="languageId">The language identifier.</param>
        /// <param name="opNameId">The op name identifier.</param>
        /// <param name="opName">Name of the op.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static List<OperationName> Search(string languageId, decimal? opNameId, string opName, int limit, int offset)
        {
            var prs = new List<MySqlParameter>
            {
                new MySqlParameter("P_LANGUAGEID", languageId.ToLower()),
                new MySqlParameter("P_OPNAMEID", opNameId),
                new MySqlParameter("P_OPNAME", opName),
                new MySqlParameter("P_LIMIT", limit),
                new MySqlParameter("P_OFFSET", offset)
            };
            var opNames = MySqlDBManager.GetAll<OperationName>("SP_MES_SEARCH_OPNM", CommandType.StoredProcedure,
                prs.ToArray());

            return opNames;
        }

        /// <summary>
        /// Counts the operation names.
        /// </summary>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 23-Jun-19
        public static int CountOperationNames()
        {
            var opNames = MySqlDBManager.GetAll<OperationName>("SP_MES_COUNT_OPNM", CommandType.StoredProcedure,
                new List<MySqlParameter>().ToArray()).FirstOrDefault();

            return opNames?.TotalRow ?? 0;
        }

        /// <summary>
        /// Gets the by action code.
        /// </summary>
        /// <param name="languageId">The language identifier.</param>
        /// <param name="moduleId">The module identifier.</param>
        /// <param name="actionCode">The action code.</param>
        /// <param name="buyer">The buyer.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 25-Jun-19
        public static List<OperationName> GetByActionCode(string languageId, string moduleId, string actionCode, string buyer)
        {
            var prs = new List<MySqlParameter>
            {
                new MySqlParameter("P_LANGUAGEID", languageId.ToLower()),
                new MySqlParameter("P_MODULEID", moduleId),
                new MySqlParameter("P_ACTIONCODE", actionCode),
                new MySqlParameter("P_BUYER", buyer),
            };
            var opNames = MySqlDBManager.GetAll<OperationName>("SP_MES_GETBYACTIONCODE_OPNM", CommandType.StoredProcedure,
                prs.ToArray());

            return opNames;
        }
        #endregion

        #region Oracle database

        /// <summary>
        /// Gets the name of the op.
        /// </summary>
        /// <param name="languageId">The language identifier.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<OperationName> GetOpName(string languageId)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_LANGUAGEID", languageId.ToLower()),
                cursor
            };
            var opNames = OracleDbManager.GetObjects<OperationName>("SP_OPS_GETOPNAME_OPNM", CommandType.StoredProcedure, oracleParams.ToArray());

            return opNames;
        }

        /// <summary>
        /// Gets the op name by code.
        /// </summary>
        /// <param name="languageId">The language identifier.</param>
        /// <param name="moduleId">The module identifier.</param>
        /// <param name="actionCode">The action code.</param>
        /// <param name="buyer">The buyer.</param>
        /// <returns></returns>
        public static List<OperationName> GetOpNameByCode(string languageId, string moduleId, string actionCode, string buyer)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_LANGUAGEID", languageId.ToLower()),
                new OracleParameter("P_MODULEID", moduleId),
                new OracleParameter("P_ACTIONCODE", actionCode),
                new OracleParameter("P_BUYER", buyer),
                cursor
            };
            var opNames = OracleDbManager.GetObjects<OperationName>("SP_OPS_GETOPNAMEBYCODE_OPNM", CommandType.StoredProcedure, 
                oracleParams.ToArray());

            return opNames;
        }

        #endregion
    }
}
