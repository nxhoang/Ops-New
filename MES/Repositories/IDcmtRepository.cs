using OPS_DAL.MesEntities;
using System.Collections.Generic;

namespace MES.Repositories
{
    interface IDcmtRepository
    {
        IEnumerable<Dcmt> GetAll();
    }
}
