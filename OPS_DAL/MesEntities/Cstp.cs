namespace OPS_DAL.MesEntities
{
    /// <summary>
    /// This table maps to t_cm_cstp
    /// This is corporation table
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    /// Created Date: 03-Jul-19
    public class Cstp
    {
        public decimal Id { get; set; }
        public string CorpCode { get; set; }
        public string HrmCorpCode { get; set; }
        public string CorpName { get; set; }
        public string Factory { get; set; }
        public string FactoryName { get; set; }
        public string LocalDbAdd { get; set; }
        public string LocalDbUser { get; set; }
        public string LocalDbPw { get; set; }
        public string LocalDbPort { get; set; }
        public string IotHost { get; set; }
        public string Remarks { get; set; }
        public string LocalDbName { get; set; }
        public string MesApiEndPoint { get; set; }
        public string CombinePackage { get; set; } //ADD) SON - 9/Jul/2019
        public string IOT_DATA_SOURCE_TYPE { get; set; }
        public int ServerNo { get; set; } //2020-09-08 Tai Le(Thomas): Add
    }
}
