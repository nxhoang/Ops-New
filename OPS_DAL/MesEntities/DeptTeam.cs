namespace OPS_DAL.MesEntities
{
    /// <summary>
    /// Mapping to t_hr_dept table
    /// Known as section or line in factory
    /// </summary>
    public class DeptTeam
    {
        public string CorpCode { get; set; }
        public string CorpName { get; set; }
        public string DeptCode { get; set; }
        public string DeptName { get; set; }
        public string LineCode { get; set; }
        public string LineName { get; set; }
        public string TeamCode { get; set; }
        public string TeamName { get; set; }
    }
}
