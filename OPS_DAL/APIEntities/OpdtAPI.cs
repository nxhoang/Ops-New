using OPS_DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.APIEntities
{
    public class OpdtAPI
    {                       
        public string StyleCode { get; set; }
        public string StyleSize { get; set; }
        public string StyleColorSerial { get; set; }
        public string RevNo { get; set; }
        public string OpRevNo { get; set; }
        public int OpSerial { get; set; }
        public string OpNum { get; set; }
        public string OpGroup { get; set; }
        public string OpName { get; set; }
        public string Factory { get; set; }
        public string MachineType { get; set; }
        public int OpTime { get; set; }
        //public decimal OpPrice { get; set; }
        //public decimal OfferOpPrice { get; set; }
        public int MachineCount { get; set; }
        public string Remarks { get; set; }         
        public int MaxTime { get; set; }
        public float ManCount { get; set; }
        public string NextOp { get; set; }
        //public string OutSourced { get; set; }    
        public string Edition { get; set; }
        public string JobType { get; set; }
        public string ModuleId { get; set; }
        public string ModuleName { get; set; }
        //public string HotSpot { get; set; }
        public string ToolId { get; set; }
        //public int OpTimeBalancing { get; set; }
        public string BenchmarkTime { get; set; }
        public string LaborType { get; set; }
        public string ActionCode { get; set; }
        public decimal StitchCount { get; set; }
        public string ProcessImage { get; set; }
        public List<OpntAPI> ListSubProcess { get; set; }
    }
}
