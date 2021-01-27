using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class Fwcp
    {
        public string FACTORY { get; set; }
        public decimal YEAR { get; set; }
        public decimal WEEKNO { get; set; }
        public decimal TOTALWORKERS { get; set; }
        public decimal CAPACITY { get; set; }
        public string CREATOR { get; set; }
        public DateTime CREATETIME { get; set; }
        public decimal TOTALMACHINES { get; set; }
        public DateTime STARTDATE { get; set; }
        public DateTime ENDATE { get; set; }
        public decimal TOTALSEWER { get; set; }
        public decimal SEWERCAPA { get; set; }
    }
}
