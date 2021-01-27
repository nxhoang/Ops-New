using OPS_DAL.MesEntities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MES.Repositories
{
    interface ICstpRepository
    {
        IEnumerable<Cstp> GetAll();

        Task<Cstp> GetByServerNo(string serverNo);
    }
}
