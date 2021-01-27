using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.APIEntities
{
    public class OpmtAPI
    {
        public string StyleCode { get; set; }
        public string StyleSize { get; set; }
        public string StyleColorSerial { get; set; }
        public string RevNo { get; set; }
        public string OpRevNo { get; set; }
        public int OpTime { get; set; }
        public decimal OpPrice { get; set; }
        public int MachineCount { get; set; }
        public string ConfirmChk { get; set; }
        public int OpCount { get; set; }
        public int ManCount { get; set; }        
        public string Edition { get; set; }
        public string Remarks { get; set; }
        public string BuyerStyleCode { get; set; }
        public string BuyerStyleName { get; set; }
        public string Buyer { get; set; }
        public string StyleName { get; set; }
        public string BuyerName { get; set; }
        public List<OpdtAPI> ListProcess { get; set; }
    }
}
