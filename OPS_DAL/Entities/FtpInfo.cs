namespace OPS_DAL.Entities
{
    /// <summary>
    /// t_cm_pftp
    /// </summary>
    /// Author: VitHV
    public class FtpInfo
    {
        public string AppType { get; set; }
        public string FtpLink { get; set; }
        public string FtpUser { get; set; }
        public string FtpPass { get; set; }
        public string FtpFolder { get; set; }

        public string FtpRoot { get; set; }

    }
}
