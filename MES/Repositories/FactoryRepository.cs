using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using System.Collections.Generic;

namespace MES.Repositories
{
    public class FactoryRepository : IFactoryRepository
    {
        public IEnumerable<Fcmt> GetByCorporation(string corporation)
        {
            return FcmtBus.GetFactoriesByCorporation(corporation);
        }

        public List<Flsm> SummarizeByCorporation(string corType, string status, string corporation, int tenantId)
        {
            return FcmtBus.SummarizeByCorp(corType, status, corporation, tenantId);
        }
    }
}