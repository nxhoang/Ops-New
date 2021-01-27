using OPS_DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.APIEntities
{
    public class BomtAPI: StyleMaster
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        //public string GenName { get; set; }
        //public string GenNameDetail { get; set; }
        public string ItemColorSerial { get; set; }
        public string ItemColorways { get; set; }
        public string MainItemCode { get; set; }
        public string MainItemName { get; set; }
        public string MainItemColorSerial { get; set; }
        public string MainItemColorways { get; set; }
        public string ConsumpUnit { get; set; }
        public int? Qty { get; set; }
        public decimal? UnitConsumption { get; set; }
        //public string ItemRegister { get; set; }
        //public string RegistryDate { get; set; }
        //public string LastModifier { get; set; }
        //public string LastModiDate { get; set; }
        public string Sos { get; set; }
        //public string SosName { get; set; }
        public string Nation { get; set; }
        public decimal? StdPrice { get; set; }
        public string CurrCode { get; set; }
        //public string PatternCode { get; set; }
        //public string Status { get; set; }

        //public decimal? EXCRATIO { get; set; }
        public decimal? OfferConsumption { get; set; }
        public decimal? OfferPrice { get; set; }
        public string Remarks { get; set; }
        public string TdCheck { get; set; }
        public string Factory { get; set; }
        //public decimal? LOTSIZE { get; set; }
        //public decimal? SPREADING { get; set; }
        //public decimal? LAYER { get; set; }
        public decimal? Marker { get; set; }
        public decimal? UseConsumption { get; set; }
        //public string IMAGENAME { get; set; }
        //public string CONFSTATUS { get; set; }
        public decimal? PatternCons { get; set; }
        public decimal? MarkerCons { get; set; }
        //public DateTime? CONFDATE { get; set; }
        public decimal? PolyCons { get; set; }
        public string Cad_Material { get; set; }
        //public string SETCHECK { get; set; }
        //public string GENNAME { get; set; }
        public string  MarkerConsUnit { get; set; }
        public string AreaConsUnit { get; set; }
        public decimal?  Marker_LossRate { get; set; }
        public decimal?  ManualCons { get; set; }
        public string  ConsumptionType { get; set; }
        public string ManualConsUnit { get; set; }

        public List<PatternAPI> Patterns { get; set; }
    }
}
