using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class Ppkg
    {
        public string PackageGroup { get; set; }
        public int SeqNo { get; set; }//decimal
        public string PPackage { get; set; }
        public string Factory { get; set; }
        public string AoNo { get; set; }
        public int OrdQty { get; set; }//decimal
        public int PlanQty { get; set; }//decimal
        public string DistributeStatus { get; set; }
        public string CreateNew { get; set; }
        public decimal RemainQty { get; set; }

        //2019-06-18 Tai Le (Thomas)
        public string LATESTQCOTIME { get; set; }
        public string NORMALIZEDPERCENT { get; set; }
        public string StyleCode { get; set; }
        public string StyleSize { get; set; }
        public string StyleColorSerial { get; set; }
        public string RevNo { get; set; }
        // ::END 2019-06-18 Tai Le (Thomas)

        //2019-07-20 Tai Le (Thomas)
        public int QCOYear { get; set; }
        public string QCOWeekNo { get; set; }
        // ::END 2019-07-20 Tai Le (Thomas)

        //2019-07-22 Tai Le (Thomas)
        public string LINENO { get; set; }
        // ::END 2019-07-22 Tai Le (Thomas)

        //2020-07-31 Tai Le (Thomas)
        public string QCORANK { get; set; }
        // ::END 2020-07-31 Tai Le (Thomas)

    }
}
