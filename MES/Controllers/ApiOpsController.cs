using MES.Repositories;
using OPS_DAL.Entities;
using System.Collections.Generic;
using System.Web.Http;

namespace MES.Controllers
{
    public class ApiOpsController : ApiController
    {
        private static readonly IOpsRepository OpsRepository = new OpsRepository();

        public IEnumerable<Opdt> Get(string styleCode, string styleSize, string styleColor, string revNo, string opRevNo,
            string edition, string languageId)
        {
            return OpsRepository.Get(styleCode, styleSize, styleColor, revNo, opRevNo, edition, languageId);
        }
    }
}
