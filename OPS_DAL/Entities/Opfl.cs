using System;

namespace OPS_DAL.Entities
{
    public class Opfl:StyleMaster
    {
        public string OpRevNo { get; set; }
        public int OpSerial { get; set; }
        public string Edition { get; set; }
        public string UploadCode { get; set; }
        public string AmendNo { get; set; }
        public string Register { get; set; }
        public DateTime RegistryDate { get; set; }
        public string SysFileName { get; set; }
        public string OrgFileName { get; set; }
        public string SourceFile { get; set; }
        public string RefLink { get; set; }

    }
}
