using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    public class Wrkman
    {
        public string FATOY { get; set; }
        public string LINENO { get; set; }
        public string MOTHNO { get; set; }
        public string WEEKNO { get; set; }
        public string PLNDAY { get; set; }
        public float MANCNT { get; set; }
        public DateTime CRTDAT { get; set; }
        public string CRTID { get; set; }
        public DateTime UPTDAT { get; set; }
        public string UPTID { get; set; }
    }
}
