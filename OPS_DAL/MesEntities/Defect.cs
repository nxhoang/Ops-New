using OPS_DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class Defect: StyleMaster
    {
        public int TOTAL_DEFECT { get; set; }
        public string STYLETEXT { get; set; }
        public string BUYERSTYLETEXT { get; set; }
        public string BUYER { get; set; }
        public string LINENAME { get; set; }
        public string MXTARGET { get; set; }
        public string AONO { get; set; }
        public string MXPACKAGE { get; set; }
    }
}
