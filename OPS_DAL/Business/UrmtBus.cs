using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using System.Data;

namespace OPS_DAL.Business
{
    public class UrmtBus
    {
        public static List<Urmt> GetListRole(string UserID)
        {
            var sb = new StringBuilder();
            sb.AppendLine(" SELECT");
            sb.AppendLine(" A.USERID,A.ROLEID,B.ROLEDESC ");
            sb.AppendLine(" FROM T_CM_URMT A ");
            sb.AppendLine(" LEFT JOIN T_CM_URLM B ON A.ROLEID = B.ROLEID ");
            sb.AppendLine(" WHERE A.USERID='" + UserID + "'");
            return OracleDbManager.GetObjects<Urmt>(sb.ToString(), null);
        }

        /// <summary>
        /// Get list of Role from MySQL
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static List<Urmt> GetListRoleMySql(string UserID)
        {
            var sb = new StringBuilder();
            sb.AppendLine(" SELECT");
            sb.AppendLine(" A.USERID,A.ROLEID,B.ROLEDESC ");
            sb.AppendLine(" , A.FACTORY "); //SON ADD) 25 Jan 2019
            sb.AppendLine(" FROM T_CM_URMT A ");
            sb.AppendLine(" LEFT JOIN T_CM_URLM B ON A.ROLEID = B.ROLEID ");
            sb.AppendLine(" WHERE A.USERID='" + UserID + "'");
            return MySqlDBManager.GetObjects<Urmt>(sb.ToString(), CommandType.Text, null); 
        }

        /// <summary>
        /// GetRoleByID
        /// </summary>
        /// <param name="RoleID"></param>
        /// <returns>Role</returns>
        /// VITHV
        public static Urmt GetRoleByID(string RoleID)
        {
            var sb = new StringBuilder();
            sb.AppendLine(" SELECT");
            sb.AppendLine(" A.USERID,A.ROLEID,B.ROLEDESC ");
            sb.AppendLine(" FROM T_CM_URMT A ");
            sb.AppendLine(" LEFT JOIN T_CM_URLM B ON A.ROLEID = B.ROLEID ");
            sb.AppendLine(" WHERE A.ROLEID='" + RoleID + "'");
            return OracleDbManager.GetObjects<Urmt>(sb.ToString(), null).FirstOrDefault();

        }


        /// <summary>
        /// GetRoleByID
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="RoleID"></param>
        /// <returns>Role</returns>
        /// VITHV
        public static List<Urmt> GetInformationByID(string UserID, string RoleID)
        {
            var sb = new StringBuilder();
            sb.AppendLine(" SELECT");
            sb.AppendLine(" A.USERID, A.ROLEID,  A.FACTORY, A.CRCODE ");
            sb.AppendLine(" FROM T_CM_URMT A ");
            sb.AppendLine(" WHERE A.ROLEID = '" + RoleID + "' ");
            sb.AppendLine(" AND A.UserID= '" + UserID + "' ");
            return MySqlDBManager.GetObjects<Urmt>(sb.ToString(), CommandType.Text, null);
        } 
    }
}
