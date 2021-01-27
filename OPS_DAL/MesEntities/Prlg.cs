using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class Prlg
    {
        public string MXPACKAGE { get; set; }
        public decimal MXTARGET { get; set; }
        public decimal ACHIEVEDQTY { get; set; }
        public decimal MACHINES { get; set; }
        public decimal WORKERS { get; set; }
        public decimal WORKERSOT { get; set; }
        public decimal WORKINGHOURS { get; set; }
        public decimal OVERTIME { get; set; }
        public string REGISTERID { get; set; }
        public DateTime REGISTRYDATE { get; set; }
        public string PACKAGEGROUP { get; set; }
        public int SEQNO { get; set; }

    }
}
