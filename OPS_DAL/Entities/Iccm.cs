using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    public class Iccm
    {
        public string ItemCode { get; set; }
        public string ItemColorways { get; set; }
        public string ItemColorSerial { get; set; }
        public string ItemColor { get; set; }
        public string RGBValue { get; set; }
        public string Old_ItemCode { get; set; }
        public string Original { get; set; }
        public string Trial { get; set; }
        public string De { get; set; }
        public string Register { get; set; }
        public DateTime? RegistryDate { get; set; }
        public string LastModifier { get; set; }
        public DateTime? LastModifyDate { get; set; }
        public string Old_ItemColorSerial { get; set; }
        public string SeasonCode { get; set; }
        public string SeasonColorSerial { get; set; }
    }
}
