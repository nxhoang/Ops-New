using OPS_DAL.MesEntities;
using PKERP.Base.Domain.Interface.Dto;
using System.Threading.Tasks;

namespace MES.Repositories
{
    public interface IMachineIoTMappingRepo
    {
        Task<TaskResult<string>> UpdateMachineIotMappingAsync(Mcmp mcmp, string UserID);
    }
}
