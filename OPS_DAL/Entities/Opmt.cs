using System;

namespace OPS_DAL.Entities
{
    public class Opmt : StyleMaster
    {
        public decimal TotalRecords { get; set; }
        public string OpRevNo { get; set; }
        public int OpTime { get; set; }
        public decimal OpPrice { get; set; }
        public int MachineCount { get; set; }
        public string ConfirmChk { get; set; }
        public int OpCount { get; set; }
        public int ManCount { get; set; }
        public string FileName { get; set; }
        public string FileName2 { get; set; }
        public string FilePdf { get; set; }
        public string FilePdf2 { get; set; }
        public string PdmFile { get; set; }
        public string ProcessWidth { get; set; }
        public string ProcessHeight { get; set; }
        public string PlanPdf { get; set; }
        public string ColorTheme { get; set; }
        public string Edition { get; set; }
        public string Edition2 { get; set; }
        public string Remarks { get; set; }
        public decimal LayoutFontSize { get; set; }
        public string Language { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public string BenchMarkTime { get; set; }
        public decimal TargetOfferPrice { get; set; }
        public decimal OfferOpPrice { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public string RegisterId { get; set; }
        public DateTime? RegistryDate { get; set; }
        public string ConfirmedId { get; set; }

        //public string StyleColorWays { get; set; }
        public string MaxOpRevNo { get; set; }
        //public string BuyerStyleCode { get; set; }
        //public string BuyerStyleName { get; set; }
        public string Buyer { get; set; }
        public decimal CanvasHeight { get; set; }
        public string GroupMode { get; set; }
        public string Factory { get; set; }
        public string StyleGroup { get; set; }
        public string SubGroup { get; set; }
        public string SubSubGroup { get; set; }
        public decimal SumOpCount { get; set; }
        public decimal? SumManCount { get; set; }
        public decimal SumMachineCount { get; set; }
        public string MxPackage { get; set; }
        public int Synced { get; set; }
        public decimal TotalOpTime { get; set; }//ADD) SON - 24 December 2019
        public decimal TargetPerDay { get; set; }//ADD) SON - 24 December 2019
        public string RegisterName { get; set; }//ADD - SON) 04/Sep/2020
        public decimal Sorting { get; set; }//ADD - SON) 04/Sep/2020
        public string FactoryName { get; set; }//ADD - SON) 9/Oct/2020

    }
}
