using OPS_DAL.MesBus;
using PKERP.Base.Domain.Interface.Dto;
using System;
using System.Threading.Tasks;

namespace MES.Repositories
{
    public class MachineIoTMappingRepo : IMachineIoTMappingRepo
    {
        private readonly McmpBus _mcmpBus = new McmpBus();

        public async Task<TaskResult<string>> UpdateMachineIotMappingAsync(OPS_DAL.MesEntities.Mcmp mcmp, string UserID)
        {
            await _mcmpBus.UpdateDGSMachineIotMappingAsync(mcmp, UserID);
            var res = await _mcmpBus.UpdateMachineIotMappingAsync(mcmp, UserID);
            if (res == "OK")
                return new SuccessTaskResult<string>();
            else
                return new FailedTaskResult<string>(res);
        }
    }
}