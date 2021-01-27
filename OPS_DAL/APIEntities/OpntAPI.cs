using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.APIEntities
{
    public class OpntAPI
    {
        public string StyleCode { get; set; }
        public string StyleSize { get; set; }
        public string StyleColorSerial { get; set; }
        public string RevNo { get; set; }
        public string OpRevNo { get; set; }
        public int OpSerial { get; set; }
        public int OpNameId { get; set; }
        public string Edition { get; set; }
        public int OpTime { get; set; }
        public int OpnSerial { get; set; }
        public string OpName { get; set; }
        public string MachineType { get; set; }
        public int MachineCount { get; set; }
        public string Remarks { get; set; }
        public int MaxTime { get; set; }
        public float ManCount { get; set; }
        public string JobType { get; set; }
        public string ToolId { get; set; }
        public string ActionCode { get; set; }
        public int StitchCount { get; set; }
    }
}
