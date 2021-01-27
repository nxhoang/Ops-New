using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    /// <summary>
    /// T_00_SFDT
    /// </summary>
    public class Sfdt
    {
        public string StyleCode { get; set; }
        public int Serial { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public string IsMain { get; set; }

    }
}
