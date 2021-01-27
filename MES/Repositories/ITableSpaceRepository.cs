using OPS_DAL.MesEntities;
using System.Collections.Generic;

namespace MES.Repositories
{
    interface ITableSpaceRepository
    {
        TableSpaceEntity OracleAdd(TableSpaceEntity item);
        TableSpaceEntity MySqlAdd(TableSpaceEntity item);
        IEnumerable<TableSpaceEntity> MySqlGet(string factory);
        IEnumerable<TableSpaceEntity> OracleGet(string factory);
    }
}