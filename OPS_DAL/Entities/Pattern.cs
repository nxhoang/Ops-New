using System;

namespace OPS_DAL.Entities
{
    public class Pattern : StyleMaster
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemColorSerial { get; set; }
        public string ItemColorWays { get; set; }
        public string PatternSerial { get; set; }
        public string MainItemCode { get; set; }
        public string MainItemName { get; set; }
        public string MainItemColorWays { get; set; }
        public string MainItemColorSerial { get; set; }
        public string MainPart { get; set; }
        public string MainPartName { get; set; }
        public string Piece { get; set; }
        public string SizeUnit { get; set; }
        public decimal? Width { get; set; }
        public decimal? EndWise { get; set; }
        public decimal? Height { get; set; }
        public decimal? Area { get; set; }
        public short PieceQty { get; set; }
        public short PieceQtyRest { get; set; }
        public string ConsumpUnit { get; set; }
        public decimal? UnitConsumption { get; set; }
        public string CurrCode { get; set; }
        public decimal? TrPrice { get; set; }
        public decimal? PatternPrice { get; set; }
        public string Remarks { get; set; }
        public string PdCode { get; set; }
        public string Register { get; set; }
        public string QtyAssumer { get; set; }
        public string PatternFile1 { get; set; }
        public string PatternFile2 { get; set; }
        public decimal? MarkGrap { get; set; }
        public decimal? MarkWidth { get; set; }
        public decimal? MarkQty { get; set; }
        public string MarkMethod { get; set; }
        public decimal? MarkEfficiency { get; set; }
        public string MarkFile { get; set; }
        public string Polygon { get; set; }
        public string ImageName { get; set; }
        public string Fill { get; set; }
        public string Stroke { get; set; }
        public string StrokeWidth { get; set; }
        public string Opposite  { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public string PieceUnique { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }

        public string PURCHASESET { get; set; }

        public DateTime? LAST_UPDATED_TIME { get; set; }
        //public string MIRROR { get; set; }
        //public string CUTTINGMACHINE { get; set; }

        //START ADD) SON - 5 Feburary 2020 - Adding for API
        public string Mirror { get; set; }
        public string CuttingMachine { get; set; }
        //END ADD) SON - 5 Feburary 2020
    }
}
