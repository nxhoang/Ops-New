using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MES.Repositories
{
    public interface IMachineIoTMappingRepo
    {
        Task<string> UpdateMachineIotMappingAsync(string machineId, string iot);
    }
}
