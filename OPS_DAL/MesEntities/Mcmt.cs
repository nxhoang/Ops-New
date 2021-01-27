
namespace OPS_DAL.MesEntities
{

    /// <summary>
    /// Refer to T_CM_MCMT table
    /// </summary>
    /// Author: Son Nguyen Cao
    public class Mcmt
    {
        public string MasterCode { get; set; }
        public string SubCode { get; set; }
        public string CodeName { get; set; }
        public string CodeDesc { get; set; }
        public string CodeDetail { get; set; }
        public string CodeDetail2 { get; set; }
        public string CodeStatus { get; set; }
    }
}
