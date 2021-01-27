namespace OPS_DAL.Entities
{
    /// <summary>
    /// Refer T_CM_URLM table
    /// </summary>
    /// Author: Son Nguyen Cao
    public class Urlm
    {
        public string RoleId { get; set; }
        public string RoleDesc { get; set; }
        public string RoleDescKr { get; set; }
        public string ParentRole { get; set; }
    }
}
