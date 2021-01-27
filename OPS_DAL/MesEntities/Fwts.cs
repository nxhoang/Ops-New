using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class Fwts
    {
        public string FACTORY { get; set; }
        public string LINENO { get; set; }
        public string PLANYEAR { get; set; }
        public string PLANMONTH { get; set; }
        public string WEEKNO { get; set; }
        public string PLANDAY { get; set; }
        public float MORNINGTIME { get; set; }
        public float AFTERNOONTIME { get; set; }
        public float OVERTIME { get; set; }
        public DateTime CREATEDATE { get; set; }
        public string CREATEID { get; set; }
        public DateTime UPDATEDATE { get; set; }
        public string UPDATEID { get; set; }
    }
}
