namespace OPS_DAL.Entities
{
    /// <summary>
    /// Style master maps to T_SD_DORM table
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    public class StyleMaster
    {
        #region Properties
        public string StyleCode { get; set; }
        public string StyleName { get; set; }
        public string StyleColorSerial { get; set; }
        public string StyleSize { get; set; }
        public string RevNo { get; set; }
        public string StyleColorWays { get; set; }
        public string BuyerStyleCode { get; set; }
        public string BuyerStyleName { get; set; }
        #endregion

        #region Constructors
        public StyleMaster() { }

        public StyleMaster(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            StyleCode = styleCode;
            StyleSize = styleSize;
            StyleColorSerial = styleColorSerial;
            RevNo = revNo;
        }
        #endregion
    }
}
