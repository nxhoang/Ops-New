using OPS_DAL.MesBus;
using PKERP.MES.Services.Interface.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PKERP.MES.Services.Implement.Services
{
    public class ConfigService : IConfigService
    {
        public async Task<int> GetIntConfigValueAsync(string configKey, int defaultValue = 0)
        {
            var value = defaultValue;

            var config = ConfigBus.GetConfig(configKey);
            if (config == null) return value;

            
            int.TryParse(config.CONFIGVALUE, out value);

            return await Task.FromResult<int>(value);
        }

        public async Task<string> GetStringConfigValueAsync(string configKey, string defaultValue = "")
        {
            var value = defaultValue;

            var config = ConfigBus.GetConfig(configKey);
            if (config == null) return value;

            value = config.CONFIGVALUE;

            return await Task.FromResult<string>(value);
        }
    }
}
