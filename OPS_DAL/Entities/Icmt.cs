using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    public class Icmt
    {
        public string ItemCode { get; set; }
        public string OldtemCode { get; set; }
        public string ItemName { get; set; }
        public string ImportFullName { get; set; }
        public string MainLevel { get; set; }
        public string LevelNo01 { get; set; }
        public string LevelNo02 { get; set; }
        public string LevelNo03 { get; set; }
        public string LevelNo04 { get; set; }
        public string LevelNo05 { get; set; }
        public string LevelNo06 { get; set; }
        public string LevelNo07 { get; set; }
        public string LevelNo08 { get; set; }
        public string LevelNo09 { get; set; }
        public string LevelNo10 { get; set; }
        public string HsCode { get; set; }
        public string PrintGrade { get; set; }
        public string Ndc { get; set; }
        public string PlanType { get; set; }
        public decimal MinOrderSize { get; set; } //SON MOD) 2019.12.05 - change string to decimal
        public string ItemRegister { get; set; }
        public string RegistryDate { get; set; }
        public string LastModiDate { get; set; }
        public string Sos { get; set; }
        public string ConsumpUnit { get; set; }
        public string SizeUnit { get; set; }
        public decimal Width { get; set; }//SON MOD) 2019.12.05 - change string to decimal
        public decimal EndWise { get; set; }//SON MOD) 2019.12.05 - change string to decimal
        public decimal Thickness { get; set; }//SON MOD) 2019.12.05 - change string to decimal
        public decimal Height { get; set; }//SON MOD) 2019.12.05 - change string to decimal
        public string WeightUnit { get; set; }
        public decimal Gravity { get; set; }//SON MOD) 2019.12.05 - change string to decimal
        public decimal Weight { get; set; }//SON MOD) 2019.12.05 - change string to decimal
        public string PackingUnit { get; set; }
        public decimal PackingQty { get; set; }//SON MOD) 2019.12.05 - change string to decimal
        public string PackingSizeUnit { get; set; }
        public decimal PackingWidth { get; set; }//SON MOD) 2019.12.05 - change string to decimal
        public decimal PackingEndWise { get; set; }//SON MOD) 2019.12.05 - change string to decimal
        public decimal PackingHeight { get; set; }//SON MOD) 2019.12.05 - change string to decimal
        public string ItemUse { get; set; }
        public string CurrCode { get; set; }
        public decimal StdPrice { get; set; }//SON MOD) 2019.12.05 - change string to decimal
        public string Remarks { get; set; }
        public string Standard { get; set; }
        public string Purchaser { get; set; }
        public decimal Cbm { get; set; }//SON MOD) 2019.12.05 - change string to decimal
        public decimal LeadTime { get; set; }//SON MOD) 2019.12.05 - change string to decimal
        public decimal EstPrice { get; set; }//SON MOD) 2019.12.05 - change string to decimal
        public string Buyer { get; set; }
        public string TempFile { get; set; }
        public decimal UsableWidth { get; set; }//SON MOD) 2019.12.05 - change string to decimal
        public decimal PoQtyUnit { get; set; }//SON MOD) 2019.12.05 - change string to decimal
        public string LatestPoSos { get; set; }
        public string LatestBuyer { get; set; }
        public string NewItem { get; set; }
        public string CustomsCode { get; set; }
        public string UserItemName { get; set; }
        public string BuyerItemCode { get; set; }
        public string SupplierItemCode { get; set; }
        public string PlacingType { get; set; }
        public string ImportedDate { get; set; }
        public decimal PackingGWeight { get; set; }//SON MOD) 2019.12.05 - change string to decimal
        public string MaxItemCode { get; set; }


    }
}
