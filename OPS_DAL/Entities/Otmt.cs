namespace OPS_DAL.Entities
{
    /// <summary>
    /// Map to T_OP_OTMT table
    /// </summary>
    /// Author: Son Nguyen Cao
    public class Otmt
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Sos { get; set; }
        public string FullName { get; set; }
        public string Buyer { get; set; }
        public string BuyerName { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public string Model { get; set; }
        public double Cost { get; set; }
        public string Unit { get; set; }
        public string Remarks { get; set; }
        public string Ranking { get; set; }
        public string Registrar { get; set; }
        public string CategId { get; set; }
        public string Purpose { get; set; }
        public string Process { get; set; }
        public string Machine { get; set; }
        public string GsdRefId { get; set; }
        public string ImagePath { get; set; }
        public string Status { get; set; }
        public decimal Total { get; set; }
        public string BrandId { get; set; }
        public string ProcessCode { get; set; }
        public string ProcessName { get; set; }
        public decimal? GroupLevel_0 { get; set; }
        public decimal? GroupLevel_1 { get; set; }
        public decimal? GroupLevel_2 { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string MachineGroup { get; set; }
        public string GroupLevel_0_Name { get; set; }
        public string GroupLevel_1_Name { get; set; }
        public string GroupLevel_2_Name { get; set; }
        public string MchGroupName { get; set; }
    }
}
