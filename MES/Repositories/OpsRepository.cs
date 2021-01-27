using OPS_DAL.Business;
using OPS_DAL.Entities;
using System.Collections.Generic;

namespace MES.Repositories
{
    public class OpsRepository : IOpsRepository
    {
        public IEnumerable<Opdt> Get(string styleCode, string styleSize, string styleColor, string revNo, string opRevNo, string edition,
            string languageId)
        {
            var opdts = OpdtBus.GetOpDetailByLanguage(styleCode, styleSize, styleColor, revNo, opRevNo, edition, languageId);
            return opdts;
        }
    }
}