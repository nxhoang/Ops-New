using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class Config
    {
        public int ID { get; set; }
        public string CONFIGKEY { get; set; }
        public string DSC { get; set; }
        public string CONFIGVALUE { get; set; }
    }
}
