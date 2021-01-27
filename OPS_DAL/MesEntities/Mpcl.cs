using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class Mpcl
    {
        public string MxPackage { get; set; }
        public string CheckListId { get; set; }
        public string Confirmer { get; set; }
        public DateTime ConfirmTime { get; set; }
    }
}
