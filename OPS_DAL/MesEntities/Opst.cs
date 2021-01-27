namespace OPS_DAL.MesEntities
{
    /// <summary>
    /// Opst (Operation plan simulation time) maps to t_mx_opst table
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    /// Created Date: 21-Nov-19
    public class Opst
    {
        public int TimeLineId { get; set; }
        public int LineSerial { get; set; }
        public int TableId { get; set; }
        public string MxPackage { get; set; }
        public decimal OpTime { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int TableWidth { get; set; }
        public int Height { get; set; }
        public string Location { get; set; }
        public int Angle { get; set; }
        public int Length { get; set; }
        public string Color { get; set; }
        public string GroupId { get; set; }
    }
}
