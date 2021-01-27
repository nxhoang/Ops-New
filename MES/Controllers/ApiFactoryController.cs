using MES.Repositories;
using OPS_DAL.MesEntities;
using System.Collections.Generic;
using System.Web.Http;

namespace MES.Controllers
{
    [SessionTimeout]
    public class ApiFactoryController : ApiController
    {
        private static readonly IFactoryRepository FactoryRepository = new FactoryRepository();

        // GET: api/ApiFactory
        public IEnumerable<Fcmt> GetByCorporation(string corp)
        {
            return FactoryRepository.GetByCorporation(corp);
        }

        public IEnumerable<Flsm> GetSummaryByCorporation(string corporation, int tenantId)
        {
            return FactoryRepository.SummarizeByCorporation("P", "OK", corporation, tenantId);
        }
    }
}
