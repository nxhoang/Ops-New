using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    public class Dfdt
    {
        public string DefectCat { get; set; }
        public string DefectCode { get; set; }
        public string RegisterId { get; set; }
        public DateTime RegistryDate { get; set; }
    }
}
