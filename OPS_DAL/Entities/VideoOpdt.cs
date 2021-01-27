namespace OPS_DAL.Entities
{
    public class VideoOpdt : OpdtKey
    {
        public string Edition { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string PartNo { get; set; }
        public string VideoFile { get; set; }
        public string VideoFullPath { get; set; }
        public string OpNum { get; set; }
        public string DisplayName { get; set; }
    }
}
