using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using System.Collections.Generic;

namespace MES.Repositories
{
    public class DcmtRepository : IDcmtRepository
    {
        IEnumerable<Dcmt> IDcmtRepository.GetAll()
        {
            return DcmtBus.GetCompanyListCoporation();
        }
    }
}