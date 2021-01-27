using OPS_DAL.MesEntities;
using System.Collections.Generic;

namespace MES.Repositories
{
    internal interface IFactoryRepository
    {
        IEnumerable<Fcmt> GetByCorporation(string corporation);
        List<Flsm> SummarizeByCorporation(string corType, string status, string corporation, int tenantId);
    }
}
