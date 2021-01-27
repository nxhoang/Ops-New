using System.Security.Cryptography;

namespace OPS_DAL.Entities
{
    /// <summary>
    /// Refer T_00_STMT table
    /// </summary>
    /// Author: Son Nguyen Cao
    public class Stmt:StyleMaster
    {
        public string OldStyleCode { get; set; }
        public string Old_StyleCode { get; set; }//ADD) SON - 11/Jul/2019
        public string StyleGroup { get; set; }
        public string SubGroup { get; set; }
        public string SubSubGroup { get; set; }
        public string Buyer { get; set; }
        public string BuyerStyleName { get; set; }
        public string BuyerStyleCode { get; set; }
        public string SizeUnit { get; set; }
        public float? Width { get; set; }
        public float? EndWise { get; set; }
        public float? Height { get; set; }
        public string WeightUnit { get; set; }
        public decimal? Weight { get; set; }
        public string PackingUnit { get; set; }
        public decimal? PackingQty { get; set; }
        public string PackingSizeUnit { get; set; }
        public float? PackingWidth { get; set; }
        public float? PackingEndWise { get; set; }
        public float? PackingHeight { get; set; }
        public string Status { get; set; }
        public string CurrCode { get; set; }
        public decimal? UnitPrice { get; set; }
        public string Factory { get; set; }
        public string RegistryDate { get; set; }
        public string LastModiDate { get; set; }//ADD) SON - 12/Jul/2019
        public string LastModifyDate { get; set; }
        public string StyleRegister { get; set; }
        public string Designer { get; set; }
        public string ItemManager { get; set; }
        public string QtyAssumer { get; set; }
        public string OpPlanner { get; set; }
        public string Technician { get; set; }
        public string ItemDMan { get; set; }
        public string ConfirmDate { get; set; }
        public string SeasonCode { get; set; }
        public string SampleStage { get; set; }
        public string Picture { get; set; }
        public string Model { get; set; }
        public string Functions { get; set; }
        public string Volume { get; set; }
        public string CollectName { get; set; }
        public string Pattern { get; set; }
        public string VolumeUnit { get; set; }
        public string FunAtch { get; set; }
        public string Fun_Atch { get; set; }//ADD) SON - 11/Jul/2019
        public string OutSourceCheck { get; set; }
        public string Sono { get; set; }

        public string StyleGroupName { get; set; }
        public string StyleSubGroupName { get; set; }
        public string StyleSubSubGroupName { get; set; }
        public string ImageLink { get; set; }
        public string StyleColorways { get; set; }//ADD - SON) 19/Mar/2020
        public string AONo { get; set; }//ADD - SON) 5/Sep/2020

    }
}
