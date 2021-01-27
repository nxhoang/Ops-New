using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    public class Dfmt
    {
        public string DefectCode { get; set; }
        public string DefectDesc { get; set; }
        public string DefectCat { get; set; }
        public string Vietnamese { get; set; }
        public string Bahasa { get; set; }
        public string Burmese { get; set; }
        public string Amharic { get; set; }
        public string HasBuyerDefect { get; set; }
    }
}
