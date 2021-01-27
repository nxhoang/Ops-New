using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class Ppdp
    {
        public string Factory { get; set; }
        public string LineNo { get; set; }
        public string AoNo { get; set; }
        public string StyleCode { get; set; }
        public string StyleSize { get; set; }
        public string StyleColorSerial { get; set; }
        public string RevNo { get; set; }
        public string PrdPkg { get; set; }
        public int PlnQty { get; set; }
        public decimal PrdQty { get; set; }
        public decimal PPRemainQty { get; set; }
    }
}
