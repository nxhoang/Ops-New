using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    /// <summary>
    /// Defect mapping between PK defect code and buyer defect code
    /// </summary>
    public class Dfmp
    {
        public string PKDFECTCODE { get; set; }
        public string CATEGORYID { get; set; }
        public string BUYERDEFECTCODE { get; set; }
        public string BUYER { get; set; }
        public string REGISTERID { get; set; }
        public DateTime? REGISTRYDATE { get; set; }
        public string DEFECTDESC { get; set; }
        public string BUYERNAME { get; set; }
        public string BUYERDEFECTDESC { get; set; }
    }
}
