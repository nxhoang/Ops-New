using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.DgsEntities
{
    public class IotMachineTime
    {
        public string MacAddress { get; set; }
        public string ExcDttm { get; set; }
        public int PowerTime { get; set; }
        public int MotoTime { get; set; }
        public int ActTime { get; set; }
        public string MachineId { get; set; }
    }
}
