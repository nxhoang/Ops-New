namespace OPS_DAL.MesEntities
{
    /// <summary>
    /// Maps to t_cm_tbsp table
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    public class TableSpaceEntity
    {
        public decimal TableId { get; set; }
        public string Factory { get; set; }
        public decimal LineSerial { get; set; }
        public string TableName { get; set; }
        public string BackgroundColor { get; set; }
        public string TbCategory { get; set; }
        public decimal Angle { get; set; }
        public string TbLocation { get; set; }
        public decimal SeatTotal { get; set; }
        public decimal SeatDistance { get; set; }
        public int SeatType { get; set; }
        public decimal VirtualWidth { get; set; }
        public decimal VirtualLength { get; set; }
        public decimal ActualWidth { get; set; }
        public decimal ActualLength { get; set; }
        public decimal Rate { get; set; }
        public object Workers { get; set; }
    }
}
