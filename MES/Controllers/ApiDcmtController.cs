using System.Collections.Generic;
using System.Web.Http;
using MES.Repositories;
using OPS_DAL.MesEntities;

namespace MES.Controllers
{
    [SessionTimeout]
    public class ApiDcmtController : ApiController
    {
        private static readonly IDcmtRepository DcmtRepository = new DcmtRepository();

        // Get list of corporations
        public IEnumerable<Dcmt> Get()
        {
            return DcmtRepository.GetAll();
        }
    }
}
