namespace OPS_DAL.Entities
{
    /// <summary>
    /// Refer T_OP_DICT table
    /// </summary>
    /// Author: Son Nguyen Cao
    public class OperationName
    {
        public decimal OpNameId { get; set; }
        public string OpName { get; set; }
        public string Remark { get; set; }
        public int TotalRow { get; set; }
    }
}
