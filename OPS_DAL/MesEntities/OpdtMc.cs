using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class OpdtMc
    {
        public int ID { get; set; }
        public string STYLECODE { get; set; }
        public string STYLESIZE { get; set; }
        public string STYLECOLORSERIAL { get; set; }
        public string REVNO { get; set; }
        public string OPREVNO { get; set; }
        public int OPSERIAL { get; set; }
        public string MCID { get; set; }
        public string IOT_MODULE_MAC { get; set; }
        public DateTime MC_PAIR_TIME { get; set; }
        public string MC_PAIR_USER { get; set; }
        public int LAST_IOT_DATA { get; set; }
        public int LAST_IOT_CALC_DATA { get; set; }
        public DateTime LAST_IOT_DATA_RECEIVE_TIME { get; set; }
        public int LAST_IOT_DATA_DGS { get; set; }
        public int LAST_IOT_CALC_DATA_DGS { get; set; }
        public DateTime LAST_IOT_DATA_RECEIVE_TIME_DGS { get; set; }
        public string EMPID { get; set; }//2020-09-10 Tai Le(Thomas)
        public string MXPACKAGE { get; set; } // 2020-09-24 Dinh Van
        // Start 2020-09-28 Dinh Van
        public string OPNAME { get; set; } 
        public string STYLENAME { get; set; }
        public string BUYERSTYLECODE { get; set; }
        public string BUYERSTYLENAME { get; set; }
        public string STYLECOLORWAYS { get; set; }
        public int MXTARGET { get; set; }
        public string LINENAME { get; set; }
        // End 2020-09-28 Dinh Van
        public string OPGROUPNAME { get; set; } // 2020-09-30 Dinh Van
        public Int64 VAN_COUNT { get; set; } // 2020-10-29 Dinh Van
        public string IMAGENAME { get; set; } // 2020-11-07 Dinh Van
        public string EMPLOYEENAME { get; set; } // 2020-11-07 Dinh Van
        public string CORPORATIONCODE { get; set; } // 2020-11-07 Dinh Van
        public decimal LOB { get; set; } // 2021-01-06 Dinh Van

    }
}
