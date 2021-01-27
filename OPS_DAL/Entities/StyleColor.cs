namespace OPS_DAL.Entities
{
    /// <summary>
    /// Color of style maps to T_00_SCMT table
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    public class StyleColorEntity: StyleMaster
    {
        public string StyleColorWays { get; set; }
        public string StyleColor { get; set; }
        public string RgbValue { get; set; }
        public string OldStyleCode { get; set; }
        public string BuyerColorCode { get; set; }
        public string ImageType { get; set; }
    }
}
