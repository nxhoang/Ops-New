using System;

namespace OPS_DAL.Entities
{
    /// <summary>
    /// Refer to T_SD_FILE table
    /// </summary>
    public class FileSd:StyleMaster
    {
        public string UploadCode { get; set; }
        public string FileNote { get; set; }
        public string FileName { get; set; }
        public string RemoteFile { get; set; }
        public string LocalFile { get; set; }
        public string Register { get; set; }
        public DateTime RegistryDate { get; set; }
        public string AmendNo { get; set; }
        public string Remark { get; set; }
        public string Confirmed { get; set; }
        public string Dept { get; set; }
        public string StyleColorWays { get; set; }
        public string Linked { get; set; }
        public string IsOpFile { get; set; }
        public string SourceFile { get; set; }
        public string RefLink { get; set; }
        public string Used { get; set; } //SON ADD) 2019/Jan/11
    }
}
