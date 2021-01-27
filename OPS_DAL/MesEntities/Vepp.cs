using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class Vepp
    {
        public string Buyer { get; set; }
        public string Factory { get; set; }
        public string LineNo { get; set; }
        public string AoNo { get; set; }
        public string StyleCode { get; set; }
        public string StyleSize { get; set; }
        public string StyleColorSerial { get; set; }
        public string RevNo { get; set; }
        public string PrdPkg { get; set; }
        public string PrdSdat { get; set; }
        public string PrdEdat { get; set; }
        public int OrdQty { get; set; }
        public int PlanQty { get; set; }
        public string Rank { get; set; }
        public string Destination { get; set; }
        public string DeliveryDate { get; set; }
        public string AdType { get; set; }
        public string AdTypeName { get; set; }

        //Style information
        public string StyleInf { get; set; }
        public string StyleColorways { get; set; }
        public string StyleName { get; set; }
        //START ADD - SON) 24/Nov/2020
        public string PackageGroup { get; set; }
        public string SeqNo { get; set; }
        public int RemainQty { get; set; }
        public int TargetQty { get; set; }
        //END ADD - SON) 24/Nov/2020

    }
}
