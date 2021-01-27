using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using System.Data;
using System.Linq;

namespace OPS_DAL.MesBus
{
    public class ConfigBus
    {
        public static Config GetConfig(string configKey)
        {
            var lstConfigs = MySqlDBManager.GetObjects<Config>(@"SELECT * FROM T_MX_CONFIG WHERE CONFIGKEY = @configKey",
                                                CommandType.Text, 
                                                new MySqlParameter[] { new MySqlParameter("configKey", configKey) });

            if (lstConfigs != null)
                return lstConfigs.FirstOrDefault();

            return null;
        }
    }
}