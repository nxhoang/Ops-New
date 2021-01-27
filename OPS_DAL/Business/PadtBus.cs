using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;

namespace OPS_DAL.Business
{
    public class PadtBus
    {
        /// <summary>
        /// Get paiting time range
        /// </summary>
        /// <param name="paintingType"></param>
        /// <param name="materialType"></param>
        /// <returns></returns>
        public static List<Padt> GetPaintingTimeRange(string paintingType, string materialType)
        {
            var paintingTypeCon = " AND PAITINGTYPE = :P_PAITINGTYPE";
            var materialTypeCon = " AND MATERIALTYPE = :P_MATERIALTYPE";

            string sql = @"SELECT * FROM T_OP_PADT WHERE 1 = 1 ";

            var oraParam = new List<OracleParameter>(){};

            if (!string.IsNullOrEmpty(paintingType))
            {
                sql += paintingTypeCon;
                oraParam.Add(new OracleParameter("P_PAITINGTYPE", paintingType));
            }

            if (!string.IsNullOrEmpty(materialType))
            {
                sql += materialTypeCon;
                oraParam.Add(new OracleParameter("P_MATERIALTYPE", materialType));
            }

            var ret = OracleDbManager.GetObjects<Padt>(sql, System.Data.CommandType.Text, oraParam.ToArray());
            return ret;
        }

        public static List<Padt> MySqlGetPaintingTimeRange(string paintingType, string materialType)
        {
            var paintingTypeCon = " AND PAITINGTYPE = :P_PAITINGTYPE";
            var materialTypeCon = " AND MATERIALTYPE = :P_MATERIALTYPE";

            string sql = @"SELECT * FROM T_OP_PADT WHERE 1 = 1 ";

            var oraParam = new List<MySqlParameter>();

            if (!string.IsNullOrEmpty(paintingType))
            {
                sql += paintingTypeCon;
                oraParam.Add(new MySqlParameter("P_PAITINGTYPE", paintingType));
            }

            if (!string.IsNullOrEmpty(materialType))
            {
                sql += materialTypeCon;
                oraParam.Add(new MySqlParameter("P_MATERIALTYPE", materialType));
            }

            var ret = MySqlDBManager.GetAll<Padt>(sql, System.Data.CommandType.Text, oraParam.ToArray());
            return ret;
        }
    }
}
