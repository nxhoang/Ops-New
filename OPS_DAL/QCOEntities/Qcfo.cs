namespace OPS_DAL.QCOEntities
{
    public class Qcfo
    {
        public decimal ID { get; set; }
        public string FACTORY { get; set; }
        public string PARAMETERNAME { get; set; }
        public string DBFIELDNAME { get; set; }
        public decimal SORTINGSEQ { get; set; }
        public string SORTDIRECTION { get; set; }

        public Qcfo(string vFACTORY , string vPARAMETERNAME, string vDBFIELDNAME, string vSORTDIRECTION)
        {
            FACTORY = vFACTORY;
            PARAMETERNAME = vPARAMETERNAME;
            DBFIELDNAME = vDBFIELDNAME;
            SORTDIRECTION = vSORTDIRECTION;
        }

        public Qcfo() { }

    }
}
