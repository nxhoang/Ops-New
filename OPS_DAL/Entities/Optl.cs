namespace OPS_DAL.Entities
{
    /// <summary>
    /// Operation Tool Linking (Machine)  Map to T_OP_OPTL
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    /// <seealso cref="StyleMaster" />
    public class Optl : StyleMaster
    {
        public string OpRevNo { get; set; }
        public int OpSerial { get; set; }
        public string ItemCode { get; set; }
        public string Machine { get; set; }
        public string ImagePath { get; set; }
        public string ItemName { get; set; }
        public string CategId { get; set; }
        public string Category { get; set; }
        public string Edition { get; set; }
        public string MainTool { get; set; }
        public string MachineType { get; set; } // HA ADD
    }
}
