using OPS_DAL.MesEntities;
using System.Collections.Generic;

namespace MES.Repositories
{
    interface ILineRepository
    {
        IEnumerable<LineEntity> GetMtopLinesByFactory(string factory);
        IEnumerable<LineEntity> GetMesLinesByFactory(string fac);
        LineEntity OracleAdd(LineEntity item);
        LineEntity MySqlAdd(LineEntity item);
        IEnumerable<LineEntity> MySqlGetMesLinesByFactory(string fac);
    }
}
