using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using OPS_Utils;
using System.Collections.Generic;
using System.Data;

namespace OPS_DAL.MesBus
{
    /// <summary>
    /// Operation layout simulation business class
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    /// Created Date: 12-Nov-19
    public class OplsBus
    {
        /// <summary>
        /// Saves the table links.
        /// </summary>
        /// <param name="opls">The operation layout simulation link.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 12-Nov-19
        public static bool SaveLinks(Opls opls)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();

                var v = $"('{opls.MxPackage}', {opls.FromTable}, {opls.ToTable})";
                var q = $"Insert into t_mx_opls(`MXPACKAGE` , `fromtable` , `totable`) values{v};";

                using (var myCmd = new MySqlCommand(q, connection))
                {
                    myCmd.CommandType = CommandType.Text;
                    myCmd.ExecuteNonQuery();
                }

                return true;
            }
        }

        /// <summary>
        /// Gets the links by mes package id.
        /// </summary>
        /// <param name="mxPackage">The mes package identify.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 12-Nov-19
        public static List<Opls> GetLinksByMxPackage(string mxPackage)
        {
            var q = "SELECT * FROM `MES`.`T_MX_OPLS` WHERE MxPackage = ?P_MxPackge;";
            var ps = new List<MySqlParameter> { new MySqlParameter("P_MxPackge", mxPackage) };

            return MySqlDBManager.GetAll<Opls>(q, CommandType.Text, ps.ToArray());
        }

        /// <summary>
        /// Deletes the line-link.
        /// </summary>
        /// <param name="link">The link of line flow.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 13-Nov-19
        public static bool DeleteLink(Opls link)
        {
            var q = "DELETE FROM `MES`.`T_MX_OPLS` WHERE MxPackage = ?P_MxPackge AND FromTable = ?P_FromTable AND ToTable = ?P_ToTable;";
            var ps = new List<MySqlParameter>
            {
                new MySqlParameter("P_MxPackge", link.MxPackage),
                new MySqlParameter("P_FromTable", link.FromTable),
                new MySqlParameter("P_ToTable", link.ToTable)
            };
            var result = MySqlDBManager.ExecuteQuery(q, CommandType.Text, ps.ToArray());

            return result != null;
        }
    }
}
