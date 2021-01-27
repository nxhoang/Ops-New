using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    public class MBom
    {
        public string StyleCode { get; set; }
        public string StyleSize { get; set; }
        public string StyleColorSerial { get; set; }
        public string RevNo { get; set; }
        public string ModuleItemCode { get; set; }
        public string ItemCode { get; set; }
        public string ItemColorSerial { get; set; }
        public string MainItemCode { get; set; }
        public string MainItemColorSerial { get; set; }
        public string ConsumpUnit { get; set; }
        public int Qty { get; set; }
        public decimal? UnitConsumption { get; set; }
        public string ItemRegister { get; set; }
        public string RegistryDate { get; set; }
        public string Sos { get; set; }
        public string Nation { get; set; }
        public string CurrCode { get; set; }
        public decimal? StdPrice { get; set; }
        public string TdCheck { get; set; }
        public decimal? PatternCons { get; set; }
        public decimal? MarkerCons { get; set; }
        public DateTime? ConfDate { get; set; }
        public decimal? PolyCons { get; set; }
        public string Cad_Material { get; set; }
        public string SetCheck { get; set; }
        public string GenName { get; set; }
        public string MarkerConsUnit { get; set; }
        public decimal? AreaCons { get; set; }
        public string AreaConsUnit { get; set; }
        public decimal? Marker_LossRate { get; set; }
        public string HasPattern { get; set; }//ADD) SON - 12 December 2019
        public string ModuleName { get; set; }//ADD) SON - 12 December 2019
        public string Linked { get; set; }//ADD - SON) 1/Oct/2020
        public string ItemColorways { get; set; }//ADD - SON) 27/Jan/2021
        public string ItemName { get; set; }//ADD - SON) 27/Jan/2021
    }
}
