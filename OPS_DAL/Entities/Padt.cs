using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    public class Padt
    {
        public string PaintingType { get; set; }
        public string MaterialType { get; set; }
        public decimal MinTemperature { get; set; }
        public decimal MaxTemperature { get; set; }
        public decimal MinDryingTime { get; set; }
        public decimal MaxDryingTime { get; set; }
        public decimal MinCoolingTime { get; set; }
        public decimal MaxCoolingTime { get; set; }
    }
}
