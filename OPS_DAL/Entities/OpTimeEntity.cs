using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    public class OpTimeEntity
    {
        public string StyleCode { get; set; }
        public string StyleSize { get; set; }
        public string StyleColorSerial { get; set; }
        public string RevNo { get; set; }
        public double OpTime { get; set; }
        public double MaxTime { get; set; }
        public decimal OpPrice { get; set; }
        public int MachineCount { get; set; }
        public Single ManCount { get; set; }
        public string Edition { get; set; }
        public string OpRevNo { get; set; }
        public double HourlyTarget { get; set; }
        public string Factoy { get; set; }
        public string OPSColorSerial { get; set; }
        public int OpCount { get; set; }

    }
}
