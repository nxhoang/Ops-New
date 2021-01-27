namespace OPS_DAL.MesEntities
{
    /// <summary>
    /// Mapping to t_hr_dept table
    /// Known as section or line in factory
    /// </summary>
    public class DeptEntity
    {
        public string DeptCode { get; set; }
        public string FullName { get; set; }
        public string Corporation { get; set; }
        public string Department { get; set; }
        public string Team { get; set; }
        public string Section { get; set; }
        public string PkName { get; set; }
    }
}
