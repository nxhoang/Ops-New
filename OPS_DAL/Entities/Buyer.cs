namespace OPS_DAL.Entities
{
    /// <summary>
    /// Buyer class maps to T_SD_BUYER table
    /// </summary>
    public class Buyer: StyleMaster
    {
        public string BuyerCode { get; set; }
        public string BuyerName { get; set; }
    }
}
