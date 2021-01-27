using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesEntities
{
    public class FileDto
    {
        public string Name { get; set; }
        public decimal Size { get; set; }
        public string Date { get; set; }
        public bool IsFile { get; set; }
    }
}
