using OPS_DAL.DAL;
using OPS_DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Business
{
    public static class UrlmBus
    {
        public static List<Urlm> GetMasterRoleList()
        {
            var sb = new StringBuilder();
            sb.Append(@" 
SELECT RoleId , RoleDesc
FROM t_cm_urlm
");
            return MySqlDBManager.GetObjects<Urlm>(sb.ToString(), CommandType.Text, null);
        }

    }
}
