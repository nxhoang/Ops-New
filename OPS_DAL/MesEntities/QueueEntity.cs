using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class QueueEntity
    {
        public string QCOFactory { get; set; }
        public decimal QCOYear { get; set; }
        public string QCOWeekNo { get; set; }
        public decimal QCORank { get; set; }
        public string Factory { get; set; }
        public string LineNo { get; set; }
        public string AoNo { get; set; }
        public string Buyer { get; set; }
        public string StyleCode { get; set; }
        public string StyleSize { get; set; }
        public string StyleColorSerial { get; set; }
        public string RevNo { get; set; }
        public string PrdPkg { get; set; }
        public DateTime CreateDate { get; set; }
        public decimal NormalizedPercent { get; set; }
        public decimal ChangeQCORank { get; set; }
        public string ChangeBy { get; set; }
        public string Reason { get; set; }
        public decimal PlanQty { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string PrdSDat { get; set; }
        public string PrdEDat { get; set; }
        public decimal OrdQty { get; set; }
        public int QCORankingNew { get; set; }
        public string StyleInf { get; set; }
        public decimal RemainQty { get; set; }
    }
}
