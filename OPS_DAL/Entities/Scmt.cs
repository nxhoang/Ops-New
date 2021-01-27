namespace OPS_DAL.Entities
{
    /// <summary>
    /// T_00_SCMT
    /// </summary>
    public class Scmt
    {
        public string StyleCode { get; set; }
        public string StyleColorWays { get; set; }
        public string StyleColorSerial { get; set; }
        public string StyleColor { get; set; }
        public string RgbValue { get; set; }
        public string OldStyleCode { get; set; }
        public string BuyerColorCode { get; set; }
        public string ImageType { get; set; }

        public string Old_StyleCode { get; set; } //ADD) SON - 6/Jun/2019

    }
}
