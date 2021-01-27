using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKERP.MES.Services.Interface.Services
{
    public interface IConfigService
    {
        Task<int> GetIntConfigValueAsync(string configKey, int defaultValue = 0);
        Task<string> GetStringConfigValueAsync(string configKey, string defaultValue = "");
    }
}
