namespace OPS_DAL.Entities
{
    /// <summary>
    /// Refer to T_SD_FLDT table
    /// </summary>
    /// Author: Son Nguyen Cao
    class Fldt:StyleMaster
    {
        public string UploadCode { get; set; }
        public string AmenDno { get; set; }
        public string MainItemCode { get; set; }
        public string ItemCode { get; set; }
        public string ItemColorSerial { get; set; }
        public string FileNote { get; set; }
        public string FileName { get; set; }
        public string RemoteFile { get; set; }
        public string LocalFile { get; set; }
        public string Register { get; set; }
        public string Remarks { get; set; }
        public string Confirmed { get; set; }
        public string Dept { get; set; }
        public string MarkerType { get; set; }
    }
}
