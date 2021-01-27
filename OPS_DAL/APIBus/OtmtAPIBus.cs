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
    public class OtmtAPIBus
    {
        #region Get Tools and machine for API
        /// <summary>
        /// Get list of machine or tool by category id
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public static List<OtmtAPI> GetMachineTools(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId)) return new List<OtmtAPI>();

            string strSql = @"SELECT * FROM T_OP_OTMT WHERE TRIM(CATEGID) = :P_CATEGID AND ( ACTIVE = '" + ConstantGeneric.Active + "' OR ACTIVE IS NULL) ";

            var oraParam = new List<OracleParameter>()
            {
                new OracleParameter("P_CATEGID", categoryId.ToUpper())
            };

            var machineList = OracleDbManager.GetObjects<OtmtAPI>(strSql, CommandType.Text, oraParam.ToArray());

            return machineList;
        }

        /// <summary>
        /// Get machine or tool by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static OtmtAPI GetMachineToolById(string id)
        {
            if (string.IsNullOrEmpty(id)) return new OtmtAPI();

            string strSql = @"SELECT * FROM T_OP_OTMT WHERE ITEMCODE= :P_ITEMCODE AND ( ACTIVE = '" + ConstantGeneric.Active + "' OR ACTIVE IS NULL) ";

            var oraParam = new List<OracleParameter>()
            {
                new OracleParameter("P_ITEMCODE", id.ToUpper())
            };

            var item = OracleDbManager.GetObjects<OtmtAPI>(strSql, CommandType.Text, oraParam.ToArray()).FirstOrDefault();

            return item;
        }

        #endregion
    }
}
