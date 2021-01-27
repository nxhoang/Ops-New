using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class Lndt
    {
        public string MxPackage { get; set; }
        public int LineSerial { get; set; }
        public string ModuleId { get; set; }
        public string ProcessGroup { get; set; }
        public DateTime ProDate { get; set; }
        public string RegisterId { get; set; }
        public DateTime RegistryDate { get; set; }
        public string ProcessGroupName { get; set; }
        public string ModuleName { get; set; }
        public string LineName { get; set; }
    }
}
