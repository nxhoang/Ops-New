namespace OPS_DAL.Entities
{
    /// <summary>
    /// Refer to T_CM_SRMT table
    /// </summary>
    /// Author: Son Nguyen Cao
    public class Srmt
    {
        public string SystemId { get; set; }
        public string OwnerId { get; set; }
        public string IsUser { get; set; }
        public string MenuId { get; set; }
        public string IsView { get; set; }
        public string IsAdd { get; set; }
        public string IsUpdate { get; set; }
        public string IsDelete { get; set; }
        public string IsPrint { get; set; }
        public string IsExport { get; set; }
        public string IsConfirm { get; set; }
        public string IsCreateUid { get; set; }
        public string IsCreateDt { get; set; }
        public string IsUpdateUid { get; set; }
        public string IsUpdateDt { get; set; }
        public string IsUnconfirm { get; set; }
        public string IsScanQRCode { get; set; } //2019-11-25 Tai Le (Thomas)

        public string FactoryRoleId
        {
            get
            {
                return string.IsNullOrEmpty(OwnerId)? OwnerId: OwnerId.Substring(0, 1);
            }
            set { value = OwnerId; }
        }
    }
}
