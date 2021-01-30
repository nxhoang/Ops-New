using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class Mcmp
    {
        public Int64 ID { get; set; }
        public string MACHINE_ID { get; set; }
        public string IOT_MAC { get; set; }
        public DateTime UPDATE_TIME { get; set; }
        public string IOT_DEVICE_TYPE { get; set; }
        public string IOT_POSITION { get; set; }
    }
}
