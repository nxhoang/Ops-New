using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.QCOEntities
{
    public class FLWS
    {
        public string FACTORY { get; set; }
        public int LINESERIAL { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public Single MORNINGTIME { get; set; }
        public Single AFTERNOONTIME { get; set; }
        public Single OVERTIME { get; set; }
        public string CREATOR { get; set; }
        public DateTime CREATETIME { get; set; }
        public decimal WEEKNO { get; set; }
        public decimal WORKERS { get; set; }
        public decimal SEWERS { get; set; }
        public decimal MACHINES { get; set; }
        public string LINENO { get; set; }
    }
}
