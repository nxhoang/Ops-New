using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    public class SdSamt
    {
        public string StyleCode { get; set; }
        public string StyleSize { get; set; }
        public string StyleColorSerial { get; set; }
        public string RevNo { get; set; }
        public string ModuleId { get; set; }
        public string Linked { get; set; }
        public string Registrar { get; set; }
        public DateTime? RegistryDate { get; set; }
        public string ModuleName { get; set; }

    }
}
