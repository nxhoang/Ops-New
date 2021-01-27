using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class Adsm
    {
        public string AdNo { get; set; }
        public string StyleCode { get; set; }
        public string StyleSize { get; set; }
        public string StyleColorSerial { get; set; }
        public string RevNo { get; set; }
        public int StyleSerial { get; set; }
        public string Destination { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int AdQty { get; set; }
        public string QtyModiDate { get; set; }
        public string CurrCode { get; set; }
        public decimal StdPrice { get; set; }
        public decimal AdPrice { get; set; }
        public string Factory { get; set; }
        public string Status { get; set; }
        public DateTime PdStr { get; set; }
        public DateTime PdEnd { get; set; }
        public DateTime AdStr { get; set; }
        public DateTime AdEnd { get; set; }
        public string AssChk { get; set; }
        public DateTime BomConfirm { get; set; }
        public DateTime FaConfirm { get; set; }
        public DateTime Mts { get; set; }
        public string WorkFactory { get; set; }
        public string CfnStyle { get; set; }
        public DateTime CfnDate { get; set; }
        public string CfnUser { get; set; }
        public string OriginAdNo { get; set; }
        public string CougarId { get; set; }
        public DateTime CougarDate { get; set; }
        public string Year { get; set; }
        public string Weekly { get; set; }
        public string OutSourceCheck { get; set; }
        public string SaleId { get; set; }
        public DateTime SalesDate { get; set; }
        public string Certain { get; set; }
        public string BuyReady { get; set; }
        public string BuyerPdNo { get; set; }

        //Add more properties
        public string Buyercode { get; set; }
        public string BuyerStyleName { get; set; }
        public string StatusName { get; set; }
        public string StyleColorWays { get; set; }

    }
}
