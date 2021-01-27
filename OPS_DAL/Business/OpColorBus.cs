using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;


namespace OPS_DAL.Business
{
    public class OpColorBus
    {
        /// <summary>
        /// Get list of color by theme.
        /// </summary>
        /// <param name="theme"></param>
        /// <returns></returns>
        public static List<OpColor> GetColorByTheme(string theme)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_COLORTHEME", theme),
                new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
            };

            var lstColor = OracleDbManager.GetObjects<OpColor>("SP_OPS_GETCOLORBYTHEME_COLOR", CommandType.StoredProcedure, oracleParams.ToArray());
            return lstColor;
        }

        /// <summary>
        /// Gets the by theme.
        /// </summary>
        /// <param name="theme">The theme.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static List<OpColor> GetByTheme(string theme)
        {
            var prs = new List<MySqlParameter>
            {
                new MySqlParameter("P_COLORTHEME", theme)
            };

            var colour = MySqlDBManager.GetAll<OpColor>("SP_MES_GETBYTHEME_COLOR", CommandType.StoredProcedure,
                prs.ToArray());
            return colour;
        }

        public static List<OpColor> GetColour()
        {
            string q = @"SELECT
                            *
                        FROM
                            t_op_color";

            var colour = OracleDbManager.GetObjects<OpColor>(q, CommandType.Text, null);
            return colour;
        }
    }
}
