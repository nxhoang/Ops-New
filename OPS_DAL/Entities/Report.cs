using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    //Author: HA NGUYEN
    public class Report
    {
        public string BuyerName { get; set; }
        public string SOS { get; set; }
        public string Nation { get; set; }
        public string Code_Name { get; set; }
        public string FullName { get; set; }
        public string ADNO { get; set; }
        public string PDNO { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemColorSerial { get; set; }
        public string ItemColor { get; set; }
        public string ConsumpUnit { get; set; }
        public decimal QTYConsumption { get; set; }
        public decimal RPRQTY { get; set; }
        public decimal PRPrice { get; set; }
        public string CurrCode { get; set; }
        public decimal POAMount { get; set; }
        public decimal POAMount_USD { get; set; }
        public string ReasonName { get; set; }
        public string Factory { get; set; }
        public string PORegistryName { get; set; }
    }
}
