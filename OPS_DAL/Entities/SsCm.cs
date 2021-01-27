

using System;

namespace OPS_DAL.Entities
{
   public class SsCm
    {
        public string Sos { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public string Nation { get; set; }
        public string Region { get; set; }
        public string Industry { get; set; }
        public string Items { get; set; }
        public string ZipCode { get; set; }
        public string Address { get; set; }
        public string ChiefUserId { get; set; }
        public string Tel { get; set; } //MOD)SON (2019.12.07) - 07 December 2019 - change Teil to Tel
        public string Fax { get; set; }
        public string Url { get; set; }
        public string CurrCode { get; set; }
        public string AvgLeadTime { get; set; }
        public string OutCheck { get; set; }
        public string FOREIGNCHECK { get; set; }
        public string CSUSE { get; set; }
        public string INCOTERMS { get; set; }
        public string BANKCODE { get; set; }
        public string ACCOUNTNO { get; set; }
        public string OPPTAPECODE { get; set; }
        public string LOSS { get; set; }
        public string CNCODE { get; set; }
        public string CSMAKER { get; set; }
        public string CNCHECK { get; set; }
        public string BUYERCHK { get; set; }
        public string SUPPLYCHK { get; set; }
        public string TREATCHK { get; set; }
        public string OUTCHK { get; set; }
        public string PURCHASER { get; set; }
        public string AGENT { get; set; }
        public string STATUS { get; set; }
        public string GACHK { get; set; }
        public string ACCCHK { get; set; }
        public DateTime? REGISTRYDATE { get; set; }
        public string REGISTERID { get; set; }
        public DateTime? MODIFYDATE { get; set; }
        public string MODIFIER { get; set; }
        public string TRANSTYPE { get; set; }
        public string PAYCONDITION { get; set; }
        public string SAPSUPPLIERID { get; set; }
        public string ERP_USE { get; set; }
        public string MRO_USE { get; set; }
        public string EMAIL { get; set; }
        public string NEWSOS { get; set; }
        public string POSYSTEM { get; set; }
        public string WEBADDRESS { get; set; }
        public string MRP_USE { get; set; }
        public string SOST1 { get; set; }
        public string NOMINATED { get; set; }
        public string MACHINESUPPLIER { get; set; }
    }
}
