namespace OPS_DAL.MesEntities
{
    /// <summary>
    /// Opsm (Operation plan simulation module) maps to t_mx_opsm table
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    /// Created Date: 20-Dec-19
    public class Opsm
    {
        public string key { get; set; } // for client-side
        public int Id { get; set; }
        public string Name { get; set; }
        public string GroupId { get; set; }
        public string MxPackage { get; set; }
        public string Factory { get; set; }
        public decimal OpTime { get; set; }
        public string Location { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Angle { get; set; }
        public string BackgroundColor { get; set; }
        public string NextModule { get; set; }
    }
}
