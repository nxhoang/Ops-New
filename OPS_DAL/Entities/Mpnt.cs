using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    public class Mpnt
    {
        public string STYLECODE { get; set; }
        public string STYLESIZE { get; set; }
        public string STYLECOLORSERIAL { get; set; }
        public string REVNO { get; set; }
        public string MODULEITEMCODE { get; set; }
        public string ITEMCODE { get; set; }
        public string ITEMCOLORSERIAL { get; set; }
        public string MAINITEMCODE { get; set; }
        public string MAINITEMCOLORSERIAL{ get; set; }
        public string PATTERNSERIAL{ get; set; }
        public string MAINPART{ get; set; }
        public string PIECE{ get; set; }
        public string SIZEUNIT{ get; set; }
        public decimal WIDTH{ get; set; }
        public decimal ENDWISE{ get; set; }
        public decimal HEIGHT{ get; set; }
        public decimal PIECEQTY{ get; set; }
        public decimal UNITCONSUMPTION{ get; set; }
        public string REMARKS{ get; set; }
        public string PDCODE{ get; set; }
        public string REGISTRAR{ get; set; }
        public string CONSUMPTIONUNIT{ get; set; }
        public string PIECEUNIQUE{ get; set; }
        public decimal AREA{ get; set; }
    }
}
