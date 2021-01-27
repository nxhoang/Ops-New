using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using System.Collections.Generic;

namespace MES.Repositories
{
    public class LineRepository : ILineRepository
    {
        public IEnumerable<LineEntity> GetMtopLinesByFactory(string factory)
        {
            return LineBus.GetMtopLinesByFactory(factory);
        }

        public LineEntity OracleAdd(LineEntity item)
        {
            return LineBus.OracleAddLine(item);
        }

        public LineEntity MySqlAdd(LineEntity item)
        {
            return LineBus.MySqlAddLine(item);
        }

        public IEnumerable<LineEntity> MySqlGetMesLinesByFactory(string fac)
        {
            return LineBus.MySqlGetMesLinesByFactory(fac);
        }

        public IEnumerable<LineEntity> GetMesLinesByFactory(string fac)
        {
            return LineBus.GetMesLinesByFactory(fac);
        }
    }
}