using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class Ioht
    {
        public int ID { get; set; }
        public string MAC { get; set; }
        public DateTime EVENT_DATE { get; set; }
        public DateTime SERVER_DATE { get; set; }
        public string PROCESSID { get; set; }
        public string TYPE { get; set; }
        public decimal DATA { get; set; }
        public decimal CUR_CYCLE { get; set; }
        public decimal SUM_CYCLE { get; set; }
    }
}
