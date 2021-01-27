using MES.Repositories;
using OPS_DAL.MesEntities;
using System.Collections.Generic;
using System.Web.Http;

namespace MES.Controllers
{
    [SessionTimeout]
    public class ApiCstpController : ApiController
    {
        private static readonly ICstpRepository CstpRepository = new CstpRepository();

        public IEnumerable<Cstp> Get()
        {
            return CstpRepository.GetAll();
        }
    }
}
