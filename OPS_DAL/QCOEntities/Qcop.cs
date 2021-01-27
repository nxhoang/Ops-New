namespace OPS_DAL.QCOEntities
{
    public class Qcop
    {
        public decimal ID { get; set; }
        public string PARAMETERNAME { get; set; }
        public decimal SEQNO { get; set; }
        public string DBFIELDNAME { get; set; } //2020-09-24 Tai Le(Thomas)
        public string SORTDIRECT { get; set; } //2020-09-24 Tai Le(Thomas)
    }
}
