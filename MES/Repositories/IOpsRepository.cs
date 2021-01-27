using OPS_DAL.Entities;
using System.Collections.Generic;

namespace MES.Repositories
{
    interface IOpsRepository
    {
        IEnumerable<Opdt> Get(string styleCode, string styleSize, string styleColor, string revNo, string opRevNo,
            string edition, string languageId);
    }
}