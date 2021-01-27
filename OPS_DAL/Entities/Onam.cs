using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    public class Onam
    {
        public decimal OpNameId { get; set; }
        public string OpName { get; set; }
        public string ActionCode { get; set; }
        public string MainLevel { get; set; }
        public string LevelNo01 { get; set; }
        public string IsTemplate { get; set; }
    }
}
