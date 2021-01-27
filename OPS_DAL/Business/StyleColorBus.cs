using OPS_DAL.DAL;
using OPS_DAL.Entities;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;

namespace OPS_DAL.Business
{
    /// <summary>
    /// The class to handle crud color of style
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    public class StyleColorBus
    {
        #region Oracle database

        public static StyleColorEntity GetStyleColorEntities(string styleCode, string styleColorSerial)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor){ Direction = ParameterDirection.Output }
            };
            var result = OracleDbManager.GetObjects<StyleColorEntity>("SP_OPS_GETSTYLECOLOR_SCMT",
                CommandType.StoredProcedure, oracleParams.ToArray());

            return result.FirstOrDefault();
        }

        #endregion


        #region MySql database

        /// <summary>
        /// Gets the color of the style.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleColorSerial">The style color serial.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 03-Jul-19
        public static StyleColorEntity GetStyleColor(string styleCode, string styleColorSerial)
        {
            var prs = new List<MySqlParameter>
            {
                new MySqlParameter("P_STYLECODE", styleCode),
                new MySqlParameter("P_STYLECOLORSERIAL", styleColorSerial),
            };
            var result = MySqlDBManager.GetAll<StyleColorEntity>("SP_MES_GETSTYLECOLOR_SCMT", CommandType.StoredProcedure,
                prs.ToArray());

            return result.FirstOrDefault();
        }

        #endregion
    }
}
