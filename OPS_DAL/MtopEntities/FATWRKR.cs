using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MtopEntities
{
    public class FATWRKR
    {
        public string FATOY { get; set; }
        public string LINENO { get; set; }
        public string MOTHNO { get; set; }
        public decimal WORKER { get; set; }
        public decimal SEWER { get; set; }
        public decimal NOSWRATE { get; set; }
        public DateTime CRTDAT { get; set; }
        public string CRTID { get; set; }
        public string UPTID { get; set; }
        public float FTYCOST { get; set; }
        public float EXPPRDTY { get; set; }
    }
}
