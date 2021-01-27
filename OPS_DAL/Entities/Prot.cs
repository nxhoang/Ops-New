using OPS_Utils;
using System;
using System.IO;

namespace OPS_DAL.Entities
{
    /// <summary>
    /// Refer T_SD_PROT (Pattern relation operation plan table)
    /// </summary>
    /// Author: Son Nguyen Cao
    /// <seealso cref="StyleMaster" />
    public class Prot : StyleMaster, ICloneable
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemColorSerial { get; set; }
        public string MainItemCode { get; set; }
        public string MainItemColorSerial { get; set; }
        public string PatternSerial { get; set; }
        public string CurPatternSerial { get; set; }
        public string OpRevNo { get; set; }
        public int OpSerial { get; set; }
        public string OpType { get; set; }
        public string Status { get; set; }
        public string ConsumpUnit { get; set; }
        public decimal? UnitConsumption { get; set; }
        public string Edition { get; set; }
        public string Piece { get; set; }
        public short PieceQty { get; set; }
        public string Url { get; set; }
        public string BomOrPattern { get; set; }
        public decimal? Width { get; set; }
        public decimal? EndWise { get; set; }
        public decimal? Height { get; set; }
        public string PieceUnique { get; set; }

        public string CadColorSerial { get; set; }
        public string CadFile { get; set; }
        public string UrlThumbnail { get; set; }
        public string OpName { get; set; }
        public string OpNameLan { get; set; }
        //public string StyleColorways { get; set; }

        public string ItemColorWays { get; set; }//ADD - SON) 26/Dec/2020
        public string HasPattern { get; set; }//ADD - SON) 26/Dec/2020
        public int PieceQtyRest { get; set; }//ADD - SON) 5/Jan/2021
        public int OpnSerial { get; set; }//ADD - SON) 5/Jan/2021
        public string ImageLink {
            get
            {
                //http://203.113.151.204:8080/PKPDM/UNI/UNI0037/UNI0037LRG000001/UNI0037LRG000001002016/U95942.PNG
                if (!string.IsNullOrEmpty(PieceUnique))
                    return $"{ConstantGeneric.PatternImageLink}{StyleCode.Substring(0, 3)}/{StyleCode}/{StyleCode}{StyleSize}{StyleColorSerial}{RevNo}/{Path.GetFileNameWithoutExtension(CadFile)}/{PieceUnique}.png";
                return string.Empty;
            }
            set
            {
                ImageLink = value;
            }
        }//ADD - SON) 5/Jan/2021


        public object Clone()
        {
            return new Prot
            {
                StyleCode = StyleCode,
                StyleSize = StyleSize,
                StyleColorSerial = StyleColorSerial,
                RevNo = RevNo,
                ItemCode = ItemCode,
                ItemName = ItemName,
                ItemColorSerial = ItemColorSerial,
                MainItemCode = MainItemCode,
                MainItemColorSerial = MainItemColorSerial,
                OpRevNo = OpRevNo,
                OpSerial = OpSerial,
                OpType = OpType,
                BomOrPattern = BomOrPattern,
                PatternSerial = PatternSerial,
                ConsumpUnit = ConsumpUnit,
                UnitConsumption = UnitConsumption,
                Edition = Edition,
                CurPatternSerial = CurPatternSerial
            };

        }
    }
}
