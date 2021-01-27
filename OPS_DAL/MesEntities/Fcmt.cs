using System;

namespace OPS_DAL.MesEntities
{
    /// <summary>
    /// T_CM_FCMT
    /// </summary>
    public class Fcmt
    {      
        public string Factory { get; set; }
        public string Name { get; set; }
        public string Sos { get; set; }
        public string Type { get; set; }
        public string AssChk { get; set; }
        public string CutChk { get; set; }
        public decimal OpPrice { get; set; }
        public string AccountNo { get; set; }
        public string Contact { get; set; }
        public string TdType { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
        public string Corporation { get; set; }
        public string HrmCorpCode { get; set; }
        public string Team { get; set; }
        public Int64 Capa { get; set; }
        public string Department { get; set; }
        public string SapVendor { get; set; }
        public string Nation { get; set; }
        
        //2019-11-29 Tai Le (Thomas)
        public string isSelected { get; set; }
    }
}
