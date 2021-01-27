using System;

namespace OPS_DAL.Entities
{

    public class Bomt : StyleMaster
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string GenName { get; set; }
        public string GenNameDetail { get; set; }
        public string ItemColorSerial { get; set; }
        public string ItemColorWays { get; set; }
        public string MainItemCode { get; set; }
        public string MainItemName { get; set; }
        public string MainItemColorSerial { get; set; }
        public string MainItemColorWays { get; set; }
        public string ConsumpUnit { get; set; }
        public int? Qty { get; set; }
        public decimal? UnitConsumption { get; set; }
        public string ItemRegister { get; set; }
        public string RegistryDate { get; set; }
        public string LastModifier  { get; set; }
        public string LastModiDate { get; set; }
        public string Sos { get; set; }
        public string SosName { get; set; }
        public string Nation { get; set; }
        public decimal? StdPrice { get; set; }
        public string CurrCode { get; set; }
        public string PatternCode { get; set; }
        public string Status { get; set; }

        public decimal? EXCRATIO { get; set; }
        public decimal? OFFERCONSUMPTION { get; set; }
        public decimal? OFFERPRICE { get; set; }
        public string REMARKS { get; set; }
        public string TDCHECK { get; set; }
        public string FACTORY { get; set; }
        public string SOSITEMCODE { get; set; }
        public string BOMFILE { get; set; }
        public decimal? LEVA { get; set; }
        public decimal? LEVB { get; set; }
        public decimal? LEVC { get; set; }
        public decimal? LOTSIZE { get; set; }
        public decimal? SPREADING { get; set; }
        public decimal? LAYER { get; set; }
        public decimal? MARKER { get; set; }
        public decimal? USECONSUMPTION { get; set; }
        public string IMAGENAME { get; set; }
        public string CONFSTATUS { get; set; }
        public decimal? PATTERNCONS { get; set; }
        public decimal? MARKERCONS { get; set; }
        public DateTime? CONFDATE { get; set; }
        public decimal? POLYCONS { get; set; }
        public string CAD_MATERIAL { get; set; }
        public string SETCHECK { get; set; }
        public string GENNAME { get; set; }
        public string MARKERCONSUNIT { get; set; }
        public decimal? AREACONS { get; set; }
        public string AREACONSUNIT { get; set; }
        public decimal? MARKER_LOSSRATE { get; set; }
        public decimal? MANUALCONS { get; set; }
        public string CONSUMPTION_TYPE { get; set; }
        public string MANUALCONSUNIT { get; set; }
        public string HasPattern { get; set; }


    }
}
