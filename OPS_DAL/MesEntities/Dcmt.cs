using System;

namespace OPS_DAL.MesEntities
{
    /// <summary>
    /// Corporation table
    /// Mapping to t_cm_dcmt table
    /// </summary>
    public class Dcmt
    {
        public string DeptCode { get; set; }
        public string DeptLevel { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UseStatus { get; set; }
    }
}
