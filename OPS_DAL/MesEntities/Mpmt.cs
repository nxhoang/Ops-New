using OPS_DAL.MesEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public class Mpmt
    {
        public string PackageGroup { get; set; }
        public string MesFactory { get; set; }
        public string StyleCode { get; set; }
        public string StyleSize { get; set; }
        public string StyleColorSerial { get; set; }
        public string RevNo { get; set; }
        public string Buyer { get; set; }
        public int TargetQty { get; set; } //Change to Int type: decimal
        public string Status { get; set; }
        public string MesPlnStartDate { get; set; }
        public string MesPlnEndDate { get; set; }
        public string MesActStartDate { get; set; }
        public string MesActEndDate { get; set; }
        public int Priority { get; set; }//Change to Int type: decimal
        public int MadeQty { get; set; }//Change to Int type: decimal
        public string Registrar { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string StyleInf { get; set; }
        public string AONo { get; set; }
        public string DistributeStatus { get; set; }
        public string CreateNew { get; set; }
        public Int64 RemainQty { get; set; }
        public Int64 MaxSeq { get; set; }
        public string PPackage { get; set; }
        public string UpdatedId { get; set; }

        public int TotalMadeQty { get; set; }

        public List<Ppkg> ListPpkg { get; set; }
        public List<Mpdt> ListMpdt { get; set; }

        public string BuyerStyleCode { get; set; }
        public string BuyerStyleName { get; set; }
        public string StyleColorways { get; set; }
        public string StyleName { get; set; }
        public string StatusName { get; set; }
        public string ImgLink { get; set; }
        public string FactoryName { get; set; }

        public decimal MATReadiness { get; set; }//2020-08-07 Tai Le(Thomas)
    }
}
