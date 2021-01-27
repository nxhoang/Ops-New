using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    /// <summary>
    /// T_MX_JGRQ (Jig request)
    /// </summary>
    public class Jgrq
    {
        public string JIGREQUESTID { get; set; }
        public string PRDPKG { get; set; }
        public string JIGCODE { get; set; }
        public int JIGQTY { get; set; }
        public string REQUESTOR { get; set; }
        public DateTime REQUESTDATE { get; set; }
    }
}
