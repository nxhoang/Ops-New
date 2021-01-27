
using System.Collections.Generic;
using OPS_DAL.Entities;
using OPS_DAL.DAL;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace OPS_DAL.Business
{
    public class IclmBus
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainLevel"></param>
        /// <param name="levelNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Iclm> GetItemLevel(string mainLevel, string levelNo)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            List<OracleParameter> oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_MAINLEVEL", mainLevel),
                new OracleParameter("P_LEVELNO", levelNo),
                cursor
            };
            var lstItemLevel = OracleDbManager.GetObjects<Iclm>("SP_OPS_GETMODULESLEVEL_ICLM", CommandType.StoredProcedure, oracleParams.ToArray());
            return lstItemLevel;
        }
    }
}
