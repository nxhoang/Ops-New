using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class Mccn
    {
        public string FACTORY { get; set; }
        public string YEAR_COUNT { get; set; }
        public string MONTH_COUNT { get; set; }
        public string DAY_COUNT { get; set; }
        public string WEEKNO { get; set; }
        public int MACHINE_COUNT_DGS { get; set; }
        public int MACHINE_COUNT_PKG { get; set; }
        public int MACHINE_COUNT_MES { get; set; }
        public int TOTAL_IOT { get; set; }
        public DateTime UPDATEDATE { get; set; }
    }
}
