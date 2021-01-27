using OPS_DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.APIEntities
{
    public class PatternAPI: StyleMaster
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemColorSerial { get; set; }
        public string ItemColorways { get; set; }
        public string PatternSerial { get; set; }
        public string MainItemCode { get; set; }
        public string MainItemColorSerial { get; set; }
        public string MainPart { get; set; }
        public string Piece { get; set; }
        public string SizeUnit { get; set; }
        public decimal? Width { get; set; }
        public decimal? EndWise { get; set; }
        public decimal? Height { get; set; }
        public short PieceQty { get; set; }
        public decimal? UnitConsumption { get; set; }
        public string CurrCode { get; set; }
        public decimal? TrPrice { get; set; }
        public decimal? PatternPrice { get; set; }
        public string Remarks { get; set; }
        public string ConsumpUnit { get; set; }
        public string PieceUnique { get; set; }
        public decimal? Area { get; set; }
        public string Mirror { get; set; }
        public string CuttingMachine { get; set; }
        public string MainPartName { get; set; }
    }
}
