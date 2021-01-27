using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MES.Repositories
{
    public class CstpRepository : ICstpRepository
    {
        private readonly CstpBus _CstpBus = new CstpBus();
        IEnumerable<Cstp> ICstpRepository.GetAll()
        {
            return CstpBus.GetAll();
        }

        public async Task<Cstp> GetByServerNo(string serverNo)
        {
            return await _CstpBus.GetByServerNo(serverNo);
        }
    }
}