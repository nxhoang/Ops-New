using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class MXOpdt
    {
        public string MXPACKAGE { get; set; }
        public int MXTARGET { get; set; }  //Plan Qty
        public string STYLECODE { get; set; }
        public string STYLESIZE { get; set; }
        public string STYLECOLORSERIAL { get; set; }
        public string REVNO { get; set; }
        public string OPREVNO { get; set; }
        public string OPGROUP { get; set; }
        public string NEXTOPGROUP { get; set; }
        public string OPGROUPNAME { get; set; }
        public int OPSERIAL { get; set; }
        public string DISPLAYCOLOR { get; set; }
        public string OPNUM{ get; set; }
        public string OPNAME { get; set; }
        public string NEXTOP { get; set; }
        public int OPREQQTY { get; set; }
        public int OPIOTCOMPQTY { get; set; }
        public DateTime? IOTLASTDATE{ get; set; }
        public int OPDGSCOMPQTY { get; set; } 
        public DateTime? DGSLASTDATE{ get; set; }
        public string EMPLOYEECODE { get; set; }
        public string EMPCORP { get; set; }
        public string EMPIMGPATH { get; set; }
        public int IOTCounter { get; set; }//2020-10-03 Tai Le(Thomas)
        public string AONO { get; set;  } //2020-10-03 Tai Le(Thomas)
        public string ImageName { get; set; } //2020-11-17 Tai Le(Thomas)
        public MXOpdt(){}
    }
}
