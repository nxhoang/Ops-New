using OPS_DAL.MesBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MES.Repositories
{
    public class MachineIoTMappingRepo : IMachineIoTMappingRepo
    {
        private readonly McmpBus _McmpBus = new McmpBus();

        public async Task<string> UpdateMachineIotMappingAsync(string machineId, string iot)
        {
            return await _McmpBus.UpdateMachineIotMapping(new OPS_DAL.MesEntities.Mcmp() { IOT_MAC = iot, MACHINE_ID = machineId }); 
        }
    }
}