namespace OPS_DAL.MesEntities
{
    /// <summary>
    /// Operation layout line simulation map to t_mx_opls table in database.
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    /// Created Date: 11-Nov-19
    public class Opls
    {
        public string MxPackage { get; set; }
        public int FromTable { get; set; }
        public int ToTable { get; set; }
    }
}
