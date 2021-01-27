using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    public class Adsm:StyleMaster
    {
        public string AdNo { get; set; }
        public string Destination { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string AdQty { get; set; }
        public string QtyModiDate { get; set; }
        public string CurrCode { get; set; }
        public decimal StdPrice { get; set; }
        public decimal AdPrice { get; set; }
        public string Factory { get; set; }
        public string Status { get; set; }
        public DateTime Pd_Str { get; set; }
        public DateTime Pd_End { get; set; }
        public DateTime Ad_Start { get; set; }
        public DateTime Ad_End { get; set; }
        public string AssChk { get; set; }
        public DateTime Bomm_Confirm { get; set; }
        public DateTime Fa_Confirm { get; set; }
        public DateTime Mts { get; set; }
        public string WorkFactory { get; set; }
        public string CfnStyle { get; set; }
        public DateTime CfnDate { get; set; }
        public string CfnUser { get; set; }
        public string OriginAoNo { get; set; }
        public string CougarId { get; set; }
        public DateTime CougarDate { get; set; }
        public string Year { get; set; }
        public string Weekly { get; set; }
        public string OutSourceCheck { get; set; }
        public string SaleId { get; set; }
        public DateTime SaleDate { get; set; }
        public int Certain { get; set; }
        public int Buy_Ready { get; set; }
        public string BuyerPdNo { get; set; }

    }
}
